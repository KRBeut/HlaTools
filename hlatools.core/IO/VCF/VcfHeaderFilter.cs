using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.VCF
{
    public class VcfHeaderFilter : VcfMetaInfo
    {
        public VcfHeaderFilter(Dictionary<string, string> dictionary)
            : base(dictionary)
        {

        }

        public VcfHeaderFilter(int capacity) 
            : base(capacity)
        {

        }

        public VcfHeaderFilter() 
            : base()
        {

        }

        

    }
}
