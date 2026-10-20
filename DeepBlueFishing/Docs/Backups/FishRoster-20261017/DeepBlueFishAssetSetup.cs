using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Sets the Quaternius fish FBX import options and creates small prefabs whose
/// normal swim clips play automatically in the catch showcase.
/// </summary>
internal sealed class DeepBlueFishAssetSetup : AssetPostprocessor
{
    // Keep the superseded Quaternius pack available to the Editor without shipping it as runtime Resources.
    private const string ModelFolder = "Assets/Archive/QuaterniusFish/Models";
    private const string PrefabFolder = "Assets/Archive/QuaterniusFish/Prefabs";
    private const string ControllerFolder = "Assets/Archive/QuaterniusFish/Controllers";
    private static readonly string[] FishModelNames =
    {
        "Anglerfish",
        "ArmoredCatfish",
        "Betta",
        "BlackLionFish",
        "Blobfish",
        "BlueGoldfish",
        "BlueTang",
        "ButterflyFish",
        "CardinalFish",
        "Clownfish",
        "CoralGrouper",
        "Cowfish",
        "Flatfish",
        "FlowerHorn",
        "GoblinShark",
        "Goldfish",
        "Humphead",
        "Koi",
        "Lionfish",
        "MandarinFish",
        "MoorishIdol",
        "ParrotFish",
        "Piranha",
        "Puffer",
        "RedSnapper",
        "RoyalGramma",
        "Shark",
        "Sunfish",
        "Swordfish",
        "Tang",
        "Tetra",
        "Tuna",
        "Turbot",
        "YellowTang",
        "ZebraClownFish"
    };

    private static bool isBuilding;

    [InitializeOnLoadMethod]
    private static void BuildExistingAssetsAfterEditorLoad()
    {
        EditorApplication.delayCall += BuildFishAssets;
    }

    private void OnPreprocessModel()
    {
        if (!IsTargetModel(assetPath)) return;

        var importer = (ModelImporter)assetImporter;
        bool changed = false;

        if (!importer.importAnimation)
        {
            importer.importAnimation = true;
            changed = true;
        }

        if (importer.animationType != ModelImporterAnimationType.Generic)
        {
            importer.animationType = ModelImporterAnimationType.Generic;
            changed = true;
        }

        if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            changed = true;
        }

        if (importer.importNormals != ModelImporterNormals.Calculate)
        {
            importer.importNormals = ModelImporterNormals.Calculate;
            changed = true;
        }

        if (importer.importTangents != ModelImporterTangents.CalculateMikk)
        {
            importer.importTangents = ModelImporterTangents.CalculateMikk;
            changed = true;
        }

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
            clips = importer.defaultClipAnimations;

        if (clips != null)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                bool shouldLoop = ShouldLoopClip(clips[i].name);
                if (clips[i].loopTime != shouldLoop)
                {
                    clips[i].loopTime = shouldLoop;
                    changed = true;
                }

                if (clips[i].loopPose != shouldLoop)
                {
                    clips[i].loopPose = shouldLoop;
                    changed = true;
                }
            }

            if (changed) importer.clipAnimations = clips;
        }
    }

    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (importedAssets.Any(IsTargetModel))
            EditorApplication.delayCall += BuildFishAssets;
    }

    private static void BuildFishAssets()
    {
        if (isBuilding || EditorApplication.isCompiling) return;
        isBuilding = true;

        try
        {
            EnsureAssetFolder(PrefabFolder);
            EnsureAssetFolder(ControllerFolder);

            bool generatedAny = false;
            foreach (string modelName in FishModelNames)
            {
                string modelPath = ModelFolder + "/" + modelName + ".fbx";
                if (!File.Exists(modelPath)) continue;

                var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
                if (importer == null) continue;

                if (!IsConfiguredForAnimation(importer))
                {
                    importer.importAnimation = true;
                    importer.animationType = ModelImporterAnimationType.Generic;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.importNormals = ModelImporterNormals.Calculate;
                    importer.importTangents = ModelImporterTangents.CalculateMikk;
                    ModelImporterClipAnimation[] clips = importer.clipAnimations;
                    if (clips == null || clips.Length == 0)
                        clips = importer.defaultClipAnimations;

                    if (clips != null)
                    {
                        for (int i = 0; i < clips.Length; i++)
                        {
                        bool shouldLoop = ShouldLoopClip(clips[i].name);
                        clips[i].loopTime = shouldLoop;
                        clips[i].loopPose = shouldLoop;
                        }
                        importer.clipAnimations = clips;
                    }

                    importer.SaveAndReimport();
                    return;
                }

                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                AnimationClip swimClip = FindSwimClip(modelPath);
                AnimationClip caughtClip = FindCaughtClip(modelPath);
                if (model == null || swimClip == null)
                {
                    Debug.LogWarning("Deep Blue fish setup skipped " + modelName +
                                     ": its imported FBX model or animation clip was not found.");
                    continue;
                }

                string controllerPath = ControllerFolder + "/" + modelName + ".controller";
                AnimatorController controller = GetOrCreateController(controllerPath, swimClip, caughtClip);
                if (controller == null) continue;

                string prefabPath = PrefabFolder + "/" + modelName + ".prefab";
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Animator existingAnimator = existingPrefab != null
                    ? existingPrefab.GetComponent<Animator>()
                    : null;

                if (existingAnimator != null && existingAnimator.runtimeAnimatorController == controller)
                    continue;

                GameObject instance = PrefabUtility.InstantiatePrefab(model) as GameObject;
                if (instance == null) instance = UnityEngine.Object.Instantiate(model);
                instance.name = modelName;

                Animator animator = instance.GetComponent<Animator>();
                if (animator == null) animator = instance.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                if (animator.avatar == null)
                {
                    Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(modelPath)
                        .OfType<Avatar>()
                        .FirstOrDefault();
                    if (avatar != null) animator.avatar = avatar;
                }

                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                UnityEngine.Object.DestroyImmediate(instance);
                generatedAny = true;
            }

            AssetDatabase.SaveAssets();
            if (generatedAny)
            {
                Debug.Log("Deep Blue imported the free Quaternius fish models and prepared their looping swim prefabs.");
            }
        }
        finally
        {
            isBuilding = false;
        }
    }

    private static bool IsConfiguredForAnimation(ModelImporter importer)
    {
        if (!importer.importAnimation || importer.animationType != ModelImporterAnimationType.Generic ||
            importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel ||
            importer.importNormals != ModelImporterNormals.Calculate ||
            importer.importTangents != ModelImporterTangents.CalculateMikk)
            return false;

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0) clips = importer.defaultClipAnimations;
        return clips != null && clips.Length > 0 && clips.All(clip =>
            clip.loopTime == ShouldLoopClip(clip.name) && clip.loopPose == ShouldLoopClip(clip.name));
    }

    private static AnimationClip FindSwimClip(string modelPath)
    {
        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(modelPath)
            .OfType<AnimationClip>()
            .Where(clip => clip != null && !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return clips
            .OrderByDescending(clip =>
                clip.name.IndexOf("swimming_normal", StringComparison.OrdinalIgnoreCase) >= 0 ? 5 :
                clip.name.IndexOf("swimming_slow", StringComparison.OrdinalIgnoreCase) >= 0 ? 5 :
                clip.name.IndexOf("swimming", StringComparison.OrdinalIgnoreCase) >= 0 ? 4 :
                clip.name.IndexOf("swim", StringComparison.OrdinalIgnoreCase) >= 0 ? 3 :
                clip.name.IndexOf("slow", StringComparison.OrdinalIgnoreCase) >= 0 ? 2 :
                clip.name.IndexOf("idle", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0)
            .FirstOrDefault();
    }

    private static AnimationClip FindCaughtClip(string modelPath)
    {
        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(modelPath)
            .OfType<AnimationClip>()
            .Where(clip => clip != null && !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return clips
            .OrderByDescending(clip =>
                clip.name.IndexOf("out_of_water", StringComparison.OrdinalIgnoreCase) >= 0 ? 4 :
                clip.name.IndexOf("attack", StringComparison.OrdinalIgnoreCase) >= 0 ? 3 :
                clip.name.IndexOf("swimming_impulse", StringComparison.OrdinalIgnoreCase) >= 0 ? 2 :
                clip.name.IndexOf("swimming_fast", StringComparison.OrdinalIgnoreCase) >= 0 ? 1 : 0)
            .FirstOrDefault(clip =>
                clip.name.IndexOf("out_of_water", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clip.name.IndexOf("attack", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clip.name.IndexOf("swimming_impulse", StringComparison.OrdinalIgnoreCase) >= 0 ||
                clip.name.IndexOf("swimming_fast", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static AnimatorController GetOrCreateController(string path, AnimationClip swimClip, AnimationClip caughtClip)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null)
            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        if (controller == null || controller.layers.Length == 0) return controller;

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState child in stateMachine.states)
            stateMachine.RemoveState(child.state);

        AnimatorState swimState = stateMachine.AddState("Swim");
        swimState.motion = swimClip;
        stateMachine.defaultState = swimState;

        if (caughtClip != null && caughtClip != swimClip)
        {
            AnimatorState caughtState = stateMachine.AddState("Caught Reaction");
            caughtState.motion = caughtClip;
            AnimatorStateTransition settleIntoSwim = caughtState.AddTransition(swimState);
            settleIntoSwim.hasExitTime = true;
            settleIntoSwim.exitTime = 0.96f;
            settleIntoSwim.duration = 0.14f;
            settleIntoSwim.hasFixedDuration = false;
        }

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static bool ShouldLoopClip(string clipName)
    {
        return clipName.IndexOf("swim", StringComparison.OrdinalIgnoreCase) >= 0 ||
               clipName.IndexOf("idle", StringComparison.OrdinalIgnoreCase) >= 0;
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

    private static bool IsTargetModel(string path)
    {
        if (string.IsNullOrEmpty(path) || !path.StartsWith(ModelFolder + "/", StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            return false;

        string modelName = Path.GetFileNameWithoutExtension(path);
        return FishModelNames.Contains(modelName);
    }
}
