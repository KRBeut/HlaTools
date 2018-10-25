using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class BedFeature : SequenceFeature
    {
        public BedFeature() 
            : base()
        {

        }
        
        /// <summary>
        /// Bed file column 4 (Optional): name - Label to be displayed under the feature, if turned on in "Configure this page".
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Bed file column 7 (Optional): thickStart - coordinate at which to start drawing the feature as a solid rectangle
        /// </summary>
        public int ThickStart { get; set; }

        /// <summary>
        /// Bed file column 8 (Optional): thickEnd - coordinate at which to stop drawing the feature as a solid rectangle
        /// </summary>
        public int ThickEnd { get; set; }

        /// <summary>
        /// Bed file column 9 (Optional): itemRgb - an RGB colour value(e.g. 0,0,255). Only used if there is a track line with the value of itemRgb set to "on" (case-insensitive).
        /// </summary>
        public int[] ItemRgb { get; set; }

        /// <summary>
        /// Bed file column 10 (Optional): blockCount - the number of sub-elements(e.g.exons) within the feature
        /// </summary>
        public int BlockCount { get; set; }

        /// <summary>
        /// Bed file column 11 (Optional): blockSizes - the size of these sub-elements
        /// </summary>
        public int[] BlockSizes { get; set; }

        /// <summary>
        /// Bed file column 12 (Optional): blockStarts - the start coordinate of each sub-element
        /// </summary>
        public int[] BlockStarts { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name))
            {
                return string.Format("{0}({3}) {1}-{2}", Rname, Pos, End, Name);
            }
            else
            {
                return string.Format("{0} {1}-{2}", Rname, Pos, End);
            }
            
        }

    }
}
