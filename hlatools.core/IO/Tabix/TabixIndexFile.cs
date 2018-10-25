using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.Tabix
{
    public class TabixIndexFile : AmgDataObject
    {

        public TabixIndexFile()
        {
            
        }

        public TabixHeader header { get; set; }
        public Dictionary<string, TabixIndex> indices;
        public ulong n_no_coor;

        public static int reg2bin(int beg, int end)
        {
            --end;
            if (beg >> 14 == end >> 14)
            {
                return ((1 << 15) - 1) / 7 + (beg >> 14);
            }
            if (beg >> 17 == end >> 17)
            {
                return ((1 << 12) - 1) / 7 + (beg >> 17);
            }
            if (beg >> 20 == end >> 20)
            {
                return ((1 << 9) - 1) / 7 + (beg >> 20);
            }
            if (beg >> 23 == end >> 23)
            {
                return ((1 << 6) - 1) / 7 + (beg >> 23);
            }
            if (beg >> 26 == end >> 26)
            {
                return ((1 << 3) - 1) / 7 + (beg >> 26);
            }
            return 0;
        }

        public static int reg2bins(int rbeg, int rend, int[] list)
        {
            int i = 0, k, end = rend;
            if (rbeg < 0)
            {
                rbeg = 0;
            }
            if (rbeg >= end)
            {
                return 0;
            }
            if (end >= 1 << 29)
            {
                end = 1 << 29;
            }
            --end;
            list[i++] = 0;
            for (k = 1 + (rbeg >> 26); k <= 1 + (end >> 26); ++k)
            {
                list[i++] = k;
            }
            for (k = 9 + (rbeg >> 23); k <= 9 + (end >> 23); ++k)
            {
                list[i++] = k;
            }
            for (k = 73 + (rbeg >> 20); k <= 73 + (end >> 20); ++k)
            {
                list[i++] = k;
            }
            for (k = 585 + (rbeg >> 17); k <= 585 + (end >> 17); ++k)
            {
                list[i++] = k;
            }
            for (k = 4681 + (rbeg >> 14); k <= 4681 + (end >> 14); ++k)
            {
                list[i++] = k;
            }
            return i;
            //int i = 0, k;
            //--rend;
            //list[i++] = 0;
            //for (k = 1 + (rbeg >> 26); k <= 1 + (rend >> 26); ++k) list[i++] = k;
            //for (k = 9 + (rbeg >> 23); k <= 9 + (rend >> 23); ++k) list[i++] = k;
            //for (k = 73 + (rbeg >> 20); k <= 73 + (rend >> 20); ++k) list[i++] = k;
            //for (k = 585 + (rbeg >> 17); k <= 585 + (rend >> 17); ++k) list[i++] = k;
            //for (k = 4681 + (rbeg >> 14); k <= 4681 + (rend >> 14); ++k) list[i++] = k;
            //return i; // #elements in list[]
        }
    }
}
