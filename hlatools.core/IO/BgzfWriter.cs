using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public class BgzfWriter : System.IO.Stream
    {
        
        byte[] compressedData;
        byte[] uncompressedData;
        int uncompressedPos = 0;
        const int bufferSize = (2 << 15);

        System.IO.Stream BaseStream;

        public BgzfWriter(System.IO.Stream stream) 
            : base()
        {
            BaseStream = stream;
            uncompressedData = new byte[bufferSize];
            compressedData = new byte[bufferSize];
        }

        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override void Close()
        {
            Flush();
            base.Close();
        }

        static readonly int blockHeaderSize = 18;
        static readonly int blockFooterSize = 8;

        public override void Flush()
        {
            if (uncompressedPos < 1)
            {
                return;
            }

            using (var localCompressedStrm = new MemoryStream(compressedData))
            {
                using (DeflateStream deflateStrm = new DeflateStream(localCompressedStrm, CompressionMode.Compress, true))
                {
                    deflateStrm.Write(uncompressedData, 0, uncompressedPos);
                }

                //compute the crc32 of the uncompressed data
                var crcCalc = new CRC32();
                crcCalc.SlurpBlock(uncompressedData, 0, uncompressedPos);
                var crcBytes = BitConverter.GetBytes(crcCalc.Crc32Result);

                localCompressedStrm.Flush();
                var bSize = localCompressedStrm.Position + blockHeaderSize + blockFooterSize;
                localCompressedStrm.Seek(0, SeekOrigin.Begin);

                BaseStream.Write(BitConverter.GetBytes((byte)31), 0, 1);//ID1            
                BaseStream.Write(BitConverter.GetBytes((byte)139), 0, 1);//ID2
                BaseStream.Write(BitConverter.GetBytes((byte)8), 0, 1);//CM
                BaseStream.Write(BitConverter.GetBytes((byte)4), 0, 1);//FLG
                BaseStream.Write(BitConverter.GetBytes((UInt32)0), 0, 4);//MTIME
                BaseStream.Write(BitConverter.GetBytes((byte)0), 0, 1);//XLF
                BaseStream.Write(BitConverter.GetBytes((byte)255), 0, 1);//OS
                BaseStream.Write(BitConverter.GetBytes((UInt16)6), 0, 2);//XLEN
                BaseStream.Write(BitConverter.GetBytes((byte)66), 0, 1);//SI1
                BaseStream.Write(BitConverter.GetBytes((byte)67), 0, 1);//SI2
                BaseStream.Write(BitConverter.GetBytes((UInt16)2), 0, 2);//SLEN
                BaseStream.Write(BitConverter.GetBytes((UInt16)(bSize - 1)), 0, 2);//BSIZE

                localCompressedStrm.CopyTo(BaseStream);//CDATA

                BaseStream.Write(crcBytes, 0, 4);//write the CRC32 checksum
                BaseStream.Write(BitConverter.GetBytes((UInt32)uncompressedPos), 0, 4);//ISIZE
            }

            uncompressedPos = 0;
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (uncompressedPos + count - 1 > bufferSize)
            {
                Flush();
            }
            Array.Copy(buffer, offset, uncompressedData, uncompressedPos, count);
            //buffer.CopyTo(uncompressedData, uncompressedPos);
            uncompressedPos += count;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        

    }
}
