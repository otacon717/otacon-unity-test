using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Detects clicks (press + release without dragging) on placed props and
/// opens the context menu next to the clicked prop.
/// </summary>
public class PropSelectionController : MonoBehaviour
{
    [SerializeField] private PropContextMenu contextMenu;
    [SerializeField] private float clickMoveTolerance = 8f;

    private Camera mainCamera;
    private Vector2 pressPosition;
    private bool pressStartedOverUI;

    private Camera Cam
    {
        get
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            return mainCamera;
        }
    }

    private void Update()
    {
        if (PlatformSwitcher.XRActive)
        {
            UpdateVR();
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            pressPosition = mouse.position.ReadValue();
            pressStartedOverUI = IsPointerOverUI();
        }

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            Vector2 releasePosition = mouse.position.ReadValue();
            bool isClick = (releasePosition - pressPosition).magnitude <= clickMoveTolerance;
            if (!isClick || pressStartedOverUI || IsPointerOverUI())
            {
                return; // UI interactions and drags are handled elsewhere
            }

            HandleSceneClick(releasePosition);
        }
    }

    /// <summary>
    /// VR selection: a trigger press aims the controller ray at a placed prop
    /// to open the menu. Presses consumed by placement are skipped, and while
    /// aiming at the open menu the press is left to the UI buttons.
    /// </summary>
    private void UpdateVR()
    {
        if (!XRControllerInput.RightTriggerDown
            || Time.frameCount == DragPlacementController.LastVRActionFrame)
        {
            return;
        }
        var placement = GetComponent<DragPlacementController>();
        if (placement != null && placement.PlacementActive)
        {
            return;
        }
        if (!PointerService.Current.TryGetPointerRay(out Ray ray))
        {
            return;
        }
        if (contextMenu.IsOpen && RayRectUtil.RayHitsRect(ray, (RectTransform)contextMenu.transform))
        {
            return; // let the menu's own buttons handle this press
        }

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, GameLayers.PropMask))
        {
            PlacedProp prop = hit.collider.GetComponentInParent<PlacedProp>();
            if (prop != null)
            {
                contextMenu.Open(prop);
                return;
            }
        }
        contextMenu.Close();
    }

    private void HandleSceneClick(Vector2 screenPosition)
    {
        if (!PointerService.Current.TryGetPointerRay(out Ray ray))
        {
            ray = Cam.ScreenPointToRay(screenPosition);
        }
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, GameLayers.PropMask))
        {
            PlacedProp prop = hit.collider.GetComponentInParent<PlacedProp>();
            if (prop != null)
            {
                contextMenu.Open(prop);
                return;
            }
        }
        contextMenu.Close();
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
