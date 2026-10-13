using UnityEngine;
using UnityEngine.UI;

namespace JinYanMaoYing
{
    public sealed class TaiHeFinale : MonoBehaviour
    {
        public Button[] parts;
        public Text instruction;
        public Text feedback;
        public Text progress;
        public GameObject completionPanel;
        public LevelIntroVideo introVideo;

        private readonly int[] order = { 1, 2, 0 };
        private readonly string[] clues = {
            "第一笔：从下方开始。哪一部分托起大殿？",
            "第二笔：抬头看檐下。哪一部分承接屋檐？",
            "最后一笔：最高处的哪一部分仍缺少颜色？"
        };
        private readonly string[] restored = { "台基", "斗拱", "屋脊" };
        private int step;
        private bool finished;

        private void Start()
        {
            if (completionPanel != null) completionPanel.SetActive(false);
            ShowStep();
        }

        public void SelectPart(int index)
        {
            if (finished || (introVideo != null && introVideo.IsPlaying) || parts == null || parts.Length != 3 || index < 0 || index >= parts.Length) return;
            if (index != order[step])
            {
                if (feedback != null) feedback.text = "再看看线索：沿着建筑由下向上寻找。";
                return;
            }

            Button selected = parts[index];
            selected.interactable = false;
            Image image = selected.GetComponent<Image>();
            if (image != null) image.color = new Color(0.65f, 0.35f, 0.24f, 0.95f);
            if (feedback != null) feedback.text = "已找回「" + restored[step] + "」的色彩。";
            step++;
            if (step == order.Length)
            {
                finished = true;
                GameNavigator.TaiHeDianCleared = true;
                if (progress != null) progress.text = "复原 3/3";
                if (instruction != null) instruction.text = "太和殿复色完成。";
                if (completionPanel != null) completionPanel.SetActive(true);
                return;
            }
            ShowStep();
        }

        private void ShowStep()
        {
            if (progress != null) progress.text = "复原 " + step + "/3";
            if (instruction != null) instruction.text = clues[step];
        }

        public void OpenEnding() { SceneTransition.Load(GameNavigator.Ending); }
        public void OpenMap() { SceneTransition.Load(GameNavigator.Map); }
    }
}
