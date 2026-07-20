using UnityEngine;

/// <summary>Attached to every prop instance placed in the scene.</summary>
public class PlacedProp : MonoBehaviour
{
    [SerializeField] private string displayName;

    public string DisplayName
    {
        get => displayName;
        set => displayName = value;
    }
}
