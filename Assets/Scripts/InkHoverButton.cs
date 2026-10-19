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
        private Image[] framePieces;
        private static Font readableFont;

        private void Awake()
        {
            background = GetComponent<Image>();
            homeBackground = background != null ? background.color : Color.white;
            homeScale = transform.localScale;
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.transition = Selectable.Transition.None;
                if (GetComponent<UIButtonClickSound>() == null) gameObject.AddComponent<UIButtonClickSound>();
            }
            border = GetComponent<Outline>();
            if (border == null) border = gameObject.AddComponent<Outline>();
            border.effectDistance = new Vector2(2f, -2f);
            border.effectColor = new Color(0.68f, 0.53f, 0.33f, 0.55f);
        }

        private void Start()
        {
            FindLabel();
            if (background == null || background.sprite != null || background.color.a < 0.2f) return;
            if (readableFont == null) readableFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 36);
            if (label != null)
            {
                label.font = readableFont;
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = 20;
                label.resizeTextMaxSize = 36;
                label.fontSize = 36;
                label.horizontalOverflow = HorizontalWrapMode.Wrap;
                label.verticalOverflow = VerticalWrapMode.Truncate;
                label.alignment = TextAnchor.MiddleCenter;
                RectTransform box = label.rectTransform;
                RectTransform parent = transform as RectTransform;
                if (parent != null)
                {
                    box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
                    box.anchoredPosition = Vector2.zero;
                    box.sizeDelta = new Vector2(Mathf.Max(90f, parent.rect.width - 34f),
                        Mathf.Max(38f, parent.rect.height - 14f));
                }
                homeText = label.color;
            }
            DrawInkFrame();
        }

        private void DrawInkFrame()
        {
            RectTransform rect = transform as RectTransform;
            if (rect == null || rect.rect.width < 120f || rect.rect.height < 48f) return;
            Color inkGold = new Color(0.86f, 0.70f, 0.45f, 0.92f);
            framePieces = new Image[8];
            float halfW = rect.rect.width * 0.5f - 7f;
            float halfH = rect.rect.height * 0.5f - 6f;
            framePieces[0] = FrameStroke("上沿", new Vector2(0f, halfH), new Vector2(rect.rect.width - 29f, 2f), inkGold);
            framePieces[1] = FrameStroke("下沿", new Vector2(0f, -halfH), new Vector2(rect.rect.width - 29f, 2f), inkGold);
            framePieces[2] = FrameStroke("左沿", new Vector2(-halfW, 0f), new Vector2(2f, rect.rect.height - 25f), inkGold);
            framePieces[3] = FrameStroke("右沿", new Vector2(halfW, 0f), new Vector2(2f, rect.rect.height - 25f), inkGold);
            framePieces[4] = FrameStroke("左上角", new Vector2(-halfW, halfH), new Vector2(9f, 9f), inkGold);
            framePieces[5] = FrameStroke("右上角", new Vector2(halfW, halfH), new Vector2(9f, 9f), inkGold);
            framePieces[6] = FrameStroke("左下角", new Vector2(-halfW, -halfH), new Vector2(9f, 9f), inkGold);
            framePieces[7] = FrameStroke("右下角", new Vector2(halfW, -halfH), new Vector2(9f, 9f), inkGold);
            for (int i = 4; i < framePieces.Length; i++)
                framePieces[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            // Frame elements stay at the edge; the button label remains in its own inset area.
        }

        private Image FrameStroke(string name, Vector2 position, Vector2 size, Color color)
        {
            GameObject part = new GameObject("墨金边框·" + name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            part.transform.SetParent(transform, false);
            RectTransform rect = part.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image graphic = part.GetComponent<Image>();
            graphic.color = color;
            graphic.raycastTarget = false;
            return graphic;
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
            if (background != null && homeBackground.a >= 0.2f)
                background.color = new Color(0.47f, 0.23f, 0.16f, 0.96f);
            if (label != null) label.color = new Color(1f, 0.96f, 0.77f);
            if (border != null) border.effectColor = new Color(1f, 0.77f, 0.43f, 0.9f);
            TintFrame(new Color(1f, 0.87f, 0.57f, 1f));
            transform.localScale = homeScale * 1.035f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;
            Restore();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (background != null && homeBackground.a >= 0.2f)
                background.color = new Color(0.68f, 0.36f, 0.24f, 1f);
            TintFrame(new Color(1f, 0.96f, 0.76f, 1f));
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
            TintFrame(new Color(0.86f, 0.70f, 0.45f, 0.92f));
            transform.localScale = homeScale;
        }

        private void TintFrame(Color color)
        {
            if (framePieces == null) return;
            foreach (Image piece in framePieces) if (piece != null) piece.color = color;
        }
    }
}
