using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.Utils
{
    public class DictDefinedStringComparer : StringComparer
    {
        Dictionary<string, int> _strToSortIndx;
        public DictDefinedStringComparer(Dictionary<string,int> strToSortIndx)
        {
            _strToSortIndx = strToSortIndx;
        }

        public override int Compare(string x, string y)
        {
            return _strToSortIndx[x].CompareTo(_strToSortIndx[y]);
        }

        public override bool Equals(string x, string y)
        {
            return _strToSortIndx[x].Equals(_strToSortIndx[y]);
        }

        public override int GetHashCode(string obj)
        {
            return obj.GetHashCode() + _strToSortIndx.GetHashCode();
        }
    }
}
