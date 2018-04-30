using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Helpers.Extensions;

namespace GZipTest.Util
{

    public static class ProgramOptions
    {
        #region constants

        //It's a sort of a file separator or divider - every file that is larger than value we consider as a real large file (and process it by chunks)
        //the other files is considered as a quite small files => that's why that file's types are processed in full
        public const int MaxFileSizeForFullyProcessingInBytes = 16*1024*1024; // 16 MB

        #endregion

        #region static

        /// <summary>
        /// help
        /// </summary>
        /// <returns>command line help info</returns>
        public static string GetHelpInfo()
        {
            return
                "You must use 3 parameters: {action} {inputFile} {outputFile}\ncompress\t - Compress inputFile to outputFile\ndecompress\t - Decompress inputFile to outputFile";
        }

        /// <summary>
        /// Initialize promram otions by command line args
        /// </summary>
        /// <param name="args">command line args</param>
        public static void Initialize(string[] args)
        {
            //general
            if (args == null || args.Length != 3 || args.Any(s => s.IsNullOrWhiteSpace()))
            {
                throw new ArgumentException("Invalid command line arguments' number");
            }

            //check first cmd argument and initialize program's mode
            try
            {
                ProgramOptions.InitializeMode(args[0]);
            }
            catch (InvalidOperationException iEx)
            {
                throw new ArgumentException(string.Format("Parameter {0} is wrong", args[0]), iEx);
            }

            //check second cmd argument and assign
            if (!File.Exists(args[1]))
            {
                FileNotFoundException innerEx = new FileNotFoundException("File hasn't been found or not existed", args[1]);
                throw new ArgumentException(string.Format("Parameter {0} is wrong", args[1]), innerEx);
            }
            ProgramOptions.PathForInput = args[1];

            //check third cmd argument and assign
            try
            {
                string outputDirectory = Path.GetDirectoryName(args[2]);
                string outputFileName = Path.GetFileName(args[2]);
                if (outputFileName.IsNullOrWhiteSpace() || outputDirectory == null || (outputDirectory != string.Empty && !Directory.Exists(outputDirectory)))
                {
                    throw new Exception("Wrong output path");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("Parameter {0} is wrong", args[2]), ex);
            }
            ProgramOptions.PathForOutput = args[2];

            ProgramOptions.InitializeZippingType();
        }

        private static void InitializeMode(string value)
        {
            if (value.ToLower() != "decompress" && value.ToLower() != "compress")
            {
                throw new InvalidOperationException(string.Format("The specified command: {0} is invalid", value));
            }

            ProgramOptions.Mode = (CompressionMode) Enum.Parse(typeof (CompressionMode), value, true);
        }

        //determine what kind of processing can be better
        private static void InitializeZippingType()
        {
            //determine by size on input file
            long inputFileSizeInBytes = new FileInfo(ProgramOptions.PathForInput).Length;
            ProcessingFileMode processingFileMode = inputFileSizeInBytes <= ProgramOptions.MaxFileSizeForFullyProcessingInBytes
                ? ProcessingFileMode.InFull
                : ProcessingFileMode.ByChank;

            //if decompress needs one more checking
            //large file could be compressed chunk by chunk and became quite small =)
            if (ProgramOptions.Mode == CompressionMode.Decompress && processingFileMode == ProcessingFileMode.InFull)
            {
                byte[] boundaryBytes = Boundary.GetBoundaryBytes();
                byte[] possibleBoundary = new byte[boundaryBytes.Length];

                //detect by special boundary in first line of compressed file
                using (BinaryReader reader = new BinaryReader(new FileStream(ProgramOptions.PathForInput, FileMode.Open)))
                {
                    reader.Read(possibleBoundary, 0, boundaryBytes.Length);
                }

                if (possibleBoundary.SequenceEqual(boundaryBytes))
                {
                    processingFileMode = ProcessingFileMode.ByChank;
                }
            }

            ProgramOptions.ProcessingFileMode = processingFileMode;
        }

        public static CompressionMode Mode { get; private set; }
        public static string PathForInput { get; private set; }
        public static string PathForOutput { get; private set; }
        public static ProcessingFileMode ProcessingFileMode { get; set; }

        #endregion
    }

}