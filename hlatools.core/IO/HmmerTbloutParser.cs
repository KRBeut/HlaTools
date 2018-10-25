using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public class HmmerTbloutParser
    {

        TextReader _txtRdr;

        public HmmerTbloutParser(TextReader txtRdr)
        {
            _txtRdr = txtRdr;
        }

        public IEnumerable<HmmerTbloutRec> GetRecords()
        {
            var fileLine = _txtRdr.ReadLine();//read the column header line
            fileLine = _txtRdr.ReadLine();//read the header line
            while ((fileLine = _txtRdr.ReadLine()) != null)
            {
                var lineToks = fileLine.Split(new char[] { ' ','\t' });
                var rec = new HmmerTbloutRec();
                rec.TargetName = lineToks[0];
                rec.TargetAccession = lineToks[1];
                rec.QueryName = lineToks[2];
                rec.QueryAccession = lineToks[3];
                rec.Hmmfrom = int.Parse(lineToks[4]);
                rec.HmmTo = int.Parse(lineToks[5]);
                rec.Alifrom = int.Parse(lineToks[6]);
                rec.AliTo = int.Parse(lineToks[7]);
                rec.Envfrom = int.Parse(lineToks[8]);
                rec.EnvTo = int.Parse(lineToks[9]);
                rec.SqLen = int.Parse(lineToks[10]);
                rec.Strand = lineToks[11];
                rec.EValue = double.Parse(lineToks[12]);
                rec.Score = double.Parse(lineToks[13]);
                rec.Bias = double.Parse(lineToks[14]);
                rec.DescriptionOfTarget = lineToks[15];
                yield return rec;
            }
            
        }

    }
}
