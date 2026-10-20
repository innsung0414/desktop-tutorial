using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// Unity Editor에서 기본 낚시 장면을 만드는 도구입니다.
/// Unity 메뉴의 Deep Blue > Build Fishing Scene으로 다시 실행할 수도 있습니다.
/// </summary>
public static class DeepBlueSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/DeepBlueFishing.unity";
    private const string WaterTexturePath = "Assets/Art/Textures/Water_BaseColor.png";
    private static string[] ActiveSpeciesIds => DeepBlueFishDetails.CurrentSpeciesIds;
    private static string[] ActiveFishNames => ActiveSpeciesIds.Select(DeepBlueFishDetails.DisplayName).ToArray();
    private static string[] ActiveFishIconPaths => ActiveSpeciesIds.Select(id =>
        id.StartsWith("FishAlive_", System.StringComparison.Ordinal)
            ? "Assets/Art/UI/FishAlive/" + id + ".png"
            : "Assets/Art/UI/Quaternius/" + id + ".png").ToArray();

    public static int FishRewardCount => ActiveFishIconPaths.Length;

    [MenuItem("Deep Blue/Build Fishing Scene")]
    public static void BuildFishingScene()
    {
        System.IO.Directory.CreateDirectory("Assets/Art/Materials");
        AssetDatabase.Refresh();
        PrepareTexture(WaterTexturePath, TextureImporterType.Default, TextureWrapMode.Repeat);
        PrepareFishTextures();

        Material groundMaterial = CreateMaterial(
            "Assets/Art/Materials/Mat_ShoreGround.mat",
            new Color(0.25f, 0.36f, 0.28f),
            null,
            Vector2.one);

        Texture2D waterTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(WaterTexturePath);
        Material waterMaterial = CreateMaterial(
            "Assets/Art/Materials/Mat_LakeWater.mat",
            Color.white,
            waterTexture,
            new Vector2(8f, 6f));

        Material woodMaterial = CreateMaterial(
            "Assets/Art/Materials/Mat_DockWood.mat",
            new Color(0.31f, 0.22f, 0.15f),
            null,
            Vector2.one);
        Material redMaterial = CreateMaterial(
            "Assets/Art/Materials/Mat_BobberRed.mat",
            new Color(0.9f, 0.2f, 0.13f),
            null,
            Vector2.one);
        Material whiteMaterial = CreateMaterial(
            "Assets/Art/Materials/Mat_BobberWhite.mat",
            new Color(0.9f, 0.94f, 0.9f),
            null,
            Vector2.one);
        Material rodMaterial = CreateMaterial(
            "Assets/Art/Materials/Mat_Rod.mat",
            new Color(0.13f, 0.17f, 0.2f),
            null,
            Vector2.one);
        Material lineMaterial = CreateMaterial(
            "Assets/Art/Materials/Mat_FishingLine.mat",
            new Color(0.86f, 0.94f, 0.9f),
            null,
            Vector2.one);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.51f, 0.56f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.56f, 0.69f, 0.72f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 38f;
        RenderSettings.fogEndDistance = 105f;

        CreateGround(groundMaterial);
        CreateWater(waterMaterial);
        CreateWaterDeathZone();
        CreateLakeBed();
        CreateDock(woodMaterial);
        CreateShorelineBoundary();
        CreateSun();

        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0f, 3.5f);
        player.transform.rotation = Quaternion.identity;

        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = 2f;
        characterController.radius = 0.35f;
        characterController.center = new Vector3(0f, 1f, 0f);
        // Let the player walk back onto the dock from the shallow water bed.
        characterController.stepOffset = 0.5f;
        characterController.skinWidth = 0.08f;

        player.AddComponent<FirstPersonMovement>();
        FirstPersonLook look = player.AddComponent<FirstPersonLook>();
        player.AddComponent<DrowningController>();

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        cameraObject.transform.localRotation = Quaternion.identity;
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.fieldOfView = 72f;
        camera.nearClipPlane = 0.05f;
        camera.farClipPlane = 150f;

        SerializedObject lookObject = new SerializedObject(look);
        lookObject.FindProperty("cameraPivot").objectReferenceValue = cameraObject.transform;
        lookObject.ApplyModifiedPropertiesWithoutUndo();

        Transform rodTip = CreateRod(cameraObject.transform, rodMaterial, lineMaterial);
        Transform bobber = CreateBobber(redMaterial, whiteMaterial);
        LineRenderer fishingLine = CreateFishingLine(lineMaterial);

        GameObject fishingSystems = new GameObject("Fishing Systems");
        FishingGameController controller = fishingSystems.AddComponent<FishingGameController>();
        Sprite fishIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ActiveFishIconPaths[0]);
        SerializedObject controllerObject = new SerializedObject(controller);
        controllerObject.FindProperty("viewCamera").objectReferenceValue = cameraObject.transform;
        controllerObject.FindProperty("rodTip").objectReferenceValue = rodTip;
        controllerObject.FindProperty("bobber").objectReferenceValue = bobber;
        controllerObject.FindProperty("fishingLine").objectReferenceValue = fishingLine;
        controllerObject.FindProperty("catchFishIcon").objectReferenceValue = fishIcon;
        controllerObject.FindProperty("hookWindowSeconds").floatValue = 5f;
        AssignFishRewards(controllerObject);
        controllerObject.ApplyModifiedPropertiesWithoutUndo();
        fishingSystems.AddComponent<FishingHUD>();
        DeepBlueMenus menus = fishingSystems.AddComponent<DeepBlueMenus>();
        SerializedObject menuSettings = new SerializedObject(menus);
        menuSettings.FindProperty("titleBackground").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UI/Title/DeepBlue_TitleLake.png");
        menuSettings.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.productName = "Deep Blue";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Keep the environment art when rebuilding the playable scene from this menu.
        if (AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Environment/Textures/Dawn_Sky.png") != null)
        {
            DeepBlueEnvironmentBuilder.UpgradeEnvironment();
        }

        Debug.Log("Deep Blue fishing scene created: " + ScenePath);
    }

    [MenuItem("Deep Blue/Configure Fish Rewards")]
    public static void ConfigureFishRewardsInOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            Debug.LogWarning("Open DeepBlueFishing.unity before configuring fish rewards.");
            return;
        }

        PrepareFishTextures();
        FishingGameController controller = Object.FindFirstObjectByType<FishingGameController>();
        if (controller == null)
        {
            Debug.LogError("FishingGameController was not found in the open scene.");
            return;
        }

        SerializedObject controllerObject = new SerializedObject(controller);
        AssignFishRewards(controllerObject);
        controllerObject.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Configured " + FishRewardCount + " fish reward species in: " + ScenePath);
    }

    private static void PrepareFishTextures()
    {
        foreach (string fishIconPath in ActiveFishIconPaths)
        {
            PrepareTexture(fishIconPath, TextureImporterType.Sprite, TextureWrapMode.Clamp);
        }
    }

    private static void AssignFishRewards(SerializedObject controllerObject)
    {
        SerializedProperty rewards = controllerObject.FindProperty("fishRewards");
        if (rewards == null)
        {
            Debug.LogError("The fishRewards field was not found on FishingGameController.");
            return;
        }

        string[] fishNames = ActiveFishNames;
        string[] fishIconPaths = ActiveFishIconPaths;
        rewards.arraySize = fishIconPaths.Length;
        for (int i = 0; i < fishIconPaths.Length; i++)
        {
            SerializedProperty reward = rewards.GetArrayElementAtIndex(i);
            reward.FindPropertyRelative("displayName").stringValue = fishNames[i];
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(fishIconPaths[i]);
            reward.FindPropertyRelative("icon").objectReferenceValue = icon;
            SerializedProperty spawnWeight = reward.FindPropertyRelative("spawnWeight");
            if (spawnWeight != null) spawnWeight.floatValue = DeepBlueFishDetails.SpawnWeight(icon);
        }

        SerializedProperty catchFishIcon = controllerObject.FindProperty("catchFishIcon");
        if (catchFishIcon != null)
            catchFishIcon.objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(fishIconPaths[0]);
    }

    private static void PrepareTexture(string path, TextureImporterType type, TextureWrapMode wrapMode)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("Texture importer not found: " + path);
            return;
        }

        importer.textureType = type;
        importer.wrapMode = wrapMode;
        importer.alphaIsTransparency = type == TextureImporterType.Sprite;
        importer.mipmapEnabled = type == TextureImporterType.Default;
        if (type == TextureImporterType.Sprite)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
        }
        else
        {
            importer.sRGBTexture = true;
            importer.filterMode = FilterMode.Bilinear;
        }

        importer.SaveAndReimport();
    }

    private static Material CreateMaterial(string path, Color color, Texture texture, Vector2 textureScale)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        material.color = color;
        material.mainTexture = texture;
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
            material.SetTextureScale("_BaseMap", textureScale);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void CreateGround(Material material)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Shore Ground";
        ground.transform.position = new Vector3(0f, 0f, 0f);
        ground.transform.localScale = new Vector3(7f, 1f, 2f);
        ground.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateWater(Material material)
    {
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Lake Water";
        // A Plane is 10 x 10 metres before scaling. Start the lake at the shore's z=10 edge.
        water.transform.position = new Vector3(0f, 0.025f, 26f);
        water.transform.localScale = new Vector3(6f, 1f, 3.2f);
        water.GetComponent<Renderer>().sharedMaterial = material;
        Collider waterCollider = water.GetComponent<Collider>();
        if (waterCollider != null)
        {
            Object.DestroyImmediate(waterCollider);
        }
    }

    private static void CreateWaterDeathZone()
    {
        // Cover the whole lake and the player's ankles above its shallow bed.
        // The 0.15 m top stays below the player's feet on the dock (0.28 m).
        GameObject zone = new GameObject("Water Death Zone");
        zone.transform.position = new Vector3(0f, -0.1f, 26f);

        BoxCollider trigger = zone.AddComponent<BoxCollider>();
        // BoxCollider starts at 1 x 1 x 1, unlike the 10 x 10 Plane above.
        trigger.size = new Vector3(60f, 0.5f, 32f);
        trigger.isTrigger = true;

        Rigidbody body = zone.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezeAll;

        zone.AddComponent<WaterDeathZone>();
    }

    private static void CreateLakeBed()
    {
        // A shallow lake bed keeps the water passable on foot; the dock and shore
        // stay within the CharacterController's step height for an easy exit.
        GameObject lakeBed = new GameObject("Lake Bed (Collision)");
        lakeBed.transform.position = new Vector3(0f, -0.45f, 26f);
        lakeBed.AddComponent<BoxCollider>().size = new Vector3(60f, 0.4f, 32f);
    }

    private static void CreateDock(Material material)
    {
        GameObject dock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        dock.name = "Small Fishing Dock";
        dock.transform.position = new Vector3(0f, 0.1f, 8f);
        dock.transform.localScale = new Vector3(3.8f, 0.2f, 8f);
        dock.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateShorelineBoundary()
    {
        GameObject boundary = new GameObject("Shoreline Boundary");
        boundary.transform.position = new Vector3(0f, 1f, 13.2f);
        BoxCollider collider = boundary.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(60f, 2f, 0.35f);
    }

    private static void CreateSun()
    {
        GameObject sun = new GameObject("Sun", typeof(Light));
        sun.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
        Light light = sun.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.color = new Color(1f, 0.9f, 0.75f);
        light.shadows = LightShadows.Soft;
    }

    private static Transform CreateRod(Transform camera, Material material, Material tipMaterial)
    {
        GameObject rodRoot = new GameObject("Fishing Rod");
        rodRoot.transform.SetParent(camera, false);
        rodRoot.transform.localPosition = new Vector3(0.30f, -0.80f, 0.48f);
        rodRoot.transform.localRotation = Quaternion.Euler(25f, 0f, -28f);

        GameObject rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rod.name = "Rod Shaft";
        rod.transform.SetParent(rodRoot.transform, false);
        rod.transform.localPosition = new Vector3(0f, 0.48f, 0f);
        rod.transform.localScale = new Vector3(0.025f, 0.62f, 0.025f);
        rod.GetComponent<Renderer>().sharedMaterial = material;
        Collider rodCollider = rod.GetComponent<Collider>();
        if (rodCollider != null)
        {
            Object.DestroyImmediate(rodCollider);
        }

        GameObject tip = new GameObject("Rod Tip");
        tip.transform.SetParent(rodRoot.transform, false);
        tip.transform.localPosition = new Vector3(0f, 1.12f, 0.02f);

        GameObject tipGuide = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tipGuide.name = "Rod Tip Guide";
        tipGuide.transform.SetParent(tip.transform, false);
        tipGuide.transform.localScale = Vector3.one * 0.045f;
        tipGuide.GetComponent<Renderer>().sharedMaterial = tipMaterial;
        Collider tipCollider = tipGuide.GetComponent<Collider>();
        if (tipCollider != null) Object.DestroyImmediate(tipCollider);

        return tip.transform;
    }

    private static Transform CreateBobber(Material redMaterial, Material whiteMaterial)
    {
        GameObject root = new GameObject("Bobber");
        GameObject red = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        red.name = "Red Float";
        red.transform.SetParent(root.transform, false);
        red.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        red.transform.localScale = new Vector3(0.17f, 0.17f, 0.17f);
        red.GetComponent<Renderer>().sharedMaterial = redMaterial;

        GameObject white = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        white.name = "White Float Base";
        white.transform.SetParent(root.transform, false);
        white.transform.localPosition = new Vector3(0f, -0.055f, 0f);
        white.transform.localScale = new Vector3(0.12f, 0.1f, 0.12f);
        white.GetComponent<Renderer>().sharedMaterial = whiteMaterial;

        Collider[] colliders = root.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            Object.DestroyImmediate(collider);
        }

        root.SetActive(false);
        return root.transform;
    }

    private static LineRenderer CreateFishingLine(Material material)
    {
        GameObject lineObject = new GameObject("Fishing Line");
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startWidth = 0.012f;
        line.endWidth = 0.009f;
        line.sharedMaterial = material;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.enabled = false;
        return line;
    }
}

/// <summary>
/// 새 플레이 장면이 아직 없을 때 Unity 프로젝트를 처음 연 뒤 한 번 생성합니다.
/// Unity Editor 안에서 다시 만들고 싶으면 Deep Blue 메뉴의 Build Fishing Scene을 사용하세요.
/// </summary>
[InitializeOnLoad]
public static class DeepBlueSceneBuilderStartup
{
    private const string ScenePath = "Assets/Scenes/DeepBlueFishing.unity";
    private static int sceneLoadWaitFrames;

    static DeepBlueSceneBuilderStartup()
    {
        EditorApplication.update += BuildSceneWhenReady;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void BuildSceneWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
        {
            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                sceneLoadWaitFrames++;
                if (sceneLoadWaitFrames > 120)
                {
                    EditorApplication.update -= BuildSceneWhenReady;
                }
                return;
            }

            EditorApplication.update -= BuildSceneWhenReady;
            EnsureFishRewardsOnOpenScene();
            return;
        }

        EditorApplication.update -= BuildSceneWhenReady;
        try
        {
            DeepBlueSceneBuilder.BuildFishingScene();
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.path == ScenePath)
        {
            EditorApplication.delayCall += EnsureFishRewardsOnOpenScene;
        }
    }

    private static void EnsureFishRewardsOnOpenScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (SceneManager.GetActiveScene().path != ScenePath)
        {
            return;
        }

        FishingGameController controller = Object.FindFirstObjectByType<FishingGameController>();
        if (controller == null)
        {
            return;
        }

        SerializedObject controllerObject = new SerializedObject(controller);
        SerializedProperty rewards = controllerObject.FindProperty("fishRewards");
        if (rewards != null && rewards.arraySize == DeepBlueSceneBuilder.FishRewardCount)
        {
            bool allIconsAssigned = true;
            for (int i = 0; i < rewards.arraySize; i++)
            {
                if (rewards.GetArrayElementAtIndex(i).FindPropertyRelative("icon").objectReferenceValue == null)
                {
                    allIconsAssigned = false;
                    break;
                }
            }

            if (allIconsAssigned)
            {
                return;
            }
        }

        DeepBlueSceneBuilder.ConfigureFishRewardsInOpenScene();
    }
}
