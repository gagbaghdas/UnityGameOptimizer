using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;


public static class Utils
{
    private static readonly HashSet<string> imageExtensions = new HashSet<string> { ".png", ".jpg", ".jpeg", ".tga", ".bmp" };

    public enum TextureAnalysisType
    {
        All,
        Used
    }

    public static bool IsAssetUsed(Object asset)
    {
        string assetPath = AssetDatabase.GetAssetPath(asset);

        return IsAssetUsed(assetPath);
    }

    public static bool IsAssetUsed(string assetPath)
    {
        return IsAssetHaveDependecies(assetPath) || (IsAssetInResourcesFolder (assetPath) && IsAssetReferencedInCode(assetPath));
    }

    public static bool IsAssetHaveDependecies (string assetPath)
    {
        string[] dependencies = AssetDatabase.GetDependencies(assetPath);
        return dependencies.Length > 1;
    }

    public static bool IsAssetInResourcesFolder(string assetPath)
    {
        return assetPath.StartsWith("Assets/Resources/");
    }

    public static bool IsAssetInResourcesFolderButNotReferencedInCode(string assetPath)
    {
        return IsAssetInResourcesFolder(assetPath) && !IsAssetReferencedInCode(assetPath);
    }

    public static bool IsAssetReferencedInCode(string assetPath)
    {
        string[] allScripts = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        foreach (string scriptPath in allScripts)
        {
            string scriptContent = File.ReadAllText(scriptPath);

            // Regular expression to find Resources.Load("Your/Path")
            string patternLoad = "Resources.Load\\(\\s?\"(.*?)\"\\s?\\)";
            MatchCollection matchesLoad = Regex.Matches(scriptContent, patternLoad);

            foreach (Match match in matchesLoad.Cast<Match>())
            {
                if (match.Groups[1].Value == assetPath)
                {
                    return true;
                }
            }

            // Regular expression to find Resources.LoadAll("Your/Path")
            string patternLoadAll = "Resources.LoadAll\\(\\s?\"(.*?)\"\\s?\\)";
            MatchCollection matchesLoadAll = Regex.Matches(scriptContent, patternLoadAll);

            foreach (Match match in matchesLoadAll.Cast<Match>())
            {
                string referencedPath = match.Groups[1].Value;
                if (assetPath.StartsWith(referencedPath))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsGameObjectOrParentUsedInCode(GameObject gameObject)
    {
        // Check the GameObject itself
        if (IsGameObjectUsedInCode(gameObject.name))
        {
            return true;
        }

        // Check all parents
        Transform parentTransform = gameObject.transform.parent;
        while (parentTransform != null)
        {
            if (IsGameObjectUsedInCode(parentTransform.gameObject.name))
            {
                return true;
            }
            parentTransform = parentTransform.parent;
        }

        return false;
    }

    public static bool IsGameObjectUsedInCode(string gameObjectName)
    {
        string scriptsPath = Application.dataPath + "/Scripts/";
        string[] scriptFiles = Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories);

        string[] activationPatterns = new string[]
        {
        $"{gameObjectName}.SetActive(true)",
        $"{gameObjectName}.activeSelf = true",
        $"{gameObjectName}.activeInHierarchy",
        $"SetActiveRecursively({gameObjectName}, true)",  // Obsolete but sometimes used
        $"{gameObjectName}.gameObject.SetActive(true)",
        $"{gameObjectName}.gameObject.activeSelf = true",
        $"{gameObjectName}.gameObject.activeInHierarchy",
        $"SetActiveRecursively({gameObjectName}.gameObject, true)",  // Obsolete but sometimes used
        };

        foreach (string scriptPath in scriptFiles)
        {
            string scriptContent = File.ReadAllText(scriptPath);

            // Look for patterns that indicate the GameObject is activated in code.
            foreach (var pattern in activationPatterns)
            {
                if (scriptContent.Contains(pattern))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool AreTexturesSimilar(Texture2D tex1, Texture2D tex2)
    {
        Texture2D smallTex1 = ResizeTexture(tex1, 8, 8);
        Texture2D smallTex2 = ResizeTexture(tex2, 8, 8);

        Color[] colors1 = smallTex1.GetPixels();
        Color[] colors2 = smallTex2.GetPixels();

        for (int i = 0; i < colors1.Length; i++)
        {
            if (Mathf.Abs(colors1[i].grayscale - colors2[i].grayscale) > 0.1f)
            {
                return false;
            }
        }

        return true;
    }

    public static Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        return nTex;
    }

    public static bool IsImage(string assetPath)
    {
        var extension = Path.GetExtension(assetPath).ToLower();

        return imageExtensions.Contains(extension);
    }

    public static HashSet<string> GetUsedInScenesOrPrefabsAssetPaths()
    {
        string currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

        HashSet<string> usedAssets = new HashSet<string>();

        // 2. Find Assets Used in Scenes
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                EditorSceneManager.OpenScene(scene.path);

                string[] dependencies = AssetDatabase.GetDependencies(scene.path);
                foreach (string dep in dependencies)
                {
                    usedAssets.Add(dep);
                }
            }
        }

        // 3. Find Assets Used in Prefabs
        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string prefab in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefab);
            string[] dependencies = AssetDatabase.GetDependencies(path);
            foreach (string dep in dependencies)
            {
                usedAssets.Add(dep);
            }
        }

        // Re-open the original scene
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath);
        }

        return usedAssets;
    }

    public static bool IsPossibleSDKFolder(string folderPath)
    {
        // Check for indicative file names
        var indicativeFiles = new[] { "readme.txt", "readme.md", "documentation.txt", "documentation.pdf", "license.txt", "license.md", "changelog.txt", "changelog.md" };
        var filesInFolder = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);

        foreach (var file in indicativeFiles)
        {
            if (filesInFolder.Any(f => f.EndsWith(file, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        // Check folder structure
        var subFolders = Directory.GetDirectories(folderPath);
        if (subFolders.Any(sf => sf.EndsWith("examples", StringComparison.OrdinalIgnoreCase) || sf.EndsWith("demos", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }

    public static bool MethodExists(Type type, string methodName)
    {
        return type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null;
    }

    public static bool IsGooglePlaySDK(string assetPath)
    {
        return assetPath.Contains("GooglePlayServices") ||
               assetPath.Contains("/GooglePlay/") ||
               assetPath.Contains("google-play-");
    }

    public static bool CheckIfSpriteAtlasIsUsed(SpriteAtlas spriteAtlas)
    {
        string currentScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

        bool isUsed = false;

        // Load all scenes
        for (int i = 0; i < EditorBuildSettings.scenes.Length; ++i)
        {
            var scenePath = EditorBuildSettings.scenes[i].path;
            EditorSceneManager.OpenScene(scenePath);

            // Check all GameObjects in the scene
            GameObject[] allGameObjects = Object.FindObjectsOfType<GameObject>();
            foreach (GameObject go in allGameObjects)
            {
                // Check SpriteRenderers
                SpriteRenderer[] spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in spriteRenderers)
                {
                    if (sr.sprite != null && spriteAtlas.CanBindTo(sr.sprite))
                    {
                        isUsed = true;
                        return isUsed;
                    }
                }

                // Check UI Images
                Image[] images = go.GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    if (img.sprite != null && spriteAtlas.CanBindTo(img.sprite))
                    {
                        isUsed = true;
                        return isUsed;
                    }
                }
            }
        }

        // Check prefabs
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        List<Object> allPrefabs = new List<Object>();
        foreach (string assetPath in allAssetPaths)
        {
            if (assetPath.Contains(".prefab"))
            {
                Object prefab = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
                allPrefabs.Add(prefab);
            }
        }

        foreach (Object prefab in allPrefabs)
        {
            GameObject go = (GameObject)prefab;

            // Check SpriteRenderers
            SpriteRenderer[] spriteRenderers = go.GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in spriteRenderers)
            {
                if (sr.sprite != null && spriteAtlas.CanBindTo(sr.sprite))
                {
                    isUsed = true;
                    return isUsed;
                }
            }

            // Check UI Images
            Image[] images = go.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.sprite != null && spriteAtlas.CanBindTo(img.sprite))
                {
                    isUsed = true;
                    return isUsed;
                }
            }
        }

        // Re-open the original scene
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath);
        }

        return isUsed;
    }

}
