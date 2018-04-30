using System;
using System.IO;
using System.Linq;
using System.Text;
using GZipTest.Core;
using GZipTest.Util;
using ZipCore;
using ZipCore.Util;

namespace GZipTest
{

    internal class Program
    {
        #region static

        #region test

        private static void CreateLargeFile(string fileName, int sizeInMegaBytes, bool isRandom)
        {
            Random r = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 65536*2))
            {
                using (StreamWriter sWr = new StreamWriter(fs, Encoding.GetEncoding(1251), 65536*2))
                {
                    for (int i = 0; i < 8*sizeInMegaBytes; i++)
                    {
                        char[] chBuffer;
                        if (isRandom)
                        {
                            chBuffer = Enumerable.Repeat(chars, 65536*2).Select(s => s[r.Next(s.Length)]).ToArray();
                        }
                        else
                        {
                            chBuffer = Enumerable.Repeat('1', 65536*2).ToArray();
                        }

                        sWr.Write(chBuffer, 0, chBuffer.Length);
                    }
                }
            }
        }

        #endregion

        private static int Main(string[] args)
        {
            #region for testing

            //Program.CreateLargeFile("large.txt", 30, false);

            //args = new string[3];

            //args[0] = "decompress"; //decompress    //compress
            //args[2] = "1_1.txt";
            //args[1] = "result.gz";

            #endregion

            try
            {
                ProgramOptions.Initialize(args);

                // This is the only format that is supported now
                IZipProcessor zipProcessor = ZipCreator.Create(ZipFormat.GZip);
                BaseCodingProcessor codingProcessor = CodingProcessorCreator.CreateCodingProcessor(zipProcessor, ProgramOptions.ProcessingFileMode, ProgramOptions.Mode);

                Console.CancelKeyPress += (s, a) =>
                {
                    try
                    {
                        codingProcessor.Shutdown();
                        Console.WriteLine("\nThe operation has been interrupted.");
                    }
                    catch (NotSupportedException)
                    {
                        //cancel methods doesn't supported for InFullCodingProcessor (it's already take a minimum time)
                    }
                };

                codingProcessor.Do(ProgramOptions.PathForInput, ProgramOptions.PathForOutput);
            }
            catch (ArgumentException argEx)
            {
                string errMsg = argEx.ToString();
                Console.WriteLine(errMsg);
                string help = ProgramOptions.GetHelpInfo();
                Console.WriteLine(help);
                return 1;
            }
            catch (Exception ex)
            {
                string errMsg = ex.ToString();
                Console.WriteLine("Error has been occurred - {0}", errMsg);
                return 1;
            }
            return 0;
        }

        #endregion
    }

}