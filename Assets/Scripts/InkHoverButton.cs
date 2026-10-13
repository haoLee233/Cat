using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class InkHoverButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        private Image background;
        private Text label;
        private Outline border;
        private Color homeBackground;
        private Color homeText;
        private Vector3 homeScale;
        private bool hovered;

        private void Awake()
        {
            background = GetComponent<Image>();
            homeBackground = background != null ? background.color : Color.white;
            homeScale = transform.localScale;
            Button button = GetComponent<Button>();
            if (button != null) button.transition = Selectable.Transition.None;
            border = GetComponent<Outline>();
            if (border == null) border = gameObject.AddComponent<Outline>();
            border.effectDistance = new Vector2(2f, -2f);
            border.effectColor = new Color(0.68f, 0.53f, 0.33f, 0.55f);
        }

        private void FindLabel()
        {
            if (label != null) return;
            label = GetComponentInChildren<Text>(true);
            if (label != null) homeText = label.color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            FindLabel();
            if (background != null) background.color = new Color(0.47f, 0.23f, 0.16f, 0.96f);
            if (label != null) label.color = new Color(1f, 0.96f, 0.77f);
            if (border != null) border.effectColor = new Color(1f, 0.77f, 0.43f, 0.9f);
            transform.localScale = homeScale * 1.035f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            Restore();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (background != null) background.color = new Color(0.68f, 0.36f, 0.24f, 1f);
            transform.localScale = homeScale * 0.97f;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (hovered) OnPointerEnter(eventData);
            else Restore();
        }

        private void OnDisable()
        {
            hovered = false;
            Restore();
        }

        private void Restore()
        {
            if (background != null) background.color = homeBackground;
            if (label != null) label.color = homeText;
            if (border != null) border.effectColor = new Color(0.68f, 0.53f, 0.33f, 0.55f);
            transform.localScale = homeScale;
        }
    }
}
