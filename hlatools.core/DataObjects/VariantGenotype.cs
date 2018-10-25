using hlatools.core.IO.VCF;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class VcfGenotype : Dictionary<string,string>
    {

        public VcfGenotype() 
            : base()
        {
            Genotype = new List<VcfHaplotype>(2);
        }

        public VcfGenotype(int capacity) 
            : base(capacity)
        {
            Genotype = new List<VcfHaplotype>(2);
        }

        public bool IsPhased { get; set; }

        /// <summary>
        /// Returns true if the genotype is homozygous
        /// </summary>
        public bool IsHom
        {
            get
            {
                return Genotype[0] == Genotype[1];
            }
        }

        /// <summary>
        /// Returns true if the genotype is heterozygous
        /// </summary>
        public bool IsHet
        {
            get
            {
                return !IsHom;
            }
        }

        /// <summary>
        /// Returns true if the genotype is homozygous
        /// reference (e.g. 0/0 or 0|0), and false otherwise
        /// </summary>
        public bool IsHomRef
        {
            get
            {
                return IsHom && Genotype.Any(g => g.Type == VcfAlleleType.Reference);
            }
        }

        /// <summary>
        /// Returns true if the genotype is heterozygous
        /// reference (e.g. 0/1 or 0|1), and false otherwise
        /// </summary>
        public bool IsHetRef
        {
            get
            {
                return IsHet && Genotype.Any(g=>g.Type == VcfAlleleType.Reference);
            }
        }

        //IList<VcfHaplotype> genotype = new List<VcfHaplotype>(2);
        public IList<VcfHaplotype> Genotype
        {
            get;
            protected set;
        }

        public int ReadDepth
        {
            get { return int.Parse(Dict.Get(this, "DP","-1")); }
            set { Dict.Upsert(this, "DP", value.ToString()); }
        }

        public string Filter
        {
            get { return Dict.Get(this, "FT"); }
            set { Dict.Upsert(this, "FT", value); }
        }

        public string PhaseSet
        {
            get { return Dict.Get(this, "PS"); }
            set { Dict.Upsert(this, "PS", value); }
        }

        public double PhaseQuality
        {
            get { return double.Parse(Dict.Get(this, "PQ","-1")); }
            set { Dict.Upsert(this, "PQ", value.ToString()); }
        }

        public int MQ
        {
            get { return int.Parse(Dict.Get(this, "MQ", "-1")); }
            set { Dict.Upsert(this, "MQ", value.ToString()); }
        }

    }
}
