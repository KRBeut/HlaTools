using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hlatools.core.IO
{    
    public class FastqWriter
    {

        public static void WriteToFile(string outputFilepath, FastQSeq seq)
        {
            WriteToFile(outputFilepath, new List<FastQSeq>() { seq });
        }

        public static void WriteToFile(string outputFilepath, IEnumerable<FastQSeq> seqs)
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

        public FastqWriter()
        {

        }

        public void WriteRecords(TextWriter txtWrtr, IEnumerable<FastQSeq> seqs)
        {
            foreach (var seq in seqs)
            {
                WriteRecord(txtWrtr, seq);
            }
        }

        public void WriteRecord(TextWriter txtWrtr, FastQSeq seq)
        {
            txtWrtr.WriteLine("@{0}\n{1}\n+{2}\n{3}", seq.Qname, seq.Seq, seq.QualId, seq.Qual);
        }

    }
}
