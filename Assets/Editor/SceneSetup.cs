using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Generates the main scene from code so the whole setup is reproducible
/// (run from the menu or via -executeMethod SceneSetup.Generate in batch mode).
/// </summary>
public static class SceneSetup
{
    public const string ScenePath = "Assets/Scenes/Main.unity";
    private const string SurfaceMaterialPath = "Assets/Materials/Surface.mat";

    [MenuItem("Tools/Setup/Generate Main Scene")]
    public static void Generate()
    {
        EnsureLayers();
        EnsureFolders();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateLighting();
        CreateCamera();
        CreateSurface();

        EditorSceneManager.SaveScene(scene, ScenePath);
        SetBuildScenes();
        AssetDatabase.SaveAssets();
        Debug.Log($"[SceneSetup] Scene generated at {ScenePath}");
    }

    private static void EnsureLayers()
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        layers.GetArrayElementAtIndex(6).stringValue = GameLayers.Surface;
        layers.GetArrayElementAtIndex(7).stringValue = GameLayers.Prop;
        tagManager.ApplyModifiedProperties();
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Materials");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Data");
    }

    public static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string leaf = System.IO.Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }

    private static void CreateLighting()
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.96f, 0.88f);
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

        RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
    }

    private static void CreateCamera()
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        camGo.AddComponent<AudioListener>();
        cam.fieldOfView = 55f;
        camGo.transform.position = new Vector3(0f, 17f, -17f);
        camGo.transform.rotation = Quaternion.Euler(47f, 0f, 0f);
    }

    private static void CreateSurface()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(SurfaceMaterialPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.42f, 0.6f, 0.38f)
            };
            mat.SetFloat("_Smoothness", 0.15f);
            AssetDatabase.CreateAsset(mat, SurfaceMaterialPath);
        }

        var surfaceGo = new GameObject("CurvedSurface");
        surfaceGo.layer = LayerMask.NameToLayer(GameLayers.Surface);
        // The mesh itself is built at runtime in Awake so the scene file stays
        // lightweight; only the component configuration is serialized here.
        surfaceGo.AddComponent<CurvedSurface>();
        surfaceGo.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }

    private static void SetBuildScenes()
    {
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }
}
