using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class CollectionGalleryController : MonoBehaviour
    {
        public Button[] entries;
        public RawImage[] icons;
        public Text[] names;
        public GameObject[] lockedMarks;
        public GameObject details;
        public RawImage detailIcon;
        public Text detailTitle;
        public Text detailBody;
        public CanvasGroup detailGroup;
        public Texture[] detailPhotos;

        private readonly string[] titles = { "龙形脊兽", "凤形脊兽", "瑞兽脊兽", "铜鹤", "嘉量", "日晷" };
        private readonly string[] descriptions = {
            "龙形脊兽：留意水墨造型中盘曲的身姿。屋脊走兽由匠人塑形、烧制、安装，既装点檐角，也寄托守护宫殿的心愿。",
            "凤形脊兽：鸟形轮廓与龙形脊兽不同。观察羽翼和头部的线条，想一想匠人如何让屋脊上相邻的形象各有姿态。",
            "瑞兽脊兽：它代表传统屋脊走兽中的吉祥形象。三只脊兽一同归位后，午门的屋檐才逐渐恢复色彩。",
            "铜鹤是乾清宫殿前的吉祥陈设。鹤的长寿意象寄托着对安宁与祥瑞的祝愿，也体现铸造与装饰工艺。",
            "嘉量是古代度量衡标准器，用来统一容量单位。器身与量值体现了工匠对尺度、铸造和规范的重视。",
            "日晷借助日光形成的影子辨认时刻。晷面、晷针与石质底座共同构成这件观测时间的器物。"
        };
        private bool[] unlocked;

        private void Start()
        {
            if (details != null) details.SetActive(false);
            Font readable = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 30);
            if (detailTitle != null)
            {
                detailTitle.font = readable;
                detailTitle.fontSize = 46;
                detailTitle.rectTransform.anchoredPosition = new Vector2(150f, 215f);
                detailTitle.rectTransform.sizeDelta = new Vector2(590f, 86f);
            }
            if (detailBody != null)
            {
                detailBody.font = readable;
                detailBody.fontSize = 29;
                detailBody.lineSpacing = 1.25f;
                detailBody.rectTransform.anchoredPosition = new Vector2(150f, -5f);
                detailBody.rectTransform.sizeDelta = new Vector2(590f, 340f);
            }
            unlocked = new[] {
                GameNavigator.WuMenCleared, GameNavigator.WuMenCleared, GameNavigator.WuMenCleared,
                GameNavigator.QianQingGongCleared, GameNavigator.QianQingGongCleared, GameNavigator.QianQingGongCleared
            };
            for (int i = 0; i < entries.Length; i++)
            {
                int index = i;
                if (entries[i] != null)
                {
                    entries[i].interactable = unlocked[i];
                    entries[i].onClick.AddListener(() => ShowEntry(index));
                }
                if (lockedMarks != null && i < lockedMarks.Length && lockedMarks[i] != null)
                    lockedMarks[i].SetActive(!unlocked[i]);
                if (icons[i] != null)
                {
                    icons[i].color = unlocked[i] ? Color.white : new Color(0.42f, 0.40f, 0.36f, 0.62f);
                    Texture texture = icons[i].texture;
                    if (texture != null)
                    {
                        float width = texture.width * icons[i].uvRect.width;
                        float height = texture.height * icons[i].uvRect.height;
                        float scale = Mathf.Min(150f / Mathf.Max(1f, width), 86f / Mathf.Max(1f, height));
                        icons[i].rectTransform.sizeDelta = new Vector2(width * scale, height * scale);
                        icons[i].rectTransform.anchoredPosition = new Vector2(0f, 23f);
                    }
                }
                if (names[i] != null) names[i].text = unlocked[i] ? titles[i] : "";
            }
        }

        private void ShowEntry(int index)
        {
            if (index < 0 || index >= entries.Length || !unlocked[index]) return;
            detailTitle.text = titles[index];
            detailBody.text = descriptions[index];
            // Each roof beast has its own artwork; one shared roof photograph made all three entries look identical.
            Texture photo = index < 3 ? null : detailPhotos != null && index < detailPhotos.Length ? detailPhotos[index] : null;
            detailIcon.texture = photo != null ? photo : icons[index].texture;
            detailIcon.uvRect = photo != null ? new Rect(0f, 0f, 1f, 1f) : icons[index].uvRect;
            float width = detailIcon.texture.width * detailIcon.uvRect.width;
            float height = detailIcon.texture.height * detailIcon.uvRect.height;
            // Artifact photographs use the inner paper area; the wider beast art
            // keeps its existing placement and size.
            bool artifact = index >= 3;
            detailIcon.rectTransform.anchoredPosition = new Vector2(artifact ? -300f : -375f, 10f);
            float scale = Mathf.Min((artifact ? 290f : 410f) / width,
                (artifact ? 300f : 350f) / height);
            detailIcon.rectTransform.sizeDelta = new Vector2(width * scale, height * scale);
            details.SetActive(true);
            if (detailGroup != null)
            {
                detailGroup.alpha = 0f;
                StartCoroutine(FadeIn());
            }
        }

        private IEnumerator FadeIn()
        {
            for (float t = 0f; t < 0.28f; t += Time.unscaledDeltaTime)
            {
                detailGroup.alpha = Mathf.SmoothStep(0f, 1f, t / 0.28f);
                yield return null;
            }
            detailGroup.alpha = 1f;
        }

        public void CloseDetails() { if (details != null) details.SetActive(false); }
        public void ReturnToMap() { SceneTransition.Load(GameNavigator.Map); }
    }
}
