using System;
using System.IO.Compression;
using GZipTest.Util;
using ZipCore;

namespace GZipTest.Core
{
    /// <summary>
    /// Create concreate implementation BaseCodingProcessor interface by ProcessingFileMode
    /// </summary>
    public static class CodingProcessorCreator
    {
        #region static

        public static BaseCodingProcessor CreateCodingProcessor(IZipProcessor zipProcessor, ProcessingFileMode processingFileMode, CompressionMode mode)
        {
            BaseCodingProcessor codingProcessor;
            switch (processingFileMode)
            {
                case ProcessingFileMode.InFull:
                {
                    codingProcessor = new InFullCodingProcessor(zipProcessor, mode);
                    break;
                }
                case ProcessingFileMode.ByChank:
                {
                    codingProcessor = new ByChunkCodingProcessor(zipProcessor, mode);
                    break;
                }
                default:
                {
                    throw new NotImplementedException(string.Format("{0} case hasn't been implemented yet", processingFileMode));
                }
            }
            return codingProcessor;
        }

        #endregion
    }

}