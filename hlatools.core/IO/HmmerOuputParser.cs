using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace hlatools.core.IO
{
    public class HmmerOuputParser
    {
        
        public HmmerOuputParser()
        {

        }

        public void ParseHeader(TextReader txtRdr, out string rName, out int rLen)
        {
            string fileLine;
            while (txtRdr.Peek() == '#' && (fileLine = txtRdr.ReadLine()) != null)
            {
                //fast-forward over the header lines
            }
            txtRdr.ReadLine();//read the blank line after the header
            ParseMatchTable(txtRdr, out rName, out rLen);
        }

        protected static readonly char[] hitTableSplitChars = new char[] { ' ' };

        public IEnumerable<SamSeq> ParseAlignments(TextReader txtRdr, double minScore = double.MinValue)
        {
            foreach (var read in ParseAlignments(txtRdr))
            {
                yield return read;
            }
        }

        protected void ParseQueryLine(TextReader txtRdr, out string rName, out int rLen)
        {
            ParseHeader(txtRdr, out rName, out rLen);
            ParseQueryString(txtRdr.ReadLine(), out rName, out rLen);
        }

        protected void ParseQueryString(string queryLine, out string rName, out int rLen)
        {
            var toks = queryLine.Split(new char[] { ' ', '=', ']' }, StringSplitOptions.RemoveEmptyEntries);
            rName = toks[1];
            rLen = int.Parse(toks[3]);
        }

        protected void ParseMatchTable(TextReader txtRdr, out string rName, out int rLen, double minScore = double.MinValue)
        {
            string fileLine;
            rLen = 0;
            rName = string.Empty;
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
                    while ((fileLine = txtRdr.ReadLine()) != null 
                        && !fileLine.Trim().StartsWith("Annotation")
                        && !fileLine.Trim().StartsWith("Domain"))
                    {
                        //read to the start of the alignment section
                    }
                    break;
                    //var lineToks = fileLine.Split(hitTableSplitChars, StringSplitOptions.RemoveEmptyEntries);
                    //if (lineToks.Length < 6)
                    //{
                    //    if (string.IsNullOrWhiteSpace(fileLine) || fileLine.Trim().StartsWith("---"))
                    //    {
                    //        while ((fileLine = txtRdr.ReadLine()) != null 
                    //            && !fileLine.Trim().StartsWith("Annotation")
                    //            && !fileLine.Trim().StartsWith("Domain"))
                    //        {
                    //            //read to the start of the alignment section
                    //        }
                    //        break;
                    //    }
                    //}
                    //if (fileLine.Trim().StartsWith("---") || lineToks.Length < 5)
                    //{
                    //    continue;
                    //}

                    //float evalue;
                    //if (!float.TryParse(lineToks[0],out evalue))
                    //{
                    //    continue;
                    //}

                    //float score;
                    //if (!float.TryParse(lineToks[1], out score))
                    //{
                    //    continue;
                    //}
                    //if (score < minScore)
                    //{
                    //    continue;
                    //}
                    //if (float.IsNaN(maxScore))
                    //{
                    //    maxScore = score;
                    //}

                    //float bias;
                    //if (!float.TryParse(lineToks[2],out bias))
                    //{
                    //    continue;
                    //}

                    //var qName = lineToks[3];

                    //double start;
                    //if (!double.TryParse(lineToks[4],out start))
                    //{
                    //    continue;
                    //}

                    //double end;
                    //if (!double.TryParse(lineToks[5], out end))
                    //{
                    //    continue;
                    //}
                    //var description = lineToks.Length > 6 ? lineToks[6] : string.Empty;

                    //var read = new SamSeq()
                    //{
                    //    Mapq = (int)Math.Round(score),
                    //    Qname = qName,
                    //    Pos = (int)start-1
                    //};
                    //read.Opts.Add("HE", new SamSeqFloatOpt("HE") { Value = evalue });
                    //read.Opts.Add("HS", new SamSeqFloatOpt("HS") { Value = score });
                    //read.Opts.Add("HN", new SamSeqFloatOpt("HN") { Value = score/maxScore });
                    //read.Opts.Add("HB", new SamSeqFloatOpt("HB") { Value = bias });

                    //Dict.Upsert(dict, qName, read);
                    //dict.Add(qName, read);                    
                }
            }
        }

        public IEnumerable<SamSeq> ParseAlignments(TextReader txtRdr)
        {
            int n = 0;
            while (txtRdr.Peek() != -1)
            {
                var read = ParseAlignment(txtRdr);
                if (read == null)
                {
                    yield break;
                }
                n++;
                yield return read;
            }            
        }

        public SamSeq ParseAlignment(TextReader txtRdr)
        {

            var fileLine = txtRdr.ReadLine();//>> <read name>
            if (string.IsNullOrWhiteSpace(fileLine))
            {
                return null;//end of alignments
            }
            var readName = fileLine.Substring(3).Split()[0].Trim();
            var read = new SamSeq() { Qname = readName };

            fileLine = txtRdr.ReadLine();//column headers
            txtRdr.ReadLine();//dashes under the column headers
            fileLine = txtRdr.ReadLine();//match information

            var toks = fileLine.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var score = (float)double.Parse(toks[1]);
            var bias = (float)double.Parse(toks[2]);
            var evalue = (float)double.Parse(toks[3]);

            read.Opts.Add("HE", new SamSeqFloatOpt("HE") { Value = evalue });
            //read.Opts.Add("HS", new SamSeqFloatOpt("HS") { Value = score });
            read.Opts.Add("HB", new SamSeqFloatOpt("HB") { Value = bias });
            read.Mapq = (int)score;

            txtRdr.ReadLine();//blank line
            txtRdr.ReadLine();//Alignment:
            txtRdr.ReadLine();//score: <float> bits
            var alignedRefSeq = ParseAlignmentReferenceLine(txtRdr, read);//*gene name + alignmentStartPos + consensus sequence of hmm (Dots (.) in this line indicate insertions in the target sequence with respect to the model.)
            var seqMtch = ParseSequenceMatchLine(txtRdr.ReadLine());//matches between the query model and target sequence. A + indicates positive score, which can be interpreted as “conservative substitution”, with respect to what the model expects at that position.
            var alignedTargetSeq = ParseAlignmentTargetLine(txtRdr.ReadLine(), alignedRefSeq, read);//*read name + read start pos + read sequence ( Dashes (-) in this line indicate deletions in the target sequence with respect to the model.) + read end pos
            var pp = ParseBackwardsLine(txtRdr.ReadLine(), read);//*results from the backwards algorithm applied to the traceback matrix (The bottom line represents the posterior probability (essentially the expected accuracy) of each aligned residue.A 0 means 0 - 5 %, 1 means 5 - 15 %, and so on; 9 means 85 - 95 %, and a *means 95 - 100 % posterior probability.You can use these posterior probabilities to decide which parts of the alignment are welldetermined or not.You’ll often observe, for example, that expected alignment accuracy degrades around locations of insertion and deletion, which you’d intuitively expect. You’ll also see expected alignment accuracy degrade at the ends of an alignment – this is because “alignment accuracy” posterior probabilities currently not only includes whether the residue is aligned to one model position versus others, but also confounded with whether a residue should be considered to be homologous(aligned to the model somewhere) versus not homologous at all.)
            txtRdr.ReadLine();//blank line
            read.QualId = read.Rname;
            read.Qual = "*";//pp.Replace(".", "");
            
            //if the qname has been packed with additional read data, then parse it out and assign it appropriately
            //the format for this additional data is Qname|SamFlags|FullReadSeq|FullReadQvals
            //if any on field is missing (except Qname, of course) it should be set to '*'
            var qNameToks = read.Qname.Split('|');
            read.Qname = qNameToks[0];
            
            if (qNameToks.Length > 1 && int.TryParse(qNameToks[1], out int flags))
            {
                read.Flag = read.Flag | (SamFlag)flags;
            }
            if (qNameToks.Length > 2 && qNameToks[2] != "*")
            {
                read.Seq = qNameToks[2];

                //add soft-clipping to the end of the read as needed
                var cigLen = read.Cigar.ComputeQLen();
                var lenDif = read.Seq.Length - cigLen;
                if (lenDif > 0)
                {
                    read.Cigar.Add(new CigTok("S", lenDif));
                }

                if (read.Flag.HasFlag(SamFlag.REVERSESEQ))
                {
                    read.Seq = new String(SeqUtils.RevComp(read.Seq).ToArray());
                                        
                    //flip the soft clipping in the cigar
                    var first = read.Cigar.First();
                    var last = read.Cigar.Last();
                    if (first != last)
                    {
                        if (first.Op == "S" && last.Op == "S")
                        {
                            read.Cigar[0] = last;
                            read.Cigar[read.Cigar.Count - 1] = first;
                        }
                        else if (last.Op == "S")
                        {
                            read.Cigar.RemoveAt(read.Cigar.Count - 1);
                            read.Cigar.Insert(0, last);
                        }
                        else if (first.Op == "S")
                        {
                            read.Cigar.RemoveAt(0);
                            read.Cigar.Add(first);
                        }

                    }
                }
            }
            if (qNameToks.Length > 3 && qNameToks[3] != "*")
            {
                read.Qual = qNameToks[3];
                if (read.Flag.HasFlag(SamFlag.REVERSESEQ))
                {
                    read.Qual = new String(read.Qual.Reverse().ToArray());
                }
            }

            read.Rnext = "*";
            return read;
        }

        public string ParseAlignmentTargetLine(string targetLineStr, string alignedRefSeq, SamSeq read)
        {
            var toks = targetLineStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var targetStartPos = int.Parse(toks[1]);
            var alignedReadSeq = toks[2];
            var targetEndPos = int.Parse(toks[3]);

            read.Cigar = Cigar.FromAlignedSeqs(alignedRefSeq, alignedReadSeq);
            var startSoftClip = Math.Min(targetStartPos,targetEndPos) - 1;
            if (startSoftClip > 0)
            {
                var cigToks = new List<CigTok>() { new CigTok("S", startSoftClip) };
                cigToks.AddRange(read.Cigar);
                read.Cigar = new Cigar(cigToks);
            }

            if (targetStartPos > targetEndPos)
            {
                read.Flag = read.Flag | SamFlag.REVERSESEQ;
            }
            else
            {

            }

            read.Seq = new String(alignedReadSeq.Where(c => "atgcATGC".Contains(c)).ToArray());
            return alignedReadSeq;
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
