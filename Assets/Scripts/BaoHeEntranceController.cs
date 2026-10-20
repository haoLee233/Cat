using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class BaoHeEntranceController : MonoBehaviour
    {
        // A scene-to-scene event, not saved progress: an earlier completed save must
        // never skip the puzzle or replay its completion sequence.
        public static bool PendingRecolor { get; set; }

        public RectTransform cat;
        public Image catImage;
        public Sprite[] runFrames;
        public Sprite idleFrame;
        public CanvasGroup colorBackground;
        public GameObject dialoguePanel;
        public Text speakerText;
        public Text speakerRoleText;
        public Text dialogueText;
        public Text continueHint;
        public Image dialoguePortrait;
        public Text approachHint;
        public Text sceneMessage;
        public GameObject sceneMessagePanel;
        public LevelIntroVideo completionVideo;

        private readonly string[] speakers = { "营造匠人 · 阿榆", "墨宝", "营造匠人 · 阿榆", "墨宝" };
        private readonly string[] lines =
        {
            "我是阿榆，守着这卷保和殿营造图。你看，屋檐和阶陛都失了颜色。",
            "喵？散开的图案，就是原来的营造图吗？",
            "正是。请看清殿门、屋檐和石阶，把每块图案转回原位。",
            "喵！交给我吧。"
        };

        private int lineIndex;
        private bool conversing;
        private bool conversationDone;
        private bool restoring;
        private float animationClock;
        private float idleClock;
        private Material revealMaterial;
        private Coroutine typewriter;
        private bool typing;

        private void Start()
        {
            bool returning = PendingRecolor;
            PendingRecolor = false;
            PlayerPrefs.DeleteKey("BaoHeReturnForRecolor");

            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (cat != null) cat.anchoredPosition = new Vector2(-720f, -315f);
            if (catImage != null && idleFrame != null) catImage.sprite = idleFrame;
            // The color layer stays present so the brush shader can reveal it. The
            // greyscale image underneath remains visible everywhere else.
            if (colorBackground != null)
            {
                Image colorImage = colorBackground.GetComponent<Image>();
                if (colorImage != null && colorImage.material != null &&
                    colorImage.material.HasProperty("_Reveal"))
                {
                    revealMaterial = new Material(colorImage.material);
                    revealMaterial.SetFloat("_Reveal", -0.12f);
                    colorImage.material = revealMaterial;
                    colorBackground.alpha = 1f;
                }
                else colorBackground.alpha = 0f;
            }
            if (sceneMessage != null) sceneMessage.text = "";
            if (sceneMessagePanel != null) sceneMessagePanel.SetActive(returning);
            if (returning) StartCoroutine(RestoreColorAndOpenKnowledge());
            else if (completionVideo != null) completionVideo.Play();
        }

        private void Update()
        {
            if (restoring || (completionVideo != null && completionVideo.IsPlaying)) return;
            if (conversing)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) ||
                    Input.GetKeyDown(KeyCode.Space)) AdvanceDialogue();
                return;
            }

            if (cat == null) return;
            float horizontal = Input.GetAxisRaw("Horizontal");
            float nextX = Mathf.Clamp(cat.anchoredPosition.x + horizontal * 420f * Time.deltaTime, -810f, 470f);
            cat.anchoredPosition = new Vector2(nextX, cat.anchoredPosition.y);
            if (catImage != null)
            {
                if (Mathf.Abs(horizontal) > 0.1f)
                {
                    idleClock = 0f;
                    animationClock += Time.deltaTime * 11f;
                    if (runFrames != null && runFrames.Length > 0)
                        catImage.sprite = runFrames[(int)animationClock % runFrames.Length];
                    catImage.rectTransform.localScale = new Vector3(horizontal < 0f ? -1f : 1f, 1f, 1f);
                }
                else
                {
                    idleClock += Time.deltaTime;
                    if (idleClock > 2.0f && idleFrame != null) catImage.sprite = idleFrame;
                }
            }

            bool closeToNpc = nextX >= 300f;
            SetApproachHint(closeToNpc ? "按 E 与营造匠人交谈" : "按 A / D 或 ← / → 移动，走近匠人");
            if (closeToNpc && !conversationDone)
            {
                BeginDialogue();
                return;
            }
            if (closeToNpc && Input.GetKeyDown(KeyCode.E)) BeginDialogue();
        }

        public void BeginDialogue()
        {
            if (restoring || conversing || cat == null || cat.anchoredPosition.x < 300f) return;
            conversing = true;
            lineIndex = 0;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            SetApproachHint("");
            ShowLine();
        }

        private void SetApproachHint(string message)
        {
            if (approachHint != null)
            {
                approachHint.text = message;
                Transform backing = approachHint.transform.parent != null
                    ? approachHint.transform.parent.Find("操作提示底色") : null;
                if (backing != null) backing.gameObject.SetActive(!string.IsNullOrEmpty(message));
            }
        }

        public void AdvanceDialogue()
        {
            if (!conversing) return;
            if (typing)
            {
                if (typewriter != null) StopCoroutine(typewriter);
                typewriter = null;
                typing = false;
                if (dialogueText != null) dialogueText.text = lines[lineIndex];
                if (continueHint != null) continueHint.text = "点击继续  /  E  /  Enter";
                return;
            }
            lineIndex++;
            if (lineIndex < lines.Length)
            {
                ShowLine();
                return;
            }
            conversing = false;
            conversationDone = true;
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            SceneTransition.Load(GameNavigator.BaoHeDian);
        }

        private void ShowLine()
        {
            if (speakerText != null) speakerText.text = speakers[lineIndex];
            bool catSpeaking = speakers[lineIndex] == "墨宝";
            if (speakerRoleText != null)
                speakerRoleText.text = catSpeaking ? "失色画卷的巡游者" : "保和殿营造图守护者";
            if (dialoguePortrait != null)
                dialoguePortrait.color = catSpeaking
                    ? new Color(0.58f, 0.58f, 0.58f, 0.82f) : Color.white;
            if (typewriter != null) StopCoroutine(typewriter);
            typewriter = StartCoroutine(TypeLine(lines[lineIndex]));
        }

        private IEnumerator TypeLine(string line)
        {
            typing = true;
            if (dialogueText != null) dialogueText.text = "";
            if (continueHint != null) continueHint.text = "点击显示全文  /  E  /  Enter";
            for (int count = 1; count <= line.Length; count++)
            {
                if (dialogueText != null) dialogueText.text = line.Substring(0, count);
                yield return new WaitForSecondsRealtime(0.045f);
            }
            typing = false;
            typewriter = null;
            if (continueHint != null) continueHint.text = "点击继续  /  E  /  Enter";
        }

        private IEnumerator RestoreColorAndOpenKnowledge()
        {
            restoring = true;
            if (approachHint != null) approachHint.text = "";
            if (sceneMessage != null) sceneMessage.text = "营造图归位，跟随墨宝，看色彩重回殿宇……";
            if (cat != null) cat.anchoredPosition = new Vector2(-850f, -315f);
            yield return new WaitForSecondsRealtime(0.6f);
            const float duration = 5.2f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float sweep = Mathf.SmoothStep(0f, 1f, progress);
                if (cat != null) cat.anchoredPosition = new Vector2(Mathf.Lerp(-850f, 850f, sweep), -315f);
                if (catImage != null && runFrames != null && runFrames.Length > 0)
                {
                    int frame = (int)(elapsed * 12f) % runFrames.Length;
                    if (runFrames[frame] != null) catImage.sprite = runFrames[frame];
                }
                if (revealMaterial != null)
                    revealMaterial.SetFloat("_Reveal", Mathf.Lerp(-0.12f, 1.12f, sweep));
                if (colorBackground != null)
                    colorBackground.alpha = revealMaterial != null ? 1f : sweep;
                yield return null;
            }
            if (revealMaterial != null) revealMaterial.SetFloat("_Reveal", 1.12f);
            if (colorBackground != null) colorBackground.alpha = 1f;
            if (catImage != null && idleFrame != null) catImage.sprite = idleFrame;
            if (sceneMessage != null) sceneMessage.text = "保和殿的色彩回来了";
            yield return new WaitForSecondsRealtime(1.8f);
            OpenKnowledge();
        }

        public void OpenKnowledge()
        {
            // Older serialized scenes can still have the video's completion event
            // wired here; it must not end the level when the entry film finishes.
            if (restoring) SceneTransition.Load(GameNavigator.BaoHeKnowledge);
        }
        public void ReturnToMap() { SceneTransition.Load(GameNavigator.Map); }

        private void OnDestroy()
        {
            if (revealMaterial != null) Destroy(revealMaterial);
        }
    }
}
