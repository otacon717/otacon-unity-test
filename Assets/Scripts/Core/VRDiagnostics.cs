using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

/// <summary>
/// Logs the runtime state of key scene objects shortly after startup so
/// headset-only issues can be diagnosed from logcat.
/// </summary>
public class VRDiagnostics : MonoBehaviour
{
    private float nextReport = 3f;
    private int reportsLeft = 2;

    private void Update()
    {
        if (reportsLeft <= 0 || Time.time < nextReport)
        {
            return;
        }
        reportsLeft--;
        nextReport = Time.time + 5f;
        Report();
    }

    private void Report()
    {
        var sb = new StringBuilder();
        sb.AppendLine("===== VRDiagnostics =====");
        sb.AppendLine($"XR active:{XRSettings.isDeviceActive} device:{XRSettings.loadedDeviceName} eyeTex:{XRSettings.eyeTextureWidth}x{XRSettings.eyeTextureHeight}");

        Camera cam = Camera.main;
        if (cam == null)
        {
            sb.AppendLine("Camera.main: NULL");
        }
        else
        {
            sb.AppendLine($"Camera '{cam.name}' pos:{cam.transform.position} euler:{cam.transform.eulerAngles} " +
                          $"near:{cam.nearClipPlane} far:{cam.farClipPlane} mask:{cam.cullingMask:X} enabled:{cam.enabled} targetTex:{cam.targetTexture}");
        }

        var surface = FindFirstObjectByType<CurvedSurface>();
        if (surface == null)
        {
            sb.AppendLine("CurvedSurface: NULL");
        }
        else
        {
            var renderer = surface.GetComponent<MeshRenderer>();
            var filter = surface.GetComponent<MeshFilter>();
            Material mat = renderer.sharedMaterial;
            sb.AppendLine($"Surface verts:{(filter.sharedMesh != null ? filter.sharedMesh.vertexCount : -1)} " +
                          $"bounds:{renderer.bounds} visible:{renderer.isVisible} enabled:{renderer.enabled} " +
                          $"mat:{(mat != null ? mat.name : "null")} shader:{(mat != null ? mat.shader.name : "null")} supported:{(mat != null && mat.shader.isSupported)}");
        }

        sb.AppendLine($"Skybox:{(RenderSettings.skybox != null ? RenderSettings.skybox.shader.name : "null")} supported:{(RenderSettings.skybox != null && RenderSettings.skybox.shader.isSupported)}");

        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            var rect = (RectTransform)canvas.transform;
            sb.AppendLine($"Canvas '{canvas.name}' mode:{canvas.renderMode} pos:{rect.position} scale:{rect.lossyScale} " +
                          $"size:{rect.sizeDelta} worldCam:{(canvas.worldCamera != null ? canvas.worldCamera.name : "null")} " +
                          $"enabled:{canvas.enabled} activeSelf:{canvas.gameObject.activeInHierarchy} childCount:{canvas.transform.childCount}");
        }

        int shown = 0;
        foreach (Renderer r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (r.GetComponentInParent<CurvedSurface>() != null || shown >= 12)
            {
                continue;
            }
            shown++;
            Material m = r.sharedMaterial;
            sb.AppendLine($"Renderer '{r.transform.root.name}/{r.name}' visible:{r.isVisible} " +
                          $"shader:{(m != null && m.shader != null ? m.shader.name : "null")} supported:{(m != null && m.shader != null && m.shader.isSupported)}");
        }

        Font font = UIFontProvider.Font;
        sb.AppendLine($"UI font:{(font != null ? font.name : "NULL")}");
        Debug.Log(sb.ToString());
    }
}
