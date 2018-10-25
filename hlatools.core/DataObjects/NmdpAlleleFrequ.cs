using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class NmdpAlleleFrequ
    {

        public NmdpAlleleFrequ()
        {

        }

        public string AlleleName { get; set; }
        
        public double EUR_freq { get; set; }
        public double EUR_rank { get; set; }
        public double AFA_freq { get; set; }
        public double AFA_rank { get; set; }
        public double API_freq { get; set; }
        public double API_rank { get; set; }
        public double HIS_freq { get; set; }
        public double HIS_rank { get; set; }

        public double MaxFrequ
        {
            get
            {
                return (new List<double>() { EUR_freq, AFA_freq, API_freq, HIS_freq }).Max();
            }        
        }

        public double MinFrequ
        {
            get
            {
                return (new List<double>() { EUR_freq, AFA_freq, API_freq, HIS_freq }).Min();
            }
        }

    }
}
