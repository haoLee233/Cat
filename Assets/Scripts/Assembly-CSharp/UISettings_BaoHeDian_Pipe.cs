using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UISettings_BaoHeDian_Pipe : MonoBehaviour
{
	[Header("===== 面板 =====")]
	public GameObject settingPanel;

	public CanvasGroup canvasGroup;

	[Header("===== 按钮（只上移） =====")]
	public GameObject btn_setting;

	public GameObject btn_close;

	[Header("===== 功能按钮（上移+切图） =====")]
	public GameObject btn_back;

	public Sprite back_normal;

	public Sprite back_hover;

	public GameObject btn_restart;

	public Sprite restart_normal;

	public Sprite restart_hover;

	public GameObject btn_exit;

	public Sprite exit_normal;

	public Sprite exit_hover;

	[Header("===== 按钮效果 =====")]
	public float moveY = 8f;

	public float smoothSpeed = 8f;

	private bool isOpen;

	private Vector3 settingOriginPos;

	private Vector3 closeOriginPos;

	private Vector3 backOriginPos;

	private Vector3 restartOriginPos;

	private Vector3 exitOriginPos;

	private void Awake()
	{
		if (canvasGroup == null)
		{
			canvasGroup = settingPanel.GetComponent<CanvasGroup>();
		}
		canvasGroup.alpha = 0f;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.interactable = false;
		settingOriginPos = btn_setting.transform.localPosition;
		closeOriginPos = btn_close.transform.localPosition;
		backOriginPos = btn_back.transform.localPosition;
		restartOriginPos = btn_restart.transform.localPosition;
		exitOriginPos = btn_exit.transform.localPosition;
		Canvas component = settingPanel.GetComponent<Canvas>();
		if (component != null)
		{
			component.sortingOrder = 10;
		}
	}

	private void Start()
	{
		Button component = btn_setting.GetComponent<Button>();
		Button component2 = btn_close.GetComponent<Button>();
		Button component3 = btn_back.GetComponent<Button>();
		Button component4 = btn_restart.GetComponent<Button>();
		Button component5 = btn_exit.GetComponent<Button>();
		component.interactable = true;
		component2.interactable = true;
		component3.interactable = true;
		component4.interactable = true;
		component5.interactable = true;
		component.onClick.AddListener(OpenPanel);
		component2.onClick.AddListener(ClosePanel);
		component3.onClick.AddListener(BackGame);
		component4.onClick.AddListener(RestartGame);
		component5.onClick.AddListener(ExitGame);
		AddMoveOnly(btn_setting, settingOriginPos);
		AddMoveOnly(btn_close, closeOriginPos);
		AddMoveAndChange(btn_back, back_normal, back_hover, backOriginPos);
		AddMoveAndChange(btn_restart, restart_normal, restart_hover, restartOriginPos);
		AddMoveAndChange(btn_exit, exit_normal, exit_hover, exitOriginPos);
	}

	private void AddMoveOnly(GameObject obj, Vector3 originPos)
	{
		EventTrigger eventTrigger = obj.GetComponent<EventTrigger>();
		if (eventTrigger == null)
		{
			eventTrigger = obj.AddComponent<EventTrigger>();
		}
		else
		{
			eventTrigger.triggers.Clear();
		}
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener((BaseEventData data) =>
		{
			StopAllCoroutines();
			Vector3 target = originPos;
			target.y += moveY;
			StartCoroutine(MoveY(obj.transform, target));
		});
		eventTrigger.triggers.Add(entry);
		EventTrigger.Entry entry2 = new EventTrigger.Entry();
		entry2.eventID = EventTriggerType.PointerExit;
		entry2.callback.AddListener((BaseEventData data) =>
		{
			StopAllCoroutines();
			StartCoroutine(MoveY(obj.transform, originPos));
		});
		eventTrigger.triggers.Add(entry2);
	}

	private void AddMoveAndChange(GameObject obj, Sprite normal, Sprite hover, Vector3 originPos)
	{
		EventTrigger eventTrigger = obj.GetComponent<EventTrigger>();
		if (eventTrigger == null)
		{
			eventTrigger = obj.AddComponent<EventTrigger>();
		}
		else
		{
			eventTrigger.triggers.Clear();
		}
		Image img = obj.GetComponent<Image>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = EventTriggerType.PointerEnter;
		entry.callback.AddListener((BaseEventData data) =>
		{
			if (img.sprite != hover)
			{
				img.sprite = hover;
			}
			StopAllCoroutines();
			Vector3 target = originPos;
			target.y += moveY;
			StartCoroutine(MoveY(obj.transform, target));
		});
		eventTrigger.triggers.Add(entry);
		EventTrigger.Entry entry2 = new EventTrigger.Entry();
		entry2.eventID = EventTriggerType.PointerExit;
		entry2.callback.AddListener((BaseEventData data) =>
		{
			if (img.sprite != normal)
			{
				img.sprite = normal;
			}
			StopAllCoroutines();
			StartCoroutine(MoveY(obj.transform, originPos));
		});
		eventTrigger.triggers.Add(entry2);
	}

	private IEnumerator MoveY(Transform t, Vector3 target)
	{
		while (Vector3.Distance(t.localPosition, target) > 0.01f)
		{
			t.localPosition = Vector3.Lerp(t.localPosition, target, smoothSpeed * Time.unscaledDeltaTime);
			yield return null;
		}
		t.localPosition = target;
	}

	public void OpenPanel()
	{
		if (!isOpen)
		{
			isOpen = true;
			StopAllCoroutines();
			Time.timeScale = 0f;
			canvasGroup.alpha = 1f;
			canvasGroup.blocksRaycasts = true;
			canvasGroup.interactable = true;
			btn_setting.GetComponent<Button>().interactable = false;
		}
	}

	public void ClosePanel()
	{
		if (isOpen)
		{
			isOpen = false;
			StopAllCoroutines();
			Time.timeScale = 1f;
			canvasGroup.alpha = 0f;
			canvasGroup.blocksRaycasts = false;
			canvasGroup.interactable = false;
			btn_setting.GetComponent<Button>().interactable = true;
		}
	}

	private void BackGame()
	{
		ClosePanel();
	}

	private void RestartGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void ExitGame()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene("Map_Panel1");
	}
}
