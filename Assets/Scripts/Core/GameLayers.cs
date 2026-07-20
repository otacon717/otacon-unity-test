using UnityEngine;

/// <summary>Central definition of the physics layers used by the game.</summary>
public static class GameLayers
{
    public const string Surface = "Surface";
    public const string Prop = "Prop";

    public static int SurfaceMask => LayerMask.GetMask(Surface);
    public static int PropMask => LayerMask.GetMask(Prop);

    public static void SetLayerRecursively(GameObject root, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            t.gameObject.layer = layer;
        }
    }
}
