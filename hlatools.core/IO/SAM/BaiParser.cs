using hlatools.core.IO.Tabix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using hlatools.core.DataObjects;
using System.IO;

namespace hlatools.core.IO.SAM
{
    public class BaiParser : TabixIndexParser
    {

        public static new TabixIndexFile Parse(string tbiFilepath)
        {
            TabixIndexFile index = null;
            using (var tbiPrsr = FromFilepath(tbiFilepath))
            {
                index = tbiPrsr.Parse();
            }
            return index;
        }

        public static new BaiParser FromFilepath(string tbiFilepath)
        {
            BaiParser tbiPrsr = null;
            var strm = File.OpenRead(tbiFilepath);
            Stream bgzfStrm = new BgzfReader(strm);
            tbiPrsr = new BaiParser(bgzfStrm);
            return tbiPrsr;
        }

        public BaiParser(Stream bgzfStream) 
            : base(bgzfStream)
        {
        }

        int n_ref;

        protected override TabixHeader ParseHeader(BinaryReader binrdr, out string[] indexNames)
        {
            TabixHeader header = new TabixHeader
            {
                magic = new String(binrdr.ReadChars(4))// read "BAI\1"
            };
            n_ref = binrdr.ReadInt32();
            header.n_ref = n_ref;
            indexNames = Enumerable.Range(0, n_ref).Select(x => x.ToString()).ToArray();
            return header;
        }
        
    }
}
