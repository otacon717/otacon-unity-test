using System.Collections.Generic;
using UnityEngine;

/// <summary>The full list of props offered by the dropdown UI.</summary>
[CreateAssetMenu(fileName = "PropCatalog", menuName = "Props/Prop Catalog")]
public class PropCatalog : ScriptableObject
{
    [SerializeField] private List<PropDefinition> props = new List<PropDefinition>();

    public IReadOnlyList<PropDefinition> Props => props;

    public void SetProps(List<PropDefinition> list)
    {
        props = list;
    }
}
