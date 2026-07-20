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

        // The main canvas becomes a world-space board beside the surface.
        if (mainCanvas != null)
        {
            mainCanvas.renderMode = RenderMode.WorldSpace;
            mainCanvas.worldCamera = rigCamera;
            var rect = (RectTransform)mainCanvas.transform;
            rect.sizeDelta = new Vector2(1920f, 1080f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.position = new Vector3(2.4f, 2.4f, -9.5f);
            rect.rotation = Quaternion.Euler(0f, 18f, 0f);
            rect.localScale = Vector3.one * 0.0015f;
        }

        // The context menu gets its own small world-space canvas.
        if (menuCanvas != null)
        {
            menuCanvas.renderMode = RenderMode.WorldSpace;
            menuCanvas.worldCamera = rigCamera;
            menuCanvas.transform.localScale = Vector3.one * 0.004f;
        }
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
