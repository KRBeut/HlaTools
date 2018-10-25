using hlatools.core.DataObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace hlatools.core.IO
{
    /// <summary>
    /// this is an earlier version of the BgzfReader class that is derived from 
    /// the .NET Bio open source project on codeplex. This implementation of a 
    /// BGZF reader uses streams instead of the buffers that are used in BgzfReader.
    /// Because of BgzfStream's lack of memory management, using this class triggers
    /// very frequent garbage collections. These GCs may not greatly impact the overall
    /// performance, but it does make profiling much more difficult!
    /// </summary>

    /*
    public class BgzfStrm : Stream
    {

        public Stream BaseStream { get; protected set;}
        
        public BgzfStrm(Stream baseStrm) 
            : base()
        {
            BaseStream = baseStrm;
            ValidateReader();
        }        
        
        /// <summary>
        /// A temporary file stream to hold uncompressed blocks.
        /// </summary>
        private Stream deCompressedStream;
        
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

        
        byte[] bgzfHeaderArray = new byte[18];

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
        /// Gets next block by reading and decompressing the compressed block from compressed BAM file.
        /// </summary>
        private void GetNextBlock(int uncompressedOffset = 0)
        {
            deCompressedStream = null;
            if (BaseStream.Position >= BaseStream.Length)
            {
                return;
            }
            
            //read the bgzf header array
            BaseStream.Read(bgzfHeaderArray, 0, 18);

            int offset = 0;
            var id1 = bgzfHeaderArray[offset++];//always equal to 31
            var id2 = bgzfHeaderArray[offset++];//always equal to 139
            var compMode = bgzfHeaderArray[offset++];//always equal to 8
            var flg = bgzfHeaderArray[offset++];//always equal to 4
            var mTime = BitConverter.ToUInt32(bgzfHeaderArray, offset);//typically equal to 0
            offset += 4;
            var xfl = bgzfHeaderArray[offset++];//typically equal to 0
            var os = bgzfHeaderArray[offset++];//typically equal to 255
            var xLen = BitConverter.ToUInt16(bgzfHeaderArray, offset);//typically equal to 6
            offset += 2;
            var si1 = bgzfHeaderArray[offset++];//always equal to 66
            var si2 = bgzfHeaderArray[offset++];//always equal to 67
            var sLen = BitConverter.ToUInt16(bgzfHeaderArray, offset);//always equal to 2
            offset += 2;
            var bSize = BitConverter.ToUInt16(bgzfHeaderArray, offset);
            offset += 2;

            int BSIZE = 0;
            int ELEN = BitConverter.ToUInt16(bgzfHeaderArray, 10);
            //verify there is an extra field, get the block size
            if (ELEN != 0)
            {
                BSIZE = BitConverter.ToUInt16(bgzfHeaderArray, 12 + ELEN - 2);
            }
            int size = BSIZE + 1;

            byte[] block = new byte[size];
            using (var memStream = new MemoryStream(size))
            {
                bgzfHeaderArray.CopyTo(block, 0);
                if (BaseStream.Read(block, 18, size - 18) != size - 18)
                {
                    throw new Exception("BAM_UnableToReadCompressedBlock");
                }

                uint unCompressedBlockSize = BitConverter.ToUInt32(block, size - 4);

                deCompressedStream = GetTempStream(unCompressedBlockSize);

                var blockBuf = string.Join("\n",block.Select(b => (int)b));

                memStream.Write(block, 0, size);
                memStream.Seek(0, SeekOrigin.Begin);
                Decompress(memStream, deCompressedStream);
            }
            //deCompressedStream.Seek(uncompressedOffset, SeekOrigin.Begin);
            //var debugBuffer = new byte[64*1024];
            //deCompressedStream.Read(debugBuffer, 0, 64 * 1024);
            //var debugBufferStr = string.Join("\n",debugBuffer.Select(b => (int)b));
            deCompressedStream.Seek(uncompressedOffset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Decompresses specified compressed stream to out stream.
        /// </summary>
        /// <param name="compressedStream">Compressed stream to decompress.</param>
        /// <param name="outStream">Out stream.</param>
        private static void Decompress(Stream compressedStream, Stream outStream)
        {            
            using (var stream = new GZipStream(compressedStream, CompressionMode.Decompress, true))
            {
                stream.CopyTo(outStream);
            }
        }

        /// <summary>
        /// Gets the temp stream to store Decompressed blocks.
        /// If the specified capacity is with in the Maximum integer (32 bit int) limit then 
        /// a MemoryStream is created else a temp file is created to store Decompressed data.
        /// </summary>
        /// <param name="capacity">Required capacity.</param>
        private Stream GetTempStream(uint capacity)
        {
            if (deCompressedStream != null)
            {
                deCompressedStream.Dispose();
                deCompressedStream = null;
            }

            if (capacity <= int.MaxValue)
            {
                deCompressedStream = new MemoryStream((int)capacity);
            }
            else
            {
                throw new NotImplementedException();
            }

            return deCompressedStream;
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

            if (deCompressedStream == null || deCompressedStream.Length - deCompressedStream.Position == 0)
            {
                GetNextBlock();
            }

            if (deCompressedStream == null)
            {
                return 0;
            }

            long remainingBlockSize = deCompressedStream.Length - deCompressedStream.Position;
            if (remainingBlockSize == 0)
            {
                return 0;
            }

            int bytesToRead = remainingBlockSize >= (long)count ? count : (int)remainingBlockSize;
            deCompressedStream.Read(array, offset, bytesToRead);

            if (bytesToRead < count)
            {
                GetNextBlock();
                return bytesToRead + Read(array, offset + bytesToRead, count - bytesToRead);
            }
            return bytesToRead;
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
            deCompressedStream = null;
            BaseStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {            
            long compressedOffset = getBlockAddress(offset);
            int uncompressedOffset = getBlockOffset(offset);

            deCompressedStream = null;
            var pos = BaseStream.Seek(compressedOffset, origin);
            GetNextBlock(uncompressedOffset);

            //return pos;
            var finalOffset = getVirtualOffset(deCompressedStream.Position, pos);            
            //var finalOffset = getVirtualOffset(deCompressedStream.Position, BaseStream.Position);            
            return finalOffset;
        }

        public override void SetLength(long value)
        {
            deCompressedStream = null;
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
                    BaseStream.Close();
                }
            }
            finally
            {
                if (BaseStream != null)
                {
                    BaseStream = null;
                }
                base.Dispose(disposing);
            }
            
        }

    }     
    */

}
