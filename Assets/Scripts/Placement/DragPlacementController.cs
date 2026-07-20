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
    [SerializeField] private float invalidHoverDistance = 15f;

    private Camera mainCamera;
    private PropDefinition draggedDefinition;
    private GameObject ghost;
    private bool hasSurfaceHit;
    private Vector3 surfacePoint;

    private void Awake()
    {
        mainCamera = Camera.main;
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
        Ray ray = mainCamera.ScreenPointToRay(eventData.position);
        hasSurfaceHit = Physics.Raycast(ray, out RaycastHit hit, 500f, GameLayers.SurfaceMask);
        if (hasSurfaceHit)
        {
            surfacePoint = hit.point;
        }

        bool valid = hasSurfaceHit && !IsPointerOverUI(eventData);
        ghost.transform.position = hasSurfaceHit ? surfacePoint : ray.GetPoint(invalidHoverDistance);
        ApplyGhostMaterial(valid ? ghostValidMaterial : ghostInvalidMaterial);
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
    }
}
