using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;
using Object = UnityEngine.Object;

public class UnusedAssetsAnalyzer : IAnalyzer
{
    private static readonly UnusedAssetsAnalyzer instance = new UnusedAssetsAnalyzer();

    private UnusedAssetsAnalyzer() { }

    public static UnusedAssetsAnalyzer Instance => instance;

    public void Analyze(Action<float, string> progressCallback = null)
    {
        HashSet<string> usedAssets = Utils.GetUsedInScenesOrPrefabsAssetPaths();
        HashSet<Object> unusedAssets = new HashSet<Object>();

        // Additional code to handle ScriptableObjects
        string[] allScriptableObjects = AssetDatabase.FindAssets("t:ScriptableObject");
        foreach (var guid in allScriptableObjects)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string[] dependencies = AssetDatabase.GetDependencies(path, true);
            foreach (var dependency in dependencies)
            {
                usedAssets.Add(dependency);  // Mark as used
            }
        }

        // 1. Get All Assets
        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        bool found = false;

        float totalSteps = allAssets.Length;
        float currentStep = 0;

        // 4. Compare and List Potentially Unused Assets
        foreach (string asset in allAssets)
        {
            // Ensure asset path is not just a dot
            if (string.IsNullOrWhiteSpace(asset) || asset == ".")
            {
                continue;
            }
            if (IsAssetShouldBeConsidered(asset) && !usedAssets.Contains(asset))
            {
                currentStep++;
                if (currentStep % 10 == 0) // Update progress every 10 assets
                {
                    progressCallback?.Invoke(currentStep / totalSteps, "Analyzing unused assets...");
                }
                // Check if asset is in a possible SDK folder
                var assetDirectory = Path.GetDirectoryName(asset);

                if (assetDirectory.Contains("/Library/") || assetDirectory.Contains("/m2repository/") || assetDirectory.Contains("/Samples/") || assetDirectory.Contains("/Temp/") || assetDirectory.Contains("/Obj/") || Utils.IsPossibleSDKFolder(assetDirectory))
                {
                    continue;
                }

                string[] dependencies = AssetDatabase.GetDependencies(asset, false);

                foreach (var dependency in dependencies)
                {
                    if (usedAssets.Contains(dependency))
                    {
                        continue;
                    }

                    usedAssets.Add(dependency);  // Mark as used
                }

                if (!usedAssets.Contains(asset))
                {
                    Object object_ = AssetDatabase.LoadAssetAtPath<Object>(asset);
                    unusedAssets.Add(object_);
                    found = true;
                    Logger.LogWarning($"Potentially unused asset: {asset}", object_);
                }
            }
        }

        progressCallback?.Invoke(1f, "Unused assets analysis complete.");

        ProblematicAssetsWindow.SetProblematicAssets(unusedAssets);

        if (!found)
        {
            Logger.LogSuccess("Congratulations: No Unused asset detected!!!");
        }
    }

    private bool IsAssetShouldBeConsidered(string asset)
    {
        return !Directory.Exists(asset) &&
                !asset.StartsWith("ProjectSettings/") &&
                !asset.StartsWith("Packages/") &&
                !asset.StartsWith("Library/") &&
                !asset.StartsWith("Assets/Editor/") &&
                !asset.StartsWith("Assets/Plugins/") &&
                !asset.StartsWith("Assets/Samples/") &&
                !asset.EndsWith(".cs") &&
                !asset.EndsWith(".dll") &&
                !asset.EndsWith(".unity") &&
                !asset.EndsWith(".pdb") &&
                !asset.EndsWith(".asmdef") &&
                !asset.EndsWith(".swift") &&
                !asset.EndsWith(".h") &&
                !asset.EndsWith(".bundle") &&
                !asset.EndsWith(".bzl") &&
                !asset.EndsWith(".md") &&
                !asset.EndsWith(".m") &&
                !asset.EndsWith(".mm") &&
                !asset.EndsWith(".so") &&
                !asset.EndsWith(".swift") &&
                !asset.Contains(".srcaar") &&
                !asset.EndsWith("Dependencies.xml") &&
                !asset.EndsWith(".aar") &&
                !asset.Contains("m2repository") &&
                !asset.Contains("/GooglePlay/") &&
                !asset.EndsWith(".dll") &&
                !Utils.IsGooglePlaySDK(asset);
    }
}
