using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T[] Slice<T>(this T[] source, int start, int end)
    {
        // Handles negative ends.
        if (end < 0)
        {
            end = source.Length + end;
        }
        int len = end - start;

        // Return new array.
        T[] res = new T[len];
        for (int i = 0; i < len; i++)
        {
            res[i] = source[i + start];
        }
        return res;
    }

    public static float Difference(this Color color, Color other, bool alpha = false)
    {
        int maxIndex = alpha ? 4 : 3;
        float diff = 0;

        for (int i = 0; i < maxIndex; i++)
        {
            color[i] = Mathf.Clamp01(color[i]);
            other[i] = Mathf.Clamp01(other[i]);

            diff += Mathf.Abs(color[0] - other[0]);
        }

        return diff;
    }

    public static int CloserTo(this int[] values, int value)
    {
        int i = 0;
        for (i = 0; i < values.Length - 1; i++)
        {
            int current = values[i];
            int next = values[i + 1];

            if (current <= value && next >= value)
            {
                var diffToCurrent = value - current;
                var diffToNext = next - value;

                i = diffToCurrent <= diffToNext ? i : i + 1;
                break;
            }
        }

        return values[i];
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        System.Random rnd = new System.Random();
        while (n > 1)
        {
            int k = (rnd.Next(0, n) % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}