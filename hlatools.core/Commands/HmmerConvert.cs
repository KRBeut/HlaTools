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
            HmmerOuputParser h = new HmmerOuputParser();
            if (inputFilePaths == null || (inputFilePaths.Count() == 1 && inputFilePaths.First() == "-"))
            {
                //get the reads from standard in
                var filepath = inputFilePaths.First();
                using (var inputStrm = Console.OpenStandardInput())
                using (var strmRdr = new StreamReader(inputStrm))
                {
                    h.ParseHeader(strmRdr, out string rName, out int rLen);
                    var reads = h.ParseAlignments(strmRdr);
                    foreach (var r in reads)
                    {
                        yield return r;
                    }
                }
            }
            else
            {
                //get the reads from specified file(s)
                foreach (var f in inputFilePaths)
                {
                    using (var strm = File.Open(f, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var strmRdr = f.EndsWith(".gz") ? new StreamReader(new GZipStream(strm, CompressionMode.Decompress)) : new StreamReader(strm))
                    {
                        h.ParseHeader(strmRdr, out string rName, out int rLen);
                        var reads = h.ParseAlignments(strmRdr);
                        foreach (var r in reads)
                        {
                            yield return r;
                        }

                        //var wlkr = new ReadNameWalker(reads.Where(r => r.Mapq > 79).OrderBy(r=>r.Qname));
                        //var readPairs = new List<List<SamSeq>>();
                        //foreach (var readList in wlkr.ReadSets())
                        //{
                        //    var bestRead1 = readList.Where(r => r.Flag.HasFlag(SamFlag.READ1)).OrderBy(r => -r.Mapq).FirstOrDefault();
                        //    var bestRead2 = readList.Where(r => r.Flag.HasFlag(SamFlag.READ2)).OrderBy(r => -r.Mapq).FirstOrDefault();
                        //    if (bestRead1 != null && bestRead2 != null)
                        //    {
                        //        //set the read paired flags for each read
                        //        bestRead1.Flag |= SamFlag.PAIRED;
                        //        bestRead2.Flag |= SamFlag.PAIRED;

                        //        //set the read properly paired flag, not neccessarily because
                        //        //the read is properly paired, but just so that samtools fixmate
                        //        //can unset the flag if it is not properly paired
                        //        bestRead1.Flag |= SamFlag.PROPER_PAIR;
                        //        bestRead2.Flag |= SamFlag.PROPER_PAIR;

                        //        //set the appropriate mate reverse strand flag
                        //        if (bestRead1.Flag.HasFlag(SamFlag.REVERSESEQ))
                        //        {
                        //            bestRead2.Flag |= SamFlag.MREVERSESEQ;
                        //        }
                        //        if (bestRead2.Flag.HasFlag(SamFlag.REVERSESEQ))
                        //        {
                        //            bestRead1.Flag |= SamFlag.MREVERSESEQ;
                        //        }

                        //        //set the Pnext for each read
                        //        bestRead1.Pnext = bestRead2.Pos;
                        //        bestRead2.Pnext = bestRead1.Pos;

                        //        //set Rnext for each read
                        //        if (bestRead1.Rname == bestRead2.Rname)
                        //        {
                        //            bestRead1.Rnext = "=";
                        //            bestRead2.Rnext = "=";
                        //        }
                        //        else
                        //        {
                        //            bestRead1.Rnext = bestRead2.Rname;
                        //            bestRead2.Rnext = bestRead1.Rname;
                        //        }

                        //        //compute and set the tempalte length (TLEN)
                        //        var maxCoord = Math.Max(bestRead1.Pos + bestRead1.Cigar.ComputeTLen(), bestRead2.Pos + bestRead2.Cigar.ComputeTLen());
                        //        var minCoord = Math.Min(bestRead1.Pos, bestRead2.Pos);
                        //        var tLen = maxCoord - minCoord;
                        //        bestRead1.Length = tLen;
                        //        bestRead2.Length = tLen;

                        //        yield return bestRead1;
                        //        yield return bestRead2;
                        //    }
                        //}
                    }
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
