using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace JinYanMaoYing
{
    public sealed class GameNavigator : MonoBehaviour
    {
        public const string Cover = "封面";
        public const string Map = "故宫地图";
        public const string WuMen = "午门追逐";
        public const string BaoHeDian = "保和殿水管";
        public const string WuMenKnowledge = "午门营造录";
        public const string QianQingGong = "乾清宫";
        public const string QianQingKnowledge = "乾清宫营造录";
        public const string TaiHeDian = "太和殿";
        public const string Ending = "终章";
        public const string Collection = "匠心图鉴";
        public const string BaoHeKnowledge = "保和殿营造录";

        public LevelIntroVideo openingIntro;

        private void Start()
        {
            if (SceneManager.GetActiveScene().name != Cover) return;
            Text[] texts = FindObjectsOfType<Text>(true);
            foreach (Text text in texts)
            {
                if (text.gameObject.name != "旅程缘起") continue;
                text.text = text.text.TrimEnd('。');
                Outline glow = text.GetComponent<Outline>();
                if (glow == null) glow = text.gameObject.AddComponent<Outline>();
                glow.effectColor = new Color(1f, 0.78f, 0.42f, 0.96f);
                glow.effectDistance = new Vector2(1.35f, 1.35f);
                glow.useGraphicAlpha = true;
                break;
            }
        }

        public void OpenCover() { SceneTransition.Load(Cover); }
        public void OpenMap() { SceneTransition.Load(Map); }
        public void StartNewJourney()
        {
            PlayerPrefs.DeleteKey("WuMenCleared");
            PlayerPrefs.DeleteKey("QianQingGongCleared");
            PlayerPrefs.DeleteKey("BaoHeDianCleared");
            PlayerPrefs.DeleteKey("TaiHeDianCleared");
            PlayerPrefs.DeleteKey("MapShownRecolorCount");
            PlayerPrefs.SetInt("WuMenIntroWatched", 0);
            PlayerPrefs.Save();
            if (openingIntro != null) openingIntro.Play();
            else SceneTransition.Load(Map);
        }

        public void OpeningIntroFinished()
        {
            PlayerPrefs.SetInt("WuMenIntroWatched", 1);
            PlayerPrefs.Save();
            SceneTransition.Load(Map);
        }
        public void OpenWuMen() { SceneTransition.Load(WuMen); }
        public void OpenBaoHeDian() { SceneTransition.Load(BaoHeDian); }
        public void OpenWuMenKnowledge() { SceneTransition.Load(WuMenKnowledge); }
        public void OpenQianQingKnowledge() { SceneTransition.Load(QianQingKnowledge); }
        public void OpenEnding() { SceneTransition.Load(Ending); }
        public void OpenCollection() { SceneTransition.Load(Collection); }
        public void OpenBaoHeKnowledge() { SceneTransition.Load(BaoHeKnowledge); }
        public void Quit() { SceneTransition.Quit(); }

        public static bool WuMenCleared
        {
            get { return PlayerPrefs.GetInt("WuMenCleared", 0) == 1; }
            set
            {
                PlayerPrefs.SetInt("WuMenCleared", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool BaoHeDianCleared
        {
            get { return PlayerPrefs.GetInt("BaoHeDianCleared", 0) == 1; }
            set
            {
                PlayerPrefs.SetInt("BaoHeDianCleared", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool QianQingGongCleared
        {
            get { return PlayerPrefs.GetInt("QianQingGongCleared", 0) == 1; }
            set
            {
                PlayerPrefs.SetInt("QianQingGongCleared", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public static bool TaiHeDianCleared
        {
            get { return PlayerPrefs.GetInt("TaiHeDianCleared", 0) == 1; }
            set
            {
                PlayerPrefs.SetInt("TaiHeDianCleared", value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }
    }
}
