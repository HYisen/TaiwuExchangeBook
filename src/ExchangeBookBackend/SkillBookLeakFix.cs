using System.Collections.Generic;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Item;
using GameData.Domains.Merchant;
using HarmonyLib;

namespace ExchangeBookBackend;

internal static class SkillBookLeakFix
{
	[HarmonyPostfix]
	[HarmonyPatch(typeof(MerchantDomain), "FinishBookTrade")]
	public static void FinishBookTrade_Postfix(DataContext context, MerchantDomain __instance)
	{
		object value = Traverse.Create((object)__instance).Field("_skillBookTradeInfo").GetValue();
		if (value == null)
		{
			CleanupSkillBooks(context, removeLibraryOwned: true, removeUnowned: true);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ItemDomain), "CheckUnownedItems")]
	public static void CheckUnownedItems_Prefix()
	{
		DataContext currentThreadDataContext = DataContextManager.GetCurrentThreadDataContext();
		CleanupSkillBooks(currentThreadDataContext, removeLibraryOwned: false, removeUnowned: true);
	}

	private static void CleanupSkillBooks(DataContext context, bool removeLibraryOwned, bool removeUnowned)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Invalid comparison between Unknown and I4
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (context == null || (!removeLibraryOwned && !removeUnowned))
		{
			return;
		}
		Dictionary<int, SkillBook> value = Traverse.Create((object)DomainManager.Item).Field("_skillBooks").GetValue<Dictionary<int, SkillBook>>();
		if (value == null || value.Count == 0)
		{
			return;
		}
		List<ItemKey> list = new List<ItemKey>();
		foreach (KeyValuePair<int, SkillBook> item in value)
		{
			SkillBook value2 = item.Value;
			if (!ItemDomain.IsPureStackable((ItemBase)(object)value2))
			{
				ItemOwnerType ownerType = ((ItemBase)value2).Owner.OwnerType;
				if ((removeUnowned && (int)ownerType == 0) || (removeLibraryOwned && (int)ownerType == 17))
				{
					list.Add(((ItemBase)value2).GetItemKey());
				}
			}
		}
		if (list.Count != 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				DomainManager.Item.RemoveItem(context, list[i]);
			}
			ModMain.LogInfo($"[ExchangeBookPlusBackend] Removed {list.Count} orphan SkillBook(s).");
		}
	}
}
