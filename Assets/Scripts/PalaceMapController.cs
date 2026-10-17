using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class PalaceMapController : MonoBehaviour
    {
        // 顺序固定：午门、乾清宫、保和殿、太和殿。
        public GameObject[] colorLayers;
        public GameObject lockedPrompt;
        public Text promptText;
        public Text missionText;
        public CanvasGroup openingInk;
        public Image completedMap;

        private readonly string[] missions = {
            "墨宝拾起褪色的故宫画卷。先到午门找回屋脊脊兽，让城门重新上色。",
            "午门已复色。去乾清宫观察三件器物，找回匠人的尺度与时间。",
            "乾清宫已复色。到保和殿复原殿宇营造图，让阶陛与屋檐重现。",
            "保和殿已复色。去太和殿按建筑层次落下最后一笔。",
            "四处宫殿已经复色。回看这幅画卷，也回看匠人的故事。"
        };

        private void Start()
        {
            if (colorLayers == null || colorLayers.Length < 4) return;
            // Older map scenes stored the central Taihe plate in the Qianqing slot
            // and the north-west Qianqing plate in the Taihe slot.
            Image qianqing = colorLayers[1].GetComponent<Image>();
            Image taihe = colorLayers[3].GetComponent<Image>();
            if (qianqing != null && taihe != null && qianqing.sprite != null && taihe.sprite != null &&
                qianqing.sprite.name == "乾清宫_彩色" && taihe.sprite.name == "太和殿_彩色")
            {
                Sprite plate = qianqing.sprite;
                qianqing.sprite = taihe.sprite;
                taihe.sprite = plate;
            }
            // Keep each extracted color plate within its own building area.
            ClipColorLayer(0, new Vector2(-450f, -255f), new Vector2(1080f, 570f));
            ClipColorLayer(1, new Vector2(-450f, 210f), new Vector2(1050f, 580f));
            ClipColorLayer(2, new Vector2(670f, 180f), new Vector2(580f, 700f));
            ClipColorLayer(3, new Vector2(270f, 40f), new Vector2(650f, 600f));
            int progress = GameNavigator.TaiHeDianCleared ? 4 : GameNavigator.BaoHeDianCleared ? 3 :
                GameNavigator.QianQingGongCleared ? 2 : GameNavigator.WuMenCleared ? 1 : 0;
            int shown = Mathf.Clamp(PlayerPrefs.GetInt("MapShownRecolorCount", 0), 0, progress);
            if (completedMap != null)
            {
                completedMap.gameObject.SetActive(progress == 4);
                CanvasGroup fullGroup = completedMap.GetComponent<CanvasGroup>();
                if (fullGroup == null) fullGroup = completedMap.gameObject.AddComponent<CanvasGroup>();
                fullGroup.alpha = shown == 4 ? 1f : 0f;
                fullGroup.blocksRaycasts = false;
            }
            for (int i = 0; i < 4; i++)
            {
                colorLayers[i].SetActive(i < progress);
                CanvasGroup group = colorLayers[i].GetComponent<CanvasGroup>();
                if (group == null) group = colorLayers[i].AddComponent<CanvasGroup>();
                group.alpha = i < shown ? 1f : 0f;
                group.blocksRaycasts = false;
            }
            if (missionText != null)
            {
                missionText.text = missions[progress];
                missionText.fontSize = Mathf.Max(missionText.fontSize, 38);
                Outline outline = missionText.GetComponent<Outline>();
                if (outline == null) outline = missionText.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.97f, 0.84f, 0.9f);
                outline.effectDistance = new Vector2(2f, -2f);
                StartCoroutine(PulseMission());
            }
            if (lockedPrompt != null) lockedPrompt.SetActive(false);
            StartCoroutine(RevealMap(shown, progress));
        }

        private void ClipColorLayer(int index, Vector2 position, Vector2 size)
        {
            GameObject artwork = colorLayers[index];
            if (artwork == null) return;
            RectTransform imageRect = artwork.GetComponent<RectTransform>();
            if (imageRect == null || imageRect.parent == null || imageRect.parent.GetComponent<RectMask2D>() != null) return;
            Transform parent = imageRect.parent;
            int sibling = imageRect.GetSiblingIndex();
            GameObject mask = new GameObject(artwork.name + "范围", typeof(RectTransform), typeof(RectMask2D));
            RectTransform maskRect = mask.GetComponent<RectTransform>();
            maskRect.SetParent(parent, false);
            maskRect.anchorMin = maskRect.anchorMax = new Vector2(0.5f, 0.5f);
            maskRect.pivot = new Vector2(0.5f, 0.5f);
            maskRect.sizeDelta = size;
            maskRect.anchoredPosition = position;
            maskRect.SetSiblingIndex(sibling);
            imageRect.SetParent(maskRect, false);
            imageRect.anchoredPosition = -position;
            colorLayers[index] = mask;
        }

        private IEnumerator RevealMap(int shown, int progress)
        {
            if (openingInk != null)
            {
                openingInk.blocksRaycasts = false;
                for (float t = 0f; t < 1.2f; t += Time.unscaledDeltaTime)
                {
                    openingInk.alpha = 1f - Mathf.SmoothStep(0f, 1f, t / 1.2f);
                    yield return null;
                }
                openingInk.alpha = 0f;
            }
            for (int i = shown; i < progress; i++)
            {
                CanvasGroup group = colorLayers[i].GetComponent<CanvasGroup>();
                for (float t = 0f; t < 2.1f; t += Time.unscaledDeltaTime)
                {
                    group.alpha = Mathf.SmoothStep(0f, 1f, t / 2.1f);
                    yield return null;
                }
                group.alpha = 1f;
            }
            if (progress == 4 && shown < 4 && completedMap != null)
            {
                CanvasGroup fullGroup = completedMap.GetComponent<CanvasGroup>();
                for (float t = 0f; t < 1.8f; t += Time.unscaledDeltaTime)
                {
                    fullGroup.alpha = Mathf.SmoothStep(0f, 1f, t / 1.8f);
                    yield return null;
                }
                fullGroup.alpha = 1f;
            }
            PlayerPrefs.SetInt("MapShownRecolorCount", progress);
            PlayerPrefs.Save();
        }

        private IEnumerator PulseMission()
        {
            CanvasGroup group = missionText.GetComponent<CanvasGroup>();
            if (group == null) group = missionText.gameObject.AddComponent<CanvasGroup>();
            while (missionText != null)
            {
                group.alpha = 0.86f + 0.14f * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.3f));
                yield return null;
            }
        }

        public bool IsUnlocked(int index)
        {
            switch (index)
            {
                case 0: return true;
                case 1: return GameNavigator.WuMenCleared;
                case 2: return GameNavigator.WuMenCleared && GameNavigator.QianQingGongCleared;
                case 3: return GameNavigator.WuMenCleared && GameNavigator.QianQingGongCleared && GameNavigator.BaoHeDianCleared;
                default: return false;
            }
        }

        public void Enter(int index)
        {
            if (!IsUnlocked(index)) { ShowLocked(); return; }
            switch (index)
            {
                case 0:
                    // Starting the first level from the map should always play its CG.
                    // Restarts from inside the level do not pass through this entry point.
                    PlayerPrefs.SetInt("WuMenLevelIntroWatched", 0);
                    PlayerPrefs.Save();
                    SceneTransition.Load(GameNavigator.WuMen);
                    break;
                case 1: SceneTransition.Load(GameNavigator.QianQingGong); break;
                case 2: SceneTransition.Load(GameNavigator.BaoHeDian); break;
                case 3: SceneTransition.Load(GameNavigator.TaiHeDian); break;
            }
        }

        public void ShowLocked()
        {
            if (promptText != null) promptText.text = "当前关卡未解锁，\n请完成所有前置关卡再来挑战吧！";
            if (lockedPrompt != null) lockedPrompt.SetActive(true);
        }

        public void ClosePrompt()
        {
            if (lockedPrompt != null) lockedPrompt.SetActive(false);
        }
    }

}
