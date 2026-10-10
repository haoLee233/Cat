using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsLogic : MonoBehaviour
{
	public GameObject settingsPanel;

	public void OpenPanel()
	{
		settingsPanel.SetActive(value: true);
		Time.timeScale = 0f;
	}

	public void ClosePanel()
	{
		settingsPanel.SetActive(value: false);
		Time.timeScale = 1f;
	}

	public void RestartGame()
	{
		Time.timeScale = 1f;
		string text = SceneManager.GetActiveScene().name;
		if (text == "Map_Panel1")
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			Debug.Log("全游戏进度已重置");
		}
		SceneManager.LoadScene(text);
	}

	public void ExitOrReturn()
	{
		Time.timeScale = 1f;
		if (SceneManager.GetActiveScene().name == "Map_Panel1")
		{
			Debug.Log("从地图退出：关闭程序");
			Application.Quit();
		}
		else
		{
			Debug.Log("从关卡退出：回到主地图");
			SceneManager.LoadScene("Map_Panel1");
		}
	}
}
