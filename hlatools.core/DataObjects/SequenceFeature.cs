using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class SequenceFeature : AmgDataObject, ICoverRegion
    {

        /// <summary>
        /// Math.Abs(Pos - End);
        /// </summary>
        public virtual int Length
        {
            get
            {
                return Math.Abs(Pos - End);
            }

        }

        /// <summary>
        /// Bed file column 3 (Required): chromEnd - End position of the feature in 
        /// standard chromosomal coordinates
        /// </summary>
        public int End
        {
            get;
            set;
        }

        /// <summary>
        /// Bed file column 2 (Required): chromStart - Start position of the feature in 
        /// standard chromosomal coordinates (i.e. first base is 0).
        /// </summary>
        public int Pos
        {
            get;
            set;
        }

        /// <summary>
        /// Bed file column 1 (Required): chrom - name of the chromosome or scaffold. 
        /// Any valid seq_region_name can be used, and chromosome names 
        /// can be given with or without the 'chr' prefix.
        /// </summary>
        public string Rname
        {
            get;
            set;
        }

        /// <summary>
        /// Bed file column 5 (Optional): score - A score between 0 and 1000. See track lines, below, for ways to configure the display style of scored data.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Bed file column 6 (Optional): strand - defined as + (forward) or - (reverse).
        /// </summary>
        public DnaStrand Strand { get; set; }
    }
}
