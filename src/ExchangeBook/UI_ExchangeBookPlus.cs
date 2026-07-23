using System;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;
using Config;
using Config.Common;
using FrameWork;
using FrameWork.UISystem.UIElements;
using Game.Components.ListStyleGeneralScroll;
using Game.Components.ListStyleGeneralScroll.Item;
using Game.Components.SortAndFilter;
using Game.Components.SortAndFilter.Item.Apply;
using GameData.Common;
using GameData.Domains.Character;
using GameData.Domains.Character.Display;
using GameData.Domains.Character.Relation;
using GameData.Domains.Item;
using GameData.Domains.Item.Display;
using GameData.Domains.Merchant;
using GameData.Domains.Organization;
using GameData.Domains.Taiwu;
using GameData.GameDataBridge;
using GameData.Serializer;
using GameData.Utilities;
using GameDataExtensions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace ExchangeBook;

public class UI_ExchangeBookPlus : UIBase
{
	private const float RowHeight = 42f;

	private const int BookPageSize = 80;

	private const int BookBodyPageCount = 5;

	private const int BookBodyPageOffset = 1;

	private const float PanelWidth = 2200f;

	private const float PanelHeight = 1240f;

	private const float ScreenPadding = 40f;

	private const float MinPanelScale = 0.65f;

	private static UIElement element;

	private static TMP_FontAsset cachedFont;

	private int organizationId;

	private int taiwuId;

	private string organizationName;

	private bool isCombatSkill;

	private bool npcLoadingDone;

	private int loadedNpcCount;

	private int pendingPageCount;

	private int loadedPageCount;

	private int activeTradeCharId = -1;

	private bool isExchanging;

	private int bookPageIndex;

	private int selectedGrade;

	private int selectedSkillType;

	private int firstPageFilter;

	private int totalAuthority;

	private sbyte approveHighestGrade;

	private List<List<ItemDisplayData>> pendingExchangeGroups;

	private int pendingExchangeGroupIndex;

	private int pendingSelfAuthority;

	private readonly List<CharacterDisplayData> npcDatas = new List<CharacterDisplayData>();

	private readonly Dictionary<int, int> authorities = new Dictionary<int, int>();

	private readonly Dictionary<int, (sbyte[], sbyte[])> pagesDatas = new Dictionary<int, (sbyte[], sbyte[])>();

	private readonly Dictionary<int, List<ItemDisplayData>> npcItems = new Dictionary<int, List<ItemDisplayData>>();

	private readonly List<ItemDisplayData> currItems = new List<ItemDisplayData>();

	private readonly List<ItemDisplayData> exchangeList = new List<ItemDisplayData>();

	private readonly HashSet<int> selectedItemIds = new HashSet<int>();

	private readonly List<short> learnedBooks = new List<short>();

	private (int, int) taiwuLoads;

	private RectTransform bookContent;

	private RectTransform exchangeContent;

	private ItemListScroll bookItemScroll;

	private ItemListScroll exchangeItemScroll;

	private readonly List<ITradeableContent> visibleBookItems = new List<ITradeableContent>();

	private readonly List<ITradeableContent> visibleExchangeItems = new List<ITradeableContent>();

	private RectTransform panelRect;

	private Vector2 lastAvailableSize;

	private TMP_Text titleText;

	private TMP_Text statusText;

	private TMP_Text summaryText;

	private TMP_Text bookPageText;

	private CButton previousBookPageButton;

	private CButton nextBookPageButton;

	private CButton confirmButton;

	private readonly List<CToggle> skillTypeToggles = new List<CToggle>();

	private readonly List<CToggle> gradeToggles = new List<CToggle>();

	private readonly List<CToggle> firstPageToggles = new List<CToggle>();

	private readonly List<CToggle> directPageToggles = new List<CToggle>();

	private readonly List<CToggle> reversePageToggles = new List<CToggle>();

	private readonly List<CToggle> completePageToggles = new List<CToggle>();

	private readonly HashSet<int> directPageFilters = new HashSet<int>();

	private readonly HashSet<int> reversePageFilters = new HashSet<int>();

	private readonly HashSet<int> completePageFilters = new HashSet<int>();

	private bool suppressFilterToggleCallback;

	public static UIElement GetUI()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (element != null && (Object)(object)element.UiBase != (Object)null)
		{
			return element;
		}
		element = new UIElement();
		Traverse.Create((object)element).Field("_path").SetValue((object)"UI_ExchangeBookPlus");
		GameObject val = UIBuilder.BuildMainUI("UI_ExchangeBookPlus");
		UI_ExchangeBookPlus uI_ExchangeBookPlus = val.AddComponent<UI_ExchangeBookPlus>();
		((UIBase)uI_ExchangeBookPlus).UiType = (UILayer)3;
		((UIBase)uI_ExchangeBookPlus).Element = element;
		((UIBase)uI_ExchangeBookPlus).RelativeAtlases = Array.Empty<SpriteAtlas>();
		uI_ExchangeBookPlus.NeedDataListenerId = true;
		uI_ExchangeBookPlus.Init(val);
		element.UiBase = (UIBase)(object)uI_ExchangeBookPlus;
		((Object)element.UiBase).name = element.Name;
		UIManager.Instance.PlaceUI(element.UiBase);
		return element;
	}

	private void Init(GameObject obj)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_054c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = CreateImage("Mask", obj.transform, new Color(0f, 0f, 0f, 0.65f));
		SetStretch(val.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		GameObject val2 = CreateImage("MainWindow", obj.transform, new Color(0.12f, 0.1f, 0.08f, 0.96f));
		panelRect = val2.GetComponent<RectTransform>();
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(2200f, 1240f);
		panelRect.anchoredPosition = Vector2.zero;
		titleText = CreateText(val2.transform, "Title", "门派藏书", 40, (TextAlignmentOptions)514);
		RectTransform component = ((Component)titleText).GetComponent<RectTransform>();
		SetTop(component, 30f, 0f, 1940f, 70f);
		CButton val3 = CreateButton(val2.transform, "Close", "关闭", 160f, 54f);
		RectTransform component2 = ((Component)val3).GetComponent<RectTransform>();
		SetTopRight(component2, 30f, 30f, 160f, 54f);
		val3.ClearAndAddListener((Action)((UIBase)this).QuickHide);
		statusText = CreateText(val2.transform, "Status", "正在读取...", 24, (TextAlignmentOptions)513);
		RectTransform component3 = ((Component)statusText).GetComponent<RectTransform>();
		SetTop(component3, 35f, 105f, 2130f, 40f);
		summaryText = CreateText(val2.transform, "Summary", "", 24, (TextAlignmentOptions)513);
		RectTransform component4 = ((Component)summaryText).GetComponent<RectTransform>();
		SetBottom(component4, 35f, 30f, 1750f, 54f);
		confirmButton = CreateButton(val2.transform, "Confirm", "确认换书", 180f, 54f);
		RectTransform component5 = ((Component)confirmButton).GetComponent<RectTransform>();
		SetBottomRight(component5, 230f, 30f, 180f, 54f);
		confirmButton.ClearAndAddListener((Action)ChangeBook);
		CButton val4 = CreateButton(val2.transform, "Reset", "清空", 160f, 54f);
		RectTransform component6 = ((Component)val4).GetComponent<RectTransform>();
		SetBottomRight(component6, 35f, 30f, 160f, 54f);
		val4.ClearAndAddListener((Action)ResetPage);
		TMP_Text val5 = CreateText(val2.transform, "BookHeader", "可换藏书", 28, (TextAlignmentOptions)513);
		SetTop(((Component)val5).GetComponent<RectTransform>(), 45f, 335f, 400f, 40f);
		previousBookPageButton = CreateButton(val2.transform, "PreviousBookPage", "上一页", 110f, 40f);
		SetTop(((Component)previousBookPageButton).GetComponent<RectTransform>(), 720f, 331f, 110f, 40f);
		previousBookPageButton.ClearAndAddListener((Action)PreviousBookPage);
		bookPageText = CreateText(val2.transform, "BookPage", "", 22, (TextAlignmentOptions)514);
		SetTop(((Component)bookPageText).GetComponent<RectTransform>(), 840f, 335f, 110f, 40f);
		nextBookPageButton = CreateButton(val2.transform, "NextBookPage", "下一页", 110f, 40f);
		SetTop(((Component)nextBookPageButton).GetComponent<RectTransform>(), 960f, 331f, 110f, 40f);
		nextBookPageButton.ClearAndAddListener((Action)NextBookPage);
		TMP_Text val6 = CreateText(val2.transform, "ExchangeHeader", "已选择", 28, (TextAlignmentOptions)513);
		SetTop(((Component)val6).GetComponent<RectTransform>(), 1130f, 335f, 400f, 40f);
		CreateFilterControls(val2.transform);
		bookItemScroll = CreateOfficialItemScroll(val2.transform, "BookScroll", new Vector2(40f, 377f), new Vector2(1040f, 693f), "exchange_book_plus_books", OnRenderBookItem, OnClickBookItem);
		if ((Object)(object)bookItemScroll == (Object)null)
		{
			bookContent = CreateScroll(val2.transform, "BookScroll", new Vector2(40f, 377f), new Vector2(1040f, 693f));
		}
		exchangeItemScroll = CreateOfficialItemScroll(val2.transform, "ExchangeScroll", new Vector2(1120f, 377f), new Vector2(1040f, 693f), "exchange_book_plus_selected", OnRenderExchangeItem, OnClickExchangeItem);
		if ((Object)(object)exchangeItemScroll == (Object)null)
		{
			exchangeContent = CreateScroll(val2.transform, "ExchangeScroll", new Vector2(1120f, 377f), new Vector2(1040f, 693f));
		}
		bool flag = (Object)(object)bookItemScroll != (Object)null && (Object)(object)exchangeItemScroll != (Object)null;
		((Component)previousBookPageButton).gameObject.SetActive(!flag);
		((Component)nextBookPageButton).gameObject.SetActive(!flag);
		UpdateResolutionLayout(force: true);
		obj.SetActive(false);
	}

	private void CreateFilterControls(Transform parent)
	{
		TMP_Text val = CreateText(parent, "TypeTitle", "类型", 22, (TextAlignmentOptions)513);
		SetTop(((Component)val).GetComponent<RectTransform>(), 45f, 158f, 70f, 36f);
		for (int i = 0; i < 16; i++)
		{
			int capturedIndex = i;
			CToggle item = CreateFilterToggle(parent, "SkillType" + i, "", 100f + (float)i * 82f, 154f, 78f, delegate(bool isOn)
			{
				if (isOn && !suppressFilterToggleCallback)
				{
					selectedSkillType = capturedIndex;
					OnFilterChanged();
				}
			});
			skillTypeToggles.Add(item);
		}
		TMP_Text val2 = CreateText(parent, "GradeTitle", "品阶", 22, (TextAlignmentOptions)513);
		SetTop(((Component)val2).GetComponent<RectTransform>(), 45f, 205f, 70f, 36f);
		CreateFilterToggleGroup(parent, gradeToggles, new string[9] { "9品", "8品", "7品", "6品", "5品", "4品", "3品", "2品", "1品" }, 100f, 202f, 70f, delegate(int index)
		{
			selectedGrade = index;
			OnFilterChanged();
		});
		TMP_Text val3 = CreateText(parent, "FirstPageTitle", "首页", 22, (TextAlignmentOptions)513);
		SetTop(((Component)val3).GetComponent<RectTransform>(), 800f, 205f, 60f, 36f);
		CreateFilterToggleGroup(parent, firstPageToggles, new string[6] { "全部", "承", "合", "解", "异", "独" }, 850f, 202f, 56f, delegate(int index)
		{
			firstPageFilter = index - 1;
			OnFilterChanged();
		});
		TMP_Text val4 = CreateText(parent, "DirectTitle", "正页", 22, (TextAlignmentOptions)513);
		SetTop(((Component)val4).GetComponent<RectTransform>(), 45f, 252f, 60f, 36f);
		CreatePageMultiToggleGroup(parent, directPageToggles, new string[5] { "修", "思", "源", "参", "藏" }, 100f, 249f, delegate(int pageIndex, bool isOn)
		{
			SetPageFilter(directPageFilters, reversePageFilters, pageIndex, isOn);
		});
		TMP_Text val5 = CreateText(parent, "ReverseTitle", "逆页", 22, (TextAlignmentOptions)513);
		SetTop(((Component)val5).GetComponent<RectTransform>(), 430f, 252f, 60f, 36f);
		CreatePageMultiToggleGroup(parent, reversePageToggles, new string[5] { "用", "奇", "巧", "化", "绝" }, 485f, 249f, delegate(int pageIndex, bool isOn)
		{
			SetPageFilter(reversePageFilters, directPageFilters, pageIndex, isOn);
		});
		TMP_Text val6 = CreateText(parent, "CompleteTitle", "完整页", 22, (TextAlignmentOptions)513);
		SetTop(((Component)val6).GetComponent<RectTransform>(), 815f, 252f, 80f, 36f);
		CreatePageMultiToggleGroup(parent, completePageToggles, new string[5] { "1", "2", "3", "4", "5" }, 895f, 249f, delegate(int pageIndex, bool isOn)
		{
			if (isOn)
			{
				completePageFilters.Add(pageIndex);
			}
			else
			{
				completePageFilters.Remove(pageIndex);
			}
			OnFilterChanged();
		});
	}

	private void CreateFilterToggleGroup(Transform parent, List<CToggle> toggles, string[] labels, float startX, float top, float width, Action<int> onSelect)
	{
		for (int i = 0; i < labels.Length; i++)
		{
			int capturedIndex = i;
			CToggle item = CreateFilterToggle(parent, "FilterToggle" + toggles.Count, labels[i], startX + (float)i * (width + 6f), top, width, delegate(bool isOn)
			{
				if (isOn && !suppressFilterToggleCallback)
				{
					onSelect(capturedIndex);
				}
			});
			toggles.Add(item);
		}
	}

	private void CreatePageMultiToggleGroup(Transform parent, List<CToggle> toggles, string[] labels, float startX, float top, Action<int, bool> onChanged)
	{
		for (int i = 0; i < 5; i++)
		{
			int pageIndex = i;
			CToggle item = CreateFilterToggle(parent, "PageToggle" + toggles.Count, labels[i], startX + (float)i * 58f, top, 52f, delegate(bool isOn)
			{
				if (!suppressFilterToggleCallback)
				{
					onChanged(pageIndex, isOn);
				}
			});
			toggles.Add(item);
		}
	}

	private void SetPageFilter(HashSet<int> targetFilters, HashSet<int> oppositeFilters, int pageIndex, bool isOn)
	{
		if (isOn)
		{
			targetFilters.Add(pageIndex);
			oppositeFilters.Remove(pageIndex);
		}
		else
		{
			targetFilters.Remove(pageIndex);
		}
		OnFilterChanged();
	}

	public override void OnInit(ArgumentBox argsBox)
	{
		UpdateResolutionLayout(force: true);
		ResetData();
		taiwuId = SingletonObject.getInstance<BasicGameData>().TaiwuCharId;
		argsBox.Get("OrganizationId", out organizationId);
		argsBox.Get("OrganizationName", out organizationName);
		argsBox.Get("IsCombatSkill", out isCombatSkill);
		titleText.text = organizationName + (isCombatSkill ? "的门派藏书" : "的技艺藏书");
		statusText.text = "正在读取门派成员...";
		RenderAll();
		base.Element.OnListenerIdReady = (Action)Delegate.Remove(base.Element.OnListenerIdReady, new Action(OnListenerIdReady));
		base.Element.OnListenerIdReady = (Action)Delegate.Combine(base.Element.OnListenerIdReady, new Action(OnListenerIdReady));
	}

	public override void NotifyUIShow()
	{
		UpdateResolutionLayout(force: true);
		((UIBase)this).NotifyUIShow();
	}

	private void Update()
	{
		UpdateResolutionLayout(force: false);
	}

	private void UpdateResolutionLayout(bool force)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)panelRect == (Object)null))
		{
			Vector2 availableSize = GetAvailableSize();
			if (force || !(Mathf.Abs(availableSize.x - lastAvailableSize.x) < 1f) || !(Mathf.Abs(availableSize.y - lastAvailableSize.y) < 1f))
			{
				lastAvailableSize = availableSize;
				float num = Mathf.Max((availableSize.x - 80f) / 2200f, 0.65f);
				float num2 = Mathf.Max((availableSize.y - 80f) / 1240f, 0.65f);
				float num3 = Mathf.Min(new float[3] { 1f, num, num2 });
				((Transform)panelRect).localScale = new Vector3(num3, num3, 1f);
				panelRect.anchoredPosition = Vector2.zero;
			}
		}
	}

	private Vector2 GetAvailableSize()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = ((Component)this).transform.parent;
		RectTransform val = (RectTransform)(object)((parent is RectTransform) ? parent : null);
		Rect rect;
		if ((Object)(object)val != (Object)null)
		{
			rect = val.rect;
			if (rect.width > 0f)
			{
				rect = val.rect;
				if (rect.height > 0f)
				{
					rect = val.rect;
					return rect.size;
				}
			}
		}
		Transform transform = ((Component)this).transform;
		RectTransform val2 = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		if ((Object)(object)val2 != (Object)null)
		{
			rect = val2.rect;
			if (rect.width > 0f)
			{
				rect = val2.rect;
				if (rect.height > 0f)
				{
					rect = val2.rect;
					return rect.size;
				}
			}
		}
		return new Vector2((float)Screen.width, (float)Screen.height);
	}

	private void ResetData()
	{
		npcLoadingDone = false;
		loadedNpcCount = 0;
		pendingPageCount = 0;
		loadedPageCount = 0;
		activeTradeCharId = -1;
		isExchanging = false;
		pendingExchangeGroups = null;
		pendingExchangeGroupIndex = 0;
		pendingSelfAuthority = 0;
		bookPageIndex = 0;
		selectedGrade = 0;
		selectedSkillType = 0;
		firstPageFilter = -1;
		totalAuthority = 0;
		approveHighestGrade = 0;
		taiwuLoads = (0, 0);
		npcDatas.Clear();
		authorities.Clear();
		pagesDatas.Clear();
		npcItems.Clear();
		currItems.Clear();
		exchangeList.Clear();
		selectedItemIds.Clear();
		learnedBooks.Clear();
		directPageFilters.Clear();
		reversePageFilters.Clear();
		completePageFilters.Clear();
	}

	private void OnListenerIdReady()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		OrganizationDomainMethod.AsyncCall.GetSettlementMembers((IAsyncMethodRequestHandler)(object)this, (short)organizationId, new AsyncMethodCallbackDelegate(OnGetSettlementMembers));
	}

	private void OnGetSettlementMembers(int offset, RawDataPool dataPool)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		List<CharacterDisplayData> list = null;
		Serializer.Deserialize(dataPool, offset, ref list);
		if (list == null || list.Count == 0)
		{
			npcLoadingDone = true;
			statusText.text = "没有读取到门派成员。";
			base.Element.ShowAfterRefresh();
			RenderAll();
			return;
		}
		npcDatas.AddRange(list);
		CharacterDomainMethod.AsyncCall.GetHighestGradeCombatSkillById((IAsyncMethodRequestHandler)(object)this, npcDatas[0].CharacterId, new AsyncMethodCallbackDelegate(OnGetHighestGradeCombatSkill));
		for (int i = 0; i < npcDatas.Count; i++)
		{
			base.MonitorFields.Add(new MonitorDataField((ushort)4, (ushort)0, (ulong)npcDatas[i].CharacterId, new uint[1] { 34u }));
			GetBook(i);
		}
		base.MonitorFields.Add(new MonitorDataField((ushort)4, (ushort)0, (ulong)taiwuId, new uint[4] { 104u, 105u, 34u, 59u }));
		base.Element.MonitorData();
	}

	private void OnGetHighestGradeCombatSkill(int offset, RawDataPool dataPool)
	{
		Serializer.Deserialize(dataPool, offset, ref approveHighestGrade);
	}

	private void GetBook(int npcIndex)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		int characterId = npcDatas[npcIndex].CharacterId;
		MerchantDomainMethod.AsyncCall.GetTradeBookDisplayData((IAsyncMethodRequestHandler)(object)this, characterId, !isCombatSkill, (AsyncMethodCallbackDelegate)delegate(int offset, RawDataPool dataPool)
		{
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Expected O, but got Unknown
			activeTradeCharId = characterId;
			List<ItemDisplayData> list = new List<ItemDisplayData>();
			Serializer.Deserialize(dataPool, offset, ref list);
			List<ItemDisplayData> list2 = new List<ItemDisplayData>();
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ItemDisplayData val = list[i];
					if (IsCurrentBookType(val))
					{
						val.ItemSourceType = 4;
						val.OwnerCharId = characterId;
						val.SpecialArg = npcIndex;
						SetBookAuthority(val);
						list2.Add(val);
						pendingPageCount++;
						ItemDomainMethod.AsyncCall.GetSkillBookPagesInfo((IAsyncMethodRequestHandler)(object)this, val.Key, new AsyncMethodCallbackDelegate(OnGetPageInfo));
					}
				}
			}
			npcItems[characterId] = list2;
			loadedNpcCount++;
			if (loadedNpcCount >= npcDatas.Count)
			{
				npcLoadingDone = true;
			}
			TryFinishLoading();
		});
	}

	private bool IsCurrentBookType(ItemDisplayData item)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (item == null || item.Key.TemplateId < 0)
		{
			return false;
		}
		SkillBookItem val = ((ConfigData<SkillBookItem, short>)(object)SkillBook.Instance)[item.Key.TemplateId];
		if (isCombatSkill)
		{
			return val.CombatSkillTemplateId >= 0;
		}
		return val.ItemSubType == 1000 && val.LifeSkillTemplateId >= 0;
	}

	private void OnGetPageInfo(int offset, RawDataPool dataPool)
	{
		SkillBookPageDisplayData val = null;
		Serializer.Deserialize(dataPool, offset, ref val);
		if (val != null && !pagesDatas.ContainsKey(val.ItemKey.Id))
		{
			pagesDatas.Add(val.ItemKey.Id, (val.Type, val.State));
		}
		loadedPageCount++;
		TryFinishLoading();
	}

	private void TryFinishLoading()
	{
		if (!npcLoadingDone || loadedPageCount < pendingPageCount)
		{
			statusText.text = $"正在读取藏书... 成员 {loadedNpcCount}/{npcDatas.Count}，书页 {loadedPageCount}/{pendingPageCount}";
			RenderSummary();
		}
		else
		{
			RebuildCurrentItems();
			statusText.text = $"读取完成，共找到 {CountAllBooks()} 本藏书，当前筛选 {currItems.Count + exchangeList.Count} 本。";
			RenderAll();
			base.Element.ShowAfterRefresh();
		}
	}

	private void FinishActiveTradeSession()
	{
		int characterId = activeTradeCharId;
		if (characterId < 0 && npcDatas.Count > 0)
		{
			characterId = npcDatas[npcDatas.Count - 1].CharacterId;
		}
		if (characterId < 0)
		{
			characterId = taiwuId;
		}
		MerchantDomainMethod.Call.FinishBookTrade(characterId, !isCombatSkill);
		activeTradeCharId = -1;
	}

	public override void OnNotifyGameData(List<NotificationWrapper> notifications)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < notifications.Count; i++)
		{
			Notification notification = notifications[i].Notification;
			if (notification.Type != 0)
			{
				continue;
			}
			DataUid uid = notification.Uid;
			if (uid.DomainId == 4 && uid.DataId == 0 && (int)uid.SubId0 == taiwuId && (uid.SubId1 == 105 || uid.SubId1 == 104))
			{
				int num = 0;
				Serializer.Deserialize(notifications[i].DataPool, notification.ValueOffset, ref num);
				if (uid.SubId1 == 105)
				{
					taiwuLoads.Item2 = num;
				}
				else
				{
					taiwuLoads.Item1 = num;
				}
			}
			else if (uid.DomainId == 4 && uid.DataId == 0 && uid.SubId1 == 34)
			{
				ResourceInts val = default(ResourceInts);
				Serializer.Deserialize(notifications[i].DataPool, notification.ValueOffset, ref val);
				authorities[(int)uid.SubId0] = val.Get(7);
			}
			else if (uid.DomainId == 4 && uid.DataId == 0 && (int)uid.SubId0 == taiwuId && uid.SubId1 == 59)
			{
				List<short> list = null;
				Serializer.Deserialize(notifications[i].DataPool, notification.ValueOffset, ref list);
				learnedBooks.Clear();
				if (list != null)
				{
					learnedBooks.AddRange(list);
				}
			}
		}
		RenderAll();
	}

	private void RebuildCurrentItems()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		currItems.Clear();
		foreach (KeyValuePair<int, List<ItemDisplayData>> npcItem in npcItems)
		{
			for (int i = 0; i < npcItem.Value.Count; i++)
			{
				ItemDisplayData val = npcItem.Value[i];
				if (!selectedItemIds.Contains(val.Key.Id) && ShouldShowItem(val))
				{
					currItems.Add(val);
				}
			}
		}
		currItems.Sort(CompareBook);
		ClampBookPageIndex();
	}

	private bool ShouldShowItem(ItemDisplayData item)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		SkillBookItem val = ((ConfigData<SkillBookItem, short>)(object)SkillBook.Instance)[item.Key.TemplateId];
		if (val.Grade != selectedGrade)
		{
			return false;
		}
		if (GetBookSkillType(val) != selectedSkillType)
		{
			return false;
		}
		if (!pagesDatas.TryGetValue(item.Key.Id, out var value))
		{
			return firstPageFilter < 0 && directPageFilters.Count == 0 && reversePageFilters.Count == 0 && completePageFilters.Count == 0;
		}
		return MatchPageFilters(value.Item1, value.Item2);
	}

	private bool MatchPageFilters(sbyte[] pageTypes, sbyte[] pageStates)
	{
		if (isCombatSkill && firstPageFilter >= 0 && !PageTypeEquals(pageTypes, 0, firstPageFilter))
		{
			return false;
		}
		foreach (int directPageFilter in directPageFilters)
		{
			if (!PageTypeEquals(pageTypes, GetBodyPageDataIndex(directPageFilter), 0))
			{
				return false;
			}
		}
		foreach (int reversePageFilter in reversePageFilters)
		{
			if (!PageTypeEquals(pageTypes, GetBodyPageDataIndex(reversePageFilter), 1))
			{
				return false;
			}
		}
		foreach (int completePageFilter in completePageFilters)
		{
			if (!PageStateComplete(pageStates, GetBodyPageDataIndex(completePageFilter)))
			{
				return false;
			}
		}
		return true;
	}

	private int GetBodyPageDataIndex(int filterPageIndex)
	{
		return isCombatSkill ? (filterPageIndex + 1) : filterPageIndex;
	}

	private bool PageTypeEquals(sbyte[] pageTypes, int pageIndex, int expectedType)
	{
		return pageTypes != null && pageIndex >= 0 && pageIndex < pageTypes.Length && pageTypes[pageIndex] == expectedType;
	}

	private bool PageStateComplete(sbyte[] pageStates, int pageIndex)
	{
		return pageStates != null && pageIndex >= 0 && pageIndex < pageStates.Length && pageStates[pageIndex] == 0;
	}

	private int GetBookSkillType(SkillBookItem skillBookItem)
	{
		return isCombatSkill ? skillBookItem.CombatSkillType : skillBookItem.LifeSkillType;
	}

	private int GetSkillTypeCount()
	{
		return isCombatSkill ? 14 : 16;
	}

	private string GetSkillTypeName(int skillType)
	{
		if (isCombatSkill)
		{
			return ((ConfigData<CombatSkillTypeItem, sbyte>)(object)CombatSkillType.Instance)[skillType].Name;
		}
		return ((ConfigData<LifeSkillTypeItem, sbyte>)(object)Config.LifeSkillType.Instance)[skillType].Name;
	}

	private void OnFilterChanged()
	{
		bookPageIndex = 0;
		RebuildCurrentItems();
		RenderAll();
	}

	private void RefreshFilterTexts()
	{
		suppressFilterToggleCallback = true;
		int skillTypeCount = GetSkillTypeCount();
		for (int i = 0; i < skillTypeToggles.Count; i++)
		{
			bool flag = i < skillTypeCount;
			((Component)skillTypeToggles[i]).gameObject.SetActive(flag);
			if (flag)
			{
				SetToggleText(skillTypeToggles[i], GetSkillTypeName(i));
				((Toggle)skillTypeToggles[i]).isOn = i == selectedSkillType;
			}
			SetFilterToggleVisual(skillTypeToggles[i], flag && i == selectedSkillType);
		}
		for (int j = 0; j < gradeToggles.Count; j++)
		{
			((Toggle)gradeToggles[j]).isOn = j == selectedGrade;
			SetFilterToggleVisual(gradeToggles[j], j == selectedGrade);
		}
		for (int k = 0; k < firstPageToggles.Count; k++)
		{
			((Component)firstPageToggles[k]).gameObject.SetActive(isCombatSkill);
			((Toggle)firstPageToggles[k]).isOn = k == firstPageFilter + 1;
			SetFilterToggleVisual(firstPageToggles[k], isCombatSkill && k == firstPageFilter + 1);
		}
		RefreshPageToggleGroup(directPageToggles, directPageFilters, isCombatSkill);
		RefreshPageToggleGroup(reversePageToggles, reversePageFilters, isCombatSkill);
		RefreshPageToggleGroup(completePageToggles, completePageFilters, active: true);
		suppressFilterToggleCallback = false;
	}

	private void RefreshPageToggleGroup(List<CToggle> toggles, HashSet<int> filters, bool active)
	{
		for (int i = 0; i < toggles.Count; i++)
		{
			((Component)toggles[i]).gameObject.SetActive(active);
			bool isOn = active && filters.Contains(i);
			((Toggle)toggles[i]).isOn = isOn;
			SetFilterToggleVisual(toggles[i], isOn);
		}
	}

	private void PreviousBookPage()
	{
		if (bookPageIndex > 0)
		{
			bookPageIndex--;
			RenderBookRows();
		}
	}

	private void NextBookPage()
	{
		int bookTotalPages = GetBookTotalPages();
		if (bookPageIndex < bookTotalPages - 1)
		{
			bookPageIndex++;
			RenderBookRows();
		}
	}

	private int GetBookTotalPages()
	{
		return Math.Max(1, (currItems.Count + 80 - 1) / 80);
	}

	private void ClampBookPageIndex()
	{
		int bookTotalPages = GetBookTotalPages();
		bookPageIndex = Mathf.Clamp(bookPageIndex, 0, bookTotalPages - 1);
	}

	private int CompareBook(ItemDisplayData left, ItemDisplayData right)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		SkillBookItem val = ((ConfigData<SkillBookItem, short>)(object)SkillBook.Instance)[left.Key.TemplateId];
		SkillBookItem val2 = ((ConfigData<SkillBookItem, short>)(object)SkillBook.Instance)[right.Key.TemplateId];
		sbyte grade = val.Grade;
		int num = grade.CompareTo(val2.Grade);
		if (num != 0)
		{
			return num;
		}
		short itemSubType = val.ItemSubType;
		num = itemSubType.CompareTo(val2.ItemSubType);
		if (num != 0)
		{
			return num;
		}
		return left.Key.TemplateId.CompareTo(right.Key.TemplateId);
	}

	private void AddExchangeItem(ItemDisplayData item)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!selectedItemIds.Contains(item.Key.Id))
		{
			ItemDisplayData val = item.Clone(1);
			val.ItemSourceType = Extentions.ToSbyte((Enum)(object)(ItemSourceType)1);
			val.OwnerCharId = item.OwnerCharId;
			val.SpecialArg = item.SpecialArg;
			selectedItemIds.Add(item.Key.Id);
			exchangeList.Add(val);
			RebuildCurrentItems();
			RenderAll();
		}
	}

	private void RemoveExchangeItem(ItemDisplayData item)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		for (int num = exchangeList.Count - 1; num >= 0; num--)
		{
			if (exchangeList[num].Key.Id == item.Key.Id)
			{
				exchangeList.RemoveAt(num);
				break;
			}
		}
		selectedItemIds.Remove(item.Key.Id);
		RebuildCurrentItems();
		RenderAll();
	}

	private void ResetPage()
	{
		exchangeList.Clear();
		selectedItemIds.Clear();
		RebuildCurrentItems();
		RenderAll();
	}

	private void ChangeBook()
	{
		if (isExchanging || exchangeList.Count == 0 || GetAuthority(taiwuId) < totalAuthority)
		{
			return;
		}
		exchangeList.Sort((ItemDisplayData left, ItemDisplayData right) => left.SpecialArg.CompareTo(right.SpecialArg));
		List<List<ItemDisplayData>> list = new List<List<ItemDisplayData>>();
		List<ItemDisplayData> list2 = new List<ItemDisplayData>();
		for (int num = 0; num < exchangeList.Count; num++)
		{
			ItemDisplayData val = exchangeList[num];
			list2.Add(val);
			if (num == exchangeList.Count - 1 || val.SpecialArg != exchangeList[num + 1].SpecialArg)
			{
				list.Add(list2);
				list2 = new List<ItemDisplayData>();
			}
		}
		isExchanging = true;
		((Selectable)confirmButton).interactable = false;
		pendingExchangeGroups = list;
		pendingExchangeGroupIndex = 0;
		pendingSelfAuthority = GetAuthority(taiwuId);
		statusText.text = "正在换书...";
		ProcessNextExchangeGroup();
	}

	private void ProcessNextExchangeGroup()
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		if (pendingExchangeGroups == null || pendingExchangeGroupIndex >= pendingExchangeGroups.Count)
		{
			FinishExchangeBatch();
			return;
		}
		List<ItemDisplayData> boughtItems = pendingExchangeGroups[pendingExchangeGroupIndex];
		int characterId = npcDatas[boughtItems[0].SpecialArg].CharacterId;
		int num = 0;
		for (int i = 0; i < boughtItems.Count; i++)
		{
			num += (int)Math.Min(2147483647L, boughtItems[i].Value);
		}
		int selfAuthority = pendingSelfAuthority - num;
		int npcNewAuthority = GetAuthority(characterId) + num;
		activeTradeCharId = characterId;
		MerchantDomainMethod.AsyncCall.GetTradeBookDisplayData((IAsyncMethodRequestHandler)(object)this, characterId, !isCombatSkill, (AsyncMethodCallbackDelegate)delegate(int offset, RawDataPool dataPool)
		{
			List<ItemDisplayData> availableItems = new List<ItemDisplayData>();
			Serializer.Deserialize(dataPool, offset, ref availableItems);
			List<ItemDisplayData> list = RematchBooksByTemplateId(boughtItems, availableItems);
			if (list.Count != boughtItems.Count)
			{
				MerchantDomainMethod.Call.FinishBookTrade(characterId, !isCombatSkill);
				activeTradeCharId = -1;
				isExchanging = false;
				statusText.text = "换书失败：无法匹配后端书籍，请重新打开界面。";
				RenderAll();
			}
			else
			{
				List<ItemDisplayData> list2 = new List<ItemDisplayData>();
				MerchantDomainMethod.Call.ExchangeBook(characterId, list, list2, selfAuthority, npcNewAuthority);
				MerchantDomainMethod.Call.FinishBookTrade(characterId, !isCombatSkill);
				activeTradeCharId = -1;
				RemoveNpcItems(characterId, boughtItems);
				pendingSelfAuthority = selfAuthority;
				pendingExchangeGroupIndex++;
				ProcessNextExchangeGroup();
			}
		});
	}

	private List<ItemDisplayData> RematchBooksByTemplateId(List<ItemDisplayData> wantedItems, List<ItemDisplayData> availableItems)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		List<ItemDisplayData> list = new List<ItemDisplayData>();
		if (availableItems == null)
		{
			return list;
		}
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < wantedItems.Count; i++)
		{
			ItemDisplayData val = wantedItems[i];
			ItemDisplayData val2 = null;
			for (int j = 0; j < availableItems.Count; j++)
			{
				ItemDisplayData val3 = availableItems[j];
				if (!hashSet.Contains(val3.Key.Id) && val3.Key.TemplateId == val.Key.TemplateId)
				{
					val2 = val3;
					break;
				}
			}
			if (val2 != null)
			{
				hashSet.Add(val2.Key.Id);
				ItemDisplayData val4 = val2.Clone(1);
				val4.ItemSourceType = Extentions.ToSbyte((Enum)(object)(ItemSourceType)1);
				val4.OwnerCharId = val.OwnerCharId;
				val4.SpecialArg = val.SpecialArg;
				val4.Value = val.Value;
				list.Add(val4);
			}
		}
		return list;
	}

	private void FinishExchangeBatch()
	{
		exchangeList.Clear();
		selectedItemIds.Clear();
		pendingExchangeGroups = null;
		isExchanging = false;
		RebuildCurrentItems();
		statusText.text = "已提交换书请求。";
		RenderAll();
	}

	private void RemoveNpcItems(int characterId, List<ItemDisplayData> boughtItems)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!npcItems.TryGetValue(characterId, out var value))
		{
			return;
		}
		for (int i = 0; i < boughtItems.Count; i++)
		{
			int itemId = boughtItems[i].Key.Id;
			short templateId = boughtItems[i].Key.TemplateId;
			value.RemoveAll((ItemDisplayData item) => item.Key.Id == itemId || item.Key.TemplateId == templateId);
		}
	}

	private void RenderAll()
	{
		RefreshFilterTexts();
		RenderSummary();
		RenderBookRows();
		RenderExchangeRows();
		((Selectable)confirmButton).interactable = exchangeList.Count > 0 && GetAuthority(taiwuId) >= totalAuthority;
	}

	private int CountAllBooks()
	{
		int num = 0;
		foreach (KeyValuePair<int, List<ItemDisplayData>> npcItem in npcItems)
		{
			num += npcItem.Value.Count;
		}
		return num;
	}

	private void RenderSummary()
	{
		totalAuthority = 0;
		int num = 0;
		for (int i = 0; i < exchangeList.Count; i++)
		{
			num += exchangeList[i].Weight;
			totalAuthority += (int)Math.Min(2147483647L, exchangeList[i].Value);
		}
		float num2 = (float)(taiwuLoads.Item2 + num) / 100f;
		float num3 = (float)taiwuLoads.Item1 / 100f;
		int authority = GetAuthority(taiwuId);
		string text = (isCombatSkill ? "" : " 技艺书按好感与精纯限制。");
		summaryText.text = $"筛选 {currItems.Count}/{CountAllBooks()} 本，已选 {exchangeList.Count} 本，消耗威望 {totalAuthority}/{authority}，负重 {num2:F1}/{num3:F1}。{text}";
	}

	private void RenderBookRows()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)bookItemScroll != (Object)null)
		{
			RenderOfficialBookRows();
			return;
		}
		ClearChildren((Transform)(object)bookContent);
		bookContent.anchoredPosition = Vector2.zero;
		ClampBookPageIndex();
		int bookTotalPages = GetBookTotalPages();
		bookPageText.text = ((currItems.Count > 0) ? $"{bookPageIndex + 1}/{bookTotalPages} ({currItems.Count})" : "0/0 (0)");
		((Selectable)previousBookPageButton).interactable = currItems.Count > 0 && bookPageIndex > 0;
		((Selectable)nextBookPageButton).interactable = currItems.Count > 0 && bookPageIndex < bookTotalPages - 1;
		if (currItems.Count == 0)
		{
			GameObject val = CreateRow((Transform)(object)bookContent, "BookEmptyRow");
			SetRowRect(val.GetComponent<RectTransform>(), 0);
			CreateRowText(val.transform, "没有可显示的藏书", 1320f, (TextAlignmentOptions)514);
			SetContentHeight(bookContent, 1);
			return;
		}
		int num = bookPageIndex * 80;
		int num2 = Math.Min(currItems.Count, num + 80);
		for (int i = num; i < num2; i++)
		{
			ItemDisplayData val2 = currItems[i];
			string reason;
			bool flag = CanSelect(val2, out reason);
			GameObject val3 = CreateRow((Transform)(object)bookContent, "BookRow");
			SetRowRect(val3.GetComponent<RectTransform>(), i - num);
			CreateRowText(val3.transform, GetBookDescription(val2), 1120f, (TextAlignmentOptions)513);
			CreateRowText(val3.transform, GetOwnerName(val2), 190f, (TextAlignmentOptions)513);
			CreateRowText(val3.transform, val2.Value.ToString(), 80f, (TextAlignmentOptions)514);
			CButton val4 = CreateButton(val3.transform, "Add", flag ? "加入" : "不可换", 110f, 34f);
			((Selectable)val4).interactable = flag;
			if (!flag)
			{
				((Component)val4).GetComponentInChildren<TMP_Text>().text = reason;
			}
			ItemDisplayData capturedItem = val2;
			val4.ClearAndAddListener((Action)delegate
			{
				AddExchangeItem(capturedItem);
			});
		}
		SetContentHeight(bookContent, num2 - num);
	}

	private void RenderExchangeRows()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)exchangeItemScroll != (Object)null)
		{
			RenderOfficialExchangeRows();
			return;
		}
		ClearChildren((Transform)(object)exchangeContent);
		exchangeContent.anchoredPosition = Vector2.zero;
		for (int i = 0; i < exchangeList.Count; i++)
		{
			ItemDisplayData val = exchangeList[i];
			GameObject val2 = CreateRow((Transform)(object)exchangeContent, "ExchangeRow");
			SetRowRect(val2.GetComponent<RectTransform>(), i);
			CreateRowText(val2.transform, GetBookDescription(val), 390f, (TextAlignmentOptions)513);
			CreateRowText(val2.transform, val.Value.ToString(), 70f, (TextAlignmentOptions)514);
			CButton val3 = CreateButton(val2.transform, "Remove", "移除", 110f, 34f);
			ItemDisplayData capturedItem = val;
			val3.ClearAndAddListener((Action)delegate
			{
				RemoveExchangeItem(capturedItem);
			});
		}
		SetContentHeight(exchangeContent, exchangeList.Count);
	}

	private void RenderOfficialBookRows()
	{
		bookPageText.text = currItems.Count + " 本";
		visibleBookItems.Clear();
		for (int i = 0; i < currItems.Count; i++)
		{
			visibleBookItems.Add((ITradeableContent)(object)currItems[i]);
		}
		bookItemScroll.SetItemList((IReadOnlyList<ITradeableContent>)visibleBookItems);
		bookItemScroll.ReRender();
	}

	private void RenderOfficialExchangeRows()
	{
		visibleExchangeItems.Clear();
		for (int i = 0; i < exchangeList.Count; i++)
		{
			visibleExchangeItems.Add((ITradeableContent)(object)exchangeList[i]);
		}
		exchangeItemScroll.SetItemList((IReadOnlyList<ITradeableContent>)visibleExchangeItems);
		exchangeItemScroll.ReRender();
	}

	private void OnRenderBookItem(ITradeableContent itemData, RowItemLine rowItemLine)
	{
		ItemDisplayData val = (ItemDisplayData)(object)((itemData is ItemDisplayData) ? itemData : null);
		if (val == null)
		{
			return;
		}
		RowItemMain val2 = BindRowItemMain(rowItemLine, (ITradeableContent)(object)val);
		string reason;
		bool flag = CanSelect(val, out reason);
		rowItemLine.SetLocked(!flag);
		((RowItem)rowItemLine).SetInteractable(flag, true);
		((RowItem)rowItemLine).SetDisabled(!flag);
		SetBookTip(rowItemLine, val);
		if (!((Object)(object)val2 == (Object)null))
		{
			if (flag)
			{
				val2.HideInteractionState();
			}
			else
			{
				val2.SetInteractionStateLockText(reason);
			}
		}
	}

	private void OnClickBookItem(ITradeableContent itemData, RowItemLine rowItemLine)
	{
		ItemDisplayData val = (ItemDisplayData)(object)((itemData is ItemDisplayData) ? itemData : null);
		if (val != null && CanSelect(val, out var _))
		{
			AddExchangeItem(val);
		}
	}

	private void OnRenderExchangeItem(ITradeableContent itemData, RowItemLine rowItemLine)
	{
		ItemDisplayData val = (ItemDisplayData)(object)((itemData is ItemDisplayData) ? itemData : null);
		if (val != null)
		{
			RowItemMain val2 = BindRowItemMain(rowItemLine, (ITradeableContent)(object)val);
			rowItemLine.SetLocked(false);
			((RowItem)rowItemLine).SetInteractable(true, true);
			((RowItem)rowItemLine).SetDisabled(false);
			if (val2 != null)
			{
				val2.HideInteractionState();
			}
			SetBookTip(rowItemLine, val);
		}
	}

	private void OnClickExchangeItem(ITradeableContent itemData, RowItemLine rowItemLine)
	{
		ItemDisplayData val = (ItemDisplayData)(object)((itemData is ItemDisplayData) ? itemData : null);
		if (val != null)
		{
			RemoveExchangeItem(val);
		}
	}

	private void SetBookTip(RowItemLine rowItemLine, ItemDisplayData item)
	{
		if (!((Object)(object)((RowItem)rowItemLine).TipDisplayer == (Object)null))
		{
			RowItemLine.SetMouseTipDisplayer(true, (ITradeableContent)(object)item, ((RowItem)rowItemLine).TipDisplayer);
		}
	}

	private RowItemMain BindRowItemMain(RowItemLine rowItemLine, ITradeableContent itemData)
	{
		RowItemMain componentInChildren = ((Component)rowItemLine).GetComponentInChildren<RowItemMain>();
		if ((Object)(object)componentInChildren == (Object)null)
		{
			return null;
		}
		componentInChildren.SetData(itemData);
		rowItemLine.Set(componentInChildren, true);
		((RowItem)rowItemLine).SetSelected(false);
		return componentInChildren;
	}

	private bool CanSelect(ItemDisplayData item, out string reason)
	{
		CharacterDisplayData ownerDisplayData = GetOwnerDisplayData(item);
		if (IsBaseExchangeLocked(item, ownerDisplayData, out reason))
		{
			return false;
		}
		if (isCombatSkill)
		{
			if (IsLockByApproveEnough(item))
			{
				reason = LocalStringManager.Get((LanguageKey)5234);
				return false;
			}
			if (!IsTaiwuLearned(item))
			{
				reason = LocalStringManager.Get((LanguageKey)5235);
				return false;
			}
			reason = "";
			return true;
		}
		if (!IsFavorEnoughForPrivateBookTrade(ownerDisplayData, out reason))
		{
			return false;
		}
		if (!CanReadBookByConsummateLevel(item) && ownerDisplayData != null && ownerDisplayData.OrgInfo.OrgTemplateId != 16)
		{
			reason = "精纯不足";
			return false;
		}
		reason = "";
		return true;
	}

	private bool IsBaseExchangeLocked(ItemDisplayData item, CharacterDisplayData owner, out string reason)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		reason = "";
		if (owner != null && AgeGroup.GetAgeGroup(owner.PhysiologicalAge) == 0)
		{
			reason = "持有者幼年";
			return true;
		}
		bool flag = (int)item.UsingType != 2 || ItemTemplateHelper.IsDetachable(item.Key.ItemType, item.Key.TemplateId);
		bool flag2 = ItemTemplateHelper.IsTransferable(item.Key.ItemType, item.Key.TemplateId) && flag;
		bool flag3 = ItemTemplateHelper.MiscResourceCanExchange(item.Key.ItemType, item.Key.TemplateId);
		if ((flag2 || flag3) && !ItemDisplayDataHelper.IsItemLockedByTask((ITradeableContent)(object)item) && !item.IsLocked)
		{
			return false;
		}
		if (item.IsLocked)
		{
			reason = "物品锁定";
		}
		else if (ItemDisplayDataHelper.IsItemLockedByTask((ITradeableContent)(object)item))
		{
			reason = "任务占用";
		}
		else if (!flag)
		{
			reason = "装备不可卸";
		}
		else
		{
			reason = "不可转移";
		}
		return true;
	}

	private bool IsFavorEnoughForPrivateBookTrade(CharacterDisplayData owner, out string reason)
	{
		if (owner == null)
		{
			reason = "好感不足";
			return false;
		}
		sbyte favorabilityType = FavorabilityType.GetFavorabilityType(owner.FavorabilityToTaiwu);
		sbyte exchangeBookRequiredFavorabilityType = GetExchangeBookRequiredFavorabilityType(owner.BehaviorType);
		if (favorabilityType >= exchangeBookRequiredFavorabilityType)
		{
			reason = "";
			return true;
		}
		reason = "好感需" + CommonUtils.GetFavorStringByLevel(exchangeBookRequiredFavorabilityType);
		return false;
	}

	private sbyte GetExchangeBookRequiredFavorabilityType(sbyte behaviorType)
	{
		return behaviorType switch
		{
			0 => 6, 
			1 => 4, 
			2 => 3, 
			3 => 2, 
			4 => 5, 
			_ => 3, 
		};
	}

	private bool CanReadBookByConsummateLevel(ItemDisplayData item)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		sbyte grade = ItemTemplateHelper.GetGrade(item.Key.ItemType, item.Key.TemplateId);
		return SingletonObject.getInstance<BasicGameData>().XiangshuProgress >= (grade - 1) * 2;
	}

	private bool IsTaiwuLearned(ItemDisplayData item)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		SkillBookItem val = ((ConfigData<SkillBookItem, short>)(object)SkillBook.Instance)[item.Key.TemplateId];
		return learnedBooks.Contains(val.CombatSkillTemplateId);
	}

	private bool IsLockByApproveEnough(ItemDisplayData item)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return approveHighestGrade < ItemTemplateHelper.GetGrade(item.Key.ItemType, item.Key.TemplateId);
	}

	private int GetAuthority(int characterId)
	{
		if (authorities.TryGetValue(characterId, out var value))
		{
			return value;
		}
		return 0;
	}

	private string GetOwnerName(ItemDisplayData item)
	{
		CharacterDisplayData ownerDisplayData = GetOwnerDisplayData(item);
		if (ownerDisplayData == null)
		{
			return "";
		}
		return Extentions.SetGradeColor(NameCenter.GetMonasticTitleOrDisplayName(ownerDisplayData, false), (int)ownerDisplayData.OrgInfo.Grade);
	}

	private CharacterDisplayData GetOwnerDisplayData(ItemDisplayData item)
	{
		if (item.SpecialArg < 0 || item.SpecialArg >= npcDatas.Count)
		{
			return null;
		}
		return npcDatas[item.SpecialArg];
	}

	private string GetBookDescription(ItemDisplayData item)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		SkillBookItem val = ((ConfigData<SkillBookItem, short>)(object)SkillBook.Instance)[item.Key.TemplateId];
		string text = (isCombatSkill ? ((ConfigData<CombatSkillTypeItem, sbyte>)(object)CombatSkillType.Instance)[val.CombatSkillType].Name : ((ConfigData<LifeSkillTypeItem, sbyte>)(object)Config.LifeSkillType.Instance)[val.LifeSkillType].Name);
		string text2 = Extentions.SetGradeColor(ItemTemplateHelper.GetName(item.Key.ItemType, item.Key.TemplateId), (int)val.Grade);
		string text3 = 9 - val.Grade + "品";
		return $"{text3} {text} {text2} 页:{GetPageSummary(item)} 耐久:{item.Durability}/{item.MaxDurability}";
	}

	private string GetPageSummary(ItemDisplayData item)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (!pagesDatas.TryGetValue(item.Key.Id, out var value) || value.Item2 == null)
		{
			return "?/5";
		}
		int num = 0;
		for (int i = 0; i < 5; i++)
		{
			if (PageStateComplete(value.Item2, GetBodyPageDataIndex(i)))
			{
				num++;
			}
		}
		return num + "/" + 5;
	}

	private void SetBookAuthority(ItemDisplayData itemData)
	{
		sbyte behaviorType = npcDatas[itemData.SpecialArg].BehaviorType;
		int num = 100;
		switch (behaviorType)
		{
		case 0:
		case 4:
			num = 200;
			break;
		case 2:
			num = 150;
			break;
		}
		int num2 = Math.Max((int)itemData.MaxDurability, 1);
		itemData.Value = (int)((double)itemData.Value * (0.5 + 0.5 * (double)itemData.Durability / (double)num2) / 10.0 * (double)num / 100.0);
	}

	public override void QuickHide()
	{
		FinishActiveTradeSession();
		exchangeList.Clear();
		selectedItemIds.Clear();
		isExchanging = false;
		pendingExchangeGroups = null;
		((UIBase)this).QuickHide();
	}

	private RectTransform CreateScroll(Transform parent, string name, Vector2 position, Vector2 size)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = CreateImage(name, parent, new Color(0.05f, 0.05f, 0.05f, 0.75f));
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0f, 1f);
		component.anchorMax = new Vector2(0f, 1f);
		component.pivot = new Vector2(0f, 1f);
		component.anchoredPosition = new Vector2(position.x, 0f - position.y);
		component.sizeDelta = size;
		GameObject val2 = new GameObject("Content");
		val2.transform.SetParent(val.transform, false);
		RectTransform val3 = val2.AddComponent<RectTransform>();
		val3.anchorMin = new Vector2(0f, 1f);
		val3.anchorMax = new Vector2(1f, 1f);
		val3.pivot = new Vector2(0.5f, 1f);
		val3.anchoredPosition = Vector2.zero;
		val3.sizeDelta = Vector2.zero;
		return val3;
	}

	private ItemListScroll CreateOfficialItemScroll(Transform parent, string name, Vector2 position, Vector2 size, string saveKey, Action<ITradeableContent, RowItemLine> onRender, Action<ITradeableContent, RowItemLine> onClick)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		ItemListScroll itemListTemplate = UIBuilder.ItemListTemplate;
		if ((Object)(object)itemListTemplate == (Object)null)
		{
			return null;
		}
		GameObject val = Object.Instantiate<GameObject>(((Component)itemListTemplate).gameObject, parent, false);
		((Object)val).name = name;
		val.SetActive(true);
		RectTransform component = val.GetComponent<RectTransform>();
		component.anchorMin = new Vector2(0f, 1f);
		component.anchorMax = new Vector2(0f, 1f);
		component.pivot = new Vector2(0f, 1f);
		component.anchoredPosition = new Vector2(position.x, 0f - position.y);
		component.sizeDelta = size;
		ItemListScroll component2 = val.GetComponent<ItemListScroll>();
		InitializeOfficialItemScroll(component2, saveKey, onRender, onClick);
		CleanupClonedItemScroll(component2);
		return component2;
	}

	private void InitializeOfficialItemScroll(ItemListScroll itemScroll, string saveKey, Action<ITradeableContent, RowItemLine> onRender, Action<ITradeableContent, RowItemLine> onClick)
	{
		MethodInfo method = typeof(ItemListScroll).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		List<ColumnDefinition> list = CreateOfficialColumnDefinitions(itemScroll);
		Action<RowItem> action = delegate
		{
			itemScroll.PrepareRowTemplateContainers((ItemListScroll.EColumnType)1);
			itemScroll.PrepareRowTemplateContainers((ItemListScroll.EColumnType)8);
			itemScroll.PrepareRowTemplateContainers((ItemListScroll.EColumnType)32);
			itemScroll.PrepareRowTemplateContainers((ItemListScroll.EColumnType)64);
			itemScroll.PrepareRowTemplateContainers((ItemListScroll.EColumnType)4096);
		};
		object[] parameters = new object[9]
		{
			saveKey,
			(object)(ESortAndFilterControllerType)12,
			true,
			onRender,
			onClick,
			(object)(ItemListScroll.EColumnType)4201,
			null,
			list,
			action
		};
		method.Invoke(itemScroll, parameters);
		itemScroll.SetTableHeadSortEnabled(true);
	}

	private List<ColumnDefinition> CreateOfficialColumnDefinitions(ItemListScroll itemScroll)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		List<ColumnDefinition> list = new List<ColumnDefinition>
		{
			(ColumnDefinition)(object)itemScroll.ColumnIconAndName(new LayoutOption(180f, 1f, 420f, 1), true),
			(ColumnDefinition)(object)new ColumnDefinition<ITradeableContent, string>
			{
				LayoutOption = new LayoutOption(120f, 0f, 150f, 1),
				TableHeadLabel = () => "持有者",
				CellDataGenerator = GetOwnerColumnText,
				SortId = -1
			},
			(ColumnDefinition)(object)itemScroll.ColumnValue(new LayoutOption(40f, 0f, 80f, 1), true, (LanguageKey)9843),
			(ColumnDefinition)(object)itemScroll.ColumnDurability(new LayoutOption(70f, 0f, 95f, 1), true)
		};
		ColumnDefinition val = (ColumnDefinition)(object)itemScroll.ColumnBook(default(LayoutOption), false);
		val.LayoutOption.MinWidth = 360f;
		val.LayoutOption.PreferredWidth = 420f;
		list.Add(val);
		return list;
	}

	private string GetOwnerColumnText(ITradeableContent itemData)
	{
		ItemDisplayData val = (ItemDisplayData)(object)((itemData is ItemDisplayData) ? itemData : null);
		if (val == null)
		{
			return "";
		}
		return GetOwnerName(val);
	}

	private void CleanupClonedItemScroll(ItemListScroll itemScroll)
	{
		TextLanguage[] componentsInChildren = ((Component)itemScroll).GetComponentsInChildren<TextLanguage>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.DestroyImmediate((Object)(object)componentsInChildren[i]);
		}
		HideOfficialDisplayModeToggle(itemScroll);
		object obj = typeof(ItemListScroll).GetField("sortAndFilter", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(itemScroll);
		SortAndFilter val = (SortAndFilter)((obj is SortAndFilter) ? obj : null);
		if ((Object)(object)val != (Object)null)
		{
			val.SetEntryButtonForceHidden(true);
			object obj2 = typeof(SortAndFilter).GetField("filterSummaryArea", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(val);
			RectTransform val2 = (RectTransform)((obj2 is RectTransform) ? obj2 : null);
			if ((Object)(object)val2 != (Object)null)
			{
				((Component)val2).gameObject.SetActive(false);
			}
			object obj3 = typeof(SortAndFilter).GetField("entryButtonLabel", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(val);
			TextMeshProUGUI val3 = (TextMeshProUGUI)((obj3 is TextMeshProUGUI) ? obj3 : null);
			if ((Object)(object)val3 != (Object)null)
			{
				((Component)val3).gameObject.SetActive(false);
			}
		}
		RefreshTableHeadLabels(itemScroll);
	}

	private void RefreshTableHeadLabels(ItemListScroll itemScroll)
	{
		object obj = typeof(ItemListScroll).GetField("scroll", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(itemScroll);
		ListStyleGeneralScroll val = (ListStyleGeneralScroll)((obj is ListStyleGeneralScroll) ? obj : null);
		if (!((Object)(object)val == (Object)null))
		{
			TableHeadCell[] componentsInChildren = ((Component)val).GetComponentsInChildren<TableHeadCell>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].RefreshLabel();
			}
		}
	}

	private void HideOfficialDisplayModeToggle(ItemListScroll itemScroll)
	{
		object obj = typeof(ItemListScroll).GetField("btnSwitchCardMode", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(itemScroll);
		CToggleGroup val = (CToggleGroup)((obj is CToggleGroup) ? obj : null);
		if ((Object)(object)val != (Object)null)
		{
			((Component)val).gameObject.SetActive(false);
		}
	}

	private GameObject CreateRow(Transform parent, string name)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		GameObject val = CreateImage(name, parent, new Color(0.18f, 0.16f, 0.13f, 0.88f));
		HorizontalLayoutGroup val2 = val.AddComponent<HorizontalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)val2).childForceExpandWidth = false;
		((HorizontalOrVerticalLayoutGroup)val2).childForceExpandHeight = false;
		((LayoutGroup)val2).childAlignment = (TextAnchor)3;
		((LayoutGroup)val2).padding = new RectOffset(8, 8, 4, 4);
		((HorizontalOrVerticalLayoutGroup)val2).spacing = 8f;
		return val;
	}

	private void SetRowRect(RectTransform rectTransform, int rowIndex)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.anchorMin = new Vector2(0f, 1f);
		rectTransform.anchorMax = new Vector2(1f, 1f);
		rectTransform.pivot = new Vector2(0.5f, 1f);
		rectTransform.offsetMin = new Vector2(0f, rectTransform.offsetMin.y);
		rectTransform.offsetMax = new Vector2(0f, rectTransform.offsetMax.y);
		rectTransform.anchoredPosition = new Vector2(0f, (float)(-rowIndex) * 48f);
		rectTransform.sizeDelta = new Vector2(0f, 42f);
	}

	private void SetContentHeight(RectTransform content, int rowCount)
	{
		float num = ((rowCount <= 0) ? 0f : ((float)rowCount * 42f + (float)Math.Max(0, rowCount - 1) * 6f));
		content.SetSizeWithCurrentAnchors((RectTransform.Axis)1, num);
	}

	private TMP_Text CreateRowText(Transform parent, string text, float width, TextAlignmentOptions alignment)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		TMP_Text val = CreateText(parent, "Text", text, 20, alignment);
		LayoutElement val2 = ((Component)val).gameObject.AddComponent<LayoutElement>();
		val2.minWidth = width;
		val2.preferredWidth = width;
		val2.minHeight = 34f;
		val.enableWordWrapping = false;
		val.overflowMode = (TextOverflowModes)1;
		return val;
	}

	private static GameObject CreateImage(string name, Transform parent, Color color)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(name);
		val.transform.SetParent(parent, false);
		val.layer = LayerMask.NameToLayer("UI");
		val.AddComponent<RectTransform>();
		Image val2 = val.AddComponent<Image>();
		((Graphic)val2).color = color;
		((Graphic)val2).raycastTarget = color.a > 0f;
		return val;
	}

	private static TMP_Text CreateText(Transform parent, string name, string text, int fontSize, TextAlignmentOptions alignment)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(name);
		val.transform.SetParent(parent, false);
		val.layer = LayerMask.NameToLayer("UI");
		RectTransform val2 = val.AddComponent<RectTransform>();
		val2.sizeDelta = new Vector2(300f, 40f);
		TextMeshProUGUI val3 = val.AddComponent<TextMeshProUGUI>();
		ApplyGameFont((TMP_Text)(object)val3);
		((TMP_Text)val3).text = text;
		((TMP_Text)val3).fontSize = fontSize;
		((Graphic)val3).color = Color.white;
		((TMP_Text)val3).alignment = alignment;
		((TMP_Text)val3).enableWordWrapping = true;
		((Graphic)val3).raycastTarget = false;
		return (TMP_Text)(object)val3;
	}

	private static CButton CreateButton(Transform parent, string name, string text, float width, float height)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = CreateImage(name, parent, new Color(0.32f, 0.22f, 0.12f, 1f));
		RectTransform component = val.GetComponent<RectTransform>();
		component.sizeDelta = new Vector2(width, height);
		CButton result = val.AddComponent<CButton>();
		TMP_Text val2 = CreateText(val.transform, "Label", text, 22, (TextAlignmentOptions)514);
		SetStretch(((Component)val2).GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		return result;
	}

	private static CToggle CreateFilterToggle(Transform parent, string name, string text, float left, float top, float width, Action<bool> onChanged)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = CreateImage(name, parent, new Color(0.18f, 0.16f, 0.13f, 1f));
		RectTransform component = val.GetComponent<RectTransform>();
		SetTop(component, left, top, width, 38f);
		CToggle toggle = val.AddComponent<CToggle>();
		((Selectable)toggle).targetGraphic = (Graphic)(object)val.GetComponent<Image>();
		TMP_Text val2 = CreateText(val.transform, "Label", text, 18, (TextAlignmentOptions)514);
		SetStretch(((Component)val2).GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		((UnityEvent<bool>)(object)((Toggle)toggle).onValueChanged).AddListener((UnityAction<bool>)delegate(bool isOn)
		{
			SetFilterToggleVisual(toggle, isOn);
			onChanged(isOn);
		});
		SetFilterToggleVisual(toggle, isOn: false);
		return toggle;
	}

	private static void SetFilterToggleVisual(CToggle toggle, bool isOn)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Image component = ((Component)toggle).GetComponent<Image>();
		if ((Object)(object)component != (Object)null)
		{
			((Graphic)component).color = (isOn ? new Color(0.45f, 0.28f, 0.12f, 1f) : new Color(0.18f, 0.16f, 0.13f, 1f));
		}
	}

	private static void SetToggleText(CToggle toggle, string text)
	{
		TMP_Text componentInChildren = ((Component)toggle).GetComponentInChildren<TMP_Text>(true);
		if ((Object)(object)componentInChildren != (Object)null)
		{
			componentInChildren.text = text;
		}
	}

	private static void ApplyGameFont(TMP_Text label)
	{
		if ((Object)(object)label == (Object)null)
		{
			return;
		}
		if ((Object)(object)cachedFont == (Object)null)
		{
			TMP_Text[] array = Resources.FindObjectsOfTypeAll<TMP_Text>();
			for (int i = 0; i < array.Length; i++)
			{
				if ((Object)(object)array[i] != (Object)null && (Object)(object)array[i].font != (Object)null)
				{
					cachedFont = array[i].font;
					break;
				}
			}
		}
		if ((Object)(object)cachedFont != (Object)null)
		{
			label.font = cachedFont;
		}
	}

	private static void ClearChildren(Transform parent)
	{
		for (int num = parent.childCount - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)((Component)parent.GetChild(num)).gameObject);
		}
	}

	private static void SetStretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.anchorMin = anchorMin;
		rectTransform.anchorMax = anchorMax;
		rectTransform.offsetMin = offsetMin;
		rectTransform.offsetMax = offsetMax;
	}

	private static void SetTop(RectTransform rectTransform, float left, float top, float width, float height)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.anchorMin = new Vector2(0f, 1f);
		rectTransform.anchorMax = new Vector2(0f, 1f);
		rectTransform.pivot = new Vector2(0f, 1f);
		rectTransform.anchoredPosition = new Vector2(left, 0f - top);
		rectTransform.sizeDelta = new Vector2(width, height);
	}

	private static void SetTopRight(RectTransform rectTransform, float right, float top, float width, float height)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.anchorMin = new Vector2(1f, 1f);
		rectTransform.anchorMax = new Vector2(1f, 1f);
		rectTransform.pivot = new Vector2(1f, 1f);
		rectTransform.anchoredPosition = new Vector2(0f - right, 0f - top);
		rectTransform.sizeDelta = new Vector2(width, height);
	}

	private static void SetBottom(RectTransform rectTransform, float left, float bottom, float width, float height)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.anchorMin = new Vector2(0f, 0f);
		rectTransform.anchorMax = new Vector2(0f, 0f);
		rectTransform.pivot = new Vector2(0f, 0f);
		rectTransform.anchoredPosition = new Vector2(left, bottom);
		rectTransform.sizeDelta = new Vector2(width, height);
	}

	private static void SetBottomRight(RectTransform rectTransform, float right, float bottom, float width, float height)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.anchorMin = new Vector2(1f, 0f);
		rectTransform.anchorMax = new Vector2(1f, 0f);
		rectTransform.pivot = new Vector2(1f, 0f);
		rectTransform.anchoredPosition = new Vector2(0f - right, bottom);
		rectTransform.sizeDelta = new Vector2(width, height);
	}
}
