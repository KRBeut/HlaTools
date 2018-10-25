using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{

    /// <summary>
    /// Read group. Unordered multiple @RG lines are allowed.
    /// </summary>
    public class SamReadGroup : Dictionary<string,string>
    {
        /// <summary>
        /// Read group identifier. Each @RG line must have a unique ID. The value of ID is used in the RG
        /// tags of alignment records.Must be unique among all read groups in header section. Read group
        /// IDs may be modified when merging SAM files in order to handle collisions.
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
        /// Name of sequencing center producing the read.
        /// </summary>
        public string SeqCntrName
        {
            get
            {
                return Dict.Get(this, "CN");
            }
            set
            {
                Dict.Upsert(this, "CN", value);
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        public string Description
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
        /// Date the run was produced (ISO8601 date or date/time).
        /// </summary>
        public DateTime RunDate
        {
            get
            {
                return DateTime.Parse(Dict.Get(this, "DT"));
            }
            set
            {
                Dict.Upsert(this, "DT", value.ToString());
            }
        }

        /// <summary>
        /// Flow order. The array of nucleotide bases that correspond to the nucleotides used for each
        /// flow of each read.Multi-base flows are encoded in IUPAC format, and non-nucleotide flows by
        /// various other characters. Format: /\*|[ACMGRSVTWYHKDBN]+/
        /// </summary>
        public string FlowOrder
        {
            get
            {
                return Dict.Get(this, "FO");
            }
            set
            {
                Dict.Upsert(this, "FO", value);
            }
        }

        /// <summary>
        /// The array of nucleotide bases that correspond to the key sequence of each read.
        /// </summary>
        public string KeySeq
        {
            get
            {
                return Dict.Get(this, "KS");
            }
            set
            {
                Dict.Upsert(this, "KS", value);
            }
        }

        /// <summary>
        /// Library
        /// </summary>
        public string Library
        {
            get
            {
                return Dict.Get(this, "LB");
            }
            set
            {
                Dict.Upsert(this, "LB", value);
            }
        }

        /// <summary>
        /// Programs used for processing the read group.
        /// </summary>
        public string Programs
        {
            get
            {
                return Dict.Get(this, "PG");
            }
            set
            {
                Dict.Upsert(this, "PG", value);
            }
        }

        /// <summary>
        /// Predicted median insert size.
        /// </summary>
        public int PredMedLen
        {
            get
            {
                return int.Parse(Dict.Get(this, "PI"));
            }
            set
            {
                Dict.Upsert(this, "PI", value.ToString());
            }
        }

        /// <summary>
        /// Platform/technology used to produce the reads. Valid values: CAPILLARY, LS454, ILLUMINA,
        /// SOLID, HELICOS, IONTORRENT, ONT, and PACBIO.
        /// </summary>
        public string Platform
        {
            get
            {
                return Dict.Get(this, "PL");
            }
            set
            {
                Dict.Upsert(this, "PL", value);
            }
        }

        /// <summary>
        /// Platform model. Free-form text providing further details of the platform/technology used
        /// </summary>
        public string PlatformModel
        {
            get
            {
                return Dict.Get(this, "PM");
            }
            set
            {
                Dict.Upsert(this, "PM", value);
            }
        }

        /// <summary>
        /// Platform unit (e.g. flowcell-barcode.lane for Illumina or slide for SOLiD). Unique identifier.
        /// </summary>
        public string PlatformUnit
        {
            get
            {
                return Dict.Get(this, "PU");
            }
            set
            {
                Dict.Upsert(this, "PU", value);
            }
        }

        /// <summary>
        /// Sample. Use pool name where a pool is being sequenced.
        /// </summary>
        public string Sample
        {
            get
            {
                return Dict.Get(this, "SM");
            }
            set
            {
                Dict.Upsert(this, "SM", value);
            }
        }

    }
}
