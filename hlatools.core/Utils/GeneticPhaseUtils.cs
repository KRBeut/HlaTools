using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.Utils
{
    public class GeneticPhaseUtils
    {

        public static IEnumerable<Tuple<string, string>> EnumerateHaploPairs(List<List<string>> locGenos)
        {
            var locusAlleles = locGenos.FirstOrDefault();
            foreach (var locGeno in EnumerateGenotypes(locusAlleles))
            {
                if (locGenos.Count == 1)
                {
                    yield return Tuple.Create(locGeno.Item1, locGeno.Item2);                    
                }
                else
                {
                    foreach (var hap in EnumerateHaploPairs(locGenos.Skip(1).ToList()))
                    {
                        yield return Tuple.Create(string.Format("{0}-{1}", locGeno.Item1, hap.Item1), string.Format("{0}-{1}", locGeno.Item2, hap.Item2));
                        if (locGeno.Item1 != locGeno.Item2 && hap.Item1 != hap.Item2)
                        {
                            yield return Tuple.Create(string.Format("{0}-{1}", locGeno.Item2, hap.Item1), string.Format("{0}-{1}", locGeno.Item1, hap.Item2));
                        }
                    }
                }
            }
        }

        public static IEnumerable<Tuple<string,string>> EnumerateGenotypes(IEnumerable<string> alleles)
        {
            var uniqueLocAlleles = alleles.Distinct().ToList();
            if (uniqueLocAlleles.Count == 1)
            {
                var allele = uniqueLocAlleles.First();
                return new List<Tuple<string, string>>() { Tuple.Create(allele, allele) };
            }
            var combinations = uniqueLocAlleles.Select((value, index) => new { value, index })
                       .SelectMany(x => uniqueLocAlleles.Skip(x.index + 1),
                                   (x, y) => Tuple.Create(x.value, y));

            return combinations;
        }


    }
}
