using hlatools.core.DataObjects;
using hlatools.core.IO.Tabix;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.VCF
{
    public class IndexedVcfReader : TabixTextReader<Variant> 
    {

        static readonly string indexFileStr = ".tbi";

        public static IndexedVcfReader FromFilepath(string vcfFilepath, Func<Variant> varfactory = null, string tbiFilepath = null)
        {
            var vcfPrsr = VcfParser.FromFilepath(vcfFilepath, varfactory);

            if (tbiFilepath == null)
            {
                tbiFilepath = vcfFilepath + indexFileStr;
            }
            var index = TabixIndexParser.Parse(tbiFilepath);

            var rdr = new IndexedVcfReader(vcfPrsr, index);
            return rdr;
        }

        protected VcfParser vcfPrsr;

        public IndexedVcfReader(VcfParser vcfParser, TabixIndexFile index) 
            : base((StreamReader)vcfParser.BaseReader,index)
        {
            vcfPrsr = vcfParser;
        }
        
        protected override IEnumerable<Variant> GetMatchingRecord(string id, int start, int end)
        {
            var mtchCnt = 0;
            //TODO: instead of parsing the entire record to then determine
            //that it is not in the requested range, we need to write code
            //that will parse just the position info, then parse the entire 
            //variant if we determine that it is in the requested region.
            foreach (var rec in vcfPrsr.GetRecords())
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
