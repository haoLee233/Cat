using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleUIManager : MonoBehaviour
{
	[Header("封面相关组件")]
	public GameObject UI_Cover;

	public CanvasGroup CoverCanvasGroup;

	public TextMeshProUGUI StartText;

	[Header("动效参数")]
	public float fadeInTime = 1.5f;

	public float textBlinkSpeed = 1f;

	public float fadeOutTime = 1f;

	private float textAlpha = 1f;

	private bool isCoverShow = true;

	private void Start()
	{
		InitCover();
		StartCoroutine(CoverFadeIn());
	}

	private void Update()
	{
		if (isCoverShow && StartText != null)
		{
			TextBlink();
		}
		if (isCoverShow && Input.GetMouseButtonDown(0))
		{
			OnCoverClick();
		}
	}

	private void InitCover()
	{
		if (UI_Cover != null)
		{
			UI_Cover.SetActive(value: true);
		}
		if (CoverCanvasGroup != null)
		{
			CoverCanvasGroup.alpha = 0f;
			CoverCanvasGroup.interactable = true;
			CoverCanvasGroup.blocksRaycasts = true;
		}
		if (StartText != null)
		{
			StartText.alpha = 1f;
		}
	}

	private IEnumerator CoverFadeIn()
	{
		float elapsedTime = 0f;
		while (elapsedTime < fadeInTime)
		{
			if (CoverCanvasGroup != null)
			{
				CoverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInTime);
			}
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		if (CoverCanvasGroup != null)
		{
			CoverCanvasGroup.alpha = 1f;
		}
	}

	private void TextBlink()
	{
		textAlpha = Mathf.Sin(Time.time * textBlinkSpeed * (float)Math.PI);
		textAlpha = Mathf.Lerp(0.5f, 1f, textAlpha);
		StartText.alpha = textAlpha;
	}

	public void OnCoverClick()
	{
		if (isCoverShow)
		{
			isCoverShow = false;
			StartCoroutine(CoverFadeOut());
		}
	}

	private IEnumerator CoverFadeOut()
	{
		float elapsedTime = 0f;
		while (elapsedTime < fadeOutTime)
		{
			if (CoverCanvasGroup != null)
			{
				CoverCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutTime);
			}
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		if (CoverCanvasGroup != null)
		{
			CoverCanvasGroup.alpha = 0f;
			CoverCanvasGroup.interactable = false;
			CoverCanvasGroup.blocksRaycasts = false;
		}
		if (UI_Cover != null)
		{
			UI_Cover.SetActive(value: false);
		}
		SceneManager.LoadScene("Map_Panel1");
	}

	private void ShowMainMap()
	{
	}
}
