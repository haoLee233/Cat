using UnityEngine;

public class JiShouSpawner : MonoBehaviour
{
	[Header("依次生成3个不同脊兽")]
	public GameObject[] jiShouPrefabs;

	[Header("生成设置")]
	public float spawnInterval = 6f;

	public float spawnX = 200f;

	public float spawnY = 30f;

	private float nextSpawnTime;

	private int currentIndex;

	private void Start()
	{
		if (jiShouPrefabs.Length < 3)
		{
			Debug.LogError("脊兽预制体数组长度不足3个！");
			base.enabled = false;
		}
		else
		{
			nextSpawnTime = Time.time + spawnInterval;
		}
	}

	private void Update()
	{
		if ((!(GameManager.Instance != null) || !GameManager.Instance.gameOver) && currentIndex < 3 && Time.time >= nextSpawnTime)
		{
			SpawnNextBeast();
			nextSpawnTime = Time.time + spawnInterval;
		}
	}

	private void SpawnNextBeast()
	{
		if (currentIndex >= jiShouPrefabs.Length || jiShouPrefabs[currentIndex] == null)
		{
			Debug.LogWarning($"第{currentIndex + 1}个脊兽预制体未赋值！");
			currentIndex++;
			return;
		}
		Collectible component = Object.Instantiate(jiShouPrefabs[currentIndex], new Vector2(spawnX, spawnY), Quaternion.identity).GetComponent<Collectible>();
		if (component != null)
		{
			component.scrollSpeed = 64f;
		}
		currentIndex++;
	}
}
