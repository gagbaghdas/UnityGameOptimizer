using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProblematicAssetsWindow : EditorWindow
{
    private static HashSet<Object> problematicAssets;
    private Vector2 scrollPosition;

    [MenuItem("Optimization Analyzer/Show Problematic Assets")]
    public static void ShowWindow()
    {
        var window = GetWindow<ProblematicAssetsWindow>(true, "Problematic Assets", true);

        window.ShowUtility();
    }

    public static void SetProblematicAssets(HashSet<Object> problematicAssets_)
    {
        problematicAssets = problematicAssets_;
        ShowWindow();
    }

    void OnGUI()
    {
        if (problematicAssets == null || problematicAssets.Count == 0)
        {
            GUILayout.Label("No unused textures found.");
            return;
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);


        foreach (Object object_ in problematicAssets)
        {
            if (GUILayout.Button(object_.name))
            {
                Selection.activeObject = object_;
                EditorUtility.FocusProjectWindow();
            }
        }

        EditorGUILayout.EndScrollView();
    }
}
