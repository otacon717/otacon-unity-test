using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Right-hand side prop dropdown: a header button that expands/collapses the
/// list of draggable prop entries built from the catalog.
/// </summary>
public class PropDropdownPanel : MonoBehaviour
{
    [SerializeField] private PropCatalog catalog;
    [SerializeField] private Button headerButton;
    [SerializeField] private Text headerLabel;
    [SerializeField] private RectTransform contentRoot;

    private bool expanded;

    private void Awake()
    {
        UIFontProvider.Apply(gameObject);
        headerButton.onClick.AddListener(Toggle);
        BuildItems();
        SetExpanded(false);
    }

    private void BuildItems()
    {
        if (catalog == null)
        {
            Debug.LogWarning("[PropDropdownPanel] No catalog assigned.");
            return;
        }

        foreach (PropDefinition def in catalog.Props)
        {
            CreateItem(def);
        }
    }

    private void CreateItem(PropDefinition def)
    {
        var itemGo = new GameObject($"Item_{def.name}", typeof(RectTransform));
        itemGo.transform.SetParent(contentRoot, false);

        Image bg = itemGo.AddComponent<Image>();
        bg.color = new Color(0.22f, 0.25f, 0.3f, 0.95f);

        LayoutElement layout = itemGo.AddComponent<LayoutElement>();
        layout.preferredHeight = 46f;

        var labelGo = new GameObject("Label", typeof(RectTransform));
        labelGo.transform.SetParent(itemGo.transform, false);
        var labelRect = (RectTransform)labelGo.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(14f, 0f);
        labelRect.offsetMax = new Vector2(-8f, 0f);

        Text label = labelGo.AddComponent<Text>();
        label.font = UIFontProvider.Font;
        label.fontSize = 22;
        label.alignment = TextAnchor.MiddleLeft;
        label.color = Color.white;
        label.text = def.DisplayName;
        label.raycastTarget = false;

        itemGo.AddComponent<PropListItem>().Bind(def);
    }

    private void Toggle()
    {
        SetExpanded(!expanded);
    }

    private void SetExpanded(bool value)
    {
        expanded = value;
        contentRoot.gameObject.SetActive(expanded);
        if (headerLabel != null)
        {
            headerLabel.text = expanded ? "道具 ▲" : "道具 ▼";
        }
    }
}
