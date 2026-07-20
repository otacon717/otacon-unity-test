using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates the prop prefabs, their materials and the catalog assets from code
/// (run from the menu or via -executeMethod PropAssetGenerator.Generate).
/// </summary>
public static class PropAssetGenerator
{
    private const string PrefabFolder = "Assets/Prefabs";
    private const string MaterialFolder = "Assets/Materials/Props";
    private const string DataFolder = "Assets/Data";
    public const string CatalogPath = DataFolder + "/PropCatalog.asset";

    [MenuItem("Tools/Setup/Generate Prop Assets")]
    public static void Generate()
    {
        SceneSetup.EnsureFolder(PrefabFolder);
        SceneSetup.EnsureFolder(MaterialFolder);
        SceneSetup.EnsureFolder(DataFolder + "/Props");

        var definitions = new List<PropDefinition>
        {
            BuildProp("紅色方塊", "RedCube", BuildCube),
            BuildProp("藍色圓球", "BlueSphere", BuildSphere),
            BuildProp("黃色圓柱", "YellowCylinder", BuildCylinder),
            BuildProp("紫色膠囊", "PurpleCapsule", BuildCapsule),
            BuildProp("綠色小樹", "GreenTree", BuildTree),
            BuildProp("灰色拱門", "GreyArch", BuildArch),
        };

        PropCatalog catalog = AssetDatabase.LoadAssetAtPath<PropCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<PropCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }
        catalog.SetProps(definitions);
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log($"[PropAssetGenerator] Generated {definitions.Count} props.");
    }

    private static PropDefinition BuildProp(string displayName, string assetName,
        System.Func<GameObject, GameObject> buildRoot)
    {
        var root = new GameObject(assetName);
        try
        {
            buildRoot(root);
            foreach (Collider c in root.GetComponentsInChildren<Collider>())
            {
                c.gameObject.layer = 0; // reset, layer applied recursively below
            }
            GameLayers.SetLayerRecursively(root, GameLayers.Prop);
            root.AddComponent<PlacedProp>().DisplayName = displayName;

            string prefabPath = $"{PrefabFolder}/{assetName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            string defPath = $"{DataFolder}/Props/{assetName}.asset";
            PropDefinition def = AssetDatabase.LoadAssetAtPath<PropDefinition>(defPath);
            if (def == null)
            {
                def = ScriptableObject.CreateInstance<PropDefinition>();
                AssetDatabase.CreateAsset(def, defPath);
            }
            def.Initialize(displayName, prefab);
            EditorUtility.SetDirty(def);
            return def;
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    // --- Shape builders (root pivot sits at the bottom of each prop) ---

    private static GameObject BuildCube(GameObject root)
    {
        AddPrimitive(root, PrimitiveType.Cube, "Body",
            new Vector3(0f, 0.6f, 0f), new Vector3(1.2f, 1.2f, 1.2f), Color("#D14B4B"));
        return root;
    }

    private static GameObject BuildSphere(GameObject root)
    {
        AddPrimitive(root, PrimitiveType.Sphere, "Body",
            new Vector3(0f, 0.65f, 0f), new Vector3(1.3f, 1.3f, 1.3f), Color("#3F76C9"));
        return root;
    }

    private static GameObject BuildCylinder(GameObject root)
    {
        AddPrimitive(root, PrimitiveType.Cylinder, "Body",
            new Vector3(0f, 0.8f, 0f), new Vector3(0.9f, 0.8f, 0.9f), Color("#E0B93C"));
        return root;
    }

    private static GameObject BuildCapsule(GameObject root)
    {
        AddPrimitive(root, PrimitiveType.Capsule, "Body",
            new Vector3(0f, 1f, 0f), new Vector3(0.9f, 1f, 0.9f), Color("#8C57C9"));
        return root;
    }

    private static GameObject BuildTree(GameObject root)
    {
        Material trunk = GetMaterial("TreeTrunk", Color("#7A5230"));
        Material leaves = GetMaterial("TreeLeaves", Color("#3E8C3E"));
        AddPrimitive(root, PrimitiveType.Cylinder, "Trunk",
            new Vector3(0f, 0.5f, 0f), new Vector3(0.3f, 0.5f, 0.3f), trunk);
        AddPrimitive(root, PrimitiveType.Sphere, "LeavesLow",
            new Vector3(0f, 1.55f, 0f), new Vector3(1.45f, 1.35f, 1.45f), leaves);
        AddPrimitive(root, PrimitiveType.Sphere, "LeavesTop",
            new Vector3(0f, 2.35f, 0f), new Vector3(0.9f, 0.85f, 0.9f), leaves);
        return root;
    }

    private static GameObject BuildArch(GameObject root)
    {
        Material stone = GetMaterial("ArchStone", Color("#9AA0A8"));
        AddPrimitive(root, PrimitiveType.Cube, "PillarLeft",
            new Vector3(-0.65f, 0.8f, 0f), new Vector3(0.4f, 1.6f, 0.4f), stone);
        AddPrimitive(root, PrimitiveType.Cube, "PillarRight",
            new Vector3(0.65f, 0.8f, 0f), new Vector3(0.4f, 1.6f, 0.4f), stone);
        AddPrimitive(root, PrimitiveType.Cube, "Beam",
            new Vector3(0f, 1.8f, 0f), new Vector3(1.7f, 0.4f, 0.5f), stone);
        return root;
    }

    // --- Helpers ---

    private static void AddPrimitive(GameObject root, PrimitiveType type, string name,
        Vector3 localPos, Vector3 localScale, Color color)
    {
        AddPrimitive(root, type, name, localPos, localScale,
            GetMaterial(root.name + name, color));
    }

    private static void AddPrimitive(GameObject root, PrimitiveType type, string name,
        Vector3 localPos, Vector3 localScale, Material material)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = name;
        part.transform.SetParent(root.transform, false);
        part.transform.localPosition = localPos;
        part.transform.localScale = localScale;
        part.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static Material GetMaterial(string name, Color color)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        mat.SetFloat("_Smoothness", 0.35f);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Color Color(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
