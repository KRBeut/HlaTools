using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace hlatools.core.Utils
{
    public class HgvsUtils
    {
        static Regex delRegEx = new Regex("c.(?<start>\\d+)del(?<seq>\\D+)", RegexOptions.IgnoreCase);
        static Regex dupRegEx = new Regex("c.(?<start>\\d+)dup(?<seq>\\D+)", RegexOptions.IgnoreCase);
        static Regex snpRegEx = new Regex("c.(?<start>\\d+)(?<wtSeq>\\D+)\\>(?<mutSeq>\\D+)", RegexOptions.IgnoreCase);
                                
        public static string GetMutantAaSeqFromDnaMut(string wtDna, string hgvsc, int upstrm = 10, bool isRna = false)
        {
            string mutSeq = null;
            if (delRegEx.IsMatch(hgvsc))
            {
                var mtch = delRegEx.Match(hgvsc);
                var start = int.Parse(mtch.Groups["start"].Value);
                var delSeq = mtch.Groups["seq"].Value;
                mutSeq = GetMutantAaSeqFromDnaDel(wtDna, start, delSeq, upstrm);
            }
            else if (dupRegEx.IsMatch(hgvsc))
            {
                var mtch = dupRegEx.Match(hgvsc);
                var start = int.Parse(mtch.Groups["start"].Value);
                var dupSeq = mtch.Groups["seq"].Value;
                mutSeq = GetMutantAaSeqFromDnaDup(wtDna, start, dupSeq, upstrm);
            }
            else if (snpRegEx.IsMatch(hgvsc))
            {
                var mtch = snpRegEx.Match(hgvsc);
                var start = int.Parse(mtch.Groups["start"].Value);
                var wldTypSeq = mtch.Groups["wtSeq"].Value;
                var mtSeq = mtch.Groups["mutSeq"].Value;
                mutSeq = GetMutantAaSeqFromDnaSnp(wtDna, start, wldTypSeq, mtSeq, upstrm);
            }
            return mutSeq;
        }

        public static string GetMutantAaSeqFromDnaSnp(string wtDna, int start, string wtSeq, string mtSeq, int upstrm)
        {
            var s = start - 1;//convert start to 0-based index
            var mutDnaSeq = wtDna.Remove(s, wtSeq.Length);
            mutDnaSeq = mutDnaSeq.Insert(s, mtSeq);

            var mutCodonStart = s - (s % 3);
            var mutCodonEnd = s + (mtSeq.Length % 3);
            var mutLen = mutCodonEnd - mutCodonStart + 1;

            var wtStrt = Math.Max(0, mutCodonStart - 3 * upstrm);
            var wtStrtLen = mutCodonStart - wtStrt;

            var wtEnd = mutCodonEnd + 1;
            var wtEndLen = Math.Min(mutDnaSeq.Length - wtEnd, 3 * upstrm);

            //var finalDnaSeq = mutDnaSeq.Substring(wtStrt, wtStrtLen)
            //    + mutDnaSeq.Substring(mutCodonStart, mutCodonEnd - mutCodonStart+1)
            //    + mutDnaSeq.Substring(wtEnd, wtEndLen);
            var finalDnaSeq = mutDnaSeq.Substring(wtStrt, wtStrtLen + wtEndLen + mutLen);
            
            var finalSeq = new string(SeqUtils.Translate(finalDnaSeq, true).ToArray());
            return finalSeq;
        }

        public static string GetMutantAaSeqFromDnaDel(string wtDna, int start, string delSeq, int upstrm)
        {
            var mutDnaSeq = wtDna.Remove(start-1, delSeq.Length);

            var wtEnum = SeqUtils.Translate(wtDna).GetEnumerator();
            var mutEnum = SeqUtils.Translate(mutDnaSeq).GetEnumerator();

            int k = 0;
            while (wtEnum.MoveNext() && mutEnum.MoveNext() 
                && mutEnum.Current == wtEnum.Current)
            {
                k++;
            }
                        
            var subStart = 3*(Math.Max(0, k - upstrm));
            var subLen = 3*k - subStart;
            var finalSeq = new string(SeqUtils.Translate(wtDna.Substring(subStart, subLen)).ToArray());
            finalSeq += new String(SeqUtils.Translate(mutDnaSeq.Substring(3*k), true).ToArray());
            return finalSeq;
        }
        
        public static string GetMutantAaSeqFromDnaDup(string wtDna, int start, string dupSeq, int upstrm)
        {
            var mutDnaSeq = wtDna.Insert(start - 1, dupSeq);

            var wtEnum = SeqUtils.Translate(wtDna).GetEnumerator();
            var mutEnum = SeqUtils.Translate(mutDnaSeq).GetEnumerator();

            int k = 0;
            while (wtEnum.MoveNext() && mutEnum.MoveNext()
                && mutEnum.Current == wtEnum.Current)
            {
                k++;
            }

            var subStart = 3 * (Math.Max(0, k - upstrm));
            var subLen = 3 * k - subStart;
            var finalSeq = new string(SeqUtils.Translate(wtDna.Substring(subStart, subLen)).ToArray());
            finalSeq += new String(SeqUtils.Translate(mutDnaSeq.Substring(3 * k), true).ToArray());
            return finalSeq;
        }

    }
}
