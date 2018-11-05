using hlatools.core.DataObjects;
using hlatools.core.IO;
using hlatools.core.IO.SAM;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hlatools.core
{
    public class HmmerConvert : CommandVerb
    {
        public override string Verb
        {
            get
            {
                return "hmmerConvert";
            }
        }

        public override string GetParameterHelp()
        {
            return string.Join("\n",
                "Optional Parameters",
                "\t-f/--format [fa|fq|sam] desired output format. Deafult is fa.",
                "\t-o/--output is the filepath to the output file. Default is standard output.",
                "\t-i/--input is the input hmmer -o filepath(s) to be converted. Default is standard input.",
                "\t-c/--compression [on|off] output should be gzipped compressed. Default is on.");
        }

        public override string GetShortDescription()
        {
            return "converts hmmer -o output file(s) to fa, fq, or sam format.";
        }

        public override string GetUsage()
        {
            return "amgcompbio hmmerConvert [-o/--output <outputFilePath>] [-c/--compression on/off] hmmer-oFile [hmmer-oFile2, hmmer-oFile3, ....]";
        }

        public override string Run(IDictionary<string, string> inputKvps)
        {

            string inputPath = null;
            List<string> inputFilePaths = null;
            if (inputKvps.TryGetValue("input", out inputPath) || inputKvps.TryGetValue("i", out inputPath))
            {
                inputFilePaths = inputPath.Split(';').Distinct().Select(f => f.Trim()).ToList();
            }
            inputKvps.Remove("input");
            inputKvps.Remove("i");

            string outputPath = null;
            if (!(inputKvps.TryGetValue("output", out outputPath) || inputKvps.TryGetValue("o", out outputPath)))
            {
                //outputPath will be null, and the output will be sent to standard out
            }
            inputKvps.Remove("output");
            inputKvps.Remove("o");
                        
            string format = null;
            if (!(inputKvps.TryGetValue("f", out format) || inputKvps.TryGetValue("format", out format)))
            {
                //deafult format is fa
                format = "fa";
            }
            format = format.ToLower();
            inputKvps.Remove("format");
            inputKvps.Remove("f");

            string compression = null;
            if (!(inputKvps.TryGetValue("c", out compression) || inputKvps.TryGetValue("compression", out compression)))
            {
                //default is to gzip the fa or fq output
                compression = "on";
            }
            if (format == "sam")
            {
                //obviously, we don't want to gzip the sam output
                compression = "off";
            }
            compression = compression.ToLower();
            inputKvps.Remove("compression");
            inputKvps.Remove("c");
                                    

            var reads = GetReads(inputFilePaths);
            using (var outputStrm = GetOutputStream(outputPath, format, compression))
            {
                ConvertHmmer(format, reads, outputStrm);
            }
            return string.Empty;
        }

        static IEnumerable<SamSeq> GetReads(IList<string> inputFilePaths)
        {
            int rLen;
            string rName;
            IEnumerable<SamSeq> reads;
            if (inputFilePaths == null || (inputFilePaths.Count() == 1 && inputFilePaths.First() == "-"))
            {
                //get the reads from standard in
                var filepath = inputFilePaths.First();
                using (var inputStrm = Console.OpenStandardInput())
                using (var strmRdr = new StreamReader(inputStrm))
                {
                    HmmerOuputParser h = new HmmerOuputParser();
                    reads = h.Parse(strmRdr, out rName, out rLen).Values;
                    foreach (var r in reads)
                    {
                        yield return r;
                    }
                }
            }
            else
            {
                //get the reads from specified file(s)
                reads = inputFilePaths.SelectMany(f => HmmerOuputParser.Parse(f, out rName, out rLen).Values);
                foreach (var r in reads)
                {
                    yield return r;
                }
            }

            
        }

        static StreamWriter GetOutputStream(string output, string format, string compression)
        {
            Stream strm = null;
            if (string.IsNullOrWhiteSpace(output))
            {
                strm = Console.OpenStandardOutput();
            }
            else
            {
                strm = new FileStream(output,FileMode.Create,FileAccess.Write,FileShare.Read);
            }

            if (format != "sam")
            {
                if (compression == "on")
                {
                    strm = new GZipStream(strm, CompressionMode.Compress);
                }
            }
            var strmWrtr = new StreamWriter(strm);
            //strmWrtr.AutoFlush = true;
            return strmWrtr;
        }
        
        static string ConvertHmmer(string format, IEnumerable<SamSeq> reads, StreamWriter strmWrtr)
        {
            if (format == "sam")
            {
                SamWriter.WriteToFile(strmWrtr, reads.ToList());
            }
            else
            {
                //don't output duplicate reads in the fa or fq file format
                reads = reads.GroupBy(g => g.Qname).Select(g => g.First()).OrderBy(r => r.Qname);
                if (format.ToLower() == "fa")
                {
                    var faWrtr = new FastaWriter();
                    faWrtr.WriteRecords(strmWrtr, reads);
                }
                else if (format.ToLower() == "fq")
                {
                    var wrtr = new FastqWriter();
                    wrtr.WriteRecords(strmWrtr, reads);
                }
            }
            return string.Empty;
        }

    }
}
