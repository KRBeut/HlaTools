using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class Gff3Feature : SequenceFeature
    {

        public Gff3Feature() 
            : base()
        {
            Attributes = new Dictionary<string, string>();
        }

        
        /// <summary>
        /// Column 2: Keyword identifying the source of the feature, 
        /// like a program (e.g. Augustus or RepeatMasker) or an 
        /// organization (like TAIR).
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Column 3: The feature type name, like "gene" or "exon". In a 
        /// well structured GFF file, all the children features always 
        /// follow their parents in a single block (so all exons of a 
        /// transcript are put after their parent "transcript" feature 
        /// line and before any other parent transcript line). In GFF3, 
        /// all features and their relationships should be compatible 
        /// with the standards released by the Sequence Ontology Project
        /// </summary>
        public string Feature { get; set; }
        
        /// <summary>
        /// Frame or phase of CDS features; it can be either one of 0, 1, 2 (for 
        /// CDS features) or "." (for everything else). Frame and Phase are not 
        /// the same, See following subsection.
        /// </summary>
        public ReadingFrame Frame { get; set; }

        /// <summary>
        /// All the other information pertaining to this feature. The format, 
        /// structure and content of this field is the one which varies the 
        /// most between the three competing file formats.
        /// </summary>
        public Dictionary<string,string> Attributes { get; set; }

    }
}
