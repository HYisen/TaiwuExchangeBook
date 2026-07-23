using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
using Config;
using Config.Common;
using FrameWork;
using FrameWork.UISystem.UIElements;
using Game.Views.SettlementInformation;
using GameData.Domains.Organization.Display;
using HarmonyLib;
using TMPro;
using TaiwuModdingLib.Core.Plugin;
using UnityEngine;
using UnityEngine.UI;

namespace ExchangeBook;

[PluginConfig("ExchangeBookPlus", "p", "1.0")]
public class MainPatch : TaiwuRemakePlugin
{
	public static string ModId = string.Empty;

	private static short SettlementId;

	private static bool OnlyInSect;

	private Harmony harmony;

	private static CButton exchangeCombatSkillBookBtn;

	private static CButton exchangeLifeSkillBookBtn;

	public override void OnModSettingUpdate()
	{
		ModManager.GetSetting(((TaiwuRemakePlugin)this).ModIdStr, "OnlyInSect", ref OnlyInSect);
	}

	public override void Initialize()
	{
		ModId = ((TaiwuRemakePlugin)this).ModIdStr;
		harmony = Harmony.CreateAndPatchAll(typeof(MainPatch), (string)null);
		UIBuilder.PrepareMaterial();
	}

	public override void Dispose()
	{
		Harmony obj = harmony;
		if (obj != null)
		{
			obj.UnpatchSelf();
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ViewSettlementInformation), "OnInit")]
	public static void ViewSettlementInformation_OnInit_Postfix(ArgumentBox argsBox, ViewSettlementInformation __instance)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)exchangeCombatSkillBookBtn == (Object)null)
		{
			exchangeCombatSkillBookBtn = CreateExchangeButton(__instance, "ExchangeCombatSkillBook", "门派换书", new Vector2(300f, 16.8f), isCombatSkill: true);
		}
		if ((Object)(object)exchangeLifeSkillBookBtn == (Object)null)
		{
			exchangeLifeSkillBookBtn = CreateExchangeButton(__instance, "ExchangeLifeSkillBook", "技艺换书", new Vector2(550f, 16.8f), isCombatSkill: false);
		}
		SetExchangeButtonsVisible(__instance, visible: false);
		if (OnlyInSect)
		{
			SettlementId = 0;
			if (argsBox != null)
			{
				argsBox.Get("SettlementId", out SettlementId);
			}
			if (SettlementId != 0)
			{
				SetExchangeButtonsVisible(__instance, SettlementId);
			}
		}
	}

	private static CButton CreateExchangeButton(ViewSettlementInformation instance, string buttonName, string labelText, Vector2 anchoredPosition, bool isCombatSkill)
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		GameObject value = Traverse.Create((object)instance).Field("sectSkill").GetValue<GameObject>();
		if ((Object)(object)value == (Object)null)
		{
			return null;
		}
		GameObject val = Object.Instantiate<GameObject>(value, value.transform.parent);
		((Object)val).name = buttonName;
		TextLanguage[] componentsInChildren = val.GetComponentsInChildren<TextLanguage>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.DestroyImmediate((Object)(object)componentsInChildren[i]);
		}
		RectTransform component = val.GetComponent<RectTransform>();
		if ((Object)(object)component != (Object)null)
		{
			component.anchoredPosition = anchoredPosition;
		}
		TMP_Text[] componentsInChildren2 = val.GetComponentsInChildren<TMP_Text>(true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			componentsInChildren2[j].text = labelText;
			((Graphic)componentsInChildren2[j]).SetAllDirty();
		}
		CButton component2 = val.GetComponent<CButton>();
		if ((Object)(object)component2 != (Object)null)
		{
			component2.ClearAndAddListener((Action)delegate
			{
				OnClick(instance, isCombatSkill);
			});
		}
		val.SetActive(!OnlyInSect);
		return component2;
	}

	public static void OnClick(ViewSettlementInformation instance, bool isCombatSkill)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		int curSettlementInDisplay = Traverse.Create((object)instance).Field("_curSettlementInDisplay").GetValue<int>();
		if (curSettlementInDisplay != -1)
		{
			List<SettlementDisplayData> value = Traverse.Create((object)instance).Field("_visitedSettlements").GetValue<List<SettlementDisplayData>>();
			SettlementDisplayData val = value.First((SettlementDisplayData data) => data.SettlementId == curSettlementInDisplay);
			if (((ConfigData<OrganizationItem, sbyte>)(object)Organization.Instance)[val.OrgTemplateId].IsSect)
			{
				ArgumentBox val2 = EasyPool.Get<ArgumentBox>();
				val2.Set("OrganizationId", curSettlementInDisplay);
				val2.Set("OrganizationName", ((ConfigData<OrganizationItem, sbyte>)(object)Organization.Instance)[val.OrgTemplateId].Name);
				val2.Set("IsCombatSkill", isCombatSkill);
				UI_ExchangeBookPlus.GetUI().SetOnInitArgs(val2);
				UIManager.Instance.ShowUI(UI_ExchangeBookPlus.GetUI(), true);
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(ViewSettlementInformation), "OnClickSettlement")]
	public static void ViewSettlementInformation_OnClickSettlement_Postfix(ViewSettlementInformation __instance, int ____curSettlementInDisplay)
	{
		bool flag = IsSectSettlement(__instance, ____curSettlementInDisplay);
		if (OnlyInSect)
		{
			flag = flag && ____curSettlementInDisplay == SettlementId;
		}
		SetExchangeButtonsVisible(__instance, flag);
	}

	private static bool IsSectSettlement(ViewSettlementInformation instance, int settlementId)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (settlementId < 0)
		{
			return false;
		}
		List<SettlementDisplayData> value = Traverse.Create((object)instance).Field("_visitedSettlements").GetValue<List<SettlementDisplayData>>();
		if (value == null)
		{
			return false;
		}
		SettlementDisplayData val = value.FirstOrDefault((SettlementDisplayData data) => data.SettlementId == settlementId);
		return val.SettlementId == settlementId && ((ConfigData<OrganizationItem, sbyte>)(object)Organization.Instance)[val.OrgTemplateId].IsSect;
	}

	private static void SetExchangeButtonsVisible(ViewSettlementInformation instance, bool visible)
	{
		CButton obj = exchangeCombatSkillBookBtn;
		if (obj != null)
		{
			((Component)obj).gameObject.SetActive(visible);
		}
		CButton obj2 = exchangeLifeSkillBookBtn;
		if (obj2 != null)
		{
			((Component)obj2).gameObject.SetActive(visible);
		}
	}

	private static void SetExchangeButtonsVisible(ViewSettlementInformation instance, short settlementId)
	{
		SetExchangeButtonsVisible(instance, IsSectSettlement(instance, settlementId));
	}
}
