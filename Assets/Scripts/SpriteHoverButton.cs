using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class SpriteHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Sprite normal;
        public Sprite highlighted;
        public float lift;

        private Image image;
        private RectTransform rect;
        private Vector2 home;

        private void Awake()
        {
            image = GetComponent<Image>();
            rect = GetComponent<RectTransform>();
            home = rect.anchoredPosition;
            if (normal != null) image.sprite = normal;
            if (GetComponent<Button>() != null && GetComponent<UIButtonClickSound>() == null)
                gameObject.AddComponent<UIButtonClickSound>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (highlighted != null) image.sprite = highlighted;
            rect.anchoredPosition = home + Vector2.up * lift;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (normal != null) image.sprite = normal;
            rect.anchoredPosition = home;
        }

        private void OnDisable()
        {
            if (image != null && normal != null) image.sprite = normal;
            if (rect != null) rect.anchoredPosition = home;
        }
    }
}
