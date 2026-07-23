return {
	Source = 1,
	Author = "p（发射的熟鸡蛋自用修复）",
	Description = "AI无敌了，我直接投降\n-------------------------\n门派换书自用修复\n小修一下，也许会有问题也说不定，至少可以当搜索器",
	Title = "全门派换书",
	FrontendPlugins = {
		"ExchangeBook.dll",
	},
	BackendPlugins = {
		"ExchangeBookBackend.dll",
	},
	DefaultSettings = {
		{
			SettingType = "Toggle",
			Key = "OnlyInSect",
			DisplayName = "地点限制",
			Description = "只有在门派地格才能换书",
			DefaultValue = false,
		},
	},
	FileId = 2975299672,
	GameVersion = "1.0.67",
	Version = "1.0.3.0",
	TagList = {
		"Extensions",
	},
	Visibility = 0,
	NeedRestartWhenSettingChanged = false,
	Cover = "cover.jpg",
	WorkshopCover = "cover.jpg",
}
