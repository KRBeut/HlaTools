using hlatools.core.DataObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace hlatools.core.IO.Tabix
{
    public class TabixBin : AmgDataObject
    {

        public TabixBin()
        {

        }

        public uint bin;
        public List<TabixChunk> chunks;
        public int n_chunk { get { return chunks.Count(); } }
    }
}