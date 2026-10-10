using System;
using UnityEngine;
using UnityEngine.UI;

public class CollectionDisplay : MonoBehaviour
{
	[Serializable]
	public struct CollectionItem
	{
		public string levelKey;

		public Image[] icons;
	}

	[Header("关卡与文物映射表")]
	public CollectionItem[] levelMappings;

	private void OnEnable()
	{
		RefreshCabinet();
	}

	public void RefreshCabinet()
	{
		CollectionItem[] array = levelMappings;
		for (int i = 0; i < array.Length; i++)
		{
			CollectionItem collectionItem = array[i];
			bool flag = PlayerPrefs.GetInt(collectionItem.levelKey, 0) == 1;
			Image[] icons = collectionItem.icons;
			foreach (Image image in icons)
			{
				if (image != null)
				{
					if (flag)
					{
						image.color = Color.white;
					}
					else
					{
						image.color = new Color(0f, 0f, 0f, 0.6f);
					}
				}
			}
		}
	}
}
