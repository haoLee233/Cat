using System;
using System.IO;
using JinYanMaoYing;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

// Applies presentation changes to the existing authored scenes without regenerating them.
public static class FeedbackScenePatch
{
    [MenuItem("锦檐猫影/检查本轮场景配置")]
    public static void ValidateLatestFeedback()
    {
        if (Array.Exists(EditorBuildSettings.scenes, item => item.path.EndsWith("/太和殿营造录.unity") && item.enabled))
            throw new Exception("太和殿营造录仍在构建列表中");
        ValidateScene("故宫地图", () => {
            PalaceMapController map = UnityEngine.Object.FindObjectOfType<PalaceMapController>();
            if (map == null || map.completedMap == null || map.completedMap.sprite == null)
                throw new Exception("主地图完整彩图未配置");
        });
        ValidateScene("保和殿水管", () => {
            BaoHeDianPipes level = UnityEngine.Object.FindObjectOfType<BaoHeDianPipes>();
            if (level == null || level.introVideo == null || level.introVideo.clip == null || level.introVideo.buttonFont == null)
                throw new Exception("保和殿片头或跳过按钮字体未配置");
            if (level.resultPanel.transform.Find("查看保和殿营造录") != null)
                throw new Exception("保和殿旧了解按钮仍存在");
        });
        ValidateScene("保和殿营造录", () => {
            Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (canvas.transform.Find("保和殿外景照片") == null || canvas.transform.Find("保和殿内景照片") == null ||
                canvas.transform.Find("返回地图") != null || canvas.transform.Find("退出科普") == null)
                throw new Exception("保和殿营造录布局不完整");
        });
        ValidateScene("太和殿", () => {
            TaiHeFinale level = UnityEngine.Object.FindObjectOfType<TaiHeFinale>();
            if (level == null || level.introVideo == null || level.introVideo.buttonFont == null)
                throw new Exception("太和殿片头字体未配置");
            Button ending = level.completionPanel.transform.Find("观看结局").GetComponent<Button>();
            if (ending.GetComponentInChildren<Text>(true).text != "观看结局" ||
                ending.onClick.GetPersistentMethodName(0) != "OpenEnding")
                throw new Exception("太和殿通关按钮未恢复结局视频");
        });
        ValidateScene("终章", () => {
            EndingReveal ending = UnityEngine.Object.FindObjectOfType<EndingReveal>();
            if (ending == null || ending.endingClip == null || ending.buttonFont == null)
                throw new Exception("结局视频或跳过字体未配置");
        });
        ValidateScene("乾清宫", () => {
            QianQingExplorer explorer = UnityEngine.Object.FindObjectOfType<QianQingExplorer>();
            RectTransform sundial = explorer.rooms[2].relic;
            float catBottom = explorer.rooms[2].GroundAt(sundial.anchoredPosition.x) + 47f - explorer.cat.rect.height * 0.5f;
            float sundialBottom = sundial.anchoredPosition.y - sundial.rect.height * 0.5f;
            if (Mathf.Abs(catBottom - sundialBottom) > 1f)
                throw new Exception("日晷与猫的图片底边没有对齐");
        });
        ValidateScene("匠心图鉴", () => {
            CollectionGalleryController gallery = UnityEngine.Object.FindObjectOfType<CollectionGalleryController>();
            if (gallery == null || gallery.detailPhotos == null || gallery.detailPhotos.Length != 6)
                throw new Exception("图鉴实景照片未配置");
            for (int i = 0; i < 6; i++)
            {
                if (gallery.detailPhotos[i] == null || gallery.icons[i] == null) throw new Exception("图鉴藏品资源缺失: " + i);
                RawImage icon = gallery.icons[i];
                float sourceRatio = icon.texture.width * icon.uvRect.width / (icon.texture.height * icon.uvRect.height);
                float displayRatio = icon.rectTransform.rect.width / icon.rectTransform.rect.height;
                if (Mathf.Abs(sourceRatio - displayRatio) > 0.02f) throw new Exception("图鉴藏品比例有误: " + i);
            }
        });
        Debug.Log("LATEST_FEEDBACK_VALIDATED");
    }

    private static void ValidateScene(string name, Action check)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/" + name + ".unity", OpenSceneMode.Single);
        check();
    }

    [MenuItem("锦檐猫影/生成本轮反馈 Windows 版")]
    public static void BuildPreview()
    {
        string[] names = {
            "封面", "故宫地图", "午门追逐", "乾清宫", "保和殿水管",
            "午门营造录", "乾清宫营造录", "保和殿营造录", "太和殿", "匠心图鉴", "终章"
        };
        string[] scenes = Array.ConvertAll(names, name => "Assets/Scenes/" + name + ".unity");
        string output = Path.GetFullPath("Builds/WindowsFeedbackPreview/JinYanMaoYing.exe");
        Directory.CreateDirectory(Path.GetDirectoryName(output));
        BuildPlayerOptions options = new BuildPlayerOptions {
            scenes = scenes,
            locationPathName = output,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("本轮反馈 Windows 构建失败: " + report.summary.result);
        Debug.Log("FEEDBACK_WINDOWS_BUILD_READY " + output);
    }

    [MenuItem("锦檐猫影/应用本轮反馈到场景")]
    public static void Apply()
    {
        string[] scenes = {
            "封面", "故宫地图", "午门追逐", "乾清宫", "保和殿水管",
            "午门营造录", "乾清宫营造录", "保和殿营造录", "太和殿", "终章"
        };
        foreach (string name in scenes) PatchButtons("Assets/Scenes/" + name + ".unity");
        PatchEnding();
        AssetDatabase.SaveAssets();
        Debug.Log("FEEDBACK_SCENES_READY");
    }

    [MenuItem("锦檐猫影/接入第三第四段关卡CG和结局ED")]
    public static void ApplyLevelIntros()
    {
        PatchLevelIntro("Assets/Scenes/保和殿水管.unity", "Assets/Videos/第三.m4v", true);
        PatchLevelIntro("Assets/Scenes/太和殿.unity", "Assets/Videos/太和殿cg.m4v", false);
        PatchEnding();
        AssetDatabase.SaveAssets();
        Debug.Log("LEVEL_INTROS_AND_ENDING_READY");
    }

    private static void PatchLevelIntro(string scenePath, string clipPath, bool baoHe)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        MonoBehaviour controller = baoHe
            ? (MonoBehaviour)UnityEngine.Object.FindObjectOfType<BaoHeDianPipes>()
            : UnityEngine.Object.FindObjectOfType<TaiHeFinale>();
        if (controller == null) throw new Exception("关卡控制器缺失: " + scenePath);
        VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>(clipPath);
        if (clip == null) throw new Exception("关卡视频素材缺失: " + clipPath);
        LevelIntroVideo intro = controller.GetComponent<LevelIntroVideo>();
        if (intro == null) intro = controller.gameObject.AddComponent<LevelIntroVideo>();
        intro.clip = clip;
        if (baoHe) ((BaoHeDianPipes)controller).introVideo = intro;
        else ((TaiHeFinale)controller).introVideo = intro;
        EditorSceneManager.SaveScene(scene);
    }

    private static void PatchButtons(string path)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (canvas == null) throw new Exception("找不到场景画布: " + path);
        foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
        {
            if (button.GetComponent<Image>() == null || button.GetComponentInChildren<Text>(true) == null) continue;
            if (button.GetComponent<InkHoverButton>() == null) button.gameObject.AddComponent<InkHoverButton>();
        }
        if (path.EndsWith("封面.unity"))
        {
            Transform premise = canvas.transform.Find("旅程缘起");
            if (premise != null)
            {
                Text label = premise.GetComponent<Text>();
                if (label != null) label.fontSize = 36;
                premise.GetComponent<RectTransform>().sizeDelta = new Vector2(1500f, 90f);
            }
            PatchCoverJourneyButtons(canvas);
        }
        EditorSceneManager.SaveScene(scene);
    }

    private static void PatchCoverJourneyButtons(Canvas canvas)
    {
        Transform backdrop = canvas.transform.Find("点击提示底色");
        Transform continueButton = canvas.transform.Find("开始游戏");
        GameNavigator nav = UnityEngine.Object.FindObjectOfType<GameNavigator>();
        if (backdrop == null || continueButton == null || nav == null)
            throw new Exception("封面进度按钮配置缺失");

        backdrop.GetComponent<RectTransform>().sizeDelta = new Vector2(1260f, 130f);
        RectTransform continueRect = continueButton.GetComponent<RectTransform>();
        continueRect.sizeDelta = new Vector2(550f, 92f);
        continueRect.anchoredPosition = new Vector2(-295f, -420f);
        Text continueLabel = continueButton.GetComponentInChildren<Text>(true);
        continueLabel.text = "继续当前进度";

        Transform existing = canvas.transform.Find("从头开始");
        if (existing != null) return;
        GameObject fresh = new GameObject("从头开始", typeof(RectTransform), typeof(CanvasRenderer),
            typeof(Image), typeof(Button));
        fresh.transform.SetParent(canvas.transform, false);
        RectTransform rect = fresh.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(550f, 92f);
        rect.anchoredPosition = new Vector2(295f, -420f);
        fresh.GetComponent<Image>().color = new Color(0.14f, 0.11f, 0.11f, 0.87f);
        Button button = fresh.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(button.onClick, nav.StartNewJourney);
        GameObject textObject = new GameObject("文字", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(fresh.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        Text label = textObject.GetComponent<Text>();
        label.font = continueLabel.font;
        label.fontSize = 44;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(1f, 0.94f, 0.84f);
        label.text = "从头开始";
        label.raycastTarget = false;
        fresh.AddComponent<InkHoverButton>();
    }

    private static void PatchEnding()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/终章.unity", OpenSceneMode.Single);
        EndingReveal reveal = UnityEngine.Object.FindObjectOfType<EndingReveal>();
        if (reveal == null) throw new Exception("终章播放控制器缺失");
        VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Videos/游戏结尾cg.m4v");
        if (clip == null) throw new Exception("结局ED视频 游戏结尾cg.m4v 缺失");
        reveal.endingClip = clip;
        EditorSceneManager.SaveScene(scene);
    }
}
