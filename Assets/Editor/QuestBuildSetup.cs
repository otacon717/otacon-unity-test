using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;

/// <summary>
/// One-shot configuration for the Meta Quest standalone build: enables the
/// OpenXR Quest features and applies the required Android player settings.
/// Run from the menu or via -executeMethod QuestBuildSetup.Configure.
/// </summary>
public static class QuestBuildSetup
{
    [MenuItem("Tools/Setup/Configure Quest Build")]
    public static void Configure()
    {
        ConfigureOpenXRFeatures();
        ConfigureAndroidPlayerSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("[QuestBuildSetup] Quest build configuration applied.");
    }

    private static void ConfigureOpenXRFeatures()
    {
        FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
        OpenXRSettings settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
        if (settings == null)
        {
            Debug.LogError("[QuestBuildSetup] No OpenXR settings for Android.");
            return;
        }

        EnableFeature(settings.GetFeature<MetaQuestFeature>(), "Meta Quest Support");
        EnableFeature(settings.GetFeature<OculusTouchControllerProfile>(), "Oculus Touch Controller Profile");
        EditorUtility.SetDirty(settings);
    }

    private static void EnableFeature(OpenXRFeature feature, string label)
    {
        if (feature == null)
        {
            Debug.LogError($"[QuestBuildSetup] Feature not found: {label}");
            return;
        }
        feature.enabled = true;
        EditorUtility.SetDirty(feature);
        Debug.Log($"[QuestBuildSetup] Enabled: {label}");
    }

    private static void ConfigureAndroidPlayerSettings()
    {
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)29;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.otacon.unitytest");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
            new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });
        EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
    }
}
