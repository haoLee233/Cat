using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
	public static GameUI Instance;

	[Header("UI 物体")]
	public GameObject startPopup;

	public TextMeshProUGUI collectText;

	[Header("设置")]
	public int totalNeed = 3;

	public float popupSlideSpeed = 1200f;

	public float popupStayTime = 3f;

	private int currentCollect;

	private bool popupPlayed;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		SetupPopupAsReadOnly();
		if (startPopup != null)
		{
			CanvasGroup canvasGroup = startPopup.GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = startPopup.AddComponent<CanvasGroup>();
			}
			canvasGroup.alpha = 0f;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
		}
	}

	private void SetupPopupAsReadOnly()
	{
		if (!(startPopup == null))
		{
			Button component = startPopup.GetComponent<Button>();
			if (component != null)
			{
				Object.Destroy(component);
			}
			EventTrigger component2 = startPopup.GetComponent<EventTrigger>();
			if (component2 != null)
			{
				Object.Destroy(component2);
			}
			CanvasGroup canvasGroup = startPopup.GetComponent<CanvasGroup>();
			if (canvasGroup == null)
			{
				canvasGroup = startPopup.AddComponent<CanvasGroup>();
			}
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
		}
	}

	private void Start()
	{
		UpdateCollectUI();
	}

	private void Update()
	{
		if (!popupPlayed && startPopup != null && startPopup.activeSelf)
		{
			popupPlayed = true;
			StartCoroutine(PopupSlideInOut());
		}
	}

	private IEnumerator PopupSlideInOut()
	{
		new Vector3(-Screen.width / 2 - 100, startPopup.transform.localPosition.y, 0f);
		Vector3 targetPos = new Vector3(0f, startPopup.transform.localPosition.y, 0f);
		CanvasGroup cg = startPopup.GetComponent<CanvasGroup>();
		if (cg == null)
		{
			cg = startPopup.AddComponent<CanvasGroup>();
		}
		while (Vector3.Distance(startPopup.transform.localPosition, targetPos) > 10f)
		{
			startPopup.transform.localPosition = Vector3.MoveTowards(startPopup.transform.localPosition, targetPos, popupSlideSpeed * Time.unscaledDeltaTime);
			cg.alpha = Mathf.MoveTowards(cg.alpha, 1f, 5f * Time.unscaledDeltaTime);
			yield return null;
		}
		startPopup.transform.localPosition = targetPos;
		cg.alpha = 1f;
		yield return new WaitForSecondsRealtime(popupStayTime);
		Vector3 endPos = new Vector3(Screen.width / 2 + 100, startPopup.transform.localPosition.y, 0f);
		while (Vector3.Distance(startPopup.transform.localPosition, endPos) > 10f)
		{
			startPopup.transform.localPosition = Vector3.MoveTowards(startPopup.transform.localPosition, endPos, popupSlideSpeed * Time.unscaledDeltaTime);
			cg.alpha = Mathf.MoveTowards(cg.alpha, 0f, 5f * Time.unscaledDeltaTime);
			yield return null;
		}
		startPopup.transform.localPosition = endPos;
		cg.alpha = 0f;
		startPopup.SetActive(value: false);
	}

	public void AddCollect()
	{
		currentCollect = Mathf.Min(currentCollect + 1, totalNeed);
		UpdateCollectUI();
	}

	private void UpdateCollectUI()
	{
		if (collectText != null)
		{
			collectText.text = currentCollect + " / " + totalNeed;
		}
	}
}
