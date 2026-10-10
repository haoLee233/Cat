using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class WuMenRunner : MonoBehaviour
    {
        public RectTransform playArea;
        public RectTransform cat;
        public Text statusText;
        public Text hintText;
        public GameObject resultPanel;
        public Text resultText;
        public Sprite coinSprite;
        public Sprite[] beastSprites;
        public Sprite lionSprite;
        public Sprite lanternSprite;

        private const float GroundY = -260f;
        private const float CatX = -600f;
        private readonly List<SpawnedItem> items = new List<SpawnedItem>();
        private float verticalSpeed;
        private float jumpHeight;
        private float nextSpawn;
        private float elapsed;
        private int health = 3;
        private int coins;
        private int beasts;
        private bool finished;
        private bool paused;

        private sealed class SpawnedItem
        {
            public RectTransform rect;
            public int kind; // 0 coin, 1 beast, 2 obstacle
        }

        private void Start()
        {
            Time.timeScale = 1f;
            if (resultPanel != null) resultPanel.SetActive(false);
            nextSpawn = 1.25f;
            RefreshStatus();
        }

        private void Update()
        {
            if (finished || paused) return;
            float dt = Time.deltaTime;
            elapsed += dt;

            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetMouseButtonDown(0)) && jumpHeight <= 0.01f)
                verticalSpeed = 840f;
            bool ducking = Input.GetKey(KeyCode.DownArrow) && jumpHeight <= 0.01f;
            cat.localScale = ducking ? new Vector3(1.25f, 0.64f, 1f) : Vector3.one;

            verticalSpeed -= 2100f * dt;
            jumpHeight = Mathf.Max(0f, jumpHeight + verticalSpeed * dt);
            if (jumpHeight <= 0f) verticalSpeed = 0f;
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
                if (dx < 85f && dy < (ducking ? 47f : 75f))
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

        private void SpawnNext()
        {
            int roll = Random.Range(0, 5);
            int kind = roll == 0 ? 1 : roll <= 2 ? 0 : 2;
            Sprite sprite = kind == 0 ? coinSprite : kind == 1 ? beastSprites[Random.Range(0, beastSprites.Length)] : (Random.value < 0.5f ? lionSprite : lanternSprite);
            if (sprite == null) return;
            GameObject obj = new GameObject(kind == 0 ? "金币" : kind == 1 ? "脊兽" : "障碍", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            obj.transform.SetParent(playArea, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = kind == 0 ? new Vector2(92f, 92f) : kind == 1 ? new Vector2(140f, 150f) : new Vector2(150f, 170f);
            rect.anchoredPosition = new Vector2(1060f, kind == 0 ? -120f : kind == 1 ? -225f : -235f);
            Image image = obj.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            items.Add(new SpawnedItem { rect = rect, kind = kind });
        }

        private void RefreshStatus()
        {
            if (statusText != null) statusText.text = "脊兽 " + beasts + "/3     金币 " + coins + "     体力 " + health;
            if (hintText != null) hintText.text = "空格 / ↑ 跳跃    ↓ 蹲下    收集三只脊兽";
        }

        private void Finish(bool win)
        {
            finished = true;
            if (win) GameNavigator.WuMenCleared = true;
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null) resultText.text = win ? "午门已通关\n脊兽已归位" : "旅途暂歇\n再试一次吧";
        }

        public void Restart() { Time.timeScale = 1f; SceneManager.LoadScene(GameNavigator.WuMen); }
        public void OpenMap() { Time.timeScale = 1f; SceneManager.LoadScene(GameNavigator.Map); }
        public void TogglePause()
        {
            if (finished) return;
            paused = !paused;
            if (resultPanel != null) resultPanel.SetActive(paused);
            if (resultText != null) resultText.text = paused ? "暂停游戏" : "";
        }
    }
}
