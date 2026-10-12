using System.Collections;
using UnityEngine;

namespace JinYanMaoYing
{
    public sealed class EndingReveal : MonoBehaviour
    {
        public CanvasGroup monochromeMap;
        public CanvasGroup story;

        private void Start()
        {
            if (monochromeMap == null || story == null) return;
            monochromeMap.alpha = 1f;
            monochromeMap.blocksRaycasts = false;
            story.alpha = 0f;
            story.interactable = false;
            story.blocksRaycasts = false;
            StartCoroutine(PlayReveal());
        }

        private IEnumerator PlayReveal()
        {
            for (float t = 0f; t < 2.7f; t += Time.unscaledDeltaTime)
            {
                monochromeMap.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 2.1f));
                story.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 1.2f) / 1.5f));
                yield return null;
            }
            monochromeMap.alpha = 0f;
            story.alpha = 1f;
            story.interactable = true;
            story.blocksRaycasts = true;
        }
    }
}
