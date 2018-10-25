using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{
    [Flags]
    public enum SamFlag
    {
        //1 0x1	PAIRED  paired-end (or multiple-segment) sequencing technology
        PAIRED = 0x1,

        //2 0x2	PROPER_PAIR each segment properly aligned according to the aligner
        PROPER_PAIR = 0x2,

        //4 0x4	UNMAP   segment unmapped
        UNMAP = 0x4,

        //8 0x8	MUNMAP  next segment in the template unmapped
        MUNMAP = 0x8,

        //16 0x10	REVERSE SEQ is reverse complemented
        REVERSESEQ = 0x10,

        //32 0x20	MREVERSE    SEQ of the next segment in the template is reverse complemented
        MREVERSESEQ = 0x20,

        //64 0x40	READ1   the first segment in the template
        READ1 = 0x40,

        //128 0x80	READ2   the last segment in the template
        READ2 = 0x80,

        //256 0x100	SECONDARY   secondary alignment
        SECONDARY = 0x100,

        //512 0x200	QCFAIL  not passing quality controls
        QCFAIL = 0x200,

        //1024 0x400	DUP PCR or optical duplicate
        DUPPCR = 0x400,

        //2048 0x800	SUPPLEMENTARY   supplementary alignment
        SUPPLEMENTARY = 0x800
    }
}
