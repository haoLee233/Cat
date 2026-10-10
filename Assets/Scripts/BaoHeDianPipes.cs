using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class BaoHeDianPipes : MonoBehaviour
    {
        public Button[] tiles;
        public Text timerText;
        public GameObject resultPanel;
        public Text resultText;
        public float timeLimit = 300f;

        private readonly int[] angles = new int[25];
        private float remaining;
        private bool ended;

        private void Start()
        {
            remaining = timeLimit;
            if (resultPanel != null) resultPanel.SetActive(false);
            if (tiles == null || tiles.Length != 25)
            {
                Debug.LogError("保和殿必须配置 25 块管道");
                enabled = false;
                return;
            }
            for (int i = 0; i < tiles.Length; i++)
            {
                int index = i;
                tiles[i].onClick.AddListener(() => Rotate(index));
                angles[i] = Random.Range(0, 4);
                tiles[i].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, angles[i] * 90f);
            }
            if (AllCorrect()) Rotate(0);
            RefreshTimer();
        }

        private void Update()
        {
            if (ended) return;
            remaining = Mathf.Max(0f, remaining - Time.deltaTime);
            RefreshTimer();
            if (remaining <= 0f) End(false);
        }

        private void Rotate(int index)
        {
            if (ended || index < 0 || index >= tiles.Length) return;
            angles[index] = (angles[index] + 1) % 4;
            tiles[index].GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, angles[index] * 90f);
            if (AllCorrect()) End(true);
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
            foreach (Button tile in tiles) if (tile != null) tile.interactable = false;
            if (resultPanel != null) resultPanel.SetActive(true);
            if (resultText != null) resultText.text = won ? "水路接通\n保和殿已通关" : "时间已到\n再试一次吧";
        }

        public void Restart() { SceneManager.LoadScene(GameNavigator.BaoHeDian); }
        public void OpenMap() { SceneManager.LoadScene(GameNavigator.Map); }
    }
}
