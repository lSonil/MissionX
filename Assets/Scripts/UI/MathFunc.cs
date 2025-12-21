using System.Collections.Generic;
using UnityEngine;

public static class MathFunc
{
    public static int Fibonacci(int n)
    {
        if (n <= 1)
            return 1;

        int a = 1;
        int b = 1;

        for (int i = 2; i < n; i++)
        {
            int next = a + b;
            a = b;
            b = next;
        }

        return b;
    }
    public static int Triangular(int n)
    {
        return n * (n + 1) / 2;
    }
    public static void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

}
