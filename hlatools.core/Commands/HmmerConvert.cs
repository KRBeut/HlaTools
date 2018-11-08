using hlatools.core.DataObjects;
using hlatools.core.IO;
using hlatools.core.IO.SAM;
using hlatools.core.Utils;
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
                "\t-i/--input is the input hmmer -o filepath to be converted. Default is standard input.",
                "\t-c/--compression [on|off] output should be gzipped compressed. Default is on.");
        }

        public override string GetShortDescription()
        {
            return "converts hmmer -o output file to fa, fq, or sam format.";
        }

        public override string GetUsage()
        {
            return "amgcompbio hmmerConvert [-o/--output <outputFilePath>] [-c/--compression on/off] hmmer-oFile";
        }

        public override string Run(IDictionary<string, string> inputKvps)
        {

            string inputPath = null;
            if (!(inputKvps.TryGetValue("input", out inputPath) || inputKvps.TryGetValue("i", out inputPath)))
            {
                inputPath = null;
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


            using (var readStrm = GetReadStream(inputPath))
            using (var outputStrm = GetOutputStream(outputPath, format, compression))
            {
                ConvertHmmer(format, readStrm, outputStrm);
            }
            return string.Empty;
        }

        static StreamReader GetReadStream(string inputFilePath)
        {
            HmmerOuputParser h = new HmmerOuputParser();
            if (inputFilePath == null || inputFilePath == "-")
            {
                //get the reads from standard in
                var inputStrm = Console.OpenStandardInput();
                var strmRdr = new StreamReader(inputStrm);
                return strmRdr;
            }
            else
            {
                //get the reads from specified file(s)
                var strm = File.Open(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var strmRdr = inputFilePath.EndsWith(".gz") ? new StreamReader(new GZipStream(strm, CompressionMode.Decompress)) : new StreamReader(strm);
                return strmRdr;
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
        
        static string ConvertHmmer(string format, StreamReader readStrm, StreamWriter strmWrtr)
        {
            var h = new HmmerOuputParser();
            h.ParseHeader(readStrm, out string rName, out int rLen);
            var reads = h.ParseAlignments(readStrm);
            
            if (format == "sam")
            {
                var samRefSeq = new List<SamRefSeq>() { new SamRefSeq() { SeqName = rName, SeqLen = rLen } };
                var samHdr = new SamHeader() { SQ = samRefSeq.ToDictionary(r => r.SeqName, r => r) };
                SamWriter.WriteToFile(strmWrtr, samHdr, reads);
            }
            else
            {
                if (format.ToLower() == "fa")
                {
                    var faWrtr = new FastaWriter();
                    foreach (var read in reads)
                    {
                        read.Qname = string.Format("{0}|{1}", read.Qname, read.Flag & (SamFlag.PAIRED | SamFlag.READ1 | SamFlag.READ2));
                        if (read.Flag.HasFlag(SamFlag.REVERSESEQ))
                        {
                            read.Seq = new String(SeqUtils.RevComp(read.Seq).ToArray());
                        }
                        faWrtr.WriteRecord(strmWrtr, read);
                    }
                }
                else if (format.ToLower() == "fq")
                {
                    var wrtr = new FastqWriter();
                    foreach (var read in reads)
                    {
                        read.Qname = string.Format("{0}|{1}", read.Qname, read.Flag & (SamFlag.PAIRED | SamFlag.READ1 | SamFlag.READ2));
                        if (read.Flag.HasFlag(SamFlag.REVERSESEQ))
                        {
                            read.Seq = new String(SeqUtils.RevComp(read.Seq).ToArray());
                            read.Qual = new String(read.Qual.Reverse().ToArray());
                        }
                        wrtr.WriteRecord(strmWrtr, read);
                    }
                    
                }
            }
            return string.Empty;
        }

    }
}
