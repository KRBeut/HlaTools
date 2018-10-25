using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.Tabix
{
    public class TabixChunkComparer : IComparer<TabixChunk>
    {

        static TabixChunkComparer instance = null;
        public static TabixChunkComparer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TabixChunkComparer();
                }
                return instance;
            }
        }

        public int Compare(TabixChunk x, TabixChunk y)
        {
            return x.CompareTo(y);
        }
    }
}
