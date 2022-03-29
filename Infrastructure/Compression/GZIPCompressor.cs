using System;
using System.IO;
using System.IO.Compression;

namespace Infrastructure.Compression
{
    public class GZIPCompressor
    {
        public Stream Compress(Stream streamForCompression, CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            Stream outputStream;
            if (streamForCompression.Length >= Compressor.MaxInMemoryCompressionSize)
            {
                outputStream = File.Create(Guid.NewGuid().ToString(), 64 * 1024, FileOptions.DeleteOnClose);
                using (var gzipStream = new GZipStream(outputStream, compressionLevel, true))
                {
                    streamForCompression.CopyTo(gzipStream);
                }
                outputStream.Position = 0;
            }
            else
            {
                outputStream = new MemoryStream();
                using (var gzipStream = new GZipStream(outputStream, compressionLevel, true))
                {
                    streamForCompression.CopyTo(gzipStream);
                }
                outputStream.Position = 0;
            }
            return outputStream;
        }

        public Stream Decompress(Stream streamForDecompression)
        {
            return new GZipStream(streamForDecompression, CompressionMode.Decompress, true);
        }

    }
}