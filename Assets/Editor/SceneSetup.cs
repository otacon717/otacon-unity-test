using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

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
        Camera desktopCamera = CreateCamera();
        CreateSurface();
        GameObject canvas = CreateUI();
        PropContextMenu contextMenu = CreateContextMenu();
        GameObject xrRig = CreateXRRig(out Transform rightRayOrigin);
        CreateGameController(contextMenu, desktopCamera, canvas, xrRig, rightRayOrigin);

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

    private static Camera CreateCamera()
    {
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        camGo.AddComponent<AudioListener>();
        cam.fieldOfView = 55f;
        camGo.transform.position = new Vector3(0f, 17f, -17f);
        camGo.transform.rotation = Quaternion.Euler(47f, 0f, 0f);
        return cam;
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

    private static GameObject CreateUI()
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
        // Lets XR ray interactors hit this canvas when it is world-space in VR.
        canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();
        canvasGo.AddComponent<CanvasFontApplier>();

        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<InputSystemUIInputModule>();
        // XR UI module is enabled by PlatformSwitcher only when a headset is active.
        eventSystemGo.AddComponent<XRUIInputModule>().enabled = false;

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

        return canvasGo;
    }

    private static PropContextMenu CreateContextMenu()
    {
        // The context menu lives on its own canvas so PlatformSwitcher can flip
        // just this canvas to world-space in VR while the desktop math stays put.
        var canvasGo = CreateUIObject("ContextMenuCanvas", null);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();
        canvasGo.AddComponent<CanvasFontApplier>();

        var menuGo = CreateUIObject("PropContextMenu", canvasGo.transform);
        var menuRect = (RectTransform)menuGo.transform;
        menuRect.anchorMin = new Vector2(0.5f, 0.5f);
        menuRect.anchorMax = new Vector2(0.5f, 0.5f);
        menuRect.pivot = new Vector2(0f, 0.5f);
        menuRect.sizeDelta = new Vector2(210f, 0f);

        menuGo.AddComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.96f);
        var layout = menuGo.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 6f;
        layout.padding = new RectOffset(10, 10, 10, 10);
        menuGo.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var nameGo = CreateUIObject("NameLabel", menuGo.transform);
        nameGo.AddComponent<LayoutElement>().preferredHeight = 34f;
        Text nameLabel = CreateText(nameGo.transform, "道具", 22, TextAnchor.MiddleCenter);

        Button deleteButton = CreateMenuButton(menuGo.transform, "DeleteButton", "刪除",
            new Color(0.75f, 0.25f, 0.25f, 1f), 44f);
        Button closeButton = CreateMenuButton(menuGo.transform, "CloseButton", "關閉",
            new Color(0.3f, 0.33f, 0.38f, 1f), 36f);

        var menu = menuGo.AddComponent<PropContextMenu>();
        var so = new SerializedObject(menu);
        so.FindProperty("canvasRect").objectReferenceValue = (RectTransform)canvasGo.transform;
        so.FindProperty("nameLabel").objectReferenceValue = nameLabel;
        so.FindProperty("deleteButton").objectReferenceValue = deleteButton;
        so.FindProperty("closeButton").objectReferenceValue = closeButton;
        so.ApplyModifiedPropertiesWithoutUndo();

        return menu;
    }

    private static Button CreateMenuButton(Transform parent, string name, string label,
        Color background, float height)
    {
        GameObject buttonGo = CreateUIObject(name, parent);
        var image = buttonGo.AddComponent<Image>();
        image.color = background;
        var button = buttonGo.AddComponent<Button>();
        button.targetGraphic = image;
        buttonGo.AddComponent<LayoutElement>().preferredHeight = height;
        CreateText(buttonGo.transform, label, 22, TextAnchor.MiddleCenter);
        return button;
    }

    private static GameObject CreateXRRig(out Transform rightRayOrigin)
    {
        rightRayOrigin = null;
        string[] guids = AssetDatabase.FindAssets("\"XR Origin (XR Rig)\" t:prefab");
        if (guids.Length == 0)
        {
            Debug.LogWarning("[SceneSetup] XR Origin prefab not found; skipping XR rig.");
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        rig.name = "XR Origin (XR Rig)";
        rig.transform.position = new Vector3(0f, 0.4f, -13.5f);
        rig.transform.rotation = Quaternion.identity;

        foreach (Transform t in rig.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Contains("Right") && t.name.Contains("Controller"))
            {
                rightRayOrigin = t;
                break;
            }
        }

        // Inactive by default; PlatformSwitcher activates it when a headset is present.
        rig.SetActive(false);
        return rig;
    }

    private static void CreateGameController(PropContextMenu contextMenu, Camera desktopCamera,
        GameObject mainCanvasGo, GameObject xrRig, Transform rightRayOrigin)
    {
        Material ghostValid = CreateTransparentMaterial(
            "Assets/Materials/GhostValid.mat", new Color(0.35f, 1f, 0.45f, 0.45f));
        Material ghostInvalid = CreateTransparentMaterial(
            "Assets/Materials/GhostInvalid.mat", new Color(1f, 0.32f, 0.32f, 0.45f));

        Material guideMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GuideLine.mat");
        if (guideMat == null)
        {
            guideMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(guideMat, "Assets/Materials/GuideLine.mat");
        }
        guideMat.color = new Color(0.25f, 0.9f, 1f, 1f);
        EditorUtility.SetDirty(guideMat);

        var controllerGo = new GameObject("GameController");
        var switcher = controllerGo.AddComponent<PlatformSwitcher>();
        var switcherSo = new SerializedObject(switcher);
        switcherSo.FindProperty("desktopCamera").objectReferenceValue = desktopCamera;
        switcherSo.FindProperty("xrRig").objectReferenceValue = xrRig;
        switcherSo.FindProperty("mainCanvas").objectReferenceValue = mainCanvasGo.GetComponent<Canvas>();
        switcherSo.FindProperty("menuCanvas").objectReferenceValue = contextMenu.GetComponentInParent<Canvas>(true);
        switcherSo.FindProperty("rightRayOrigin").objectReferenceValue = rightRayOrigin;
        switcherSo.ApplyModifiedPropertiesWithoutUndo();

        var drag = controllerGo.AddComponent<DragPlacementController>();
        var so = new SerializedObject(drag);
        so.FindProperty("ghostValidMaterial").objectReferenceValue = ghostValid;
        so.FindProperty("ghostInvalidMaterial").objectReferenceValue = ghostInvalid;
        so.FindProperty("guideMaterial").objectReferenceValue = guideMat;
        so.ApplyModifiedPropertiesWithoutUndo();

        var selection = controllerGo.AddComponent<PropSelectionController>();
        var selectionSo = new SerializedObject(selection);
        selectionSo.FindProperty("contextMenu").objectReferenceValue = contextMenu;
        selectionSo.ApplyModifiedPropertiesWithoutUndo();
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
