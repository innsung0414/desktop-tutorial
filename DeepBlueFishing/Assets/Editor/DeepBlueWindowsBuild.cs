#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds the enabled Deep Blue scenes into a Windows x64 player.
/// </summary>
public static class DeepBlueWindowsBuild
{
    [MenuItem("Deep Blue/Build Windows x64")]
    public static void BuildWindows64()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Windows", "Deep Blue.exe"));
    }

    /// <summary>Creates a dated preview without replacing the ordinary local build.</summary>
    [MenuItem("Deep Blue/Build Windows Preview r6")]
    public static void BuildWindows64PreviewR6()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Delivery", "DeepBlue-Windows-20261017-preview-r6", "Deep Blue.exe"));
    }

    /// <summary>Creates a new dated preview without replacing earlier deliveries.</summary>
    [MenuItem("Deep Blue/Build Windows Preview r7")]
    public static void BuildWindows64PreviewR7()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Delivery", "DeepBlue-Windows-20261017-preview-r7", "Deep Blue.exe"));
    }

    /// <summary>Creates a new dated preview without replacing earlier deliveries.</summary>
    [MenuItem("Deep Blue/Build Windows Preview r8")]
    public static void BuildWindows64PreviewR8()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Delivery", "DeepBlue-Windows-20261017-preview-r8", "Deep Blue.exe"));
    }

    /// <summary>Creates a new dated preview without replacing earlier deliveries.</summary>
    [MenuItem("Deep Blue/Build Windows Preview r9")]
    public static void BuildWindows64PreviewR9()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Delivery", "DeepBlue-Windows-20261017-preview-r9", "Deep Blue.exe"));
    }

    /// <summary>Creates a fresh preview containing the latest fish presentation changes.</summary>
    [MenuItem("Deep Blue/Build Windows Preview r10")]
    public static void BuildWindows64PreviewR10()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Delivery", "DeepBlue-Windows-20261017-preview-r10", "Deep Blue.exe"));
    }

    /// <summary>Builds a new delivery preview with Fish Alive fish and the HUD/rod fixes.</summary>
    [MenuItem("Deep Blue/Build Windows Preview r12 Fish Alive")]
    public static void BuildWindows64PreviewR12FishAlive()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Delivery", "DeepBlue-Windows-20261017-preview-r12-fishalive", "Deep Blue.exe"));
    }

    /// <summary>Builds the Fish Alive delivery without the superseded Quaternius pack in Resources.</summary>
    [MenuItem("Deep Blue/Build Windows Preview r13 Fish Alive Archive Clean")]
    public static void BuildWindows64PreviewR13FishAliveArchiveClean()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Delivery", "DeepBlue-Windows-20261017-preview-r13-fishalive-archive-clean", "Deep Blue.exe"));
    }

    /// <summary>Builds the two Fish Alive runtime species without legacy fish assets or source notes in Resources.</summary>
    [MenuItem("Deep Blue/Build Windows Preview r14 Fish Alive Resources Only")]
    public static void BuildWindows64PreviewR14FishAliveResourcesOnly()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Delivery", "DeepBlue-Windows-20261017-preview-r14-fishalive-resources-only", "Deep Blue.exe"));
    }

    /// <summary>Builds the 37-entry fish collection with all runtime fish prefabs included.</summary>
    [MenuItem("Deep Blue/Build Windows Preview r15 Full Fish Roster")]
    public static void BuildWindows64PreviewR15FullFishRoster()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        BuildWindows64At(Path.Combine(projectRoot, "Builds", "Delivery", "DeepBlue-Windows-20261017-preview-r15-full-fish-roster", "Deep Blue.exe"));
    }

    /// <summary>Builds an isolated QA profile and restores the project's normal identity immediately.</summary>
    [MenuItem("Deep Blue/Build Windows QA r10")]
    public static void BuildWindows64QaR10()
    {
        string originalCompany = PlayerSettings.companyName;
        string originalProduct = PlayerSettings.productName;
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        try
        {
            PlayerSettings.companyName = "DeepBlue QA";
            PlayerSettings.productName = "Deep Blue QA";
            BuildWindows64At(Path.Combine(projectRoot, "Builds", "QA", "DeepBlue-Windows-20261017-r10", "Deep Blue QA.exe"));
        }
        finally
        {
            PlayerSettings.companyName = originalCompany;
            PlayerSettings.productName = originalProduct;
        }
    }

    private static void BuildWindows64At(string executablePath)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("Deep Blue build stopped: add an enabled scene in Build Profiles first.");
            return;
        }

        string outputDirectory = Path.GetDirectoryName(executablePath);
        Directory.CreateDirectory(outputDirectory);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = executablePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            float sizeInMegabytes = report.summary.totalSize / (1024f * 1024f);
            Debug.Log($"Deep Blue Windows build succeeded: {executablePath} ({sizeInMegabytes:F1} MB).");
        }
        else
        {
            Debug.LogError($"Deep Blue Windows build failed: {report.summary.result}, " +
                $"errors: {report.summary.totalErrors}.");
        }
    }
}
#endif
