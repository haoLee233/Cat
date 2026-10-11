using UnityEngine;
using UnityEngine.EventSystems;
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
                case 3: ShowUnavailable(); break;
            }
        }

        public void ShowLocked()
        {
            if (promptText != null) promptText.text = "当前关卡未解锁，\n请完成所有前置关卡再来挑战吧！";
            if (lockedPrompt != null) lockedPrompt.SetActive(true);
        }

        private void ShowUnavailable()
        {
            if (promptText != null) promptText.text = "太和殿关卡正在制作中，\n后续版本开放。";
            if (lockedPrompt != null) lockedPrompt.SetActive(true);
        }

        public void ClosePrompt()
        {
            if (lockedPrompt != null) lockedPrompt.SetActive(false);
        }
    }

    public sealed class PalaceMapHotspot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public PalaceMapController controller;
        public int index;
        public RectTransform marker;
        public GameObject hoverName;
        public Image markerImage;

        private void Start()
        {
            if (hoverName != null) hoverName.SetActive(false);
            if (markerImage != null && controller != null && !controller.IsUnlocked(index))
                markerImage.color = new Color(0.22f, 0.20f, 0.18f, 0.68f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (controller == null || !controller.IsUnlocked(index)) return;
            if (marker != null) marker.localScale = Vector3.one * 1.12f;
            if (hoverName != null) hoverName.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (marker != null) marker.localScale = Vector3.one;
            if (hoverName != null) hoverName.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && controller != null)
                controller.Enter(index);
        }
    }
}
