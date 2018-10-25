using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.BED
{
    public class BedWriter : IDisposable
    {

        public static BedWriter FromFilepath(string outputBedFilepath, int nColumns = 3, bool compress = false)
        {
            System.IO.Stream strm = File.OpenWrite(outputBedFilepath);
            if (compress)
            {
                strm = new GZipStream(strm, CompressionMode.Compress);
            }
            var strmWrtr = new StreamWriter(strm);
            var bedWrtr = new BedWriter(strmWrtr, nColumns);
            return bedWrtr;
        }

        int _nColumns;
        TextWriter _txtWrtr;
        public BedWriter(TextWriter txtWrtr, int nColumns = 3)
        {
            _txtWrtr = txtWrtr;
            _nColumns = nColumns;
        }

        public void WriteRecord(BedFeature bed)
        {
            _txtWrtr.Write(string.Join("\t", bed.Rname, bed.Pos, bed.End));

            var extraFields = new List<string>(9);
            if (_nColumns >= 4)
            {
                extraFields.Add(bed.Name);
            }
            if (_nColumns >= 5)
            {
                extraFields.Add(bed.Score.ToString());
            }
            if (_nColumns >= 6)
            {
                extraFields.Add(bed.Strand == DnaStrand.Forward ? "+" : "-");
            }
            if (_nColumns >= 7)
            {
                extraFields.Add(bed.ThickStart.ToString());
            }
            if (_nColumns >= 8)
            {
                extraFields.Add(bed.ThickEnd.ToString());
            }
            if (_nColumns >= 9)
            {
                extraFields.Add(string.Join(",",bed.ItemRgb.Select(x=>x.ToString())));
            }
            if (_nColumns >= 10)
            {
                extraFields.Add(bed.BlockCount.ToString());
            }
            if (_nColumns >= 11)
            {
                extraFields.Add(string.Join(",", bed.BlockSizes.Select(x => x.ToString())));                
            }
            if (_nColumns >= 12)
            {
                extraFields.Add(string.Join(",", bed.BlockStarts.Select(x => x.ToString())));
            }

            if (extraFields.Count > 0)
            {
                _txtWrtr.Write("\t");
                _txtWrtr.WriteLine(string.Join("\t", extraFields));
            }
            else
            {
                _txtWrtr.WriteLine();
            }
        }

        public void Dispose()
        {
            _txtWrtr.Close();
            _txtWrtr.Dispose();
        }
    }
}
