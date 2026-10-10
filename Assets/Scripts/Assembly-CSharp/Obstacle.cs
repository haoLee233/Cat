using UnityEngine;

public class Obstacle : MonoBehaviour
{
	private float destroyX = -100f;

	private void Update()
	{
		base.transform.Translate(Vector2.left * 130f * Time.deltaTime);
		if (base.transform.position.x < destroyX)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("PlayerCat"))
		{
			Debug.Log("撞到障碍物！游戏结束");
			GameManager.Instance.Lose();
		}
	}
}
