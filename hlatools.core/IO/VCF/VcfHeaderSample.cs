using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hlatools.core.IO.VCF
{
    public class VcfHeaderSample : VcfMetaInfo
    {
        public VcfHeaderSample(Dictionary<string, string> dictionary) 
            : base(dictionary)
        {

        }

        public VcfHeaderSample(int capacity) 
            : base(capacity)
        {
        }


        public VcfHeaderSample() 
            : base()
        {

        }

        public string Name
        {
            get
            {
                return Dict.Get(this, "NAME");
            }
            set
            {
                Dict.Upsert(this, "NAME", value);
            }
        }

        public string Aliquot
        {
            get
            {
                return Dict.Get(this, "ALIQUOT_ID");
            }
            set
            {
                Dict.Upsert(this, "ALIQUOT_ID", value);
            }
        }

        public string Bam
        {
            get
            {
                return Dict.Get(this, "BAM_ID");
            }
            set
            {
                Dict.Upsert(this, "BAM_ID", value);
            }
        }

    }
}
