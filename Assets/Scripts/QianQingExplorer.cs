using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class QianQingExplorer : MonoBehaviour
    {
        public QianQingRoom[] rooms;
        public RectTransform cat;
        public Image catImage;
        public Sprite[] runFrames;
        public Text roomTitle;
        public Text status;
        public Text hint;
        public Image[] mapNodes;
        public GameObject ending;
        public CanvasGroup fade;

        private readonly bool[] found = new bool[3];
        private readonly string[] roomNames = { "乾清宫前庭", "丹陛高台", "乾清宫正殿" };
        private readonly string[] relicNames = { "铜鹤", "嘉量", "正大光明匾" };
        private int currentRoom;
        private int jumps;
        private float verticalSpeed;
        private float hintUntil;
        private float animationTime;
        private bool changing;
        private const float Floor = -353f;
        private const float HalfCat = 47f;
        private const float Gravity = 1500f;

        private void Start()
        {
            if (ending != null) ending.SetActive(false);
            if (fade != null) { fade.alpha = 0f; fade.blocksRaycasts = false; }
            cat.anchoredPosition = new Vector2(-760f, Floor + HalfCat);
            ShowRoom(0);
            SetHint("A/D 或方向键移动 · 空格二段跳 · 靠近器物按 E 查阅", 8f);
        }

        private void Update()
        {
            if (changing || (ending != null && ending.activeSelf)) return;
            float dt = Time.deltaTime;
            Vector2 old = cat.anchoredPosition;
            float input = Input.GetAxisRaw("Horizontal");
            float x = old.x + input * 390f * dt;
            if (catImage != null && runFrames != null && runFrames.Length > 0 && Mathf.Abs(input) > 0.1f)
            {
                animationTime += dt * 9f;
                catImage.sprite = runFrames[(int)animationTime % runFrames.Length];
                catImage.rectTransform.localScale = new Vector3(input < 0f ? -1f : 1f, 1f, 1f);
            }
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && jumps < 2)
            {
                verticalSpeed = jumps == 0 ? 720f : 660f;
                jumps++;
            }
            verticalSpeed -= Gravity * dt;
            float y = old.y + verticalSpeed * dt;
            float oldFeet = old.y - HalfCat;
            float newFeet = y - HalfCat;
            float landing = Floor;
            if (rooms != null && currentRoom < rooms.Length && rooms[currentRoom] != null)
            {
                foreach (RectTransform platform in rooms[currentRoom].platforms)
                {
                    if (platform == null) continue;
                    float top = platform.anchoredPosition.y + platform.sizeDelta.y * 0.5f;
                    float halfWidth = platform.sizeDelta.x * 0.5f;
                    if (verticalSpeed <= 0f && oldFeet >= top - 3f && newFeet <= top &&
                        x + 55f > platform.anchoredPosition.x - halfWidth && x - 55f < platform.anchoredPosition.x + halfWidth)
                        landing = Mathf.Max(landing, top);
                }
            }
            if (newFeet <= landing && verticalSpeed <= 0f)
            {
                y = landing + HalfCat;
                verticalSpeed = 0f;
                jumps = 0;
            }
            cat.anchoredPosition = new Vector2(Mathf.Clamp(x, -980f, 980f), y);

            if (Input.GetKeyDown(KeyCode.E)) TryInspect();
            if (x > 943f)
            {
                if (currentRoom < 2) StartCoroutine(ChangeRoom(currentRoom + 1, true));
                else if (AllFound()) Finish();
                else { cat.anchoredPosition = new Vector2(920f, y); SetHint("还有器物尚未查阅，沿左侧返回寻找。", 3f); }
            }
            else if (x < -943f)
            {
                if (currentRoom > 0) StartCoroutine(ChangeRoom(currentRoom - 1, false));
                else cat.anchoredPosition = new Vector2(-920f, y);
            }
            if (Time.time > hintUntil && hint != null) hint.text = "走到场景边缘切换区域 · 靠近器物按 E 查阅";
        }

        private void TryInspect()
        {
            QianQingRoom room = rooms[currentRoom];
            if (found[currentRoom] || room.relic == null) return;
            if (Vector2.Distance(cat.anchoredPosition, room.relic.anchoredPosition) > 160f)
            {
                SetHint("走近「" + relicNames[currentRoom] + "」后按 E。", 2.5f);
                return;
            }
            found[currentRoom] = true;
            room.relic.gameObject.SetActive(false);
            UpdateStatus();
            SetHint("已查阅「" + relicNames[currentRoom] + "」！继续向右探索。", 4f);
        }

        private bool AllFound() { return found[0] && found[1] && found[2]; }

        private void Finish()
        {
            GameNavigator.QianQingGongCleared = true;
            if (ending != null) ending.SetActive(true);
            SetHint("三处乾清宫器物均已查阅。", 6f);
        }

        public void ReturnToMap() { SceneTransition.Load(GameNavigator.Map); }

        private IEnumerator ChangeRoom(int next, bool forward)
        {
            changing = true;
            if (fade != null)
            {
                fade.blocksRaycasts = true;
                for (float t = 0f; t < 0.32f; t += Time.unscaledDeltaTime)
                { fade.alpha = Mathf.Clamp01(t / 0.32f); yield return null; }
                fade.alpha = 1f;
            }
            ShowRoom(next);
            cat.anchoredPosition = new Vector2(forward ? -845f : 845f, Floor + HalfCat);
            verticalSpeed = 0f;
            jumps = 0;
            if (fade != null)
            {
                for (float t = 0f; t < 0.32f; t += Time.unscaledDeltaTime)
                { fade.alpha = 1f - Mathf.Clamp01(t / 0.32f); yield return null; }
                fade.alpha = 0f;
                fade.blocksRaycasts = false;
            }
            changing = false;
        }

        private void ShowRoom(int index)
        {
            currentRoom = index;
            for (int i = 0; i < rooms.Length; i++) rooms[i].gameObject.SetActive(i == index);
            roomTitle.text = roomNames[index];
            for (int i = 0; i < mapNodes.Length; i++)
                mapNodes[i].color = i == index ? new Color(0.70f, 0.31f, 0.22f) :
                    found[i] ? new Color(0.72f, 0.56f, 0.31f) : new Color(0.35f, 0.31f, 0.27f);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            int count = (found[0] ? 1 : 0) + (found[1] ? 1 : 0) + (found[2] ? 1 : 0);
            status.text = "已查阅器物 " + count + "/3";
        }

        private void SetHint(string text, float seconds)
        {
            if (hint != null) hint.text = text;
            hintUntil = Time.time + seconds;
        }
    }
}
