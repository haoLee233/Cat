using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoEndRedirect : MonoBehaviour
{
	private VideoPlayer vp;

	[Header("设置跳转目标")]
	[Tooltip("在这里填入视频结束或跳过后要进入的场景名字")]
	public string nextSceneName;

	private void Start()
	{
		vp = GetComponent<VideoPlayer>();
		if (vp != null)
		{
			vp.loopPointReached += OnVideoFinished;
		}
	}

	private void OnVideoFinished(VideoPlayer source)
	{
		JumpToNextScene();
	}

	public void SkipVideo()
	{
		Debug.Log("玩家点击跳过...");
		JumpToNextScene();
	}

	private void JumpToNextScene()
	{
		if (!string.IsNullOrEmpty(nextSceneName))
		{
			SceneManager.LoadScene(nextSceneName);
		}
		else
		{
			Debug.LogError(base.gameObject.name + " 脚本里没填场景名字！");
		}
	}
}
