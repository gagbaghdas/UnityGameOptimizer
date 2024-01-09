using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class ScriptAnalyzer : IAnalyzer
{
    private static readonly ScriptAnalyzer instance = new ScriptAnalyzer();

    private ScriptAnalyzer() { }

    public static ScriptAnalyzer Instance => instance;

    public void Analyze(Action<float, string> progressCallback = null)
    {
        float totalSteps = 3;  // Total number of steps
        float currentStep = 0;

        progressCallback?.Invoke(currentStep / totalSteps, "Analyzing Update Usage...");
        AnalyzeUpdateUsage();
        currentStep++;

        progressCallback?.Invoke(currentStep / totalSteps, "Analyzing Raycast Usage...");
        AnalyzeRaycastUsage();
        currentStep++;

        progressCallback?.Invoke(currentStep / totalSteps, "Checking for GC Allocations...");
        CheckForGCAllocations();
        currentStep++;

        progressCallback?.Invoke(currentStep / totalSteps, "Analysis Completed");
    }

    private void AnalyzeUpdateUsage()
    {
        MonoBehaviour[] scripts = Object.FindObjectsOfType<MonoBehaviour>();
        int updateCount = 0, fixedUpdateCount = 0, lateUpdateCount = 0;

        foreach (var script in scripts)
        {
            Type type = script.GetType();
            while (type != null && type != typeof(MonoBehaviour))
            {
                if (Utils.MethodExists(type, "Update"))
                {
                    updateCount++;
                    break; // No need to keep looking up the hierarchy
                }
                type = type.BaseType;
            }

            type = script.GetType();
            while (type != null && type != typeof(MonoBehaviour))
            {
                if (Utils.MethodExists(type, "FixedUpdate"))
                {
                    fixedUpdateCount++;
                    break; // No need to keep looking up the hierarchy
                }
                type = type.BaseType;
            }

            type = script.GetType();
            while (type != null && type != typeof(MonoBehaviour))
            {
                if (Utils.MethodExists(type, "LateUpdate"))
                {
                    lateUpdateCount++;
                    break; // No need to keep looking up the hierarchy
                }
                type = type.BaseType;
            }
        }

        Debug.Log($"Number of scripts using Update: {updateCount}");
        Debug.Log($"Number of scripts using FixedUpdate: {fixedUpdateCount}");
        Debug.Log($"Number of scripts using LateUpdate: {lateUpdateCount}");
    }
  
    private void AnalyzeRaycastUsage()
    {
        MonoBehaviour[] scripts = Object.FindObjectsOfType<MonoBehaviour>();
        int raycastCount = 0;

        foreach (var script in scripts)
        {
            MethodInfo[] methods = script.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                if (method.ToString().Contains("RaycastHit"))
                {
                    raycastCount++;
                }
            }
        }

        Debug.Log($"Number of scripts potentially using raycasts: {raycastCount}");
    }

    private void CheckForGCAllocations()
    {
        bool found = false;
        string[] allScriptFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

        string updatePattern = @"\bvoid\s+Update\s*\(\s*\)\s*{[^}]*}";

        string newPattern = @"(?<=\s|^)new\s+[\w\<\>]+\s*(\(.*\))?(?=\s|;)";

        string concatPattern = @"(\"".+?\""\s*\+\s*.+?\b)|(\b.+?\s*\+\s*\"".+?\"")";
        string linqPattern = @"\.(Where|Select|First|FirstOrDefault|Last|LastOrDefault|Any|All|Count|ToList|ToArray)\(";

        string boxingPattern1 = @"\bobject\s+\w+\s*=\s*\w+;";
        string boxingPattern2 = @"\bArrayList\s+\w+\s*=\s*new\s+ArrayList\(\);";

        string lambdaPattern = @"=>";
        string anonMethodPattern = @"delegate\s*\(";
        string eventPattern = @"(\w+(\s*\+=\s*)(new\s+)?\s*[\w\.]+\s*\(\s*\)\s*;)|(event\s+\w+\s*\+=)";

        foreach (var file in allScriptFiles)
        {
            bool foundConcat = false, foundLinq = false, foundNew = false, foundBoxing = false, foundLambda = false;

            string content = File.ReadAllText(file);
            string contentWithoutComments = Regex.Replace(content, @"//.*$", string.Empty, RegexOptions.Multiline);

            Match match = Regex.Match(contentWithoutComments, updatePattern, RegexOptions.Singleline);

            if (match.Success)
            {
                int lineNumber = content.Take(match.Index).Count(c => c == '\n') + 1; // To get the line number of the Update method's start
                string relativePath = file.Replace(Application.dataPath, "Assets");

                if (!foundConcat && Regex.IsMatch(match.Value, concatPattern))
                {
                    // Convert absolute path to a Unity relative path
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);

                    Logger.LogWarning($"Potential string concatenation detected in Update method of script: {relativePath}:{lineNumber}", script);
                    foundConcat = true;
                    found = true;
                }

                // Check if LINQ namespace is imported
                if (!foundLinq && content.Contains("using System.Linq;") && Regex.IsMatch(match.Value, linqPattern))
                {
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);

                    Logger.LogWarning($"Potential LINQ usage detected in Update method of script:  {relativePath}:{lineNumber}", script);
                    foundLinq = true;
                    found = true;
                }

                if (!foundNew && Regex.IsMatch(match.Value, newPattern))
                {
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);

                    Logger.LogWarning($"Potential GC allocation detected in Update method of script:  {relativePath}:{lineNumber}", script);
                    foundNew = true;
                    found = true;
                }

                if (!foundBoxing && Regex.IsMatch(match.Value, boxingPattern1) || Regex.IsMatch(match.Value, boxingPattern2))
                {
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);

                    Logger.LogWarning($"Potential boxing detected in Update method of script:  {relativePath}:{lineNumber}", script);
                    foundBoxing = true;
                    found = true;
                }

                if (!foundLambda && (Regex.IsMatch(match.Value, lambdaPattern) || Regex.IsMatch(match.Value, anonMethodPattern) || Regex.IsMatch(match.Value, eventPattern)))
                {
                    MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(relativePath);

                    Logger.LogWarning($"Potential delegate allocation detected in Update method of script:  {relativePath}:{lineNumber}", script);
                    foundLambda = true;
                    found = true;
                }

                if (foundConcat && foundLinq && foundNew && foundBoxing && foundLambda)
                {
                    break;
                }

            }
           

                    
        }

        if (!found)
        {
            Logger.LogSuccess("Congratulations: No GC problem detected!!!");
        }

    }
}