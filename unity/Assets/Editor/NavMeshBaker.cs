using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

/// <summary>
/// NavMesh baker for The Hush scene.
/// Two entry points:
///   • Lumen → Bake NavMesh (TheHush) — interactive in Unity editor menu
///   • Bake() static method — invoked from CLI:
///       Unity -batchmode -quit -projectPath . -executeMethod NavMeshBaker.Bake
///
/// Walks every NavMeshSurface in the open scene, calls BuildNavMesh() on each,
/// then saves the scene so the baked NavMeshData asset persists.
/// </summary>
public static class NavMeshBaker
{
    [MenuItem("Lumen/Bake NavMesh (TheHush)")]
    public static void Bake()
    {
        const string scenePath = "Assets/Scenes/TheHush.unity";

        var active = EditorSceneManager.GetActiveScene();
        if (active.path != scenePath)
        {
            Debug.Log($"[NavMeshBaker] Opening {scenePath} (was: '{active.path}')");
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }

        var surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
        if (surfaces.Length == 0)
        {
            Debug.LogError("[NavMeshBaker] No NavMeshSurface components found in scene — nothing to bake.");
            EditorApplication.Exit(2);
            return;
        }

        Debug.Log($"[NavMeshBaker] Baking {surfaces.Length} NavMeshSurface(s)...");
        foreach (var surface in surfaces)
        {
            Debug.Log($"[NavMeshBaker] → Surface on '{surface.gameObject.name}' (agentTypeID={surface.agentTypeID})");
            surface.BuildNavMesh();
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("[NavMeshBaker] Bake complete; scene saved.");

        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
