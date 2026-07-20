using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

/// <summary>
/// Installs the XR packages (XR Plugin Management + OpenXR) so the project is
/// ready for VR builds. Run once via -executeMethod XRPackageInstaller.Install.
/// </summary>
public static class XRPackageInstaller
{
    [MenuItem("Tools/Setup/Install XR Packages")]
    public static void Install()
    {
        AddPackage("com.unity.xr.management");
        AddPackage("com.unity.xr.openxr");
    }

    private static void AddPackage(string id)
    {
        AddRequest request = Client.Add(id);
        while (!request.IsCompleted)
        {
            Thread.Sleep(100);
        }
        if (request.Status == StatusCode.Success)
        {
            Debug.Log($"[XRPackageInstaller] Installed {request.Result.packageId}");
        }
        else
        {
            Debug.LogError($"[XRPackageInstaller] Failed to install {id}: {request.Error?.message}");
        }
    }
}
