using UnityEngine;

/// <summary>Swaps every Text under this canvas to the OS font at startup.</summary>
public class CanvasFontApplier : MonoBehaviour
{
    private void Awake()
    {
        UIFontProvider.Apply(gameObject);
    }
}
