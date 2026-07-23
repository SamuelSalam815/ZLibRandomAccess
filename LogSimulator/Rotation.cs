using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace LogSimulator;

[CollectionBuilder(typeof(RotationBuilder), nameof(RotationBuilder.Create))]
public record Rotation<T>(ImmutableList<T> Items, int CurrentIndex = 0) : IEnumerable<T>
{
    public Rotation(IEnumerable<T> items) : this(items.ToImmutableList())
    {
    }

    public Rotation<T> Cycle()
    {
        return Cycle(out var _);
    }

    public Rotation<T> Cycle(out T nextItem)
    {
        return Cycle(true, out nextItem);
    }

    private Rotation<T> Cycle(bool goForward, out T nextItem)
    {
        var step = goForward ? 1 : -1;
        nextItem = Items[CurrentIndex];
        return this with { CurrentIndex = ((CurrentIndex + step) + Items.Count) % Items.Count };
    }

    public Rotation<T> Cycle(int numCycles, out List<T> itemsCycled)
    {
        itemsCycled = [];
        var numSteps = Math.Abs(numCycles);
        var result = this;
        for (var i = 0; i < numSteps; i++)
        {
            result = result.Cycle(numCycles > 0, out var nextItem);
            itemsCycled.Add(nextItem);
        }
        return result;
    }

    public IEnumerator<T> GetEnumerator()
    {
        Cycle(Items.Count, out var items);
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class RotationBuilder
{
    public static Rotation<T> Create<T>(ReadOnlySpan<T> items) => new([..items]);
}
