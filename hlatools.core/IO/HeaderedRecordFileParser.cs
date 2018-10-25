using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public abstract class HeaderedRecordFileParser<RecordType, HeaderType> : RecordFileParser<RecordType>
    {
        public HeaderType Header { get; set; }

        protected HeaderedRecordFileParser() 
            : base()
        {

        }

        public HeaderedRecordFileParser(TextReader txtRdr, Func<RecordType> recFactory) 
            : base(txtRdr, recFactory)
        {
            Header = ParseHeader(txtRdr);
        }

        protected abstract HeaderType ParseHeader(TextReader txtRdr);
        
    }
}
