//using UnityEngine;
//using UnityEditor;
//using System.Reflection;
//using System;

//public class OptimizationAnalyzerWindow : EditorWindow
//{
//    private static readonly IAnalyzer[] analyzers = new IAnalyzer[]
//    {
//        MaterialAndShaderAnalyzer.Instance,
//        MeshAnalyzer.Instance,
//        TextureAnalyzer.Instance,
//        UnusedAssetsAnalyzer.Instance,
//        InactiveGameObjectAnalyzer.Instance,
//        ScriptAnalyzer.Instance,
//        SceneAnalyzer.Instance,
//        ResourcesFolderAnalyzer.Instance
//    };

//    public static Utils.TextureAnalysisType selectedTextureAnalysisType = Utils.TextureAnalysisType.All;

//    public static int vertexCountThreshold = 1000;
//    public static int materialPassesThrehold = 2;
//    public static int sceneGameObjectThreshold = 1000;
//    public static int sceneRealTimeLightningThreshold = 10;
//    public static int terrainHeightMapSizeThreshold = 1024;
//    public static int collidersOverlapThreshold = 100;
//    public static GUILayoutOption intFieldWidth = GUILayout.Width(60);
//    public static GUILayoutOption labelWidth = GUILayout.Width(220);

//    private Vector2 windowSize = new Vector2(300, 550);

//    [MenuItem("Optimization Analyzer/Open Analyzer")]
//    public static void ShowWindow()
//    {
//        var window =  GetWindow<OptimizationAnalyzerWindow>("Optimization Analyzer");
//        window.minSize = window.windowSize;
//        window.maxSize = window.windowSize;
//    }

//    private void OnGUI()
//    {
//        RenderAnalyzeProjectButton();
//        RenderAnalyzeTexturesButton();
//        RenderAnalyzeInactiveObjectsButton();
//        RenderAnalyzeScenesButton();
//        RenderAnalyzeScriptsButton();
//        RenderAnalyzeUnusedAssetsButton();
//        RenderAnalyzeResourcesFolderButton();
//        RenderAnalyzeMaterialsAndShadersButton();
//        RenderAnalyzeMeshesButton();
//    }

//    private void RenderAnalyzeProjectButton()
//    {
//        CreateButton("Analyze Project", () =>
//        {
//            ClearConsole();
//            foreach (var analyzer in analyzers)
//            {
//                analyzer.Analyze();
//            }
//        });
//    }

//    private void RenderAnalyzeButtons()
//    {


//        CreateButton("Analyze Inactive Objects", () =>
//        {
           
//        });

//        CreateButton("Analyze Scenes", () =>
//        {
            
//        });

//        CreateButton("Analyze Scripts", () =>
//        {
            
//        });

//        CreateButton("Analyze Unused Assets", () =>
//        {
            
//        });

//        CreateButton("Analyze Resources Folder", () =>
//        {
            
//        });

//        CreateButton("Analyze Materials and Shaders", () =>
//        {
            
//        });

//        CreateButton("Analyze Meshes", () =>
//        {
            
//        });
//    }

//    private void CreateButton(string buttonText, Action onClick)
//    {
//        GUILayout.BeginHorizontal();
//        GUILayout.FlexibleSpace();
//        if (GUILayout.Button(buttonText, GUILayout.Width(200), GUILayout.Height(30)))
//        {
//            onClick.Invoke();
//        }
//        GUILayout.FlexibleSpace();
//        GUILayout.EndHorizontal();
//        GUILayout.Space(5);
//    }

//    private void RenderThresholdSettings()
//    {
//        GUILayout.Label("Analysis Type:", EditorStyles.boldLabel);
//        selectedTextureAnalysisType = (Utils.TextureAnalysisType)EditorGUILayout.EnumPopup("Select Type:", selectedTextureAnalysisType, GUILayout.Width(220));

//        CreateThresholdField("GameObject Threshold:", ref sceneGameObjectThreshold);
//        CreateThresholdField("Realtime Lightning Threshold:", ref sceneRealTimeLightningThreshold);
//        CreateThresholdField("Terrain Heightmap Size Threshold:", ref terrainHeightMapSizeThreshold);
//        CreateThresholdField("Colliders Overlap Threshold:", ref collidersOverlapThreshold);
//        CreateThresholdField("Material Passes Threshold:", ref materialPassesThrehold);
//        CreateThresholdField("Vertex Count Threshold:", ref vertexCountThreshold);
//    }

//    private void CreateThresholdField(string labelText, ref int fieldValue)
//    {
//        GUILayout.BeginHorizontal();
//        GUILayout.Space(10);
//        GUILayout.Label(labelText, EditorStyles.boldLabel, labelWidth);
//        fieldValue = EditorGUILayout.IntField(fieldValue, intFieldWidth);
//        GUILayout.EndHorizontal();
//        GUILayout.Space(3);
//    }

//    private void ClearConsole()
//    {
//        Assembly assembly = Assembly.GetAssembly(typeof(Editor));
//        Type logEntries = assembly.GetType("UnityEditor.LogEntries");
//        MethodInfo clearMethod = logEntries.GetMethod("Clear");
//        clearMethod.Invoke(new object(), null);
//    }
//}


using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

public class OptimizationAnalyzerWindow : EditorWindow
{
    private static readonly IAnalyzer[] analyzers = new IAnalyzer[]
    {
        MaterialAndShaderAnalyzer.Instance,
        MeshAnalyzer.Instance,
        TextureAnalyzer.Instance,
        UnusedAssetsAnalyzer.Instance,
        InactiveGameObjectAnalyzer.Instance,
        ScriptAnalyzer.Instance,
        SceneAnalyzer.Instance,
        ResourcesFolderAnalyzer.Instance
    };

    public static Utils.TextureAnalysisType selectedTextureAnalysisType = Utils.TextureAnalysisType.All;
    public static int vertexCountThreshold = 1000;
    public static int materialPassesThrehold = 2;
    public static int sceneGameObjectThreshold = 1000;
    public static int sceneRealTimeLightningThreshold = 10;
    public static int terrainHeightMapSizeThreshold = 1024;
    public static int collidersOverlapThreshold = 100;
    public static GUILayoutOption intFieldWidth = GUILayout.Width(60);
    public static GUILayoutOption labelWidth = GUILayout.Width(220);

    private Vector2 windowSize = new Vector2(300, 550);

    [MenuItem("Optimization Analyzer/Open Analyzer")]
    public static void ShowWindow()
    {
        var window = GetWindow<OptimizationAnalyzerWindow>("Optimization Analyzer");
        window.minSize = window.windowSize;
        window.maxSize = window.windowSize;
    }

    private void OnGUI()
    {
        RenderAnalyzeProjectButton();
        RenderAnalyzeTexturesButton();
        RenderAnalyzeInactiveObjectsButton();
        RenderAnalyzeScenesButton();
        RenderAnalyzeScriptsButton();
        RenderAnalyzeUnusedAssetsButton();
        RenderAnalyzeResourcesFolderButton();
        RenderAnalyzeMaterialsAndShadersButton();
        RenderAnalyzeMeshesButton();
    }

    private void RenderAnalyzeProjectButton()
    {
        CreateButton("Analyze Project", () =>
        {
            ClearConsole();
            foreach (var analyzer in analyzers)
            {
                analyzer.Analyze();
            }
        });
    }

    private void RenderAnalyzeTexturesButton()
    {

        CreateButton("Analyze Textures", () =>
        {
            ClearConsole();

            EditorUtility.DisplayProgressBar("Analyzing Textures", "Starting...", 0f);

            TextureAnalyzer.Instance.Analyze((progress, message) =>
            {
                EditorUtility.DisplayProgressBar("Analyzing Textures", message, progress);
            });
            EditorUtility.ClearProgressBar();
        });
        
        CreateThresholdField("Texture Analysis Type:", ref selectedTextureAnalysisType);
    }

    private void RenderAnalyzeInactiveObjectsButton()
    {
        CreateButton("Analyze Inactive Objects", () =>
        {
            ClearConsole();
            EditorUtility.DisplayProgressBar("Analyzing Inactive GameObjects", "Starting...", 0f);
            InactiveGameObjectAnalyzer.Instance.Analyze((progress, message) =>
            {
                EditorUtility.DisplayProgressBar("Analyzing Inactive GameObjects", message, progress);
            });

            EditorUtility.ClearProgressBar();
        });
    }

    private void RenderAnalyzeScenesButton()
    {
        CreateButton("Analyze Scenes", () =>
        {
            ClearConsole();
            EditorUtility.DisplayProgressBar("Analyzing Scenes", "Starting...", 0f);

            SceneAnalyzer.Instance.Analyze((progress, message) =>
            {
                EditorUtility.DisplayProgressBar("Analyzing Scenes", message, progress);
            });

            EditorUtility.ClearProgressBar();
        });
        CreateThresholdField("GameObject Threshold:", ref sceneGameObjectThreshold);
        CreateThresholdField("Realtime Lightning Threshold:", ref sceneRealTimeLightningThreshold);
        CreateThresholdField("Terrain Heightmap Size Threshold:", ref terrainHeightMapSizeThreshold);
        CreateThresholdField("Colliders Overlap Threshold:", ref collidersOverlapThreshold);
    }

    private void RenderAnalyzeScriptsButton()
    {
        CreateButton("Analyze Scripts", () =>
        {
            ClearConsole();
            EditorUtility.DisplayProgressBar("Analyzing Scripts", "Starting...", 0f);

            ScriptAnalyzer.Instance.Analyze((progress, message) =>
            {
                EditorUtility.DisplayProgressBar("Analyzing Scripts", message, progress);
            });

            EditorUtility.ClearProgressBar();
        });
    }

    private void RenderAnalyzeUnusedAssetsButton()
    {
        CreateButton("Analyze Unused Assets", () =>
        {
            ClearConsole();
            EditorUtility.DisplayProgressBar("Analyzing Unused Assets", "Starting...", 0f);

            UnusedAssetsAnalyzer.Instance.Analyze((progress, message) =>
            {
                EditorUtility.DisplayProgressBar("Analyzing Unused Assets", message, progress);
            });

            EditorUtility.ClearProgressBar();
        });
    }

    private void RenderAnalyzeResourcesFolderButton()
    {
        CreateButton("Analyze Resources Folder", () =>
        {
            ClearConsole();
            EditorUtility.DisplayProgressBar("Analyzing Resources Folder", "Starting...", 0f);
            ResourcesFolderAnalyzer.Instance.Analyze((progress, message) =>
            {
                EditorUtility.DisplayProgressBar("Analyzing Resources Folder", message, progress);
            });

            EditorUtility.ClearProgressBar();
        });
    }

    private void RenderAnalyzeMaterialsAndShadersButton()
    {
        CreateButton("Analyze Materials and Shaders", () =>
        {
            ClearConsole();
            EditorUtility.DisplayProgressBar("Analyzing Materials and Shaders", "Starting...", 0f);

            MaterialAndShaderAnalyzer.Instance.Analyze((progress, message) =>
            {
                EditorUtility.DisplayProgressBar("Analyzing Materials and Shaders", message, progress);
            });

            EditorUtility.ClearProgressBar();
        });
        CreateThresholdField("Material Passes Threshold:", ref materialPassesThrehold);
    }

    private void RenderAnalyzeMeshesButton()
    {
        CreateButton("Analyze Meshes", () =>
        {
            ClearConsole();
            EditorUtility.DisplayProgressBar("Analyzing Meshes", "Starting...", 0f);

            MeshAnalyzer.Instance.Analyze((progress, message) =>
            {
                EditorUtility.DisplayProgressBar("Analyzing Meshes", message, progress);
            });

            EditorUtility.ClearProgressBar();
        });
        CreateThresholdField("Vertex Count Threshold:", ref vertexCountThreshold);
    }

    private void CreateButton(string buttonText, Action onClick)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(buttonText, GUILayout.Width(200), GUILayout.Height(30)))
        {
            onClick.Invoke();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
    }

    private void CreateThresholdField(string labelText, ref int fieldValue)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label(labelText, EditorStyles.boldLabel, labelWidth);
        fieldValue = EditorGUILayout.IntField(fieldValue, intFieldWidth);
        GUILayout.EndHorizontal();
        GUILayout.Space(3);
    }

    private void CreateThresholdField(string labelText, ref Utils.TextureAnalysisType fieldValue)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label(labelText, EditorStyles.boldLabel, labelWidth);
        fieldValue = (Utils.TextureAnalysisType)EditorGUILayout.EnumPopup(fieldValue, intFieldWidth);
        GUILayout.EndHorizontal();
        GUILayout.Space(3);
    }

    private void ClearConsole()
    {
        Assembly assembly = Assembly.GetAssembly(typeof(Editor));
        Type logEntries = assembly.GetType("UnityEditor.LogEntries");
        MethodInfo clearMethod = logEntries.GetMethod("Clear");
        clearMethod.Invoke(new object(), null);
    }
}
