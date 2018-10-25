using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.VCF
{
    public class VcfHeaderInfo : VcfMetaInfo
    {

        public VcfHeaderInfo(Dictionary<string, string> dictionary) 
            : base(dictionary)
        {

        }

        public VcfHeaderInfo(int capacity)
            : base(capacity)
        {

        }

        public VcfHeaderInfo() 
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

        public string Source
        {
            get
            {
                return Dict.Get(this, "Source");
            }
            set
            {
                Dict.Upsert(this, "Source", value);
            }
        }

        public string Version
        {
            get
            {
                return Dict.Get(this, "Version");
            }
            set
            {
                Dict.Upsert(this, "Version", value);
            }
        }

        public bool IsAltAlleleValued()
        {
            return string.Compare(Type, "A", true) == 0;
        }

        public bool IsAlleleValued()
        {
            return string.Compare(Type, "R", true) == 0;
        }

        public bool IsGenotypeValued()
        {
            return string.Compare(Type, "G", true) == 0;
        }

        public bool IsValueNumberKnown()
        {
            return string.Compare(Type, ".", true) != 0;
        }

        public bool IsAFlag()
        {
            return string.Compare(Type, "FLAG", true) == 0;
        }

    }
}
