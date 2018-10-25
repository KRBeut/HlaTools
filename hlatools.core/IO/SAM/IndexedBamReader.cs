using hlatools.core.DataObjects;
using hlatools.core.IO.Tabix;
using hlatools.core.IO.VCF;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    public class IndexedBamReader : IndexedBamReader<SamSeq, SamHeader>
    {
        public IndexedBamReader(BamParserCore<SamSeq, SamHeader> bamParser, TabixIndexFile index) 
            : base(bamParser, index)
        {
        }
    }

    public class IndexedBamReader<T,H> : TabixBinaryReader<T> where T: SamSeq where H : SamHeader, new()
    {

        static readonly string indexFileStr = ".bai";

        public static IndexedBamReader<T, H> FromFilepath(string vcfFilepath, Func<T> varfactory = null, string tbiFilepath = null)
        {
            var bamPrsr = BamParserCore<T,H>.FromFilepath(vcfFilepath, varfactory);

            if (tbiFilepath == null)
            {
                tbiFilepath = vcfFilepath + indexFileStr;
            }
            var index = BaiParser.Parse(tbiFilepath);

            var rdr = new IndexedBamReader<T,H>(bamPrsr, index);
            return rdr;
        }

        public BamParserCore<T, H> BamParser { get; protected set; }

        public IndexedBamReader(BamParserCore<T,H> bamParser, TabixIndexFile index) 
            : base(bamParser.BinRdr, index)
        {
            BamParser = bamParser;
        }

        protected override TabixIndex GetIndex(string rName)
        {
            SamRefSeq sq;
            if (!BamParser.Header.SQ.TryGetValue(rName,out sq))
            {
                return null;
            }
            var rNameIndex = sq.SortIndex.ToString();

            TabixIndex indx = Dict.Get(Index.indices, rNameIndex, null);
            return indx;
        }

        protected override IEnumerable<T> GetMatchingRecord(string id, int start, int end)
        {
            var mtchCnt = 0;
            //TODO: instead of parsing the entire record to then determine
            //that it is not in the requested range, we need to write code
            //that will parse just the position info, then parse the entire 
            //variant if we determine that it is in the requested region.
            foreach (var rec in BamParser.GetRecords())
            {
                if (rec.Rname == id && rec.Pos >= start && rec.Pos <= end)
                {
                    mtchCnt++;
                    yield return rec;
                }
                else if (mtchCnt > 0)
                {
                    //this is the case where we have returned some matching
                    //records, then over-shot the requested region. In this
                    //case we know we can stop iterating.
                    yield break;
                }
            }
        }
    }
}
