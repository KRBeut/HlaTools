using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class HmmerTbloutRec
    {

        public HmmerTbloutRec()
        {

        }

        public string TargetName          { get; set; }
        public string TargetAccession     { get; set; }
        public string QueryName           { get; set; }
        public string QueryAccession      { get; set; }
        public int    Hmmfrom             { get; set; }
        public int    HmmTo               { get; set; }
        public int    Alifrom             { get; set; }
        public int    AliTo               { get; set; }
        public int    Envfrom             { get; set; }
        public int    EnvTo               { get; set; }
        public int    SqLen               { get; set; }
        public string Strand              { get; set; }
        public double EValue              { get; set; }
        public double Score               { get; set; }
        public double Bias                { get; set; }
        public string DescriptionOfTarget { get; set; }

    }
}
