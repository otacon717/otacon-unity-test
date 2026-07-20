using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Attached to every prop instance placed in the scene. In XR mode the prop
/// registers itself as an interactable so a controller trigger-select opens
/// the context menu.
/// </summary>
public class PlacedProp : MonoBehaviour
{
    [SerializeField] private string displayName;

    public string DisplayName
    {
        get => displayName;
        set => displayName = value;
    }

    private void Awake()
    {
        if (PlatformSwitcher.XRActive)
        {
            var interactable = gameObject.AddComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(_ =>
            {
                if (PropContextMenu.Instance != null)
                {
                    PropContextMenu.Instance.Open(this);
                }
            });
        }
    }
}
