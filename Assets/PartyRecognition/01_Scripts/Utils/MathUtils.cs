using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class MathUtils
{
    /// <summary>
    /// Computes the Squared Euclidean Distance between two points in 2D
    /// </summary>
    public static float SqrEuclideanDistance(Vector2 a, Vector2 b)
    {
        return (a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y);
    }

    /// <summary>
    /// Computes the Euclidean Distance between two points in 2D
    /// </summary>
    public static float EuclideanDistance(Vector2 a, Vector2 b)
    {
        return Mathf.Sqrt(SqrEuclideanDistance(a, b));
    }

    public static float FixedRandomNumber(float min, float max)
    {
        return UnityEngine.Random.Range(0f, 1f) * (max - min) + min;
    }

    public static float[][] IdentityMatrixDotValue(int count)
    {
        float[][] outs = new float[count][];
        for (var i = 0; i < count; i++)
        {
            outs[i] = new float[count];
            for (var j = 0; j < count; j++)
            {
                outs[i][j] = i == j ? 0.99f : 0.0f;
            }
        }
        return outs;
    }

    public static List<int> FastRandomNumberList(int total)
    {
        List<int> numberList = new List<int>(total);
        for(int i=0; i<total; i++)
        {
            numberList.Add(i);
        }

        int[] randomList = new int[total];
        for(int i= total -1; i> -1; i--)
        {
            int picked = Mathf.FloorToInt(UnityEngine.Random.Range(0f, 1f) * i);
            randomList[i] = numberList[picked];
            numberList[picked] = numberList[i];
        }
        return new List<int>(randomList);
    }
}
