using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	[Header("结算图片")]
	public GameObject SuccessImg;

	public GameObject FailImg;

	public GameObject RankBgImg;

	public GameObject ChaofanImg;

	public GameObject JiaImg;

	public GameObject YiImg;

	[Header("得分计数器")]
	public TextMeshProUGUI scoreText;

	[Header("动画设置")]
	public float fadeDuration = 0.8f;

	public float showSuccessTime = 2f;

	public int score;

	private int beastCount;

	public bool gameOver;

	private void Awake()
	{
		Instance = this;
		if (scoreText != null)
		{
			scoreText.gameObject.SetActive(value: true);
			scoreText.text = score.ToString();
			scoreText.alpha = 1f;
		}
		SetAlpha(SuccessImg, 0f);
		SetAlpha(FailImg, 0f);
		SetAlpha(RankBgImg, 0f);
		SetAlpha(ChaofanImg, 0f);
		SetAlpha(JiaImg, 0f);
		SetAlpha(YiImg, 0f);
	}

	public void CollectBeast()
	{
		beastCount++;
		if (beastCount >= 3)
		{
			StartCoroutine(WinSequence());
		}
	}

	public void AddScore(int value)
	{
		score += value;
		if (scoreText != null)
		{
			scoreText.text = score.ToString();
			scoreText.ForceMeshUpdate();
		}
	}

	private IEnumerator WinSequence()
	{
		gameOver = true;
		yield return new WaitForSeconds(0.2f);
		Time.timeScale = 0f;
		Object.FindObjectOfType<UISettings>().GameOverPause();
		SuccessImg.SetActive(value: true);
		yield return StartCoroutine(FadeIn(SuccessImg, fadeDuration));
		yield return new WaitForSecondsRealtime(showSuccessTime);
		RankBgImg.SetActive(value: true);
		yield return StartCoroutine(FadeIn(RankBgImg, fadeDuration));
		yield return new WaitForSecondsRealtime(0.3f);
		if (score >= 2000)
		{
			ChaofanImg.SetActive(value: true);
			yield return StartCoroutine(FadeIn(ChaofanImg, fadeDuration));
		}
		else if (score >= 1000)
		{
			JiaImg.SetActive(value: true);
			yield return StartCoroutine(FadeIn(JiaImg, fadeDuration));
		}
		else
		{
			YiImg.SetActive(value: true);
			yield return StartCoroutine(FadeIn(YiImg, fadeDuration));
		}
		PlayerPrefs.SetInt("WuMen_Solved", 1);
		PlayerPrefs.Save();
		yield return new WaitForSecondsRealtime(3f);
		Time.timeScale = 1f;
		Debug.Log("准备跳转回地图，场景名是：Map_Panel1");
		SceneManager.LoadScene("Map_Panel1");
	}

	public void Lose()
	{
		if (!gameOver)
		{
			gameOver = true;
			StartCoroutine(LoseSequence());
		}
	}

	private IEnumerator LoseSequence()
	{
		Time.timeScale = 0f;
		Object.FindObjectOfType<UISettings>().GameOverPause();
		FailImg.SetActive(value: true);
		yield return StartCoroutine(FadeIn(FailImg, fadeDuration));
	}

	private IEnumerator FadeIn(GameObject obj, float duration)
	{
		if (!(obj.GetComponent<SpriteRenderer>() == null))
		{
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.unscaledDeltaTime;
				float alpha = Mathf.Clamp01(elapsed / duration);
				SetAlpha(obj, alpha);
				yield return null;
			}
			SetAlpha(obj, 1f);
		}
	}

	private void SetAlpha(GameObject obj, float alpha)
	{
		SpriteRenderer component = obj.GetComponent<SpriteRenderer>();
		if (component != null)
		{
			Color color = component.color;
			color.a = alpha;
			component.color = color;
		}
	}
}
