using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>낚기 성공 뒤 카메라 앞에 나타나 살짝 움직이는 3D 물고기입니다.</summary>
public sealed class DeepBlueFishShowcase : MonoBehaviour
{
    private readonly List<Mesh> generatedMeshes = new List<Mesh>();
    private Material bodyMaterial;
    private Material bellyMaterial;
    private Material finMaterial;
    private Material spotMaterial;
    private Transform fishModel;
    private Transform fallbackModel;
    private GameObject importedFishInstance;
    private string importedFishName;
    private Vector3 fishCenteringOffset;
    private float fishYaw;
    private float presentationClock;
    private Vector3 presentationStartPosition;
    private Quaternion presentationStartRotation;
    private Vector3 presentationTargetScale = Vector3.one;
    private Transform tail;
    private Transform[] spineBones = new Transform[0];
    private Transform markings;
    private Vector3 bodyScale = new Vector3(0.8f, 0.29f, 0.19f);
    private float sizeMultiplier = 1f;
    private Color bodyColor = new Color(0.8f, 0.49f, 0.25f);

    private void Awake()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        bodyMaterial = MakeMaterial(shader, Color.white, 0.22f);
        bellyMaterial = MakeMaterial(shader, Color.white, 0.12f);
        finMaterial = MakeMaterial(shader, Color.white, 0.18f);
        spotMaterial = MakeMaterial(shader, new Color(0.08f, 0.10f, 0.08f), 0.05f);
        BuildModel();
    }

    private void Update()
    {
        if (!gameObject.activeSelf || fishModel == null) return;
        presentationClock += Time.unscaledDeltaTime;

        // 조과 화면이 열릴 때 아래쪽에서 부드럽게 올라오고, 이후에는 천천히 헤엄칩니다.
        const float entranceDuration = 0.62f;
        float entrance = Mathf.Clamp01(presentationClock / entranceDuration);
        entrance = entrance * entrance * (3f - 2f * entrance);
        float swimTime = Mathf.Max(0f, presentationClock - entranceDuration);
        float wave = Mathf.Sin(swimTime * 3.2f);

        // Keep the caught fish fully to the left of the right-anchored result card.
        Vector3 targetPosition = new Vector3(-2.2f, -0.12f, 2.75f) + fishCenteringOffset;
        Vector3 swimOffset = new Vector3(
            Mathf.Sin(swimTime * 1.15f) * 0.035f,
            Mathf.Sin(swimTime * 2.8f) * 0.045f,
            Mathf.Cos(swimTime * 1.3f) * 0.018f);
        fishModel.localPosition = Vector3.Lerp(presentationStartPosition, targetPosition, entrance) + swimOffset * entrance;

        Quaternion swimmingRotation = Quaternion.Euler(
            Mathf.Sin(swimTime * 1.8f) * 2.2f,
            fishYaw + Mathf.Sin(swimTime * 1.25f) * 4.5f,
            wave * 2.6f);
        fishModel.localRotation = Quaternion.Slerp(presentationStartRotation, swimmingRotation, entrance);
        float popScale = 1f + Mathf.Sin(entrance * Mathf.PI) * 0.055f;
        fishModel.localScale = presentationTargetScale * (Mathf.Lerp(0.86f, 1f, entrance) * popScale);

        if (tail != null && fishModel == fallbackModel)
            tail.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(swimTime * 8f) * 18f * entrance);
    }

    private void LateUpdate()
    {
        if (!gameObject.activeInHierarchy || importedFishInstance == null || spineBones == null || spineBones.Length == 0)
            return;

        // Animator가 기본 수영 포즈를 만든 다음, 꼬리 쪽으로 갈수록 커지는 작은 흔들림을 더합니다.
        // LateUpdate에서 더해야 매 프레임 Animator가 계산한 관절 포즈를 덮어쓰지 않습니다.
        float entrance = Mathf.Clamp01(presentationClock / 0.62f);
        entrance = entrance * entrance * (3f - 2f * entrance);
        float catchEnergy = Mathf.Exp(-presentationClock * 0.72f);
        float beatFrequency = Mathf.Lerp(2.0f, 2.8f, catchEnergy);
        float tailStrength = Mathf.Lerp(3.5f, 8.5f, catchEnergy);

        for (int i = 0; i < spineBones.Length; i++)
        {
            Transform bone = spineBones[i];
            if (bone == null) continue;

            float alongBody = (i + 1f) / spineBones.Length;
            float tailBias = Mathf.Pow(alongBody, 1.6f);
            float phase = presentationClock * beatFrequency * Mathf.PI * 2f - i * 0.48f;
            float angle = Mathf.Sin(phase) * Mathf.Lerp(0.5f, tailStrength, tailBias) * entrance;
            bone.localRotation = bone.localRotation * Quaternion.Euler(0f, 0f, angle);
        }
    }

    public void Show(Sprite fish, float weightKg)
    {
        if (fish == null) return;
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        Configure(fish, weightKg);
        BeginPresentation();
    }

    public void Hide()
    {
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }

    private void BeginPresentation()
    {
        presentationClock = 0f;
        presentationTargetScale = fishModel.localScale;
        presentationStartPosition = fishModel.localPosition + new Vector3(0f, -0.72f, -0.26f);
        presentationStartRotation = fishModel.localRotation * Quaternion.Euler(-17f, -24f, -12f);
    }

    private void BuildModel()
    {
        fishModel = new GameObject("3D Fish Catch").transform;
        fishModel.SetParent(transform, false);
        fishModel.localPosition = new Vector3(-2.2f, -0.12f, 2.75f);
        fallbackModel = fishModel;

        GameObject body = CreateSphere("Fish Body", fishModel, bodyMaterial,
            Vector3.zero, new Vector3(0.8f, 0.29f, 0.19f));
        GameObject belly = CreateSphere("Fish Belly", fishModel, bellyMaterial,
            new Vector3(0.02f, -0.11f, -0.01f), new Vector3(0.57f, 0.105f, 0.145f));
        body.name = "Fish Body";
        belly.name = "Fish Belly";

        tail = new GameObject("Tail Fin").transform;
        tail.SetParent(fishModel, false);
        tail.localPosition = new Vector3(-0.58f, 0f, 0f);
        CreateFin("Tail Shape", tail,
            new[] { Vector3.zero, new Vector3(-0.40f, 0.32f, 0f), new Vector3(-0.28f, 0f, 0f), new Vector3(-0.40f, -0.32f, 0f) },
            new[] { 0, 2, 1, 0, 3, 2 }, finMaterial);
        CreateFin("Dorsal Fin", fishModel,
            new[] { new Vector3(-0.23f, 0.22f, 0f), new Vector3(0.02f, 0.48f, 0f), new Vector3(0.25f, 0.23f, 0f) },
            new[] { 0, 1, 2 }, finMaterial);
        CreateFin("Lower Fin", fishModel,
            new[] { new Vector3(-0.18f, -0.22f, 0f), new Vector3(0.03f, -0.40f, 0f), new Vector3(0.18f, -0.23f, 0f) },
            new[] { 0, 2, 1 }, finMaterial);
        CreateFin("Pectoral Fin", fishModel,
            new[] { new Vector3(0.10f, -0.01f, -0.16f), new Vector3(-0.13f, -0.26f, -0.23f), new Vector3(0.31f, -0.22f, -0.19f) },
            new[] { 0, 2, 1 }, finMaterial);

        CreateSphere("Near Eye", fishModel, spotMaterial,
            new Vector3(0.33f, 0.055f, -0.16f), new Vector3(0.064f, 0.064f, 0.032f));
        CreateSphere("Far Eye", fishModel, spotMaterial,
            new Vector3(0.33f, 0.055f, 0.16f), new Vector3(0.064f, 0.064f, 0.032f));

        markings = new GameObject("Species Markings").transform;
        markings.SetParent(fishModel, false);
        for (int i = 0; i < 4; i++)
        {
            float x = -0.24f + i * 0.14f;
            CreateSphere("Mark " + i, markings, spotMaterial,
                new Vector3(x, 0.01f, -0.176f), new Vector3(0.055f, 0.12f, 0.018f));
        }
        SetMarkings(false);
    }

    private void Configure(Sprite fish, float weightKg)
    {
        string id = fish.name;
        Vector2 weightRange = DeepBlueFishDetails.WeightRange(fish);
        sizeMultiplier = Mathf.Lerp(0.86f, 1.18f, Mathf.InverseLerp(weightRange.x, weightRange.y, weightKg));
        bodyScale = new Vector3(0.8f, 0.29f, 0.19f);
        bodyColor = new Color(0.78f, 0.48f, 0.24f);
        bool hasPattern = false;

        switch (id)
        {
            case "Trout_CatchIcon": bodyScale = new Vector3(0.78f, 0.27f, 0.18f); bodyColor = new Color(0.77f, 0.48f, 0.32f); hasPattern = true; break;
            case "Fish_Bass": bodyScale = new Vector3(0.76f, 0.34f, 0.22f); bodyColor = new Color(0.28f, 0.48f, 0.27f); hasPattern = true; break;
            case "Fish_Pike": bodyScale = new Vector3(0.98f, 0.22f, 0.16f); bodyColor = new Color(0.43f, 0.53f, 0.31f); hasPattern = true; break;
            case "Fish_Bluegill": bodyScale = new Vector3(0.62f, 0.37f, 0.23f); bodyColor = new Color(0.26f, 0.55f, 0.68f); hasPattern = true; break;
            case "Fish_Carp": bodyScale = new Vector3(0.84f, 0.35f, 0.23f); bodyColor = new Color(0.74f, 0.60f, 0.29f); break;
            case "Fish_Catfish": bodyScale = new Vector3(0.92f, 0.27f, 0.20f); bodyColor = new Color(0.30f, 0.36f, 0.40f); break;
            case "Fish_YellowPerch": bodyScale = new Vector3(0.70f, 0.31f, 0.20f); bodyColor = new Color(0.83f, 0.66f, 0.22f); hasPattern = true; break;
            case "Fish_Walleye": bodyScale = new Vector3(0.84f, 0.28f, 0.21f); bodyColor = new Color(0.60f, 0.60f, 0.34f); hasPattern = true; break;
            case "Fish_BrookTrout": bodyScale = new Vector3(0.74f, 0.28f, 0.19f); bodyColor = new Color(0.75f, 0.43f, 0.32f); hasPattern = true; break;
            case "Fish_LakeSturgeon": bodyScale = new Vector3(1.00f, 0.23f, 0.17f); bodyColor = new Color(0.43f, 0.48f, 0.47f); break;
            case "Fish_BlackCrappie": bodyScale = new Vector3(0.65f, 0.36f, 0.21f); bodyColor = new Color(0.39f, 0.43f, 0.38f); hasPattern = true; break;
            case "Fish_Burbot": bodyScale = new Vector3(0.92f, 0.26f, 0.18f); bodyColor = new Color(0.55f, 0.44f, 0.32f); break;
        }

        if (TryActivateAnimatedFish(id)) return;

        fishCenteringOffset = Vector3.zero;
        fishYaw = 0f;
        spineBones = new Transform[0];
        if (importedFishInstance != null) importedFishInstance.SetActive(false);
        fallbackModel.gameObject.SetActive(true);
        fishModel = fallbackModel;
        fishModel.localPosition = new Vector3(-2.2f, -0.12f, 2.75f);
        fishModel.localRotation = Quaternion.Euler(0f, fishYaw, 0f);
        fishModel.localScale = Vector3.one;

        Transform body = fishModel.Find("Fish Body");
        Transform belly = fishModel.Find("Fish Belly");
        body.localScale = bodyScale * sizeMultiplier;
        belly.localScale = new Vector3(bodyScale.x * 0.72f, bodyScale.y * 0.36f, bodyScale.z * 0.78f) * sizeMultiplier;
        belly.localPosition = new Vector3(0.02f * sizeMultiplier, -bodyScale.y * 0.39f * sizeMultiplier, -0.01f);
        bodyMaterial.color = bodyColor;
        bellyMaterial.color = Color.Lerp(bodyColor, new Color(0.96f, 0.87f, 0.67f), 0.72f);
        finMaterial.color = Color.Lerp(bodyColor, Color.black, 0.20f);
        SetMarkings(hasPattern);
    }

    private bool TryActivateAnimatedFish(string speciesId)
    {
        string modelName = GetAnimatedFishModelName(speciesId);
        if (string.IsNullOrEmpty(modelName)) return false;

        string resourcePath = GetAnimatedFishResourcePath(speciesId, modelName);
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null) return false;

        if (importedFishInstance == null || importedFishName != resourcePath)
        {
            if (importedFishInstance != null) Destroy(importedFishInstance);
            importedFishInstance = Instantiate(prefab, transform, false);
            importedFishInstance.name = "Animated Catch - " + modelName;
            importedFishName = resourcePath;
        }

        fallbackModel.gameObject.SetActive(false);
        importedFishInstance.SetActive(true);
        fishModel = importedFishInstance.transform;
        bool usesFishAliveRig = speciesId == DeepBlueFishDetails.GuppyIconId ||
                                speciesId == DeepBlueFishDetails.ClownfishIconId;
        spineBones = usesFishAliveRig ? new Transform[0] : FindSpineBones(importedFishInstance);
        fishYaw = 90f;
        fishModel.localRotation = Quaternion.Euler(0f, fishYaw, 0f);
        fishModel.localScale = Vector3.one;
        fishModel.localPosition = new Vector3(-2.2f, -0.12f, 2.75f);

        Renderer[] renderers = importedFishInstance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);

            float longestSide = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (longestSide > 0.001f)
            {
                float scale = 1.25f * sizeMultiplier / longestSide;
                fishModel.localScale = Vector3.one * scale;
                fishCenteringOffset = -transform.InverseTransformVector((bounds.center - fishModel.position) * scale);
                fishModel.localPosition += fishCenteringOffset;
            }
        }

        Animator animator = importedFishInstance.GetComponentInChildren<Animator>(true);
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.speed = 1f;
            int caughtReaction = Animator.StringToHash("Base Layer.Caught Reaction");
            int fishAliveSwim = Animator.StringToHash("Base Layer.Swim");
            if (animator.HasState(0, caughtReaction))
            {
                animator.Play(caughtReaction, 0, 0f);
            }
            else if (usesFishAliveRig && animator.HasState(0, fishAliveSwim))
            {
                // Fish Alive's controller state is named "Swim"; its clips
                // (such as "swim_left") are motions inside that state.
                // Keep the caught fish swimming in place instead of wandering
                // away from the result card using its demo-scene motion logic.
                foreach (MonoBehaviour behaviour in importedFishInstance.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour != null && behaviour.GetType().Name == "FishMotion")
                        behaviour.enabled = false;
                }
                animator.SetFloat(Animator.StringToHash("swimSpeed"), 1f);
                animator.SetFloat(Animator.StringToHash("turn"), 0.5f);
                animator.Play(fishAliveSwim, 0, 0f);
            }
            else
            {
                RuntimeAnimatorController controller = animator.runtimeAnimatorController;
                if (controller != null && controller.animationClips.Length > 0)
                {
                    int clipState = Animator.StringToHash("Base Layer." + controller.animationClips[0].name);
                    if (animator.HasState(0, clipState))
                        animator.Play(clipState, 0, Random.value);
                    // If no matching state exists, let the controller's default
                    // state run instead of asking Animator.Play to log a warning.
                }
            }
        }

        return true;
    }

    private static Transform[] FindSpineBones(GameObject root)
    {
        var bones = new Transform[6];
        if (root == null) return bones;

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null || !candidate.name.StartsWith("Main", System.StringComparison.Ordinal))
                continue;

            if (int.TryParse(candidate.name.Substring(4), out int number) && number >= 1 && number <= bones.Length)
                bones[number - 1] = candidate;
        }

        return bones;
    }

    private static string GetAnimatedFishModelName(string speciesId)
    {
        const string assetFishPrefix = "Fish_Asset_";
        if (!string.IsNullOrEmpty(speciesId) &&
            speciesId.StartsWith(assetFishPrefix, System.StringComparison.Ordinal))
            return speciesId.Substring(assetFishPrefix.Length);

        switch (speciesId)
        {
            // Imported sample prefabs will be copied under Resources/DeepBlueFish/Prefabs.
            case DeepBlueFishDetails.GuppyIconId:
                return "Guppy";
            case DeepBlueFishDetails.ClownfishIconId:
                return "Clownfish";
            // 예전 도감 종 ID와 저장 키는 유지하고, 화면 모델은 무료 번들의 가장 가까운 형태로 연결합니다.
            case "Trout_CatchIcon":
                return "Tuna";
            case "Fish_Bass":
            case "Fish_YellowPerch":
            case "Fish_Walleye":
                return "CoralGrouper";
            case "Fish_Pike":
                return "Swordfish";
            case "Fish_Bluegill":
            case "Fish_BlackCrappie":
                return "Sunfish";
            case "Fish_Carp":
                return "Koi";
            case "Fish_Catfish":
            case "Fish_Burbot":
                return "ArmoredCatfish";
            case "Fish_BrookTrout":
                return "RedSnapper";
            case "Fish_LakeSturgeon":
                return "Shark";
            default:
                return null;
        }
    }

    private static string GetAnimatedFishResourcePath(string speciesId, string modelName)
    {
        if (speciesId == DeepBlueFishDetails.GuppyIconId ||
            speciesId == DeepBlueFishDetails.ClownfishIconId)
            return "DeepBlueFish/FishAlive/" + modelName;

        return "DeepBlueFish/Prefabs/" + modelName;
    }

    private void SetMarkings(bool visible)
    {
        if (markings == null) return;
        for (int i = 0; i < markings.childCount; i++) markings.GetChild(i).gameObject.SetActive(visible);
    }

    private static Material MakeMaterial(Shader shader, Color color, float smoothness)
    {
        var material = new Material(shader) { color = color };
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        return material;
    }

    private static GameObject CreateSphere(string name, Transform parent, Material material, Vector3 position, Vector3 scale)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        return part;
    }

    private void CreateFin(string name, Transform parent, Vector3[] vertices, int[] triangles, Material material)
    {
        GameObject part = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        part.transform.SetParent(parent, false);
        var mesh = new Mesh { name = name + " Mesh" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        part.GetComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = part.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        generatedMeshes.Add(mesh);
    }

    private void OnDestroy()
    {
        foreach (Mesh mesh in generatedMeshes) if (mesh != null) Destroy(mesh);
        if (importedFishInstance != null) Destroy(importedFishInstance);
        if (bodyMaterial != null) Destroy(bodyMaterial);
        if (bellyMaterial != null) Destroy(bellyMaterial);
        if (finMaterial != null) Destroy(finMaterial);
        if (spotMaterial != null) Destroy(spotMaterial);
    }
}
