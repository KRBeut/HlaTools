using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hlatools.core.IO.VCF
{
    public class VcfHeaderWorkflow : VcfMetaInfo
    {
        public VcfHeaderWorkflow(Dictionary<string, string> dictionary) 
            : base(dictionary)
        {

        }

        public VcfHeaderWorkflow(int capacity) 
            : base(capacity)
        {
        }

        
        public VcfHeaderWorkflow() 
            : base()
        {

        }

        public string Name
        {
            get
            {
                return Dict.Get(this, "Name");
            }
            set
            {
                Dict.Upsert(this, "Name", value);
            }
        }

    }
}
