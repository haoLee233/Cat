using System.Collections;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
	public GameObject coinPrefab;

	public float minSpawnTime = 0.8f;

	public float maxSpawnTime = 2.5f;

	private void Start()
	{
		StartCoroutine(SpawnLoop());
	}

	private IEnumerator SpawnLoop()
	{
		while (true)
		{
			float time = Random.Range(minSpawnTime, maxSpawnTime);
			yield return new WaitForSecondsRealtime(time);
			if (GameManager.Instance != null && GameManager.Instance.gameOver)
			{
				break;
			}
			Object.Instantiate(position: new Vector2(220f, -70f), original: coinPrefab, rotation: Quaternion.identity);
		}
	}
}
