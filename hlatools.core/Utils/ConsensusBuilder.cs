using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core
{
    public class ConsensusBuilder
    {

        public static string BuildConsensus(IEnumerable<SamSeq> seqs, Dictionary<int, Dictionary<char, int>> posDict, bool offSetPadding = false, int minLength = int.MinValue)
        {
            var minCoord = int.MaxValue;
            var maxCoord = int.MinValue;
            if (posDict.Count > 0)
            {
                minCoord = posDict.Keys.Min();
                maxCoord = posDict.Keys.Max();
            }
            foreach (var seq in seqs)
            {
                var pos = seq.Pos;
                minCoord = pos < minCoord ? pos : minCoord;                
                var alignedSeq = Cigar.GetAlignedSeq(seq.Seq, seq.Cigar);
                foreach (var neuc in alignedSeq.ToUpper())
                {
                    if ("ATGC".Contains(neuc))
                    {
                        Dictionary<char, int> dict;
                        if (!posDict.TryGetValue(pos, out dict))
                        {
                            dict = new Dictionary<char, int>(6);
                            posDict.Add(pos, dict);
                        }
                        Dict.Addsert(dict, neuc, 1);
                    }
                    pos++;
                }
                maxCoord = pos > maxCoord ? pos : maxCoord;
            }

            StringBuilder strBlder;
            if (offSetPadding && maxCoord > 0)
            {
                strBlder = new StringBuilder(new String('N', Math.Max(0,minCoord-1)), maxCoord);
                //minCoord = 1;
            }
            else
            {
                strBlder = new StringBuilder(maxCoord - minCoord);
            }

            minCoord = Math.Max(1, minCoord);
            for (int i = minCoord; i < maxCoord; i++)
            {
                Dictionary<char, int> dict;
                if (!posDict.TryGetValue(i, out dict))
                {
                    strBlder.Append('N');
                }
                else
                {
                    strBlder.Append(dict.OrderBy(v=>-v.Value).First().Key);
                }
            }
            var consensusSeq = strBlder.ToString();
            if (consensusSeq.Length < minLength)
            {
                consensusSeq += new String('N', minLength - consensusSeq.Length);
            }
            return consensusSeq;
        }

    }
}
