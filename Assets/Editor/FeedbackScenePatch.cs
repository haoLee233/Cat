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
    [MenuItem("锦檐猫影/生成本轮反馈 Windows 版")]
    public static void BuildPreview()
    {
        string[] names = {
            "封面", "故宫地图", "午门追逐", "乾清宫", "保和殿水管",
            "午门营造录", "乾清宫营造录", "太和殿", "终章"
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
            "午门营造录", "乾清宫营造录", "太和殿", "终章"
        };
        foreach (string name in scenes) PatchButtons("Assets/Scenes/" + name + ".unity");
        PatchEnding();
        AssetDatabase.SaveAssets();
        Debug.Log("FEEDBACK_SCENES_READY");
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
        VideoClip clip = AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Videos/Scene_end_video.m4v");
        if (clip == null) throw new Exception("结局ED视频 Scene_end_video.m4v 缺失");
        reveal.endingClip = clip;
        EditorSceneManager.SaveScene(scene);
    }
}
