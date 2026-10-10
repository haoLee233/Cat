using UnityEngine;

public class BgLooper : MonoBehaviour
{
	public float scrollSpeed = 2f;

	private float width;

	private void Start()
	{
		width = GetComponent<SpriteRenderer>().bounds.size.x;
	}

	private void Update()
	{
		base.transform.Translate(Vector2.left * scrollSpeed * Time.deltaTime);
		if (base.transform.position.x < 0f - width)
		{
			base.transform.position += new Vector3(width * 2f, 0f, 0f);
		}
	}
}
