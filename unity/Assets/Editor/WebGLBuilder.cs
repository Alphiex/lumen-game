using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// WebGL build entry point.
///   • Build → Build WebGL menu item — interactive in editor.
///   • WebGLBuilder.BuildWebGL — invoked from CLI:
///       Unity -batchmode -quit -projectPath unity \
///             -buildTarget WebGL -executeMethod WebGLBuilder.BuildWebGL
///
/// Output goes to <project-root>/_builds/webgl (sibling of unity/), which is
/// gitignored. Uses TheHush as the only included scene.
/// </summary>
public class WebGLBuilder
{
    [MenuItem("Build/Build WebGL")]
    public static void BuildWebGL()
    {
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "../_builds/webgl");
        Directory.CreateDirectory(outputPath);

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/TheHush.unity" },
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("WebGL build succeeded: " + outputPath);
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError("WebGL build FAILED: " + report.summary.result);
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }
}
