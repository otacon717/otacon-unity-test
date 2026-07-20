using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small menu shown next to a placed prop, offering actions on it (delete).
/// The panel tracks the prop's position on screen while open.
/// </summary>
public class PropContextMenu : MonoBehaviour
{
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Text nameLabel;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Vector2 screenOffset = new Vector2(30f, 10f);
    [SerializeField] private float anchorHeight = 1.6f;

    private Camera mainCamera;
    private PlacedProp target;
    private RectTransform rect;

    private void Awake()
    {
        mainCamera = Camera.main;
        rect = (RectTransform)transform;
        deleteButton.onClick.AddListener(DeleteTarget);
        closeButton.onClick.AddListener(Close);
        gameObject.SetActive(false);
    }

    public bool IsOpen => gameObject.activeSelf;

    public void Open(PlacedProp prop)
    {
        target = prop;
        nameLabel.text = prop.DisplayName;
        gameObject.SetActive(true);
        UpdatePosition();
    }

    public void Close()
    {
        target = null;
        gameObject.SetActive(false);
    }

    private void DeleteTarget()
    {
        if (target != null)
        {
            Destroy(target.gameObject);
        }
        Close();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Close();
            return;
        }
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector3 world = target.transform.position + Vector3.up * anchorHeight;
        Vector3 screen = mainCamera.WorldToScreenPoint(world);
        if (screen.z < 0f)
        {
            Close();
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screen, null, out Vector2 local);
        rect.anchoredPosition = local + screenOffset;
    }
}
