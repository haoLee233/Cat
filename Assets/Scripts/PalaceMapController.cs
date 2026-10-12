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
            int progress = GameNavigator.TaiHeDianCleared ? 4 : GameNavigator.BaoHeDianCleared ? 3 :
                GameNavigator.QianQingGongCleared ? 2 : GameNavigator.WuMenCleared ? 1 : 0;
            int shown = Mathf.Clamp(PlayerPrefs.GetInt("MapShownRecolorCount", 0), 0, progress);
            for (int i = 0; i < 4; i++)
            {
                colorLayers[i].SetActive(i < progress);
                CanvasGroup group = colorLayers[i].GetComponent<CanvasGroup>();
                if (group == null) group = colorLayers[i].AddComponent<CanvasGroup>();
                group.alpha = i < shown ? 1f : 0f;
                group.blocksRaycasts = false;
            }
            if (missionText != null) missionText.text = missions[progress];
            if (lockedPrompt != null) lockedPrompt.SetActive(false);
            StartCoroutine(RevealMap(shown, progress));
        }

        private IEnumerator RevealMap(int shown, int progress)
        {
            if (openingInk != null)
            {
                openingInk.blocksRaycasts = false;
                for (float t = 0f; t < 0.8f; t += Time.unscaledDeltaTime)
                {
                    openingInk.alpha = 1f - Mathf.SmoothStep(0f, 1f, t / 0.8f);
                    yield return null;
                }
                openingInk.alpha = 0f;
            }
            for (int i = shown; i < progress; i++)
            {
                CanvasGroup group = colorLayers[i].GetComponent<CanvasGroup>();
                for (float t = 0f; t < 1.4f; t += Time.unscaledDeltaTime)
                {
                    group.alpha = Mathf.SmoothStep(0f, 1f, t / 1.4f);
                    yield return null;
                }
                group.alpha = 1f;
            }
            PlayerPrefs.SetInt("MapShownRecolorCount", progress);
            PlayerPrefs.Save();
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
                case 0: SceneTransition.Load(GameNavigator.WuMen); break;
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
