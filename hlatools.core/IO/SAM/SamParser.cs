using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using hlatools.core.DataObjects;
using System.Diagnostics;
using hlatools.core.Utils;

namespace hlatools.core.IO.SAM
{

    public class SamParser : SamParser<SamSeq, SamHeader>
    {
        public SamParser(TextReader txtRdr, Func<SamSeq> recFactory) 
            : base(txtRdr, recFactory)
        {
        }
        
    }

    /// <summary>
    /// Does not parse the read metadata. This makes parsing way faster for the cases
    /// where the caller does not need the read metadata. Please use SamParser class 
    /// instead of this class when the read metadata is needed.
    /// </summary>
    public class SamParser<S,H> : SamParserCore<S,H> where S : SamSeq where H : SamHeader, new()
    {

        public static new SamParser<S, H> FromFilepath(string filepath, Func<S> fact)
        {
            var samFileRdr = StringToTxtRdr(filepath);
            var prsr = new SamParser<S, H>(samFileRdr, fact);
            return prsr;
        }

        public SamParser(TextReader txtRdr, Func<S> recFactory)
            : base(txtRdr, recFactory)
        {
            //columnDelims = new char[] { '\t' };
        }

        protected override void PopulateMetadata(Dictionary<string, SamSeqOpt> dict, IEnumerable<string> strs)
        {
            foreach (var str in strs)
            {
                var opt = ParseMetadata(str);
                if (opt != null)
                {
                    Dict.Upsert(dict, opt.Tag, opt);
                }
            }
        }

    }
}
