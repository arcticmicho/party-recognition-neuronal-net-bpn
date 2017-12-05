using UnityEngine;
using System.Collections;
using System;

public class Greedy5RecognitionStrategy : IRecognitionHeuristicStrategy {

    private float m_epsilon;

    private bool m_usesWeight;
    public bool UsesWeight
    {
        get { return m_usesWeight; }
    }

    private int m_weightFactor;

    /// <summary>
    /// Greedy 5 Recognizer
    /// </summary>
    /// <param name="epsilon">Define how many times the search will be excuted, epsilon must be a value between 0 and 1</param>
    /// <param name="usesWeight">Weight Parameter for the Greedy 5 algorithm, with value 0 the weight is not included in the algorithm and the class will behave as a Greedy2</param>
    public Greedy5RecognitionStrategy(float epsilon, bool usesWeight)
    {
        m_epsilon = Mathf.Min(1f,epsilon);
        m_usesWeight = usesWeight;
        m_weightFactor = usesWeight ? 1 : 0;
    }

    public float ProcessHeuristic(PRPatternDefinition pattern1, PRPatternDefinition pattern2)
    {
        int cloudLenght = pattern1.CloudPoints.Count;
        int steps = Mathf.FloorToInt(Mathf.Pow(cloudLenght, 1f - m_epsilon));
        float minPointsDistance = float.MaxValue;

        for(int i=0; i<cloudLenght; i += steps)
        {
            //float firstDistance = Greedy5PointsDistance(pattern1.CloudPoints, pattern2.CloudPoints, i);
            //float secondDistance = Greedy5PointsDistance(pattern2.CloudPoints, pattern1.CloudPoints, i);
            //
            //minPointsDistance = Mathf.Min(firstDistance, secondDistance, minPointsDistance);
        }
        return minPointsDistance;
    }

    private float Greedy5PointsDistance(Vector2[] cloudPoints1, Vector2[] cloudPoints2, int startIndex)
    {
        int cloudPoints = cloudPoints1.Length;
        bool[] matchedPoints = new bool[cloudPoints];

        float pointsDistance = 0f;
        int i = startIndex;
        do
        {
            int index = -1;
            float minDistance = float.MaxValue;
            for (int m = 0; m < cloudPoints; m++)
            {
                if (!matchedPoints[m])
                {
                    float distance = MathUtils.SqrEuclideanDistance(cloudPoints1[i], cloudPoints2[m]);
                    if (distance < minDistance)
                    {
                        index = m;
                        minDistance = distance;
                    }
                }
            }
            matchedPoints[index] = true;
            float weight = 1.0f - ((i - startIndex + cloudPoints) % cloudPoints) / (1.0f * cloudPoints);
            pointsDistance += minDistance * weight;
            i = (i + 1) % cloudPoints;
        } while (i != startIndex);

        return pointsDistance;
    }
}
