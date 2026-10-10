using UnityEngine;

public class CollectibleCoin : MonoBehaviour
{
	[Header("金币配置")]
	public int scoreValue = 50;

	public float scrollSpeed = 80f;

	private float destroyX = -120f;

	private void Update()
	{
		if (!(GameManager.Instance != null) || !GameManager.Instance.gameOver)
		{
			base.transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);
			if (base.transform.position.x < destroyX)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("PlayerCat") && GameManager.Instance != null && !GameManager.Instance.gameOver)
		{
			GameManager.Instance.AddScore(scoreValue);
			Object.Destroy(base.gameObject);
		}
	}
}
