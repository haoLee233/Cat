using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
	[Header("拼图设置")]
	public Vector3 correctPosition;

	public float snapDistance = 0.5f;

	private bool isFinished;

	private Vector3 offset;

	private int originalSortingOrder;

	private PuzzleManager manager;

	private void Start()
	{
		manager = Object.FindObjectOfType<PuzzleManager>();
		correctPosition = base.transform.position;
		int[] array = new int[4] { 0, 90, 180, 270 };
		float z = array[Random.Range(0, array.Length)];
		base.transform.rotation = Quaternion.Euler(0f, 0f, z);
		base.transform.position = new Vector3(Random.Range(-7f, 7f), Random.Range(-4f, 4f), 0f);
	}

	private void Update()
	{
		if (!isFinished && Input.GetMouseButtonDown(1))
		{
			Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			if (GetComponent<Collider2D>().OverlapPoint(vector))
			{
				base.transform.Rotate(0f, 0f, 90f);
			}
		}
	}

	private void OnMouseDown()
	{
		if (!isFinished)
		{
			originalSortingOrder = GetComponent<SpriteRenderer>().sortingOrder;
			GetComponent<SpriteRenderer>().sortingOrder = 100;
			Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			offset = base.transform.position - vector;
		}
	}

	private void OnMouseDrag()
	{
		if (!isFinished)
		{
			Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			base.transform.position = new Vector3(vector.x + offset.x, vector.y + offset.y, 0f);
		}
	}

	private void OnMouseUp()
	{
		if (!isFinished)
		{
			GetComponent<SpriteRenderer>().sortingOrder = originalSortingOrder;
			CheckPosition();
		}
	}

	private void CheckPosition()
	{
		float num = Vector3.Distance(base.transform.position, correctPosition);
		float num2 = Mathf.Abs(base.transform.eulerAngles.z);
		bool flag = num2 < 5f || num2 > 355f;
		if ((num < snapDistance) & flag)
		{
			base.transform.position = correctPosition;
			base.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
			isFinished = true;
			if (manager != null)
			{
				manager.PieceFinished();
			}
			Debug.Log("归位成功！");
		}
	}
}
