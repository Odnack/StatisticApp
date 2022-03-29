namespace Infrastructure.Compression
{
    public class Compressor
    {
        static Compressor()
        {
            GZIP = new GZIPCompressor();
            ZStandart = new ZStandartCompressor();
        }
        public const int MaxInMemoryCompressionSize = 100 * 1024 * 1024;

        public static GZIPCompressor GZIP { get; }
        public static ZStandartCompressor ZStandart { get; }
    }
}