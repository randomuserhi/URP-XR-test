using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CustomEditor(typeof(LTKRenderer))]
public class LTKRendererEditor : Editor
{
    LTKRenderer generator;

    private void OnEnable()
    {
        generator = (LTKRenderer)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Mesh")) generator.GenerateMesh(generator.surfaces);
    }
}

#endif