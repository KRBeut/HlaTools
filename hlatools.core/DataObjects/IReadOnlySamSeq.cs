using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    public interface IReadOnlySamSeq
    {
        SamFlag Flag { get; }

        string Rname { get; }

        int Pos { get; }

        int Mapq { get; }

        Cigar Cigar { get; }

        string Rnext { get; }

        int Pnext { get; }

        int Length { get; }

    }
}
