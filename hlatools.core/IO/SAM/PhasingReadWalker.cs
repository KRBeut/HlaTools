using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    public class PhasingReadWalker<T> : Walker<T> where T : ICoverRegion
    {
        public PhasingReadWalker(IEnumerator<T> objSource, Action<T> objSink) 
            : base(objSource, objSink)
        {
        }

        protected override bool FilterFromReturn(T rd, string rName, int posStart, int posEnd)
        {
            return rd.Pos + rd.Length < posEnd || rd.Pos > posStart;
        }

    }
}
