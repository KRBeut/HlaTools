using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using hlatools.core.DataObjects;
using System.IO.Compression;
using hlatools.core.Utils;

namespace hlatools.core.IO.SAM
{
    public class BamParserCore : BamParserCore<SamSeq, SamHeader>
    {
        public BamParserCore(Stream bgzfStrm, Func<SamSeq> recFactory) 
            : base(bgzfStrm, recFactory)
        {
        }

        public BamParserCore(BinaryReader binRdr, Func<SamSeq> recFactory)
            : base(binRdr, recFactory)
        {
        }
    }

    public class BamParserCore<S, H> : IDisposable where H : SamHeader, new() where S : SamSeq
    {
        public H Header { get; protected set; }
        public BinaryReader BinRdr { get; protected set; }

        readonly Func<S> RecFactory;
        public BamParserCore(Stream bgzfStrm, Func<S> recFactory) 
            : this(new BinaryReader(bgzfStrm), recFactory)
        {
            
        }

        public BamParserCore(BinaryReader binRdr, Func<S> recFactory)
        {
            BinRdr = binRdr;
            RecFactory = recFactory;
            Header = ParseHeader(BinRdr);
        }

        public static BamParserCore<S, H> FromFilepath(string filepath, Func<S> fact = null)
        {
            if (fact == null)
            {
                fact = () => (S)new SamSeq();
            }

            var strm = File.OpenRead(filepath);
            var bgzfStrm = new BgzfReader(strm);
            var prsr = new BamParserCore<S, H>(bgzfStrm, fact);
            return prsr;
        }

        protected H ParseHeader(BinaryReader binRdr)
        {
            var samHdr = new H();
            var magic = binRdr.ReadChars(4);

            if (!(magic[0] == 'B' && magic[1] == 'A' && magic[2] == 'M') && magic[3] == 1)
            {
                throw new Exception(string.Format("Invalid BAM binary header, Magic = {0} (this is not a BAM file).", string.Join("",magic)));
            }

            var l_text = binRdr.ReadInt32();
            var headerTxt = new String(binRdr.ReadChars(l_text));
            var text = headerTxt.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var hdrLine in text)
            {
                SamParserCore<S, H>.ParseHeaderLine(samHdr, hdrLine);
            }
            var intToRefSa = samHdr.SQ.Values.ToDictionary(sq => sq.SortIndex, sq => sq);
            samHdr.IntToRefSeq = intToRefSa;

            var n_ref = binRdr.ReadInt32();
            for(int k = 0; k < n_ref; k++)
            {
                var l_name = binRdr.ReadInt32();
                var seqName = binRdr.ReadChars(l_name);
                var seqLen = binRdr.ReadInt32();
            }
            return samHdr;
        }

        public IEnumerable<S> GetRecords()
        {
            int cnt = 0;
            S samSeq = null;
            byte[] recBytes = new byte[10000];
            var blkBytes = new byte[4];
            while (BinRdr.Read(blkBytes,0,4) == 4)
            {
                cnt++;
                var block_size = BitConverter.ToInt32(blkBytes, 0);
                if (recBytes == null || recBytes.Length < block_size)
                {
                    recBytes = new byte[block_size];
                }
                BinRdr.Read(recBytes, 0, block_size);
                samSeq = ParserRead(recBytes, block_size);
                yield return samSeq;
            }
        }

        static readonly char[] trailingChars = new char[] { '=' };

        protected virtual S ParserRead(byte[] alignmentBlock, int block_size)
        {
            var runningOffset = 0;
            var samSeq = RecFactory();
            var refId = BitConverter.ToInt32(alignmentBlock, runningOffset);
            runningOffset += 4;
            if (refId >= 0 && Header.IntToRefSeq.TryGetValue(refId, out SamRefSeq refSeq))
            {
                samSeq.Rname = refSeq.SeqName;
            }
            samSeq.Pos = BitConverter.ToInt32(alignmentBlock,runningOffset) + 1;
            runningOffset += 4;
                        
            var l_read_name = (int)alignmentBlock[runningOffset];
            runningOffset += 1;
            samSeq.Mapq = (int)alignmentBlock[runningOffset];
            runningOffset += 1;
            var bin = BitConverter.ToUInt16(alignmentBlock, runningOffset);
            runningOffset += 2;
            
            var n_cigar_op = BitConverter.ToInt16(alignmentBlock, runningOffset);
            runningOffset += 2;
            samSeq.Flag = (SamFlag)BitConverter.ToInt16(alignmentBlock, runningOffset);
            runningOffset += 2;
            var l_seq = BitConverter.ToInt32(alignmentBlock,runningOffset);
            runningOffset += 4;
            var rnextId = BitConverter.ToInt32(alignmentBlock, runningOffset);
            runningOffset += 4;
            if (rnextId >= 0 && Header.IntToRefSeq.TryGetValue(rnextId, out refSeq))
            {
                samSeq.Rnext = refSeq.SeqName;
            }

            samSeq.Pnext = BitConverter.ToInt32(alignmentBlock, runningOffset) + 1;
            runningOffset += 4;

            samSeq.Length = BitConverter.ToInt32(alignmentBlock, runningOffset);
            runningOffset += 4;

            var qNameBytes = alignmentBlock.Skip(runningOffset).Take(l_read_name - 1).Select(c=>(char)c).ToArray();
            samSeq.Qname = new String(qNameBytes);
            runningOffset += l_read_name;

            //read the cigar opt-by-opt
            var cigToks = new List<CigTok>(n_cigar_op);
            for (int k = 0; k < n_cigar_op; k++)
            {
                var cigOpt = BitConverter.ToUInt32(alignmentBlock,runningOffset);
                runningOffset += 4;
                var len = (int)(cigOpt >> 4);
                var opt = optMap[(short)(cigOpt & 0x0F)];
                cigToks.Add(new CigTok(opt.ToString(), len));
            }
            samSeq.Cigar = new Cigar(cigToks);

            //read the seq char-pair by char-pair
            int cnt = 0;
            var seqStr = string.Empty;
            for (int j = runningOffset; j < runningOffset+((l_seq + 1) / 2); j++)//double check this loop
            {
                cnt++;
                var byt = alignmentBlock[j];
                seqStr += Dict.Get(seqMap, (short)(byt >> 4), 'N');
                seqStr += Dict.Get(seqMap, (short)(byt & 0x0F), 'N');
            }
            runningOffset += cnt;//do not read the '\0' that terminates the string
            samSeq.Seq = seqStr.TrimEnd(trailingChars);

            //read and convert the qual string
            var qualArray = new char[l_seq];
            Array.Copy(alignmentBlock, runningOffset, qualArray, 0, l_seq);
            samSeq.Qual = new String(qualArray.Select(x=>(char)(x+33)).ToArray());
            runningOffset += l_seq;

            samSeq.Opts.Clear();
            ParseMetadata(alignmentBlock, samSeq, block_size, runningOffset);
            return samSeq;
        }
        
        protected virtual void ParseMetadata(byte[] binRdr, SamSeq read, int bufferEnd = int.MinValue, int offset = 0)
        {
            
        }
        
        public void Dispose()
        {
            if (this.BinRdr != null)
            {
                this.BinRdr.Dispose();
                this.BinRdr = null;
            }
        }

        static readonly Dictionary<short, string> optMap = new Dictionary<short, string>()
        {
            //‘MIDNSHP=X’→‘012345678’
            { 0, "M" },
            { 1, "I" },
            { 2, "D" },
            { 3, "N" },
            { 4, "S" },
            { 5, "H" },
            { 6, "P" },
            { 7, "=" },
            { 8, "X" }
        };
        
        static readonly Dictionary<short, char> seqMap = new Dictionary<short, char>()
        {
            // ‘=ACMGRSVTWYHKDBN’→ [0, 15]
            { 0, '=' },
            { 1, 'A'},
            { 2, 'C' },
            { 3, 'M'},
            { 4, 'G' },
            { 5, 'R'},
            { 6, 'S' },
            { 7, 'V'},
            { 8, 'T' },
            { 9, 'W'},
            { 10, 'Y' },
            { 11, 'H'},
            { 12, 'K' },
            { 13, 'D'},
            { 14, 'B' },
            { 15, 'N' }
        };

    }
}
