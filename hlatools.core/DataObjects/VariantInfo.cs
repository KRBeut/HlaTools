using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class VariantInfo : Dictionary<string,string>
    {

        public VariantInfo(int capacity) 
            : base(capacity)
        {

        }

        public VariantInfo(IDictionary<string,string> dictionary) 
            : base(dictionary)
        {

        }

        public VariantInfo() 
            : base()
        {

        }

        //ancestral allele
        public string AA
        {
            get
            {
                return Dict.Get(this, "AA");
            }
            set
            {
                Dict.Upsert(this, "AA", value);
            }
        }

        //allele count in genotypes, for each ALT allele, in the same order as listed
        public string AC
        {
            get
            {
                return Dict.Get(this, "AC");
            }
            set
            {
                Dict.Upsert(this, "AC", value);
            }
        }

        //allele frequency for each ALT allele in the same order as listed: use this when estimated from primary data, not called genotypes
        public string AF
        {
            get
            {
                return Dict.Get(this, "AF");
            }
            set
            {
                Dict.Upsert(this, "AF", value);
            }
        }

        //total number of alleles in called genotypes
        public string AN
        {
            get
            {
                return Dict.Get(this, "AN");
            }
            set
            {
                Dict.Upsert(this, "AN", value);
            }
        }

        //RMS base quality at this position
        public string BQ
        {
            get
            {
                return Dict.Get(this, "BQ");
            }
            set
            {
                Dict.Upsert(this, "BQ", value);
            }
        }

        //cigar string describing how to align an alternate allele to the reference allele
        public string CIGAR
        {
            get
            {
                return Dict.Get(this, "CIGAR");
            }
            set
            {
                Dict.Upsert(this, "CIGAR", value);
            }
        }

        //dbSNP membership
        public string DB
        {
            get
            {
                return Dict.Get(this, "DB");
            }
            set
            {
                Dict.Upsert(this, "DB", value);
            }
        }

        //combined depth across samples, e.g.DP= 154
        public string DP
        {
            get
            {
                return Dict.Get(this, "DP");
            }
            set
            {
                Dict.Upsert(this, "DP", value);
            }
        }

        //end position of the variant described in this record (for use with symbolic alleles)
        public string END
        {
            get
            {
                return Dict.Get(this, "END");
            }
            set
            {
                Dict.Upsert(this, "END", value);
            }
        }

        //membership in hapmap2
        public string H2
        {
            get
            {
                return Dict.Get(this, "H2");
            }
            set
            {
                Dict.Upsert(this, "H2", value);
            }
        }

        //membership in hapmap3
        public string H3
        {
            get
            {
                return Dict.Get(this, "H3");
            }
            set
            {
                Dict.Upsert(this, "H3", value);
            }
        }

        //RMS mapping quality, e.g. MQ=52
        public string MQ
        {
            get
            {
                return Dict.Get(this, "MQ");
            }
            set
            {
                Dict.Upsert(this, "MQ", value);
            }
        }

        //Number of MAPQ == 0 reads covering this record
        public string MQ0
        {
            get
            {
                return Dict.Get(this, "MQ0");
            }
            set
            {
                Dict.Upsert(this, "MQ0", value);
            }
        }

        //Number of samples with data
        public string NS
        {
            get
            {
                return Dict.Get(this, "NS");
            }
            set
            {
                Dict.Upsert(this, "NS", value);
            }
        }

        //strand bias at this position
        public string SB
        {
            get
            {
                return Dict.Get(this, "SB");
            }
            set
            {
                Dict.Upsert(this, "SB", value);
            }
        }

        //indicates that the record is a somatic mutation, for cancer genomics
        public string SOMATIC
        {
            get
            {
                return Dict.Get(this, "SOMATIC");
            }
            set
            {
                Dict.Upsert(this, "SOMATIC", value);
            }
        }

        //validated by follow-up experiment
        public string VALIDATED
        {
            get
            {
                return Dict.Get(this, "VALIDATED");
            }
            set
            {
                Dict.Upsert(this, "VALIDATED", value);
            }
        }

        //membership in 1000 Genomes
        public string KG
        {
            get
            {
                return Dict.Get(this, "1000G");
            }
            set
            {
                Dict.Upsert(this, "1000G", value);
            }
        }

    }
}
