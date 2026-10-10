using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniMapEntrance : MonoBehaviour
{
	[Header("跳转设置")]
	public string puzzleSceneName = "Level2_Puzzle";

	[Header("存盘设置")]
	public string solveKey = "TaiHeDian_Solved";

	[Header("提示文字")]
	public TextMeshProUGUI tipText;

	public float fadeTime = 0.3f;

	public float showTime = 1.2f;

	private bool isShowingTip;

	private void Start()
	{
		if (tipText != null)
		{
			tipText.alpha = 0f;
			tipText.text = "";
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		Debug.Log("撞到入口了！");
		if (collision.CompareTag("Player") || collision.name == "Mobao")
		{
			if (GameStatusManager.instance.CanEnterPuzzle())
			{
				Debug.Log("收集齐了！正在保存进度并跳转...");
				PlayerPrefs.SetInt(solveKey, 1);
				PlayerPrefs.Save();
				SceneManager.LoadScene(puzzleSceneName);
			}
			else
			{
				Debug.Log("还没找齐东西呢，快去寻找残页吧！");
				ShowRandomTip();
			}
		}
	}

	private void ShowRandomTip()
	{
		if (!isShowingTip && !(tipText == null))
		{
			string[] array = new string[2] { "水坑下似乎藏着什么机关......", "将文物全部收集后再试试？" };
			string msg = array[Random.Range(0, array.Length)];
			StartCoroutine(FadeTip(msg));
		}
	}

	private IEnumerator FadeTip(string msg)
	{
		isShowingTip = true;
		tipText.text = msg;
		tipText.alpha = 0f;
		float t = 0f;
		while (t < fadeTime)
		{
			t += Time.unscaledDeltaTime;
			tipText.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
			yield return null;
		}
		yield return new WaitForSecondsRealtime(showTime);
		t = 0f;
		while (t < fadeTime)
		{
			t += Time.unscaledDeltaTime;
			tipText.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
			yield return null;
		}
		tipText.alpha = 0f;
		isShowingTip = false;
	}
}
