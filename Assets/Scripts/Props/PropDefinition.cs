using UnityEngine;

/// <summary>Describes one placeable prop: display name plus the prefab to spawn.</summary>
[CreateAssetMenu(fileName = "PropDefinition", menuName = "Props/Prop Definition")]
public class PropDefinition : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private GameObject prefab;

    public string DisplayName => displayName;
    public GameObject Prefab => prefab;

    public void Initialize(string name, GameObject prefabAsset)
    {
        displayName = name;
        prefab = prefabAsset;
    }
}
