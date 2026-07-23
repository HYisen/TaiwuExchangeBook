using HarmonyLib;
using NLog;
using TaiwuModdingLib.Core.Plugin;

namespace ExchangeBookBackend;

[PluginConfig("ExchangeBookPlusBackend", "p", "1.0")]
public class ModMain : TaiwuRemakePlugin
{
	private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

	private Harmony harmony;

	public override void Initialize()
	{
		harmony = Harmony.CreateAndPatchAll(typeof(SkillBookLeakFix), (string)null);
		Logger.Info("[ExchangeBookPlusBackend] Initialized.");
	}

	public override void Dispose()
	{
		Harmony obj = harmony;
		if (obj != null)
		{
			obj.UnpatchSelf();
		}
		harmony = null;
	}

	internal static void LogInfo(string message)
	{
		Logger.Info(message);
	}
}
