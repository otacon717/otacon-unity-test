using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Windows standalone build entry point
/// (menu item or -executeMethod BuildScript.BuildWindows).
/// </summary>
public static class BuildScript
{
    private const string WindowsOutputPath = "Build/Windows/UnityTest.exe";
    private const string QuestOutputPath = "Build/Android/UnityTest.apk";

    [MenuItem("Tools/Build/Windows x64")]
    public static void BuildWindows()
    {
        Build(WindowsOutputPath, BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Tools/Build/Quest APK")]
    public static void BuildQuest()
    {
        Build(QuestOutputPath, BuildTarget.Android);
    }

    private static void Build(string outputPath, BuildTarget target)
    {
        var options = new BuildPlayerOptions
        {
            scenes = new[] { SceneSetup.ScenePath },
            locationPathName = outputPath,
            target = target,
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
