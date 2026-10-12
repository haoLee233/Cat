using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class SceneTransition : MonoBehaviour
    {
        private static SceneTransition instance;
        private Image veil;
        private bool transitioning;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (instance != null) return;
            GameObject root = new GameObject("场景渐变", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(SceneTransition));
            DontDestroyOnLoad(root);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            GameObject panel = new GameObject("黑色幕布", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            instance = root.GetComponent<SceneTransition>();
            instance.veil = panel.GetComponent<Image>();
            instance.veil.color = Color.black;
            instance.StartCoroutine(instance.Fade(1f, 0f, 0.45f));
        }

        public static void Load(string sceneName)
        {
            if (instance == null) Create();
            if (!instance.transitioning) instance.StartCoroutine(instance.Change(sceneName));
        }

        public static void Quit()
        {
            if (instance == null) Create();
            if (!instance.transitioning) instance.StartCoroutine(instance.Exit());
        }

        private IEnumerator Exit()
        {
            transitioning = true;
            yield return Fade(veil.color.a, 1f, 0.38f);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private IEnumerator Change(string sceneName)
        {
            transitioning = true;
            yield return Fade(veil.color.a, 1f, 0.38f);
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
            yield return null;
            yield return Fade(1f, 0f, 0.45f);
            transitioning = false;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                veil.color = new Color(0f, 0f, 0f, alpha);
                veil.raycastTarget = alpha > 0.01f;
                yield return null;
            }
            veil.color = new Color(0f, 0f, 0f, to);
            veil.raycastTarget = to > 0.01f;
        }
    }
}
