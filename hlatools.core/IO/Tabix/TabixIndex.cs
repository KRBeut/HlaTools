using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.Tabix
{
    public class TabixIndex : AmgDataObject
    {
        public TabixIndex()
        {

        }
        
        public Dictionary<int, TabixBin> bins;
        public int n_bin { get { return bins.Count; } }

        public List<TabixInterval> intervals;
        public int n_intv { get { return intervals.Count; } }

    }
}
