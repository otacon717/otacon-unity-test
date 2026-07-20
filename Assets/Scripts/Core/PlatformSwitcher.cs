using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Picks the pointer provider for the current platform at startup:
/// controller ray when an XR device is active, otherwise mouse.
/// The rest of the game only talks to PointerService.
/// </summary>
public class PlatformSwitcher : MonoBehaviour
{
    private void Awake()
    {
        bool xrActive = XRSettings.isDeviceActive;
        PointerService.Current = xrActive
            ? (IPointerProvider)new XRRayPointerProvider()
            : new MousePointerProvider();
        Debug.Log($"[PlatformSwitcher] XR device active: {xrActive} -> input mode: {PointerService.Current.ModeName}");
    }
}
