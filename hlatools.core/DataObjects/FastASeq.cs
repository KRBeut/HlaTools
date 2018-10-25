using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class FastASeq : AmgDataObject
    {

        public FastASeq()
        {

        }

        public string Seq { get; set; }

        public string Qname { get; set; }

        public override string ToString()
        {
            return Qname;
        }

    }
}
