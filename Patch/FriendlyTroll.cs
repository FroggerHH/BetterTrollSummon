using HarmonyLib;

namespace BetterTrollSummon.Patch;

[HarmonyPatch, HarmonyWrapSafe]
public class FriendlyTroll
{
    [HarmonyPatch(typeof(SpawnAbility), nameof(SpawnAbility.Spawn))]
    [HarmonyPrefix]
    private static void _(SpawnAbility __instance)
    {
        if (__instance.m_spawnPrefab.FirstOrDefault()?.GetPrefabName() != "Troll_Summoned") return;
        var num = SpawnSystem.GetNrOfInstances(__instance.m_spawnPrefab.First());
        if (num >= __instance.m_maxSpawned)
        {
            __instance.m_owner.Message(MessageHud.MessageType.Center, "$BetterTrollSummon_msg_MaxSpawned");
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.Start))]
    [HarmonyPostfix]
    private static void _(Humanoid __instance)
    {
        if (__instance.IsPlayer() || !__instance.GetPrefabName().Equals("Troll_Summoned")) return;

        var isFriendlyTroll = __instance.m_nview.GetZDO().GetBool("FriendlyTroll", false);
        if (isFriendlyTroll) __instance.m_faction = friendlyTrollFaction.Value;
        else __instance.m_faction = normalTrollFaction.Value;
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.RPC_Command))]
    [HarmonyPrefix]
    private static bool _(Tameable __instance, ZDOID characterID)
    {
        var troll = __instance.m_character;
        if (troll.IsPlayer() || !__instance.GetPrefabName().Equals("Troll_Summoned")) return true;
        if (__instance.GetPlayer(characterID) != m_localPlayer) return true;

        var chance = GetCurrentFriendlyTrollChance();
        if (FloorToInt(Random.value * 100) <= chance)
        {
            m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$BetterTrollSummon_msg_FriendlyTroll_Success");
            troll.m_faction = friendlyTrollFaction.Value;
        } else
        {
            m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$BetterTrollSummon_msg_FriendlyTroll_Fail");
            troll.m_faction = normalTrollFaction.Value;
        }

        troll.m_nview.GetZDO().Set("FriendlyTroll", troll.m_faction == friendlyTrollFaction.Value ? troll : false);
        ZNetScene.instance.GetPrefab("Troll_Summoned").GetComponent<Humanoid>().m_faction = normalTrollFaction.Value;
        Destroy(__instance);
        return false;
    }

    [HarmonyPatch(typeof(ItemDrop.ItemData), nameof(ItemDrop.ItemData.GetTooltip), []), HarmonyPostfix]
    static void ItemDropItemDataGetTooltip(ItemDrop.ItemData __instance, ref string __result)
    {
        if (__instance == null || !__instance.m_dropPrefab
                               || __instance.m_dropPrefab.GetPrefabName() != "StaffRedTroll") return;
        string adition = "";

        adition += string.Format("$BetterTrollSummon_tip_FriendlyTroll_Chance".Localize(),
            $"<color=#EBA917>{GetCurrentFriendlyTrollChance()}%</color>");
        var spawnAbility = __instance.m_shared.m_attack.m_attackProjectile?.GetComponent<SpawnAbility>();
        if (!spawnAbility || spawnAbility.m_levelUpSettings.Count == 0) return;


        SpawnAbility.LevelUpSettings levelUpSetting = null;
        for (int index = spawnAbility.m_levelUpSettings.Count - 1; index >= 0; --index)
        {
            levelUpSetting = spawnAbility.m_levelUpSettings[index];
            if (!(m_localPlayer.m_skills.GetSkillLevel(levelUpSetting.m_skill)
                  >= levelUpSetting.m_skillLevel)) continue;
            break;
        }

        if (levelUpSetting != null)
        {
            var maxCount = spawnAbility.m_setMaxInstancesFromWeaponLevel
                ? __instance.m_quality
                : levelUpSetting.m_maxSpawns;
            var level = levelUpSetting.m_setLevel;
            adition += string.Format("\n$BetterTrollSummon_tip_Troll_Level".Localize(),
                $"<color=#EBA917>{level}</color>");
            adition += string.Format("\n$BetterTrollSummon_tip_Troll_Count".Localize(),
                $"<color=#EBA917>{maxCount}</color>");
        }

        __result = $"{adition}\n\n{__result}";
    }
}