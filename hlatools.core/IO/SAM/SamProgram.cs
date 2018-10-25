using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    /// <summary>
    /// Program.
    /// </summary>
    public class SamProgram : Dictionary<string,string>
    {
        

        /// <summary>
        /// Program record identifier. Each @PG line must have a unique ID. The value of ID is used in the
        /// alignment PG tag and PP tags of other @PG lines.PG IDs may be modified when merging SAM
        /// files in order to handle collisions.
        /// </summary>
        public string Id
        {
            get
            {
                return Dict.Get(this, "ID");
            }
            set
            {
                Dict.Upsert(this, "ID", value);
            }
        }

        /// <summary>
        /// Program name
        /// </summary>
        public string ProgName
        {
            get
            {
                return Dict.Get(this, "PN");
            }
            set
            {
                Dict.Upsert(this, "PN", value);
            }
        }

        /// <summary>
        /// Command line
        /// </summary>
        public string CmndLine
        {
            get
            {
                return Dict.Get(this, "CL");
            }
            set
            {
                Dict.Upsert(this, "CL", value);
            }
        }

        /// <summary>
        /// Previous @PG-ID. Must match another @PG header’s ID tag. @PG records may be chained using PP
        /// tag, with the last record in the chain having no PP tag.This chain defines the order of programs
        /// that have been applied to the alignment.PP values may be modified when merging SAM files
        /// in order to handle collisions of PG IDs.The first PG record in a chain (i.e.the one referred to
        /// by the PG tag in a SAM record) describes the most recent program that operated on the SAM
        /// record.The next PG record in the chain describes the next most recent program that operated
        /// on the SAM record.The PG ID on a SAM record is not required to refer to the newest PG record
        /// in a chain. It may refer to any PG record in a chain, implying that the SAM record has been
        /// operated on by the program in that PG record, and the program(s) referred to via the PP tag.
        /// </summary>
        public string PrevProgName
        {
            get
            {
                return Dict.Get(this, "PP");
            }
            set
            {
                Dict.Upsert(this, "PP", value);
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        public string Desc
        {
            get
            {
                return Dict.Get(this, "DS");
            }
            set
            {
                Dict.Upsert(this, "DS", value);
            }
        }

        /// <summary>
        /// Program version
        /// </summary>
        public string ProgVersion
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

    }
}
