using UnityEngine;
using UnityEngine.UI;

public class GameStatusManager : MonoBehaviour
{
	public static GameStatusManager instance;

	[Header("关卡设置")]
	public int itemsNeeded = 2;

	public int currentItems;

	[Header("UI图标列表")]
	public Image[] collectIcons;

	private void Awake()
	{
		instance = this;
	}

	public void CollectItem(string name)
	{
		currentItems++;
		if (currentItems <= collectIcons.Length)
		{
			collectIcons[currentItems - 1].color = Color.white;
		}
		Debug.Log("收集进度：" + currentItems + "/" + itemsNeeded);
	}

	public bool CanEnterPuzzle()
	{
		return currentItems >= itemsNeeded;
	}
}
