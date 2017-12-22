using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ExampleUI : MonoBehaviour
{
    [SerializeField]
    private Text m_modeText;

    [SerializeField]
    private Dropdown m_patternDropdown;

    [SerializeField]
    private InputField m_patternName;

    [SerializeField]
    private Example m_example;

    public void Init()
    {
        UpdateModeText();
        UpdatePatternList();
    }

    private void UpdateModeText()
    {
        m_modeText.text = m_example.Mode.ToString();
    }

    public string GetPatternName()
    {
        return m_patternName.text;
    }

    public void ChangeMode()
    {
        if(m_example.Mode == Example.ExampleMode.Recognize)
        {
            m_example.Mode = Example.ExampleMode.SavePattern;
        }else
        {
            m_example.Mode = Example.ExampleMode.Recognize;
        }
        UpdateModeText();
    }

    public void UpdatePatternList()
    {
        List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();

        foreach(var pattern in PartyRecognitionManager.Instance.PatternDefinitionSet)
        {
            data.Add(new Dropdown.OptionData(pattern.PatternName));
        }

        m_patternDropdown.options = data;
        m_patternDropdown.value = 0;
    }

    public void DrawPattern()
    {
        m_example.DrawPattern(m_patternDropdown.options[m_patternDropdown.value].text);
    }

    public void PrintAngles()
    {
        StringBuilder stringB = new StringBuilder();
        PRPatternDefinition def;
        if(PartyRecognitionManager.Instance.TryGetPatternById(m_patternDropdown.options[m_patternDropdown.value].text, out def))
        {
            foreach(float angle in def.GetAngles())
            {
                stringB.Append(" | " + angle);
            }
        }
        Debug.Log(stringB.ToString());
    }

    public void CleanPatterns()
    {
        PartyRecognitionManager.Instance.ClearPatterns();
        UpdatePatternList();
    }

    public void WritePatternsToFile()
    {
        StreamWriter stream = File.CreateText(AssetDatabase.GetAssetPath(PartyRecognitionManager.Instance.TextAsset));
        List<Dictionary<string, object>> definitions = new List<Dictionary<string, object>>();
        foreach (PRPatternDefinition pattern in PartyRecognitionManager.Instance.PatternDefinitionSet)
        {
            definitions.Add(pattern.Serialize());
        }
        stream.WriteLine(MiniJSON.Json.Serialize(definitions));
        stream.Close();
    }


}
