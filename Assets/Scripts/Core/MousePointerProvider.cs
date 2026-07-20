using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Desktop pointer: a ray from the camera through the mouse cursor.</summary>
public class MousePointerProvider : IPointerProvider
{
    public string ModeName => "Desktop (Mouse)";

    public bool TryGetPointerRay(out Ray ray)
    {
        Camera cam = Camera.main;
        Mouse mouse = Mouse.current;
        if (cam == null || mouse == null)
        {
            ray = default;
            return false;
        }
        ray = cam.ScreenPointToRay(mouse.position.ReadValue());
        return true;
    }
}
