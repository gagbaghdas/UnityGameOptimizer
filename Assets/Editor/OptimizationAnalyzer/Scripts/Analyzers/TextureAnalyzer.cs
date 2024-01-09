using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class TextureAnalyzer : IAnalyzer
{
    private static readonly TextureAnalyzer instance = new TextureAnalyzer();

    private TextureAnalyzer() { }

    public static TextureAnalyzer Instance => instance;

    public void Analyze(Action<float, string> progressCallback = null)
    {
        bool found = false;
        // Fetch all textures in the project
        string[] guids = AssetDatabase.FindAssets("t:Texture");
        Dictionary<string, Texture2D> textures2Ds = new Dictionary<string, Texture2D>();
        int totalTextures = guids.Length;

        for (int i = 0; i < guids.Length; i++)
        {
            string guid = guids[i];

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (!Utils.IsImage(assetPath))
            {
                continue;
            }

            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);

            if (OptimizationAnalyzerWindow.selectedTextureAnalysisType == Utils.TextureAnalysisType.All ||
               (OptimizationAnalyzerWindow.selectedTextureAnalysisType == Utils.TextureAnalysisType.Used && Utils.IsAssetUsed(texture)))
            {
                // Checking for 4K textures
                if (texture.width > 4096 || texture.height > 4096)
                {
                    found = true;
                    Logger.LogWarning($"Oversized Texture (4K+) Detected: {texture.name} at {assetPath}", texture);
                }
                // Checking for 2K textures
                else if (texture.width > 2048 || texture.height > 2048)
                {
                    found = true;
                    Logger.Log($"Large Texture (2K) Detected: {texture.name} at {assetPath}. Consider downsizing if not necessary.", texture);
                }


                // Check if texture is compressed
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer != null && importer.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    found = true;
                    Logger.LogWarning($"Uncompressed Texture Detected: {texture.name} at {assetPath}", texture);

                    // Check for mipmaps
                    if (texture.width > 512 && !importer.mipmapEnabled)
                    {
                        found = true;
                        Logger.LogWarning($"Large texture without MipMaps: {texture.name} at {assetPath}. Consider enabling MipMaps.", texture);
                    }
                    else if (texture.width < 128 && importer.mipmapEnabled)
                    {
                        found = true;
                        Logger.LogWarning($"Small texture with unnecessary MipMaps: {texture.name} at {assetPath}. Consider disabling MipMaps.", texture);
                    }

                    // This is a general check.
                    if (importer.textureCompression == TextureImporterCompression.CompressedLQ)
                    {
                        found = true;
                        Logger.LogWarning($"Texture is using low-quality compression: {texture.name} at {assetPath}. Verify if this is intended.", texture);
                    }
                }

                // Check for Non-square
                if (Mathf.Abs(texture.width - texture.height) > texture.width * 0.1f) // More than 10% difference
                {
                    found = true;
                    Logger.LogWarning($"Non-square texture detected: {texture.name} at {assetPath}. Check if this is intentional.", texture);
                }

                Texture2D tex2D = texture as Texture2D;
                if (tex2D != null)
                {
                    textures2Ds.Add(assetPath, tex2D);
                }
            }

            progressCallback?.Invoke((float)i / totalTextures, $"Analyzing texture {i + 1} of {totalTextures}");
        }

        // Check for Multiple Textures with Similar Content
        List<string> keys = new List<string>(textures2Ds.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            for (int j = i + 1; j < keys.Count; j++)
            {
                var tex1 = textures2Ds[keys[i]];
                var tex2 = textures2Ds[keys[j]];

                if (Utils.AreTexturesSimilar(tex1, tex2))
                {
                    found = true;
                    Logger.LogWarning($"{keys[i]} and {keys[j]} might be similar.", tex1);
                }
            }

            progressCallback?.Invoke((float)i / totalTextures, $"Analyzing Similarity of texture {i + 1} of {textures2Ds.Keys.Count}");
        }

        if (!found)
        {
            Logger.LogSuccess("Congratulations: No Texture problem detected!!!");
        }

    }
}
