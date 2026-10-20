using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Copies the two Fish Alive free sample prefabs into a dedicated Resources folder,
/// generates stable journal sprites, then switches the open fishing scene to those fish.
/// Run this after importing the package from My Assets.
/// </summary>
internal static class DeepBlueFishAliveSampleSetup
{
    private const string ResourceFolder = "Assets/Resources/DeepBlueFish/FishAlive";
    private const string IconFolder = "Assets/Art/UI/FishAlive";
    private const string FishingScenePath = "Assets/Scenes/DeepBlueFishing.unity";

    private static readonly Sample[] Samples =
    {
        new Sample("Guppy", DeepBlueFishDetails.GuppyIconId),
        new Sample("Clownfish", DeepBlueFishDetails.ClownfishIconId)
    };

    private static readonly Queue<Sample> PendingIcons = new Queue<Sample>();
    private static Sample activeSample;

    [MenuItem("Deep Blue/Set Up Fish Alive Free Samples")]
    private static void SetUpSamples()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Exit Play mode before setting up Fish Alive samples.");
            return;
        }

        var sources = new Dictionary<string, string>();
        foreach (Sample sample in Samples)
        {
            string sourcePath = FindFishPrefab(sample.PrefabName);
            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogError("Fish Alive " + sample.PrefabName +
                               " prefab was not found. Import Fish Alive - FREE Samples from Package Manager > My Assets first.");
                return;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (prefab == null || prefab.GetComponentInChildren<Animator>(true) == null ||
                prefab.GetComponentInChildren<SkinnedMeshRenderer>(true) == null)
            {
                Debug.LogError("Fish Alive prefab needs an Animator and a SkinnedMeshRenderer: " + sourcePath);
                return;
            }

            sources.Add(sample.PrefabName, sourcePath);
        }

        EnsureAssetFolder(ResourceFolder);
        EnsureAssetFolder(IconFolder);
        foreach (Sample sample in Samples)
        {
            string destination = ResourceFolder + "/" + sample.PrefabName + ".prefab";
            string source = sources[sample.PrefabName];
            if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase) || File.Exists(destination))
                continue;

            if (!AssetDatabase.CopyAsset(source, destination))
            {
                Debug.LogError("Could not copy Fish Alive prefab into Resources: " + source);
                return;
            }
        }

        AssetDatabase.Refresh();
        PendingIcons.Clear();
        foreach (Sample sample in Samples) PendingIcons.Enqueue(sample);
        activeSample = null;
        EditorApplication.update += GenerateNextIcon;
        Debug.Log("Fish Alive samples are ready. Generating the two journal sprites before changing the fishing scene.");
    }

    private static string FindFishPrefab(string fishName)
    {
        string normalizedName = Normalize(fishName);
        return AssetDatabase.FindAssets("t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => path.IndexOf("FishAlive", StringComparison.OrdinalIgnoreCase) >= 0 ||
                           path.IndexOf("Fish Alive", StringComparison.OrdinalIgnoreCase) >= 0)
            .Where(path => !path.StartsWith(ResourceFolder + "/", StringComparison.OrdinalIgnoreCase))
            .Where(path => Normalize(Path.GetFileNameWithoutExtension(path)).Contains(normalizedName))
            .Where(path =>
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                return prefab != null && prefab.GetComponentInChildren<Animator>(true) != null &&
                       prefab.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
            })
            .OrderByDescending(path => path.IndexOf("/Prefabs/Fish/", StringComparison.OrdinalIgnoreCase) >= 0)
            .ThenBy(path => Path.GetFileNameWithoutExtension(path).Length)
            .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }

    private static string Normalize(string value)
    {
        return new string(value.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }

    private static void GenerateNextIcon()
    {
        if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (activeSample == null)
        {
            if (PendingIcons.Count == 0)
            {
                FinishSetup();
                return;
            }

            activeSample = PendingIcons.Dequeue();
        }

        string prefabPath = ResourceFolder + "/" + activeSample.PrefabName + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Texture2D icon = prefab != null ? DeepBlueFishIconGenerator.RenderFishIcon(prefab) : null;
        if (icon == null)
        {
            Debug.LogError("Unity could not render a journal icon for " + activeSample.PrefabName + ".");
            StopGeneration();
            return;
        }

        string iconPath = IconFolder + "/" + activeSample.IconId + ".png";
        try
        {
            File.WriteAllBytes(iconPath, icon.EncodeToPNG());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(icon);
        }
        AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(iconPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 512;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 100;
            importer.SaveAndReimport();
        }

        activeSample = null;
    }

    private static void FinishSetup()
    {
        StopGeneration();
        AssetDatabase.SaveAssets();
        Debug.Log("Generated the two Fish Alive journal sprites.");

        Scene scene = SceneManager.GetActiveScene();
        if (scene.path == FishingScenePath && !EditorSceneManager.IsPreviewScene(scene))
            DeepBlueSceneBuilder.ConfigureFishRewardsInOpenScene();
        else
            Debug.LogWarning("Open DeepBlueFishing.unity and choose Deep Blue > Configure Fish Rewards to apply the two-fish roster.");
    }

    private static void StopGeneration()
    {
        EditorApplication.update -= GenerateNextIcon;
        activeSample = null;
    }

    private static void EnsureAssetFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        string name = Path.GetFileName(folder);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureAssetFolder(parent);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder(parent, name);
    }

    private sealed class Sample
    {
        public readonly string PrefabName;
        public readonly string IconId;

        public Sample(string prefabName, string iconId)
        {
            PrefabName = prefabName;
            IconId = iconId;
        }
    }
}
