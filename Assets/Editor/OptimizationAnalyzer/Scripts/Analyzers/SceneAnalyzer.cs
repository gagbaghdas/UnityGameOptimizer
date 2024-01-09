using System;
using System.Drawing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public class SceneAnalyzer : IAnalyzer
{
    private static readonly SceneAnalyzer instance = new SceneAnalyzer();

    private SceneAnalyzer() { }

    public static SceneAnalyzer Instance => instance;
    private bool found = false;

    public void Analyze(Action<float, string> progressCallback = null)
    {
        string currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

        EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
        int totalScenes = buildScenes.Length;
        int currentSceneIndex = 0;
        found = false;

        foreach (EditorBuildSettingsScene scene in buildScenes)
        {
            if (scene.enabled)
            {
                EditorSceneManager.OpenScene(scene.path);

                float baseProgress = (float)currentSceneIndex / totalScenes;
                float progressStep = 1f / (totalScenes * 6);  // Assuming 6 analysis steps per scene

                progressCallback?.Invoke(baseProgress, $"Analyzing object count in {scene.path}");
                AnalyzeObjectCount();

                progressCallback?.Invoke(baseProgress + progressStep, $"Analyzing lights in {scene.path}");
                AnalyzeLights();

                progressCallback?.Invoke(baseProgress + progressStep * 2, $"Analyzing terrains in {scene.path}");
                AnalyzeTerrains();

                progressCallback?.Invoke(baseProgress + progressStep * 3, $"Checking non-batchable objects in {scene.path}");
                CheckForNonBatchableObjects();

                progressCallback?.Invoke(baseProgress + progressStep * 4, $"Analyzing overlapping colliders in {scene.path}");
                AnalyzeOverlappingColliders();

                progressCallback?.Invoke(baseProgress + progressStep * 5, $"Completed analysis for {scene.path}");
            }

            currentSceneIndex++;
        }

        if (!found)
        {
            Logger.LogSuccess("Congratulations: No Texture problem detected!!!");
        }

        // Re-open the original scene
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath);
        }
    }

    private void AnalyzeObjectCount()
    {
        int objectCount = Object.FindObjectsOfType<Transform>().Length;
        if (objectCount > OptimizationAnalyzerWindow.sceneGameObjectThreshold)
        {
            found = true;
            Debug.LogWarning($"The {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} scene contains {objectCount} objects, which might affect performance.");
        }
    }

    private void AnalyzeLights()
    {
        Light[] lights = Object.FindObjectsOfType<Light>();
        int realTimeLightsCount = 0;
        foreach (var light in lights)
        {
            if (light.shadows != LightShadows.None && light.lightmapBakeType == LightmapBakeType.Realtime && Utils.IsAssetUsed(light))
            {
                realTimeLightsCount++;
            }
        }

        if (realTimeLightsCount > OptimizationAnalyzerWindow.sceneRealTimeLightningThreshold)
        {
            found = true;
            Debug.LogWarning($"The {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} scene contains {realTimeLightsCount} real-time lights that cast shadows, which might affect performance.");
        }
    }

    private void AnalyzeTerrains()
    {
        Terrain[] terrains = Object.FindObjectsOfType<Terrain>();
        foreach (var terrain in terrains)
        {
            if (terrain.terrainData.heightmapResolution > OptimizationAnalyzerWindow.terrainHeightMapSizeThreshold && Utils.IsAssetUsed(terrain))
            {
                found = true;
                Logger.LogWarning($"Terrain '{terrain.name}' has a high-resolution height map which might be performance-intensive.", terrain);
            }
        }
    }

    private void AnalyzeOverlappingColliders()
    {
        Collider[] colliders = Object.FindObjectsOfType<Collider>();
        int overlapCount = 0;

        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = i + 1; j < colliders.Length; j++)
            {
                if (colliders[i].bounds.Intersects(colliders[j].bounds))
                {
                    overlapCount++;
                }
            }
        }

        if (overlapCount > OptimizationAnalyzerWindow.collidersOverlapThreshold)
        {
            found = true;
            Debug.LogWarning($"The {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name} scene has {overlapCount} overlapping colliders, which might be performance-intensive.");
        }
    }

    private void CheckForNonBatchableObjects()
    {
        Renderer[] renderers = Object.FindObjectsOfType<Renderer>();

        foreach (var renderer in renderers)
        {
            if (Utils.IsAssetUsed(renderer))
            {
                if (!renderer.allowOcclusionWhenDynamic)
                {
                    found = true;
                    Logger.LogWarning($"Renderer on object '{renderer.gameObject.name}' has 'Allow Occlusion When Dynamic' turned off, which can prevent batching.", renderer);
                }
                if (renderer.receiveShadows)
                {
                    found = true;
                    Logger.LogWarning($"Renderer on object '{renderer.gameObject.name}' is set to receive shadows. Complex shadow receivers can prevent batching.", renderer);
                }

                if (renderer.sharedMaterials.Length > 1)
                {
                    found = true;
                    Logger.LogWarning($"Object '{renderer.gameObject.name}' uses {renderer.sharedMaterials.Length} materials, which increases draw calls.", renderer);
                }
            }
        }
    }
}

