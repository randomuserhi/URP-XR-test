using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshGeneration))]
public class MeshGenerationEditor : Editor
{
    MeshGeneration generator;

    private void OnEnable()
    {
        generator = (MeshGeneration)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Mesh")) generator.GenerateMesh(generator.surfaces);
    }
}