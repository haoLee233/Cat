using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PipeGameManager : MonoBehaviour
{
	public static PipeGameManager Instance;

	[Header("基础管道设置")]
	public Transform gridParent;

	public GameObject pipeTilePrefab;

	public Sprite[] pipeSprites;

	public int[] correctAngles;

	[Header("计时UI")]
	public TextMeshProUGUI timerText;

	public float timeOutLimit = 300f;

	[Header("挑战成功面板")]
	public GameObject successPanel;

	public CanvasGroup successCG;

	public float successFadeIn = 0.5f;

	public float successShow = 1.5f;

	public float successFadeOut = 0.5f;

	[Header("挑战失败面板")]
	public GameObject failPanel;

	public CanvasGroup failCG;

	public float failFadeIn = 0.5f;

	public float failShow = 1.5f;

	public float failFadeOut = 0.5f;

	[Header("评级面板")]
	public GameObject rankPanel;

	public CanvasGroup rankCG;

	public Image rankImage;

	public Sprite rankS;

	public Sprite rankA;

	public Sprite rankB;

	public float rankFadeIn = 0.5f;

	private PipeTile[] allTiles;

	private float gameTime;

	private bool isPlaying;

	private bool isPaused;

	private bool isGameOver;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		InitUI();
		SpawnTiles();
		isPlaying = true;
	}

	private void Update()
	{
		if (isPlaying && !isPaused && !isGameOver)
		{
			gameTime += Time.deltaTime;
			UpdateTimer();
			if (gameTime >= timeOutLimit)
			{
				OnFail();
			}
		}
	}

	private void InitUI()
	{
		successPanel.SetActive(value: false);
		failPanel.SetActive(value: false);
		rankPanel.SetActive(value: false);
	}

	private void SpawnTiles()
	{
		allTiles = new PipeTile[25];
		for (int i = 0; i < 25; i++)
		{
			GameObject gameObject = Object.Instantiate(pipeTilePrefab, gridParent);
			PipeTile component = gameObject.GetComponent<PipeTile>();
			if (pipeSprites.Length > i)
			{
				gameObject.GetComponent<Image>().sprite = pipeSprites[i];
			}
			if (correctAngles.Length > i)
			{
				component.correctAngle = correctAngles[i];
			}
			component.RandomizeAngle();
			allTiles[i] = component;
		}
	}

	private void UpdateTimer()
	{
		int num = Mathf.FloorToInt(gameTime / 60f);
		int num2 = Mathf.FloorToInt(gameTime % 60f);
		timerText.text = $"{num:00}:{num2:00}";
	}

	public void CheckWin()
	{
		if (isGameOver)
		{
			return;
		}
		PipeTile[] array = allTiles;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].IsCorrect())
			{
				return;
			}
		}
		OnWin();
	}

	private void OnWin()
	{
		GameOverLock();
		StartCoroutine(WinFlow());
	}

	private void OnFail()
	{
		GameOverLock();
		StartCoroutine(FailFlow());
	}

	private void GameOverLock()
	{
		isGameOver = true;
		isPlaying = false;
		PipeTile[] array = allTiles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].GetComponent<Button>().interactable = false;
		}
	}

	private IEnumerator WinFlow()
	{
		successPanel.SetActive(value: true);
		successCG.alpha = 0f;
		yield return FadeIn(successCG, successFadeIn);
		yield return new WaitForSeconds(successShow);
		yield return FadeOut(successCG, successFadeOut);
		successPanel.SetActive(value: false);
		rankPanel.SetActive(value: true);
		rankCG.alpha = 0f;
		SetRankSprite();
		yield return FadeIn(rankCG, rankFadeIn);
		yield return new WaitForSeconds(3f);
		SceneManager.LoadScene("Map_Panel1");
	}

	private IEnumerator FailFlow()
	{
		failPanel.SetActive(value: true);
		failCG.alpha = 0f;
		yield return FadeIn(failCG, failFadeIn);
		yield return new WaitForSeconds(failShow);
		yield return FadeOut(failCG, failFadeOut);
		failPanel.SetActive(value: false);
	}

	private void SetRankSprite()
	{
		if (gameTime <= 100f)
		{
			rankImage.sprite = rankS;
		}
		else if (gameTime <= 150f)
		{
			rankImage.sprite = rankA;
		}
		else
		{
			rankImage.sprite = rankB;
		}
	}

	private IEnumerator FadeIn(CanvasGroup cg, float time)
	{
		float t = 0f;
		while (t < time)
		{
			t += Time.deltaTime;
			cg.alpha = t / time;
			yield return null;
		}
		cg.alpha = 1f;
	}

	private IEnumerator FadeOut(CanvasGroup cg, float time)
	{
		float t = 0f;
		while (t < time)
		{
			t += Time.deltaTime;
			cg.alpha = 1f - t / time;
			yield return null;
		}
		cg.alpha = 0f;
	}

	public void SetPause(bool p)
	{
		isPaused = p;
	}

	public void RestartGame()
	{
		isGameOver = false;
		isPlaying = true;
		gameTime = 0f;
		PipeTile[] array = allTiles;
		for (int i = 0; i < array.Length; i++)
		{
			Object.Destroy(array[i].gameObject);
		}
		SpawnTiles();
		InitUI();
	}
}
