using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core
{
    /// <summary>
    /// analyzes a read mapping to identify and uniquely assigne
    /// reads that are mapped to more than one locus
    /// </summary>
    public class CrossLocusReadAssigner
    {
        
        public CrossLocusReadAssigner()
        {
            
        }

        public List<string> AssignCrossLocusReads(IEnumerable<SamSeq> reads, Dictionary<string, Dictionary<string, double>> xLocInfo)
        {
            var readGrps = new HashSet<string>();
            foreach (var read in reads)
            {
                if (read.Qname.StartsWith("K00193:134:HH3LMBBXX:6:2112:23439:34055/2"))
                {
                    if (read.Rname == "HLA-U")
                    {

                    }
                }
                string rg;
                double xLocScore;
                Dictionary<string, double> dict;
                if (xLocInfo.TryGetValue(read.Qname, out dict) && dict.Count > 1)
                {
                    rg = string.Format("XLoc_{0}", string.Join("_", dict.Keys.OrderBy(k => k)));
                    xLocScore = (dict[read.Rname] / dict.Values.Min());
                }
                else
                {
                    rg = string.Format("{0}_Only", read.Rname);
                    xLocScore = 1.0;
                }

                read.ReadGroup = rg;
                if (!readGrps.Contains(rg))
                {
                    readGrps.Add(rg);
                }
                Dict.Upsert(read.Opts, "XS", new SamSeqFloatOpt("XS") { Value = (float)xLocScore });
            }
            return readGrps.ToList();
        }

        public Dictionary<string, Dictionary<string, double>> GatherCrossLocusReads(IEnumerable<SamSeq> reads, string scoreTag = "HE" , string xLocTag = "RG")
        {
            var xLocReads = new Dictionary<string, Dictionary<string, double>>();
            foreach (var read in reads)
            {
                Dictionary<string, double> dict;
                if (!xLocReads.TryGetValue(read.Qname, out dict))
                {
                    dict = new Dictionary<string, double>();
                    xLocReads.Add(read.Qname, dict);
                }
                var opt = Dict.Get(read.Opts, scoreTag);
                if (opt != null)
                {
                    double val;
                    double readVal = (double)(((SamSeqFloatOpt)opt).Value);
                    if (dict.TryGetValue(read.Rname, out val))
                    {
                        dict[read.Rname] = Math.Min(val, readVal);
                    }
                    else
                    {
                        dict.Add(read.Rname, readVal);
                    }
                }
            }
            return xLocReads;
        }

    }
}
