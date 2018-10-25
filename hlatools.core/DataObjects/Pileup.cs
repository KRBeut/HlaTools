using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public class Pileup<T> : HashSet<T>, ICoverRegion where T : ICoverRegion
    {

        public static Pileup<T> FromCollection(IEnumerable<T> collection)
        {
            var pileup = new Pileup<T>(collection);
            if (collection.Count()>0)
            {
                pileup.Rname = collection.First().Rname;
                pileup.Pos = collection.Min(r => r.Pos);
                pileup.Length = collection.Max(r => r.Pos + r.Length) - pileup.Pos + 1;
            }
            return pileup;
        }

        public Pileup()
            : base()
        {

        }

        public Pileup(IEnumerable<T> collection) 
            : base(collection)
        {

        }

        public string Rname { get; set; }

        public int Pos { get; set; }

        public int Length { get; set; }
    }
}
