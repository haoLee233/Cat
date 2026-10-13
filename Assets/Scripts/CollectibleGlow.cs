using UnityEngine;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    // A soft ink-and-gold edge keeps collectables legible against painted scenery.
    public sealed class CollectibleGlow : MonoBehaviour
    {
        public Color glowColor = new Color(1f, 0.83f, 0.46f, 1f);
        public float pulseAmount = 0.045f;

        private Outline outline;
        private Vector3 homeScale;

        private void Awake()
        {
            homeScale = transform.localScale;
            outline = GetComponent<Outline>();
            if (outline == null) outline = gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(5f, -5f);
            outline.useGraphicAlpha = true;
        }

        private void Update()
        {
            float wave = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3.2f + transform.GetSiblingIndex());
            transform.localScale = homeScale * (1f + pulseAmount * wave);
            Color color = glowColor;
            color.a = 0.57f + 0.38f * wave;
            outline.effectColor = color;
        }

        private void OnDisable()
        {
            transform.localScale = homeScale;
        }
    }
}
