using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.VCF
{
    public class VcfHeaderFormat : VcfMetaInfo
    {

        public VcfHeaderFormat(Dictionary<string, string> dictionary) 
            : base(dictionary)
        {

        }

        public VcfHeaderFormat(int capacity) 
            : base(capacity)
        {

        }

        public VcfHeaderFormat() 
            : base()
        {

        }

        public string Number
        {
            get
            {
                return Dict.Get(this, "Number");
            }
            set
            {
                Dict.Upsert(this, "Number", value);
            }
        }

        public string Type
        {
            get
            {
                return Dict.Get(this, "Type");
            }
            set
            {
                Dict.Upsert(this, "Type", value);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}_{1}", Id, Type);
        }

    }
}
