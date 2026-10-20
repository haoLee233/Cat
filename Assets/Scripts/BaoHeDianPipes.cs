using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class BaoHeDianPipes : MonoBehaviour
    {
        public Button[] tiles;
        public Text timerText;
        public GameObject resultPanel;
        public Text resultText;
        public GameObject retryButton;
        public Text checkFeedback;
        public float timeLimit = 300f;
        public LevelIntroVideo introVideo;

        private readonly int[] angles = new int[25];
        private float remaining;
        private bool ended;

        private void Start()
        {
            remaining = timeLimit;
            if (resultPanel != null) resultPanel.SetActive(false);
            if (tiles == null || tiles.Length != 25)
            {
                Debug.LogError("保和殿必须配置 25 块营造图拼块");
                enabled = false;
                return;
            }
            for (int i = 0; i < tiles.Length; i++)
            {
                int index = i;
                if (tiles[i].GetComponent<UIButtonClickSound>() == null)
                    tiles[i].gameObject.AddComponent<UIButtonClickSound>();
                tiles[i].onClick.AddListener(() => Rotate(index));
                angles[i] = Random.Range(0, 4);
                tiles[i].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, angles[i] * 90f);
            }
            if (AllCorrect())
            {
                angles[0] = 1;
                tiles[0].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            RefreshTimer();
            CreateSubmitControl();
        }

        private void Update()
        {
            if (ended || (introVideo != null && introVideo.IsPlaying)) return;
            remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            RefreshTimer();
            if (remaining <= 0f) End(false);
        }

        private void Rotate(int index)
        {
            if (ended || (introVideo != null && introVideo.IsPlaying) || index < 0 || index >= tiles.Length) return;
            angles[index] = (angles[index] + 1) % 4;
            tiles[index].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, angles[index] * 90f);
            if (checkFeedback != null) checkFeedback.text = "转好后，点击「提交拼图」检查。";
        }

        public void Submit()
        {
            if (ended || (introVideo != null && introVideo.IsPlaying)) return;
            if (AllCorrect()) { End(true); return; }
            int wrong = 0;
            for (int i = 0; i < angles.Length; i++) if (angles[i] != 0) wrong++;
            if (checkFeedback != null)
                checkFeedback.text = "还有 " + wrong + " 块方向不对。对照右侧原图，继续转动后再提交。";
        }

        private void CreateSubmitControl()
        {
            Canvas canvas = timerText != null ? timerText.GetComponentInParent<Canvas>() : null;
            if (canvas == null) return;
            Font readable = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 32);
            GameObject buttonObject = new GameObject("提交拼图", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(canvas.transform, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(535f, -355f);
            rect.sizeDelta = new Vector2(300f, 76f);
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.20f, 0.14f, 0.11f, 0.96f);
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(Submit);
            buttonObject.AddComponent<InkHoverButton>();
            GameObject title = new GameObject("按钮文字", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            title.transform.SetParent(buttonObject.transform, false);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero; titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(16f, 8f); titleRect.offsetMax = new Vector2(-16f, -8f);
            Text label = title.GetComponent<Text>();
            label.font = readable; label.fontSize = 34; label.text = "提交拼图";
            label.alignment = TextAnchor.MiddleCenter; label.color = new Color(1f, 0.94f, 0.81f);
            label.raycastTarget = false;

            GameObject feedbackObject = new GameObject("拼图检查反馈", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            feedbackObject.transform.SetParent(canvas.transform, false);
            RectTransform feedbackRect = feedbackObject.GetComponent<RectTransform>();
            feedbackRect.anchorMin = feedbackRect.anchorMax = new Vector2(0.5f, 0.5f);
            feedbackRect.anchoredPosition = new Vector2(535f, -417f);
            feedbackRect.sizeDelta = new Vector2(780f, 44f);
            checkFeedback = feedbackObject.GetComponent<Text>();
            checkFeedback.font = readable; checkFeedback.fontSize = 26;
            checkFeedback.text = "全部拼好后，点击上方按钮检查。";
            checkFeedback.alignment = TextAnchor.MiddleCenter;
            checkFeedback.color = new Color(0.18f, 0.11f, 0.09f);
            checkFeedback.raycastTarget = false;
            if (resultPanel != null)
            {
                buttonObject.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());
                feedbackObject.transform.SetSiblingIndex(resultPanel.transform.GetSiblingIndex());
            }
        }

        private bool AllCorrect()
        {
            for (int i = 0; i < angles.Length; i++) if (angles[i] != 0) return false;
            return true;
        }

        private void RefreshTimer()
        {
            if (timerText == null) return;
            int seconds = Mathf.CeilToInt(remaining);
            timerText.text = string.Format("剩余时间 {0:00}:{1:00}", seconds / 60, seconds % 60);
        }

        private void End(bool won)
        {
            ended = true;
            if (won) GameNavigator.BaoHeDianCleared = true;
            if (retryButton != null) retryButton.SetActive(!won);
            foreach (Button tile in tiles) if (tile != null) tile.interactable = false;
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null) resultText.text = won ? "殿宇营造图已复原\n将它带回保和殿" : "时间已到\n再试一次吧";
            if (won) StartCoroutine(OpenKnowledgeAfterResult());
        }

        private IEnumerator OpenKnowledgeAfterResult()
        {
            yield return new WaitForSecondsRealtime(2.4f);
            BaoHeEntranceController.PendingRecolor = true;
            SceneTransition.Load(GameNavigator.BaoHeEntrance);
        }

        public void Restart() { SceneTransition.Load(GameNavigator.BaoHeDian); }
        public void OpenMap() { SceneTransition.Load(GameNavigator.Map); }
    }
}
