using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public interface ICoverRegionSequence : ICoverRegion
    {

        string Seq { get; }

    }
}
