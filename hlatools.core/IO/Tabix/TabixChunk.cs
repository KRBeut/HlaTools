using hlatools.core.DataObjects;
using System;

namespace hlatools.core.IO.Tabix
{
    public class TabixChunk : AmgDataObject, IComparable<TabixChunk>
    {

        public TabixChunk()
        {

        }

        public TabixChunk(ulong beg, ulong end) 
            : this()
        {
            cnk_beg = beg;
            cnk_end = end;
        }

        public int tid;
        public ulong cnk_beg;
        public ulong cnk_end;

        public int CompareTo(TabixChunk chnk)
        {
            return cnk_beg == chnk.cnk_beg ? 0 : ((cnk_beg < chnk.cnk_beg) ^ (cnk_beg < 0) ^ (chnk.cnk_beg < 0)) ? -1 : 1; // unsigned 64-bit comparison
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", cnk_beg, cnk_end);
        }

    }
}