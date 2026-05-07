#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ClickyBoomSceneBuilder
{
    [MenuItem("Tools/Clicky Boom/Build Assessment Scene")]
    public static void BuildAssessmentScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientLight = new Color(0.18f, 0.2f, 0.24f);

        CreateCamera();
        CreateLighting();
        CreateArena();

        AudioClip clickSound = LoadAudioClip("click_pop");
        AudioClip gameOverSound = LoadAudioClip("game_over");
        AudioClip ambience = LoadAudioClip("arcade_ambience");

        ParticleSystem popEffect = CreatePopEffect();
        ClickTarget[] targets = CreateTargetPrefabs(clickSound, popEffect);

        GameManager gameManager = CreateGameManager(gameOverSound, ambience);
        TargetSpawner spawner = CreateSpawner(targets);
        CreateUi(gameManager);
        CreateEventSystem();

        Selection.activeGameObject = spawner.gameObject;

        EnsureFolder("Assets/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/ClickyBoom.unity");
        Debug.Log("Clicky Boom enhanced assessment scene built at Assets/Scenes/ClickyBoom.unity");
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<ClickRaycaster>();
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 7.2f, -10.4f);
        cameraObject.transform.LookAt(new Vector3(0f, 0.8f, 1.2f));
        camera.fieldOfView = 43f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.035f, 0.055f);
    }

    private static void CreateLighting()
    {
        GameObject keyObject = new GameObject("Key Light");
        Light key = keyObject.AddComponent<Light>();
        key.type = LightType.Directional;
        key.intensity = 1.15f;
        key.color = new Color(0.86f, 0.95f, 1f);
        keyObject.transform.rotation = Quaternion.Euler(50f, -25f, 0f);

        GameObject rimObject = new GameObject("Blue Rim Light");
        Light rim = rimObject.AddComponent<Light>();
        rim.type = LightType.Point;
        rim.range = 16f;
        rim.intensity = 2.8f;
        rim.color = new Color(0.08f, 0.5f, 1f);
        rimObject.transform.position = new Vector3(-5.5f, 4f, -2f);

        GameObject warmObject = new GameObject("Warm Accent Light");
        Light warm = warmObject.AddComponent<Light>();
        warm.type = LightType.Point;
        warm.range = 14f;
        warm.intensity = 2.2f;
        warm.color = new Color(1f, 0.25f, 0.13f);
        warmObject.transform.position = new Vector3(5.5f, 3.5f, 2f);
    }

    private static void CreateArena()
    {
        Material floorMat = CreateMaterial("Arena_Floor_Mat", new Color(0.09f, 0.105f, 0.125f), new Color(0.04f, 0.3f, 0.45f));
        Material wallMat = CreateMaterial("Arena_Wall_Mat", new Color(0.055f, 0.065f, 0.09f), new Color(0f, 0f, 0f));
        Material blueGlow = CreateMaterial("Blue_Glow_Mat", new Color(0.05f, 0.55f, 1f), new Color(0.05f, 0.65f, 1f));
        Material redGlow = CreateMaterial("Red_Glow_Mat", new Color(1f, 0.22f, 0.12f), new Color(1f, 0.14f, 0.08f));

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Clicky Boom Arena Floor";
        ground.transform.localScale = new Vector3(16f, 0.3f, 12f);
        ground.transform.position = new Vector3(0f, -0.15f, 1f);
        ground.GetComponent<Renderer>().sharedMaterial = floorMat;

        CreateWall("Back Wall", new Vector3(0f, 2f, 7.1f), new Vector3(16f, 4f, 0.35f), wallMat);
        CreateWall("Left Wall", new Vector3(-8.1f, 1.5f, 1f), new Vector3(0.35f, 3f, 12f), wallMat);
        CreateWall("Right Wall", new Vector3(8.1f, 1.5f, 1f), new Vector3(0.35f, 3f, 12f), wallMat);

        for (int i = 0; i < 5; i++)
        {
            float x = -6f + i * 3f;
            CreateGlowStrip($"Back Blue Strip {i + 1}", new Vector3(x, 2.2f, 6.88f), new Vector3(0.16f, 2.4f, 0.08f), blueGlow);
        }

        CreateGlowStrip("Left Red Strip", new Vector3(-7.88f, 1.8f, -2.5f), new Vector3(0.08f, 2.3f, 0.16f), redGlow);
        CreateGlowStrip("Right Red Strip", new Vector3(7.88f, 1.8f, -2.5f), new Vector3(0.08f, 2.3f, 0.16f), redGlow);
        CreateFruitBowlDisplay();
    }

    private static void CreateWall(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateFruitBowlDisplay()
    {
        Material bowlMat = CreateMaterial("Fruit_Bowl_Mat", new Color(0.45f, 0.22f, 0.08f), new Color(0.08f, 0.02f, 0f));
        Material appleMat = CreateMaterial("Bowl_Apple_Mat", new Color(0.92f, 0.05f, 0.04f), new Color(0.22f, 0.01f, 0f));
        Material orangeMat = CreateMaterial("Bowl_Orange_Mat", new Color(1f, 0.5f, 0.04f), new Color(0.3f, 0.08f, 0f));
        Material pearMat = CreateMaterial("Bowl_Pear_Mat", new Color(0.78f, 0.9f, 0.18f), new Color(0.16f, 0.22f, 0.02f));
        Material grapeMat = CreateMaterial("Bowl_Grape_Mat", new Color(0.45f, 0.15f, 0.85f), new Color(0.12f, 0.02f, 0.22f));
        Material leafMat = CreateMaterial("Bowl_Leaf_Mat", new Color(0.14f, 0.72f, 0.22f), new Color(0.02f, 0.18f, 0.03f));

        GameObject bowl = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bowl.name = "Background Fruit Bowl";
        bowl.transform.position = new Vector3(0f, 0.42f, 6.55f);
        bowl.transform.localScale = new Vector3(3.8f, 0.62f, 1.05f);
        bowl.GetComponent<Renderer>().sharedMaterial = bowlMat;
        Object.DestroyImmediate(bowl.GetComponent<Collider>());

        CreateDisplayFruit("Bowl Apple", new Vector3(-1.15f, 1f, 6.15f), Vector3.one * 0.55f, appleMat);
        CreateDisplayFruit("Bowl Orange", new Vector3(-0.35f, 1.05f, 6.05f), Vector3.one * 0.58f, orangeMat);
        CreateDisplayFruit("Bowl Pear", new Vector3(0.55f, 1.12f, 6.12f), new Vector3(0.52f, 0.68f, 0.52f), pearMat);
        CreateDisplayFruit("Bowl Grapes", new Vector3(1.28f, 1.02f, 6.1f), Vector3.one * 0.38f, grapeMat);

        for (int i = 0; i < 3; i++)
        {
            GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaf.name = $"Bowl Leaf {i + 1}";
            leaf.transform.position = new Vector3(-0.5f + i * 0.5f, 1.62f, 6.02f);
            leaf.transform.localScale = new Vector3(0.45f, 0.12f, 0.23f);
            leaf.transform.rotation = Quaternion.Euler(0f, 0f, -25f + i * 25f);
            leaf.GetComponent<Renderer>().sharedMaterial = leafMat;
            Object.DestroyImmediate(leaf.GetComponent<Collider>());
        }
    }

    private static void CreateDisplayFruit(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject fruit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fruit.name = name;
        fruit.transform.position = position;
        fruit.transform.localScale = scale;
        fruit.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(fruit.GetComponent<Collider>());
    }

    private static void CreateGlowStrip(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = name;
        strip.transform.position = position;
        strip.transform.localScale = scale;
        strip.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static GameManager CreateGameManager(AudioClip gameOverSound, AudioClip ambience)
    {
        GameObject managerObject = new GameObject("GameManager");
        GameManager gameManager = managerObject.AddComponent<GameManager>();

        AudioSource music = managerObject.AddComponent<AudioSource>();
        music.clip = ambience;
        music.loop = true;
        music.volume = 0.22f;
        music.spatialBlend = 0f;

        SerializedObject manager = new SerializedObject(gameManager);
        manager.FindProperty("musicSource").objectReferenceValue = music;
        manager.FindProperty("gameOverSound").objectReferenceValue = gameOverSound;
        manager.ApplyModifiedProperties();

        return gameManager;
    }

    private static TargetSpawner CreateSpawner(ClickTarget[] targets)
    {
        GameObject spawnerObject = new GameObject("TargetSpawner");
        TargetSpawner spawner = spawnerObject.AddComponent<TargetSpawner>();

        SerializedObject spawnerObjectData = new SerializedObject(spawner);
        SerializedProperty targetPrefabs = spawnerObjectData.FindProperty("targetPrefabs");
        targetPrefabs.arraySize = targets.Length;

        for (int i = 0; i < targets.Length; i++)
        {
            targetPrefabs.GetArrayElementAtIndex(i).objectReferenceValue = targets[i];
        }

        spawnerObjectData.FindProperty("maxTargetsOnScreen").intValue = 7;
        spawnerObjectData.FindProperty("spawnInterval").floatValue = 0.62f;
        spawnerObjectData.FindProperty("targetLifeTime").floatValue = 3.8f;
        spawnerObjectData.FindProperty("xRange").vector2Value = new Vector2(-5.8f, 5.8f);
        spawnerObjectData.FindProperty("zRange").vector2Value = new Vector2(-2.6f, 5.0f);
        spawnerObjectData.FindProperty("yPosition").floatValue = 0.95f;
        spawnerObjectData.FindProperty("minSpacing").floatValue = 2.15f;
        spawnerObjectData.ApplyModifiedProperties();

        return spawner;
    }

    private static void CreateUi(GameManager gameManager)
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject hud = CreatePanel(canvasObject.transform, "HUD Panel", new Color(0.015f, 0.02f, 0.035f, 0.84f));
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        SetAnchors(hudRect, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f));
        hudRect.anchoredPosition = Vector2.zero;
        hudRect.sizeDelta = new Vector2(0f, 92f);

        TextMeshProUGUI titleText = CreateText(hud.transform, "TitleText", "FRUIT BOOM", new Vector2(0f, -18f), TextAlignmentOptions.Center, 30f);
        SetAnchors(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
        titleText.rectTransform.sizeDelta = new Vector2(360f, 50f);
        titleText.color = new Color(1f, 0.35f, 0.18f);

        TextMeshProUGUI scoreText = CreateText(hud.transform, "ScoreText", "Score: 0", new Vector2(34f, -22f), TextAlignmentOptions.Left, 34f);
        SetAnchors(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        scoreText.rectTransform.sizeDelta = new Vector2(360f, 58f);

        TextMeshProUGUI timerText = CreateText(hud.transform, "TimerText", "Time: 30", new Vector2(-34f, -22f), TextAlignmentOptions.Right, 34f);
        SetAnchors(timerText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
        timerText.rectTransform.sizeDelta = new Vector2(360f, 58f);

        GameObject timerBack = CreatePanel(hud.transform, "Timer Bar Back", new Color(1f, 1f, 1f, 0.13f));
        RectTransform timerBackRect = timerBack.GetComponent<RectTransform>();
        SetAnchors(timerBackRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
        timerBackRect.anchoredPosition = new Vector2(0f, 14f);
        timerBackRect.sizeDelta = new Vector2(620f, 16f);

        GameObject timerFillObject = CreatePanel(timerBack.transform, "Timer Fill", new Color(0.15f, 0.85f, 1f, 1f));
        Image timerFill = timerFillObject.GetComponent<Image>();
        timerFill.type = Image.Type.Filled;
        timerFill.fillMethod = Image.FillMethod.Horizontal;
        timerFill.fillOrigin = 0;
        RectTransform fillRect = timerFillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject gameOverPanel = CreatePanel(canvasObject.transform, "Game Over Panel", new Color(0.015f, 0.018f, 0.028f, 0.92f));
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        SetAnchors(panelRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        panelRect.sizeDelta = new Vector2(620f, 320f);

        TextMeshProUGUI gameOverText = CreateText(gameOverPanel.transform, "GameOverText", "Game Over", new Vector2(0f, 54f), TextAlignmentOptions.Center, 48f);
        SetAnchors(gameOverText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        gameOverText.rectTransform.sizeDelta = new Vector2(560f, 160f);

        Button restartButton = CreateRestartButton(gameOverPanel.transform);
        gameOverPanel.SetActive(false);

        GameObject startPanel = CreatePanel(canvasObject.transform, "Start Menu Panel", new Color(0.015f, 0.02f, 0.032f, 0.94f));
        RectTransform startRect = startPanel.GetComponent<RectTransform>();
        SetAnchors(startRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        startRect.sizeDelta = new Vector2(760f, 420f);

        TextMeshProUGUI menuTitle = CreateText(startPanel.transform, "MenuTitle", "FRUIT BOOM", new Vector2(0f, 112f), TextAlignmentOptions.Center, 62f);
        SetAnchors(menuTitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        menuTitle.rectTransform.sizeDelta = new Vector2(680f, 92f);
        menuTitle.color = new Color(1f, 0.42f, 0.14f);

        TextMeshProUGUI menuText = CreateText(startPanel.transform, "MenuInstructions", "Click the flying fruit before the timer runs out", new Vector2(0f, 26f), TextAlignmentOptions.Center, 28f);
        SetAnchors(menuText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        menuText.rectTransform.sizeDelta = new Vector2(680f, 64f);

        TextMeshProUGUI shortcutText = CreateText(startPanel.transform, "MenuShortcut", "Press Space or click Start", new Vector2(0f, -28f), TextAlignmentOptions.Center, 24f);
        SetAnchors(shortcutText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        shortcutText.rectTransform.sizeDelta = new Vector2(520f, 46f);
        shortcutText.color = new Color(0.72f, 0.88f, 1f);

        Button startButton = CreateStartButton(startPanel.transform);

        SerializedObject manager = new SerializedObject(gameManager);
        manager.FindProperty("scoreText").objectReferenceValue = scoreText;
        manager.FindProperty("timerText").objectReferenceValue = timerText;
        manager.FindProperty("startPanel").objectReferenceValue = startPanel;
        manager.FindProperty("startButton").objectReferenceValue = startButton;
        manager.FindProperty("gameOverText").objectReferenceValue = gameOverText;
        manager.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        manager.FindProperty("restartButton").objectReferenceValue = restartButton;
        manager.FindProperty("timerFill").objectReferenceValue = timerFill;
        manager.ApplyModifiedProperties();
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, Vector2 position, TextAlignmentOptions alignment, float fontSize)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = alignment;
        tmp.enableWordWrapping = false;
        tmp.rectTransform.anchoredPosition = position;

        return tmp;
    }

    private static Button CreateRestartButton(Transform parent)
    {
        GameObject buttonObject = CreatePanel(parent, "RestartButton", new Color(1f, 0.24f, 0.12f, 1f));
        Button button = buttonObject.AddComponent<Button>();
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetAnchors(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        buttonRect.anchoredPosition = new Vector2(0f, -96f);
        buttonRect.sizeDelta = new Vector2(240f, 64f);

        TextMeshProUGUI label = CreateText(buttonObject.transform, "Label", "Restart", Vector2.zero, TextAlignmentOptions.Center, 30f);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;

        return button;
    }

    private static Button CreateStartButton(Transform parent)
    {
        GameObject buttonObject = CreatePanel(parent, "StartButton", new Color(0.08f, 0.72f, 0.28f, 1f));
        Button button = buttonObject.AddComponent<Button>();
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        SetAnchors(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        buttonRect.anchoredPosition = new Vector2(0f, -122f);
        buttonRect.sizeDelta = new Vector2(260f, 70f);

        TextMeshProUGUI label = CreateText(buttonObject.transform, "Label", "Start", Vector2.zero, TextAlignmentOptions.Center, 34f);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;

        return button;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        return panel;
    }

    private static ClickTarget[] CreateTargetPrefabs(AudioClip clickSound, ParticleSystem popEffect)
    {
        EnsureFolder("Assets/Prefabs");

        Material appleRed = CreateMaterial("Fruit_Apple_Red_Mat", new Color(0.9f, 0.05f, 0.04f), new Color(0.45f, 0.02f, 0.02f));
        Material orange = CreateMaterial("Fruit_Orange_Mat", new Color(1f, 0.48f, 0.04f), new Color(0.85f, 0.2f, 0.02f));
        Material watermelon = CreateMaterial("Fruit_Watermelon_Mat", new Color(0.05f, 0.62f, 0.22f), new Color(0.02f, 0.3f, 0.1f));
        Material stripe = CreateMaterial("Fruit_Watermelon_Stripe_Mat", new Color(0.02f, 0.22f, 0.08f), new Color(0f, 0.06f, 0.02f));
        Material banana = CreateMaterial("Fruit_Banana_Mat", new Color(1f, 0.86f, 0.12f), new Color(0.35f, 0.22f, 0f));
        Material pear = CreateMaterial("Fruit_Pear_Mat", new Color(0.72f, 0.9f, 0.18f), new Color(0.18f, 0.25f, 0.02f));
        Material strawberry = CreateMaterial("Fruit_Strawberry_Mat", new Color(0.95f, 0.04f, 0.12f), new Color(0.32f, 0f, 0.03f));
        Material grape = CreateMaterial("Fruit_Grape_Mat", new Color(0.48f, 0.16f, 0.86f), new Color(0.12f, 0.02f, 0.24f));
        Material leaf = CreateMaterial("Fruit_Leaf_Mat", new Color(0.2f, 0.85f, 0.24f), new Color(0.04f, 0.3f, 0.06f));
        Material stem = CreateMaterial("Fruit_Stem_Mat", new Color(0.28f, 0.12f, 0.04f), new Color(0f, 0f, 0f));
        Material highlight = CreateMaterial("Fruit_Highlight_Mat", new Color(1f, 0.92f, 0.78f), new Color(0.3f, 0.2f, 0.1f));

        return new[]
        {
            CreateFruitTargetPrefab("Fruit_Target_Apple", FruitKind.Apple, appleRed, leaf, stem, highlight, clickSound, popEffect, 1.02f),
            CreateFruitTargetPrefab("Fruit_Target_Orange", FruitKind.Orange, orange, leaf, stem, highlight, clickSound, popEffect, 0.98f),
            CreateFruitTargetPrefab("Fruit_Target_Watermelon", FruitKind.Watermelon, watermelon, stripe, stem, highlight, clickSound, popEffect, 1.06f),
            CreateFruitTargetPrefab("Fruit_Target_Banana", FruitKind.Banana, banana, leaf, stem, highlight, clickSound, popEffect, 1f),
            CreateFruitTargetPrefab("Fruit_Target_Pear", FruitKind.Pear, pear, leaf, stem, highlight, clickSound, popEffect, 1f),
            CreateFruitTargetPrefab("Fruit_Target_Strawberry", FruitKind.Strawberry, strawberry, leaf, stem, highlight, clickSound, popEffect, 0.95f),
            CreateFruitTargetPrefab("Fruit_Target_Grapes", FruitKind.Grapes, grape, leaf, stem, highlight, clickSound, popEffect, 0.95f)
        };
    }

    private static ClickTarget CreateFruitTargetPrefab(string name, FruitKind fruitKind, Material fruitMaterial, Material accentMaterial, Material stemMaterial, Material highlightMaterial, AudioClip clickSound, ParticleSystem popEffect, float scale)
    {
        GameObject root = new GameObject(name);
        root.transform.localScale = Vector3.one * scale;

        ClickTarget target = root.AddComponent<ClickTarget>();
        SphereCollider collider = root.AddComponent<SphereCollider>();
        collider.radius = 0.78f;

        GameObject core = GameObject.CreatePrimitive(fruitKind == FruitKind.Banana ? PrimitiveType.Capsule : PrimitiveType.Sphere);
        core.name = $"{fruitKind} Body";
        core.transform.SetParent(root.transform, false);
        core.transform.localScale = GetFruitBodyScale(fruitKind);
        core.transform.localRotation = fruitKind == FruitKind.Banana ? Quaternion.Euler(0f, 0f, 70f) : Quaternion.identity;
        core.GetComponent<Renderer>().sharedMaterial = fruitMaterial;
        Object.DestroyImmediate(core.GetComponent<Collider>());

        if (fruitKind != FruitKind.Banana && fruitKind != FruitKind.Grapes)
        {
            AddStem(root.transform, stemMaterial);
            AddLeaf(root.transform, accentMaterial);
        }

        AddHighlight(root.transform, highlightMaterial);

        if (fruitKind == FruitKind.Watermelon)
        {
            AddWatermelonStripes(root.transform, accentMaterial);
        }
        else if (fruitKind == FruitKind.Orange)
        {
            AddOrangeDimples(root.transform, highlightMaterial);
        }
        else if (fruitKind == FruitKind.Strawberry)
        {
            AddStrawberrySeeds(root.transform, highlightMaterial);
        }
        else if (fruitKind == FruitKind.Grapes)
        {
            AddGrapeCluster(root.transform, fruitMaterial, accentMaterial);
        }
        else if (fruitKind == FruitKind.Banana)
        {
            AddBananaTips(root.transform, stemMaterial);
        }

        SerializedObject targetData = new SerializedObject(target);
        targetData.FindProperty("destroySound").objectReferenceValue = clickSound;
        targetData.FindProperty("destroyEffect").objectReferenceValue = popEffect;
        targetData.ApplyModifiedPropertiesWithoutUndo();

        string path = $"Assets/Prefabs/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);

        return prefab.GetComponent<ClickTarget>();
    }

    private static Vector3 GetFruitBodyScale(FruitKind fruitKind)
    {
        switch (fruitKind)
        {
            case FruitKind.Watermelon:
                return new Vector3(1.12f, 0.9f, 1.12f);
            case FruitKind.Banana:
                return new Vector3(0.34f, 0.96f, 0.34f);
            case FruitKind.Pear:
                return new Vector3(0.82f, 1.1f, 0.82f);
            case FruitKind.Strawberry:
                return new Vector3(0.78f, 0.98f, 0.78f);
            case FruitKind.Grapes:
                return Vector3.one * 0.42f;
            default:
                return Vector3.one * 0.95f;
        }
    }

    private static void AddStem(Transform parent, Material material)
    {
        GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stem.name = "Stem";
        stem.transform.SetParent(parent, false);
        stem.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        stem.transform.localRotation = Quaternion.Euler(12f, 0f, -18f);
        stem.transform.localScale = new Vector3(0.12f, 0.36f, 0.12f);
        stem.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(stem.GetComponent<Collider>());
    }

    private static void AddLeaf(Transform parent, Material material)
    {
        GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaf.name = "Leaf";
        leaf.transform.SetParent(parent, false);
        leaf.transform.localPosition = new Vector3(0.28f, 0.78f, 0.02f);
        leaf.transform.localRotation = Quaternion.Euler(0f, 0f, -35f);
        leaf.transform.localScale = new Vector3(0.42f, 0.16f, 0.24f);
        leaf.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(leaf.GetComponent<Collider>());
    }

    private static void AddHighlight(Transform parent, Material material)
    {
        GameObject shine = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shine.name = "Soft Highlight";
        shine.transform.SetParent(parent, false);
        shine.transform.localPosition = new Vector3(-0.28f, 0.25f, -0.48f);
        shine.transform.localScale = new Vector3(0.18f, 0.28f, 0.04f);
        shine.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(shine.GetComponent<Collider>());
    }

    private static void AddWatermelonStripes(Transform parent, Material material)
    {
        for (int i = 0; i < 4; i++)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stripe.name = $"Watermelon Stripe {i + 1}";
            stripe.transform.SetParent(parent, false);
            stripe.transform.localRotation = Quaternion.Euler(90f, i * 45f, 0f);
            stripe.transform.localScale = new Vector3(0.055f, 0.56f, 0.055f);
            stripe.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(stripe.GetComponent<Collider>());
        }
    }

    private static void AddOrangeDimples(Transform parent, Material material)
    {
        Vector3[] positions =
        {
            new Vector3(0.38f, 0.14f, -0.46f),
            new Vector3(0.16f, -0.24f, -0.52f),
            new Vector3(-0.22f, -0.06f, -0.52f)
        };

        foreach (Vector3 position in positions)
        {
            GameObject dimple = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dimple.name = "Orange Dimple";
            dimple.transform.SetParent(parent, false);
            dimple.transform.localPosition = position;
            dimple.transform.localScale = Vector3.one * 0.08f;
            dimple.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(dimple.GetComponent<Collider>());
        }
    }

    private static void AddStrawberrySeeds(Transform parent, Material material)
    {
        Vector3[] positions =
        {
            new Vector3(-0.28f, 0.2f, -0.46f),
            new Vector3(0f, 0.02f, -0.52f),
            new Vector3(0.24f, 0.25f, -0.46f),
            new Vector3(-0.12f, -0.28f, -0.48f),
            new Vector3(0.22f, -0.22f, -0.45f)
        };

        foreach (Vector3 position in positions)
        {
            GameObject seed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            seed.name = "Strawberry Seed";
            seed.transform.SetParent(parent, false);
            seed.transform.localPosition = position;
            seed.transform.localScale = new Vector3(0.07f, 0.1f, 0.025f);
            seed.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(seed.GetComponent<Collider>());
        }
    }

    private static void AddGrapeCluster(Transform parent, Material grapeMaterial, Material leafMaterial)
    {
        Vector3[] positions =
        {
            new Vector3(-0.28f, 0.2f, 0f),
            new Vector3(0.05f, 0.24f, 0f),
            new Vector3(0.34f, 0.1f, 0f),
            new Vector3(-0.1f, -0.1f, 0f),
            new Vector3(0.22f, -0.18f, 0f),
            new Vector3(0.02f, -0.42f, 0f)
        };

        foreach (Vector3 position in positions)
        {
            GameObject grape = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            grape.name = "Grape";
            grape.transform.SetParent(parent, false);
            grape.transform.localPosition = position;
            grape.transform.localScale = Vector3.one * 0.34f;
            grape.GetComponent<Renderer>().sharedMaterial = grapeMaterial;
            Object.DestroyImmediate(grape.GetComponent<Collider>());
        }

        AddLeaf(parent, leafMaterial);
    }

    private static void AddBananaTips(Transform parent, Material material)
    {
        Vector3[] positions =
        {
            new Vector3(-0.55f, 0.08f, 0f),
            new Vector3(0.55f, -0.08f, 0f)
        };

        foreach (Vector3 position in positions)
        {
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = "Banana Tip";
            tip.transform.SetParent(parent, false);
            tip.transform.localPosition = position;
            tip.transform.localScale = Vector3.one * 0.14f;
            tip.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(tip.GetComponent<Collider>());
        }
    }

    private enum FruitKind
    {
        Apple,
        Orange,
        Watermelon,
        Banana,
        Pear,
        Strawberry,
        Grapes
    }

    private static ParticleSystem CreatePopEffect()
    {
        EnsureFolder("Assets/Prefabs");

        GameObject effectObject = new GameObject("Target_Pop_Effect");
        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.duration = 0.35f;
        main.startLifetime = 0.32f;
        main.startSpeed = 5f;
        main.startSize = 0.18f;
        main.startColor = new Color(1f, 0.5f, 0.12f);
        main.loop = false;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 28) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.25f;

        string path = "Assets/Prefabs/Target_Pop_Effect.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(effectObject, path);
        Object.DestroyImmediate(effectObject);

        return prefab.GetComponent<ParticleSystem>();
    }

    private static AudioClip LoadAudioClip(string name)
    {
        string path = $"Assets/Audio/{name}.wav";

        if (!System.IO.File.Exists(path))
        {
            return null;
        }

        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static Material CreateMaterial(string name, Color color, Color emission)
    {
        EnsureFolder("Assets/Materials");

        string path = $"Assets/Materials/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

        if (material == null)
        {
            material = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emission * 1.4f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SetAnchors(RectTransform rect, Vector2 min, Vector2 max, Vector2 pivot)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = pivot;
    }

    private static void EnsureFolder(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";

            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
#endif
