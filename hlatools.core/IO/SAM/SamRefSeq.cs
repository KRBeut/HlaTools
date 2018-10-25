using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    /// <summary>
    /// Reference sequence dictionary. The order of @SQ lines defines the alignment sorting order.
    /// </summary>
    public class SamRefSeq : Dictionary<string,string>
    {

        /// <summary>
        /// Reference sequence name. Each @SQ line must have a unique SN tag. The value of this field is used
        /// in the alignment records in RNAME and RNEXT fields.Regular expression: [!-)+-<>-~][!-~]*
        /// </summary>
        public string SeqName
        {
            get
            {
                return Dict.Get(this, "SN");
            }
            set
            {
                Dict.Upsert(this, "SN", value);
            }
        }

        /// <summary>
        /// Reference sequence length. Range: [1,(2^31)-1]
        /// </summary>
        public int SeqLen
        {
            get
            {
                return int.Parse(Dict.Get(this, "LN"));
            }
            set
            {
                Dict.Upsert(this, "LN", value.ToString());
            }
        }

        /// <summary>
        /// Genome assembly identifier
        /// </summary>
        public string AssemblyId
        {
            get
            {
                return Dict.Get(this, "AS");
            }
            set
            {
                Dict.Upsert(this, "AS", value);
            }
        }

        /// <summary>
        /// MD5 checksum of the sequence in the uppercase, excluding spaces but including pads (as ‘*’s)
        /// </summary>
        public string Md5Checksum
        {
            get
            {
                return Dict.Get(this, "M5");
            }
            set
            {
                Dict.Upsert(this, "M5", value);
            }
        }

        /// <summary>
        /// Species
        /// </summary>
        public string Species
        {
            get
            {
                return Dict.Get(this, "SP");
            }
            set
            {
                Dict.Upsert(this, "SP", value);
            }
        }

        /// <summary>
        /// URI of the sequence. This value may start with one of the standard protocols, e.g http: or ftp:.
        /// If it does not start with one of these protocols, it is assumed to be a file-system path.
        /// </summary>
        public string SeqUri
        {
            get
            {
                return Dict.Get(this, "UR");
            }
            set
            {
                Dict.Upsert(this, "UR", value);
            }
        }

        public int SortIndex { get; set; }

    }
}
