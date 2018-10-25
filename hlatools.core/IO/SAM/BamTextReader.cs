using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    public class BamTextReader : TextReader
    {

        BinaryReader BinRdr;

        public BamTextReader(BinaryReader binRdr) 
            : base()
        {
            BinRdr = binRdr;
        }
        
        

    }
}
