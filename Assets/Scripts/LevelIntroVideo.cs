using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;

namespace JinYanMaoYing
{
    public sealed class LevelIntroVideo : MonoBehaviour
    {
        public VideoClip clip;
        public AudioClip cutsceneAmbience;
        [Range(0f, 1f)] public float cutsceneAmbienceVolume = 0.45f;
        public Font buttonFont;
        public bool playOnStart = true;
        public string skipLabel = "跳过片头";
        public UnityEvent onFinished = new UnityEvent();
        public bool IsPlaying { get; private set; }

        private VideoPlayer player;
        private AudioSource ambienceSource;
        private RenderTexture texture;
        private GameObject overlay;

        private void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            if (IsPlaying) return;
            IsPlaying = true;
            if (clip == null)
            {
                Debug.LogError("关卡片头视频未配置: " + gameObject.name);
                Finish();
                return;
            }

            Canvas canvas = FindSceneCanvas();
            if (canvas == null)
            {
                Debug.LogError("关卡片头找不到画布: " + gameObject.name);
                Finish();
                return;
            }

            CreateOverlay(canvas);
            player = gameObject.AddComponent<VideoPlayer>();
            player.playOnAwake = false;
            player.clip = clip;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = texture;
            player.aspectRatio = VideoAspectRatio.FitInside;
            player.audioOutputMode = VideoAudioOutputMode.Direct;
            player.isLooping = false;
            player.prepareCompleted += OnPrepared;
            player.loopPointReached += OnFinished;
            player.errorReceived += OnError;
            player.Prepare();
            StartCoroutine(PreparationTimeout());
        }

        private Canvas FindSceneCanvas()
        {
            foreach (GameObject sceneRoot in gameObject.scene.GetRootGameObjects())
            {
                Canvas sceneCanvas = sceneRoot.GetComponent<Canvas>();
                if (sceneCanvas != null) return sceneCanvas;
            }
            return null;
        }

        private void CreateOverlay(Canvas canvas)
        {
            overlay = new GameObject("关卡CG", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlay.transform.SetParent(canvas.transform, false);
            overlay.transform.SetAsLastSibling();
            RectTransform full = overlay.GetComponent<RectTransform>();
            full.anchorMin = Vector2.zero;
            full.anchorMax = Vector2.one;
            full.offsetMin = full.offsetMax = Vector2.zero;
            Image black = overlay.GetComponent<Image>();
            black.color = Color.black;
            black.raycastTarget = true;

            GameObject frame = new GameObject("完整视频安全画框", typeof(RectTransform));
            frame.transform.SetParent(overlay.transform, false);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.anchorMin = frameRect.anchorMax = new Vector2(0.5f, 0.5f);
            frameRect.sizeDelta = new Vector2(1840f, 1030f);

            GameObject picture = new GameObject("CG画面", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(RawImage), typeof(AspectRatioFitter));
            picture.transform.SetParent(frame.transform, false);
            RectTransform pictureRect = picture.GetComponent<RectTransform>();
            pictureRect.anchorMin = pictureRect.anchorMax = new Vector2(0.5f, 0.5f);
            pictureRect.sizeDelta = frameRect.sizeDelta;
            AspectRatioFitter fit = picture.GetComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            int width = clip.width > 0 ? Mathf.Clamp((int)clip.width, 16, 4096) : 1920;
            int height = clip.height > 0 ? Mathf.Clamp((int)clip.height, 16, 4096) : 1080;
            fit.aspectRatio = (float)width / height;
            texture = new RenderTexture(width, height, 0);
            texture.Create();
            RawImage image = picture.GetComponent<RawImage>();
            image.texture = texture;
            image.raycastTarget = false;

            GameObject skip = new GameObject("跳过片头", typeof(RectTransform), typeof(CanvasRenderer),
                typeof(Image), typeof(Button));
            skip.transform.SetParent(overlay.transform, false);
            RectTransform skipRect = skip.GetComponent<RectTransform>();
            skipRect.anchorMin = skipRect.anchorMax = new Vector2(0.5f, 0.5f);
            skipRect.sizeDelta = new Vector2(230f, 74f);
            skipRect.anchoredPosition = new Vector2(805f, 475f);
            skip.GetComponent<Image>().color = new Color(0.15f, 0.11f, 0.10f, 0.91f);
            skip.GetComponent<Button>().onClick.AddListener(Finish);
            skip.AddComponent<InkHoverButton>();
            GameObject label = new GameObject("文字", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            label.transform.SetParent(skip.transform, false);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            Text text = label.GetComponent<Text>();
            Text reference = canvas.GetComponentInChildren<Text>(true);
            text.font = buttonFont != null ? buttonFont : reference != null ? reference.font : null;
            if (text.font == null) text.font = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 39);
            text.fontSize = 39;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.95f, 0.83f);
            text.text = string.IsNullOrEmpty(skipLabel) ? "跳过片头" : skipLabel;
            text.raycastTarget = false;
        }

        private IEnumerator PreparationTimeout()
        {
            yield return new WaitForSecondsRealtime(12f);
            if (IsPlaying && player != null && !player.isPrepared) Finish();
        }

        private void OnPrepared(VideoPlayer source)
        {
            if (!IsPlaying) return;
            source.Play();
            if (cutsceneAmbience == null) return;
            ambienceSource = gameObject.AddComponent<AudioSource>();
            ambienceSource.clip = cutsceneAmbience;
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = false;
            ambienceSource.spatialBlend = 0f;
            ambienceSource.volume = cutsceneAmbienceVolume;
            ambienceSource.Play();
            StartCoroutine(FadeAmbienceAtEnd(source));
        }

        private IEnumerator FadeAmbienceAtEnd(VideoPlayer source)
        {
            const double fadeDuration = 1.5;
            while (IsPlaying && source != null && source.clip != null)
            {
                if (source.isPlaying)
                {
                    double remaining = source.clip.length - source.time;
                    if (remaining <= fadeDuration)
                        ambienceSource.volume = cutsceneAmbienceVolume * Mathf.Clamp01((float)(remaining / fadeDuration));
                }
                yield return null;
            }
        }
        private void OnFinished(VideoPlayer source) { Finish(); }
        private void OnError(VideoPlayer source, string message)
        {
            Debug.LogError("关卡CG播放失败: " + message);
            Finish();
        }

        public void Finish()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            if (player != null) player.Stop();
            if (ambienceSource != null) ambienceSource.Stop();
            if (overlay != null) overlay.SetActive(false);
            if (onFinished != null) onFinished.Invoke();
        }

        private void OnDestroy()
        {
            if (overlay != null) Destroy(overlay);
            if (ambienceSource != null) Destroy(ambienceSource);
            if (player != null)
            {
                player.prepareCompleted -= OnPrepared;
                player.loopPointReached -= OnFinished;
                player.errorReceived -= OnError;
                player.targetTexture = null;
            }
            if (texture != null) { texture.Release(); Destroy(texture); }
        }
    }
}
