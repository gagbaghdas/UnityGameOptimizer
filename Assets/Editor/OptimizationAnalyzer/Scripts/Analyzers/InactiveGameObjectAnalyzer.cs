using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public class InactiveGameObjectAnalyzer : IAnalyzer
{
    private static readonly InactiveGameObjectAnalyzer instance = new InactiveGameObjectAnalyzer();

    private InactiveGameObjectAnalyzer() { }

    public static InactiveGameObjectAnalyzer Instance => instance;

    public void Analyze(Action<float, string> progressCallback = null)
    {
        string currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

        ListInactiveObjectsInScene(progressCallback);
        ListInactiveObjectsInPrefabs(progressCallback);

        // Re-open the original scene
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath);
        }
    }

    private void ListInactiveObjectsInScene(Action<float, string> progressCallback = null)
    {
        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

        int currentIndex = 0;
        int totalScenes = buildScenes.Length;

        foreach (EditorBuildSettingsScene buildScene in buildScenes)
        {
            if (buildScene.enabled) // Only consider enabled scenes in build settings
            {
                progressCallback?.Invoke((float)currentIndex / totalScenes, $"Checking inactive GameObjects in scene {buildScene.path}");

                EditorSceneManager.OpenScene(buildScene.path);

                GameObject[] allObjects = Object.FindObjectsOfType<GameObject>(true); // true => include inactive objects
                bool found = false;

                foreach (var obj in allObjects)
                {
                    if (!obj.activeInHierarchy && !Utils.IsGameObjectOrParentUsedInCode(obj))
                    {
                        found = true;
                        Debug.LogWarning($"Inactive GameObject found in scene '{buildScene.path}' : '{obj.name}'");
                    }
                }

                if (!found)
                {
                    Logger.LogSuccess("Congratulations: No Inactive GameObject found in scenes!!!");
                }
                currentIndex++;

            }
        }
    }

    private void ListInactiveObjectsInPrefabs(Action<float, string> progressCallback = null)
    {
        bool found = false;
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");

        int currentIndex = 0;
        int totalPrefabs = allPrefabs.Length;

        foreach (string prefabGuid in allPrefabs)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (prefab)
            {
                progressCallback?.Invoke((float)currentIndex / totalPrefabs, $"Checking inactive GameObjects in prefab {assetPath}");

                GameObject instance = Object.Instantiate(prefab);
                var allChildren = instance.GetComponentsInChildren<Transform>(true); // true => include inactive objects

                foreach (var child in allChildren)
                {
                    if (!child.gameObject.activeInHierarchy && !Utils.IsGameObjectUsedInCode(child.name))
                    {
                        found = true;
                        Debug.LogWarning($"Inactive GameObject found in prefab '{prefab.name}': {child.name}");
                    }
                }

                Object.DestroyImmediate(instance);
                currentIndex++;
            }
        }

        if (!found)
        {
            Logger.LogSuccess("Congratulations: No Inactive GameObject found in prefabs!!!");
        }
    }

}