using hlatools.core.DataObjects;
using hlatools.core.IO.Tabix;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    public class BamWriter : IDisposable    
    {

        BinaryWriter _binWrtr;

        public static void WriteToFile(string filePath, SamHeader hdr, IEnumerable<SamSeq> reads)
        {
            using (var strm = File.OpenWrite(filePath))            
            using (var bgzf = new BgzfWriter(strm))
            using (var bamWrtr = new BamWriter(bgzf))
            {
                bamWrtr.WriteHeader(hdr);
                bamWrtr.WriteRecords(reads);
            }
        }

        public static void WriteToFile(BamWriter bamWrtr, IList<SamSeq> reads)
        {
            var hdr = BuildSamHeader(reads);
            bamWrtr.WriteHeader(hdr);
            bamWrtr.WriteRecords(reads);
        }

        public static void WriteToFile(string filePath, IList<SamSeq> reads)
        {
            if (filePath == null)
            {
                using (var strm = Console.OpenStandardOutput())
                using (var bgzf = new BgzfWriter(strm))
                using (var bamWrtr = new BamWriter(bgzf))
                {
                    WriteToFile(bamWrtr, reads);
                }
            }
            else
            {
                using (var strm = File.OpenWrite(filePath))
                using (var bgzf = new BgzfWriter(strm))
                using (var bamWrtr = new BamWriter(bgzf))
                {
                    var hdr = BuildSamHeader(reads);
                    bamWrtr.WriteHeader(hdr);
                    bamWrtr.WriteRecords(reads);
                }
            }            
        }

        private static SamHeader BuildSamHeader(IList<SamSeq> reads)
        {
            var hdr = new SamHeader();
            foreach (var r in reads)
            {
                SamRefSeq sq = null;
                if (hdr.SQ.TryGetValue(r.Rname, out sq))
                {
                    sq.SeqLen = Math.Max(sq.SeqLen, r.Pos + Cigar.ComputeTLen(r.Cigar));
                }
                else
                {
                    hdr.SQ.Add(r.Rname, new SamRefSeq() { SeqName = r.Rname, SeqLen = r.Pos + Cigar.ComputeTLen(r.Cigar) });
                }
            }
            return hdr;
        }

        public BamWriter(BgzfWriter bgzfWrtr)
        {
            _binWrtr = new BinaryWriter(bgzfWrtr);
        }

        Dictionary<string, int> _refSeqToInt;

        public void WriteHeader(SamHeader hdr)
        {
            var strBldr = new StringWriter();
            SamWriter.WriteHeader(strBldr, hdr);
            var hdrStr = strBldr.ToString();
            hdrStr = hdrStr.Replace("\r", "");
            var hdrCharArray = hdrStr.ToArray();
            _binWrtr.Write(new char[] { 'B', 'A', 'M', (char)1 });//magic string
            _binWrtr.Write((Int32)hdrStr.Length);
            _binWrtr.Write(hdrStr.ToArray());
            _binWrtr.Write((Int32)hdr.SQ.Count);

            int cnt = 0;
            _refSeqToInt = new Dictionary<string, int>();
            foreach (var kvp in hdr.SQ)
            {
                _refSeqToInt.Add(kvp.Value.SeqName, cnt++);
                _binWrtr.Write((Int32)(kvp.Value.SeqName.Length + 1));
                _binWrtr.Write(kvp.Value.SeqName.Trim().ToArray());
                _binWrtr.Write('\0');//fake a null-terminated string
                _binWrtr.Write((Int32)kvp.Value.SeqLen);
            }
        }

        public void WriteRecords<T>(IEnumerable<T> reads) where T : SamSeq
        {            
            foreach (var read in reads)
            {                
                WriteRecord(read);
            }
        }

        static Dictionary<string,ushort> optMap = new Dictionary<string,ushort>()
        {
            //‘MIDNSHP=X’→‘012345678’
            { "M",0 },
            { "I",1 },
            { "D",2 },
            { "N",3 },
            { "S",4 },
            { "H",5 },
            { "P",6 },
            { "=",7 },
            { "X",8 }
        };

        static Dictionary<char,ushort> seqMap = new Dictionary<char, ushort>()
        {
            // ‘=ACMGRSVTWYHKDBN’→ [0, 15]
            { '=',0  },
            { 'A',1  },
            { 'C',2  },
            { 'M',3  },
            { 'G',4  },
            { 'R',5  },
            { 'S',6  },
            { 'V',7  },
            { 'T',8  },
            { 'W',9  },
            { 'Y',10 },
            { 'H',11 },
            { 'K',12 },
            { 'D',13 },
            { 'B',14 },
            { 'N',15 }
        };
        
        protected int ComputeReadByteSize<T>(T read) where T : SamSeq
        {
            var byteSize = 8*4;//the simple Int32's and UInt32's
            byteSize += read.Qname.Length + 1;//+1 is because the QName is null terminated
            byteSize += read.Cigar.Count * 4;//each cigar token is written as a UInt32
            byteSize += (read.Seq.Length + 1) / 2;//the seq is 4-bits encoded
            byteSize += read.Seq.Length;//each qvals is an 8-bit char (read.Seq should be the same length as read.Qual)
            foreach (var opt in read.Opts)
            {
                byteSize += opt.Value.GetBamArraySize();
            }
            //byteSize += read.Opts.Values.Sum(opt => opt.GetBamArraySize());
            return byteSize;
        }

        public void WriteRecord<T>(T read) where T : SamSeq
        {
            if (read.Qname == @"LB2QN:01575:02502")
            {

            }
            _binWrtr.Write(ComputeReadByteSize(read));//block_size
            _binWrtr.Write((Int32)Dict.Get(_refSeqToInt, read.Rname, -1));//refID            
            _binWrtr.Write((Int32)read.Pos - 1);//pos           

            //bin_mq_nl
            var l_read_name = (byte)(read.Qname.Length + 1);//l_read_name (+1 is because the string should be null terminated)
            var mq = (byte)read.Mapq;//MAPQ
            var tLen = Cigar.ComputeTLen(read.Cigar);
            var bin = TabixIndexFile.reg2bin(read.Pos, tLen);//bin                    
            _binWrtr.Write((UInt32)(bin << 16 | mq << 8 | l_read_name));
            
            _binWrtr.Write((UInt32)(((UInt16)read.Flag) << 16 | ((UInt16)read.Cigar.Count)));//flag_nc            
            _binWrtr.Write((Int32)read.Seq.Length);//l_seq
            _binWrtr.Write((Int32)Dict.Get(_refSeqToInt, read.Rnext, -1));//next_refID
            _binWrtr.Write((Int32)read.Pnext);//next_pos
            _binWrtr.Write((Int32)tLen);//tlen
            _binWrtr.Write(Encoding.UTF8.GetBytes(read.Qname));//read_name
            _binWrtr.Write((byte)'\0');//the string should be null-temrinated
            
            //cigar:
            foreach (var tok in read.Cigar)
            {
                _binWrtr.Write((UInt32)(tok.Length << 4 | optMap[tok.Op]));
            }

            //seq:
            for (int k = 0; k < read.Seq.Length; k += 2)
            {
                char neuc2 = '=';
                char neuc1 = read.Seq[k];
                if (k + 1 < read.Seq.Length)
                {
                    neuc2 = read.Seq[k + 1];
                }
                _binWrtr.Write((byte)((Dict.Get<char, ushort>(seqMap, neuc1, 0) << 4) | Dict.Get<char, ushort>(seqMap, neuc2, 0)));
            }

            //qual:
            _binWrtr.Write(Encoding.UTF8.GetBytes(read.Qual.Select(x => (char)(x - 33)).ToArray()));
            
            //auxiliary data
            foreach (var opt in read.Opts.Values)
            {
                _binWrtr.Write(opt.ToBamArray());
            }
        }
        
        public void WriteEndOfFile()
        {
            //_binWrtr.Write((byte)0x1f); 
            //_binWrtr.Write((byte)0x8b); 
            //_binWrtr.Write((byte)0x08); 
            //_binWrtr.Write((byte)0x04); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00);
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0xff); 
            //_binWrtr.Write((byte)0x06); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x42); 
            //_binWrtr.Write((byte)0x43);
            //_binWrtr.Write((byte)0x02); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x1b); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x03); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00);
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00); 
            //_binWrtr.Write((byte)0x00);
        }
        
        public void Dispose()
        {
            if (_binWrtr != null)
            {
                _binWrtr.Close();
                _binWrtr.Dispose();
            }
            
        }
    }
}
