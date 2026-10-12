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
            colorLayers[0].SetActive(GameNavigator.WuMenCleared);
            colorLayers[1].SetActive(GameNavigator.QianQingGongCleared);
            colorLayers[2].SetActive(GameNavigator.BaoHeDianCleared);
            colorLayers[3].SetActive(GameNavigator.TaiHeDianCleared);
            int progress = GameNavigator.TaiHeDianCleared ? 4 : GameNavigator.BaoHeDianCleared ? 3 :
                GameNavigator.QianQingGongCleared ? 2 : GameNavigator.WuMenCleared ? 1 : 0;
            if (missionText != null) missionText.text = missions[progress];
            if (lockedPrompt != null) lockedPrompt.SetActive(false);
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
