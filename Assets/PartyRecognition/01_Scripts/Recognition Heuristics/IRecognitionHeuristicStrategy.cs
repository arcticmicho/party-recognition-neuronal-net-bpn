using UnityEngine;
using System.Collections;

public interface IRecognitionHeuristicStrategy
{
    float ProcessHeuristic(PRPatternDefinition pattern1, PRPatternDefinition pattern2);
}
