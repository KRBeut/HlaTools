using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public class FastAParser<T> : IDisposable where T : FastASeq
    {

        public static FastAParser<FastASeq> FromFilepath(string filepath, Func<FastASeq> varFactory = null)
        {
            if (varFactory == null)
            {
                varFactory = () => new FastASeq();
            }

            System.IO.Stream strm = File.OpenRead(filepath);
            if (filepath.EndsWith(".gz") || filepath.EndsWith(".zip"))
            {
                strm = new GZipStream(strm, CompressionMode.Decompress);
            }
            var txtRdr = new StreamReader(strm);
            var vcfPrsr = new FastAParser<FastASeq>(txtRdr, varFactory);
            return vcfPrsr;
        }

        TextReader _txtRdr;
        Func<T> _recFactory;

        public FastAParser(TextReader txtRdr, Func<T> recFactory)
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
            string qNameLine, seqLine;
            T fastQSeq = default(T);
            if ((qNameLine = _txtRdr.ReadLine()) != null && (seqLine = _txtRdr.ReadLine()) != null )
            {
                fastQSeq = _recFactory();
                fastQSeq.Qname = qNameLine.Substring(1);
                fastQSeq.Seq = seqLine;
                while (_txtRdr.Peek() != '>' && (seqLine = _txtRdr.ReadLine()) != null)
                {
                    fastQSeq.Seq += seqLine;
                }
            }
            return fastQSeq;
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Flag: Has Dispose already been called?
        bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                _txtRdr.Close();
                _txtRdr.Dispose();
                _recFactory = null;
            }
            disposed = true;
        }

    }
}
