using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// XR pointer: a ray from the active right-hand ray origin (set by
/// PlatformSwitcher from the rig) with a raw device-pose fallback.
/// </summary>
public class XRRayPointerProvider : IPointerProvider
{
    /// <summary>Transform of the rig's right-hand ray origin, if available.</summary>
    public Transform RayOrigin { get; set; }

    public string ModeName => "XR (Controller Ray)";

    public bool TryGetPointerRay(out Ray ray)
    {
        if (RayOrigin != null)
        {
            ray = new Ray(RayOrigin.position, RayOrigin.forward);
            return true;
        }

        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (device.isValid
            && device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position)
            && device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
        {
            ray = new Ray(position, rotation * Vector3.forward);
            return true;
        }
        ray = default;
        return false;
    }
}
