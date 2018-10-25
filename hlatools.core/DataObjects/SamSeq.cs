using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class SamSeq : FastQSeq, ICoverRegionSequence, IReadOnlySamSeq
    {

        public SamSeq() 
            : base()
        {
            Opts = new Dictionary<string, SamSeqOpt>();
        }

        static SamSeq _defaulInstance = new SamSeq();
        public static SamSeq DefaulInstance
        {
            get
            {
                return _defaulInstance;
            }
        }

        public SamFlag Flag { get; set; }

        public string Rname{ get; set; }

        public int Pos { get; set; }

        public int Mapq { get; set; }

        public Cigar Cigar { get; set; }

        public string Rnext { get; set; }

        public int Pnext { get; set; }

        public int Length { get; set; }
        
        public string ReadGroup
        {
            get
            {
                SamSeqOpt rgOpt;
                string rg = null;
                if (Opts.TryGetValue("RG",out rgOpt))
                {
                    rg = ((SamSeqStringOpt)rgOpt).Value;
                }
                return rg;
            }

            set
            {
                SamSeqOpt rgOpt = Dict.Get(Opts,"RG");
                if (value == null && rgOpt != null)
                {
                    Opts.Remove("RG");
                }
                else
                {
                    if (rgOpt == null)
                    {
                        rgOpt = new SamSeqStringOpt("RG");
                        Opts.Add("RG", rgOpt);
                    }
                    ((SamSeqStringOpt)rgOpt).Value = value;
                }
            }
        }

        public Dictionary<string, SamSeqOpt> Opts
        {
            get;
            set;
        }

        public override string ToString()
        {
            return string.Format("{0}_Pos{1}",Qname,Pos);
        }

    }
}
