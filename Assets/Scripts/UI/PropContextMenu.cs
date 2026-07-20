using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Small menu shown next to a placed prop, offering actions on it (delete).
/// Desktop: anchored in its overlay canvas, tracking the prop on screen.
/// VR: the parent canvas is world-space; the menu floats above the prop and
/// billboards toward the headset camera.
/// </summary>
public class PropContextMenu : MonoBehaviour
{
    public static PropContextMenu Instance { get; private set; }

    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Text nameLabel;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Vector2 screenOffset = new Vector2(30f, 10f);
    [SerializeField] private float anchorHeight = 1.6f;
    [SerializeField] private float worldAnchorHeight = 2.4f;

    private Camera mainCamera;
    private PlacedProp target;
    private RectTransform rect;

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

    private void Awake()
    {
        Instance = this;
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
        if (Cam == null)
        {
            return;
        }

        if (PlatformSwitcher.XRActive)
        {
            UpdateWorldSpacePosition();
        }
        else
        {
            UpdateOverlayPosition();
        }
    }

    private void UpdateWorldSpacePosition()
    {
        Vector3 position = target.transform.position + Vector3.up * worldAnchorHeight;
        transform.position = position;
        // Canvas front faces -Z, so point +Z away from the camera.
        transform.rotation = Quaternion.LookRotation(position - Cam.transform.position);
    }

    private void UpdateOverlayPosition()
    {
        Vector3 world = target.transform.position + Vector3.up * anchorHeight;
        Vector3 screen = Cam.WorldToScreenPoint(world);
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
