using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.Tabix
{
    public class TabixHeader : AmgDataObject
    {

        public TabixHeader()
        {

        }

        public string magic { get; set; }
        public int n_ref { get; set; }
        public int format { get; set; }
        public int col_seq { get; set; }
        public int col_beg { get; set; }
        public int col_end { get; set; }
        public char meta { get; set; }
        public int skip { get; set; }
    }
}
