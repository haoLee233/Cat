using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
	public GameObject lionPrefab;

	public GameObject lanternPrefab;

	public float spawnRangeX = 200f;

	public float minSpawnInterval = 3f;

	public float maxSpawnInterval = 6f;

	public float lionY = -20f;

	public float lanternY = 10f;

	private float nextSpawnTime;

	private void Start()
	{
		nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
		if (lionPrefab == null)
		{
			Debug.LogError("狮子预制体未赋值！");
		}
		if (lanternPrefab == null)
		{
			Debug.LogError("宫灯预制体未赋值！");
		}
	}

	private void Update()
	{
		if (Time.timeScale != 0f && (!(lionPrefab == null) || !(lanternPrefab == null)) && Time.time >= nextSpawnTime)
		{
			SpawnRandomObstacle();
			nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
		}
	}

	private void SpawnRandomObstacle()
	{
		GameObject gameObject = null;
		Vector2 vector = Vector2.zero;
		if (Random.Range(0, 2) == 0 && lionPrefab != null)
		{
			gameObject = lionPrefab;
			vector = new Vector2(spawnRangeX, lionY);
		}
		else if (lanternPrefab != null)
		{
			gameObject = lanternPrefab;
			vector = new Vector2(spawnRangeX, lanternY);
		}
		if (gameObject != null)
		{
			Object.Instantiate(gameObject, vector, Quaternion.identity);
		}
	}
}
