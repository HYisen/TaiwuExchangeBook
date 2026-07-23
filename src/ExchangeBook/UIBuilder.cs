using System;
using Object = UnityEngine.Object;
using Game.Components.ListStyleGeneralScroll.Item;
using Game.Views.Exchange;
using UnityEngine;

namespace ExchangeBook;

public static class UIBuilder
{
	private static GameObject exchangeBookPrefab;

	public static ItemListScroll ItemListTemplate { get; private set; }

	public static void PrepareMaterial()
	{
		ResLoader.Load<GameObject>("RemakeResources/Prefab/Views/Exchange/ViewExchangeBook", (Action<GameObject>)delegate(GameObject obj)
		{
			exchangeBookPrefab = obj;
			ExchangeContainer componentInChildren = exchangeBookPrefab.GetComponentInChildren<ExchangeContainer>(true);
			if ((Object)(object)componentInChildren != (Object)null)
			{
				ItemListTemplate = componentInChildren.targetItemList;
			}
		}, (Action<string>)null, false);
	}

	public static GameObject BuildMainUI(string name)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(name);
		val.SetActive(false);
		val.layer = LayerMask.NameToLayer("UI");
		RectTransform val2 = val.AddComponent<RectTransform>();
		val2.anchorMin = Vector2.zero;
		val2.anchorMax = Vector2.one;
		val2.offsetMin = Vector2.zero;
		val2.offsetMax = Vector2.zero;
		((Transform)val2).localScale = Vector3.one;
		Canvas val3 = val.AddComponent<Canvas>();
		val3.renderMode = (RenderMode)2;
		val3.sortingLayerName = "UI";
		val.AddComponent<ConchShipGraphicRaycaster>();
		return val;
	}
}
