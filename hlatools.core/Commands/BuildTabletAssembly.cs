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
    public class BuildTabletAssembly : CommandVerb
    {

        public BuildTabletAssembly()
        {

        }

        public override string Verb
        {
            get
            {
                return "BuildTabletAssembly";
            }
        }

        public override string GetParameterHelp()
        {
            return string.Join("\n", 
                "\t--reads/-r\tfilepath to sam file",
                "\t--output/-o\tfilepath of .tablet project xml file",
                "\t--pathType/-p\ttype of paths in .tablet file [rel|abs]. Default is rel",
                "\t--fasta/-f\tfilepath to reference fasta file. If no value is provided, then the consesnse will be computed and used.",
                "\t--anno/-a\tfilepath(s) to tablet-supported assembly annotations to be included in the project file");
        }

        public override string GetShortDescription()
        {
            return string.Format("");
        }

        public override string GetUsage()
        {
            return string.Format("");
        }

        public override string Run(IDictionary<string, string> inputKvps)
        {
            string readsPathStr = null;
            if (inputKvps.TryGetValue("reads", out readsPathStr) || inputKvps.TryGetValue("r", out readsPathStr))
            {
                readsPathStr = readsPathStr.Trim();
            }
            inputKvps.Remove("reads");
            inputKvps.Remove("r");
            
            foreach (var readsPath in readsPathStr.Split(';').Where(f => f != null))
            {
                string outputPath = null;
                if (inputKvps.TryGetValue("output", out outputPath) || inputKvps.TryGetValue("o", out outputPath))
                {
                    outputPath = outputPath.Trim();
                }
                else
                {
                    //assign the tablet filepath, if not provided
                    if (string.IsNullOrWhiteSpace(outputPath))
                    {
                        outputPath = Path.ChangeExtension(readsPath, ".tablet");
                    }
                }

                string pathType = null;
                if (inputKvps.TryGetValue("pathType", out pathType) || inputKvps.TryGetValue("p", out pathType))
                {
                    pathType = pathType.Trim();
                }
                else
                {
                    pathType = "rel";
                }

                string fastaPath = null;
                if (inputKvps.TryGetValue("fasta", out fastaPath) || inputKvps.TryGetValue("f", out fastaPath))
                {
                    fastaPath = fastaPath.Trim();
                }
                else
                {
                    //Build the fasta reference, if not provided, and the default file name does not exist
                    if (string.IsNullOrWhiteSpace(fastaPath))
                    {
                        fastaPath = Path.ChangeExtension(readsPath, ".fa");
                        if (!File.Exists(fastaPath))
                        {
                            IEnumerable<SamSeq> allReads = null;
                            if (Path.GetExtension(readsPath) == "bam")
                            {
                                var samPrsr = BamParser.FromFilepath(readsPath, () => new SamSeq());
                                allReads = samPrsr.GetRecords().ToList();
                            }
                            else
                            {
                                var samPrsr = SamParser.FromFilepath(readsPath, () => new SamSeq());
                                allReads = samPrsr.GetRecords().ToList();
                            }

                            var faSeqs = allReads.GroupBy(r => r.Rname).Select(g => new FastASeq() { Qname = g.Key, Seq = ConsensusBuilder.BuildConsensus(g, new Dictionary<int, Dictionary<char, int>>(), true) });
                            FastaWriter.WriteToFile(fastaPath, faSeqs);
                        }
                    }
                }

                string annoPath = null;
                if (inputKvps.TryGetValue("anno", out annoPath) || inputKvps.TryGetValue("a", out annoPath))
                {
                    annoPath = annoPath.Trim();
                }
                inputKvps.Remove("anno");
                inputKvps.Remove("a");
                List<string> annos = null;
                if (!string.IsNullOrWhiteSpace(annoPath))
                {
                    annos = annoPath.Split(';').ToList();
                }

                var finalReadsPath = readsPath;
                if (pathType.ToLower() == "rel")
                {
                    Uri tmp;
                    Uri rootUri = new Uri(outputPath);
                    tmp = new Uri(fastaPath);
                    fastaPath = rootUri.MakeRelativeUri(tmp).ToString();

                    tmp = new Uri(readsPath);
                    finalReadsPath = rootUri.MakeRelativeUri(tmp).ToString();

                    if (annos != null)
                    {
                        var tmpAnnos = new List<string>();
                        foreach (var f in annos.Where(f => f != null))
                        {
                            tmp = new Uri(f);
                            annoPath = rootUri.MakeRelativeUri(tmp).ToString();
                            tmpAnnos.Add(annoPath);
                        }
                    }
                }

                //build the assembly
                var tbltAssmbly = new TabletAssembly()
                {
                    reference = fastaPath,
                    asmbly = finalReadsPath,
                    annotation = annos
                };

                //save the assembly to file
                TabletAssemblyRepository.SaveAssembly(outputPath, tbltAssmbly);
            }

            return string.Empty;
        }
    }
}
