using System;
using System.IO.Compression;
using ZipCore;

namespace GZipTest.Core
{
    /// <summary>
    /// abstract class for coding file
    /// </summary>
    public abstract class BaseCodingProcessor
    {
        #region  fields

        private readonly Action<string, string> _codingAction;
        protected readonly IZipProcessor ZipProcessor;

        #endregion

        #region  constructors


        /// <summary>
        /// BaseCodingProcessor constructor
        /// </summary>
        /// <param name="zipProcessor">Processor will is used to processing input file</param>
        /// <param name="mode">processing mode</param>
        protected BaseCodingProcessor(IZipProcessor zipProcessor, CompressionMode mode)
        {
            this.ZipProcessor = zipProcessor;
            if (mode == CompressionMode.Compress)
            {
                this._codingAction = this.Compress;
            }
            else
            {
                this._codingAction = this.Decompress;
            }
        }

        #endregion

        /// <summary>
        /// Do process
        /// </summary>
        /// <param name="inputPath">path to inputFile</param>
        /// <param name="outputPath">path to outputFile</param>
        public void Do(string inputPath, string outputPath)
        {
            if (string.IsNullOrEmpty(inputPath))
            {
                throw new ArgumentNullException(inputPath);
            }
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentNullException(outputPath);
            }

            this._codingAction(inputPath, outputPath);
        }

        /// <summary>
        /// Compressing inputFile to outputFile
        /// </summary>
        /// <param name="inputPath">path to inputFile</param>
        /// <param name="outputPath">path to outputFile</param>
        protected abstract void Compress(string inputPath, string outputPath);
        
        /// <summary>
        /// Compressing inputFile to outputFile
        /// </summary>
        /// <param name="inputPath">path to inputFile</param>
        /// <param name="outputPath">path to outputFile</param>
        protected abstract void Decompress(string inputPath, string outputPath);

        /// <summary>
        /// Forced canceling of execution
        /// </summary>
        public abstract void Shutdown();

    }

}