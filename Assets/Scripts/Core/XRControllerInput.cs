using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Frame-cached edge detection for the right-hand trigger, shared by the
/// placement and selection controllers.
/// </summary>
public static class XRControllerInput
{
    private static int lastFrame = -1;
    private static bool previous;
    private static bool downThisFrame;

    public static bool RightTriggerDown
    {
        get
        {
            Refresh();
            return downThisFrame;
        }
    }

    private static void Refresh()
    {
        if (Time.frameCount == lastFrame)
        {
            return;
        }
        lastFrame = Time.frameCount;
        InputDevice device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        bool current = device.isValid
            && device.TryGetFeatureValue(CommonUsages.triggerButton, out bool pressed)
            && pressed;
        downThisFrame = current && !previous;
        previous = current;
    }
}
