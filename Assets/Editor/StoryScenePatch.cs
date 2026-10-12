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
