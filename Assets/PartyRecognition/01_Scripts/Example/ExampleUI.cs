using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleUI : MonoBehaviour
{
    [SerializeField]
    private Text m_modeText;

    [SerializeField]
    private Dropdown m_patternDropdown;

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

    private void UpdatePatternList()
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


}
