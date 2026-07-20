using UnityEngine;

/// <summary>
/// Abstraction over "where the user is pointing" so gameplay code works the
/// same on desktop (mouse) and on XR devices (controller ray).
/// </summary>
public interface IPointerProvider
{
    string ModeName { get; }

    /// <summary>World-space ray for pointing into the scene.</summary>
    bool TryGetPointerRay(out Ray ray);
}

/// <summary>Holds the active pointer provider for the current platform.</summary>
public static class PointerService
{
    private static IPointerProvider current;

    public static IPointerProvider Current
    {
        get => current ?? (current = new MousePointerProvider());
        set => current = value;
    }
}
