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
        string qianQing = BuildQianQingGong();
        string baoHeDian = BuildBaoHeDian();
        string knowledge = BuildWuMenKnowledge();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(cover, true),
            new EditorBuildSettingsScene(map, true),
            new EditorBuildSettingsScene(wuMen, true),
            new EditorBuildSettingsScene(qianQing, true),
            new EditorBuildSettingsScene(baoHeDian, true),
            new EditorBuildSettingsScene(knowledge, true),
        };
        PlayerSettings.productName = "锦檐猫影：筑梦人";
        PlayerSettings.companyName = "锦檐猫影";
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(cover);
        Debug.Log("REMAKE_SCENES_READY " + cover + ", " + map + ", " + wuMen + ", " + qianQing + ", " + baoHeDian + ", " + knowledge);
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
        const string mapArt = "Assets/Art/主地图/地图/";
        Picture(canvas.transform, "黑白故宫地图", mapArt + "地图_黑白.png", new Vector2(1920f, 1080f), Vector2.zero);
        PalaceMapController controller = new GameObject("地图进度与入口", typeof(PalaceMapController)).GetComponent<PalaceMapController>();
        controller.colorLayers = new GameObject[4];
        controller.colorLayers[0] = Picture(canvas.transform, "午门通关彩色层", mapArt + "午门.png", new Vector2(1920f, 1080f), Vector2.zero).gameObject;
        controller.colorLayers[1] = Picture(canvas.transform, "乾清宫通关彩色层", mapArt + "乾清宫_彩色.png", new Vector2(1920f, 1080f), Vector2.zero).gameObject;
        controller.colorLayers[2] = Picture(canvas.transform, "保和殿通关彩色层", mapArt + "保和殿_彩色.png", new Vector2(1920f, 1080f), Vector2.zero).gameObject;
        controller.colorLayers[3] = Picture(canvas.transform, "太和殿通关彩色层", mapArt + "太和殿_彩色.png", new Vector2(1920f, 1080f), Vector2.zero).gameObject;
        ColorPanel(canvas.transform, "标题墨色", new Vector2(760f, 124f), new Vector2(0f, 475f), new Color(0.10f, 0.09f, 0.08f, 0.70f));
        Label(canvas.transform, "地图标题", "故宫寻迹", 68, new Vector2(700f, 112f), new Vector2(0f, 475f), Color.white);
        GameNavigator nav = new GameObject("场景导航", typeof(GameNavigator)).GetComponent<GameNavigator>();
        AddMapHotspot(canvas.transform, controller, 0, "午门", mapArt + "午门（字体）.png", new Vector2(-378f, -302f));
        AddMapHotspot(canvas.transform, controller, 1, "乾清宫", mapArt + "乾清宫（字体）.png", new Vector2(184f, 86f));
        AddMapHotspot(canvas.transform, controller, 2, "保和殿", mapArt + "保和殿（字体）.png", new Vector2(595f, 252f));
        AddMapHotspot(canvas.transform, controller, 3, "太和殿", mapArt + "太和殿（字体）.png", new Vector2(-225f, 280f));
        Button back = ActionButton(canvas.transform, "返回封面", "返回封面", new Vector2(260f, 76f), new Vector2(-805f, 438f));
        UnityEventTools.AddPersistentListener(back.onClick, nav.OpenCover);
        Button quit = ActionButton(canvas.transform, "退出游戏", "退出游戏", new Vector2(260f, 76f), new Vector2(805f, 438f));
        UnityEventTools.AddPersistentListener(quit.onClick, nav.Quit);

        RectTransform modal = Rect(canvas.transform, "未解锁提示", new Vector2(1920f, 1080f), Vector2.zero);
        Image dim = modal.gameObject.AddComponent<Image>();
        dim.color = new Color(0.07f, 0.06f, 0.05f, 0.50f);
        dim.raycastTarget = true;
        Picture(modal, "水墨提示框", "Assets/Art/WuMen/提示图.png", new Vector2(1000f, 463f), Vector2.zero);
        controller.promptText = Label(modal, "提示内容", "当前关卡未解锁，\n请完成所有前置关卡再来挑战吧！", 45,
            new Vector2(790f, 210f), new Vector2(0f, 42f), new Color(0.17f, 0.12f, 0.10f));
        Button close = ActionButton(modal, "关闭提示", "知晓了", new Vector2(225f, 66f), new Vector2(0f, -137f));
        UnityEventTools.AddPersistentListener(close.onClick, controller.ClosePrompt);
        controller.lockedPrompt = modal.gameObject;
        modal.gameObject.SetActive(false);
        return Save(scene, GameNavigator.Map);
    }

    private static void AddMapHotspot(Transform parent, PalaceMapController controller, int index,
        string label, string fontPath, Vector2 position)
    {
        RectTransform hit = Rect(parent, label + "建筑点击区", new Vector2(230f, 215f), position);
        Image input = hit.gameObject.AddComponent<Image>();
        input.color = new Color(1f, 1f, 1f, 0f);
        input.raycastTarget = true;
        PalaceMapHotspot hotspot = hit.gameObject.AddComponent<PalaceMapHotspot>();
        hotspot.controller = controller;
        hotspot.index = index;
        Image marker = Picture(hit, label + "水墨图标", fontPath,
            new Vector2(150f, 235f), Vector2.zero);
        hotspot.marker = marker.rectTransform;
        hotspot.markerImage = marker;
        Text name = Label(hit, label + "悬停场景名", label, 45,
            new Vector2(205f, 75f), new Vector2(160f, 18f), new Color(0.18f, 0.12f, 0.10f));
        name.gameObject.SetActive(false);
        hotspot.hoverName = name.gameObject;
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

    private static string BuildQianQingGong()
    {
        Scene scene = NewScene(GameNavigator.QianQingGong);
        Canvas canvas = MakeCanvas();
        QianQingExplorer game = new GameObject("乾清宫探索控制", typeof(QianQingExplorer)).GetComponent<QianQingExplorer>();
        game.rooms = new QianQingRoom[3];
        string art = "Assets/Art/QianQingGong/";
        string[] backgrounds = { "乾清宫殿外_新绘.png", "乾清宫丹陛_新绘.png", "乾清宫殿内_新绘.png" };
        string[] names = { "乾清宫前庭", "丹陛高台", "乾清宫正殿" };
        string[] relicNames = { "铜鹤", "嘉量", "正大光明匾" };
        Vector2[] relicPositions = { new Vector2(490f, -126f), new Vector2(495f, 108f), new Vector2(475f, 183f) };
        Texture2D relicSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(art + "乾清宫器物图鉴.png");
        if (relicSheet == null) throw new Exception("缺少乾清宫器物图鉴");
        for (int i = 0; i < 3; i++)
        {
            RectTransform root = Rect(canvas.transform, names[i], new Vector2(1920f, 1080f), Vector2.zero);
            QianQingRoom room = root.gameObject.AddComponent<QianQingRoom>();
            game.rooms[i] = room;
            Image background = Picture(root, "场景水墨画", art + backgrounds[i], new Vector2(1920f, 1080f), Vector2.zero);
            background.preserveAspect = false;
            background.color = new Color(0.88f, 0.86f, 0.81f, 1f);
            ColorPanel(root, "地面碰撞引导", new Vector2(1920f, 18f), new Vector2(0f, -362f), new Color(0.35f, 0.28f, 0.22f, 0.55f));
            Vector2[] steps;
            if (i == 0) steps = new[] { new Vector2(-220f, -235f), new Vector2(85f, -112f), new Vector2(405f, 8f) };
            else if (i == 1) steps = new[] { new Vector2(-530f, -235f), new Vector2(-240f, -112f), new Vector2(55f, 10f), new Vector2(340f, 130f) };
            else steps = new[] { new Vector2(-525f, -235f), new Vector2(-230f, -112f), new Vector2(65f, 10f), new Vector2(365f, 130f) };
            room.platforms = new RectTransform[steps.Length];
            for (int p = 0; p < steps.Length; p++)
            {
                RectTransform stone = Rect(root, "可跳跃石阶" + (p + 1), new Vector2(250f, 27f), steps[p]);
                Image stoneImage = stone.gameObject.AddComponent<Image>();
                stoneImage.color = new Color(0.53f, 0.48f, 0.41f, 0.94f);
                stoneImage.raycastTarget = false;
                ColorPanel(stone, "石阶亮边", new Vector2(250f, 6f), new Vector2(0f, 11f), new Color(0.96f, 0.90f, 0.75f, 0.92f));
                room.platforms[p] = stone;
            }
            RectTransform relic = Rect(root, relicNames[i] + "可查阅器物", i == 2 ? new Vector2(245f, 145f) : new Vector2(145f, 145f), relicPositions[i]);
            RawImage icon = relic.gameObject.AddComponent<RawImage>();
            icon.texture = relicSheet;
            icon.uvRect = new Rect(i / 3f, 0f, 1f / 3f, 1f);
            icon.raycastTarget = false;
            room.relic = relic;
            if (i == 2) Label(relic, "题字", "正大光明", 27, new Vector2(200f, 55f), Vector2.zero, new Color(0.18f, 0.13f, 0.09f));
            Label(root, relicNames[i] + "名称", relicNames[i] + "  ·  按 E 查阅", 30,
                new Vector2(330f, 62f), relicPositions[i] + new Vector2(0f, -112f), new Color(0.18f, 0.12f, 0.09f));
            root.gameObject.SetActive(i == 0);
        }
        Image cat = Picture(canvas.transform, "探索黑猫", "Assets/Art/WuMen/跑动1.png", new Vector2(160f, 98f), new Vector2(-760f, -306f));
        game.cat = cat.rectTransform;
        game.catImage = cat;
        game.runFrames = new[] {
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/跑动1.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/跑动2.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/跑动3.png")
        };
        Image miniMap = Picture(canvas.transform, "左上角水墨小地图", "Assets/Art/WuMen/WM_Dialog_Box.png",
            new Vector2(520f, 235f), new Vector2(-672f, 412f));
        miniMap.color = new Color(1f, 1f, 1f, 0.91f);
        Label(canvas.transform, "小地图标题", "乾清宫 · 探索地图", 36, new Vector2(440f, 56f),
            new Vector2(-672f, 480f), new Color(0.20f, 0.13f, 0.10f));
        game.mapNodes = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            float x = -820f + i * 150f;
            game.mapNodes[i] = ColorPanel(canvas.transform, "地图节点" + i, new Vector2(25f, 25f),
                new Vector2(x, 415f), new Color(0.35f, 0.31f, 0.27f));
            Label(canvas.transform, "节点名称" + i, new[] { "前庭", "丹陛", "正殿" }[i], 27,
                new Vector2(138f, 50f), new Vector2(x, 370f), new Color(0.20f, 0.13f, 0.10f));
        }
        ColorPanel(canvas.transform, "顶部关卡底色", new Vector2(660f, 112f),
            new Vector2(192f, 465f), new Color(0.13f, 0.10f, 0.09f, 0.76f));
        game.roomTitle = Label(canvas.transform, "当前场景名", "乾清宫前庭", 49,
            new Vector2(590f, 76f), new Vector2(192f, 471f), new Color(1f, 0.94f, 0.83f));
        game.status = Label(canvas.transform, "图鉴进度", "已查阅器物 0/3", 35,
            new Vector2(480f, 65f), new Vector2(615f, 390f), new Color(0.22f, 0.14f, 0.10f));
        ColorPanel(canvas.transform, "底部提示底色", new Vector2(1300f, 80f),
            new Vector2(0f, -475f), new Color(0.96f, 0.89f, 0.74f, 0.84f));
        game.hint = Label(canvas.transform, "玩法提示", "", 33, new Vector2(1240f, 70f),
            new Vector2(0f, -475f), new Color(0.19f, 0.13f, 0.11f));
        Button back = ActionButton(canvas.transform, "返回地图按钮", "返回地图", new Vector2(210f, 70f), new Vector2(825f, 475f));
        UnityEventTools.AddPersistentListener(back.onClick, game.ReturnToMap);

        RectTransform result = Rect(canvas.transform, "探索完成", new Vector2(860f, 525f), Vector2.zero);
        Image panel = Picture(result, "卷轴面板", "Assets/Art/WuMen/提示图.png", new Vector2(850f, 465f), Vector2.zero);
        panel.raycastTarget = false;
        Label(result, "通关标题", "乾清宫寻迹完成", 65, new Vector2(700f, 100f), new Vector2(0f, 88f), new Color(0.20f, 0.13f, 0.10f));
        Label(result, "器物说明", "铜鹤 · 嘉量 · 正大光明匾\n三处器物已收入图鉴", 36,
            new Vector2(680f, 110f), new Vector2(0f, -20f), new Color(0.25f, 0.17f, 0.13f));
        Button complete = ActionButton(result, "返回故宫地图", "返回故宫地图", new Vector2(310f, 72f), new Vector2(0f, -145f));
        UnityEventTools.AddPersistentListener(complete.onClick, game.ReturnToMap);
        game.ending = result.gameObject;
        result.gameObject.SetActive(false);
        Image fadeImage = ColorPanel(canvas.transform, "区域切换渐隐", new Vector2(1920f, 1080f), Vector2.zero, Color.black);
        game.fade = fadeImage.gameObject.AddComponent<CanvasGroup>();
        fadeImage.raycastTarget = false;
        return Save(scene, GameNavigator.QianQingGong);
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
