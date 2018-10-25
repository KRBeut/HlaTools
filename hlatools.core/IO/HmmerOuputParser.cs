using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace hlatools.core.IO
{
    public class HmmerOuputParser
    {
        
        public static Dictionary<string, SamSeq> Parse(string hmmerOutputFilepath, out string rName, out int rLen, double minScore = double.MinValue)
        {
            var prsr = new HmmerOuputParser();
            Dictionary<string, SamSeq> dict;
            using (var strm = File.Open(hmmerOutputFilepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var strmRdr = new StreamReader(strm))
            {
                dict = prsr.Parse(strmRdr, out rName, out rLen, minScore);
            }
            return dict;
        }

        public static void ParseQueryLine(string hmmerOutputFilepath, out string rName, out int rLen)
        {
            var prsr = new HmmerOuputParser();
            using (var strm = File.Open(hmmerOutputFilepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var strmRdr = new StreamReader(strm))
            {
                prsr.ParseQueryLine(strmRdr, out rName, out rLen);
            }
        }

        public HmmerOuputParser()
        {

        }

        protected void ParseHeader(TextReader txtRdr)
        {
            string fileLine;
            while (txtRdr.Peek() == '#' && (fileLine = txtRdr.ReadLine()) != null)
            {
                //fast-forward over the header lines
            }
            txtRdr.ReadLine();//read the blank line after the header
        }

        protected static readonly char[] hitTableSplitChars = new char[] { ' ' };

        public Dictionary<string, SamSeq> Parse(TextReader txtRdr, out string rName, out int rLen, double minScore = double.MinValue)
        {
            ParseHeader(txtRdr);
            var dict = ParseMatchTable(txtRdr, out rName, out rLen, minScore);
            if (dict != null && dict.Count > 0)
            {
                ParseAlignments(txtRdr, dict);
            }
            return dict;
        }

        protected void ParseQueryLine(TextReader txtRdr, out string rName, out int rLen)
        {
            ParseHeader(txtRdr);
            ParseQueryString(txtRdr.ReadLine(), out rName, out rLen);
        }

        protected void ParseQueryString(string queryLine, out string rName, out int rLen)
        {
            var toks = queryLine.Split(new char[] { ' ', '=', ']' }, StringSplitOptions.RemoveEmptyEntries);
            rName = toks[1];
            rLen = int.Parse(toks[3]);
        }

        protected Dictionary<string,SamSeq> ParseMatchTable(TextReader txtRdr, out string rName, out int rLen, double minScore = double.MinValue)
        {
            string fileLine;
            rLen = 0;
            rName = string.Empty;
            var dict = new Dictionary<string, SamSeq>();
            float maxScore = float.NaN;
            while ((fileLine = txtRdr.ReadLine()) != null)
            {
                if (fileLine.StartsWith("Query", StringComparison.CurrentCultureIgnoreCase))
                {
                    ParseQueryString(fileLine, out rName, out rLen);
                    var tmpTxt = txtRdr.ReadLine();//skip header text line
                    tmpTxt = txtRdr.ReadLine();//skip column headers
                    tmpTxt = txtRdr.ReadLine();//skip dashes under column headers
                }
                else
                {
                    var lineToks = fileLine.Split(hitTableSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (lineToks.Length < 6)
                    {
                        if (string.IsNullOrWhiteSpace(fileLine) || fileLine.Trim().StartsWith("---"))
                        {
                            while ((fileLine = txtRdr.ReadLine()) != null 
                                && !fileLine.Trim().StartsWith("Annotation")
                                && !fileLine.Trim().StartsWith("Domain"))
                            {
                                //read to the start of the alignment section
                            }
                            break;
                        }
                    }

                    if (fileLine.Trim().StartsWith("---") || lineToks.Length < 5)
                    {
                        continue;
                    }

                    float evalue;
                    if (!float.TryParse(lineToks[0],out evalue))
                    {
                        continue;
                    }
                                        
                    float score;
                    if (!float.TryParse(lineToks[1], out score))
                    {
                        continue;
                    }
                    if (score < minScore)
                    {
                        continue;
                    }
                    if (float.IsNaN(maxScore))
                    {
                        maxScore = score;
                    }

                    float bias;
                    if (!float.TryParse(lineToks[2],out bias))
                    {
                        continue;
                    }

                    var qName = lineToks[3];

                    double start;
                    if (!double.TryParse(lineToks[4],out start))
                    {
                        continue;
                    }

                    double end;
                    if (!double.TryParse(lineToks[5], out end))
                    {
                        continue;
                    }
                    var description = lineToks.Length > 6 ? lineToks[6] : string.Empty;

                    var read = new SamSeq()
                    {
                        Mapq = (int)Math.Round(score),
                        Qname = qName,
                        Pos = (int)start-1
                    };
                    read.Opts.Add("HE", new SamSeqFloatOpt("HE") { Value = evalue });
                    read.Opts.Add("HS", new SamSeqFloatOpt("HS") { Value = score });
                    read.Opts.Add("HN", new SamSeqFloatOpt("HN") { Value = score/maxScore });
                    read.Opts.Add("HB", new SamSeqFloatOpt("HB") { Value = bias });

                    Dict.Upsert(dict, qName, read);
                    //dict.Add(qName, read);                    
                }
            }
            return dict;
        }

        public void ParseAlignments(TextReader txtRdr, Dictionary<string, SamSeq> reads)
        {
            SamSeq read;
            string fileLine;
            while ((fileLine = txtRdr.ReadLine()) != null)
            {
                var readName = fileLine.Substring(3).Split()[0].Trim();
                if (reads.TryGetValue(readName, out read))
                {
                    ParseAlignment(txtRdr, read);
                }
                else
                {
                    break;//we have reached the alignments that are below the cutoff
                }
            }
        }

        public void ParseAlignment(TextReader txtRdr, SamSeq read)
        {
            //var fileLine = txtRdr.ReadLine();//>> <read name>
            var fileLine = txtRdr.ReadLine();//column headers
            txtRdr.ReadLine();//dashes under the column headers
            txtRdr.ReadLine();//match information
            txtRdr.ReadLine();//blank line
            txtRdr.ReadLine();//Alignment:
            txtRdr.ReadLine();//score: <double> bits
            var alignedRefSeq = ParseAlignmentReferenceLine(txtRdr, read);//*gene name + alignmentStartPos + consensus sequence of hmm (Dots (.) in this line indicate insertions in the target sequence with respect to the model.)
            var seqMtch = ParseSequenceMatchLine(txtRdr.ReadLine());//matches between the query model and target sequence. A + indicates positive score, which can be interpreted as “conservative substitution”, with respect to what the model expects at that position.
            var alignedTargetSeq = ParseAlignmentTargetLine(txtRdr.ReadLine(), read);//*read name + read start pos + read sequence ( Dashes (-) in this line indicate deletions in the target sequence with respect to the model.) + read end pos
            var pp = ParseBackwardsLine(txtRdr.ReadLine(), read);//*results from the backwards algorithm applied to the traceback matrix (The bottom line represents the posterior probability (essentially the expected accuracy) of each aligned residue.A 0 means 0 - 5 %, 1 means 5 - 15 %, and so on; 9 means 85 - 95 %, and a *means 95 - 100 % posterior probability.You can use these posterior probabilities to decide which parts of the alignment are welldetermined or not.You’ll often observe, for example, that expected alignment accuracy degrades around locations of insertion and deletion, which you’d intuitively expect. You’ll also see expected alignment accuracy degrade at the ends of an alignment – this is because “alignment accuracy” posterior probabilities currently not only includes whether the residue is aligned to one model position versus others, but also confounded with whether a residue should be considered to be homologous(aligned to the model somewhere) versus not homologous at all.)
            txtRdr.ReadLine();//blank line
            read.Cigar = Cigar.FromAlignedSeqs(alignedRefSeq, alignedTargetSeq);
            read.QualId = read.Rname;
            read.Qual = "*";//pp.Replace(".", "");
            read.Seq = new String(alignedTargetSeq.Where(c => "atgcATGC".Contains(c)).ToArray());

            var mtchOpt = new SamSeqStringOpt("MT");
            mtchOpt.Value = seqMtch.Replace(" ","-");
            //Dict.Upsert(read.Opts, "MT", mtchOpt);
        }

        public string ParseSequenceMatchLine(string mtchLine)
        {
            return mtchLine.TrimStart();
        }
        
        public string ParseBackwardsLine(string bckwrdsLine, SamSeq read)
        {
            var toks = bckwrdsLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Dict.Upsert(read.Opts, "PP", new SamSeqStringOpt("PP") { Value = toks[0] });
            //read.Opts.Add("PP", new SamSeqStringOpt("PP") { Value = toks[0] });
            return toks[0];
        }

        public string ParseAlignmentTargetLine(string targetLineStr, SamSeq read)
        {
            var toks = targetLineStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var targetStartPos = int.Parse(toks[1]);
            var alignedReadSeq = toks[2];
            var targetEndPos = int.Parse(toks[3]);

            if (targetStartPos > targetEndPos)
            {
                read.Flag = read.Flag | SamFlag.REVERSESEQ;
            }
            return alignedReadSeq;
        }

        public string ParseAlignmentReferenceLine(TextReader txtRdr, SamSeq read)
        {
            int pos;
            var refLineStr = txtRdr.ReadLine();
            var toks = refLineStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (!int.TryParse(toks[1],out pos))
            {
                refLineStr = txtRdr.ReadLine();
                toks = refLineStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                pos = int.Parse(toks[1]);
            }
            read.Rname = toks[0];
            read.Pos = pos-1;            
            return toks[2];
        }

    }
}
