using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class VariantGenotypeValue : VcfGenotype
    {

        public VariantGenotypeValue() 
            : base()
        {

        }

        public VariantGenotypeValue(int capacity)
            : base(capacity)
        {

        }

        public string Value { get; set; }

    }
}
