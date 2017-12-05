using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PartyRecognitionManager : MonoSingleton<PartyRecognitionManager>
{
    private List<PRPatternDefinition> m_patternDefinitionSet = new List<PRPatternDefinition>();
    public List<PRPatternDefinition> PatternDefinitionSet
    {
        get { return m_patternDefinitionSet; }
    }

    private Dictionary<string, PRPatternDefinition> m_patternsDefinitionsDict;

    [SerializeField]
    private Texture2D[] m_patternTextures;
    public Texture2D[] PatternTextures
    {
        get { return m_patternTextures; }
    }

    [SerializeField]
    private string m_pathToFile;
    public string PathToFile
    {
        get { return m_pathToFile; }
    }

    [SerializeField]
    private TextAsset m_textAsset;
    public TextAsset TextAsset
    {
        get { return m_textAsset; }
    }

    [SerializeField]
    private float m_defaultMomentum = 0.999f;

    [SerializeField]
    private float m_defaultTheta = 0.5f;

    [SerializeField]
    private float m_defaultSigmoidElastic = 0.5f;

    [SerializeField]
    private float m_defaultLearningRate = 0.0138236387104945f;

    [SerializeField]
    private float m_defaultNeuronNumberInput = 20;
    public float DefaultNeuronNumberInput
    {
        get { return m_defaultNeuronNumberInput; }
    }

    [SerializeField]
    private float m_successThresholdPercent = 0.95f;

    private PartyBPN m_neuronalNetwork;

#if UNITY_EDITOR
    private List<PRPatternDefinition> m_selectedPatterns = new List<PRPatternDefinition>();
    public List<PRPatternDefinition> SelectedPatterns
    {
        get { return m_selectedPatterns; }
    }
#endif

    public int CurrentBPNSamplingFactor
    {
        get { return (int)m_neuronalNetwork.NeuronNumberInput + 1; }
    }

    public override void Init()
    {
        base.Init();
        m_patternDefinitionSet = new List<PRPatternDefinition>();
        m_patternsDefinitionsDict = new Dictionary<string, PRPatternDefinition>();
        LoadDefinitionSet();

        m_neuronalNetwork = new PartyBPN(m_defaultNeuronNumberInput - 1, Mathf.RoundToInt((m_defaultNeuronNumberInput + 5)/2), 5, m_defaultLearningRate, m_defaultTheta, m_defaultSigmoidElastic, m_defaultMomentum);
    }

    private void LoadDefinitionSet()
    {
#if UNITY_ANDROID
        string json = m_textAsset.text;
#else
        string json = File.ReadAllText(Application.dataPath+"/"+m_pathToFile);
#endif
        //string json = File.ReadAllText(Application.dataPath+"/"+m_pathToFile);
        if(!string.IsNullOrEmpty(json))
        {
            List<object> definitions = (List<object>)MiniJSON.Json.Deserialize(json);
            foreach (Dictionary<string, object> pattern in definitions)
            {
                PRPatternDefinition newPattern = PRPatternDefinition.Deserialize(pattern);
                m_patternDefinitionSet.Add(newPattern);
                m_patternsDefinitionsDict.Add(newPattern.PatternName, newPattern);
            }
        }        
    }

    /// <summary>
    /// Add the pattern to the list of pattern so it could be Serialized for future purposes.
    /// </summary>
    /// <param name="newDef"></param>
    public void AddPattern(PRPatternDefinition newDef)
    {
        m_patternDefinitionSet.Add(newDef);
        m_patternsDefinitionsDict.Add(newDef.PatternName, newDef);
    }

    /// <summary>
    /// Try to train the current BPN with a list of patterns;
    /// </summary>
    /// <param name="points"></param>
    /// <param name="name"></param>
    public IEnumerator StartBPNTraining(List<PRPatternDefinition> patternDefinitions, int numberOfEpochs)
    {
        m_neuronalNetwork = new PartyBPN(m_defaultNeuronNumberInput -1, Mathf.RoundToInt((patternDefinitions.Count+m_defaultNeuronNumberInput)/2), patternDefinitions.Count, m_defaultLearningRate, m_defaultTheta, m_defaultSigmoidElastic, m_defaultMomentum);
        return m_neuronalNetwork.StartTraining(patternDefinitions, numberOfEpochs);
    }

    public bool TryGetPatternById(string patternId, out PRPatternDefinition pDef)
    {
        return m_patternsDefinitionsDict.TryGetValue(patternId, out pDef);
    }

    /// <summary>
    /// Initalize a Recognition process using the Greedy5 Strategy as default Heuristic Strategy
    /// </summary>
    /// <param name="points"></param>
    public RecognitionResult Recognize(Vector2[] points)
    {
        //return Recognize(points, new Greedy5RecognitionStrategy(0.5f, true));
        PRPatternDefinition pattern = new PRPatternDefinition(new List<Vector2>(points), (int)DefaultNeuronNumberInput);
        pattern.NormalizePoints();
        return m_neuronalNetwork.Propagate(pattern.GetAngles(), 0.8f);
    }

    public RecognitionResult Recognize(Vector2[] points, IRecognitionHeuristicStrategy strategy)
    {
        return null;
        //Vector2[] normalizedPoints = NormalizePoints(points, m_cloundPointsSampling);
        //
        //PRPatternDefinition pattern = new PRPatternDefinition(new List<Vector2>(normalizedPoints), (int)m_neuronalNetwork.NeuronNumberInput);
        //RecognitionResult result = new RecognitionResult(false,float.MaxValue);
        //for(int i=0; i< m_patternDefinitionSet.Count; i++)
        //{
        //    RecognitionResult newResult = SimpleRecognize(pattern, m_patternDefinitionSet[i], strategy);
        //    if (newResult.Success && result.RecognitionScore > newResult.RecognitionScore)
        //    {
        //        result = newResult;
        //    }
        //}
        //return result;
    }


    public RecognitionResult SimpleRecognize(PRPatternDefinition pattern1, PRPatternDefinition pattern2, IRecognitionHeuristicStrategy strategy)
    {
        RecognitionProcess process = new RecognitionProcess(pattern1, pattern2, strategy, m_successThresholdPercent);
        return process.Recognize();
    }

    public RecognitionResult SimpleRecognize(Vector2[] points, PRPatternDefinition pattern2)
    {
        return SimpleRecognize(points, pattern2, new Greedy5RecognitionStrategy(0.5f, true));
    }

    public RecognitionResult SimpleRecognize(Vector2[] points, PRPatternDefinition pattern2, IRecognitionHeuristicStrategy strategy)
    {
        //Vector2[] normalizedPoints = NormalizePoints(points, m_cloundPointsSampling);
        //PRPatternDefinition pattern = new PRPatternDefinition(new List<Vector2>(normalizedPoints), (int)m_neuronalNetwork.NeuronNumberInput);
        //return SimpleRecognize(pattern, pattern2, strategy);
        return null;
    }

    //public Vector2[] NormalizePoints(Vector2[] points)
    //{
    //    return NormalizePoints(points, m_cloundPointsSampling);
    //}
    //
    //private Vector2[] NormalizePoints(Vector2[] points, int samplingFactor)
    //{
    //    Vector2[] normalizedPoints = new Vector2[points.Length];
    //    normalizedPoints = ScalePoints(points);
    //    normalizedPoints = TranslatePointsByPoint(normalizedPoints, CalculateCentroid(normalizedPoints));
    //    normalizedPoints = ResamplePoints(normalizedPoints, m_cloundPointsSampling);
    //    return normalizedPoints;
    //}
    //
    //private Vector2[] ScalePoints(Vector2[] points)
    //{
    //    float minX = float.MaxValue;
    //    float minY = float.MaxValue;
    //    float maxX = float.MinValue;
    //    float maxY = float.MinValue;
    //
    //    for (int i = 0; i < points.Length; i++)
    //    {
    //        if (points[i].x > maxX)
    //        {
    //            maxX = points[i].x;
    //        }
    //        if (points[i].x < minX)
    //        {
    //            minX = points[i].x;
    //        }
    //        if (points[i].y > maxY)
    //        {
    //            maxY = points[i].y;
    //        }
    //        if (points[i].y < minY)
    //        {
    //            minY = points[i].y;
    //        }
    //    }
    //
    //    Vector2[] newPoints = new Vector2[points.Length];
    //    float scale = Mathf.Max(maxX - minY, maxY - minX);
    //    for (int i = 0; i < points.Length; i++)
    //    {
    //        newPoints[i] = new Vector2((points[i].x - minX) / scale, (points[i].y - minY) / scale);
    //    }
    //    return newPoints;
    //}
    //
    //private Vector2[] TranslatePointsByPoint(Vector2[] points, Vector2 point)
    //{
    //    Vector2[] newPoints = new Vector2[points.Length];
    //    for(int i=0; i<points.Length; i++)
    //    {
    //        newPoints[i] = new Vector2(points[i].x - point.x, points[i].y - point.y);
    //    }
    //    return newPoints;
    //}
    //
    //private Vector2 CalculateCentroid(Vector2[] points)
    //{
    //    float cx = 0;
    //    float cy = 0;
    //
    //    for(int i=0; i<points.Length; i++)
    //    {
    //        cx += points[i].x;
    //        cy += points[i].y;
    //    }
    //    return new Vector2(cx / (float)points.Length, cy / (float)points.Length);
    //}
    //
    //private Vector2[] ResamplePoints(Vector2[] points, int samplingFactor)
    //{
    //    if(points.Length == samplingFactor)
    //    {
    //        return points;
    //    }
    //
    //    Vector2[] newPoints = new Vector2[samplingFactor];
    //    newPoints[0] = new Vector2(points[0].x, points[0].y);
    //    int numPoints = 1;
    //    float I = (float)PointsLenght(points) / (float)(samplingFactor - 1);
    //    float D = 0;
    //
    //    for(int i=1; i < points.Length; i++)
    //    {
    //        float pDistance = MathUtils.EuclideanDistance(points[i - 1], points[i]);
    //        if(D + pDistance >= I)
    //        {
    //            Vector2 currentPoint = points[i - 1];
    //            while(D + pDistance >= I)
    //            {
    //                float t = Mathf.Min(Mathf.Max((I - D) / pDistance, 0f), 1.0f);
    //                if(float.IsNaN(t))
    //                {
    //                    t = 0.5f;
    //                }
    //                newPoints[numPoints++] = new Vector2((1.0f - t) * currentPoint.x + t * points[i].x,
    //                    (1.0f - t) * currentPoint.y + t * points[i].y);
    //                pDistance = D + pDistance - I;
    //                D = 0;
    //                currentPoint = newPoints[numPoints - 1];
    //            }
    //            D = pDistance;
    //        }else
    //        {
    //            D += pDistance;
    //        }            
    //    }
    //    if (numPoints == samplingFactor - 1)
    //    {
    //        newPoints[numPoints++] = new Vector2(points[points.Length - 1].x, points[points.Length - 1].y);
    //    }
    //
    //    return newPoints;
    //}
    //
    //private float PointsLenght(Vector2[] points)
    //{
    //    float totalLenght = 0;
    //    for(int i=1; i<points.Length; i++)
    //    {
    //        totalLenght += MathUtils.EuclideanDistance(points[i - 1], points[i]);
    //    }
    //    return totalLenght;
    //}

#if UNITY_EDITOR
    public void AddSelectedPattern(PRPatternDefinition selectedDef)
    {
        if(!m_selectedPatterns.Contains(selectedDef))
        {
            m_selectedPatterns.Add(selectedDef);
        }
    }

    public void ClearSelectedPatterns()
    {
        m_selectedPatterns.Clear();
    }
#endif

}
