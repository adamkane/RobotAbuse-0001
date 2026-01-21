using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

/// <summary>
/// Build script for creating WebGL builds via CLI.
/// </summary>
public class BuildScript
{
    [MenuItem("Build/WebGL Build")]
    public static void BuildWebGL()
    {
        Debug.Log("=== Starting WebGL Build ===");

        // Ensure build directory exists
        string buildPath = "Builds/WebGL";
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        // Get scenes from build settings
        string[] scenes = GetBuildScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes in build settings! Run Tools/Setup Main Scene first.");
            return;
        }

        Debug.Log("Building scenes: " + string.Join(", ", scenes));

        // Configure build options
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        // Execute build
        BuildReport report = BuildPipeline.BuildPlayer(options);

        // Report results
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("=== WebGL Build Succeeded ===");
            Debug.Log("Output: " + buildPath);
            Debug.Log("Total size: " + report.summary.totalSize + " bytes");
            Debug.Log("Build time: " + report.summary.totalTime);
        }
        else
        {
            Debug.LogError("=== WebGL Build Failed ===");
            Debug.LogError("Result: " + report.summary.result);

            foreach (var step in report.steps)
            {
                foreach (var message in step.messages)
                {
                    if (message.type == LogType.Error || message.type == LogType.Warning)
                    {
                        Debug.LogError(message.content);
                    }
                }
            }
        }
    }

    static string[] GetBuildScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var result = new System.Collections.Generic.List<string>();

        foreach (var scene in scenes)
        {
            if (scene.enabled)
            {
                result.Add(scene.path);
            }
        }

        return result.ToArray();
    }
}
