using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace hlatools.core.IO.GFF
{
    public class GtfParser<T> : Gff3Parser<T> where T : Gff3Feature
    {

        private static readonly Regex attribRegEx = new Regex("(?<key>[^\\s]+)\\s\"?(?<val>.+?)\"?;\\s*", RegexOptions.Compiled);
        protected override Regex AttribRegEx
        {
            get
            {
                return attribRegEx;
            }
        }

        public GtfParser(TextReader txtRdr, Func<T> recFactory) 
            : base(txtRdr,recFactory)
        {

        }

    }
}
