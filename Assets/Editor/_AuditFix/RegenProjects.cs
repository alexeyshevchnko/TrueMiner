using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Генерирует .sln/.csproj вручную через API пакета Visual Studio Editor.
/// Запуск: Unity.exe -batchmode -nographics -quit -projectPath "..." -executeMethod RegenProjects.Run
/// </summary>
public static class RegenProjects
{
    public static void Run()
    {
        Type t = null;
        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (a.GetName().Name == "Microsoft.Unity.VisualStudio.Editor")
            {
                t = a.GetType("Microsoft.Unity.VisualStudio.Editor.ProjectGeneration");
                if (t != null) break;
            }
        }

        if (t == null)
        {
            Debug.LogError("[RegenProjects] Microsoft.Unity.VisualStudio.Editor.ProjectGeneration not found");
            EditorApplication.Exit(3);
            return;
        }

        object instance = Activator.CreateInstance(t);
        MethodInfo sync = t.GetMethod("Sync", BindingFlags.Instance | BindingFlags.Public);
        if (sync == null)
        {
            Debug.LogError("[RegenProjects] Sync() method not found");
            EditorApplication.Exit(3);
            return;
        }

        sync.Invoke(instance, null);
        Debug.Log("[RegenProjects] Sync() completed");
        EditorApplication.Exit(0);
    }
}