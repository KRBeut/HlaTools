using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public abstract class RecordFileParser<RecordType> : IDisposable
    {
        protected Func<RecordType> recFactory;
        protected StringSplitOptions strSplitOpt = StringSplitOptions.None;
        protected char[] columnDelims = null;
        public TextReader BaseReader { get; protected set; }

        protected RecordFileParser()
        {

        }

        protected RecordFileParser(TextReader txtRdr, Func<RecordType> recFactory)
        {
            this.BaseReader = txtRdr;
            this.recFactory = recFactory;
        }

        protected virtual string[] GetTokensFromString(string str)
        {
            return str.Split(columnDelims, strSplitOpt);
        }

        public RecordType GetNextRecord()
        {
            string fileLine;
            RecordType rec = default(RecordType);
            while ((fileLine = this.BaseReader.ReadLine()) != null)
            {
                if (SkipLine(fileLine))
                {
                    continue;
                }
                rec = ParseFromLineTokens(GetTokensFromString(fileLine));
                break;
            }
            return rec;
        }

        public virtual bool SkipLine(string fileLine)
        {
            return false;
        }

        public IEnumerable<RecordType> GetRecords()
        {
            RecordType rec;
            while ((rec = GetNextRecord()) != null)
            {
                yield return rec;
            }
        }

        protected abstract RecordType ParseFromLineTokens(string[] lineTokens);

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
                BaseReader.Close();
                BaseReader.Dispose();
                recFactory = null;
            }
            disposed = true;
        }

        protected double ParseDbleOrDefault(string str, double def = -1.0)
        {
            if (!double.TryParse(str, out double dbl))
            {
                dbl = def;
            }
            return dbl;
        }

        protected int ParseIntOrDefault(string str, int def = int.MaxValue)
        {
            if (!int.TryParse(str, out int dbl))
            {
                dbl = def;
            }
            return dbl;
        }

    }
}
