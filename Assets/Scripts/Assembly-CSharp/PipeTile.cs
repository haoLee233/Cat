using UnityEngine;
using UnityEngine.UI;

public class PipeTile : MonoBehaviour
{
	[Header("这张图的正确角度（0/90/180/270）")]
	public int correctAngle;

	private Button tileButton;

	private int currentAngle;

	private void Awake()
	{
		tileButton = GetComponent<Button>();
		tileButton.onClick.AddListener(OnTileClick);
	}

	private void OnTileClick()
	{
		currentAngle = (currentAngle + 90) % 360;
		base.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
		PipeGameManager.Instance.CheckWin();
	}

	public bool IsCorrect()
	{
		return currentAngle == correctAngle;
	}

	public void RandomizeAngle()
	{
		currentAngle = Random.Range(0, 4) * 90;
		base.transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
	}
}
