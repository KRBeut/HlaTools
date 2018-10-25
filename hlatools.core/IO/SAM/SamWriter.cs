using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    public class SamWriter : AmgDataObject
    {
        public SamWriter() 
            : base()
        {

        }

        
        private static SamHeader BuildSamHeader(IList<SamSeq> reads)
        {
            var hdr = new SamHeader();
            foreach (var r in reads)
            {
                SamRefSeq sq = null;
                if (hdr.SQ.TryGetValue(r.Rname, out sq))
                {
                    sq.SeqLen = Math.Max(sq.SeqLen, r.Pos+1 + Cigar.ComputeTLen(r.Cigar));
                }
                else
                {
                    hdr.SQ.Add(r.Rname, new SamRefSeq() { SeqName = r.Rname, SeqLen = r.Pos+1 + Cigar.ComputeTLen(r.Cigar) });
                }
            }
            return hdr;
        }

        public static void WriteToFile(StreamWriter strmWrtr, IList<SamSeq> reads)
        {
            var samWrtr = new SamWriter();
            var hdr = BuildSamHeader(reads);
            SamWriter.WriteHeader(strmWrtr, hdr);
            samWrtr.WriteRecords(strmWrtr, reads);
        }

        public static void WriteToFile(StreamWriter strmWrtr, SamHeader hdr, IEnumerable<SamSeq> reads)
        {
            var samWrtr = new SamWriter();
            SamWriter.WriteHeader(strmWrtr, hdr);
            samWrtr.WriteRecords(strmWrtr, reads);
        }

        public static void WriteToFile(string outputSamFilepath, SamHeader hdr, IEnumerable<SamSeq> reads)
        {
            using (var strmWrtr = new StreamWriter(outputSamFilepath))
            {
                WriteToFile(strmWrtr, hdr, reads);
            }
        }

        public void WriteRecords<T>(StreamWriter strm, IEnumerable<T> reads) where T : SamSeq
        {
            foreach (var read in reads)
            {
                WriteRecord(strm, read);
            }
        }

        public virtual void WriteRecord<T>(StreamWriter strm, T read) where T : SamSeq
        {
            strm.WriteLine(string.Join("\t",
                read.Qname, 
                (int)read.Flag, 
                read.Rname, 
                read.Pos, 
                read.Mapq,
                string.Join("", read.Cigar.Select(t => t.Length.ToString() + t.Op)), 
                read.Rnext, 
                read.Pnext, 
                read.Length, 
                read.Seq, 
                read.Qual,
                OptsToString(read.Opts.Values)
                ));
        }

        public string OptsToString(IEnumerable<SamSeqOpt> opts)
        {
            return string.Join("\t", opts.Select(o => OptToString(o)));
        }

        //readonly string simplOptFormat = "{0}:{1}:{2}";
        //readonly string arrayOptFormat = "{0}:{1}:{3}:{2}";

        public string OptToString(SamSeqOpt opt)
        {
            return opt.ToString();
        }

        public static void WriteHeader(TextWriter strm, SamHeader hdr)
        {
            if (hdr.HD != null && hdr.HD.Count > 0)
            {
                WriteHeaderSection(strm, "HD", hdr.HD);
            }
            if (hdr.SQ != null && hdr.SQ.Count > 0)
            {
                WriteHeaderSection(strm, "SQ", hdr.SQ);
            }
            if (hdr.RG != null && hdr.RG.Count > 0)
            {
                WriteHeaderSection(strm, "RG", hdr.RG);
            }
            if (hdr.PG != null && hdr.PG.Count > 0)
            {
                WriteHeaderSection(strm, "PG", hdr.PG);
            }
            WriteComments(strm, hdr.CO);
            foreach (var kvp in hdr)
            {
                foreach (var dict in kvp.Value)
                {
                    WriteHeaderSection(strm, kvp.Key, dict);
                }
            }
        }
        
        static void WriteComments(TextWriter strm, IEnumerable<string> comments)
        {
            foreach (var comment in comments)
            {
                WriteComment(strm, comment);
            }
        }

        static void WriteComment(TextWriter strm, string comment)
        {
            strm.WriteLine("@CO\t{0}", comment.Trim());
        }

        static void WriteHeaderSection<T>(TextWriter strm, string tag, Dictionary<string, T> values) where T : Dictionary<string, string>
        {
            foreach (var dict in values.Values)
            {
                WriteHeaderSection(strm, tag, dict);
            }
        }

        static void WriteHeaderSection(TextWriter strm, string tag, Dictionary<string, string> values)
        {
            strm.WriteLine("@{0}\t{1}", tag, string.Join("\t",values.Select(kvp => string.Format("{0}:{1}", kvp.Key, kvp.Value))).Trim());
        }

    }
}
