namespace ArchiveViewerBackend;

/// <summary>
/// Two read-only spans that read as one contiguous sequence, for example the two segments of a wrapped
/// <see cref="RingBuffer{T}"/>. It is a ref struct, so it cannot outlive the spans it views.
/// </summary>
public readonly ref struct ReadOnlySpanPair<T>
{
    public ReadOnlySpanPair(ReadOnlySpan<T> first, ReadOnlySpan<T> second)
    {
        First = first;
        Second = second;
    }

    public ReadOnlySpan<T> First { get; }

    public ReadOnlySpan<T> Second { get; }

    public int Length => First.Length + Second.Length;

    public bool IsEmpty => Length == 0;

    public ref readonly T this[int index] => ref (index < First.Length ? ref First[index] : ref Second[index - First.Length]);

    public ReadOnlySpanPair<T> Slice(int start) => Slice(start, Length - start);

    public ReadOnlySpanPair<T> Slice(int start, int length)
    {
        if (start < 0 || start > Length || length < 0 || length > Length - start)
        {
            throw new ArgumentOutOfRangeException(
                nameof(start),
                $"Expected start {start} and length {length} to be within a sequence of length {Length}");
        }

        if (start >= First.Length)
        {
            return new ReadOnlySpanPair<T>(Second.Slice(start - First.Length, length), default);
        }

        var lengthInFirst = Math.Min(length, First.Length - start);
        return new ReadOnlySpanPair<T>(First.Slice(start, lengthInFirst), Second[..(length - lengthInFirst)]);
    }

    public void CopyTo(Span<T> destination)
    {
        if (destination.Length < Length)
        {
            throw new ArgumentException($"Expected destination length to be at least {Length}", nameof(destination));
        }

        First.CopyTo(destination);
        Second.CopyTo(destination[First.Length..]);
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private readonly ReadOnlySpanPair<T> _pair;
        private int _index = -1;

        internal Enumerator(ReadOnlySpanPair<T> pair)
        {
            _pair = pair;
        }

        public bool MoveNext() => ++_index < _pair.Length;

        public readonly T Current => _pair[_index];
    }
}
