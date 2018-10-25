using hlatools.core.DataObjects;
using hlatools.core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace hlatools.core.IO.SAM
{
    public class BamParser : BamParser<SamSeq, SamHeader>
    {
        public BamParser(Stream bgzfStrm, Func<SamSeq> recFactory)
            : base(new BinaryReader(bgzfStrm), recFactory)
        {
        }

        public BamParser(BinaryReader binRdr, Func<SamSeq> recFactory)
            : base(binRdr, recFactory)
        {
        }
    }

    public class BamParser<S, H> : BamParserCore<S, H> where H : SamHeader, new() where S : SamSeq
    {

        public static new BamParser<S, H> FromFilepath(string filepath, Func<S> fact = null)
        {
            if (fact == null)
            {
                fact = () => (S)new SamSeq();
            }

            var strm = File.OpenRead(filepath);
            var bgzfStrm = new BgzfReader(strm);
            var prsr = new BamParser<S, H>(bgzfStrm, fact);
            return prsr;
        }


        public BamParser(Stream bgzfStrm, Func<S> recFactory) 
            : this(new BinaryReader(bgzfStrm), recFactory)
        {
        }

        public BamParser(BinaryReader binRdr, Func<S> recFactory)
            : base(binRdr, recFactory)
        {
        }

        protected override void ParseMetadata(byte[] binRdr, SamSeq read, int bufferEnd = int.MinValue, int offset = 0)
        {
            if (bufferEnd == int.MinValue)
            {
                bufferEnd = binRdr.Length;
            }
            while (offset < bufferEnd)
            {
                var tag = Encoding.ASCII.GetString(binRdr, offset, 2);
                offset += 2;
                var valType = ((char)binRdr[offset++]).ToString();
                                
                if (valType == "B")
                {
                    var elementType = ((char)binRdr[offset++]).ToString();
                    var arrayLength = BitConverter.ToInt32(binRdr, offset);
                    offset += 4;
                    if (elementType == "c")
                    {
                        var opt = new SamSeqSbyteArrayOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        var vals = new sbyte[arrayLength];
                        for (int k = 0; k < arrayLength; k++)
                        {
                            vals[k] = (sbyte)binRdr[offset];
                            offset += 1;
                        }
                        opt.Value = vals;
                    }
                    else if (elementType == "C")
                    {
                        var opt = new SamSeqByteArrayOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        var vals = new byte[arrayLength];
                        for (int k = 0; k < arrayLength; k++)
                        {
                            vals[k] = binRdr[offset];
                            offset += 1;
                        }
                        opt.Value = vals;
                    }
                    else if (elementType == "s")
                    {
                        var opt = new SamSeqInt16ArrayOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        var vals = new Int16[arrayLength];
                        for (int k = 0; k < arrayLength; k++)
                        {
                            vals[k] = BitConverter.ToInt16(binRdr, offset);
                            offset += 2;
                        }
                        opt.Value = vals;
                    }
                    else if (elementType == "S")
                    {
                        var opt = new SamSeqUInt16ArrayOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        var vals = new UInt16[arrayLength];
                        for (int k = 0; k < arrayLength; k++)
                        {
                            vals[k] = BitConverter.ToUInt16(binRdr, offset);
                            offset += 2;
                        }
                        opt.Value = vals;
                    }
                    else if (elementType == "i")
                    {
                        var opt = new SamSeqInt32ArrayOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        var vals = new Int32[arrayLength];
                        for (int k = 0; k < arrayLength; k++)
                        {
                            vals[k] = BitConverter.ToInt32(binRdr, offset);
                            offset += 4;
                        }
                        opt.Value = vals;
                    }
                    else if (elementType == "I")
                    {
                        var opt = new SamSeqUInt32ArrayOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        var vals = new UInt32[arrayLength];
                        for (int k = 0; k < arrayLength; k++)
                        {
                            vals[k] = BitConverter.ToUInt32(binRdr, offset);
                            offset += 4;
                        }
                        opt.Value = vals;
                    }
                    else if (elementType == "f")
                    {
                        var opt = new SamSeqFloatArrayOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        var vals = new float[arrayLength];
                        for (int k = 0; k < arrayLength; k++)
                        {
                            vals[k] = BitConverter.ToSingle(binRdr, offset);
                            offset += 4;
                        }
                        opt.Value = vals;
                    }
                }
                else
                {
                    if (valType == "A")
                    {
                        var opt = new SamSeqCharOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        opt.Value = (char)binRdr[offset];
                        offset += 1;                        
                    }
                    else if (valType == "c")
                    {
                        var opt = new SamSeqSbyteOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        opt.Value = (sbyte)binRdr[offset];
                        offset += 1;
                    }
                    else if (valType == "C")
                    {
                        var opt = new SamSeqByteOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        opt.Value = (byte)binRdr[offset];
                        offset += 1;
                    }
                    else if (valType == "s")
                    {
                        var opt = new SamSeqInt16Opt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        opt.Value = BitConverter.ToInt16(binRdr, offset);
                        offset += 2;
                    }
                    else if (valType == "S")
                    {
                        var opt = new SamSeqUInt16Opt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        opt.Value = BitConverter.ToUInt16(binRdr, offset);
                        offset += 2;
                    }
                    else if (valType == "i")
                    {
                        var opt = new SamSeqInt32Opt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        opt.Value = BitConverter.ToInt32(binRdr, offset);
                        offset += 4;
                    }
                    else if (valType == "I")
                    {
                        var opt = new SamSeqUInt32Opt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        opt.Value = BitConverter.ToUInt32(binRdr, offset);
                        offset += 4;
                    }
                    else if (valType == "f")
                    {
                        var opt = new SamSeqFloatOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        opt.Value = BitConverter.ToSingle(binRdr, offset);
                        offset += 4;
                    }
                    else if (valType == "Z")
                    {
                        var opt = new SamSeqStringOpt(tag);
                        Dict.Upsert(read.Opts, opt.Tag, opt);
                        char c = 'x';
                        while (true)
                        {
                            c = (char)binRdr[offset++];
                            if (c == '\0')
                            {
                                break;
                            }
                            opt.Value += c;
                        }
                    }
                    else if (valType == "H")
                    {
                        throw new NotImplementedException("Sorry, I do not know how to parse hex-formated byte arrays in bam file optional fileds. I guess now will be my chance to learn!?!");
                    }
                }
            }
        }

        

    }
}
