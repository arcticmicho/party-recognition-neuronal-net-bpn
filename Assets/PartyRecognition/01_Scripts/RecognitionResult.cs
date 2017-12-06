using UnityEngine;
using System.Collections;

public class RecognitionResult
{
    private bool m_success;
    public bool Success
    {
        get { return m_success;
        }
    }

    private float m_recognitionScore;
    public float RecognitionScore
    {
        get { return m_recognitionScore; }
    }

    private string m_patterName;
    public string PatternName
    {
        get { return m_patterName; }
        set { m_patterName = value; }
    }

    public float RecognitionScoreAsPercent
    {
        get
        {
            return Mathf.Max((m_recognitionScore - 2.0f) / -2.0f, 0.0f);
        }
    }

    public RecognitionResult(bool success, float score, string patternName = "")
    {
        m_success = success;
        m_recognitionScore = score;
        m_patterName = patternName;
    }
}
