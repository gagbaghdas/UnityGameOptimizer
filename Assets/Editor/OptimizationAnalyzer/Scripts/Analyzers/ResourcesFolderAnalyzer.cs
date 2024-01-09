using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

public class ResourcesFolderAnalyzer : IAnalyzer
{
    private static readonly ResourcesFolderAnalyzer instance = new ResourcesFolderAnalyzer();

    private ResourcesFolderAnalyzer() { }

    public static ResourcesFolderAnalyzer Instance => instance;

    public void Analyze(Action<float, string> progressCallback = null)
    {
        bool found = false;
        // 1. Get All Assets
        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        HashSet<string> usedAssets = Utils.GetUsedInScenesOrPrefabsAssetPaths();
        HashSet<Object> unusedAssets = new HashSet<Object>();

        int totalAssets = allAssets.Length;
        int currentIndex = 0;

        // 4. Compare and List Potentially Unused Assets
        foreach (string asset in allAssets)
        {
            progressCallback?.Invoke((float)currentIndex / totalAssets, $"Analyzing Asset: {asset}");

            if (!Directory.Exists(asset) && Utils.IsAssetInResourcesFolderButNotReferencedInCode(asset) && !usedAssets.Contains(asset))
            {
                found = true;
                Object object_ = AssetDatabase.LoadAssetAtPath<Object>(asset);
                unusedAssets.Add(object_);
                Logger.LogWarning($"Potentially unused asset: {asset}", object_);
            }

            currentIndex++;

        }

        ProblematicAssetsWindow.SetProblematicAssets(unusedAssets);

        if (!found)
        {
            Logger.LogSuccess("Congratulations: No Unused asset detected in RESOURCES!!!");
        }
    }
}
