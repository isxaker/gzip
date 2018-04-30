using System;
using System.Security.Cryptography;
using System.Text;

namespace GZipTest.Util
{
    /// <summary>
    /// Boundary for ByChunkCodingProcessor
    /// </summary>
    public static class Boundary
    {
        #region  constructors

        //generating boundary
        static Boundary()
        {
            try
            {
                using (MD5 md5Hash = MD5.Create())
                {
                    int prevLastIndex = 0;
                    foreach (string word in Boundary.BoundaryWords)
                    {
                        byte[] hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(word));

                        Array.Copy(hash, 0, Boundary._bytes, prevLastIndex, hash.Length);
                        prevLastIndex += hash.Length;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Boundary cannot be created", ex);
            }
        }

        #endregion

        #region static

        private static readonly byte[] _bytes = new byte[16*4];

        private static readonly string[] BoundaryWords = {"---isxaker---", "---veeam---", "---chunk-by-chunk---", "11.02.2016 18:30:455"};

        public static byte[] GetBoundaryBytes()
        {
            return Boundary._bytes;
        }

        #endregion
    }

}