using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    [Serializable]
    public class TabletAssembly
    {

        public TabletAssembly()
        {
            annotation = new List<string>();
        }
        
        public string reference { get; set; }
        
        public string asmbly { get; set; }
        
        public List<string> annotation { get; set; }
        
        public string contig { get; set; }

        public int position { get; set; }

    }
}
