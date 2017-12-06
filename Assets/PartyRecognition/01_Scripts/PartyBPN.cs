using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Back Propagation Neural Net
/// </summary>
public class PartyBPN
{
    private float m_instantError;
    private float m_totalError;
    private float m_absoluteError;

    private List<float> m_inpA;
    private List<float> m_hidA;
    private List<float> m_hidN;
    private List<List<float>> m_hidW;
    private List<float> m_outA;
    private List<float> m_outN;
    private List<float> m_outD;
    private List<List<float>> m_outW;

    private float m_neuronsNumberInp;
    private float m_neuronsNumberHid;
    private float m_neuronsNumberOut;

    private float m_learningRate;
    private float m_thetaThreshold;
    private float m_elasticSigmoid;
    private float m_momentum;

    private List<string> m_patternId = new List<string>();

    public float NeuronNumberInput
    {
        get { return m_neuronsNumberInp; }
    }

    public PartyBPN()
    {

    }

    public PartyBPN(float neurNumberInput, float neuronNumberHidden, float neuronNumberOut, float eida, float theta, float elast, float momentum)
    {
        m_neuronsNumberInp = neurNumberInput;
        m_neuronsNumberHid = neuronNumberHidden;
        m_neuronsNumberOut = neuronNumberOut;

        m_learningRate = eida;
        m_thetaThreshold = theta;
        m_elasticSigmoid = elast;
        m_momentum = momentum;

        Initialize();
    }    

    public void Initialize()
    {
        m_inpA = new List<float>();
        for (int i = 0; i < m_neuronsNumberInp; i++)
        {
            m_inpA.Add(MathUtils.FixedRandomNumber(-1.0f, 1.0f));
        }

        m_hidA = new List<float>((int)m_neuronsNumberHid);
        m_hidW = new List<List<float>>((int)m_neuronsNumberHid);
        for (int i = 0; i < m_neuronsNumberHid; i++)
        {
            m_hidA.Add(MathUtils.FixedRandomNumber(-1.0f, 1.0f));
            m_hidW.Add(new List<float>());
            for (int n = 0; n < m_neuronsNumberInp; n++)
            {
                m_hidW[i].Add(MathUtils.FixedRandomNumber(-1.0f, 1.0f));
            }
        }

        m_outW = new List<List<float>>((int)m_neuronsNumberOut);
        for (int i = 0; i < m_neuronsNumberOut; i++)
        {
            m_outW.Add(new List<float>());
            for (int n = 0; n < m_neuronsNumberHid; n++)
            {
                m_outW[i].Add(MathUtils.FixedRandomNumber(-1.0f, 1.0f));
            }
        }

        m_hidN = new List<float>();
        m_hidA = new List<float>();
        for (int i=0; i< m_neuronsNumberHid;i++)
        {
            m_hidN.Add(0f);
            m_hidA.Add(0f);
        }
        m_outN = new List<float>();
        m_outD = new List<float>();
        m_outA = new List<float>();
        for (int i=0; i<m_neuronsNumberOut; i++)
        {
            m_outN.Add(0f);
            m_outD.Add(0f);
            m_outA.Add(0f);
        }
        m_totalError = 0f;
        m_absoluteError = 0f;
    }

    public IEnumerator StartTraining(List<PRPatternDefinition> patternDefinitions, int epochs)
    {
        m_patternId.Clear();
        for (int i=0, count=patternDefinitions.Count; i<count; i++)
        {
            m_patternId.Add(patternDefinitions[i].PatternName);
        }

        float[][] outInputParameters = MathUtils.IdentityMatrixDotValue(patternDefinitions.Count);
        string[] outputValues = new string[patternDefinitions.Count];
        for (int n = 0, count2 = epochs; n < count2; n++)
        {
            List<int> randomIndex = MathUtils.FastRandomNumberList(patternDefinitions.Count);
            for (int i = 0, count = patternDefinitions.Count; i < count; i++)
            {
                //We will suppose that all the pattern are Scaled, Translated and Simplified.
                LearnPattern(patternDefinitions[randomIndex[i]], new List<float>(outInputParameters[randomIndex[i]]));
                outputValues[randomIndex[i]] = "--" + randomIndex[i] + ": " + m_outD[randomIndex[i]];
            }
            //ShowOutputValues();
            string values = string.Empty;
            for(int i=0; i<outputValues.Length; i++)
            {
                values += outputValues[i];
            }
            Debug.Log(values);
            yield return null;
        }
    }

    private void LearnPattern(List<float> inValues, List<float> outValues)
    {
        if (inValues.Count != m_neuronsNumberInp || outValues.Count != m_neuronsNumberOut)
        {
            Debug.LogError("Wrong dimension for the Input values of the pattern");
        }

        for(int i=0; i<m_neuronsNumberInp; i++)
        {
            m_inpA[i] = inValues[i];
        }

        for (int i=0; i<m_neuronsNumberOut; i++)
        {
            m_outA[i] = outValues[i];
        }

        PushNewData();

        m_totalError = 0f;
        m_absoluteError = 0f;

        for(int j=0; j<m_neuronsNumberOut; j++)
        {
            m_instantError = ComputeDelta(j);
            m_totalError += m_instantError;
            m_absoluteError += Mathf.Abs(m_totalError);
        }

        UpdateWeights();
        m_learningRate *= m_momentum;
    }

    private void UpdateWeights()
    {
        float sum2 = 0f;
        for(int i=0; i<m_neuronsNumberHid; i++)
        {
            for(int n=0; n<m_neuronsNumberOut; n++)
            {
                sum2 += m_outD[n] * m_outW[n][i];
            }
            sum2 *= DlSigmoid(m_hidN[i]);
            for(int n=0; n<m_neuronsNumberInp;n++)
            {
                m_hidW[i][n] += m_learningRate * sum2 * m_inpA[n];
            }
        }
    }


    public void LearnPattern(PRPatternDefinition newDef, List<float> outputValues)
    {
        LearnPattern(newDef.GetAngles(), outputValues);
    }

    private float ComputeDelta(int j)
    {
        m_outD[j] = (m_outA[j] - Sigmoid(m_outN[j])) * (DlSigmoid(m_outN[j]) + 0.1f);
        for(int i=0; i<m_neuronsNumberHid; i++)
        {
            m_outW[j][i] += m_outD[j] * m_hidA[i] * m_learningRate;
        }
        return m_outD[j];
    }

    private void PushNewData()
    {
        float sum2 = 0f;

        for(int i=0; i<m_neuronsNumberHid; i++)
        {
            for(int n=0; n<m_neuronsNumberInp; n++)
            {
                sum2 += m_hidW[i][n] * m_inpA[n];
            }
            m_hidN[i] = sum2;
            m_hidA[i] = Sigmoid(sum2);
        }
        for(int i=0; i<m_neuronsNumberOut; i++)
        {
            sum2 = 0f;
            for(int n=0; n<m_neuronsNumberHid; n++)
            {
                sum2 += m_outW[i][n] * m_hidA[n];
            }
            m_outN[i] = sum2;
        }
    }

    public RecognitionResult Propagate(List<float> angles, float successThreshold)
    {
        float sum2 = 0f;
        if(angles.Count != m_neuronsNumberInp)
        {
            throw new Exception("Angles doesn't match with the Neuron Input dimension");
        }
        for(int i=0; i<m_neuronsNumberInp; i++)
        {
            m_inpA[i] = angles[i];
        }
        for (int i = 0; i < m_neuronsNumberHid; i++)
        {
            for(int n=0; n<m_neuronsNumberInp; n++)
            {
                sum2 += m_hidW[i][n] * m_inpA[n];                
            }
            m_hidA[i] = Sigmoid(sum2);
        }

        float bestScore = -1.0f;
        int bestScoreIndex = 0;
        for(int i=0; i<m_neuronsNumberOut; i++)
        {
            sum2 = 0f;
            for(int n=0; n<m_neuronsNumberHid; n++)
            {
                sum2 += m_outW[i][n] * m_hidA[n];                
            }
            m_outA[i] = Sigmoid(sum2);
            if (m_outA[i] > bestScore)
            {
                bestScore = m_outA[i];
                bestScoreIndex = i;
            }
        }
        
        return new RecognitionResult(bestScore >= successThreshold, bestScore, m_patternId[bestScoreIndex]);
    }

    public void ShowOutputValues()
    {
        string values = string.Empty;
        for(int i=0, count=m_outD.Count; i<count; i++)
        {
            values = values + " " + i + ": " + m_outD[i] + "   ";
        }
        Debug.Log(values);
    }

    private float Sigmoid(float value)
    {
        return (1.0f / (1.0f + Mathf.Exp(-1.0f * m_elasticSigmoid * value + m_thetaThreshold)) * 2.0f - 1.0f);
    }

    private float DlSigmoid(float value)
    {
        return 2.0f * Mathf.Exp(-1.0f * m_elasticSigmoid * value - m_thetaThreshold) / (1.0f + Mathf.Exp(-2.0f * m_elasticSigmoid * value - m_thetaThreshold));
    }

    public Dictionary<string,object> Serialize()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("learningRate", m_learningRate);
        dict.Add("elasticSigmoid", m_elasticSigmoid);
        dict.Add("momentum", m_momentum);
        dict.Add("theta", m_thetaThreshold);

        dict.Add("neuronInput", m_neuronsNumberInp);
        dict.Add("neuronHidden", m_neuronsNumberHid);
        dict.Add("neuronOut", m_neuronsNumberOut);

        dict.Add("inpA", m_inpA);
        dict.Add("hidW", m_hidW);
        dict.Add("hidA", m_hidA);
        dict.Add("hidN", m_hidN);
        dict.Add("outW", m_outW);
        dict.Add("outA", m_outA);
        dict.Add("outN", m_outN);
        dict.Add("outD", m_outD);

        dict.Add("patternIDs", m_patternId);

        return dict;
    }

    public void Deserialize(Dictionary<string, object> dict)
    {
        m_learningRate = float.Parse(dict["learningRate"].ToString());
        m_elasticSigmoid = float.Parse(dict["elasticSigmoid"].ToString());
        m_momentum = float.Parse(dict["momentum"].ToString());
        m_thetaThreshold = float.Parse(dict["theta"].ToString());

        m_neuronsNumberInp = float.Parse(dict["neuronInput"].ToString());
        m_neuronsNumberHid = float.Parse(dict["neuronHidden"].ToString());
        m_neuronsNumberOut = float.Parse(dict["neuronOut"].ToString());

        m_inpA = DeserializeList(dict["inpA"] as List<object>);
        m_hidW = DeserializeMatrix(dict["hidW"] as List<object>);
        m_hidA = DeserializeList(dict["hidA"] as List<object>);
        m_hidN = DeserializeList(dict["hidN"] as List<object>);
        m_outW = DeserializeMatrix(dict["outW"] as List<object>);
        m_outA = DeserializeList(dict["outA"] as List<object>);
        m_outN = DeserializeList(dict["outN"] as List<object>);
        m_outD = DeserializeList(dict["outD"] as List<object>);

        m_patternId = new List<string>();
        List<object> patterns = dict["patternIDs"] as List<object>;
        for(int i=0, count=patterns.Count; i<count; i++)
        {
            m_patternId.Add(patterns[i].ToString());            
        }
    }

    public List<float> DeserializeList(List<object> values)
    {
        List<float> floatValues = new List<float>();
        for(int i=0, count=values.Count; i<count; i++)
        {
            floatValues.Add(float.Parse(values[i].ToString()));
        }

        return floatValues;
    }

    public List<List<float>> DeserializeMatrix(List<object> values)
    {
        List<List<float>> floatValues = new List<List<float>>();

        for(int i=0, count1 = values.Count; i<count1; i++)
        {
            List<object> subValues = values[i] as List<object>;
            floatValues.Add(DeserializeList(subValues));
        }

        return floatValues;
    }
}
