﻿using UnityEngine;
using UnityEditor;
using System.Collections;
using PartyManagerUtils;

[CustomEditor(typeof(InputManager))]
public class InputManagerEditor : Editor
{
   // InputManager _touchManager;

	void OnEnable ()
    {
       // _touchManager = target as InputManager;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
