using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates high-resolution journal sprites from the included animated fish models.
/// This replaces the small, pixelated AssetPreview thumbnails used by older builds.
/// </summary>
[InitializeOnLoad]
internal static class DeepBlueFishIconGenerator
{
    private const string PrefabFolder = "Assets/Archive/QuaterniusFish/Prefabs";
    private const string IconFolder = "Assets/Art/UI/Quaternius";
    private const string FishAlivePrefabFolder = "Assets/Resources/DeepBlueFish/FishAlive";
    private const string FishAliveIconFolder = "Assets/Art/UI/FishAlive";
    private const string RenderVersionFile = "Library/DeepBlueFishIconRenderVersion.txt";
    private const string RenderVersion = "3";
    private const int IconResolution = 512;
    private const int TotalIconCount = 37;
    private const long MinimumFreeSpaceBytes = 256L * 1024L * 1024L;

    private static readonly string[] FishModelNames =
    {
        "Anglerfish", "ArmoredCatfish", "Betta", "BlackLionFish", "Blobfish",
        "BlueGoldfish", "BlueTang", "ButterflyFish", "CardinalFish", "Clownfish",
        "CoralGrouper", "Cowfish", "Flatfish", "FlowerHorn", "GoblinShark",
        "Goldfish", "Humphead", "Koi", "Lionfish", "MandarinFish", "MoorishIdol",
        "ParrotFish", "Piranha", "Puffer", "RedSnapper", "RoyalGramma", "Shark",
        "Sunfish", "Swordfish", "Tang", "Tetra", "Tuna", "Turbot", "YellowTang",
        "ZebraClownFish"
    };

    private static readonly Queue<IconJob> pendingModels = new Queue<IconJob>();
    private static IconJob activeJob;
    private static int completed;
    private static int failed;
    private static bool isRunning;

    static DeepBlueFishIconGenerator()
    {
        EditorApplication.delayCall += StartIfIconsNeedUpgrade;
    }

    [MenuItem("Deep Blue/Regenerate High-Resolution Fish Icons")]
    private static void RegenerateMenu()
    {
        StartGeneration(true);
    }

    private static void StartIfIconsNeedUpgrade()
    {
        bool needsUpgrade = !File.Exists(RenderVersionFile) ||
                            File.ReadAllText(RenderVersionFile).Trim() != RenderVersion;

        if (needsUpgrade)
        {
            StartGeneration(true);
            return;
        }

        foreach (string modelName in FishModelNames)
        {
            string iconPath = IconFolder + "/Fish_Asset_" + modelName + ".png";
            if (!File.Exists(iconPath))
            {
                StartGeneration(false);
                return;
            }
        }

        foreach (string modelName in new[] { "Guppy", "Clownfish" })
        {
            string iconPath = FishAliveIconFolder + "/FishAlive_" + modelName + "_CatchIcon.png";
            if (!File.Exists(iconPath))
            {
                StartGeneration(false);
                return;
            }
        }
    }

    private static void StartGeneration(bool force)
    {
        if (isRunning || EditorApplication.isPlayingOrWillChangePlaymode) return;

        string projectDrive = Path.GetPathRoot(Application.dataPath);
        long availableSpace = new DriveInfo(projectDrive).AvailableFreeSpace;
        if (availableSpace < MinimumFreeSpaceBytes)
        {
            Debug.LogError("Deep Blue fish icon generation needs at least 256 MB free on " + projectDrive +
                           ". Free disk space, then choose Deep Blue > Regenerate High-Resolution Fish Icons.");
            return;
        }

        pendingModels.Clear();
        completed = 0;
        failed = 0;
        EnsureFolder(IconFolder);
        EnsureFolder(FishAliveIconFolder);

        foreach (string modelName in FishModelNames)
        {
            string iconPath = IconFolder + "/Fish_Asset_" + modelName + ".png";
            if (force || !File.Exists(iconPath))
                pendingModels.Enqueue(new IconJob(modelName, PrefabFolder + "/" + modelName + ".prefab", iconPath));
        }

        foreach (string modelName in new[] { "Guppy", "Clownfish" })
        {
            string iconPath = FishAliveIconFolder + "/FishAlive_" + modelName + "_CatchIcon.png";
            if (force || !File.Exists(iconPath))
                pendingModels.Enqueue(new IconJob(modelName, FishAlivePrefabFolder + "/" + modelName + ".prefab", iconPath));
        }

        if (pendingModels.Count == 0) return;

        isRunning = true;
        activeJob = null;
        EditorApplication.update += GenerateNextIcon;
        Debug.Log("Deep Blue is rendering " + pendingModels.Count + " high-resolution fish icons.");
    }

    private static void GenerateNextIcon()
    {
        if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) return;

        if (activeJob == null)
        {
            if (pendingModels.Count == 0)
            {
                FinishGeneration();
                return;
            }

            activeJob = pendingModels.Dequeue();
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(activeJob.PrefabPath);
        Texture2D icon = prefab != null ? RenderFishIcon(prefab) : null;
        if (icon == null || !HasVisiblePixels(icon))
        {
            if (icon != null) UnityEngine.Object.DestroyImmediate(icon);
            failed++;
            Debug.LogWarning("Deep Blue could not render a visible fish icon for " + activeJob.DisplayName + ".");
            activeJob = null;
            return;
        }

        string outputPath = activeJob.IconPath;
        try
        {
            File.WriteAllBytes(outputPath, icon.EncodeToPNG());
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(icon);
        }

        AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(outputPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = IconResolution;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 100;
            importer.SaveAndReimport();
        }

        completed++;
        activeJob = null;
    }

    internal static Texture2D RenderFishIcon(GameObject prefab)
    {
        Scene previewScene = EditorSceneManager.NewPreviewScene();
        GameObject fish = null;
        Camera camera = null;
        RenderTexture renderTexture = null;

        try
        {
            fish = UnityEngine.Object.Instantiate(prefab);
            fish.name = "Fish Icon Preview";
            SceneManager.MoveGameObjectToScene(fish, previewScene);
            fish.transform.position = Vector3.zero;
            // Match the side-on presentation used by the in-game 3D fish display.
            fish.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            foreach (MonoBehaviour behaviour in fish.GetComponentsInChildren<MonoBehaviour>(true))
                if (behaviour != null) behaviour.enabled = false;

            foreach (Animator animator in fish.GetComponentsInChildren<Animator>(true))
                if (animator != null) animator.enabled = false;

            // Some Fish Alive sample prefabs are saved inactive. Their renderers still
            // report bounds, but Unity omits them from the preview render until activated.
            fish.SetActive(true);

            Renderer[] renderers = fish.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return null;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            fish.transform.position -= bounds.center;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            float longestSide = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (longestSide <= 0.001f) return null;

            GameObject cameraObject = new GameObject("Fish Icon Preview Camera");
            SceneManager.MoveGameObjectToScene(cameraObject, previewScene);
            camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = longestSide * 0.62f;
            camera.aspect = 1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = longestSide * 5f;
            camera.allowHDR = false;
            camera.allowMSAA = true;
            camera.useOcclusionCulling = false;
            camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(previewScene);

            Vector3 viewFromTarget = new Vector3(0.22f, 0.16f, -1f).normalized;
            camera.transform.position = bounds.center + viewFromTarget * longestSide * 2.5f;
            camera.transform.rotation = Quaternion.LookRotation(bounds.center - camera.transform.position, Vector3.up);

            CreatePreviewLight(previewScene, "Fish Icon Key Light", new Color(1f, 0.93f, 0.82f), 1.25f,
                Quaternion.Euler(35f, -35f, 0f));
            CreatePreviewLight(previewScene, "Fish Icon Fill Light", new Color(0.73f, 0.84f, 1f), 0.65f,
                Quaternion.Euler(18f, 145f, 0f));

            renderTexture = new RenderTexture(IconResolution, IconResolution, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
            {
                antiAliasing = 1,
                name = "Deep Blue Fish Icon Render"
            };
            renderTexture.Create();

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            Texture2D icon = null;
            try
            {
                if (GraphicsSettings.currentRenderPipeline != null)
                {
                    var request = new RenderPipeline.StandardRequest { destination = renderTexture };
                    if (!RenderPipeline.SupportsRenderRequest(camera, request))
                        throw new InvalidOperationException("The active render pipeline does not support icon rendering.");

                    RenderPipeline.SubmitRenderRequest(camera, request);
                }
                else
                {
                    camera.targetTexture = renderTexture;
                    camera.Render();
                }

                RenderTexture.active = renderTexture;
                icon = new Texture2D(IconResolution, IconResolution, TextureFormat.RGBA32, false, false);
                icon.ReadPixels(new Rect(0, 0, IconResolution, IconResolution), 0, 0);
                icon.Apply(false, false);
                return icon;
            }
            catch
            {
                if (icon != null) UnityEngine.Object.DestroyImmediate(icon);
                throw;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Deep Blue fish icon render failed for " + prefab.name + ": " + exception.Message);
            return null;
        }
        finally
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }

            if (previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(previewScene);
        }
    }

    private static void CreatePreviewLight(Scene scene, string objectName, Color color, float intensity,
        Quaternion rotation)
    {
        GameObject lightObject = new GameObject(objectName);
        SceneManager.MoveGameObjectToScene(lightObject, scene);
        lightObject.transform.rotation = rotation;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = color;
        light.intensity = intensity;
        light.shadows = LightShadows.None;
    }

    private static bool HasVisiblePixels(Texture2D texture)
    {
        if (texture == null) return false;

        Color32[] pixels = texture.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
            if (pixels[i].a > 8) return true;

        return false;
    }

    private static void FinishGeneration()
    {
        EditorApplication.update -= GenerateNextIcon;
        isRunning = false;
        AssetDatabase.SaveAssets();

        if (failed == 0 && completed == TotalIconCount)
        {
            File.WriteAllText(RenderVersionFile, RenderVersion);
            Debug.Log("Deep Blue generated " + completed + " high-resolution fish icons.");
        }
        else
        {
            Debug.LogWarning("Deep Blue generated " + completed + " fish icons; " + failed + " failed.");
        }
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;
        string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        if (!string.IsNullOrEmpty(parent)) AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
    }

    private sealed class IconJob
    {
        internal readonly string DisplayName;
        internal readonly string PrefabPath;
        internal readonly string IconPath;

        internal IconJob(string displayName, string prefabPath, string iconPath)
        {
            DisplayName = displayName;
            PrefabPath = prefabPath;
            IconPath = iconPath;
        }
    }
}
