using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace JinYanMaoYing
{
    public sealed class EndingReveal : MonoBehaviour
    {
        public CanvasGroup monochromeMap;
        public CanvasGroup story;
        public VideoClip endingClip;

        private VideoPlayer player;
        private RenderTexture videoTexture;
        private GameObject videoOverlay;
        private bool videoFinished;

        private void Start()
        {
            if (monochromeMap == null || story == null) return;
            monochromeMap.alpha = 1f;
            monochromeMap.blocksRaycasts = false;
            story.alpha = 0f;
            story.interactable = false;
            story.blocksRaycasts = false;
            if (endingClip == null)
            {
                Debug.LogError("结局视频 Scene_end_video.m4v 未配置");
                StartCoroutine(PlayReveal());
                return;
            }
            CreateVideoOverlay();
            player.Prepare();
            StartCoroutine(PreparationTimeout());
        }

        private void CreateVideoOverlay()
        {
            Canvas canvas = story.GetComponentInParent<Canvas>();
            GameObject overlay = new GameObject("结局ED", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlay.transform.SetParent(canvas.transform, false);
            RectTransform full = overlay.GetComponent<RectTransform>();
            full.anchorMin = Vector2.zero;
            full.anchorMax = Vector2.one;
            full.offsetMin = full.offsetMax = Vector2.zero;
            Image black = overlay.GetComponent<Image>();
            black.color = Color.black;
            black.raycastTarget = true;
            videoOverlay = overlay;

            GameObject frame = new GameObject("完整视频安全画框", typeof(RectTransform));
            frame.transform.SetParent(overlay.transform, false);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(1520f, 850f);
            GameObject picture = new GameObject("ED画面", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(AspectRatioFitter));
            picture.transform.SetParent(frame.transform, false);
            RectTransform pictureRect = picture.GetComponent<RectTransform>();
            pictureRect.anchorMin = pictureRect.anchorMax = new Vector2(0.5f, 0.5f);
            pictureRect.sizeDelta = frameRect.sizeDelta;
            RawImage screen = picture.GetComponent<RawImage>();
            screen.raycastTarget = false;
            AspectRatioFitter fit = picture.GetComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            int width = endingClip.width > 0 ? Mathf.Clamp((int)endingClip.width, 16, 4096) : 1920;
            int height = endingClip.height > 0 ? Mathf.Clamp((int)endingClip.height, 16, 4096) : 1080;
            fit.aspectRatio = (float)width / height;
            videoTexture = new RenderTexture(width, height, 0);
            videoTexture.Create();
            screen.texture = videoTexture;

            GameObject skip = new GameObject("跳过ED", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            skip.transform.SetParent(overlay.transform, false);
            RectTransform skipRect = skip.GetComponent<RectTransform>();
            skipRect.anchorMin = skipRect.anchorMax = new Vector2(0.5f, 0.5f);
            skipRect.sizeDelta = new Vector2(220f, 74f);
            skipRect.anchoredPosition = new Vector2(805f, 455f);
            skip.GetComponent<Image>().color = new Color(0.15f, 0.11f, 0.10f, 0.91f);
            skip.GetComponent<Button>().onClick.AddListener(FinishVideo);
            skip.AddComponent<InkHoverButton>();
            GameObject label = new GameObject("文字", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            label.transform.SetParent(skip.transform, false);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            Text text = label.GetComponent<Text>();
            Text reference = story.GetComponentInChildren<Text>(true);
            if (reference != null) text.font = reference.font;
            text.fontSize = 39;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.95f, 0.83f);
            text.text = "跳过片尾";
            text.raycastTarget = false;

            player = new GameObject("结局ED播放器", typeof(VideoPlayer)).GetComponent<VideoPlayer>();
            player.clip = endingClip;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = videoTexture;
            player.aspectRatio = VideoAspectRatio.FitInside;
            player.audioOutputMode = VideoAudioOutputMode.Direct;
            player.isLooping = false;
            player.prepareCompleted += OnPrepared;
            player.loopPointReached += OnFinished;
            player.errorReceived += OnError;
        }

        private IEnumerator PreparationTimeout()
        {
            yield return new WaitForSecondsRealtime(12f);
            if (!videoFinished && player != null && !player.isPrepared) FinishVideo();
        }

        private void OnPrepared(VideoPlayer source) { if (!videoFinished) source.Play(); }
        private void OnFinished(VideoPlayer source) { FinishVideo(); }
        private void OnError(VideoPlayer source, string message)
        {
            Debug.LogError("结局ED播放失败: " + message);
            FinishVideo();
        }

        public void FinishVideo()
        {
            if (videoFinished) return;
            videoFinished = true;
            if (player != null) player.Stop();
            if (videoOverlay != null) videoOverlay.SetActive(false);
            StartCoroutine(PlayReveal());
        }

        private IEnumerator PlayReveal()
        {
            for (float t = 0f; t < 4.1f; t += Time.unscaledDeltaTime)
            {
                monochromeMap.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 3f));
                story.alpha = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 2f) / 2.1f));
                yield return null;
            }
            monochromeMap.alpha = 0f;
            story.alpha = 1f;
            story.interactable = true;
            story.blocksRaycasts = true;
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.prepareCompleted -= OnPrepared;
                player.loopPointReached -= OnFinished;
                player.errorReceived -= OnError;
                player.targetTexture = null;
            }
            if (videoTexture != null) { videoTexture.Release(); Destroy(videoTexture); }
        }
    }
}
