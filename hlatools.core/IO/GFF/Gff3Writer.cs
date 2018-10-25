using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hlatools.core
{
    public class Gff3Writer
    {

        public static void WriteRecords(TextWriter txtWrtr, IEnumerable<Gff3Feature> gff3Feats)
        {
            txtWrtr.WriteLine("##gff-version 3");
            foreach (var feat in gff3Feats)
            {
                string atts = string.Empty;
                if (feat.Attributes != null)
                {
                    atts = Dict.ToString(feat.Attributes,";","=");
                }
                txtWrtr.WriteLine(string.Join("\t", 
                    feat.Rname, 
                    string.IsNullOrWhiteSpace(feat.Source) ? "." : feat.Source, 
                    feat.Feature, 
                    feat.Pos, 
                    feat.End, 
                    feat.Score, 
                    feat.Strand == DnaStrand.Neither ? "." : feat.Strand.ToString(), 
                    feat.Frame == ReadingFrame.None ? "." : feat.Frame.ToString(), 
                    atts));
            }
        }

    }
}
