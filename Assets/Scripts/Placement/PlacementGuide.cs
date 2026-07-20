using UnityEngine;

/// <summary>
/// Visual guide shown while dragging a prop: a vertical line from the hovering
/// ghost down to the exact 3D landing point on the surface, plus a ring marker.
/// </summary>
public class PlacementGuide : MonoBehaviour
{
    private const int RingSegments = 40;
    private const float RingRadius = 0.55f;

    private LineRenderer line;
    private LineRenderer ring;

    public static PlacementGuide Create(Material material)
    {
        var go = new GameObject("PlacementGuide");
        var guide = go.AddComponent<PlacementGuide>();
        guide.line = CreateLine(go.transform, "Line", material, loop: false, positions: 2);
        guide.ring = CreateLine(go.transform, "Ring", material, loop: true, positions: RingSegments);
        go.SetActive(false);
        return guide;
    }

    private static LineRenderer CreateLine(Transform parent, string name, Material material,
        bool loop, int positions)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.sharedMaterial = material;
        lr.widthMultiplier = 0.055f;
        lr.loop = loop;
        lr.positionCount = positions;
        lr.useWorldSpace = true;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        return lr;
    }

    public void Show(Vector3 from, Vector3 landing)
    {
        gameObject.SetActive(true);
        line.SetPosition(0, from);
        line.SetPosition(1, landing);
        for (int i = 0; i < RingSegments; i++)
        {
            float angle = i / (float)RingSegments * Mathf.PI * 2f;
            ring.SetPosition(i, landing + new Vector3(
                Mathf.Cos(angle) * RingRadius, 0.04f, Mathf.Sin(angle) * RingRadius));
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
