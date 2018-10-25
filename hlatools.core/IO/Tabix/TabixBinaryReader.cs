using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.Tabix
{
    public abstract class TabixBinaryReader<T> : TabixReader<T> where T : ICoverRegion
    {

        public BinaryReader BinRdr { get; protected set; }

        public TabixBinaryReader(BinaryReader binRdr, TabixIndexFile index) 
            : base(index)
        {
            BinRdr = binRdr;
        }

        protected override void Seek(long offset)
        {
            BinRdr.BaseStream.Seek(offset, SeekOrigin.Begin);
        }

    }
}
