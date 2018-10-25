using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class FastQSeq : FastASeq
    {
        public FastQSeq() 
            : base()
        {

        }

        public string Qual { get; set; }

        public string QualId { get; set; }

    }
}
