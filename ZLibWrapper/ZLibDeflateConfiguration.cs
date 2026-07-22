using ZLibBindings.Constants;

namespace ZLibWrapper;

public record ZLibDeflateConfiguration
{
    public ZCompressionLevel CompressionLevel { get; init; } = ZCompressionLevel.Z_DEFAULT_COMPRESSION;

    public ZWindowBits WindowBits { get; init; } = ZWindowBits.DefaultWindowSize;

    public ZMemoryLevel MemoryLevel { get; init; } = ZMemoryLevel.Default;

    public ZCompressionStrategy CompressionStrategy { get; init; } = ZCompressionStrategy.Z_DEFAULT_STRATEGY;
}
