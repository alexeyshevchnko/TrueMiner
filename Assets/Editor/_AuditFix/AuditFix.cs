using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Batch-утилиты: снять missing-script компоненты с префабов/сцен и пересохранить сцены в тексте.</summary>
public static class AuditFix
{
    [MenuItem("Tools/Audit/Repair Missing Scripts & Resave Scenes")]
    public static void Repair()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        PlayerSettings.productName = "TrueMiner";

        int prefabModified = 0, prefabComponents = 0;
        foreach (string g in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            int removed = RemoveMissingRecursive(go);
            if (removed > 0)
            {
                prefabComponents += removed;
                PrefabUtility.SavePrefabAsset(go);
                prefabModified++;
            }
        }

        int sceneResaved = 0, sceneComponents = 0;
        foreach (string g in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int removed = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
                removed += RemoveMissingRecursive(root);
            sceneComponents += removed;
            EditorSceneManager.SaveScene(scene, path);
            sceneResaved++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log(string.Format(
            "[AuditFix] prefabs fixed={0} (removed {1} components); scenes resaved={2} (removed {3}). serializationMode={4}",
            prefabModified, prefabComponents, sceneResaved, sceneComponents, EditorSettings.serializationMode));
        EditorApplication.Exit(0);
    }

    static int RemoveMissingRecursive(GameObject go)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform t in go.transform)
            removed += RemoveMissingRecursive(t.gameObject);
        return removed;
    }

    [MenuItem("Tools/Audit/Resave Scenes As Text")]
    public static void ResaveScenesText() => Repair();

    [MenuItem("Tools/Audit/Fix PlayerSettings")]
    public static void FixPlayerSettings()
    {
        PlayerSettings.productName = "TrueMiner";
        EditorSettings.serializationMode = SerializationMode.ForceText;
        AssetDatabase.SaveAssets();
        Debug.Log("[AuditFix] productName = " + PlayerSettings.productName);
        EditorApplication.Exit(0);
    }
}