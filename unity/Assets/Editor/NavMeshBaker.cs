using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;

/// <summary>
/// NavMesh baker for The Hush scene.
///
/// Two entry points:
///   • Lumen → Bake NavMesh (TheHush) — interactive in Unity editor menu
///   • NavMeshBaker.Bake — invoked from CLI:
///       Unity -batchmode -nographics -quit -projectPath . -executeMethod NavMeshBaker.Bake
///
/// Walks every NavMeshSurface in the open scene, calls BuildNavMesh() on each,
/// then *saves the produced NavMeshData out as a sibling .asset file* so the scene
/// file does not have to absorb a binary blob (which would force binary serialization
/// of the entire scene under some Unity versions).
/// </summary>
public static class NavMeshBaker
{
    [MenuItem("Lumen/Bake NavMesh (TheHush)")]
    public static void Bake()
    {
        const string scenePath = "Assets/Scenes/TheHush.unity";
        const string dataDir   = "Assets/Scenes/TheHush";

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

        if (!AssetDatabase.IsValidFolder(dataDir))
        {
            AssetDatabase.CreateFolder("Assets/Scenes", "TheHush");
        }

        Debug.Log($"[NavMeshBaker] Baking {surfaces.Length} NavMeshSurface(s)...");
        for (int i = 0; i < surfaces.Length; i++)
        {
            var surface = surfaces[i];
            Debug.Log($"[NavMeshBaker] → Surface on '{surface.gameObject.name}' (agentTypeID={surface.agentTypeID})");
            surface.BuildNavMesh();

            // Save the produced NavMeshData out to its own asset so it does NOT
            // get inlined into the scene YAML (which can force binary serialization).
            var data = surface.navMeshData;
            if (data != null)
            {
                string assetPath = $"{dataDir}/NavMesh-{surface.gameObject.name}-{i}.asset";
                if (!AssetDatabase.Contains(data))
                {
                    AssetDatabase.CreateAsset(data, assetPath);
                    Debug.Log($"[NavMeshBaker]    saved NavMeshData → {assetPath}");
                }
                EditorUtility.SetDirty(surface);
            }
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[NavMeshBaker] Bake complete; scene + NavMeshData asset(s) saved.");

        if (Application.isBatchMode) EditorApplication.Exit(0);
    }
}
