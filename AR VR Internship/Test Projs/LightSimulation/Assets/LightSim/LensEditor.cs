/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(Lens))]
public class LensEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //DrawPropertiesExcluding(serializedObject, "refractiveIndex", "m_Script");

        Lens lens = target as Lens;

        lens.type = (Lens.Type)EditorGUILayout.EnumPopup("Type", lens.type);

        if (lens.type != Lens.Type.Surface && lens.type != Lens.Type.CustomSurface)
        {
            SerializedProperty f = serializedObject.FindProperty("front");
            EditorGUILayout.PropertyField(f, new GUIContent("Front"));
            SerializedProperty b = serializedObject.FindProperty("back");
            EditorGUILayout.PropertyField(b, new GUIContent("Back"));
        }
        if (lens.type == Lens.Type.ConcaveLens)
        {
            lens.bound = EditorGUILayout.FloatField("Bound", lens.bound);
        }

        GUILayout.Space(10);

        lens.refractiveIndex.isFixed = EditorGUILayout.Toggle("Is Fixed", lens.refractiveIndex.isFixed);
        if (!lens.refractiveIndex.isFixed)
        {
            lens.refractiveIndex.m = EditorGUILayout.FloatField("M", lens.refractiveIndex.m);
            lens.refractiveIndex.c = EditorGUILayout.FloatField("C", lens.refractiveIndex.c);
        }
        else
        {
            lens.refractiveIndex.refractionIndex = EditorGUILayout.FloatField("Refractive Index", lens.refractiveIndex.refractionIndex);
        }

        //lens.type = EditorGUILayout.DropdownButton(lens.type, FocusType.Passive)
    }
}*/
