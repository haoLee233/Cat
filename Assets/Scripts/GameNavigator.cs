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

        public void OpenCover() { SceneTransition.Load(Cover); }
        public void OpenMap() { SceneTransition.Load(Map); }
        public void OpenWuMen() { SceneTransition.Load(WuMen); }
        public void OpenBaoHeDian() { SceneTransition.Load(BaoHeDian); }
        public void OpenWuMenKnowledge() { SceneTransition.Load(WuMenKnowledge); }
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
    }
}
