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
    private string m_pathToNeuronalNetworkFile;
    public string PathToNeuronalNetworkFile
    {
        get { return m_pathToNeuronalNetworkFile; }
    }

    [SerializeField]
    private TextAsset m_textAsset;
    public TextAsset TextAsset
    {
        get { return m_textAsset; }
    }

    [SerializeField]
    private TextAsset m_neuronalNetworkTextAsset;
    public TextAsset NeuronalNetworkTextAsset
    {
        get { return m_neuronalNetworkTextAsset; }
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
    public PartyBPN NeuronalNetwork
    {
        get { return m_neuronalNetwork; }
    }

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
        if(!LoadNeuronalNet())
        {
            m_neuronalNetwork = new PartyBPN(m_defaultNeuronNumberInput - 1, Mathf.RoundToInt((m_defaultNeuronNumberInput + 5) / 2), 5, m_defaultLearningRate, m_defaultTheta, m_defaultSigmoidElastic, m_defaultMomentum);
        }
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

    private bool LoadNeuronalNet()
    {
#if UNITY_ANDROID
        string json = m_neuronalNetworkTextAsset.text;
#else
        string json = File.ReadAllText(Application.dataPath + "/" + m_pathToNeuronalNetworkFile);
#endif
        //string json = File.ReadAllText(Application.dataPath+"/"+m_pathToFile);
        m_neuronalNetwork = new PartyBPN();
        if (!string.IsNullOrEmpty(json))
        {
            Dictionary<string, object> data = (Dictionary<string, object>)MiniJSON.Json.Deserialize(json);
            m_neuronalNetwork.Deserialize(data);
            return true;
        }else
        {
            return false;
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
