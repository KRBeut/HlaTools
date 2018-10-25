using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.VCF
{
    public class VcfHeader : AmgDataObject
    {

        public VcfHeader()
        {
            Individual = new Dictionary<string, VcfHeaderIndividual>();
            Sample = new Dictionary<string, VcfHeaderSample>();
            WorkFlow = new Dictionary<string, VcfHeaderWorkflow>();
            Info = new Dictionary<string, VcfHeaderInfo>();
            Filter = new Dictionary<string, VcfHeaderFilter>();
            Format = new Dictionary<string, VcfHeaderFormat>();
            MetaInfo = new Dictionary<string, string>();
            SampleByIndex = new Dictionary<int, string>();
            Contigs = new Dictionary<string, VcfHeaderContig>();
            AltAlleles = new Dictionary<string, VcfHeaderStructuralvar>();
        }

        public string FileFormat { get; set; }

        public string fileDate { get; set; }

        public string Source { get; set; }

        public string Rname { get; set; }

        public Dictionary<string, VcfHeaderContig> Contigs { get; set; }

        public string Phasing { get; set; }
                
        public Dictionary<string, VcfHeaderIndividual> Individual { get; set; }

        public Dictionary<string, VcfHeaderSample> Sample { get; set; }

        public Dictionary<string, VcfHeaderWorkflow> WorkFlow { get; set; }

        public Dictionary<string, VcfHeaderInfo> Info { get; set; }

        public Dictionary<string, VcfHeaderFilter> Filter { get; set; }

        public Dictionary<string, VcfHeaderFormat> Format { get; set; }

        public Dictionary<string, VcfHeaderStructuralvar> AltAlleles { get; set; }

        public Dictionary<string, string> MetaInfo { get; set; }

        public Dictionary<int, string> SampleByIndex { get; set; }

    }
}
