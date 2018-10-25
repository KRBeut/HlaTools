using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.VCF
{
    public class VcfHeaderContig : VcfMetaInfo
    {

        public VcfHeaderContig(Dictionary<string, string> dictionary) 
            : base(dictionary)
        {

        }

        public VcfHeaderContig(int capacity)
            : base(capacity)
        {

        }

        public VcfHeaderContig()
            : base()
        {

        }

        public string URL
        {
            get
            {
                return Dict.Get(this, "URL");
            }
            set
            {
                Dict.Upsert(this, "URL", value);
            }
        }

    }
}
