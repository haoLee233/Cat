using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CGController : MonoBehaviour
{
	[Header("目标游戏场景名")]
	public string targetSceneName = "Game_Chapter1";

	private VideoPlayer videoPlayer;

	private void Start()
	{
		videoPlayer = GetComponent<VideoPlayer>();
		videoPlayer.loopPointReached += OnVideoFinished;
	}

	private void OnVideoFinished(VideoPlayer vp)
	{
		LoadTargetScene();
	}

	public void SkipCG()
	{
		videoPlayer.Stop();
		LoadTargetScene();
	}

	private void LoadTargetScene()
	{
		SceneManager.LoadScene(targetSceneName);
	}

	private void OnDestroy()
	{
		if (videoPlayer != null)
		{
			videoPlayer.loopPointReached -= OnVideoFinished;
		}
	}
}
