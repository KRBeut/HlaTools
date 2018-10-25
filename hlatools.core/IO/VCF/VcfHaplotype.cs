using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.VCF
{
    public class VcfHaplotype : AmgDataObject
    {
        public VcfHaplotype(string seq) 
            : this()
        {
            Seq = seq;
        }

        public VcfHaplotype()
        {

        }

        public string Seq { get; set; }

        public VcfAlleleType Type { get; set; }

        public VcfHeaderStructuralvar StructVar { get; set; }

    }
}
