using System;
using UnityEditor;
using UnityEngine;

public class MeshAnalyzer : IAnalyzer
{
    private static readonly MeshAnalyzer instance = new MeshAnalyzer();

    private MeshAnalyzer() { }

    public static MeshAnalyzer Instance => instance;

    public void Analyze(Action<float, string> progressCallback = null)
    {
        bool found = false;
        string[] allMeshes = AssetDatabase.FindAssets("t:Mesh");

        int totalMeshes = allMeshes.Length;
        int currentIndex = 0;

        int vertexLimit = OptimizationAnalyzerWindow.vertexCountThreshold;

        foreach (string meshGuid in allMeshes)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(meshGuid);

            progressCallback?.Invoke((float)currentIndex / totalMeshes, $"Analyzing Mesh: {assetPath}");

            //Analyze vertex count
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
            if (mesh.vertexCount > vertexLimit && Utils.IsAssetUsed(mesh))
            {
                found = true;
                Debug.LogWarning($"Mesh with high vertex count: {mesh.name} ({mesh.vertexCount} vertices) at {assetPath}");
            }

            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (Utils.IsAssetUsed(obj))
            {
                //Analyze colliders
                if (!obj.GetComponent<Collider>())
                {
                    found = true;
                    Debug.LogWarning($"Mesh without a collider: {obj.name} at {assetPath}");
                }

                //Analyze LODs
                if (!obj.GetComponent<LODGroup>())
                {
                    found = true;
                    Debug.LogWarning($"Mesh without LODs: {obj.name} at {assetPath}");
                }
            }
            currentIndex++;

        }

        if (!found)
        {
            Logger.LogSuccess("Congratulations: No Mesh problem detected!!!");
        }

    }
}
