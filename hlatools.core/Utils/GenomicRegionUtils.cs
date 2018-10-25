using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.Utils
{
    public static class GenomicRegionUtils
    {

        public static bool TestIntersection(ICoverRegion x, int start, int end)
        {
            bool areIntersecting = true;
            if (x.Pos > start)
            {
                areIntersecting = false;
            }
            else if (x.Pos + x.Length < start)
            {
                areIntersecting = false;
            }
            if (start > x.Pos + x.Length)
            {
                areIntersecting = false;
            }
            else if (start < x.Pos)
            {
                areIntersecting = false;
            }

            return areIntersecting;
        }

        public static int Compare(ICoverRegion x, int start, int end)
        {
            if (TestIntersection(x, start, end))
            {
                return 0;
            }
            else if (x.Pos + x.Length < start)
            {
                return -1;
            }
            return 1;
        }

        public static int Compare(ICoverRegion x, ICoverRegion y)
        {
            return Compare(x, y.Pos, y.Pos + y.Length);
        }

        public static bool TestIntersection(ICoverRegion x, ICoverRegion y)
        {
            return TestIntersection(x, y.Pos, y.Pos + y.Length);
        }

    }
}
