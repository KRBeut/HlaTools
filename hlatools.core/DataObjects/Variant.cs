using hlatools.core.IO.VCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class Variant : AmgDataObject, ICoverRegion
    {

        public Variant()
        {
            Info = new VariantInfo();
            Haplotypes = new List<VcfHaplotype>();
            SampleData = new Dictionary<string, VcfGenotype>();
        }

        public int Length { get { return Ref.Seq.Length; } }

        public int Pos { get; set; }

        public string Rname { get; set; }

        public string Id { get; set; }

        public VcfHaplotype Ref
        {
            get
            {
                return Haplotypes.FirstOrDefault();
            }
        }

        public IEnumerable<VcfHaplotype> ALT
        {
            get
            {
                return Haplotypes.Skip(1);
            }
        
        }

        public IList<VcfHaplotype> Haplotypes { get; set; }

        public double Qual { get; set; }

        public string Filter { get; set; }

        public bool IsFiltered
        {
            get
            {
                return string.Compare(Filter, "PASS", true) == 0;
            }
        }

        public IEnumerable<string> FailedFilters { get; set; }

        public VariantInfo Info { get; set; }
        
        public Dictionary<string, VcfGenotype> SampleData { get; set; }

        public IEnumerable<VcfHeaderFormat> Format { get; set; }

        
    }
}
