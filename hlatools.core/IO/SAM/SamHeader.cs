using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    public class SamHeader : Dictionary<string,List<Dictionary<string,string>>>
    {
        public SamHeader()
        {
            HD = new SamHeaderLine();
            HD.FormatVersion = "1.4";
            HD.SortOrder = "unsorted";
            SQ = new Dictionary<string, SamRefSeq>();
            IntToRefSeq = new Dictionary<int, SamRefSeq>();
            RG = new Dictionary<string, SamReadGroup>();
            PG = new Dictionary<string, SamProgram>();
            CO = new List<string>();
        }

        public StringComparer GetSqOrder()
        {
            var dict = SQ.ToDictionary(s => s.Key, s => s.Value.SortIndex);
            return new DictDefinedStringComparer(dict);
        }

        public bool Uppend(string tag, Dictionary<string,string> val)
        {
            var retVal = true;
            List<Dictionary<string, string>> dict;
            if (!this.TryGetValue(tag, out dict))
            {
                retVal = false;
                dict = new List<Dictionary<string, string>>();
            }
            dict.Add(val);
            return retVal;
        }

        public SamHeaderLine HD { get; internal set; }

        public Dictionary<string,SamRefSeq> SQ { get; set; }

        public Dictionary<int, SamRefSeq> IntToRefSeq { get; internal set; }

        public Dictionary<string, SamReadGroup> RG { get; internal set; }

        public Dictionary<string, SamProgram> PG { get; internal set; }

        public IList<string> CO { get; internal set; }

    }
}
