using UnityEngine;
using UnityEngine.SceneManagement;

public class PuzzleManager : MonoBehaviour
{
	public int totalPieces;

	[Header("完成后的视觉反馈")]
	public GameObject fullImage;

	public GameObject piecesParent;

	public ParticleSystem winEffect;

	[Header("UI面板")]
	public GameObject knowledgeCard;

	private int finishedCount;

	public void PieceFinished()
	{
		finishedCount++;
		if (finishedCount >= totalPieces)
		{
			OnPuzzleComplete();
		}
	}

	private void OnPuzzleComplete()
	{
		Debug.Log("全部拼图完成！");
		if (fullImage != null)
		{
			fullImage.SetActive(value: true);
		}
		if (piecesParent != null)
		{
			piecesParent.SetActive(value: false);
		}
		if (knowledgeCard != null)
		{
			knowledgeCard.SetActive(value: true);
			Invoke("HideSuccessUI", 2f);
		}
		if (winEffect != null)
		{
			winEffect.Play();
		}
		Invoke("AutoReturn", 4f);
	}

	private void AutoReturn()
	{
		PlayerPrefs.SetInt("TaiHeDian_Solved", 1);
		PlayerPrefs.Save();
		SceneManager.LoadScene("Map_Panel1");
	}

	public void OnBackToMapButtonClick()
	{
		PlayerPrefs.SetInt("TaiHeDian_Solved", 1);
		PlayerPrefs.Save();
		SceneManager.LoadScene("Map_Panel1");
	}

	private void HideSuccessUI()
	{
		if (knowledgeCard != null)
		{
			knowledgeCard.SetActive(value: false);
		}
		Debug.Log("提示文字已自动隐藏");
	}
}
