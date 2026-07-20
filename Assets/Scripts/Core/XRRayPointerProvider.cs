using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// XR pointer: a ray from the right-hand controller pose. Used when an XR
/// device is active; requires an enabled XR loader (e.g. OpenXR).
/// </summary>
public class XRRayPointerProvider : IPointerProvider
{
    public string ModeName => "XR (Controller Ray)";

    public bool TryGetPointerRay(out Ray ray)
    {
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
