using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.VCF
{
    public abstract class VcfMetaInfo : Dictionary<string,string>
    {

        public VcfMetaInfo() 
            : base()
        {

        }

        public VcfMetaInfo(int capacity)
            : base(capacity)
        {

        }

        public VcfMetaInfo(Dictionary<string, string> dictionary) 
            : base(dictionary)
        {
            
        }

        public string Id
        {
            get
            {
                return Dict.Get(this, "ID");
            }
            set
            {
                Dict.Upsert(this, "ID", value);
            }
        }

        public string Description
        {
            get
            {
                return Dict.Get(this, "Description");
            }
            set
            {
                Dict.Upsert(this, "Description", value);
            }
        }

    }
}
