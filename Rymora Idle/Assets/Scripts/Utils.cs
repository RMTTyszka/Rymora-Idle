using System;
using System.Collections.Generic;

public static class ListExtensions
{
    private static Random rng = new Random();

    public static T RandomElement<T>(this List<T> list)
    {
        if (list.Count == 0)
        {
            throw new InvalidOperationException("Cannot select a random element from an empty list");
        }

        int index = rng.Next(list.Count);
        return list[index];
    }
}