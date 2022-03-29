using System;
using System.IO;
using System.IO.Compression;
using Zstandard.Net;

namespace Infrastructure.Compression
{
    public class ZStandartCompressor
    {
        public Stream Compress(Stream stream)
        {
            Stream outputStream;
            if (stream.Length >= Compressor.MaxInMemoryCompressionSize)
            {
                outputStream = File.Create(Guid.NewGuid().ToString(), 64 * 1024, FileOptions.DeleteOnClose);
                using (var compressionStream = new ZstandardStream(outputStream, CompressionMode.Compress, true))
                {
                    compressionStream.CompressionLevel = 3;
                    stream.CopyTo(compressionStream);
                }
                outputStream.Position = 0;
            }
            else
            {
                outputStream = new MemoryStream();
                using (var compressionStream = new ZstandardStream(outputStream, CompressionMode.Compress, true))
                {
                    compressionStream.CompressionLevel = 3;
                    stream.CopyTo(compressionStream);
                }
                outputStream.Position = 0;
            }
            return outputStream;
        }

        public Stream Decompress(Stream stream)
        {
            return new ZstandardStream(stream, CompressionMode.Decompress, true);
        }
    }
}