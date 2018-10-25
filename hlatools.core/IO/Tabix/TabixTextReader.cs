
using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.Tabix
{
    public abstract class TabixTextReader<T> : TabixReader<T> where T : ICoverRegion
    {

        protected StreamReader txtRdr;
        
        public TabixTextReader(StreamReader strmRdr, TabixIndexFile index)
            : base(index)
        {
            txtRdr = strmRdr;
        }

        protected override void Seek(long offset)
        {
            txtRdr.DiscardBufferedData();//this is VERY important                
            txtRdr.BaseStream.Seek(offset, SeekOrigin.Begin);//seek to virtual offset
        }
    }
}
