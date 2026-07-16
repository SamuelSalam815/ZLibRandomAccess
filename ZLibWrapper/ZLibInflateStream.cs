using System.Diagnostics;
using System.Runtime.InteropServices;
using ZLibBindings;
using ZLibBindings.Constants;
using ZLibBindings.State;
using ZLibWrapper.Extensions;

namespace ZLibWrapper;

internal unsafe class ZLibInflateStream : Stream
{
    private readonly z_stream_s* _zLibStream;
    private readonly Stream _stream;
    private readonly bool _leaveOpen;

    public ZLibInflateStream(Stream stream, bool leaveOpen = false)
    {
        _stream = stream;
        _leaveOpen = leaveOpen;
        _zLibStream = (z_stream_s*)Marshal.AllocHGlobal(sizeof(z_stream_s));
        *_zLibStream = new z_stream_s
        {
            zfree = null,
            zalloc = null,
            opaque = null
        };
        ZLibLowLevelBindings
            .inflateInit(_zLibStream)
            .GuardAgainstFatalErrors(_zLibStream);
    }

    public override void Flush()
    {
        throw new NotSupportedException("Flush is not supported!");
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        //TODO
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException("Seeking in this stream is not supported!");
    }

    public override void SetLength(long value)
    {
        throw new InvalidOperationException("Setting stream length of a read-only stream is not a valid operation!");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new InvalidOperationException("Cannot write to a read-only stream!");
    }

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => throw new NotSupportedException("Setting stream position is not supported!");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!_leaveOpen)
            {
                _stream.Dispose();
            }

            ZLibLowLevelBindings
                .inflateEnd(_zLibStream)
                .GuardAgainstFatalErrors(_zLibStream);
            Marshal.FreeHGlobal((IntPtr)_zLibStream);
        }
        base.Dispose(disposing);
    }
}
