using UnityEngine;

public class ItemCollector : MonoBehaviour
{
	public string itemName;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player") || collision.name == "Mobao")
		{
			GameStatusManager.instance.CollectItem(itemName);
			Debug.Log("拿到了：" + itemName);
			Object.Destroy(base.gameObject);
		}
	}
}
