using System;
using ZipCore.GZip;
using ZipCore.Util;

namespace ZipCore
{
    /// <summary>
    /// Create concreate implementation IZipProcessor interface by ZipFormat
    /// </summary>
    public static class ZipCreator
    {
        #region static

        public static IZipProcessor Create(ZipFormat zipFormat)
        {
            IZipProcessor zipProcessor;
            switch (zipFormat)
            {
                case ZipFormat.GZip:
                {
                    zipProcessor = new GZipProcessor();
                    break;
                }
                default:
                {
                    throw new NotImplementedException(string.Format("{0} case hasn't been implemented yet", zipFormat));
                }
            }
            return zipProcessor;
        }

        #endregion
    }

}