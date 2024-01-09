using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class MaterialAndShaderAnalyzer : IAnalyzer
{
    private static readonly MaterialAndShaderAnalyzer instance = new MaterialAndShaderAnalyzer();

    private MaterialAndShaderAnalyzer() { }

    public static MaterialAndShaderAnalyzer Instance => instance;

    public void Analyze(Action<float, string> progressCallback = null)
    {
        bool found = false;
        string[] allMaterials = AssetDatabase.FindAssets("t:Material");

        int totalMaterials = allMaterials.Length;
        int currentIndex = 0;

        foreach (string matGuid in allMaterials)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(matGuid);

            // Skip assets in the Packages folder
            if (assetPath.Contains("Packages/"))
            {
                continue;
            }
            progressCallback?.Invoke((float)currentIndex / totalMaterials, $"Analyzing Material: {assetPath}");

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat.shader && Utils.IsAssetUsed(mat))
            {
                if (mat.passCount > OptimizationAnalyzerWindow.materialPassesThrehold)
                {
                    found = true;
                    Debug.LogWarning($"Material {mat.name} uses shader {mat.shader.name} with {mat.passCount} passes at {assetPath}");
                }
            }

            if (mat.mainTexture == null && Utils.IsAssetUsed(mat))
            {
                found = true;
                Debug.LogWarning($"Material {mat.name} has no main texture assigned at {assetPath}");
            }

            currentIndex++;

        }

        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        int totalObjects = allObjects.Length;
        currentIndex = 0; // Reset index for the next loop

        foreach (var obj in allObjects)
        {
            var renderer = obj.GetComponent<Renderer>();

            progressCallback?.Invoke((float)currentIndex / totalObjects, $"Analyzing GameObject: {obj.name}");

            if (renderer && renderer.sharedMaterials.Length > 1 && Utils.IsAssetUsed(obj))
            {
                found = true;
                Debug.LogWarning($"Object {obj.name} uses {renderer.sharedMaterials.Length} materials.");
            }
            currentIndex++;

        }

        if (!found)
        {
            Logger.LogSuccess("Congratulations: No Materials and Shaders problems detected!!!");
        }
    }
}
