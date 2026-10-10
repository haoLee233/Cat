using UnityEngine;
using UnityEngine.SceneManagement;

namespace JinYanMaoYing
{
    public sealed class GameNavigator : MonoBehaviour
    {
        public const string Cover = "封面";
        public const string Map = "故宫地图";
        public const string WuMen = "午门追逐";
        public const string BaoHeDian = "保和殿水管";

        public void OpenCover() { SceneManager.LoadScene(Cover); }
        public void OpenMap() { SceneManager.LoadScene(Map); }
        public void OpenWuMen() { SceneManager.LoadScene(WuMen); }
        public void OpenBaoHeDian() { SceneManager.LoadScene(BaoHeDian); }
        public void Quit() { Application.Quit(); }

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
