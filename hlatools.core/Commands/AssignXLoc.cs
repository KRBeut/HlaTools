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
            
            using (var outputStrm = GetOutputStream(outputPath))
            using (var outStrmWrtr = new StreamWriter(outputStrm))
            {
                var reads = GetInputRead(inputPath).ToList();
                reads = AssignXLocusReads(reads).ToList();
                SamWriter.WriteToFile(outStrmWrtr, reads);
            }
            
            return string.Empty;
        }

        protected IEnumerable<SamSeq> AssignXLocusReads(IEnumerable<SamSeq> allReads)
        {
            //Special (optional) logic to convert the read groups to the locus 
            //names so that we can see the read group coverage more clearly . . .
            //foreach (var rd in allReads)
            //{
            //    rd.Rname = rd.ReadGroup;
            //}

            //filter-out unpaired reads
            //allReads = allReads.GroupBy(r => r.Rname)
            //                   .SelectMany(g => g.GroupBy(r => r.Qname.Split(new char[] { '/', '_' })[0])
            //                                     .Where(gw => gw.Count() > 1)
            //                                     .SelectMany(gsm => gsm))
            //                                     .ToList();

            //assign reads to locus based on HN (normalized hmmer score)
            //allReads = allReads.Where(r => (r.Opts["HN"] as SamSeqFloatOpt).Value >= 0.80F).ToList();
            allReads = allReads.GroupBy(r => r.Qname)
                               .Select(g => g.OrderBy(r => -(r.Opts["HN"] as SamSeqFloatOpt).Value).First())
                               .ToList();

            //filter-out unpaired reads (again)
            allReads = allReads.GroupBy(r => r.Rname)
                               .SelectMany(g => g.GroupBy(r => r.Qname.Split(new char[] { '/', '_' })[0])
                                                 .Where(gw => gw.Count() > 1)
                                                 .SelectMany(gsm => gsm))
                                                 .ToList();


            //write the alignments to the sam file
            var xLocReadAssigner = new CrossLocusReadAssigner();
            var xLocReadInfo = xLocReadAssigner.GatherCrossLocusReads(allReads);
            var readGroups = xLocReadAssigner.AssignCrossLocusReads(allReads, xLocReadInfo);

            return allReads;
        }

        protected IEnumerable<SamSeq> GetInputRead(string input)
        {
            var inputFileExtension = Path.GetExtension(input);
            if (input == "sam" || inputFileExtension == ".sam")
            {
                Stream strm;
                if (input == "sam")
                {
                    strm = Console.OpenStandardInput();
                }
                else
                {
                    strm = File.Open(input, FileMode.Open, FileAccess.Read, FileShare.Read);
                }                
                using (var strmRdr = new StreamReader(strm))
                using (var samPrsr = new SamParser(strmRdr, () => new SamSeq()))
                {
                    foreach (var read in samPrsr.GetRecords())
                    {
                        yield return read;
                    }
                }
            }
            else if (input == "bam" || inputFileExtension == ".bam")
            {
                Stream strm;
                if (input == "bam")
                {
                    strm = Console.OpenStandardInput();
                }
                else
                {
                    strm = File.Open(input, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                                
                using (var strmRdr = new BinaryReader(strm))
                using (var bamPrsr = new BamParser(strmRdr, () => new SamSeq()))
                {
                    foreach (var read in bamPrsr.GetRecords())
                    {
                        yield return read;
                    }
                }
            }
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
