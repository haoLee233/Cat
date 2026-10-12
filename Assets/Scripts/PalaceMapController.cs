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

        private void Start()
        {
            if (colorLayers == null || colorLayers.Length < 4) return;
            colorLayers[0].SetActive(GameNavigator.WuMenCleared);
            colorLayers[1].SetActive(GameNavigator.QianQingGongCleared);
            colorLayers[2].SetActive(GameNavigator.BaoHeDianCleared);
            colorLayers[3].SetActive(GameNavigator.TaiHeDianCleared);
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
