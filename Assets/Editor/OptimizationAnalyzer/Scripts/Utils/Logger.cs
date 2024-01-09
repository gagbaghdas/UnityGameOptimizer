using UnityEditor;
using UnityEngine;

public static class Logger
{
    public static void LogSuccess(string message)
    {
        Debug.Log($"<color=green>{message}</color>");
    }

    public static void LogWarning(string message, Object asset)
    {
        Debug.LogWarning(message, asset);

        EditorApplication.delayCall += () =>
        {
            if (Selection.activeObject == asset)
            {
                EditorUtility.FocusProjectWindow();
            }
        };
    }

    public static void Log(string message, Object asset)
    {
        Debug.Log(message, asset);
    }
}
