using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hlatools.core.IO.VCF
{
    public class VcfHeaderIndividual : VcfMetaInfo
    {
        public VcfHeaderIndividual(Dictionary<string, string> dictionary) 
            : base(dictionary)
        {

        }

        public VcfHeaderIndividual(int capacity) 
            : base(capacity)
        {
        }


        public VcfHeaderIndividual() 
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

    }
}
