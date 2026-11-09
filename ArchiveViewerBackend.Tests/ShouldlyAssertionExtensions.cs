using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace ArchiveViewerBackend.Tests;

public static class ShouldlyAssertionExtensions
{
    public static void ShouldMatch<T>(
        this IEnumerable<T> actual,
        IEnumerable<T> expected)
    {
        var actualList = actual.ToList();
        var expectedList = expected.ToList();
        actualList.Count.ShouldBe(expectedList.Count());
        actualList.ShouldSatisfyAllConditions(
            actualList
                .Select<T, Action>((actualItem, index) =>
                    () => actualItem.ShouldBe(expectedList[index], customMessage: $"Mismatch at index {index}"))
                .ToArray()
        );
    }
}
