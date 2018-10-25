using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public class StockholmWriter
    {

        public StockholmWriter()
        {

        }

        public static void WriteAlignments(Dictionary<string,string> alignments, string filePath)
        {
            var stoWrtr = new StockholmWriter();
            using (var stoFileWrtr = File.Open(filePath,FileMode.Create,FileAccess.Write,FileShare.Read))
            using (var txtWrtr = new StreamWriter(stoFileWrtr))
            {
                stoWrtr.Write(alignments, txtWrtr);
            }
        }

        public static readonly string header = "# STOCKHOLM 1.0\n";
        public static readonly string footer = "//";

        public void Write(Dictionary<string, string> alignments, TextWriter txtWrtr)
        {
            txtWrtr.WriteLine(header);
            foreach (var kvp in alignments)
            {
                txtWrtr.WriteLine("{0} {1}", kvp.Key, kvp.Value);
            }
            txtWrtr.Write(footer);
        }

    }
}
