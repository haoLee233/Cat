using UnityEngine;

public class Collectible : MonoBehaviour
{
	[Header("基础移动")]
	public float scrollSpeed = 5f;

	public int scoreValue = 10;

	[Header("上下浮动设置")]
	public float floatAmplitude = 5f;

	public float floatSpeed = 2f;

	[Header("倾斜设置")]
	public float tiltAngle = -18f;

	private Rigidbody2D rb;

	private Vector2 startPos;

	private float floatOffset;

	private bool isCollected;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		if (rb == null)
		{
			rb = base.gameObject.AddComponent<Rigidbody2D>();
		}
		rb.bodyType = RigidbodyType2D.Kinematic;
		rb.gravityScale = 0f;
		rb.freezeRotation = false;
		base.transform.rotation = Quaternion.Euler(0f, 0f, tiltAngle);
		startPos = base.transform.position;
	}

	private void FixedUpdate()
	{
		if (GameManager.Instance != null && GameManager.Instance.gameOver)
		{
			rb.velocity = Vector2.zero;
			return;
		}
		rb.velocity = Vector2.left * scrollSpeed;
		floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
		base.transform.position = new Vector2(base.transform.position.x, startPos.y + floatOffset);
		base.transform.rotation = Quaternion.Euler(0f, 0f, tiltAngle);
		if (base.transform.position.x < -200f && !isCollected)
		{
			if (GameManager.Instance != null && !GameManager.Instance.gameOver)
			{
				GameManager.Instance.Lose();
			}
			Object.Destroy(base.gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("PlayerCat") && !isCollected)
		{
			isCollected = true;
			if (GameManager.Instance != null)
			{
				GameManager.Instance.AddScore(scoreValue);
				GameManager.Instance.CollectBeast();
			}
			GameUI.Instance.AddCollect();
			CatPlayer component = other.GetComponent<CatPlayer>();
			if (component != null)
			{
				component.StartReturnToStart();
			}
			Object.Destroy(base.gameObject);
		}
	}
}
