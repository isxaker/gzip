using System;
using System.IO;
using System.IO.Compression;
using ZipCore;

namespace GZipTest.Core
{
    /// <summary>
    /// Processing file in fully mode
    /// </summary>
    public sealed class InFullCodingProcessor : BaseCodingProcessor
    {
        #region  constructors


        /// <summary>
        /// InFullCodingProcessor constructor
        /// </summary>
        /// <param name="zipProcessor">Processor will is used to processing input file</param>
        /// <param name="mode">processing mode</param>
        public InFullCodingProcessor(IZipProcessor zipProcessor, CompressionMode mode) : base(zipProcessor, mode)
        {
        }

        #endregion


        /// <summary>
        /// Compressing inputFile to outputFile
        /// </summary>
        /// <param name="inputPath">path to inputFile</param>
        /// <param name="outputPath">path to outputFile</param>
        protected override void Compress(string inputPath, string outputPath)
        {
            byte[] originalBytes = File.ReadAllBytes(inputPath);
            byte[] zippedBytes = base.ZipProcessor.Compress(originalBytes);
            File.WriteAllBytes(outputPath, zippedBytes);
        }

        /// <summary>
        /// Decompressing inputFile to outputFile
        /// </summary>
        /// <param name="inputPath">path to inputFile</param>
        /// <param name="outputPath">path to outputFile</param>
        protected override void Decompress(string inputPath, string outputPath)
        {
            byte[] zippedBytes = File.ReadAllBytes(inputPath);
            byte[] originalBytes = base.ZipProcessor.Decompress(zippedBytes);
            File.WriteAllBytes(outputPath, originalBytes);
        }

        public override void Shutdown()
        {
            throw new NotSupportedException("This action is not supported");
        }

    }

}