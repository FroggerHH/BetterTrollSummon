using HarmonyLib;

namespace BetterTrollSummon.Patch;

[HarmonyPatch, HarmonyWrapSafe]
public class ApplyChanges
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    [HarmonyPostfix]
    private static void _() => GetPlugin<Plugin>().UpdateStaff();
}