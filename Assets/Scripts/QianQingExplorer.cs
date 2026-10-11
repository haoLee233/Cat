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
        public GameObject quiz;
        public Text quizTitle;
        public Text quizQuestion;
        public Text quizFeedback;
        public Text[] quizAnswers;

        private readonly bool[] found = new bool[3];
        private readonly string[] roomNames = { "乾清宫前庭", "丹陛高台", "东侧月台", "乾清宫正殿" };
        private readonly string[] relicNames = { "铜鹤", "嘉量", "日晷" };
        private readonly string[] questions = {
            "铜鹤置于殿前，除了装饰，它还寄托哪种寓意？",
            "嘉量是古代工匠制作的哪一种标准器？",
            "日晷依靠什么来测定时刻？",
            "工匠之器各司其职：按「量器、计时器、吉祥陈设」排列，顺序应为？"
        };
        private readonly string[,] answers = {
            { "长寿祥瑞", "测量雨水", "传递书信" },
            { "计时器", "乐器", "容量量器" },
            { "雨水深浅", "日影方向", "风向变化" },
            { "日晷 → 铜鹤 → 嘉量", "铜鹤 → 嘉量 → 日晷", "嘉量 → 日晷 → 铜鹤" }
        };
        private readonly int[] correctChoices = { 0, 2, 1, 2 };
        private int currentRoom;
        private int quizIndex = -1;
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
            if (quiz != null) quiz.SetActive(false);
            if (fade != null) { fade.alpha = 0f; fade.blocksRaycasts = false; }
            cat.anchoredPosition = new Vector2(-760f, Floor + HalfCat);
            ShowRoom(0);
            SetHint("A/D 或方向键移动 · 空格二段跳 · 靠近器物按 E 查阅", 8f);
        }

        private void Update()
        {
            if (changing || quizIndex >= 0 || (ending != null && ending.activeSelf)) return;
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
            // Invisible ground follows the real masonry and stair painted in each room.
            float landing = rooms[currentRoom].GroundAt(x);
            bool onGround = oldFeet <= rooms[currentRoom].GroundAt(old.x) + 8f && verticalSpeed <= 0f;
            if (onGround && verticalSpeed < 0f && Mathf.Abs(landing - oldFeet) < 150f)
                newFeet = Mathf.Min(newFeet, landing);
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
                if (currentRoom < rooms.Length - 1) StartCoroutine(ChangeRoom(currentRoom + 1, true));
                else if (AllFound()) OpenQuiz(3);
                else { cat.anchoredPosition = new Vector2(920f, y); SetHint("还有器物尚未查阅，沿左侧返回寻找。", 3f); }
            }
            else if (x < -943f)
            {
                if (currentRoom > 0) StartCoroutine(ChangeRoom(currentRoom - 1, false));
                else cat.anchoredPosition = new Vector2(-920f, y);
            }
            if (Time.time > hintUntil && hint != null)
                hint.text = currentRoom == 3 ? "向右走到殿门，完成工匠识物考验" :
                    "走到场景边缘切换区域 · 靠近器物按 E 查阅";
        }

        private void TryInspect()
        {
            QianQingRoom room = rooms[currentRoom];
            if (currentRoom >= found.Length || found[currentRoom] || room.relic == null) return;
            if (Vector2.Distance(cat.anchoredPosition, room.relic.anchoredPosition) > 160f)
            {
                SetHint("走近「" + relicNames[currentRoom] + "」后按 E。", 2.5f);
                return;
            }
            OpenQuiz(currentRoom);
        }

        private void OpenQuiz(int index)
        {
            quizIndex = index;
            quiz.SetActive(true);
            quizTitle.text = index == 3 ? "工匠识物 · 通关考验" : "查阅「" + relicNames[index] + "」";
            quizQuestion.text = questions[index];
            quizFeedback.text = "选出正确答案，方可收入图鉴。";
            for (int i = 0; i < 3; i++) quizAnswers[i].text = answers[index, i];
        }

        public void Answer(int choice)
        {
            if (quizIndex < 0) return;
            if (choice != correctChoices[quizIndex]) { quizFeedback.text = "再观察器物的用途，重新作答。"; return; }
            int solved = quizIndex;
            quizIndex = -1;
            quiz.SetActive(false);
            if (solved == 3) { Finish(); return; }
            found[solved] = true;
            rooms[solved].relic.gameObject.SetActive(false);
            UpdateStatus();
            SetHint("已查阅「" + relicNames[solved] + "」！继续探索。", 4f);
        }

        public void CloseQuiz()
        {
            quizIndex = -1;
            quiz.SetActive(false);
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
            float spawnX = forward ? -845f : 845f;
            cat.anchoredPosition = new Vector2(spawnX, rooms[next].GroundAt(spawnX) + HalfCat);
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
                    i < found.Length && found[i] ? new Color(0.72f, 0.56f, 0.31f) : new Color(0.35f, 0.31f, 0.27f);
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
