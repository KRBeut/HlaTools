using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.Utils
{
    public static class DnaSequenceUtilities
    {

        public static string ReverseComplement(string seq)
        {
            var reverse = Reverse(seq);
            var reverseComplement = Complement(reverse);
            return reverseComplement;
        }

        public static string Reverse(string seq)
        {
            //if (seq == null)
            //{
            //    return null;
            //}
            int j = seq.Length - 1;
            string reverse = string.Empty;
            for (int k = seq.Length - 1; k > -1; k--)
            {
                reverse += seq[k];
            }
            return reverse;
        }

        public static string Complement(string seq)
        {

            string complement = string.Empty;
            foreach (char seqBase in seq)
            {
                complement += Complement(seqBase);
            }
            return complement;
        }

        public static char Complement(char seqBase)
        {
            char complement;
            switch (seqBase)
            {
                case 'A':
                    complement = 'T';
                    break;
                case 'T':
                    complement = 'A';
                    break;
                case 'G':
                    complement = 'C';
                    break;
                case 'C':
                    complement = 'G';
                    break;
                default:
                    complement = seqBase;
                    break;
            }
            return complement;
        }

        public static IEnumerable<string> GetAllKmers(string sequ, int k)
        {
            var nKmers = sequ.Length - k + 1;
            if (nKmers < 1)
            {
                yield break;
            }
            for (int j = 0; j < nKmers; j++)
            {
                var subSeqStr = sequ.Substring(j, k);
                if (!subSeqStr.Contains('.') && !subSeqStr.Contains('*'))
                {
                    yield return subSeqStr;
                }
            }
        }

        /// <summary>
        /// Computes the hamming distance between two strings of equal length. (a.k.a. the edit distance, a.k.a. the number of positions in which the strings differ)
        /// </summary>
        /// <param name="seq1"></param>
        /// <param name="seq2"></param>
        /// <returns></returns>
        public static int HammingDistance(string seq1, string seq2)
        {
            if (seq1.Length != seq2.Length)
            {
                throw new ArgumentException("seq1 and seq2 must be the same length");
            }

            var hammingDistance = 0;
            var seq1Enum = seq1.GetEnumerator();
            var seq2Enum = seq2.GetEnumerator();
            while (seq1Enum.MoveNext() && seq2Enum.MoveNext())
            {
                var base1 = seq1Enum.Current;
                var base2 = seq2Enum.Current;
                if ("atgcATGC".Contains(base1) && "atgcATGC".Contains(base2) && base1 != base2)
                {
                    hammingDistance++;
                }
            }
            return hammingDistance;
        }

        public static string SparseCompare(string refSeq, string querySeq, char mtchChar = '-', char padChar = '*')
        {
            var refEnumerator = refSeq.GetEnumerator();
            var queryEnumerator = querySeq.GetEnumerator();

            var strBldr = new StringBuilder();
            while (refEnumerator.MoveNext() && queryEnumerator.MoveNext())
            {
                if (refEnumerator.Current == padChar || queryEnumerator.Current == padChar)
                {
                    strBldr.Append(padChar);
                }
                else if (refEnumerator.Current != queryEnumerator.Current)
                {
                    strBldr.Append(queryEnumerator.Current);
                }
                else
                {
                    strBldr.Append(mtchChar);
                }
            }
            return strBldr.ToString();
        }

        public static Dictionary<string, string> DegenerateBaseDict = new Dictionary<string, string>()
        {
            {"ACGT","N"},
            {"ACG","V"},{"ACT","H"},{"AGT","D"},{"CGT","B"},
            {"AC","M"},{"AG","R"},{"AT","W"},{"CG","S"},{"CT","Y"},{"GT","K"}
        };

        public static string ConvertSparseAlignedSequenceToFullSequence(string referenceFullAllele, string alignedAllele)
        {
            var stringWrtr = new StringBuilder(alignedAllele.Length);
            foreach (var basePair in referenceFullAllele.Zip(alignedAllele.AsEnumerable(), (refBase, alignedBase) => new { refBase, alignedBase }))
            {
                if (basePair.alignedBase == '-')
                {
                    stringWrtr.Append(basePair.refBase);
                }
                else
                {
                    stringWrtr.Append(basePair.alignedBase);
                }
            }
            return stringWrtr.ToString();
        }

    }
}
