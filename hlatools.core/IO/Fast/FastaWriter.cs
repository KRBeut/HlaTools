using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public class FastaWriter
    {
        
        public static void WriteToFile(string outputFilepath, FastASeq seq)
        {
            WriteToFile(outputFilepath, new List<FastASeq>() { seq });
        }

        public static void WriteToFile(string outputFilepath, IEnumerable<FastASeq> seqs)
        {
            if (outputFilepath == null)
            {
                using (var strm = Console.OpenStandardOutput())
                using (var strmWrtr = new StreamWriter(strm))
                {
                    var fastaWrtr = new FastaWriter();
                    fastaWrtr.WriteRecords(strmWrtr, seqs);
                }
            }
            else
            {
                using (var strm = File.Open(outputFilepath, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var strmWrtr = new StreamWriter(strm))
                {
                    var fastaWrtr = new FastaWriter();
                    fastaWrtr.WriteRecords(strmWrtr, seqs);
                }
            }            
        }

        public FastaWriter()
        {

        }

        public void WriteRecords(TextWriter txtWrtr, IEnumerable<FastASeq> seqs)
        {
            foreach (var seq in seqs)
            {
                WriteRecord(txtWrtr, seq);
            }
        }

        public void WriteRecord(TextWriter txtWrtr, FastASeq seq)
        {
            txtWrtr.WriteLine(">{0}\n{1}", seq.Qname, seq.Seq);
        }

    }
}
