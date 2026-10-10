using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
	[Header("彩色建筑图片对象")]
	public GameObject colorTaiHeDian;

	public GameObject colorWuMen;

	public GameObject colorBaoHeDian;

	public GameObject colorQianQingGong;

	[Header("大结局设置")]
	public GameObject btnRitual;

	private void Start()
	{
		UpdateMapVisuals();
	}

	public void UpdateMapVisuals()
	{
		int num = 0;
		if (PlayerPrefs.GetInt("WuMen_Solved", 0) == 1)
		{
			if (colorWuMen != null)
			{
				colorWuMen.SetActive(value: true);
			}
			num++;
		}
		if (PlayerPrefs.GetInt("TaiHeDian_Solved", 0) == 1)
		{
			if (colorTaiHeDian != null)
			{
				colorTaiHeDian.SetActive(value: true);
			}
			num++;
		}
		if (PlayerPrefs.GetInt("BaoHeDian_Solved", 0) == 1)
		{
			if (colorBaoHeDian != null)
			{
				colorBaoHeDian.SetActive(value: true);
			}
			num++;
		}
		if (PlayerPrefs.GetInt("QianQingGong_Solved", 0) == 1)
		{
			if (colorQianQingGong != null)
			{
				colorQianQingGong.SetActive(value: true);
			}
			num++;
		}
		if (num >= 4 && btnRitual != null)
		{
			btnRitual.SetActive(value: true);
			Debug.Log("全成就达成！唤醒紫禁城！");
		}
	}

	public void LoadTargetScene(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}

	public void EnterMap()
	{
		SceneManager.LoadScene("Map_Panel1");
	}

	[ContextMenu("Reset Progress")]
	public void ResetProgress()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
		Debug.Log("进度已成功重置！");
		if (Application.isPlaying)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
		else
		{
			Debug.Log("在编辑模式下重置成功，请点击【播放】查看黑白地图效果。");
		}
	}

	public void PublicResetGame()
	{
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
		Debug.Log("游戏进度已重置");
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}
}
