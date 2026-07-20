using UnityEngine;

/// <summary>
/// Provides a dynamic OS font so CJK labels render correctly in the player
/// without shipping a font asset.
/// </summary>
public static class UIFontProvider
{
    private static Font cached;

    public static Font Font
    {
        get
        {
            if (cached == null)
            {
                cached = Font.CreateDynamicFontFromOSFont(new[]
                {
                    "Microsoft JhengHei UI",
                    "Microsoft JhengHei",
                    "Microsoft YaHei",
                    "Segoe UI",
                    "Arial"
                }, 24);
            }
            return cached;
        }
    }

    /// <summary>Swap every Text under the root to the OS font.</summary>
    public static void Apply(GameObject root)
    {
        foreach (UnityEngine.UI.Text text in root.GetComponentsInChildren<UnityEngine.UI.Text>(true))
        {
            text.font = Font;
        }
    }
}
