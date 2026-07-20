using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Turns drags that start on a prop list entry into a ghost preview that
/// follows the cursor, and spawns the real prop on the surface on release.
/// Releasing over UI or outside the surface cancels the placement.
/// </summary>
public class DragPlacementController : MonoBehaviour
{
    [SerializeField] private Material ghostValidMaterial;
    [SerializeField] private Material ghostInvalidMaterial;
    [SerializeField] private Material guideMaterial;
    [SerializeField] private float invalidHoverDistance = 15f;
    [SerializeField] private float hoverHeight = 2.4f;

    private Camera mainCamera;
    private PropDefinition draggedDefinition;
    private GameObject ghost;
    private PlacementGuide guide;
    private bool hasSurfaceHit;
    private Vector3 surfacePoint;

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

    private void OnEnable()
    {
        PropListItem.PropDragStarted += HandleDragStarted;
        PropListItem.PropDragMoved += HandleDragMoved;
        PropListItem.PropDragEnded += HandleDragEnded;
    }

    private void OnDisable()
    {
        PropListItem.PropDragStarted -= HandleDragStarted;
        PropListItem.PropDragMoved -= HandleDragMoved;
        PropListItem.PropDragEnded -= HandleDragEnded;
    }

    private void HandleDragStarted(PropDefinition definition, PointerEventData eventData)
    {
        CancelDrag();
        draggedDefinition = definition;
        ghost = Instantiate(definition.Prefab);
        ghost.name = $"{definition.name}_Ghost";
        foreach (Collider collider in ghost.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        UpdateGhost(eventData);
    }

    private void HandleDragMoved(PointerEventData eventData)
    {
        if (ghost != null)
        {
            UpdateGhost(eventData);
        }
    }

    private void HandleDragEnded(PointerEventData eventData)
    {
        if (draggedDefinition == null)
        {
            return;
        }

        UpdateGhost(eventData);
        if (!IsPointerOverUI(eventData) && hasSurfaceHit)
        {
            GameObject placed = Instantiate(draggedDefinition.Prefab, surfacePoint, Quaternion.identity);
            placed.name = draggedDefinition.name;
        }
        CancelDrag();
    }

    private void UpdateGhost(PointerEventData eventData)
    {
        // Pointing goes through the platform abstraction (mouse ray on desktop,
        // controller ray in XR); the event position is the desktop fallback.
        if (!PointerService.Current.TryGetPointerRay(out Ray ray))
        {
            ray = Cam.ScreenPointToRay(eventData.position);
        }
        hasSurfaceHit = Physics.Raycast(ray, out RaycastHit hit, 500f, GameLayers.SurfaceMask);
        if (hasSurfaceHit)
        {
            surfacePoint = hit.point;
        }

        bool valid = hasSurfaceHit && !IsPointerOverUI(eventData);

        // While a valid landing spot exists the ghost hovers above it and the
        // guide line points at the exact 3D position where it will drop.
        if (valid)
        {
            Vector3 hoverPosition = surfacePoint + Vector3.up * hoverHeight;
            ghost.transform.position = hoverPosition;
            Guide.Show(hoverPosition, surfacePoint);
        }
        else
        {
            ghost.transform.position = hasSurfaceHit ? surfacePoint : ray.GetPoint(invalidHoverDistance);
            Guide.Hide();
        }
        ApplyGhostMaterial(valid ? ghostValidMaterial : ghostInvalidMaterial);
    }

    private PlacementGuide Guide
    {
        get
        {
            if (guide == null)
            {
                guide = PlacementGuide.Create(guideMaterial);
            }
            return guide;
        }
    }

    private static bool IsPointerOverUI(PointerEventData eventData)
    {
        return eventData.pointerCurrentRaycast.gameObject != null;
    }

    private void ApplyGhostMaterial(Material material)
    {
        foreach (Renderer renderer in ghost.GetComponentsInChildren<Renderer>())
        {
            var materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }
            renderer.sharedMaterials = materials;
        }
    }

    private void CancelDrag()
    {
        if (ghost != null)
        {
            Destroy(ghost);
        }
        ghost = null;
        draggedDefinition = null;
        hasSurfaceHit = false;
        if (guide != null)
        {
            guide.Hide();
        }
    }
}
