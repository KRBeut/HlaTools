using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace hlatools.core.DataObjects
{
    public class Cigar : List<CigTok>
    {

        public static string MtchMismtch = "M";
        public static string Ins = "I";
        public static string Del = "D";
        public static string Skipped = "N";
        public static string SoftClipped = "S";
        public static string HardClipped = "H";        
        public static string Padded = "P";
        public static string Mtch = "=";
        public static string Mismtch = "X";
        public static Regex parsingRegex = new Regex(@"(?<len>\d+)(?<op>.)", RegexOptions.IgnoreCase & RegexOptions.Compiled & RegexOptions.CultureInvariant & RegexOptions.ExplicitCapture);

        public static Cigar FromAlignedSeq(string seq, bool useXAndEquals = false)
        {
            seq = seq.ToUpper();            
            var cigBldr = new CigarBuilder(useXAndEquals);
            foreach (var n in seq)
            {
                if ("atgcATGC".Contains(n))
                {
                    cigBldr.Append('M');//base match
                }
                else
                {
                    cigBldr.Append('D');//deletion
                }
            }
            return cigBldr.GetCigar();
        }

        public static Cigar FromAlignedSeqs(string refSeq, string querySeq, bool useXAndEquals = false)
        {
            var cigBldr = new CigarBuilder(useXAndEquals);
            refSeq = refSeq.ToUpper();
            querySeq = querySeq.ToUpper();
            for (int k = 0; k < refSeq.Length; k++)
            {
                var refChar = refSeq[k];
                var queryChar = querySeq[k];
                if (refChar == queryChar)
                {
                    cigBldr.Append('M');//base match
                }
                else
                {
                    var isRefABase = "atgcATGC".Contains(refChar);
                    var isQueryABase = "atgcATGC".Contains(queryChar);
                    
                    if (!isRefABase && !isQueryABase)
                    {
                        //ignore it
                    }
                    else if (!isRefABase)
                    {
                        cigBldr.Append('I');//insertion
                    }
                    else if (!isQueryABase)
                    {
                        cigBldr.Append('D');//deletion
                    }
                    else
                    {
                        cigBldr.Append('M');//base mismatch
                    }
                }
            }
            return cigBldr.GetCigar();
        }

        public static bool IsClipping(string op)
        {
            return op == SoftClipped || op == HardClipped;
        }

        public static bool IsInQseq(string op)
        {
            return op == MtchMismtch || op == Ins || op == SoftClipped || op == Mtch || op == Mismtch;
        }

        public static bool IsAlignedToTemplate(string op)
        {
            return op == MtchMismtch || op == Del || op == Skipped || op == Padded || op == Mtch || op == Mismtch;
        }

        public static bool IsDel(string op)
        {
            return op == Del;
        }

        public static bool IsSkipped(string op)
        {
            return op == Skipped;
        }

        public static int ComputeTLen(string cigStr)
        {
            var tLen = ComputeTLen(TokenizeCigar(cigStr));
            return tLen;
        }

        public static int ComputeTLen(IEnumerable<CigTok> cig)
        {
            var tLen = cig.Where(c => IsAlignedToTemplate(c.Op)).Sum(c => c.Length);
            return tLen;
        }

        public static string GetAlignedSeq(string seq, string cig)
        {
            return GetAlignedSeq(seq, TokenizeCigar(cig));
        }

        public static string GetAlignedSeq(string seq, IEnumerable<CigTok> cig)
        {
            return string.Join("", GetAlignedSegs(seq, cig));
        }

        public static string GetFullLengthAlignedSeq(int pos, string seq, string cigar, int length = -1, char paddingChar = '*')
        {
            return GetFullLengthAlignedSeq(pos, seq, TokenizeCigar(cigar), length, paddingChar);
        }

        public static string GetFullLengthAlignedSeq(int pos, string seq, IEnumerable<CigTok> cigar, int length = -1, char paddingChar = '*')
        {
            var FivePrimePadding = pos > 1 ? new string(paddingChar, pos - 1) : string.Empty;
            var alignedReadSeq = GetAlignedSeq(seq, cigar);
            var threePrimeOffset = length > 0 ? length - (pos + alignedReadSeq.Length - 1) : 0;
            var ThreePrimePadding = threePrimeOffset > 0 ? new string(paddingChar, threePrimeOffset) : string.Empty;
            return string.Format("{0}{1}{2}", FivePrimePadding, alignedReadSeq, ThreePrimePadding);
        }

        public static string Consolodate(string cigToks)
        {
            return CigToksToString(Consolodate(TokenizeCigar(cigToks)));
        }

        public static IEnumerable<CigTok> Consolodate(IEnumerable<CigTok> cigToks)
        {
            CigTok currentCigTok = null;
            foreach (var cigTok in cigToks)
            {
                if (currentCigTok == null)
                {
                    currentCigTok = cigTok;
                }
                else if (currentCigTok.Op == cigTok.Op)
                {
                    currentCigTok = new CigTok(currentCigTok.Op, currentCigTok.Length + cigTok.Length);
                }
                else
                {
                    yield return currentCigTok;
                    currentCigTok = cigTok;
                }
            }
        }

        public static IEnumerable<string> GetAlignedSegs(string seq, string cig)
        {
            return GetAlignedSegs(seq, TokenizeCigar(cig));
        }

        public static string Expand(IEnumerable<CigTok> cig)
        {
            var flatCig = string.Join("", cig.Select(c => new string(c.Op[0], c.Length)));
            return flatCig;
        }

        public static IEnumerable<CigTok> Compress(string expandedCig)
        {
            int currentCnt = 0;
            char currentOp = ' ';
            string cigStr = string.Empty;
            foreach (var c in expandedCig)
            {
                if (currentOp == ' ')
                {
                    currentOp = c;
                    currentCnt = 1;
                }
                else if (c == currentOp)
                {
                    currentCnt++;
                }
                else
                {
                    yield return new CigTok(currentOp.ToString(), currentCnt);
                    currentOp = c;
                    currentCnt = 1;
                }
            }
            yield return new CigTok(currentOp.ToString(), currentCnt);
        }

        public static KeyValuePair<string, Cigar> Subset(string seq, IEnumerable<CigTok> cig, int start, int length)
        {
            var pos = 1;
            var end = start + length;
            var cigStr = string.Empty;
            var seqStr = string.Empty;
            foreach (var x in Pairoff(seq,cig))
            {
                if (pos >= start)
                {                    
                    cigStr += x.Value;
                    if (IsInQseq(x.Value.ToString()))
                    {
                        pos++;
                        seqStr += x.Key;
                        if (pos > end)
                        {
                            break;
                        }
                    }
                }                
            }
            return new KeyValuePair<string, Cigar>(seqStr, new Cigar(Compress(cigStr)));
        }

        public static IEnumerable<KeyValuePair<char, char>> Pairoff(string seq, IEnumerable<CigTok> cig)
        {
            int seqIndx = 0;
            foreach (var cigTok in cig)
            {
                if (IsDel(cigTok.Op))
                {
                    for (int k = 0; k < cigTok.Length; k++)
                    {
                        yield return new KeyValuePair<char, char>('.', cigTok.Op[0]);
                    }
                }
                else if (IsSkipped(cigTok.Op))
                {
                    for (int k = 0; k < cigTok.Length; k++)
                    {
                        yield return new KeyValuePair<char, char>('N', cigTok.Op[0]);
                    }
                }
                else if (IsInQseq(cigTok.Op))
                {
                    if (IsAlignedToTemplate(cigTok.Op))
                    {
                        foreach (var n in seq.Substring(seqIndx, cigTok.Length))
                        {
                            yield return new KeyValuePair<char, char>(n, cigTok.Op[0]);
                        }                        
                    }
                    seqIndx += cigTok.Length;
                }
            }
        }

        public static IEnumerable<string> GetAlignedSegs(string seq, IEnumerable<CigTok> cig)
        {
            int seqIndx = 0;
            foreach (var cigTok in cig)
            {
                if (IsDel(cigTok.Op))
                {
                    yield return new string('.', cigTok.Length);
                }
                else if (IsSkipped(cigTok.Op))
                {
                    yield return new string('N', cigTok.Length);
                }
                else if (IsInQseq(cigTok.Op))
                {
                    if (IsAlignedToTemplate(cigTok.Op))
                    {
                        yield return seq.Substring(seqIndx, cigTok.Length);
                    }
                    seqIndx += cigTok.Length;
                }
            }
        }

        public static string CigToksToString(IEnumerable<CigTok> cigToks)
        {
            return string.Join("", cigToks.Select(t => t.Length.ToString() + t.Op));
        }

        public static IEnumerable<CigTok> TokenizeCigar(string cigStr)
        {
            var mtchs = parsingRegex.Matches(cigStr);
            for (int k = 0; k < mtchs.Count; k++)
            {
                var mtch = mtchs[k];
                var cigTok = new CigTok(mtch.Groups["op"].Value, int.Parse(mtch.Groups["len"].Value));
                yield return cigTok;
            }
        }

        public static Cigar FromString(string cigStr)
        {
            var cig = new Cigar(TokenizeCigar(cigStr));
            return cig;
        }

        public Cigar(IEnumerable<CigTok> collection) 
            : base(collection)
        {

        }

        public string GetAlignedSeq(string seq)
        {
            return GetAlignedSeq(seq, this);
        }

        public int ComputeTLen()
        {
            return ComputeTLen(this);
        }
        
        public override string ToString()
        {
            return CigToksToString(this);
        }

    }
}
