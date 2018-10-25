using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace hlatools.core.DataObjects
{

    public class SamSeqFloatOpt : SamSeqOpt<float>
    {
        public SamSeqFloatOpt(string tag) 
            : base(tag)
        {

        }

        public override int ValueSize { get { return 4; } }

        public override int GetBamArraySize()
        {
            return 3 + ValueSize;
        }

        public override string Type { get { return "f"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.UTF8.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            BitConverter.GetBytes(Value).CopyTo(retVal, 3);
            return retVal;
        }
    }
    
    public class SamSeqUInt32Opt : SamSeqOpt<UInt32>
    {
        public SamSeqUInt32Opt(string tag) 
            : base(tag)
        {

        }

        public override int ValueSize { get { return 4; } }

        public override int GetBamArraySize()
        {
            return 3 + ValueSize;
        }

        public override string Type { get { return "I"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.UTF8.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            BitConverter.GetBytes(Value).CopyTo(retVal, 3);
            return retVal;
        }

    }

    public class SamSeqInt32Opt : SamSeqOpt<Int32>
    {
        public SamSeqInt32Opt(string tag) 
            : base(tag)
        {

        }

        public override int ValueSize { get { return 4; } }

        public override int GetBamArraySize()
        {
            return 3 + ValueSize;
        }

        public override string Type { get { return "i"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.UTF8.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            BitConverter.GetBytes(Value).CopyTo(retVal, 3);
            return retVal;
        }

    }

    public class SamSeqUInt16Opt : SamSeqOpt<UInt16>
    {
        public SamSeqUInt16Opt(string tag) 
            : base(tag)
        {

        }

        public override int ValueSize { get { return 2; } }

        public override int GetBamArraySize()
        {
            return 3 + ValueSize;
        }

        public override string Type { get { return "S"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.UTF8.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            BitConverter.GetBytes(Value).CopyTo(retVal, 3);
            return retVal;
        }

    }

    public class SamSeqInt16Opt : SamSeqOpt<Int16>
    {
        public SamSeqInt16Opt(string tag) 
            : base(tag)
        {

        }

        public override int ValueSize { get { return 2; } }

        public override int GetBamArraySize()
        {
            return 3 + ValueSize;
        }

        public override string Type { get { return "s"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.UTF8.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            BitConverter.GetBytes(Value).CopyTo(retVal, 3);
            return retVal;
        }

    }

    public class SamSeqByteOpt : SamSeqOpt<byte>
    {
        public SamSeqByteOpt(string tag) 
            : base(tag)
        {

        }

        public override int ValueSize { get { return 1; } }

        public override int GetBamArraySize()
        {
            return 3 + ValueSize;
        }

        public override string Type { get { return "C"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.UTF8.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = Value;            
            return retVal;
        }

    }

    public class SamSeqSbyteOpt : SamSeqOpt<sbyte>
    {
        public SamSeqSbyteOpt(string tag) 
            : base(tag)
        {

        }

        public override int ValueSize { get { return 1; } }

        public override int GetBamArraySize()
        {
            return 3 + ValueSize;
        }

        public override string Type { get { return "c"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.UTF8.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = (byte)Value;
            return retVal;
        }

    }

    public class SamSeqCharOpt : SamSeqOpt<char>
    {
        public SamSeqCharOpt(string tag) 
            : base(tag)
        {

        }

        public override int ValueSize { get { return 1; } }

        public override int GetBamArraySize()
        {
            return 3 + ValueSize;
        }

        public override string Type { get { return "A"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.UTF8.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = (byte)Value;
            return retVal;
        }

    }

    public class SamSeqStringOpt : SamSeqOpt<string>
    {
        public SamSeqStringOpt(string tag) 
            : base(tag)
        {

        }

        public override int ValueSize { get { return Value.Length+1; } }

        public override int GetBamArraySize()
        {
            return 3 + ValueSize;
        }

        public override string Type { get { return "Z"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.UTF8.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            Encoding.UTF8.GetBytes(Value).CopyTo(retVal, 3);
            retVal[GetBamArraySize() - 1] = (byte)'\0';
            return retVal;
        }
    }

    public class SamSeqFloatArrayOpt : SamSeqArrayOpt<float>
    {
        public SamSeqFloatArrayOpt(string tag) 
            : base(tag)
        {

        }

        public override int ArrayElementSize
        {
            get
            {
                return 4;
            }
        }

        public override string Type { get { return "B"; } }
        public override string ArrayElementType { get { return "f"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.Default.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = (byte)ArrayElementType[0];
            BitConverter.GetBytes((Int32)Value.Length).CopyTo(retVal, 4);
            var offset = 8;
            foreach (var val in Value)
            {
                BitConverter.GetBytes(val).CopyTo(retVal, offset);
                offset += ArrayElementSize;
            }
            return retVal;
        }

    }

    public class SamSeqUInt32ArrayOpt : SamSeqArrayOpt<UInt32>
    {
        public SamSeqUInt32ArrayOpt(string tag) 
            : base(tag)
        {

        }

        public override int ArrayElementSize
        {
            get
            {
                return 4;
            }
        }

        public override string Type { get { return "B"; } }
        public override string ArrayElementType { get { return "I"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.Default.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = (byte)ArrayElementType[0];
            BitConverter.GetBytes((Int32)Value.Length).CopyTo(retVal, 4);
            var offset = 8;
            foreach (var val in Value)
            {
                BitConverter.GetBytes(val).CopyTo(retVal, offset);
                offset += ArrayElementSize;
            }
            return retVal;
        }

    }

    public class SamSeqInt32ArrayOpt : SamSeqArrayOpt<Int32>
    {
        public SamSeqInt32ArrayOpt(string tag) 
            : base(tag)
        {

        }

        public override int ArrayElementSize
        {
            get
            {
                return 4;
            }
        }

        public override string Type { get { return "B"; } }
        public override string ArrayElementType { get { return "i"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.Default.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = (byte)ArrayElementType[0];
            BitConverter.GetBytes((Int32)Value.Length).CopyTo(retVal, 4);
            var offset = 8;
            foreach (var val in Value)
            {
                BitConverter.GetBytes(val).CopyTo(retVal, offset);
                offset += ArrayElementSize;
            }
            return retVal;
        }
    }

    public class SamSeqUInt16ArrayOpt : SamSeqArrayOpt<UInt16>
    {
        public SamSeqUInt16ArrayOpt(string tag) 
            : base(tag)
        {

        }

        public override int ArrayElementSize
        {
            get
            {
                return 2;
            }
        }

        public override string Type { get { return "B"; } }
        public override string ArrayElementType { get { return "S"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.Default.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = (byte)ArrayElementType[0];
            BitConverter.GetBytes((Int32)Value.Length).CopyTo(retVal, 4);
            var offset = 8;
            foreach (var val in Value)
            {
                BitConverter.GetBytes(val).CopyTo(retVal, offset);
                offset += ArrayElementSize;
            }
            return retVal;
        }

    }

    public class SamSeqInt16ArrayOpt : SamSeqArrayOpt<Int16>
    {
        public SamSeqInt16ArrayOpt(string tag) 
            : base(tag)
        {

        }

        public override int ArrayElementSize
        {
            get
            {
                return 2;
            }
        }

        public override string Type { get { return "B"; } }
        public override string ArrayElementType { get { return "s"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.Default.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = (byte)ArrayElementType[0];
            BitConverter.GetBytes((Int32)Value.Length).CopyTo(retVal, 4);
            var offset = 8;
            foreach (var val in Value)
            {
                BitConverter.GetBytes(val).CopyTo(retVal, offset);
                offset += ArrayElementSize;
            }
            return retVal;
        }

    }

    public class SamSeqByteArrayOpt : SamSeqArrayOpt<byte>
    {
        public SamSeqByteArrayOpt(string tag) 
            : base(tag)
        {

        }

        public override int ArrayElementSize
        {
            get
            {
                return 1;
            }
        }

        public override string Type { get { return "B"; } }
        public override string ArrayElementType { get { return "C"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.Default.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = (byte)ArrayElementType[0];
            BitConverter.GetBytes((Int32)Value.Length).CopyTo(retVal, 4);
            var offset = 8;
            foreach (var val in Value)
            {
                BitConverter.GetBytes(val).CopyTo(retVal, offset);
                offset += ArrayElementSize;
            }
            return retVal;
        }

    }

    public class SamSeqSbyteArrayOpt : SamSeqArrayOpt<sbyte>
    {
        public SamSeqSbyteArrayOpt(string tag) 
            : base(tag)
        {

        }

        public override int ArrayElementSize
        {
            get
            {
                return 1;
            }
        }

        public override string Type { get { return "B"; } }
        public override string ArrayElementType { get { return "c"; } }

        public override byte[] ToBamArray()
        {
            var retVal = new byte[GetBamArraySize()];
            Encoding.Default.GetBytes(Tag).CopyTo(retVal, 0);
            retVal[2] = (byte)Type[0];
            retVal[3] = (byte)ArrayElementType[0];
            BitConverter.GetBytes((Int32)Value.Length).CopyTo(retVal,4);
            var offset = 8;
            foreach (var val in Value)
            {
                BitConverter.GetBytes(val).CopyTo(retVal, offset);
                offset += ArrayElementSize;
            }
            return retVal;
        }
    }

    public abstract class SamSeqArrayOpt<T> : SamSeqOpt<T[]> 
    {
        public SamSeqArrayOpt(string tag) 
            : base(tag)
        {

        }

        public abstract int ArrayElementSize { get; }

        public abstract string ArrayElementType { get; }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2},{3}", Tag, Type, ArrayElementType, string.Join(",",Value.Select(x=>x.ToString())));
        }

        public override int ValueSize { get { return ArrayElementSize * Value.Length; } }

        public override int GetBamArraySize()
        {            
            return 8 + ValueSize;
        }
        
    }

    public abstract class SamSeqOpt<T> : SamSeqOpt
    {
        public T Value { get; set; }

        public SamSeqOpt(string tag) 
            : base(tag)
        {
            
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}",Tag,Type,Value);
        }

        public abstract int ValueSize{ get; }        
    }

    public abstract class SamSeqOpt
    {

        public SamSeqOpt(string tag)
        {
            Tag = tag;
        }

        public string Tag { get; set; }

        public abstract string Type { get; }
        
        public bool isStringValued
        {
            get
            {
                return Type == "Z";
            }
        }

        public bool isArrayValued
        {
            get
            {
                return Type == "B" || Type == "H";
            }
        }

        public abstract override string ToString();

        public abstract byte[] ToBamArray();

        public abstract int GetBamArraySize();

    }

}
