using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using LightTK;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(LTKCollider))]
public class LTKColliderEditor : Editor
{
    //float focalLength;

    LTKCollider c;
    SerializedProperty surfaceProperty;

    private void OnEnable()
    {
        c = (LTKCollider)target;
        ref SurfaceSettings settings = ref c._surface.settings;
        surfaceProperty = serializedObject.FindProperty("surface");
    }

    public override void OnInspectorGUI()
    {
        ref SurfaceSettings settings = ref c._surface.settings;

        EditorGUILayout.PropertyField(surfaceProperty);

        settings.type = (SurfaceSettings.SurfaceType)EditorGUILayout.EnumPopup("Surface Type", settings.type);
        switch(settings.type)
        {
            case SurfaceSettings.SurfaceType.IdealLens:

                //focalLength = EditorGUILayout.FloatField("Focal Length", focalLength);
                //settings.setFocalLength(focalLength);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Radii of Curvature", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical("box");
                settings.R1 = EditorGUILayout.FloatField("Radius 1", settings.R1);
                settings.R2 = EditorGUILayout.FloatField("Radius 2", settings.R2);
                EditorGUILayout.EndVertical();

                //focalLength = settings.focalLength(new LightRay());
                EditorGUILayout.Space();
                DrawRefractionEquation("Refraction Settings", ref settings.refractionSettings.single);
                //focalLength = settings.focalLength();
                break;
            case SurfaceSettings.SurfaceType.Refractive:
                DrawRefractionSettings(ref settings.refractionSettings);
                break;
        }
        serializedObject.ApplyModifiedProperties();
        //base.OnInspectorGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(c);
            EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
        }
    }

    private void DrawRefractionEquation(string label, ref RefractionEquation rEquation)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        rEquation.isFixed = EditorGUILayout.Toggle("Is Fixed", rEquation.isFixed);
        if (!rEquation.isFixed)
        {
            rEquation.m = EditorGUILayout.FloatField("Gradient", rEquation.m);
            rEquation.c = EditorGUILayout.FloatField("Intercept", rEquation.c);
        }
        else
        {
            rEquation.refractionIndex = EditorGUILayout.FloatField("Refractive Index", rEquation.refractionIndex);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawRefractionSettings(ref RefractionSettings rSettings)
    {
        ref RefractionEquation positive = ref rSettings.positive;
        ref RefractionEquation negative = ref rSettings.negative;

        rSettings.type = (RefractionSettings.Type)EditorGUILayout.EnumPopup("Type", rSettings.type);
        EditorGUILayout.Space();

        switch(rSettings.type)
        {
            case RefractionSettings.Type.Single:
                DrawRefractionEquation("Single Face", ref rSettings.single);
                break;
            case RefractionSettings.Type.Edge:
                DrawRefractionEquation("Front Face", ref positive);
                EditorGUILayout.Space();
                DrawRefractionEquation("Back Face", ref negative);
                break;
        }
    }
}