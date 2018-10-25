using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public abstract class ColumnHeaderedRecordFileParser<RecordType> : HeaderedRecordFileParser<RecordType, Dictionary<string,int>>
    {
        public ColumnHeaderedRecordFileParser() 
            : base()
        {

        }

        public ColumnHeaderedRecordFileParser(TextReader txtRdr, Func<RecordType> recFactory) 
            : base(txtRdr, recFactory)
        {
        }

        protected override Dictionary<string,int> ParseHeader(TextReader txtRdr)
        {
            int k = 0;
            var hdr = new Dictionary<string,int>();
            var hdrTokens = GetTokensFromString(txtRdr.ReadLine());
            foreach (var token in hdrTokens)
            {
                hdr.Add(token,k);
                k++;
            }
            return hdr;
        }
    }
}
