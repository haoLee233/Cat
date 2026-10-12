using System;
using System.Reflection;
using JinYanMaoYing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Updates only the scenes affected by the story pass; other authored scenes stay intact.
public static class StoryScenePatch
{
    public static void Apply()
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/STXINGKA.TTF");
        if (font == null) throw new Exception("缺少书法字体");
        PatchCover(font);
        PatchMap(font);
        FieldInfo fontField = typeof(RebuildScenes).GetField("font", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo buildBaoHe = typeof(RebuildScenes).GetMethod("BuildBaoHeDian", BindingFlags.NonPublic | BindingFlags.Static);
        if (fontField == null || buildBaoHe == null) throw new Exception("无法找到保和殿场景生成器");
        fontField.SetValue(null, font);
        buildBaoHe.Invoke(null, null);
        AssetDatabase.SaveAssets();
        Debug.Log("REMAKE_STORY_SCENES_READY");
    }

    public static void ApplyVisual()
    {
        PatchMapInk();
        PatchEnding();
        AssetDatabase.SaveAssets();
        Debug.Log("REMAKE_VISUAL_SCENES_READY");
    }

    public static void ApplyCatMotion()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/乾清宫.unity", OpenSceneMode.Single);
        QianQingExplorer explorer = UnityEngine.Object.FindObjectOfType<QianQingExplorer>();
        if (explorer == null || explorer.catImage == null) throw new Exception("乾清宫黑猫配置缺失");
        explorer.idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/WuMen/趴下.png");
        if (explorer.idleSprite == null) throw new Exception("黑猫休息姿态缺失");
        explorer.catImage.preserveAspect = true;
        EditorSceneManager.SaveScene(scene);
        Debug.Log("REMAKE_CAT_MOTION_READY");
    }

    private static void PatchMapInk()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/故宫地图.unity", OpenSceneMode.Single);
        Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        PalaceMapController controller = UnityEngine.Object.FindObjectOfType<PalaceMapController>();
        if (canvas == null || controller == null) throw new Exception("地图画布或控制器缺失");
        Transform existing = canvas.transform.Find("入画墨色");
        if (existing == null)
        {
            RectTransform rect = AddRect(canvas.transform, "入画墨色", new Vector2(1920f, 1080f), Vector2.zero);
            Image ink = rect.gameObject.AddComponent<Image>();
            ink.color = new Color(0.07f, 0.07f, 0.07f, 0.83f);
            ink.raycastTarget = false;
            controller.openingInk = rect.gameObject.AddComponent<CanvasGroup>();
            Transform modal = canvas.transform.Find("未解锁提示");
            if (modal != null) rect.SetSiblingIndex(modal.GetSiblingIndex());
        }
        else controller.openingInk = existing.GetComponent<CanvasGroup>();
        EditorSceneManager.SaveScene(scene);
    }

    private static void PatchEnding()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/终章.unity", OpenSceneMode.Single);
        Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (canvas == null) throw new Exception("终章画布缺失");
        if (canvas.transform.Find("终章故事") != null) return;
        Transform dim = canvas.transform.Find("阅读遮罩");
        if (dim == null) throw new Exception("终章阅读遮罩缺失");
        Sprite gray = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/主地图/地图/地图_黑白.png");
        if (gray == null) throw new Exception("黑白故宫地图缺失");
        RectTransform grayRect = AddRect(canvas.transform, "最后的失色画卷", new Vector2(1920f, 1080f), Vector2.zero);
        Image grayImage = grayRect.gameObject.AddComponent<Image>();
        grayImage.sprite = gray;
        grayImage.preserveAspect = false;
        grayImage.raycastTarget = false;
        grayRect.SetSiblingIndex(dim.GetSiblingIndex());
        dim.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.04f, 0.27f);
        RectTransform story = AddRect(canvas.transform, "终章故事", new Vector2(1920f, 1080f), Vector2.zero);
        string[] children = { "结局卷轴", "结局标题", "旅程记录", "结局正文", "再看故宫地图", "返回封面" };
        foreach (string name in children)
        {
            Transform child = canvas.transform.Find(name);
            if (child == null) throw new Exception("终章对象缺失: " + name);
            child.SetParent(story, false);
        }
        EndingReveal reveal = new GameObject("终章画卷复色", typeof(EndingReveal)).GetComponent<EndingReveal>();
        reveal.monochromeMap = grayRect.gameObject.AddComponent<CanvasGroup>();
        reveal.story = story.gameObject.AddComponent<CanvasGroup>();
        EditorSceneManager.SaveScene(scene);
    }

    private static void PatchCover(Font font)
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/封面.unity", OpenSceneMode.Single);
        Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        if (canvas == null) throw new Exception("封面画布缺失");
        if (canvas.transform.Find("旅程缘起") == null)
            AddText(canvas.transform, "旅程缘起", "墨宝拾起一幅褪色的故宫画卷，沿着匠人的线索找回四处色彩。",
                font, 30, new Vector2(1300f, 70f), new Vector2(0f, -320f));
        EditorSceneManager.SaveScene(scene);
    }

    private static void PatchMap(Font font)
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/故宫地图.unity", OpenSceneMode.Single);
        Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
        PalaceMapController controller = UnityEngine.Object.FindObjectOfType<PalaceMapController>();
        if (canvas == null || controller == null) throw new Exception("地图画布或控制器缺失");
        if (canvas.transform.Find("墨宝的任务") == null)
        {
            RectTransform panel = AddRect(canvas.transform, "任务卷轴底色", new Vector2(1660f, 100f), new Vector2(0f, -467f));
            panel.gameObject.AddComponent<Image>().color = new Color(0.96f, 0.90f, 0.78f, 0.88f);
            Text mission = AddText(canvas.transform, "墨宝的任务", "", font, 30,
                new Vector2(1570f, 86f), new Vector2(0f, -467f));
            Transform modal = canvas.transform.Find("未解锁提示");
            if (modal != null)
            {
                panel.SetSiblingIndex(modal.GetSiblingIndex());
                mission.transform.SetSiblingIndex(modal.GetSiblingIndex());
            }
            controller.missionText = mission;
        }
        EditorSceneManager.SaveScene(scene);
    }

    private static RectTransform AddRect(Transform parent, string name, Vector2 size, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    private static Text AddText(Transform parent, string name, string content, Font font, int size,
        Vector2 dimensions, Vector2 position)
    {
        RectTransform rect = AddRect(parent, name, dimensions, position);
        Text label = rect.gameObject.AddComponent<Text>();
        label.text = content;
        label.font = font;
        label.fontSize = size;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = new Color(0.19f, 0.13f, 0.10f);
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;
        return label;
    }
}
