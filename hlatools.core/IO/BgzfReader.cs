using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    public class BgzfReader : Stream
    {
        
        public Stream BaseStream { get; protected set; }

        byte[] _compressedBuffer;
        byte[] _unCompressedBuffer;
        int _unCompressedBufferSize = 0;
        int _unCompressedBufferPosition = 0;

        public BgzfReader(Stream baseStrm)
            : base()
        {
            BaseStream = baseStrm;
            _compressedBuffer = new byte[(2 << 15)+1];
            _unCompressedBuffer = new byte[(2 << 15)+1];            
            ValidateReader();
        }

        /// <summary>
        /// Reads specified number of uncompressed bytes from BAM file to byte array
        /// </summary>
        /// <param name="array">Byte array to copy.</param>
        /// <param name="offset">Offset of Byte array from which the data has to be copied.</param>
        /// <param name="count">Number of bytes to copy.</param>
        public override int Read(byte[] array, int offset, int count)
        {
            if (!isCompressed)
            {
                return BaseStream.Read(array, offset, count);
            }

            if (_unCompressedBufferPosition >= _unCompressedBufferSize)
            {
                GetNextBlock();
            }

            long remainingBlockSize = _unCompressedBufferSize - _unCompressedBufferPosition;

            if (remainingBlockSize == 0)
            {
                //reached the end of the file. Everything is good!
                return 0;
            }
            else if (_unCompressedBufferSize > _unCompressedBuffer.Length)
            {
                //WARNING: The file has an improper ending or is possibly incorrectly truncated!!!
                return 0;
            }

            int bytesToRead = remainingBlockSize >= (long)count ? count : (int)remainingBlockSize;
            Array.Copy(_unCompressedBuffer, _unCompressedBufferPosition, array, offset, bytesToRead);
            _unCompressedBufferPosition += bytesToRead;

            if (bytesToRead < count)
            {
                GetNextBlock();
                return bytesToRead + Read(array, offset + bytesToRead, count - bytesToRead);
            }
            return bytesToRead;
        }

        /// <summary>
        /// Gets next block by reading and decompressing the compressed block from compressed BAM file.
        /// </summary>
        private void GetNextBlock(int uncompressedOffset = 0)
        {
            if (BaseStream.CanSeek && BaseStream.Position >= BaseStream.Length)
            {
                return;
            }

            //read the bgzf header array
            BaseStream.Read(_compressedBuffer, 0, 18);

            //int offset = 0;
            //var id1 = _compressedBuffer[offset++];//always equal to 31
            //var id2 = _compressedBuffer[offset++];//always equal to 139
            //var compMode = _compressedBuffer[offset++];//always equal to 8
            //var flg = _compressedBuffer[offset++];//always equal to 4
            //var mTime = BitConverter.ToUInt32(_compressedBuffer, offset);//typically equal to 0
            //offset += 4;
            //var xfl = _compressedBuffer[offset++];//typically equal to 0
            //var os = _compressedBuffer[offset++];//typically equal to 255
            //var xLen = BitConverter.ToUInt16(_compressedBuffer, offset);//typically equal to 6
            //offset += 2;
            //var si1 = _compressedBuffer[offset++];//always equal to 66
            //var si2 = _compressedBuffer[offset++];//always equal to 67
            //var sLen = BitConverter.ToUInt16(_compressedBuffer, offset);//always equal to 2
            //offset += 2;
            //var bSize = BitConverter.ToUInt16(_compressedBuffer, offset);
            //offset += 2;

            int BSIZE = 0;
            int ELEN = BitConverter.ToUInt16(_compressedBuffer, 10);
            //verify there is an extra field, get the block size
            if (ELEN != 0)
            {
                BSIZE = BitConverter.ToUInt16(_compressedBuffer, 12 + ELEN - 2);
            }
            int size = BSIZE + 1;

            int bytesread;
            if ((bytesread = BaseStream.Read(_compressedBuffer, 18, size - 18)) != size - 18)
            {
                return;
                //throw new Exception("BAM_UnableToReadCompressedBlock");
            }
            _unCompressedBufferSize = (int)BitConverter.ToUInt32(_compressedBuffer, size - 4);

            if (_unCompressedBufferSize > _unCompressedBuffer.Length)
            {
                return;
            }

            using (var copmressedMemStrm = new MemoryStream(_compressedBuffer))
            using (var gZipStream = new GZipStream(copmressedMemStrm, CompressionMode.Decompress, true))
            {
                gZipStream.Read(_unCompressedBuffer, 0, _unCompressedBufferSize);
            }
            _unCompressedBufferPosition = uncompressedOffset;
        }
        
        /// <summary>
        /// Flag to indicate whether the BAM file is compressed or not.
        /// </summary>
        private bool isCompressed;

        /// <summary>
        /// Validates the BAM stream.
        /// </summary>
        private void ValidateReader()
        {
            if (!BaseStream.CanSeek)
            {
                isCompressed = true;
                return;
            }
            isCompressed = false;
            byte[] array = new byte[4];
            if (BaseStream.Read(array, 0, 4) != 4)
            {
                // cannot read file properly.
                throw new Exception("Invalid .bgzf file -- could not read the first 4 bytes.");
            }
            isCompressed = IsCompressedFile(array);
            BaseStream.Seek(0, SeekOrigin.Begin);
        }
        
        private static int SHIFT_AMOUNT = 16;
        private static int OFFSET_MASK = 0xffff;
        private static long ADDRESS_MASK = 0xFFFFFFFFFFFFL;

        /// <summary>
        /// File offset of start of BGZF block for this virtual file pointer.
        /// </summary>
        /// <param name="virtualFilePointer"></param>
        /// <returns></returns>
        public static long getBlockAddress(long virtualFilePointer)
        {
            return (virtualFilePointer >> SHIFT_AMOUNT) & ADDRESS_MASK;
        }

        /// <summary>
        /// Offset into uncompressed block for this virtual file pointer.
        /// </summary>
        /// <param name="virtualFilePointer"></param>
        /// <returns>Offset into uncompressed block for this virtual file pointer.</returns>
        public static int getBlockOffset(long virtualFilePointer)
        {
            return (int)(virtualFilePointer & OFFSET_MASK);
        }

        public static long getVirtualOffset(long uncompressedOffset, long compressedOffset)
        {
            return (compressedOffset << SHIFT_AMOUNT) | uncompressedOffset;
        }
        
        /// <summary>
        /// Returns a boolean value indicating whether a BAM file is compressed or uncompressed.
        /// </summary>
        /// <param name="array">Byte array containing first 4 bytes of a BAM file</param>
        /// <returns>Returns true if the specified byte array indicates that the BAM file is compressed else returns false.</returns>
        private static bool IsCompressedFile(byte[] array)
        {
            return array[0] == 31
                && array[1] == 139
                && array[2] == 8;
        }

        public override bool CanRead
        {
            get
            {
                return BaseStream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return BaseStream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return BaseStream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return BaseStream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return BaseStream.Position;
            }

            set
            {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long compressedOffset = getBlockAddress(offset);
            int uncompressedOffset = getBlockOffset(offset);

            
            var pos = BaseStream.Seek(compressedOffset, origin);
            GetNextBlock(uncompressedOffset);

            //return pos;
            var finalOffset = getVirtualOffset(_unCompressedBufferPosition, pos);
            //var finalOffset = getVirtualOffset(deCompressedStream.Position, BaseStream.Position);            
            return finalOffset;
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
            GetNextBlock();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (this.BaseStream != null))
                {
                    if (BaseStream != null)
                    {
                        BaseStream.Close();
                        BaseStream.Dispose();
                    }
                    //if (gZipStream != null)
                    //{
                    //    gZipStream.Close();
                    //    gZipStream.Dispose();
                    //}
                    //if (copmressedMemStrm != null)
                    //{
                    //    copmressedMemStrm.Close();
                    //    copmressedMemStrm.Dispose();
                    //}
                }
            }
            finally
            {
                if (BaseStream != null)
                {
                    BaseStream = null;
                }
                //if (gZipStream != null)
                //{
                //    gZipStream.Close();
                //    gZipStream.Dispose();
                //}
                //if (copmressedMemStrm != null)
                //{
                //    copmressedMemStrm.Close();
                //    copmressedMemStrm.Dispose();
                //}
                base.Dispose(disposing);
            }

        }

    }
}
