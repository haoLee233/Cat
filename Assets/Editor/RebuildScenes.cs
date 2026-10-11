using System;
using System.IO;
using JinYanMaoYing;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public static class RebuildScenes
{
    private static Font font;

    [MenuItem("锦檐猫影/生成首批场景")]
    public static void Build()
    {
        ConfigureArt();
        font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/STXINGKA.TTF");
        if (font == null) throw new Exception("缺少 STXINGKA 字体");

        Directory.CreateDirectory("Assets/Scenes");
        string cover = BuildCover();
        string map = BuildMap();
        string wuMen = BuildWuMen();
        string baoHeDian = BuildBaoHeDian();
        string knowledge = BuildWuMenKnowledge();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(cover, true),
            new EditorBuildSettingsScene(map, true),
            new EditorBuildSettingsScene(wuMen, true),
            new EditorBuildSettingsScene(baoHeDian, true),
            new EditorBuildSettingsScene(knowledge, true),
        };
        PlayerSettings.productName = "锦檐猫影：筑梦人";
        PlayerSettings.companyName = "锦檐猫影";
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(cover);
        Debug.Log("REMAKE_SCENES_READY " + cover + ", " + map + ", " + wuMen + ", " + baoHeDian + ", " + knowledge);
    }

    private static void ConfigureArt()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.textureType == TextureImporterType.Sprite) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
        }
    }

    private static Scene NewScene(string name)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = name;
        GameObject cameraObject = new GameObject("主摄像机", typeof(Camera), typeof(AudioListener));
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.4f;
        camera.backgroundColor = new Color(0.06f, 0.06f, 0.07f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        new GameObject("事件系统", typeof(EventSystem), typeof(StandaloneInputModule));
        return scene;
    }

    private static Canvas MakeCanvas()
    {
        GameObject obj = new GameObject("画布", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = obj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = obj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 position)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private static Image Picture(Transform parent, string name, string assetPath, Vector2 size, Vector2 position)
    {
        RectTransform rect = Rect(parent, name, size, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (image.sprite == null) throw new Exception("缺少图片: " + assetPath);
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static Image ColorPanel(Transform parent, string name, Vector2 size, Vector2 position, Color color)
    {
        RectTransform rect = Rect(parent, name, size, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text Label(Transform parent, string name, string content, int size, Vector2 bounds, Vector2 position, Color color)
    {
        RectTransform rect = Rect(parent, name, bounds, position);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.text = content;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.resizeTextForBestFit = false;
        text.raycastTarget = false;
        return text;
    }

    private static Button ActionButton(Transform parent, string name, string content, Vector2 size, Vector2 position)
    {
        RectTransform rect = Rect(parent, name, size, position);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.14f, 0.11f, 0.11f, 0.87f);
        Button button = rect.gameObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.84f, 0.65f, 0.55f, 1f);
        colors.pressedColor = new Color(0.65f, 0.38f, 0.31f, 1f);
        button.colors = colors;
        Label(rect, "文字", content, 44, size, Vector2.zero, new Color(1f, 0.94f, 0.84f));
        return button;
    }

    private static Button SpriteButton(Transform parent, string name, string normalPath, string hoverPath,
        Vector2 size, Vector2 position, float lift = 0f)
    {
        Image image = Picture(parent, name, normalPath, size, position);
        image.raycastTarget = true;
        Button button = image.gameObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.targetGraphic = image;
        SpriteHoverButton hover = image.gameObject.AddComponent<SpriteHoverButton>();
        hover.normal = image.sprite;
        hover.highlighted = string.IsNullOrEmpty(hoverPath) ? null : AssetDatabase.LoadAssetAtPath<Sprite>(hoverPath);
        hover.lift = lift;
        if (!string.IsNullOrEmpty(hoverPath) && hover.highlighted == null)
            throw new Exception("缺少按钮高亮图片: " + hoverPath);
        return button;
    }

    private static string Save(Scene scene, string name)
    {
        string path = "Assets/Scenes/" + name + ".unity";
        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, path)) throw new Exception("无法保存场景 " + path);
        return path;
    }

    private static string BuildCover()
    {
        Scene scene = NewScene(GameNavigator.Cover);
        Canvas canvas = MakeCanvas();
        Picture(canvas.transform, "完整封面", "Assets/Art/CoverBg.png", new Vector2(1920f, 1080f), Vector2.zero);
        ColorPanel(canvas.transform, "点击提示底色", new Vector2(720f, 130f), new Vector2(0f, -420f), new Color(1f, 1f, 1f, 0.33f));
        GameNavigator nav = new GameObject("场景导航", typeof(GameNavigator)).GetComponent<GameNavigator>();
        Button start = ActionButton(canvas.transform, "开始游戏", "点击开始旅程", new Vector2(600f, 92f), new Vector2(0f, -420f));
        UnityEventTools.AddPersistentListener(start.onClick, nav.OpenMap);
        Button quit = ActionButton(canvas.transform, "退出游戏", "退出游戏", new Vector2(235f, 72f), new Vector2(810f, 460f));
        UnityEventTools.AddPersistentListener(quit.onClick, nav.Quit);
        return Save(scene, GameNavigator.Cover);
    }

    private static string BuildMap()
    {
        Scene scene = NewScene(GameNavigator.Map);
        Canvas canvas = MakeCanvas();
        Picture(canvas.transform, "故宫水墨地图", "Assets/Art/主地图/地图/地图_副本.png", new Vector2(1920f, 1080f), Vector2.zero);
        ColorPanel(canvas.transform, "标题底色", new Vector2(1920f, 130f), new Vector2(0f, 475f), new Color(0.07f, 0.07f, 0.07f, 0.61f));
        Label(canvas.transform, "地图标题", "故宫寻迹", 72, new Vector2(700f, 112f), new Vector2(0f, 475f), Color.white);
        GameNavigator nav = new GameObject("场景导航", typeof(GameNavigator)).GetComponent<GameNavigator>();
        Button wuMen = ActionButton(canvas.transform, "午门入口", "午门 · 开始追逐", new Vector2(480f, 90f), new Vector2(-560f, -340f));
        UnityEventTools.AddPersistentListener(wuMen.onClick, nav.OpenWuMen);
        Button taiHe = ActionButton(canvas.transform, "太和殿入口", "太和殿 · 制作中", new Vector2(480f, 90f), new Vector2(0f, -340f));
        taiHe.interactable = false;
        Button baoHe = ActionButton(canvas.transform, "保和殿入口", "保和殿 · 水管谜题", new Vector2(480f, 90f), new Vector2(560f, -340f));
        UnityEventTools.AddPersistentListener(baoHe.onClick, nav.OpenBaoHeDian);
        Button back = ActionButton(canvas.transform, "返回封面", "返回封面", new Vector2(290f, 76f), new Vector2(-780f, 430f));
        UnityEventTools.AddPersistentListener(back.onClick, nav.OpenCover);
        Button quit = ActionButton(canvas.transform, "退出游戏", "退出游戏", new Vector2(270f, 76f), new Vector2(790f, 430f));
        UnityEventTools.AddPersistentListener(quit.onClick, nav.Quit);
        Label(canvas.transform, "进度说明", "午门与保和殿已开放；通关进度会自动保存", 34, new Vector2(1150f, 62f), new Vector2(0f, -430f), new Color(0.18f, 0.13f, 0.12f));
        return Save(scene, GameNavigator.Map);
    }

    private static string BuildWuMen()
    {
        Scene scene = NewScene(GameNavigator.WuMen);
        Canvas canvas = MakeCanvas();
        Image background1 = Picture(canvas.transform, "午门滚动背景1", "Assets/Art/WuMen/WM_Bg_Run.png", new Vector2(1920f, 1080f), Vector2.zero);
        Image background2 = Picture(canvas.transform, "午门滚动背景2", "Assets/Art/WuMen/WM_Bg_Run.png", new Vector2(1920f, 1080f), new Vector2(1920f, 0f));
        background1.preserveAspect = background2.preserveAspect = false;
        RectTransform playArea = Rect(canvas.transform, "游戏区域", new Vector2(1920f, 1080f), Vector2.zero);
        Image cat = Picture(playArea, "奔跑黑猫", "Assets/Art/WuMen/跑动1.png", new Vector2(260f, 155f), new Vector2(-600f, -260f));
        Image hudFrame = Picture(canvas.transform, "午门顶部卷轴", "Assets/Art/WuMen/WM_Dialog_Box.png", new Vector2(1660f, 235f), new Vector2(0f, 420f));
        hudFrame.preserveAspect = false;
        Text status = Label(canvas.transform, "收集得分与体力", "", 37, new Vector2(1250f, 86f), new Vector2(-105f, 420f), new Color(0.16f, 0.12f, 0.10f));
        Text hint = Label(canvas.transform, "操作提示", "", 34, new Vector2(1150f, 66f), new Vector2(0f, -480f), new Color(0.22f, 0.13f, 0.12f));
        WuMenRunner runner = new GameObject("午门关卡控制", typeof(WuMenRunner)).GetComponent<WuMenRunner>();
        runner.playArea = playArea;
        runner.backgrounds = new[] { background1.rectTransform, background2.rectTransform };
        runner.cat = cat.rectTransform;
        runner.catImage = cat;
        runner.crouchSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/趴下.png");
        runner.runFrames = new[] {
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/跑动1.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/跑动2.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/跑动3.png")
        };
        runner.statusText = status;
        runner.hudFrame = hudFrame;
        runner.hintText = hint;
        runner.coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/金币.png");
        runner.beastSprites = new[] {
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/Collect_JiShou_01.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/Collect_JiShou_02.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/Collect_JiShou_03.png")
        };
        runner.lionSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/Obstacle_Lion.png");
        runner.lanternSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/Obstacle_Lantern.png");
        Button settings = SpriteButton(canvas.transform, "设置按钮", "Assets/Art/WuMen/设置.png", null,
            new Vector2(130f, 130f), new Vector2(840f, 440f), 3f);
        UnityEventTools.AddPersistentListener(settings.onClick, runner.OpenSettings);

        RectTransform settingsPanel = Rect(canvas.transform, "设置页面", new Vector2(1920f, 1080f), Vector2.zero);
        Image shade = settingsPanel.gameObject.AddComponent<Image>();
        shade.color = new Color(0f, 0f, 0f, 0.65f);
        shade.raycastTarget = true;
        Picture(settingsPanel, "原版设置面板", "Assets/Art/WuMen/设置面板.png", new Vector2(700f, 920f), Vector2.zero);
        runner.settingsPanel = settingsPanel.gameObject;
        Button resume = SpriteButton(settingsPanel, "返回游戏", "Assets/Art/WuMen/返回游戏.png",
            "Assets/Art/WuMen/返回游戏-选中.png", new Vector2(250f, 70f), new Vector2(0f, 180f));
        Button restart = SpriteButton(settingsPanel, "重新开始", "Assets/Art/WuMen/重新开始.png",
            "Assets/Art/WuMen/重新开始-选中.png", new Vector2(250f, 70f), new Vector2(0f, 10f));
        Button exit = SpriteButton(settingsPanel, "退出游戏", "Assets/Art/WuMen/退出游戏.png",
            "Assets/Art/WuMen/退出游戏-选中.png", new Vector2(250f, 70f), new Vector2(0f, -160f));
        Button close = SpriteButton(settingsPanel, "关闭设置", "Assets/Art/WuMen/叉号.png", null,
            new Vector2(70f, 90f), new Vector2(315f, 380f), 3f);
        UnityEventTools.AddPersistentListener(resume.onClick, runner.ReturnGame);
        UnityEventTools.AddPersistentListener(restart.onClick, runner.Restart);
        UnityEventTools.AddPersistentListener(exit.onClick, runner.QuitGame);
        UnityEventTools.AddPersistentListener(close.onClick, runner.ReturnGame);

        RectTransform panel = Rect(canvas.transform, "结算与暂停面板", new Vector2(680f, 420f), Vector2.zero);
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.97f, 0.93f, 0.84f, 0.95f);
        panel.gameObject.AddComponent<CanvasGroup>();
        runner.resultPanel = panel.gameObject;
        runner.resultText = Label(panel, "结算文字", "", 60, new Vector2(600f, 190f), new Vector2(0f, 90f), new Color(0.15f, 0.10f, 0.10f));
        runner.rankImage = Picture(panel, "通关评级", "Assets/Art/WuMen/评级乙.png", new Vector2(330f, 155f), new Vector2(0f, 75f));
        runner.rankExcellent = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/评级超凡.png");
        runner.rankFirst = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/评级甲.png");
        runner.rankSecond = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/评级乙.png");
        runner.scoreText = Label(panel, "结算得分", "", 39, new Vector2(580f, 65f), new Vector2(0f, -30f), new Color(0.15f, 0.10f, 0.10f));
        Button retry = ActionButton(panel, "重新开始", "重新开始", new Vector2(270f, 85f), new Vector2(-155f, -120f));
        Button toMap = ActionButton(panel, "返回地图", "返回地图", new Vector2(270f, 85f), new Vector2(155f, -120f));
        UnityEventTools.AddPersistentListener(retry.onClick, runner.Restart);
        UnityEventTools.AddPersistentListener(toMap.onClick, runner.OpenMap);

        Image prompt = Picture(canvas.transform, "午门任务提示", "Assets/Art/WuMen/提示图.png", new Vector2(1360f, 630f), new Vector2(-2200f, 0f));
        runner.missionPrompt = prompt.rectTransform;
        RectTransform videoOverlay = Rect(canvas.transform, "午门片头CG", new Vector2(1920f, 1080f), Vector2.zero);
        Image black = videoOverlay.gameObject.AddComponent<Image>();
        black.color = Color.black;
        black.raycastTarget = true;
        // 给原片留安全边距；即使视频编码含边缘信息，也完整落在屏幕内。
        RectTransform safeVideoFrame = Rect(videoOverlay, "完整视频安全画框", new Vector2(1520f, 850f), Vector2.zero);
        RectTransform videoRect = Rect(safeVideoFrame, "视频画面", new Vector2(1520f, 850f), Vector2.zero);
        RawImage raw = videoRect.gameObject.AddComponent<RawImage>();
        raw.color = Color.white;
        raw.raycastTarget = false;
        AspectRatioFitter fit = videoRect.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        Button skip = ActionButton(videoOverlay, "跳过片头", "跳过片头", new Vector2(230f, 72f), new Vector2(805f, 460f));
        UnityEventTools.AddPersistentListener(skip.onClick, runner.SkipVideo);
        runner.videoOverlay = videoOverlay.gameObject;
        runner.videoScreen = raw;
        VideoPlayer player = new GameObject("午门CG播放器", typeof(VideoPlayer)).GetComponent<VideoPlayer>();
        runner.introPlayer = player;
        runner.introClip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Videos/Scene_one_video.m4v");
        if (runner.introClip == null) throw new Exception("缺少午门片头 Scene_one_video.m4v");
        return Save(scene, GameNavigator.WuMen);
    }

    private static string BuildWuMenKnowledge()
    {
        Scene scene = NewScene(GameNavigator.WuMenKnowledge);
        Canvas canvas = MakeCanvas();
        ColorPanel(canvas.transform, "宣纸黄底", new Vector2(1920f, 1080f), Vector2.zero,
            new Color(0.93f, 0.84f, 0.62f));
        ColorPanel(canvas.transform, "内层纸面", new Vector2(1800f, 950f), new Vector2(0f, -18f),
            new Color(1f, 0.97f, 0.83f, 0.76f));
        ColorPanel(canvas.transform, "朱砂标题线", new Vector2(1710f, 5f), new Vector2(0f, 350f),
            new Color(0.56f, 0.20f, 0.16f));
        Label(canvas.transform, "主题", "午门营造录 · 匠心与脊兽", 61,
            new Vector2(1120f, 95f), new Vector2(-175f, 420f), new Color(0.28f, 0.13f, 0.09f));
        GameNavigator nav = new GameObject("场景导航", typeof(GameNavigator)).GetComponent<GameNavigator>();
        Button exit = ActionButton(canvas.transform, "退出科普", "退出", new Vector2(210f, 75f), new Vector2(790f, 422f));
        UnityEventTools.AddPersistentListener(exit.onClick, nav.OpenMap);

        Picture(canvas.transform, "午门实景照片", "Assets/Art/WuMen/Education/MeridianGate.jpg",
            new Vector2(770f, 465f), new Vector2(-440f, 64f));
        Picture(canvas.transform, "脊兽实景照片", "Assets/Art/WuMen/Education/RoofBeasts.jpg",
            new Vector2(770f, 465f), new Vector2(440f, 64f));
        Label(canvas.transform, "午门图注", "午门 · 紫禁城正门", 32,
            new Vector2(760f, 56f), new Vector2(-440f, -205f), new Color(0.40f, 0.17f, 0.13f));
        Label(canvas.transform, "脊兽图注", "太和殿屋脊上的走兽", 32,
            new Vector2(760f, 56f), new Vector2(440f, -205f), new Color(0.40f, 0.17f, 0.13f));
        Text wuText = Label(canvas.transform, "午门介绍",
            "午门平面呈‘凹’字形，城楼与两翼相互呼应，\n宛如朱雀展翅，因此又称‘五凤楼’。\n层层递进的门庑布局，凝结着匠人的营造智慧。",
            32, new Vector2(780f, 180f), new Vector2(-440f, -330f), new Color(0.22f, 0.16f, 0.11f));
        wuText.alignment = TextAnchor.UpperLeft;
        Text beastText = Label(canvas.transform, "脊兽介绍",
            "走兽立于屋顶檐角，兼具建筑装饰与等级象征。\n太和殿有十只走兽，是故宫建筑中的特例。\n这些精细的琉璃构件，留下了匠人的手艺。",
            32, new Vector2(780f, 180f), new Vector2(440f, -330f), new Color(0.22f, 0.16f, 0.11f));
        beastText.alignment = TextAnchor.UpperLeft;
        Label(canvas.transform, "资料来源", "史料：故宫博物院  ·  摄影：そらみみ、秋水平湖（授权见工程 CREDITS.md）",
            23, new Vector2(1650f, 42f), new Vector2(0f, -480f), new Color(0.37f, 0.26f, 0.18f));
        return Save(scene, GameNavigator.WuMenKnowledge);
    }

    private static string BuildBaoHeDian()
    {
        Scene scene = NewScene(GameNavigator.BaoHeDian);
        Canvas canvas = MakeCanvas();
        Picture(canvas.transform, "水管背景", "Assets/Art/BaoHeDian/水管背景.png", new Vector2(1920f, 1080f), Vector2.zero);
        ColorPanel(canvas.transform, "游戏区底色", new Vector2(850f, 850f), new Vector2(0f, -12f), new Color(0.96f, 0.92f, 0.83f, 0.90f));
        BaoHeDianPipes game = new GameObject("保和殿关卡控制", typeof(BaoHeDianPipes)).GetComponent<BaoHeDianPipes>();
        game.tiles = new Button[25];
        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                int number = (row + 1) * 10 + col + 1;
                int index = row * 5 + col;
                Vector2 position = new Vector2((col - 2) * 158f, (2 - row) * 158f - 12f);
                RectTransform tile = Rect(canvas.transform, "管道" + number, new Vector2(150f, 150f), position);
                Image background = tile.gameObject.AddComponent<Image>();
                background.color = new Color(0.13f, 0.12f, 0.10f, 1f);
                Button button = tile.gameObject.AddComponent<Button>();
                button.targetGraphic = background;
                Image art = Picture(tile, "图片", "Assets/Art/BaoHeDian/拼图/" + number + ".jpg", new Vector2(144f, 144f), Vector2.zero);
                art.preserveAspect = false;
                game.tiles[index] = button;
            }
        }
        ColorPanel(canvas.transform, "顶部底色", new Vector2(1920f, 122f), new Vector2(0f, 478f), new Color(0.07f, 0.07f, 0.07f, 0.69f));
        Label(canvas.transform, "关卡名", "保和殿 · 水管谜题", 53, new Vector2(630f, 92f), new Vector2(-520f, 478f), Color.white);
        game.timerText = Label(canvas.transform, "倒计时", "剩余时间 05:00", 48, new Vector2(460f, 90f), new Vector2(355f, 478f), Color.white);
        Label(canvas.transform, "玩法说明", "点击管道旋转九十度，在五分钟内拼好整幅水路图", 31, new Vector2(1100f, 58f), new Vector2(0f, -486f), new Color(0.18f, 0.12f, 0.12f));
        Button back = ActionButton(canvas.transform, "返回地图", "返回地图", new Vector2(250f, 76f), new Vector2(-820f, 478f));
        UnityEventTools.AddPersistentListener(back.onClick, game.OpenMap);

        RectTransform result = Rect(canvas.transform, "结算面板", new Vector2(650f, 410f), Vector2.zero);
        Image resultImage = result.gameObject.AddComponent<Image>();
        resultImage.color = new Color(0.97f, 0.93f, 0.84f, 0.96f);
        game.resultPanel = result.gameObject;
        game.resultText = Label(result, "结果文字", "", 57, new Vector2(590f, 190f), new Vector2(0f, 90f), new Color(0.15f, 0.10f, 0.10f));
        Button retry = ActionButton(result, "再试一次", "再试一次", new Vector2(270f, 82f), new Vector2(-155f, -120f));
        Button toMap = ActionButton(result, "回到地图", "回到地图", new Vector2(270f, 82f), new Vector2(155f, -120f));
        UnityEventTools.AddPersistentListener(retry.onClick, game.Restart);
        UnityEventTools.AddPersistentListener(toMap.onClick, game.OpenMap);
        return Save(scene, GameNavigator.BaoHeDian);
    }
}
