namespace LogSimulator.Appliction;

public class BroadcastStream(List<SubscribedStream> subscriptions) : Stream
{
    private bool isDisposed;
    public override void Flush()
    {
        foreach (var subscription in subscriptions)
        {
            subscription.Stream.Flush();
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        foreach (var subscription in subscriptions)
        {
            subscription.Stream.Write(buffer, offset, count);
        }
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => subscriptions.All(subscription => subscription.Stream.CanWrite);
    public override long Length => subscriptions.First().Stream.Length;
    public override long Position
    {
        get => subscriptions.First().Stream.Position;
        set => Seek(value, SeekOrigin.Begin);
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing || isDisposed)
        {
            return;
        }

        foreach (var subscription in subscriptions.Where(subscription => !subscription.LeaveOpen))
        {
            subscription.Stream.Dispose();
        }

        isDisposed = true;
    }
}
