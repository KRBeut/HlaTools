using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.Utils
{
    public class Dict
    {

        public static string ToString<K,V>(Dictionary<K,V> dict, string sepStr = "\n", string delimStr = "\t")
        {
            return string.Join(sepStr, dict.Select(kvp => string.Format("{0}{1}{2}", kvp.Key,delimStr, kvp.Value)));
        }

        public static bool Appendsert<K>(Dictionary<K, string> dict, K key, string val, string delim = ";")
        {
            string str;
            var listExists = dict.TryGetValue(key, out str);
            if (listExists)
            {
                dict[key] = string.Format("{0}{1}{2}", str, delim, val);
            }
            else
            {
                dict.Add(key, val);
            }
            return listExists;
        }
        
        public static bool Addsert<K, V>(Dictionary<K, List<V>> dict, K key, V val)
        {
            List<V> list;
            var listExists = dict.TryGetValue(key, out list);
            if (!listExists)
            {
                list = new List<V>();
                dict.Add(key, list);
            }
            list.Add(val);
            return listExists;
        }

        public static bool Addsert<K, V>(Dictionary<K, IList<V>> dict, K key, V val)
        {
            IList<V> list;
            var listExists = dict.TryGetValue(key, out list);
            if (!listExists)
            {
                list = new List<V>();
                dict.Add(key, list);
            }
            list.Add(val);
            return listExists;
        }

        public static bool Addsert<K>(IDictionary<K, int> dict, K key, int val = 1)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] += val;
                return true;
            }
            dict.Add(key, val);
            return false;
        }
        
        public static bool Addsert<K>(IDictionary<K, double> dict, K key, double val = 1.0)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] += val;
                return true;
            }
            dict.Add(key, val);
            return false;
        }

        public static bool Upsert<K, V>(Dictionary<K, V> dict, Dictionary<K, V> d)
        {
            var retVal = true;
            foreach (var kvp in d)
            {
                retVal &= Upsert(dict, kvp.Key, kvp.Value);
            }
            return retVal;
        }

        public static Dictionary<string, string> FromTokens(IEnumerable<string> tokens,char delim = '=')
        {
            var dict = new Dictionary<string, string>(tokens.Count());
            foreach (var tok in tokens)
            {
                var kvp = tok.Split(delim);
                if (kvp.Length > 1)
                {
                    Dict.Upsert(dict, kvp[0], kvp[1]);
                }
                else
                {
                    Dict.Upsert(dict, kvp[0], null);
                }
            }
            return dict;
        }

        public static bool Upsert<K, V>(Dictionary<K,V> dict, K key, V val)
        {
            bool result;
            if (dict.ContainsKey(key))
            {
                dict[key] = val;
                result = true;
            }
            else
            {
                dict.Add(key, val);
                result = false;
            }
            return result;
        }

        public static V Get<K,V>(Dictionary<K,V> dict, K key, V v = default(V))
        {
            if (key == null)
            {
                return v;
            }
            V retVal;
            if (!dict.TryGetValue(key, out retVal))
            {
                retVal = v;
            }
            return retVal;
        }

    }
}
