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
            "脊兽安放在屋脊檐角，兼具装饰与守护寓意。午门寻回的第一件脊兽，提醒墨宝沿着宫殿屋脊继续追寻失落的色彩。",
            "屋脊上的琉璃走兽形态各异，排列次序也体现建筑等级。它们由匠人塑造、烧制，再稳稳安置于檐角。",
            "脊兽在风雨中守望宫阙，也成为故宫屋顶最醒目的细节之一。收集它们，便找回午门屋脊的一部分色彩。",
            "铜鹤是乾清宫殿前的吉祥陈设。鹤的长寿意象寄托着对安宁与祥瑞的祝愿，也体现铸造与装饰工艺。",
            "嘉量是古代度量衡标准器，用来统一容量单位。器身与量值体现了工匠对尺度、铸造和规范的重视。",
            "日晷借助日光形成的影子辨认时刻。晷面、晷针与石质底座共同构成这件观测时间的器物。"
        };
        private bool[] unlocked;

        private void Start()
        {
            if (details != null) details.SetActive(false);
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
                    icons[i].color = unlocked[i] ? Color.white : new Color(0.42f, 0.40f, 0.36f, 0.62f);
                if (names[i] != null) names[i].text = unlocked[i] ? titles[i] : "";
            }
        }

        private void ShowEntry(int index)
        {
            if (index < 0 || index >= entries.Length || !unlocked[index]) return;
            detailTitle.text = titles[index];
            detailBody.text = descriptions[index];
            Texture photo = detailPhotos != null && index < detailPhotos.Length ? detailPhotos[index] : null;
            detailIcon.texture = photo != null ? photo : icons[index].texture;
            detailIcon.uvRect = photo != null ? new Rect(0f, 0f, 1f, 1f) : icons[index].uvRect;
            float width = detailIcon.texture.width * detailIcon.uvRect.width;
            float height = detailIcon.texture.height * detailIcon.uvRect.height;
            float scale = Mathf.Min(410f / width, 350f / height);
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
