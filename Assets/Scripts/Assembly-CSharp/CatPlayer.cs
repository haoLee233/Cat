using UnityEngine;

public class CatPlayer : MonoBehaviour
{
	[Header("基础移动")]
	public float autoRunSpeed = 4f;

	public float jumpForce = 700f;

	public float gravityScale = 60f;

	public KeyCode crouchKey = KeyCode.LeftControl;

	[Header("二段跳设置")]
	public int jumpCountMax = 2;

	public float doubleJumpForce = 650f;

	[Header("跳跃水平减速")]
	public float jumpSpeedMultiplier = 0.5f;

	[Header("趴下持续时间")]
	public float crouchDuration = 1.2f;

	[Header("收集后滑回位置")]
	public float returnSpeed = 5f;

	public float startXPosition = -100f;

	private Rigidbody2D rb;

	private Animator anim;

	private BoxCollider2D col;

	private bool isGrounded = true;

	private bool isCrouching;

	private float originalColliderHeight;

	private float crouchTimer;

	private bool needReturn;

	private int jumpCount;

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();
		col = GetComponent<BoxCollider2D>();
		if (rb != null)
		{
			rb.bodyType = RigidbodyType2D.Dynamic;
			rb.gravityScale = gravityScale;
			rb.freezeRotation = true;
		}
		if (col != null)
		{
			originalColliderHeight = col.size.y;
		}
		jumpCount = 0;
	}

	private void Update()
	{
		if (rb.gravityScale != gravityScale)
		{
			rb.gravityScale = gravityScale;
		}
		if (needReturn)
		{
			SmoothReturnToStart();
			PlayerJumpInReturnState();
			UpdateAnimation();
		}
		else
		{
			PlayerJump();
			PlayerCrouch();
			UpdateAnimation();
		}
	}

	private void FixedUpdate()
	{
		if (!needReturn)
		{
			AutoRun();
		}
	}

	private void AutoRun()
	{
		float num = autoRunSpeed;
		if (!isGrounded)
		{
			num *= jumpSpeedMultiplier;
		}
		base.transform.Translate(Vector2.right * num * Time.fixedDeltaTime);
	}

	private void PlayerJump()
	{
		if (Input.GetKeyDown(KeyCode.Space) && !isCrouching)
		{
			if (isGrounded)
			{
				jumpCount = 1;
				rb.velocity = new Vector2(rb.velocity.x, 0f);
				rb.velocity = new Vector2(rb.velocity.x, jumpForce);
				isGrounded = false;
			}
			else if (jumpCount < jumpCountMax)
			{
				jumpCount++;
				rb.velocity = new Vector2(rb.velocity.x, 0f);
				rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
			}
		}
	}

	private void PlayerJumpInReturnState()
	{
		if (Input.GetKeyDown(KeyCode.Space) && !isCrouching)
		{
			if (isGrounded)
			{
				jumpCount = 1;
				rb.velocity = new Vector2(rb.velocity.x, 0f);
				rb.velocity = new Vector2(rb.velocity.x, jumpForce);
				isGrounded = false;
			}
			else if (jumpCount < jumpCountMax)
			{
				jumpCount++;
				rb.velocity = new Vector2(rb.velocity.x, 0f);
				rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
			}
		}
	}

	private void PlayerCrouch()
	{
		if (!isGrounded)
		{
			isCrouching = false;
			ResetCollider();
			crouchTimer = 0f;
			return;
		}
		if (Input.GetKeyDown(crouchKey) && !isCrouching)
		{
			isCrouching = true;
			crouchTimer = crouchDuration;
			col.size = new Vector2(col.size.x, originalColliderHeight / 2f);
		}
		if (crouchTimer > 0f)
		{
			crouchTimer -= Time.deltaTime;
		}
		else if (isCrouching)
		{
			isCrouching = false;
			ResetCollider();
		}
	}

	private void ResetCollider()
	{
		if (col != null)
		{
			col.size = new Vector2(col.size.x, originalColliderHeight);
		}
	}

	private void UpdateAnimation()
	{
		if (anim != null)
		{
			anim.SetBool("IsJumping", !isGrounded);
			anim.SetBool("IsCrouching", isCrouching);
		}
	}

	private void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.CompareTag("Ground"))
		{
			isGrounded = true;
			jumpCount = 0;
		}
	}

	private void OnCollisionExit2D(Collision2D other)
	{
		if (other.gameObject.CompareTag("Ground"))
		{
			isGrounded = false;
		}
	}

	public void StartReturnToStart()
	{
		needReturn = true;
		isCrouching = false;
		ResetCollider();
	}

	private void SmoothReturnToStart()
	{
		base.transform.position = Vector2.MoveTowards(base.transform.position, new Vector2(startXPosition, base.transform.position.y), returnSpeed * Time.deltaTime);
		if (Mathf.Abs(base.transform.position.x - startXPosition) < 1f)
		{
			needReturn = false;
		}
	}
}
