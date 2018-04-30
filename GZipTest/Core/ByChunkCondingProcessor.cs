using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GZipTest.Util;
using Helpers.DataStructure;
using Helpers.Sort;
using isxaker;
using Microsoft.VisualBasic.Devices;
using ZipCore;

namespace GZipTest.Core
{

    /// <summary>
    ///     Processing file by chunks
    /// </summary>
    public class ByChunkCodingProcessor : BaseCodingProcessor
    {
        #region constants

        //the size of the buffer and standart data block length
        private const int BufferSize = 1 << 17; //65536 * 2 = 131072 = 128KB

        #endregion

        #region  fields

        //marker for shutdown
        private bool _isStop;

        //fixed queue of chunks have been already processed
        private BlockingSortableMinQueue<ZipBlock> _processedChunks;
        //the max size of _processedChunks
        private int _processedChunksQueueSize;

        private AutoResetEvent _regularChunkProcessed;
        //total blocks
        private long _unprocessedBlockCounter;
        //fixed queue of chunks haven't been processed yet
        private BlockingQueue<ZipBlock> _unprocessedChunks;
        //the max size of _unprocessedChunks
        private int _unprocessedChunksQueueSize;

        #endregion

        #region static

        private static void Dump(FileStream writer, ZipBlock currentBlock, bool writeWithBlockLength)
        {
            if (writeWithBlockLength)
            {
                //write header
                byte[] chunkHeader = BitConverter.GetBytes(currentBlock.Data.Length);
                writer.Write(chunkHeader, 0, chunkHeader.Length);
            }

            //write data
            writer.Write(currentBlock.Data, 0, currentBlock.Data.Length);
        }

        private static ZipBlock GetMinBlock(BlockingSortableMinQueue<ZipBlock> queue)
        {
            ZipBlock min;
            try
            {
                min = queue.Min;
            }
            catch (InvalidOperationException)
            {
                //queue is empty
                min = null;
            }
            return min;
        }

        private static ZipBlock GetNextChunk(BlockingSortableMinQueue<ZipBlock> queue)
        {
            ZipBlock currentBlock = null;

            try
            {
                currentBlock = queue.Peek();
            }
            catch (InvalidOperationException)
            {
                //queue is empty
                currentBlock = null;
            }

            return currentBlock;
        }

        #endregion

        #region BaseCodingProcessor

        public ByChunkCodingProcessor(IZipProcessor zipProcessor, CompressionMode mode) : base(zipProcessor, mode)
        {
        }

        /// <summary>
        ///     Compressing inputFile to outputFile
        /// </summary>
        /// <param name="inputPath">path to inputFile</param>
        /// <param name="outputPath">path to outputFile</param>
        protected override void Compress(string inputPath, string outputPath)
        {
            try
            {
                this.BaseInitialize(CompressionMode.Compress);
                this.InitializingCompression(inputPath);

                Thread writeThread = new Thread(() => { this.WritingThreadBody(outputPath, CompressionMode.Compress); });
                writeThread.Start();

                Thread compress = new Thread(() => { this.ProcessThreadBody(base.ZipProcessor.Compress); });
                compress.Start();

                using (FileStream reader = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.None, ByChunkCodingProcessor.BufferSize))
                {
                    byte[] buffer = new byte[ByChunkCodingProcessor.BufferSize];
                    int byteRead;
                    long blockIndex = 0;
                    while ((byteRead = reader.Read(buffer, 0, ByChunkCodingProcessor.BufferSize)) > 0)
                    {
                        if (this._isStop)
                        {
                            return;
                        }

                        using (MemoryStream mStream = new MemoryStream())
                        {
                            mStream.Write(buffer, 0, byteRead);
                            ZipBlock block = new ZipBlock(mStream.ToArray(), blockIndex);
                            this._unprocessedChunks.Enqueue(block);
                        }

                        blockIndex++;
                    }
                }

                writeThread.Join();
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occurred during the compression", ex);
            }
            finally
            {
                this._processedChunks.Close();
                this._unprocessedChunks.Close();
            }
        }

        /// <summary>
        ///     Decompressing inputFile to outputFile
        /// </summary>
        /// <param name="inputPath">path to inputFile</param>
        /// <param name="outputPath">path to outputFile</param>
        protected override void Decompress(string inputPath, string outputPath)
        {
            try
            {
                this.BaseInitialize(CompressionMode.Decompress);

                Thread writeThread = new Thread(() => { this.WritingThreadBody(outputPath, CompressionMode.Decompress); });
                Thread decompress = new Thread(() => { this.ProcessThreadBody(base.ZipProcessor.Decompress); });

                using (FileStream reader = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.None, ByChunkCodingProcessor.BufferSize))
                {
                    //first inint then start
                    this.InitializingDecompression(reader);
                    writeThread.Start();
                    decompress.Start();

                    int bytesRead;
                    long blockIndex = 0;
                    do
                    {
                        if (this._isStop)
                        {
                            return;
                        }

                        //read header
                        byte[] chunkHeader = new byte[sizeof (int)];
                        int chunkHeaderLength = reader.Read(chunkHeader, 0, sizeof (int));
                        if (chunkHeaderLength == 0)
                        {
                            break;
                        }
                        if (chunkHeaderLength != sizeof (int))
                        {
                            throw new InvalidOperationException("Invalid chunk header");
                        }

                        //determine the exact length of the next chunk
                        int chunkSize = BitConverter.ToInt32(chunkHeader, 0);
                        byte[] chunk = new byte[chunkSize];

                        bytesRead = reader.Read(chunk, 0, chunkSize);
                        if (bytesRead <= 0)
                        {
                            break;
                        }
                        ZipBlock block = new ZipBlock(chunk, blockIndex);
                        this._unprocessedChunks.Enqueue(block);
                        blockIndex++;
                    } while (bytesRead > 0);
                }

                writeThread.IsBackground = false;
            }
            catch (Exception ex)
            {
                throw new Exception("An error has occurred during the decompression", ex);
            }
            finally
            {
                this._processedChunks.Close();
                this._unprocessedChunks.Close();
            }
        }

        /// <summary>
        ///     Forced canceling of execution
        /// </summary>
        public override void Shutdown()
        {
            this._isStop = true;
        }

        #endregion

        #region helper methods

        #region compressing

        //write general data about compression to file
        private void WriteGeneralCompressionData(FileStream writer)
        {
            //always put on boundary markup for processing file by chunks
            byte[] boundaryBytes = Boundary.GetBoundaryBytes();
            writer.Write(boundaryBytes, 0, boundaryBytes.Length);

            //write total chunk's count
            byte[] totalBlocksCounterBytes = BitConverter.GetBytes(this._unprocessedBlockCounter);
            writer.Write(totalBlocksCounterBytes, 0, totalBlocksCounterBytes.Length);
        }

        //compression initialization
        private void InitializingCompression(string inputPath)
        {
            //init total block counts
            long length = new FileInfo(inputPath).Length;
            long bufferSizeInt64 = Convert.ToInt64(ByChunkCodingProcessor.BufferSize);
            this._unprocessedBlockCounter = (length + bufferSizeInt64 - 1L)/bufferSizeInt64;
        }

        #endregion

        #region decompression

        //decompression initialization
        private void InitializingDecompression(FileStream reader)
        {
            //skip boundary bytes
            byte[] boundaryBytes = Boundary.GetBoundaryBytes();
            reader.Seek(boundaryBytes.Length, SeekOrigin.Begin);

            //init total block counts
            byte[] totalBlocksCounterBytes = new byte[sizeof (long)];
            int totalBlocksCounterByteRead = reader.Read(totalBlocksCounterBytes, 0, sizeof (long));
            if (totalBlocksCounterByteRead != sizeof (long))
            {
                throw new InvalidOperationException("Invalid file header");
            }

            this._unprocessedBlockCounter = BitConverter.ToInt64(totalBlocksCounterBytes, 0);
        }

        #endregion

        #region general

        //the thread's body for writing to file during the compression/decompression operation
        private void WritingThreadBody(string outputPath, CompressionMode mode)
        {
            using (FileStream writer = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, ByChunkCodingProcessor.BufferSize))
            {
                //helped data must be written only for compressing
                if (mode == CompressionMode.Compress)
                {
                    this.WriteGeneralCompressionData(writer);
                }

                int i = 0;
                while (i < this._unprocessedBlockCounter)
                {
                    ZipBlock min = null;
                    while (min == null || min.Index != i)
                    {
                        this._regularChunkProcessed.WaitOne();
                        if (this._isStop)
                        {
                            return;
                        }

                        min = ByChunkCodingProcessor.GetMinBlock(this._processedChunks);
                    }

                    //the queue have to be sorted
                    this._processedChunks.Sort(new ShellSort<ZipBlock>());

                    //dump to file while it possible
                    ZipBlock currentChunk = null;
                    do
                    {
                        if (this._isStop || !this._processedChunks.TryDequeue(out currentChunk))
                        {
                            return;
                        }

                        ByChunkCodingProcessor.Dump(writer, currentChunk, mode == CompressionMode.Compress);

                        //go to next chunk in the queue
                        i++;
                        currentChunk = ByChunkCodingProcessor.GetNextChunk(this._processedChunks);
                    } while (i < this._unprocessedBlockCounter && currentChunk != null && currentChunk.Index == i);

                    //forced min refresging action has to be done after dequeing
                    this._processedChunks.RefreshMin();
                }
            }
        }

        //parallel executing passed processAction
        private void ProcessThreadBody(Func<byte[], byte[]> processAction)
        {
            #region check

            if (processAction == null)
            {
                throw new ArgumentNullException("processAction");
            }

            #endregion

            using (SimpleThreadPool threadPool = new SimpleThreadPool())
            {
                for (long i = 0; i < this._unprocessedBlockCounter; i++)
                {
                    if (this._isStop)
                    {
                        return;
                    }

                    threadPool.QueueTask(() => { this.ProcessChunk(processAction); });
                }
            }
        }

        ////atomic chunk processing
        //private void AtomicChunkProcessingAction(Func<byte[], byte[]> processAction)
        //{
        //    if (this._isStop)
        //    {
        //        return;
        //    }

        //    this.ProcessChunk(processAction);
        //}

        //base logic of chunk processing
        private void ProcessChunk(Func<byte[], byte[]> processAction)
        {
            #region check

            if (processAction == null)
            {
                throw new ArgumentNullException("processAction");
            }

            #endregion

            //get
            ZipBlock unprocessedBlock;
            if (this._unprocessedChunks.TryDequeue(out unprocessedBlock))
            {
                //process
                byte[] processedBytes = processAction(unprocessedBlock.Data);
                ZipBlock processedBlock = new ZipBlock(processedBytes, unprocessedBlock.Index);
                //dump
                this._processedChunks.Enqueue(processedBlock);
                this._regularChunkProcessed.Set();
            }
        }

        //base initialization before either compression and decompression operation
        private void BaseInitialize(CompressionMode mode)
        {
            //base init
            this._isStop = false;
            this._regularChunkProcessed = new AutoResetEvent(false);

            //init chunks sizes
            ulong maxNetAllowed = 1UL << 31; // 2GB
            ulong freePhMemory = new ComputerInfo().AvailablePhysicalMemory;
            ulong ourAllowedBytes = Math.Min(freePhMemory, maxNetAllowed);

            double loadFactor = 0.9D;
            ulong bytesIsGoingTobeUsing = (ulong) (ourAllowedBytes*loadFactor);

            int bothQueuesPossibleSize = (int) (bytesIsGoingTobeUsing/ByChunkCodingProcessor.BufferSize);

            if (CompressionMode.Decompress == mode)
            {
                this._unprocessedChunksQueueSize = bothQueuesPossibleSize/3;
                this._processedChunksQueueSize = bothQueuesPossibleSize - this._unprocessedChunksQueueSize;
            }

            if (CompressionMode.Compress == mode)
            {
                this._processedChunksQueueSize = bothQueuesPossibleSize/3;
                this._unprocessedChunksQueueSize = bothQueuesPossibleSize - this._processedChunksQueueSize;
            }

            //checking
            if (this._processedChunksQueueSize == 0 || this._unprocessedChunksQueueSize == 0)
            {
                throw new InsufficientMemoryException("Not enough free physical memory");
            }

            this._processedChunks = new BlockingSortableMinQueue<ZipBlock>(this._processedChunksQueueSize, false);
            this._unprocessedChunks = new BlockingQueue<ZipBlock>(this._unprocessedChunksQueueSize);
        }

        #endregion

        #endregion
    }

}