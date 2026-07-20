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

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
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

    private void HandleSceneClick(Vector2 screenPosition)
    {
        if (!PointerService.Current.TryGetPointerRay(out Ray ray))
        {
            ray = mainCamera.ScreenPointToRay(screenPosition);
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
