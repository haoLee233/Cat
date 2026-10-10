using UnityEngine;

public class MobaoMovement : MonoBehaviour
{
	[Header("移动设置")]
	public float walkSpeed = 5f;

	public float runSpeed = 10f;

	public float jumpForce = 7f;

	[Header("双击设置")]
	public float doubleTapThreshold = 0.3f;

	private float lastTapTimeA = -10f;

	private float lastTapTimeD = -10f;

	[Header("待机设置")]
	public float afkThreshold = 10f;

	private float idleTimer;

	private Rigidbody2D rb;

	private SpriteRenderer spriteRenderer;

	private Animator anim;

	private bool isRunning;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		anim = GetComponent<Animator>();
	}

	private void Update()
	{
		float axisRaw = Input.GetAxisRaw("Horizontal");
		DetectDoubleTap();
		bool flag = Mathf.Abs(axisRaw) > 0.01f;
		if (flag)
		{
			idleTimer = 0f;
			anim.SetBool("isAFK", value: false);
		}
		else
		{
			idleTimer += Time.deltaTime;
			if (idleTimer >= afkThreshold)
			{
				anim.SetBool("isAFK", value: true);
			}
		}
		float num = (isRunning ? runSpeed : walkSpeed);
		if (!flag)
		{
			isRunning = false;
		}
		rb.velocity = new Vector2(axisRaw * num, rb.velocity.y);
		if (axisRaw > 0f)
		{
			spriteRenderer.flipX = false;
		}
		else if (axisRaw < 0f)
		{
			spriteRenderer.flipX = true;
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			rb.velocity = new Vector2(rb.velocity.x, jumpForce);
			idleTimer = 0f;
			anim.SetBool("isAFK", value: false);
		}
		anim.SetBool("isWalking", flag);
		anim.SetBool("isRunning", isRunning & flag);
	}

	private void DetectDoubleTap()
	{
		if (Input.GetKeyDown(KeyCode.D))
		{
			if (Time.time - lastTapTimeD < doubleTapThreshold)
			{
				isRunning = true;
			}
			lastTapTimeD = Time.time;
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			if (Time.time - lastTapTimeA < doubleTapThreshold)
			{
				isRunning = true;
			}
			lastTapTimeA = Time.time;
		}
	}
}
