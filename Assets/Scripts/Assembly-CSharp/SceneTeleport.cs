using System.Collections;
using TMPro;
using UnityEngine;

public class SceneTeleport : MonoBehaviour
{
	[Header("传送位置配置")]
	public Vector3 targetMobaoPos;

	public Vector3 targetCameraPos;

	[Header("场景文字提示配置")]
	[TextArea]
	public string sceneTipText = "太和殿主殿";

	public GameObject sceneTipUI;

	public float tipShowTime = 2f;

	public float fadeTime = 0.5f;

	private TextMeshProUGUI tipText;

	private CanvasGroup tipCanvasGroup;

	private static float lastTeleportTime;

	private const float cooldown = 0.5f;

	private void Awake()
	{
		if (sceneTipUI == null)
		{
			Debug.LogError("【" + base.gameObject.name + "】未配置SceneTipUI！请在Inspector中拖入SceneTipUI对象");
			return;
		}
		tipText = sceneTipUI.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		if (tipText == null)
		{
			Debug.LogError("【" + base.gameObject.name + "】SceneTipUI下未找到TextMeshProUGUI组件！请检查UI层级");
			return;
		}
		tipCanvasGroup = sceneTipUI.GetComponent<CanvasGroup>();
		if (tipCanvasGroup == null)
		{
			tipCanvasGroup = sceneTipUI.AddComponent<CanvasGroup>();
		}
		sceneTipUI.SetActive(value: false);
		tipCanvasGroup.alpha = 0f;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (Time.time < lastTeleportTime + 0.5f)
		{
			return;
		}
		lastTeleportTime = Time.time;
		if (collision.CompareTag("Player") || collision.name == "Mobao")
		{
			collision.transform.position = targetMobaoPos;
			Camera.main.transform.position = new Vector3(targetCameraPos.x, targetCameraPos.y, -10f);
			Debug.Log(base.gameObject.name + " 传送成功！");
			if (tipText != null && sceneTipUI != null)
			{
				ShowSceneTip();
			}
		}
	}

	private void ShowSceneTip()
	{
		StopAllCoroutines();
		tipText.text = sceneTipText;
		sceneTipUI.SetActive(value: true);
		StartCoroutine(FadeTipRoutine());
	}

	private IEnumerator FadeTipRoutine()
	{
		float t = 0f;
		while (t < fadeTime)
		{
			t += Time.deltaTime;
			tipCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeTime);
			yield return null;
		}
		tipCanvasGroup.alpha = 1f;
		yield return new WaitForSeconds(tipShowTime);
		t = 0f;
		while (t < fadeTime)
		{
			t += Time.deltaTime;
			tipCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
			yield return null;
		}
		tipCanvasGroup.alpha = 0f;
		sceneTipUI.SetActive(value: false);
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}
}
