using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.Fast
{
    public class FastQParser<T> where T: FastQSeq
    {
        TextReader _txtRdr;
        Func<T> _recFactory;

        public FastQParser(TextReader txtRdr, Func<T> recFactory)
        {
            _txtRdr = txtRdr;
            _recFactory = recFactory;
        }

        public IEnumerable<T> GetRecords()
        {
            T nxtRec;
            while ((nxtRec = GetNextRec()) != null)
            {
                yield return nxtRec;
            }
        }

        protected T GetNextRec()
        {
            string qNameLine, seqLine, qualNameLine, qualLine;
            T fastQSeq = default(T);
            if ((qNameLine = _txtRdr.ReadLine()) != null 
                && (seqLine= _txtRdr.ReadLine()) != null
                && (qualNameLine= _txtRdr.ReadLine()) != null
                && (qualLine = _txtRdr.ReadLine()) != null
                )
            {
                fastQSeq = _recFactory();
                fastQSeq.Qname = qNameLine.Substring(1);
                fastQSeq.Seq = seqLine;
                fastQSeq.QualId = qualNameLine.Substring(1);
                fastQSeq.Qual = qualLine;
            }
            return fastQSeq;
        }

    }
}
