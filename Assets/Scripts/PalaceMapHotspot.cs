using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JinYanMaoYing
{
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
            // The calligraphy on the map is already the place name.
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
