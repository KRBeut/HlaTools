using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    [Flags]
    public enum DnaStrand
    {
        Neither = 0x0,
        Forward = 0x1,
        Reverse = 0x2,
        Both = DnaStrand.Forward | DnaStrand.Reverse
    }
}
