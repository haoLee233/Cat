using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace JinYanMaoYing
{
    public sealed class WuMenRunner : MonoBehaviour
    {
        private const float RunFrameDuration = 0.12f;
        public RectTransform playArea;
        public RectTransform[] backgrounds;
        public RectTransform cat;
        public Image catImage;
        public Sprite[] runFrames;
        public Sprite airborneSprite;
        public Sprite crouchSprite;
        public Text statusText;
        public Image hudFrame;
        public Text hintText;
        public GameObject resultPanel;
        public Text resultText;
        public Image rankImage;
        public Text scoreText;
        public Sprite rankExcellent;
        public Sprite rankFirst;
        public Sprite rankSecond;
        public GameObject settingsPanel;
        public RectTransform missionPrompt;
        public GameObject videoOverlay;
        public RawImage videoScreen;
        public VideoPlayer introPlayer;
        public VideoClip introClip;
        public Sprite coinSprite;
        public Sprite[] beastSprites;
        public Sprite lionSprite;
        public Sprite lanternSprite;

        private const float GroundY = -260f;
        private const float CatStartX = -600f;
        private const float AirborneTiltDegrees = 12f;
        private const float TutorialTriggerDistance = 410f;
        private const float RunningHitHeight = 118f;
        private const float CrouchingHitHeight = 70f;
        private const float LionHitHalfWidth = 68f;
        private const float LionHitHeight = 88f;
        private const float LanternHitHalfWidth = 52f;
        private const float LanternHitHeight = 100f;
        private readonly List<SpawnedItem> items = new List<SpawnedItem>();
        private RenderTexture videoTexture;
        private Sprite slicedHudSprite;
        private Quaternion catRestRotation;
        private float verticalSpeed, jumpHeight, nextSpawn, elapsed, runFrameAccumulator;
        private float catX = CatStartX, forwardImpulse;
        private int jumpsUsed, coins, beasts, spawnCount, runFrameIndex;
        private int health = 3;
        private bool finished, paused, introPlaying = true;
        private bool ducking;
        private bool lionTutorialSeen, lanternTutorialSeen;
        private SpawnedItem tutorialItem;
        private GameObject tutorialOverlay;
        private Text tutorialTitle, tutorialBody, tutorialAction;
        private Image tutorialPreview;

        private sealed class SpawnedItem
        {
            public RectTransform rect;
            public int kind; // 0 金币，1 脊兽，2 障碍
            public int beastIndex;
            public float baseY;
            public float phase;
            public bool lantern;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            if (cat != null) catRestRotation = cat.localRotation;
            if (resultPanel != null) resultPanel.SetActive(false);
            if (rankImage != null) rankImage.gameObject.SetActive(false);
            if (scoreText != null) scoreText.gameObject.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (missionPrompt != null) missionPrompt.gameObject.SetActive(false);
            CreateObstacleTutorial();
            nextSpawn = 1.15f;
            if (hudFrame != null && hudFrame.sprite != null)
            {
                Sprite source = hudFrame.sprite;
                slicedHudSprite = Sprite.Create(source.texture, source.rect, new Vector2(0.5f, 0.5f),
                    source.pixelsPerUnit, 0, SpriteMeshType.FullRect, new Vector4(170f, 65f, 170f, 65f));
                hudFrame.sprite = slicedHudSprite;
                hudFrame.type = Image.Type.Sliced;
            }
            RefreshStatus();
            if (PlayerPrefs.GetInt("WuMenIntroWatched", 0) == 1)
            {
                introPlaying = false;
                if (videoOverlay != null) videoOverlay.SetActive(false);
                return;
            }
            if (introClip == null || introPlayer == null || videoScreen == null)
            {
                Debug.LogError("午门片头视频未配置");
                StartCoroutine(ShowMission());
                return;
            }
            int videoWidth = introClip.width > 0 ? Mathf.Clamp((int)introClip.width, 16, 4096) : 1920;
            int videoHeight = introClip.height > 0 ? Mathf.Clamp((int)introClip.height, 16, 4096) : 1080;
            videoTexture = new RenderTexture(videoWidth, videoHeight, 0);
            videoTexture.Create();
            videoScreen.texture = videoTexture;
            AspectRatioFitter fit = videoScreen.GetComponent<AspectRatioFitter>();
            if (fit != null) fit.aspectRatio = (float)videoWidth / videoHeight;
            introPlayer.clip = introClip;
            introPlayer.targetTexture = videoTexture;
            introPlayer.renderMode = VideoRenderMode.RenderTexture;
            introPlayer.aspectRatio = VideoAspectRatio.FitInside;
            introPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            introPlayer.isLooping = false;
            introPlayer.prepareCompleted += OnVideoPrepared;
            introPlayer.loopPointReached += OnVideoFinished;
            introPlayer.errorReceived += OnVideoError;
            videoOverlay.SetActive(true);
            introPlayer.Prepare();
            StartCoroutine(VideoPreparationTimeout());
        }

        private IEnumerator VideoPreparationTimeout()
        {
            yield return new WaitForSecondsRealtime(12f);
            if (videoOverlay != null && videoOverlay.activeSelf && !introPlayer.isPrepared)
            {
                Debug.LogError("午门片头视频准备超时");
                FinishVideo();
            }
        }

        private void OnVideoPrepared(VideoPlayer player)
        {
            if (videoOverlay != null && videoOverlay.activeSelf) player.Play();
        }
        private void OnVideoFinished(VideoPlayer player) { FinishVideo(); }
        private void OnVideoError(VideoPlayer player, string message)
        {
            Debug.LogError("午门片头视频播放失败: " + message);
            FinishVideo();
        }

        private void FinishVideo()
        {
            if (videoOverlay == null || !videoOverlay.activeSelf) return;
            introPlayer.Stop();
            videoOverlay.SetActive(false);
            StartCoroutine(ShowMission());
        }

        public void SkipVideo() { FinishVideo(); }

        private IEnumerator ShowMission()
        {
            if (missionPrompt == null) { introPlaying = false; yield break; }
            missionPrompt.gameObject.SetActive(true);
            yield return SlideMission(-2200f, 0f, 0.95f);
            yield return new WaitForSecondsRealtime(5f);
            yield return SlideMission(0f, 2200f, 0.95f);
            missionPrompt.gameObject.SetActive(false);
            introPlaying = false;
        }

        private IEnumerator SlideMission(float from, float to, float duration)
        {
            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(time / duration));
                missionPrompt.anchoredPosition = new Vector2(Mathf.Lerp(from, to, t), 0f);
                yield return null;
            }
            missionPrompt.anchoredPosition = new Vector2(to, 0f);
        }

        private void Update()
        {
            if (finished || paused || introPlaying) return;
            if (tutorialItem != null) { HandleTutorialInput(); return; }
            float dt = Time.deltaTime;
            elapsed += dt;
            MoveBackground(dt);
            forwardImpulse = Mathf.MoveTowards(forwardImpulse, 0f, 55f * dt);
            float rhythmDrift = Mathf.Sin(elapsed * 1.8f) * 15f;
            float targetX = Mathf.Clamp(CatStartX + forwardImpulse + rhythmDrift, -660f, -150f);
            catX = Mathf.MoveTowards(catX, targetX, 185f * dt);
            bool keyboardJump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
            bool mouseJump = Input.GetMouseButtonDown(0) &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject());
            if ((keyboardJump || mouseJump) && jumpsUsed < 2)
            {
                verticalSpeed = jumpsUsed == 0 ? 840f : 780f;
                jumpsUsed++;
            }
            ducking = Input.GetKey(KeyCode.DownArrow) && jumpHeight <= 0.01f;
            AnimateCat(dt, jumpHeight > 0.01f || verticalSpeed > 0f);
            verticalSpeed -= 2100f * dt;
            jumpHeight = Mathf.Max(0f, jumpHeight + verticalSpeed * dt);
            if (jumpHeight <= 0f) { verticalSpeed = 0f; jumpsUsed = 0; }
            cat.anchoredPosition = new Vector2(catX, GroundY + jumpHeight);

            if (elapsed >= nextSpawn)
            {
                SpawnNext();
                nextSpawn = elapsed + Random.Range(0.96f, 1.26f);
            }
            for (int i = items.Count - 1; i >= 0; i--)
            {
                SpawnedItem item = items[i];
                float speed = Mathf.Min(930f, 690f + elapsed * 5.3f);
                Vector2 position = item.rect.anchoredPosition + Vector2.left * speed * dt;
                if (item.kind == 1) position.y = item.baseY + Mathf.Sin(elapsed * 3.4f + item.phase) * 16f;
                item.rect.anchoredPosition = position;
                if (item.kind == 2 && position.x > catX && position.x - catX <= TutorialTriggerDistance &&
                    (item.lantern ? !lanternTutorialSeen : !lionTutorialSeen) && tutorialOverlay != null)
                {
                    ShowObstacleTutorial(item);
                    return;
                }
                float dx = Mathf.Abs(item.rect.anchoredPosition.x - catX);
                float hitY = item.kind == 2 && item.lantern ? position.y - 86f : position.y;
                float dy = Mathf.Abs(hitY - cat.anchoredPosition.y);
                float catHeight = item.kind == 2 ? (ducking ? CrouchingHitHeight : RunningHitHeight) :
                    (ducking ? 95f : cat.sizeDelta.y);
                float itemHeight = item.kind != 2 ? item.rect.sizeDelta.y :
                    (item.lantern ? LanternHitHeight : LionHitHeight);
                float halfWidth = item.kind != 2 ? 85f : (item.lantern ? LanternHitHalfWidth : LionHitHalfWidth);
                if (dx < halfWidth && dy < (catHeight + itemHeight) * 0.5f)
                {
                    if (item.kind == 0) { coins++; forwardImpulse = Mathf.Min(400f, forwardImpulse + 28f); }
                    else if (item.kind == 1 && item.beastIndex == beasts) { beasts++; forwardImpulse = Mathf.Min(400f, forwardImpulse + 110f); }
                    else { health--; forwardImpulse = Mathf.Max(-50f, forwardImpulse - 105f); }
                    Destroy(item.rect.gameObject);
                    items.RemoveAt(i);
                    RefreshStatus();
                    if (beasts >= 3) { Finish(true); break; }
                    if (health <= 0) { Finish(false); break; }
                }
                else if (item.rect.anchoredPosition.x < -1100f)
                {
                    Destroy(item.rect.gameObject);
                    items.RemoveAt(i);
                }
            }
        }

        private void MoveBackground(float dt)
        {
            if (backgrounds == null) return;
            foreach (RectTransform background in backgrounds)
            {
                if (background == null) continue;
                Vector2 pos = background.anchoredPosition + Vector2.left * Mathf.Min(370f, 265f + elapsed * 2.4f) * dt;
                if (pos.x <= -1920f) pos.x += 3840f;
                background.anchoredPosition = pos;
            }
        }

        private void AnimateCat(float dt, bool airborne)
        {
            if (catImage == null) return;
            if (airborne)
            {
                cat.sizeDelta = new Vector2(260f, 155f);
                cat.localRotation = catRestRotation * Quaternion.Euler(0f, 0f, AirborneTiltDegrees);
                if (airborneSprite != null) catImage.sprite = airborneSprite;
                return;
            }
            cat.localRotation = catRestRotation;
            if (ducking && crouchSprite != null)
            {
                catImage.sprite = crouchSprite;
                cat.sizeDelta = new Vector2(205f, 112f);
                return;
            }
            cat.sizeDelta = new Vector2(260f, 155f);
            if (runFrames == null || runFrames.Length == 0) return;
            runFrameAccumulator += Mathf.Max(0f, dt);
            while (runFrameAccumulator >= RunFrameDuration)
            {
                runFrameAccumulator -= RunFrameDuration;
                runFrameIndex = (runFrameIndex + 1) % runFrames.Length;
            }
            if (runFrames[runFrameIndex] != null) catImage.sprite = runFrames[runFrameIndex];
        }

        private void CreateObstacleTutorial()
        {
            Canvas canvas = playArea != null ? playArea.GetComponentInParent<Canvas>() : null;
            Font font = hintText != null ? hintText.font : statusText != null ? statusText.font : null;
            if (canvas == null || font == null)
            {
                Debug.LogWarning("午门障碍教学缺少 Canvas 或字体，跳过首次引导。");
                return;
            }

            tutorialOverlay = new GameObject("午门障碍操作教学", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            tutorialOverlay.transform.SetParent(canvas.transform, false);
            RectTransform shadeRect = tutorialOverlay.GetComponent<RectTransform>();
            shadeRect.anchorMin = Vector2.zero;
            shadeRect.anchorMax = Vector2.one;
            shadeRect.offsetMin = Vector2.zero;
            shadeRect.offsetMax = Vector2.zero;
            UnityEngine.UI.Image shade = tutorialOverlay.GetComponent<UnityEngine.UI.Image>();
            shade.color = new Color(0.035f, 0.028f, 0.025f, 0.77f);
            shade.raycastTarget = true;

            GameObject scroll = new GameObject("教学卷轴", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            scroll.transform.SetParent(tutorialOverlay.transform, false);
            RectTransform scrollRect = scroll.GetComponent<RectTransform>();
            scrollRect.anchorMin = scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.sizeDelta = new Vector2(1320f, 600f);
            UnityEngine.UI.Image scrollImage = scroll.GetComponent<UnityEngine.UI.Image>();
            UnityEngine.UI.Image missionImage = missionPrompt != null ? missionPrompt.GetComponent<UnityEngine.UI.Image>() : null;
            scrollImage.sprite = missionImage != null ? missionImage.sprite : null;
            scrollImage.color = scrollImage.sprite != null ? Color.white : new Color(0.96f, 0.9f, 0.77f, 0.98f);
            scrollImage.preserveAspect = scrollImage.sprite != null;
            scrollImage.raycastTarget = false;

            GameObject previewBacking = new GameObject("障碍预览底色", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            previewBacking.transform.SetParent(scroll.transform, false);
            RectTransform backingRect = previewBacking.GetComponent<RectTransform>();
            backingRect.anchorMin = backingRect.anchorMax = new Vector2(0.5f, 0.5f);
            backingRect.anchoredPosition = new Vector2(-355f, -10f);
            backingRect.sizeDelta = new Vector2(330f, 390f);
            UnityEngine.UI.Image backingImage = previewBacking.GetComponent<UnityEngine.UI.Image>();
            backingImage.color = new Color(0.26f, 0.20f, 0.16f, 0.90f);
            backingImage.raycastTarget = false;

            GameObject preview = new GameObject("障碍形象", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            preview.transform.SetParent(previewBacking.transform, false);
            RectTransform previewRect = preview.GetComponent<RectTransform>();
            previewRect.anchorMin = previewRect.anchorMax = new Vector2(0.5f, 0.5f);
            previewRect.sizeDelta = new Vector2(300f, 350f);
            tutorialPreview = preview.GetComponent<UnityEngine.UI.Image>();
            tutorialPreview.preserveAspect = true;
            tutorialPreview.raycastTarget = false;
            Outline previewEdge = preview.AddComponent<Outline>();
            previewEdge.effectColor = new Color(1f, 0.89f, 0.67f, 0.95f);
            previewEdge.effectDistance = new Vector2(5f, -5f);

            Color ink = new Color(0.18f, 0.12f, 0.09f, 1f);
            tutorialTitle = CreateTutorialText(scroll.transform, "障碍名称", font, 68,
                new Vector2(240f, 155f), new Vector2(650f, 105f), ink);
            tutorialBody = CreateTutorialText(scroll.transform, "操作说明", font, 47,
                new Vector2(240f, 5f), new Vector2(670f, 200f), ink);
            tutorialAction = CreateTutorialText(scroll.transform, "继续提示", font, 42,
                new Vector2(240f, -155f), new Vector2(660f, 85f), new Color(0.48f, 0.16f, 0.10f, 1f));
            tutorialOverlay.SetActive(false);
        }

        private static Text CreateTutorialText(Transform parent, string name, Font font, int fontSize,
            Vector2 position, Vector2 size, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Text));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            UnityEngine.UI.Text label = obj.GetComponent<UnityEngine.UI.Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            return label;
        }

        private void ShowObstacleTutorial(SpawnedItem item)
        {
            tutorialItem = item;
            if (item.lantern)
            {
                lanternTutorialSeen = true;
                tutorialTitle.text = "宫灯低垂";
                tutorialBody.text = "按住 ↓ 蹲下\n从宫灯下穿过；跳跃会撞上灯。";
                tutorialAction.text = "按住 ↓ 开始下蹲";
                tutorialPreview.sprite = lanternSprite;
            }
            else
            {
                lionTutorialSeen = true;
                tutorialTitle.text = "石狮挡路";
                tutorialBody.text = "按空格 / ↑ / 左键跳过\n连续障碍可再按一次二段跳。";
                tutorialAction.text = "按跳跃键开始跳跃";
                tutorialPreview.sprite = lionSprite;
            }
            tutorialOverlay.transform.SetAsLastSibling();
            tutorialOverlay.SetActive(true);
        }

        private void HandleTutorialInput()
        {
            // The obstacle stays still while an already airborne cat finishes landing.
            if (jumpHeight > 0f)
            {
                float dt = Time.deltaTime;
                verticalSpeed -= 2100f * dt;
                jumpHeight = Mathf.Max(0f, jumpHeight + verticalSpeed * dt);
                if (jumpHeight <= 0f) { verticalSpeed = 0f; jumpsUsed = 0; }
                cat.anchoredPosition = new Vector2(catX, GroundY + jumpHeight);
                AnimateCat(dt, jumpHeight > 0f);
                return;
            }
            if (tutorialItem.lantern)
            {
                if (!Input.GetKey(KeyCode.DownArrow)) return;
                ducking = true;
                AnimateCat(0f, false);
            }
            else
            {
                if (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.UpArrow) &&
                    !Input.GetMouseButtonDown(0)) return;
                verticalSpeed = 840f;
                jumpsUsed = 1;
            }
            tutorialOverlay.SetActive(false);
            tutorialItem = null;
        }

        private void SpawnNext()
        {
            spawnCount++;
            // 每约十二秒出现一只待收集脊兽；漏掉时下一轮仍会出现。
            int kind = beasts < 3 && spawnCount % 11 == 0 ? 1 : (Random.value < 0.54f ? 0 : 2);
            if (kind == 0) { SpawnCoinTrail(); return; }
            bool lantern = kind == 2 && Random.value < 0.5f;
            Sprite sprite = kind == 1 ? beastSprites[beasts] : (lantern ? lanternSprite : lionSprite);
            if (sprite == null) return;
            GameObject obj = new GameObject(kind == 1 ? "脊兽" : lantern ? "宫灯" : "石狮", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(playArea, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = kind == 1 ? new Vector2(112f, 120f) :
                lantern ? new Vector2(230f, 260f) : new Vector2(124f, 132f);
            float beastY = new[] { -215f, -95f, -160f }[beasts];
            float y = kind == 1 ? beastY : lantern ? -75f : -254f;
            rect.anchoredPosition = new Vector2(1060f, y);
            if (kind == 1) rect.localRotation = Quaternion.Euler(0f, 0f, beasts == 1 ? 8f : -7f);
            Image image = obj.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            if (kind == 1) obj.AddComponent<CollectibleGlow>();
            else
            {
                Outline edge = obj.AddComponent<Outline>();
                edge.effectColor = new Color(0.46f, 0.14f, 0.09f, 0.95f);
                edge.effectDistance = new Vector2(5f, -5f);
                edge.useGraphicAlpha = true;
            }
            items.Add(new SpawnedItem { rect = rect, kind = kind, beastIndex = kind == 1 ? beasts : -1,
                baseY = y, phase = spawnCount * 1.7f, lantern = lantern });
        }

        private void SpawnCoinTrail()
        {
            if (coinSprite == null) return;
            // 多条路线让玩家在避障和拾币之间做选择。
            float[][] paths = {
                new[] { -230f, -190f, -135f, -190f, -230f },
                new[] { -210f, -140f, -65f, -140f, -210f },
                new[] { -230f, -175f, -120f, -65f, -120f }
            };
            float[] path = paths[(spawnCount / 2) % paths.Length];
            for (int i = 0; i < path.Length; i++)
            {
                GameObject obj = new GameObject("金币" + (i + 1), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                obj.transform.SetParent(playArea, false);
                RectTransform rect = obj.GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(70f, 70f);
                rect.anchoredPosition = new Vector2(1060f + i * 88f, path[i]);
                Image image = obj.GetComponent<Image>();
                image.sprite = coinSprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                obj.AddComponent<CollectibleGlow>().pulseAmount = 0.025f;
                items.Add(new SpawnedItem { rect = rect, kind = 0, beastIndex = -1, baseY = path[i] });
            }
        }

        private void RefreshStatus()
        {
            if (statusText != null) statusText.text = "脊兽 " + beasts + "/3     金币 " + coins + "     得分 " + (coins * 20) + "     体力 " + health;
            if (hintText != null) hintText.text = "石狮：跳跃（可二段跳）   宫灯：按住↓蹲下   发光脊兽：收集";
        }

        private void Finish(bool win)
        {
            finished = true;
            if (win) GameNavigator.WuMenCleared = true;
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null) resultText.text = win ? "午门已通关\n脊兽已归位" : "旅途暂歇\n再试一次吧";
            StartCoroutine(ShowResult(win));
        }

        private IEnumerator ShowResult(bool win)
        {
            if (resultPanel == null) yield break;
            CanvasGroup group = resultPanel.GetComponent<CanvasGroup>();
            if (group == null) group = resultPanel.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            float time = 0f;
            while (time < 0.6f)
            {
                time += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(time / 0.6f);
                yield return null;
            }
            group.alpha = 1f;
            if (!win) { group.blocksRaycasts = true; yield break; }

            yield return new WaitForSecondsRealtime(2.2f);
            if (resultText != null)
            {
                Color original = resultText.color;
                time = 0f;
                while (time < 0.4f)
                {
                    time += Time.unscaledDeltaTime;
                    resultText.color = new Color(original.r, original.g, original.b, 1f - Mathf.Clamp01(time / 0.4f));
                    yield return null;
                }
                resultText.gameObject.SetActive(false);
            }

            int score = coins * 20;
            if (rankImage != null)
            {
                // 三只脊兽相隔约十二秒，按约二十组金币路线的常见拾取量评级。
                rankImage.sprite = score >= 800 ? rankExcellent : score >= 400 ? rankFirst : rankSecond;
                rankImage.gameObject.SetActive(true);
            }
            if (scoreText != null)
            {
                scoreText.text = "本局得分 " + score + "  ·  金币 " + coins + " 枚";
                scoreText.gameObject.SetActive(true);
            }
            time = 0f;
            while (time < 0.5f)
            {
                time += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(time / 0.5f);
                if (rankImage != null) rankImage.color = new Color(1f, 1f, 1f, alpha);
                if (scoreText != null)
                {
                    Color c = scoreText.color;
                    scoreText.color = new Color(c.r, c.g, c.b, alpha);
                }
                yield return null;
            }
            group.blocksRaycasts = true;
            yield return new WaitForSecondsRealtime(4f);
            SceneTransition.Load(GameNavigator.WuMenKnowledge);
        }

        public void OpenSettings()
        {
            if (introPlaying || finished || paused) return;
            paused = true;
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void ReturnGame()
        {
            paused = false;
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        public void Restart() { SceneTransition.Load(GameNavigator.WuMen); }
        public void OpenMap() { SceneTransition.Load(GameNavigator.Map); }
        public void QuitGame() { SceneTransition.Quit(); }

        private void OnDestroy()
        {
            if (introPlayer != null)
            {
                introPlayer.prepareCompleted -= OnVideoPrepared;
                introPlayer.loopPointReached -= OnVideoFinished;
                introPlayer.errorReceived -= OnVideoError;
                introPlayer.targetTexture = null;
            }
            if (videoTexture != null) { videoTexture.Release(); Destroy(videoTexture); }
            if (slicedHudSprite != null) Destroy(slicedHudSprite);
            if (tutorialOverlay != null) Destroy(tutorialOverlay);
        }
    }
}
