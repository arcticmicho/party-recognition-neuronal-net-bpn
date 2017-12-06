using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

[CustomEditor(typeof(PartyRecognitionManager))]
public class PartyRecognitionManagerEditor : Editor
{
    PartyRecognitionManager m_instance;
    private int m_selectedPatternIndex = 0;
    private int m_numberOfEpochs = 2000;

    private IEnumerator m_trainingProcess;

    public override void OnInspectorGUI()
    {
        if (m_instance == null)
            m_instance = target as PartyRecognitionManager;
        base.OnInspectorGUI();

        if(GUILayout.Button("Write Patterns"))
        {
            ReadSprites();
        }
        if(GUILayout.Button("Write Current Patterns To JSON File"))
        {
            WriteCurrentPatternsToJsonFile();
        }
        int numberOfEpochs = EditorGUILayout.IntField(m_numberOfEpochs);
        if(numberOfEpochs != m_numberOfEpochs)
        {
            m_numberOfEpochs = numberOfEpochs;
        }
        if(GUILayout.Button("Train NeuronalNet"))
        {           
            EditorApplication.update += Update;
        }
        if(GUILayout.Button("Write NeuronalNet"))
        {            
            if(m_instance.NeuronalNetwork != null)
            {
                WriteCurrentNeuronalNetwork();
            }else
            {
                Debug.LogError("There is not NeuronalNetwork initialized");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.Label("Selected Patterns:");

        for(int i=0, count=m_instance.SelectedPatterns.Count; i<count;i++)
        {
            GUILayout.Label(m_instance.SelectedPatterns[i].PatternName);            
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        int selectedPatternIndex = EditorGUILayout.Popup(m_selectedPatternIndex, CurrentPatternsList());
        if(selectedPatternIndex != m_selectedPatternIndex)
        {
            m_selectedPatternIndex = selectedPatternIndex;
        }
        if(GUILayout.Button("Add Pattern To Selected List"))
        {
            m_instance.AddSelectedPattern(m_instance.PatternDefinitionSet[m_selectedPatternIndex]);
        }
    }

    private void Update()
    {
        try
        {
            if(m_trainingProcess == null)
            {
                m_trainingProcess = m_instance.StartBPNTraining(m_instance.SelectedPatterns, m_numberOfEpochs);
            }

            if (!m_trainingProcess.MoveNext())
            {
                m_trainingProcess = null;
                EditorApplication.update -= Update;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
            EditorApplication.update -= Update;            
        }
    }

    private string[] CurrentPatternsList()
    {
        string[] names = new string[m_instance.PatternDefinitionSet.Count];
        for(int i=0,count= m_instance.PatternDefinitionSet.Count; i<count;i++)
        {
            names[i] = m_instance.PatternDefinitionSet[i].PatternName;
        }
        return names;
    }

    private void ReadSprites()
    {
        StreamWriter stream = File.CreateText(AssetDatabase.GetAssetPath(m_instance.TextAsset));
        List<Dictionary<string, object>> definitions = new List<Dictionary<string, object>>();
        for (int i=0; i<m_instance.PatternTextures.Length; i++)
        {
            PRPatternDefinition def = GeneratePatternFromSprite(m_instance.PatternTextures[i]);
            definitions.Add(def.Serialize());
        }
        stream.WriteLine(MiniJSON.Json.Serialize(definitions));
        stream.Close();
    }

    private void WriteCurrentNeuronalNetwork()
    {
        StreamWriter stream = File.CreateText(AssetDatabase.GetAssetPath(m_instance.NeuronalNetworkTextAsset));
        Dictionary<string, object> neuronalNetworkData = m_instance.NeuronalNetwork.Serialize();
        stream.WriteLine(MiniJSON.Json.Serialize(neuronalNetworkData));
        stream.Close();
    }

    private void WriteCurrentPatternsToJsonFile()
    {
        StreamWriter stream = File.CreateText(AssetDatabase.GetAssetPath(m_instance.TextAsset));
        List<Dictionary<string, object>> definitions = new List<Dictionary<string, object>>();
        foreach(PRPatternDefinition pattern in m_instance.PatternDefinitionSet)
        {
            definitions.Add(pattern.Serialize());
        }
        stream.WriteLine(MiniJSON.Json.Serialize(definitions));
        stream.Close();
    }

    private PRPatternDefinition GeneratePatternFromSprite(Texture2D tex)
    {
        float halfWidth = tex.width / 2f;
        float offSetWidth = 2f;
        float halftHeight = tex.height / 2f;
        float offSetHeight = 2f;
        List<Vector2> points = new List<Vector2>();
        for(int i = 0; i<tex.width; i++)
        {
            for(int n=0; n<tex.height; n++)
            {
                Color pixelColor = tex.GetPixel(i, n);
                float colorValue = (pixelColor.r + pixelColor.g + pixelColor.b) / 3f;
                if(colorValue <= 0.5f)
                {
                    points.Add(new Vector2((i - halfWidth) * offSetWidth, (n - halftHeight) * offSetHeight));
                }
            }
        }
        List<Vector2> arrayPoints = OrderPoints(points);
        PRPatternDefinition def = new PRPatternDefinition(arrayPoints, (int)PartyRecognitionManager.Instance.DefaultNeuronNumberInput, tex.name);
        def.NormalizePoints();
        return def;
    }

    public List<Vector2> OrderPoints(List<Vector2> points)
    {
        List<Vector2> pointsSet = new List<Vector2>(points);
        List<Vector2> orderedPoints = new List<Vector2>();

        Vector2 currentPoint = pointsSet[0];
        pointsSet.Remove(currentPoint);
        orderedPoints.Add(currentPoint);
        while(pointsSet.Count > 0)
        {
            Vector2 nearestPoint = new Vector2(float.MaxValue, float.MaxValue);
            foreach(Vector2 point in pointsSet)
            {
                if(MathUtils.EuclideanDistance(point, currentPoint) < MathUtils.EuclideanDistance(nearestPoint, currentPoint))
                {
                    nearestPoint = point;
                }
            }
            orderedPoints.Add(nearestPoint);
            currentPoint = nearestPoint;
            pointsSet.Remove(nearestPoint);
        }
        return orderedPoints;
    }
}
