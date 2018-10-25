using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.Tabix
{
    public class TabixIndexParser : IDisposable
    {

        public static TabixIndexFile Parse(string tbiFilepath)
        {
            TabixIndexFile index = null;
            using (var tbiPrsr = FromFilepath(tbiFilepath))
            {
                index = tbiPrsr.Parse();
            }
            return index;
        }

        public static TabixIndexParser FromFilepath(string tbiFilepath)
        {
            TabixIndexParser tbiPrsr = null;
            var strm = File.OpenRead(tbiFilepath);
            var bgzfStrm = new BgzfReader(strm);
            tbiPrsr = new TabixIndexParser(bgzfStrm);
            return tbiPrsr;
        }

        protected BinaryReader binRdr;

        public TabixIndexParser(Stream bgzfStream)
        {
            binRdr = new BinaryReader(bgzfStream);
        }

        protected string[] ParseIndexNames(BinaryReader binrdr)
        {
            var l_nm = binrdr.ReadInt32();
            var indxNames = (new String(binrdr.ReadChars(l_nm))).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            return indxNames;
        }

        protected TabixChunk ParseChunk(BinaryReader binrdr)
        {
            var tbiChunk = new TabixChunk();
            tbiChunk.cnk_beg = binrdr.ReadUInt64();
            tbiChunk.cnk_end = binrdr.ReadUInt64();
            return tbiChunk;
        }
        
        protected TabixBin ParseBin(BinaryReader binrdr)
        {
            var tbiBin = new TabixBin();
            tbiBin.bin = binrdr.ReadUInt32();
            var nChunks = binrdr.ReadInt32();
            var chunks = new List<TabixChunk>(nChunks);
            for (int i = 0; i < nChunks; i++)
            {
                var tbiChunk = ParseChunk(binrdr);
                chunks.Add(tbiChunk);
            }
            tbiBin.chunks = chunks;
            return tbiBin;
        }

        protected void ParseIndex(BinaryReader binrdr, TabixIndex tbiIndex)
        {
            var n_bin = binrdr.ReadUInt32();
            var bins = new Dictionary<int, TabixBin>((int)n_bin);
            for (int j = 0; j < n_bin; j++)
            {
                var tbiBin = ParseBin(binrdr);
                bins.Add((int)tbiBin.bin, tbiBin);
            }
            tbiIndex.bins = bins;
            var nIntv = binrdr.ReadInt32();
            var intervals = new List<TabixInterval>(nIntv);
            for (int n = 0; n < nIntv; n++)
            {
                var tbiInterval = new TabixInterval();
                tbiInterval.ioff = binRdr.ReadUInt64();
                intervals.Add(tbiInterval);
            }
            tbiIndex.intervals = intervals;
        }

        public TabixIndexFile Parse()
        {
            var tbi = new TabixIndexFile();
            string[] indxNames;
            tbi.header = ParseHeader(binRdr, out indxNames);
            
            tbi.indices = new Dictionary<string, TabixIndex>(indxNames.ToDictionary(ind => ind, ind => new TabixIndex()));
            foreach (var tbiIndex in tbi.indices)
            {                
                ParseIndex(binRdr, tbiIndex.Value);
            }

            var bytes = binRdr.ReadBytes(8);
            tbi.n_no_coor = bytes.Length == 8 ? BitConverter.ToUInt64(bytes, 0) : 0;
            return tbi;
        }

        protected virtual TabixHeader ParseHeader(BinaryReader binrdr, out string[] indexNames)
        {
            TabixHeader header = new TabixHeader();
            header.magic = new String(binrdr.ReadChars(4));// read "TBI\1"
            header.n_ref = binrdr.ReadInt32();
            header.format = binrdr.ReadInt32();
            header.col_seq = binrdr.ReadInt32();
            header.col_beg = binrdr.ReadInt32();
            header.col_end = binrdr.ReadInt32();
            header.meta = (char)binrdr.ReadInt32();
            header.skip = binrdr.ReadInt32();
            indexNames = ParseIndexNames(binRdr);
            return header;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (binRdr != null)
                {
                    binRdr.Close();
                    binRdr.Dispose();
                    binRdr = null;
                }
            }
        }
    }
}
