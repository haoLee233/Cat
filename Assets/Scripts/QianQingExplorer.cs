using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Sprites;

namespace JinYanMaoYing
{
    public sealed class QianQingExplorer : MonoBehaviour
    {
        public QianQingRoom[] rooms;
        public RectTransform cat;
        public Image catImage;
        public Sprite[] runFrames;
        public Sprite idleSprite;
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
        public GameObject videoOverlay;
        public RawImage videoScreen;
        public VideoPlayer introPlayer;
        public VideoClip introClip;

        private readonly bool[] found = new bool[3];
        private readonly string[] roomNames = { "乾清宫前庭", "丹陛高台", "东侧月台", "乾清宫正殿" };
        private readonly string[] relicNames = { "铜鹤", "嘉量", "日晷" };
        private readonly string[] questions = {
            "铜鹤置于殿前，除了装饰，它还寄托哪种寓意？",
            "嘉量是古代工匠制作的哪一种标准器？",
            "日晷依靠什么来测定时刻？",
            "工匠之器各司其职：按「量器、计时器、吉祥陈设」排列，顺序应为？"
        };
        private readonly string[] observations = {
            "先看前庭：铜鹤立在殿前石地，长颈展翼，是匠人制作的吉祥陈设。记住它的造型，再辨认用途。",
            "先看丹陛：嘉量是一件带量筒的标准器，安放在殿前高台。观察器身，再想想它用来统一什么。",
            "先看东侧月台：日晷立在石地基座上，晷针投影随日光而变。观察晷面，再判断它怎样计时。"
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
        private bool observing;
        private int jumps;
        private float verticalSpeed;
        private float hintUntil;
        private float animationTime;
        private float idleTime;
        private float secondJumpPulse;
        private float lastDirectionTapTime = -10f;
        private int lastTapDirection;
        private int facing = 1;
        private bool sprinting;
        private bool changing;
        private bool introPlaying = true;
        private RenderTexture videoTexture;
        private RawImage relicPreview;
        private RectTransform relicPreviewFrame;
        private Sprite jumpSprite;
        private const float Floor = -353f;
        private const float HalfCat = 47f;
        private const float Gravity = 1500f;

        private void Start()
        {
            // Exploration uses the complete WuMen running cycle when available.
            string[] frameNames = { "猫跑1", "猫跑中间帧1", "猫跑中间帧4", "猫跑2",
                "猫跑中间帧2", "猫跑中间帧5", "猫跑3", "猫跑中间帧3", "猫跑中间帧6" };
            Sprite[] explorationFrames = new Sprite[frameNames.Length];
            bool completeCycle = true;
            for (int i = 0; i < frameNames.Length; i++)
            {
                explorationFrames[i] = Resources.Load<Sprite>("TaiHeFinale/" + frameNames[i]);
                completeCycle &= explorationFrames[i] != null;
            }
            if (completeCycle) runFrames = explorationFrames;
            jumpSprite = Resources.Load<Sprite>("TaiHeFinale/猫跑中间帧2");
            if (ending != null) ending.SetActive(false);
            if (quiz != null) quiz.SetActive(false);
            CreateRelicPreview();
            if (hint != null)
            {
                hint.font = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 32);
                hint.fontSize = 30;
                hint.horizontalOverflow = HorizontalWrapMode.Wrap;
                hint.verticalOverflow = VerticalWrapMode.Truncate;
                hint.rectTransform.sizeDelta = new Vector2(1500f, 104f);
                hint.rectTransform.anchoredPosition = new Vector2(0f, -457f);
                Transform backing = hint.transform.parent.Find("底部提示底色");
                if (backing is RectTransform backingRect)
                {
                    backingRect.sizeDelta = new Vector2(1580f, 124f);
                    backingRect.anchoredPosition = new Vector2(0f, -457f);
                }
            }
            quizTitle.fontSize = Mathf.Max(quizTitle.fontSize, 54);
            quizQuestion.fontSize = Mathf.Max(quizQuestion.fontSize, 40);
            quizFeedback.fontSize = Mathf.Max(quizFeedback.fontSize, 36);
            foreach (Text answer in quizAnswers) answer.fontSize = Mathf.Max(answer.fontSize, 42);
            if (rooms != null)
                foreach (QianQingRoom room in rooms)
                    if (room != null && room.relic != null && room.relic.GetComponent<CollectibleGlow>() == null)
                        room.relic.gameObject.AddComponent<CollectibleGlow>();
            if (fade != null) { fade.alpha = 0f; fade.blocksRaycasts = false; }
            cat.anchoredPosition = new Vector2(-760f, Floor + HalfCat);
            if (catImage != null) catImage.preserveAspect = true;
            ShowRoom(0);
            SetHint("按键盘 A、D 键左右移动；按空格键跳跃。\n走近发光的器物后，按键盘 E 键查看。", 8f);
            if (introClip == null || introPlayer == null || videoScreen == null || videoOverlay == null)
            {
                Debug.LogError("乾清宫第二段片头视频未配置");
                introPlaying = false;
                if (videoOverlay != null) videoOverlay.SetActive(false);
                return;
            }
            int width = introClip.width > 0 ? Mathf.Clamp((int)introClip.width, 16, 4096) : 1920;
            int height = introClip.height > 0 ? Mathf.Clamp((int)introClip.height, 16, 4096) : 1080;
            videoTexture = new RenderTexture(width, height, 0);
            videoTexture.Create();
            videoScreen.texture = videoTexture;
            AspectRatioFitter fit = videoScreen.GetComponent<AspectRatioFitter>();
            if (fit != null) fit.aspectRatio = (float)width / height;
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
            if (introPlaying && !introPlayer.isPrepared)
            {
                Debug.LogError("乾清宫片头准备超时");
                FinishVideo();
            }
        }

        private void OnVideoPrepared(VideoPlayer player) { if (introPlaying) player.Play(); }
        private void OnVideoFinished(VideoPlayer player) { FinishVideo(); }
        private void OnVideoError(VideoPlayer player, string message)
        {
            Debug.LogError("乾清宫片头播放失败: " + message);
            FinishVideo();
        }
        private void FinishVideo()
        {
            if (!introPlaying) return;
            introPlaying = false;
            if (introPlayer != null) introPlayer.Stop();
            if (videoOverlay != null) videoOverlay.SetActive(false);
        }
        public void SkipVideo() { FinishVideo(); }
        private void OnDestroy()
        {
            if (introPlayer != null)
            {
                introPlayer.prepareCompleted -= OnVideoPrepared;
                introPlayer.loopPointReached -= OnVideoFinished;
                introPlayer.errorReceived -= OnVideoError;
            }
            if (videoTexture != null)
            {
                videoTexture.Release();
                Destroy(videoTexture);
            }
        }

        private void Update()
        {
            if (introPlaying || changing || quizIndex >= 0 || (ending != null && ending.activeSelf)) return;
            float dt = Time.deltaTime;
            Vector2 old = cat.anchoredPosition;
            float input = Input.GetAxisRaw("Horizontal");
            int tap = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ? 1 :
                Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0;
            if (tap != 0)
            {
                sprinting = tap == lastTapDirection && Time.unscaledTime - lastDirectionTapTime < 0.32f;
                lastTapDirection = tap;
                lastDirectionTapTime = Time.unscaledTime;
            }
            if (Mathf.Abs(input) < 0.1f) sprinting = false;
            if (Mathf.Abs(input) > 0.1f) facing = input < 0f ? -1 : 1;
            float x = old.x + input * (sprinting ? 650f : 390f) * dt;
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && jumps < 2)
            {
                if (jumps == 1) secondJumpPulse = 0.18f;
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
            AnimateCat(dt, Mathf.Abs(input) > 0.1f);

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
                hint.text = RoomHint(currentRoom);
        }

        private void AnimateCat(float dt, bool moving)
        {
            if (catImage == null) return;
            secondJumpPulse = Mathf.Max(0f, secondJumpPulse - dt);
            bool airborne = jumps > 0 || Mathf.Abs(verticalSpeed) > 1f;
            if (airborne)
            {
                idleTime = 0f;
                if (jumpSprite != null) catImage.sprite = jumpSprite;
                float impulse = secondJumpPulse > 0f ? 1.08f : 1f;
                cat.localScale = new Vector3(facing * impulse, impulse, 1f);
                cat.localRotation = Quaternion.Euler(0f, 0f, facing * (secondJumpPulse > 0f ? 22f : 12f));
                return;
            }
            cat.localRotation = Quaternion.identity;
            if (moving && runFrames != null && runFrames.Length > 0)
            {
                idleTime = 0f;
                animationTime += dt * (sprinting ? 16f : 12f);
                catImage.sprite = runFrames[(int)animationTime % runFrames.Length];
                cat.localScale = new Vector3(facing, 1f, 1f);
                return;
            }
            idleTime += dt;
            if (idleTime > 3.5f && idleSprite != null) catImage.sprite = idleSprite;
            else if (runFrames != null && runFrames.Length > 0) catImage.sprite = runFrames[0];
            float breath = 1f + Mathf.Sin(Time.unscaledTime * 2.3f) * 0.018f;
            cat.localScale = new Vector3(facing * breath, breath, 1f);
        }

        private void TryInspect()
        {
            QianQingRoom room = rooms[currentRoom];
            if (currentRoom >= found.Length || found[currentRoom] || room.relic == null) return;
            if (Vector2.Distance(cat.anchoredPosition, room.relic.anchoredPosition) > 160f)
            {
                SetHint("先走近发光的「" + relicNames[currentRoom] + "」，再按键盘 E 键查看。", 2.5f);
                return;
            }
            OpenQuiz(currentRoom);
        }

        private void OpenQuiz(int index)
        {
            quizIndex = index;
            observing = index < 3;
            quiz.SetActive(true);
            ShowQuizStep();
        }

        private void ShowQuizStep()
        {
            quizTitle.text = quizIndex == 3 ? "工匠识物 · 通关考验" : "查阅「" + relicNames[quizIndex] + "」";
            quizQuestion.text = observing ? observations[quizIndex] : questions[quizIndex];
            quizFeedback.text = observing ? "先观察场景与器物，再进入辨识。" : "选出正确答案，方可收入图鉴。";
            bool showRelic = observing && quizIndex < 3 && relicPreviewFrame != null;
            if (relicPreviewFrame != null) relicPreviewFrame.gameObject.SetActive(showRelic);
            if (showRelic) ShowRelicPreview(quizIndex);
            RectTransform feedbackRect = quizFeedback.rectTransform;
            feedbackRect.anchoredPosition = new Vector2(0f, observing ? -335f : -258f);
            for (int i = 0; i < 3; i++)
            {
                quizAnswers[i].transform.parent.gameObject.SetActive(!observing || i == 0);
                quizAnswers[i].text = observing ? "我看清了，继续辨识" : answers[quizIndex, i];
                quizAnswers[i].transform.parent.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(0f, observing ? -255f : 35f - i * 86f);
            }
        }

        private void CreateRelicPreview()
        {
            if (quiz == null) return;
            GameObject frame = new GameObject("器物观察画框", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            frame.transform.SetParent(quiz.transform, false);
            relicPreviewFrame = frame.GetComponent<RectTransform>();
            relicPreviewFrame.anchorMin = relicPreviewFrame.anchorMax = new Vector2(0.5f, 0.5f);
            relicPreviewFrame.sizeDelta = new Vector2(350f, 255f);
            relicPreviewFrame.anchoredPosition = new Vector2(0f, -45f);
            Image paper = frame.GetComponent<Image>();
            paper.color = new Color(1f, 0.96f, 0.84f, 0.96f);
            paper.raycastTarget = false;
            Outline border = frame.AddComponent<Outline>();
            border.effectColor = new Color(0.47f, 0.29f, 0.16f, 0.85f);
            border.effectDistance = new Vector2(3f, -3f);
            GameObject icon = new GameObject("器物大图", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            icon.transform.SetParent(frame.transform, false);
            RectTransform rect = icon.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(260f, 225f);
            rect.anchoredPosition = Vector2.zero;
            relicPreview = icon.GetComponent<RawImage>();
            relicPreview.raycastTarget = false;
            relicPreviewFrame.SetSiblingIndex(3);
            frame.SetActive(false);
        }

        private void ShowRelicPreview(int index)
        {
            if (rooms == null || index >= rooms.Length || rooms[index] == null || rooms[index].relic == null) return;
            RawImage atlasIcon = rooms[index].relic.GetComponent<RawImage>();
            Image spriteIcon = rooms[index].relic.GetComponent<Image>();
            if (atlasIcon != null)
            {
                relicPreview.texture = atlasIcon.texture;
                relicPreview.uvRect = atlasIcon.uvRect;
            }
            else if (spriteIcon != null && spriteIcon.sprite != null)
            {
                relicPreview.texture = spriteIcon.sprite.texture;
                Vector4 uv = DataUtility.GetOuterUV(spriteIcon.sprite);
                relicPreview.uvRect = new Rect(uv.x, uv.y, uv.z - uv.x, uv.w - uv.y);
            }
            Rect uvRect = relicPreview.uvRect;
            if (relicPreview.texture != null)
            {
                float aspect = relicPreview.texture.width * uvRect.width /
                    Mathf.Max(1f, relicPreview.texture.height * uvRect.height);
                relicPreview.rectTransform.sizeDelta = aspect >= 1f ?
                    new Vector2(290f, 290f / aspect) : new Vector2(225f * aspect, 225f);
            }
        }

        public void Answer(int choice)
        {
            if (quizIndex < 0) return;
            if (observing) { observing = false; ShowQuizStep(); return; }
            if (choice != correctChoices[quizIndex]) { quizFeedback.text = "再观察器物的用途，重新作答。"; return; }
            int solved = quizIndex;
            quizIndex = -1;
            observing = false;
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
            observing = false;
            quiz.SetActive(false);
        }

        private bool AllFound() { return found[0] && found[1] && found[2]; }

        private void Finish()
        {
            GameNavigator.QianQingGongCleared = true;
            if (ending != null) ending.SetActive(true);
            SetHint("三处器物已查阅；墨宝找回了匠人的尺度与时间，乾清宫重现色彩。", 6f);
            StartCoroutine(OpenKnowledgeAfterResult());
        }

        private IEnumerator OpenKnowledgeAfterResult()
        {
            yield return new WaitForSecondsRealtime(4f);
            OpenKnowledge();
        }

        public void OpenKnowledge() { SceneTransition.Load(GameNavigator.QianQingKnowledge); }

        public void ReturnToMap() { SceneTransition.Load(GameNavigator.Map); }

        private IEnumerator ChangeRoom(int next, bool forward)
        {
            changing = true;
            if (fade != null)
            {
                fade.blocksRaycasts = true;
                for (float t = 0f; t < 0.55f; t += Time.unscaledDeltaTime)
                { fade.alpha = Mathf.Clamp01(t / 0.55f); yield return null; }
                fade.alpha = 1f;
            }
            ShowRoom(next);
            float spawnX = forward ? -845f : 845f;
            cat.anchoredPosition = new Vector2(spawnX, rooms[next].GroundAt(spawnX) + HalfCat);
            verticalSpeed = 0f;
            jumps = 0;
            if (fade != null)
            {
                for (float t = 0f; t < 0.55f; t += Time.unscaledDeltaTime)
                { fade.alpha = 1f - Mathf.Clamp01(t / 0.55f); yield return null; }
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
            if (index > 0) SetHint(RoomHint(index), 9f);
        }

        private static string RoomHint(int index)
        {
            if (index == 1) return "一直往右走，沿石阶登上高台。\n走近发光的嘉量，按键盘 E 键查看。";
            if (index == 2) return "走到石地上的日晷旁，按键盘 E 键查看。\n继续往右走，可以进入正殿。";
            if (index == 3) return "三件器物都看过后，走到画面最右边，完成最后一题。";
            return "按键盘 A、D 键左右移动；按空格键跳跃。\n走近发光的铜鹤，按键盘 E 键查看。";
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
