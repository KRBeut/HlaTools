using hlatools.core.DataObjects;
using hlatools.core.IO;
using hlatools.core.IO.SAM;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hlatools.core
{
    public class AssignXLoc : CommandVerb
    {
        public override string Verb
        {
            get
            {
                return "AssignXLoc";
            }
        }

        public override string GetParameterHelp()
        {
            return string.Join("\n","");
        }

        public override string GetShortDescription()
        {
            return "Assigns reads that map to multiple loci to a single locus with a simple ML analysis";
        }

        public override string GetUsage()
        {
            return "amgcompbio AssignXLoc -o outputFilepath.sam -i inputFilepath.[sam|bam]";
        }

        public override string Run(IDictionary<string, string> inputKvps)
        {
            string outputPath = null;
            if (inputKvps.TryGetValue("output", out outputPath) || inputKvps.TryGetValue("o", out outputPath))
            {
                outputPath = outputPath.Trim();
            }
            inputKvps.Remove("output");
            inputKvps.Remove("o");

            string inputPath = null;
            if (inputKvps.TryGetValue("input", out inputPath) || inputKvps.TryGetValue("i", out inputPath))
            {
                inputPath = inputPath.Trim();
            }
            inputKvps.Remove("input");
            inputKvps.Remove("i");

            int tol = 0;
            if (inputKvps.TryGetValue("tol", out string tolStr) || inputKvps.TryGetValue("t", out tolStr))
            {
                if (!int.TryParse(tolStr, out tol))
                {
                    Console.Error.WriteLine("WARNING: invalide tol value, \"{0}\". Defaulting to 0", tolStr);
                }
            }
            tol = Math.Abs(tol);
            inputKvps.Remove("tol");
            inputKvps.Remove("t");

            using (var outputStrm = GetOutputStream(outputPath))
            using (var outStrmWrtr = new StreamWriter(outputStrm))
            using (var bamPrsr = GetInputParser(inputPath))
            {
                var samHdr = bamPrsr.Header;
                var reads = RemoveXLocusReads(bamPrsr.GetRecords(), tol);
                SamWriter.WriteToFile(outStrmWrtr, samHdr, reads);
            }
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allReads"></param>
        /// <param name="tol">All read pairs with combined mapping score within tol of the max mapping score will be returned</param>
        /// <returns></returns>
        protected IEnumerable<SamSeq> RemoveXLocusReads(IEnumerable<SamSeq> allReads, int tol)
        {
            var wlkr = new ReadNameWalker(allReads);
            foreach (var readGroup in wlkr.ReadSets())
            {
                var readPairs = ReadPairUtils.PairReads(readGroup).ToList();
                if (readPairs.Count < 1)
                {
                    continue;
                }
                if (readPairs.Count == 1)
                {
                    var p = readPairs.First();
                    yield return p.ReadOne;
                    yield return p.ReadTwo;
                }
                else
                {
                    var maxPairScore = readPairs.Max(p => ScoreReadPair(p));
                    var bestReadPairs = readPairs.Where(p => ScoreReadPair(p) >= maxPairScore - tol).ToList();
                    if (bestReadPairs.Count > 1)
                    {
                        continue;
                    }
                    foreach (var bestPair in bestReadPairs)
                    {
                        yield return bestPair.ReadOne;
                        yield return bestPair.ReadTwo;
                    }
                }
            }
            yield break;
        }

        protected double ScoreReadPair(ReadPair p)
        {
            return (double)(p.ReadOne.Mapq + p.ReadTwo.Mapq);
            //return 1.0/(double)((p.ReadOne.Opts["HE"] as SamSeqFloatOpt).Value + (p.ReadTwo.Opts["HE"] as SamSeqFloatOpt).Value);
        }

        protected BamParser GetInputParser(string input)
        {
            var inputFileExtension = Path.GetExtension(input);
            Stream strm;
            if (input == "bam")
            {
                strm = Console.OpenStandardInput();
            }
            else
            {
                strm = File.Open(input, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            var strmRdr = new BinaryReader(strm);
            
            var bgzfStrm = new BgzfReader(strm);
            var bamPrsr = new BamParser(bgzfStrm, ()=>new SamSeq());
            return bamPrsr;
        }

        protected Stream GetOutputStream(string output)
        {
            if (string.IsNullOrWhiteSpace(output) || output == "-")
            {
                return Console.OpenStandardOutput();
            }
            var strm = File.Open(output, FileMode.Create, FileAccess.Write, FileShare.Read);
            return strm;
        }

    }
}
