using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.VCF
{
    public class VcfHeaderStructuralvar : VcfMetaInfo
    {

        public VcfHeaderStructuralvar() 
            : base()
        {

        }

        public VcfHeaderStructuralvar(int capacity) 
            : base(capacity)
        {

        }

        public VcfHeaderStructuralvar(Dictionary<string, string> dictionary) 
            : base(dictionary)
        {
            
        }

    }
}
