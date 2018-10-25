using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace hlatools.core.IO
{
    public class NmdpHlaAlleleFreqFileParser : ColumnHeaderedRecordFileParser<NmdpAlleleFrequ>
    {

        int _alleleCols;

        public NmdpHlaAlleleFreqFileParser(TextReader txtRdr, Func<NmdpAlleleFrequ> recFactory) 
            : base(txtRdr, recFactory)
        {
        }

        protected override Dictionary<string, int> ParseHeader(TextReader txtRdr)
        {
            var dict = base.ParseHeader(txtRdr);
            _alleleCols = dict.Keys.ToList().IndexOf("EUR_freq");
            return dict;
        }

        protected override NmdpAlleleFrequ ParseFromLineTokens(string[] lineTokens)
        {
            var frequ = recFactory();

            //frequ.AlleleName = lineTokens[0];
            frequ.AlleleName = string.Join("-", Header.Keys.Take(_alleleCols).Zip(lineTokens.Take(_alleleCols), (loc, allele) => string.Format("{0}*{1}", loc, allele)));

            frequ.EUR_freq = ParseDbleOrDefault(lineTokens[_alleleCols] + 0);
            frequ.EUR_rank = ParseDbleOrDefault(lineTokens[_alleleCols + 1]);
            frequ.AFA_freq = ParseDbleOrDefault(lineTokens[_alleleCols + 2]);
            frequ.AFA_rank = ParseDbleOrDefault(lineTokens[_alleleCols + 3]);
            frequ.API_freq = ParseDbleOrDefault(lineTokens[_alleleCols + 4]);
            frequ.API_rank = ParseDbleOrDefault(lineTokens[_alleleCols + 5]);
            frequ.HIS_freq = ParseDbleOrDefault(lineTokens[_alleleCols + 6]);
            frequ.HIS_rank = ParseDbleOrDefault(lineTokens[_alleleCols + 7]);
            return frequ;
        }

        public static IEnumerable<NmdpAlleleFrequ> ParseFile(string filepath, Func<NmdpAlleleFrequ> recFactory = null)
        {
            if (recFactory == null)
            {
                recFactory = new Func<NmdpAlleleFrequ>(() => new NmdpAlleleFrequ());
            }

            using (var strm = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var strmRdr = new StreamReader(strm))
            using (var freqPrsr = new NmdpHlaAlleleFreqFileParser(strmRdr, recFactory))
            {
                foreach (var frequ in freqPrsr.GetRecords())
                {
                    yield return frequ;
                }
            }

        }
    }
}
