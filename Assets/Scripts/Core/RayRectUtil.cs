using UnityEngine;

/// <summary>Shared helper: does a world ray pass through a world-space RectTransform?</summary>
public static class RayRectUtil
{
    public static bool RayHitsRect(Ray ray, RectTransform rect)
    {
        if (rect == null || !rect.gameObject.activeInHierarchy)
        {
            return false;
        }
        var plane = new Plane(rect.forward, rect.position);
        if (!plane.Raycast(ray, out float enter))
        {
            return false;
        }
        Vector2 local = rect.InverseTransformPoint(ray.GetPoint(enter));
        return rect.rect.Contains(local);
    }
}
