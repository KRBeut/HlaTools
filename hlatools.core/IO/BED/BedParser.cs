using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.BED
{
    public class BedParser : RecordFileParser<BedFeature>
    {

        public static IList<BedFeature> Parse(string bedFilepath, Func<BedFeature> varFactory = null)
        {
            if ( bedFilepath == null)
            {
                return null;
            }

            IList<BedFeature> beds = null;
            using (var bedPrsr = BedParser.FromFilepath(bedFilepath))
            {
                beds = bedPrsr.GetRecords().ToList();
            }
            return beds;
        }

        public static BedParser FromFilepath(string bedFilepath, Func<BedFeature> varFactory = null)
        {
            if (varFactory == null)
            {
                varFactory = () => new BedFeature();
            }

            System.IO.Stream strm = File.OpenRead(bedFilepath);
            if (bedFilepath.EndsWith(".gz"))
            {
                strm = new BgzfReader(strm);
            }
            var txtRdr = new StreamReader(strm);
            var vcfPrsr = new BedParser(txtRdr, varFactory);
            return vcfPrsr;
        }

        public BedParser(TextReader txtRdr, Func<BedFeature> bedFactory ) 
            : base(txtRdr,bedFactory)
        {

        }

        protected override BedFeature ParseFromLineTokens(string[] lineTokens)
        {
            var bed = new BedFeature();
            bed.Rname = lineTokens[0];
            bed.Pos = int.Parse(lineTokens[1]);
            bed.End = int.Parse(lineTokens[2]);

            if (lineTokens.Length >= 4)
            {
                bed.Name = lineTokens[3];
            }
            if (lineTokens.Length >= 5)
            {
                bed.Score = double.Parse(lineTokens[4]);
            }
            if (lineTokens.Length >= 6)
            {
                bed.Strand = lineTokens[5] == "+" ? DnaStrand.Forward : DnaStrand.Reverse;
            }
            if (lineTokens.Length >= 7)
            {
                bed.ThickStart = int.Parse(lineTokens[6]);
            }
            if (lineTokens.Length >= 8)
            {
                bed.ThickEnd = int.Parse(lineTokens[7]);
            }
            if (lineTokens.Length >= 9)
            {
                bed.ItemRgb = lineTokens[8].Split(',').Select(n => int.Parse(n)).ToArray();
            }
            if (lineTokens.Length >= 10)
            {
                bed.BlockCount = int.Parse(lineTokens[9]);
            }
            if (lineTokens.Length >= 11)
            {
                bed.BlockSizes = lineTokens[10].Split(',').Select(n => int.Parse(n)).ToArray();
            }
            if (lineTokens.Length >= 12)
            {
                bed.BlockStarts = lineTokens[11].Split(',').Select(n => int.Parse(n)).ToArray();
            }
            return bed;
        }
    }
}
