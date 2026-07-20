using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Windows standalone build entry point
/// (menu item or -executeMethod BuildScript.BuildWindows).
/// </summary>
public static class BuildScript
{
    private const string OutputPath = "Build/Windows/UnityTest.exe";

    [MenuItem("Tools/Build/Windows x64")]
    public static void BuildWindows()
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[] { SceneSetup.ScenePath },
            locationPathName = OutputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log($"[BuildScript] Result: {summary.result}, output: {summary.outputPath}, " +
                  $"size: {summary.totalSize / (1024 * 1024)} MB, errors: {summary.totalErrors}");

        if (Application.isBatchMode && summary.result != BuildResult.Succeeded)
        {
            EditorApplication.Exit(1);
        }
    }
}
