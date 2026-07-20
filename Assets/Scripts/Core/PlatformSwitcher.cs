using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Configures the scene for the current platform at startup: desktop camera +
/// mouse pointer + overlay UI, or XR rig + controller ray + world-space UI
/// when an XR device is active. The rest of the game only talks to
/// PointerService and standard EventSystem events.
/// </summary>
public class PlatformSwitcher : MonoBehaviour
{
    [SerializeField] private Camera desktopCamera;
    [SerializeField] private GameObject xrRig;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Transform rightRayOrigin;

    public static bool XRActive { get; private set; }

    private void Awake()
    {
        XRActive = XRSettings.isDeviceActive;
        if (XRActive)
        {
            EnterVRMode();
        }
        else
        {
            EnterDesktopMode();
        }
        Debug.Log($"[PlatformSwitcher] XR device active: {XRActive} -> input mode: {PointerService.Current.ModeName}");
    }

    private void EnterDesktopMode()
    {
        PointerService.Current = new MousePointerProvider();
        if (xrRig != null)
        {
            xrRig.SetActive(false);
        }
        if (desktopCamera != null)
        {
            desktopCamera.gameObject.SetActive(true);
        }
        SetInputModule(useXR: false);
    }

    private void EnterVRMode()
    {
        PointerService.Current = new XRRayPointerProvider { RayOrigin = rightRayOrigin };

        if (desktopCamera != null)
        {
            desktopCamera.gameObject.SetActive(false);
        }
        if (xrRig != null)
        {
            xrRig.SetActive(true);
        }
        SetInputModule(useXR: true);

        Camera rigCamera = xrRig != null ? xrRig.GetComponentInChildren<Camera>(true) : null;

        // The main canvas becomes a world-space board; its final pose is set
        // once the headset pose is known (AlignRigWhenReady).
        if (mainCanvas != null)
        {
            mainCanvas.renderMode = RenderMode.WorldSpace;
            mainCanvas.worldCamera = rigCamera;
            var rect = (RectTransform)mainCanvas.transform;
            rect.sizeDelta = new Vector2(1920f, 1080f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one * 0.001f;
        }

        // The context menu gets its own small world-space canvas; the panel
        // positions itself in world space next to the prop when opened.
        if (menuCanvas != null)
        {
            menuCanvas.renderMode = RenderMode.WorldSpace;
            menuCanvas.worldCamera = rigCamera;
            menuCanvas.transform.position = Vector3.zero;
            menuCanvas.transform.rotation = Quaternion.identity;
            menuCanvas.transform.localScale = Vector3.one * 0.004f;
        }

        StartCoroutine(AlignRigWhenReady(rigCamera));
    }

    /// <summary>
    /// The headset's initial yaw comes from the real-world guardian, so once
    /// tracking is up we stand the rig on the surface and rotate it so the
    /// user faces the surface centre, then park the UI board in front of them.
    /// </summary>
    private System.Collections.IEnumerator AlignRigWhenReady(Camera rigCamera)
    {
        yield return new WaitForSeconds(1f);
        if (xrRig == null || rigCamera == null)
        {
            yield break;
        }

        // Stand on the surface (inside its bounds) instead of floating outside it.
        Vector3 anchor = new Vector3(0f, 0f, -8.5f);
        if (Physics.Raycast(anchor + Vector3.up * 60f, Vector3.down, out RaycastHit hit, 120f, GameLayers.SurfaceMask))
        {
            anchor.y = hit.point.y;
        }
        xrRig.transform.position = anchor;
        yield return null;

        Vector3 lookTarget = Vector3.zero; // surface centre
        Vector3 forward = rigCamera.transform.forward;
        forward.y = 0f;
        Vector3 toTarget = lookTarget - rigCamera.transform.position;
        toTarget.y = 0f;
        if (forward.sqrMagnitude > 0.001f && toTarget.sqrMagnitude > 0.001f)
        {
            float angle = Vector3.SignedAngle(forward, toTarget, Vector3.up);
            xrRig.transform.RotateAround(rigCamera.transform.position, Vector3.up, angle);
        }
        yield return null;

        PlaceBoardInFrontOfCamera(rigCamera);
        Debug.Log($"[PlatformSwitcher] Rig aligned. cam pos:{rigCamera.transform.position} yaw:{rigCamera.transform.eulerAngles.y:F0}");
    }

    private void PlaceBoardInFrontOfCamera(Camera rigCamera)
    {
        if (mainCanvas == null)
        {
            return;
        }
        Vector3 forward = rigCamera.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 right = new Vector3(forward.z, 0f, -forward.x);
        Vector3 pos = rigCamera.transform.position + forward * 1.8f + right * 0.8f;
        pos.y = rigCamera.transform.position.y;

        var rect = (RectTransform)mainCanvas.transform;
        rect.position = pos;
        // Canvas front faces -Z, so point +Z away from the camera.
        rect.rotation = Quaternion.LookRotation(pos - rigCamera.transform.position);
    }

    private void SetInputModule(bool useXR)
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include);
        if (eventSystem == null)
        {
            return;
        }
        var desktopModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        var xrModule = eventSystem.GetComponent<XRUIInputModule>();
        if (desktopModule != null)
        {
            desktopModule.enabled = !useXR;
        }
        if (xrModule != null)
        {
            xrModule.enabled = useXR;
        }
    }
}
