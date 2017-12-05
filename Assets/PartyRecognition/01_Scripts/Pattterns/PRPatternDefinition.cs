using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class PRPatternDefinition
{
    [SerializeField]
    private List<Vector2> m_cloudPoints;
    public List<Vector2> CloudPoints
    {
        get { return m_cloudPoints; }
    }

    /// <summary>
    /// Only for pre-defined patterns;
    /// </summary>
    [SerializeField]
    private string m_patternName;
    public string PatternName
    {
        get { return m_patternName; }
    }

    [SerializeField]
    private int m_samplingFactor;
    public int SamplingFactor
    {
        get { return m_samplingFactor; }
    }

    public PRPatternDefinition(List<Vector2> points, int samplingFactor, string name = "")
    {
        m_cloudPoints = points;
        m_patternName = name;
        m_samplingFactor = samplingFactor;
    }

    public void NormalizePoints()
    {
        //ScalePoints();
        //TranslatePointsByCenter();
        Simplify();
    }

    public void ScalePoints()
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        for (int i = 0; i < m_cloudPoints.Count; i++)
        {
            if (m_cloudPoints[i].x > maxX)
            {
                maxX = m_cloudPoints[i].x;
            }
            if (m_cloudPoints[i].x < minX)
            {
                minX = m_cloudPoints[i].x;
            }
            if (m_cloudPoints[i].y > maxY)
            {
                maxY = m_cloudPoints[i].y;
            }
            if (m_cloudPoints[i].y < minY)
            {
                minY = m_cloudPoints[i].y;
            }
        }

        List<Vector2> newPoints = new List<Vector2>(m_cloudPoints.Count);
        float scale = Mathf.Max(maxX - minY, maxY - minX);
        for (int i = 0; i < m_cloudPoints.Count; i++)
        {
            newPoints.Add(new Vector2((m_cloudPoints[i].x - minX) / scale, (m_cloudPoints[i].y - minY) / scale));
        }
        m_cloudPoints = newPoints;
    }

    public void TranslatePointsByCenter()
    {
        TranslatePointsByPoint(CalculateCentroid());
    }

    public void TranslatePointsByPoint(Vector2 point)
    {
        Vector2[] newPoints = new Vector2[m_cloudPoints.Count];
        for (int i = 0; i < m_cloudPoints.Count; i++)
        {
            newPoints[i] = new Vector2(m_cloudPoints[i].x - point.x, m_cloudPoints[i].y - point.y);
        }
        m_cloudPoints = new List<Vector2>(newPoints);
    }

    private Vector2 CalculateCentroid()
    {
        float cx = 0;
        float cy = 0;

        for (int i = 0; i < m_cloudPoints.Count; i++)
        {
            cx += m_cloudPoints[i].x;
            cy += m_cloudPoints[i].y;
        }
        return new Vector2(cx / (float)m_cloudPoints.Count, cy / (float)m_cloudPoints.Count);
    }

    public List<Vector2> ResamplePoints(int samplingFactor)
    {
        if (m_cloudPoints.Count == samplingFactor)
        {
            return m_cloudPoints;
        }

        List<Vector2> newPoints = new List<Vector2>(samplingFactor);
        newPoints[0] = new Vector2(m_cloudPoints[0].x, m_cloudPoints[0].y);
        int numPoints = 1;
        float I = PointsLenght() / (float)(samplingFactor - 1);
        float D = 0;

        for (int i = 1; i < m_cloudPoints.Count; i++)
        {
            float pDistance = MathUtils.EuclideanDistance(m_cloudPoints[i - 1], m_cloudPoints[i]);
            if (D + pDistance >= I)
            {
                Vector2 currentPoint = m_cloudPoints[i - 1];
                while (D + pDistance >= I)
                {
                    float t = Mathf.Min(Mathf.Max((I - D) / pDistance, 0f), 1.0f);
                    if (float.IsNaN(t))
                    {
                        t = 0.5f;
                    }
                    newPoints[numPoints++] = new Vector2((1.0f - t) * currentPoint.x + t * m_cloudPoints[i].x,
                        (1.0f - t) * currentPoint.y + t * m_cloudPoints[i].y);
                    pDistance = D + pDistance - I;
                    D = 0;
                    currentPoint = newPoints[numPoints - 1];
                }
                D = pDistance;
            }
            else
            {
                D += pDistance;
            }
        }
        if (numPoints == samplingFactor - 1)
        {
            newPoints[numPoints++] = new Vector2(m_cloudPoints[m_cloudPoints.Count - 1].x, m_cloudPoints[m_cloudPoints.Count - 1].y);
        }

        return newPoints;
    }

    public List<float> GetAngles()
    {
        List<float> angles = new List<float>();
        for(int i=1; i<m_cloudPoints.Count; i++)
        {
            float theta = Mathf.Atan2(m_cloudPoints[i].y - m_cloudPoints[i - 1].y, m_cloudPoints[i].x - m_cloudPoints[i - 1].x) / Mathf.PI;
            angles.Add(theta);
        }
        return angles;
    }

    private float PointsLenght()
    {
        float totalLenght = 0;
        for (int i = 1; i < m_cloudPoints.Count; i++)
        {
            totalLenght += MathUtils.EuclideanDistance(m_cloudPoints[i - 1], m_cloudPoints[i]);
        }
        return totalLenght;
    }

    public void Simplify()
    {
        Simplify(m_samplingFactor);
    }

    public void Simplify(int newFactorSampling)
    {
        m_samplingFactor = newFactorSampling;

        int tolerance = 6;
        float leastDistance = float.MaxValue;
        for(int i=1; i<m_cloudPoints.Count; i++)
        {
            float distance = MathUtils.SqrEuclideanDistance(m_cloudPoints[i], m_cloudPoints[i - 1]);
            if(distance < leastDistance && distance > tolerance)
            {
                leastDistance = distance;
            }
        }

        while(true)
        {
            int count = 0;
            for(int i=1; i<m_cloudPoints.Count; i++)
            {
                float distance = MathUtils.SqrEuclideanDistance(m_cloudPoints[i], m_cloudPoints[i - 1]);
                if(distance > leastDistance)
                {
                    m_cloudPoints.Insert(i, new Vector2((m_cloudPoints[i].x + m_cloudPoints[i - 1].x) * 0.5f, (m_cloudPoints[i].y + m_cloudPoints[i - 1].y) * 0.5f));
                    count++;
                }
            }
            if(count == 0)
            {
                break;
            }
        }

        List<Vector2> newPoints = new List<Vector2>();
        while (m_cloudPoints.Count < m_samplingFactor)
        {
            newPoints.Clear();
            newPoints.Add(m_cloudPoints[0]);
            for(int i=1; i<m_cloudPoints.Count; i++)
            {
                newPoints.Add(new Vector2((m_cloudPoints[i].x + m_cloudPoints[i - 1].x) * 0.5f, (m_cloudPoints[i].y + m_cloudPoints[i - 1].y) * 0.5f));
                newPoints.Add(m_cloudPoints[i]);
            }
            m_cloudPoints = newPoints;
        }

        if(m_cloudPoints.Count > m_samplingFactor)
        {
            float step = m_cloudPoints.Count / m_samplingFactor;
            float j = 0;
            List<Vector2> newPoints2 = new List<Vector2>(m_samplingFactor);
            for(int i=0; i<m_samplingFactor; i++)
            {
                newPoints2.Add(m_cloudPoints[Mathf.FloorToInt(j)]);
                j += step;
            }
            m_cloudPoints = newPoints2;
        }
    }

    public static PRPatternDefinition Deserialize(Dictionary<string,object> dict)
    {
        string m_patternName = dict["m_patternName"] as string;
        int samplingFactor = int.Parse(dict["m_samplingFactor"].ToString());
        List<object> point = ((List<object>)dict["m_cloudPoints"]);
        List<Vector2> m_cloudPoints = new List<Vector2>(point.Count);
        for(int i=0; i<point.Count;i++)
        {
            m_cloudPoints.Add(ParseVector2((string)point[i]));
        }
        return new PRPatternDefinition(m_cloudPoints, samplingFactor, m_patternName);
    }

    public static Vector2 ParseVector2(string vectorStr)
    {
        float xValue = float.Parse(vectorStr.Split(',')[0]);
        float yValue = float.Parse(vectorStr.Split(',')[1]);
        return new Vector2(xValue, yValue);
    }

    public Dictionary<string,object> Serialize()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("m_patternName", m_patternName);
        dict.Add("m_cloudPoints", TransformVectors(m_cloudPoints));
        dict.Add("m_samplingFactor", m_samplingFactor.ToString());
        return dict;
    }

    private List<Vector2i> TransformVectors(List<Vector2> points)
    {
        List<Vector2i> newPoints = new List<Vector2i>();
        foreach(Vector2 point in points)
        {
            newPoints.Add(new Vector2i(point.x, point.y));
        }
        return newPoints;
    }

    /// <summary>
    /// Auxiliar class to help the serialization of a Vector2
    /// </summary>
    public class Vector2i
    {
        [SerializeField]
        private float x;
        public float xValue
        {
            get { return x; }
        }

        [SerializeField]
        private float y;
        public float yValue
        {
            get { return y; }
        }

        public Vector2i(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return x.ToString() + "," + y.ToString();
        }
    }
}


