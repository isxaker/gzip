namespace ZipCore
{
    /// <summary>
    /// Interface for compressing and decompressing input byte array to outpout byte array
    /// </summary>
    public interface IZipProcessor
    {
        /// <summary>
        /// Compressing byte array
        /// </summary>
        /// <param name="data">byte array for compressing</param>
        /// <returns>compressed byte arryas</returns>
        byte[] Compress(byte[] data);

        /// <summary>
        /// Decompressing byte array
        /// </summary>
        /// <param name="data">byte array for decompressing</param>
        /// <returns>decompressed byte arryas</returns>
        byte[] Decompress(byte[] data);

    }

}