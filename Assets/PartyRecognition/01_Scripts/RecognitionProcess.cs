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

public class RecognitionProcess
{

    private PRPatternDefinition m_pattern1;

    private PRPatternDefinition m_pattern2;

    private IRecognitionHeuristicStrategy m_heuristic;
    public IRecognitionHeuristicStrategy HeuristicStrategy
    {
        get { return m_heuristic; }
    }

    private RecognitionResult m_lastRecognitionResult;

    private float m_successThreshold;

    public RecognitionProcess(PRPatternDefinition pattern1, PRPatternDefinition pattern2, IRecognitionHeuristicStrategy heuristicStrategy, float thresholdSuccess = 0.95f)
    {
        m_pattern1 = pattern1;
        m_pattern2 = pattern2;

        m_heuristic = heuristicStrategy;
        m_successThreshold = thresholdSuccess;
        m_lastRecognitionResult = null;
    }

    public void SetHeuristicStrategy(IRecognitionHeuristicStrategy newStrategy)
    {
        if(newStrategy == null)
        {
            Debug.LogWarning("Trying to set a Null Heuristic Strategy");
            return;
        }

        m_heuristic = newStrategy;
    }


    public RecognitionResult Recognize()
    {
        float score = m_heuristic.ProcessHeuristic(m_pattern1, m_pattern2);
        float scorePercent = Mathf.Max((score - 2.0f) / -2.0f, 0.0f);
        if(scorePercent >= m_successThreshold)
        {
            //
        }
        m_lastRecognitionResult = new RecognitionResult(scorePercent >= m_successThreshold, score, m_pattern2.PatternName);

        return m_lastRecognitionResult;
    }
    

    /// <summary>
    /// TODO
    /// </summary>
    /// <returns></returns>
    public IEnumerator RecognizeAsync()
    {
        yield break;
    }
	
}
