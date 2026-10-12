using UnityEngine;

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

        public void OpenCover() { SceneTransition.Load(Cover); }
        public void OpenMap() { SceneTransition.Load(Map); }
        public void OpenWuMen() { SceneTransition.Load(WuMen); }
        public void OpenBaoHeDian() { SceneTransition.Load(BaoHeDian); }
        public void OpenWuMenKnowledge() { SceneTransition.Load(WuMenKnowledge); }
        public void OpenQianQingKnowledge() { SceneTransition.Load(QianQingKnowledge); }
        public void OpenEnding() { SceneTransition.Load(Ending); }
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
