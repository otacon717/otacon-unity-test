using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        CreateUI();
        CreateGameController();

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

    private static void CreateUI()
    {
        // Canvas scaled with screen size so the layout adapts to any aspect ratio.
        var canvasGo = CreateUIObject("UICanvas", null);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<CanvasFontApplier>();

        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<InputSystemUIInputModule>();

        // Right-hand prop dropdown panel.
        var panelGo = CreateUIObject("PropDropdownPanel", canvasGo.transform);
        var panelRect = (RectTransform)panelGo.transform;
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-24f, -24f);
        panelRect.sizeDelta = new Vector2(280f, 0f);
        var panelLayout = panelGo.AddComponent<VerticalLayoutGroup>();
        panelLayout.childControlWidth = true;
        panelLayout.childControlHeight = true;
        panelLayout.childForceExpandHeight = false;
        panelLayout.spacing = 4f;
        panelGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var headerGo = CreateUIObject("Header", panelGo.transform);
        var headerImage = headerGo.AddComponent<Image>();
        headerImage.color = new Color(0.16f, 0.2f, 0.26f, 0.96f);
        var headerButton = headerGo.AddComponent<Button>();
        headerButton.targetGraphic = headerImage;
        headerGo.AddComponent<LayoutElement>().preferredHeight = 56f;
        Text headerLabel = CreateText(headerGo.transform, "道具 ▼", 26, TextAnchor.MiddleCenter);

        var contentGo = CreateUIObject("Content", panelGo.transform);
        var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.spacing = 2f;
        contentLayout.padding = new RectOffset(4, 4, 4, 4);
        contentGo.AddComponent<Image>().color = new Color(0.1f, 0.12f, 0.15f, 0.9f);

        var panel = panelGo.AddComponent<PropDropdownPanel>();
        var so = new SerializedObject(panel);
        so.FindProperty("catalog").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<PropCatalog>(PropAssetGenerator.CatalogPath);
        so.FindProperty("headerButton").objectReferenceValue = headerButton;
        so.FindProperty("headerLabel").objectReferenceValue = headerLabel;
        so.FindProperty("contentRoot").objectReferenceValue = (RectTransform)contentGo.transform;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Small usage hint, bottom-left.
        var hintGo = CreateUIObject("HintLabel", canvasGo.transform);
        var hintRect = (RectTransform)hintGo.transform;
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(0f, 0f);
        hintRect.pivot = new Vector2(0f, 0f);
        hintRect.anchoredPosition = new Vector2(24f, 18f);
        hintRect.sizeDelta = new Vector2(760f, 40f);
        Text hint = CreateText(hintGo.transform, "從右側「道具」清單拖曳道具到曲面上放置；點選已放置的道具可開啟互動選單", 20, TextAnchor.MiddleLeft);
        hint.color = new Color(1f, 1f, 1f, 0.75f);
    }

    private static void CreateGameController()
    {
        Material ghostValid = CreateTransparentMaterial(
            "Assets/Materials/GhostValid.mat", new Color(0.35f, 1f, 0.45f, 0.45f));
        Material ghostInvalid = CreateTransparentMaterial(
            "Assets/Materials/GhostInvalid.mat", new Color(1f, 0.32f, 0.32f, 0.45f));

        var controllerGo = new GameObject("GameController");
        var drag = controllerGo.AddComponent<DragPlacementController>();
        var so = new SerializedObject(drag);
        so.FindProperty("ghostValidMaterial").objectReferenceValue = ghostValid;
        so.FindProperty("ghostInvalidMaterial").objectReferenceValue = ghostInvalid;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Material CreateTransparentMaterial(string path, Color color)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        if (parent != null)
        {
            go.transform.SetParent(parent, false);
        }
        return go;
    }

    private static Text CreateText(Transform parent, string content, int size, TextAnchor anchor)
    {
        GameObject go = CreateUIObject("Label", parent);
        var rect = (RectTransform)go.transform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(10f, 0f);
        rect.offsetMax = new Vector2(-10f, 0f);
        var text = go.AddComponent<Text>();
        // Placeholder font at edit time; UIFontProvider swaps to an OS font
        // with CJK coverage at runtime.
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = content;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static void SetBuildScenes()
    {
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
    }
}
