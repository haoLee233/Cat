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
        public RectTransform playArea;
        public RectTransform[] backgrounds;
        public RectTransform cat;
        public Image catImage;
        public Sprite[] runFrames;
        public Text statusText;
        public Text hintText;
        public GameObject resultPanel;
        public Text resultText;
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
        private const float CatX = -600f;
        private readonly List<SpawnedItem> items = new List<SpawnedItem>();
        private RenderTexture videoTexture;
        private float verticalSpeed, jumpHeight, nextSpawn, elapsed, runFrameTime;
        private int jumpsUsed, coins, beasts;
        private int health = 3;
        private bool finished, paused, introPlaying = true;

        private sealed class SpawnedItem
        {
            public RectTransform rect;
            public int kind; // 0 金币，1 脊兽，2 障碍
        }

        private void Start()
        {
            Time.timeScale = 1f;
            if (resultPanel != null) resultPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (missionPrompt != null) missionPrompt.gameObject.SetActive(false);
            nextSpawn = 1.25f;
            RefreshStatus();
            if (introClip == null || introPlayer == null || videoScreen == null)
            {
                Debug.LogError("午门片头视频未配置");
                StartCoroutine(ShowMission());
                return;
            }
            videoTexture = new RenderTexture(1920, 1080, 0);
            videoTexture.Create();
            videoScreen.texture = videoTexture;
            introPlayer.clip = introClip;
            introPlayer.targetTexture = videoTexture;
            introPlayer.renderMode = VideoRenderMode.RenderTexture;
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

        private void OnVideoPrepared(VideoPlayer player) { player.Play(); }
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

        private IEnumerator ShowMission()
        {
            if (missionPrompt == null) { introPlaying = false; yield break; }
            missionPrompt.gameObject.SetActive(true);
            yield return SlideMission(-2200f, 0f, 0.65f);
            yield return new WaitForSecondsRealtime(3.3f);
            yield return SlideMission(0f, 2200f, 0.65f);
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
            float dt = Time.deltaTime;
            elapsed += dt;
            MoveBackground(dt);
            AnimateCat(dt);
            bool keyboardJump = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow);
            bool mouseJump = Input.GetMouseButtonDown(0) &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject());
            if ((keyboardJump || mouseJump) && jumpsUsed < 2)
            {
                verticalSpeed = jumpsUsed == 0 ? 840f : 780f;
                jumpsUsed++;
            }
            bool ducking = Input.GetKey(KeyCode.DownArrow) && jumpHeight <= 0.01f;
            cat.localScale = ducking ? new Vector3(1f, 0.64f, 1f) : Vector3.one;
            verticalSpeed -= 2100f * dt;
            jumpHeight = Mathf.Max(0f, jumpHeight + verticalSpeed * dt);
            if (jumpHeight <= 0f) { verticalSpeed = 0f; jumpsUsed = 0; }
            cat.anchoredPosition = new Vector2(CatX, GroundY + jumpHeight);

            if (elapsed >= nextSpawn)
            {
                SpawnNext();
                nextSpawn = elapsed + Random.Range(1.15f, 1.75f);
            }
            for (int i = items.Count - 1; i >= 0; i--)
            {
                SpawnedItem item = items[i];
                item.rect.anchoredPosition += Vector2.left * 560f * dt;
                float dx = Mathf.Abs(item.rect.anchoredPosition.x - CatX);
                float dy = Mathf.Abs(item.rect.anchoredPosition.y - cat.anchoredPosition.y);
                float halfHeight = (cat.sizeDelta.y * cat.localScale.y + item.rect.sizeDelta.y) * 0.5f;
                if (dx < 85f && dy < halfHeight)
                {
                    if (item.kind == 0) coins++;
                    else if (item.kind == 1) beasts++;
                    else health--;
                    Destroy(item.rect.gameObject);
                    items.RemoveAt(i);
                    RefreshStatus();
                    if (beasts >= 3) Finish(true);
                    else if (health <= 0) Finish(false);
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
                Vector2 pos = background.anchoredPosition + Vector2.left * 240f * dt;
                if (pos.x <= -1920f) pos.x += 3840f;
                background.anchoredPosition = pos;
            }
        }

        private void AnimateCat(float dt)
        {
            if (catImage == null || runFrames == null || runFrames.Length == 0) return;
            runFrameTime += dt;
            int index = Mathf.FloorToInt(runFrameTime / 0.13f) % runFrames.Length;
            if (runFrames[index] != null) catImage.sprite = runFrames[index];
        }

        private void SpawnNext()
        {
            int roll = Random.Range(0, 5);
            int kind = roll == 0 ? 1 : roll <= 2 ? 0 : 2;
            bool lantern = kind == 2 && Random.value < 0.5f;
            Sprite sprite = kind == 0 ? coinSprite : kind == 1 ? beastSprites[Random.Range(0, beastSprites.Length)] :
                (lantern ? lanternSprite : lionSprite);
            if (sprite == null) return;
            GameObject obj = new GameObject(kind == 0 ? "金币" : kind == 1 ? "脊兽" : lantern ? "宫灯" : "石狮", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(playArea, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = kind == 0 ? new Vector2(92f, 92f) : kind == 1 ? new Vector2(140f, 150f) : new Vector2(150f, 170f);
            rect.anchoredPosition = new Vector2(1060f, kind == 0 ? -120f : kind == 1 ? -225f : lantern ? -120f : -235f);
            Image image = obj.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            items.Add(new SpawnedItem { rect = rect, kind = kind });
        }

        private void RefreshStatus()
        {
            if (statusText != null) statusText.text = "脊兽 " + beasts + "/3     金币 " + coins + "     体力 " + health;
            if (hintText != null) hintText.text = "空格 / ↑ 二段跳    ↓ 蹲下躲宫灯    收集三只脊兽";
        }

        private void Finish(bool win)
        {
            finished = true;
            if (win) GameNavigator.WuMenCleared = true;
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null) resultText.text = win ? "午门已通关\n脊兽已归位" : "旅途暂歇\n再试一次吧";
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
        }
    }
}
