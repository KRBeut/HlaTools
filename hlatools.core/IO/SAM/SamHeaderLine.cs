using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    /// <summary>
    /// The header line. The first line if present.
    /// </summary>
    public class SamHeaderLine : Dictionary<string, string>
    {

        /// <summary>
        /// Format version. Accepted format: /^[0-9]+\.[0-9]+$/
        /// </summary>
        public string FormatVersion
        {
            get
            {
                return Dict.Get(this, "VN");
            }
            set
            {
                Dict.Upsert(this, "VN", value);
            }
        }

        /// <summary>
        /// Sorting order of alignments. Valid values: unknown (default), unsorted, queryname and
        /// coordinate.For coordinate sort, the major sort key is the RNAME field, with order defined
        /// by the order of @SQ lines in the header.The minor sort key is the POS field.For alignments
        /// with equal RNAME and POS, order is arbitrary.All alignments with ‘*’ in RNAME field follow
        /// alignments with some other value but otherwise are in arbitrary order.
        /// </summary>
        public string SortOrder
        {

            get
            {
                return Dict.Get(this, "SO");
            }
            set
            {
                Dict.Upsert(this, "SO", value);
            }
        }
        
        /// <summary>
        /// Grouping of alignments, indicating that similar alignment records are grouped together but the
        /// file is not necessarily sorted overall.Valid values: none (default), query(alignments are grouped
        /// by QNAME), and reference(alignments are grouped by RNAME/POS)
        /// </summary>
        public string Grouping
        {
            get
            {
                return Dict.Get(this, "GO");
            }
            set
            {
                Dict.Upsert(this, "GO", value);
            }
        }

    }
}
