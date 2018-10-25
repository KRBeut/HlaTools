using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class CigTok : AmgDataObject
    {

        public CigTok(string op, int length) 
            : base()
        {
            Op = op;
            Length = length;
        }

        public int Length { get; set; }

        public string Op { get; set; }

        public override string ToString()
        {
            return string.Format("{0}{1}", Length, Op);
        }

    }
}
