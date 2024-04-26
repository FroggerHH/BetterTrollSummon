using BepInEx;
using BepInEx.Configuration;
using BetterTrollSummon.LocalizationManager;

namespace BetterTrollSummon;

[BepInPlugin(ModGUID, ModName, ModVersion)]
[BepInDependency("com.Frogger.NoUselessWarnings", BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    private const string ModName = "BetterTrollSummon",
        ModAuthor = "Frogger",
        ModVersion = "0.1.0",
        ModGUID = $"com.{ModAuthor}.{ModName}";

    public static ConfigEntry<int> maxlevelBloodMagicTrollLevel { get; private set; }
    public static ConfigEntry<int> maxlevelBloodMagicTrollCount { get; private set; }
    public static ConfigEntry<int> minlevelFriendlyTroll { get; private set; }
    public static ConfigEntry<int> startChanceFriendlyTroll { get; private set; }
    public static ConfigEntry<Character.Faction> friendlyTrollFaction { get; private set; }
    public static ConfigEntry<Character.Faction> normalTrollFaction { get; private set; }

    private void Awake()
    {
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGUID);
        OnConfigurationChanged += UpdateStaff;
        Localizer.Load();

        //TODO: Describe mods functionality in README.md
        //TODO: List all configs in README.md
        maxlevelBloodMagicTrollLevel = config("Troll", "Max Blood Magic Troll Level", 2,
            new ConfigDescription("When BloodMagic skill is 100, Trolls will spawn with this level",
                new AcceptableValueRange<int>(1, 3)));
        maxlevelBloodMagicTrollCount = config("Troll", "Max Blood Magic Troll Count", 4,
            new ConfigDescription("When BloodMagic skill is 100, you will be able to summon up to this many Trolls",
                new AcceptableValueRange<int>(1, 10)));
        minlevelFriendlyTroll = config("Troll", "Min Friendly Troll Level", 45,
            new ConfigDescription(
                "When BloodMagic skill is more than this level, there will be chance to summon friendly Troll",
                new AcceptableValueRange<int>(0, 100)));
        startChanceFriendlyTroll = config("Troll", "Start Chance Friendly Troll", 10,
            new ConfigDescription(
                "Base chance to summon friendly Troll when BloodMagic skill is more than [minlevelFriendlyTroll]. 0 = never, 100 = always. "
                + "It will automatically grow with increasing BloodMagic skill level up to 100",
                new AcceptableValueRange<int>(0, 100)));
        friendlyTrollFaction = config("Troll", "Faction", Character.Faction.Players,
            "For advanced mod users and special purposes");
        normalTrollFaction = config("Troll", "Normal Troll Faction", Character.Faction.AnimalsVeg,
            "For advanced mod users and special purposes");
    }

    public void UpdateStaff()
    {
        var staff = ZNetScene.instance?.GetPrefab("StaffRedTroll")?.GetComponent<ItemDrop>();
        if (!staff) return;
        var shared = staff.m_itemData.m_shared;
        MainAttack();
        SecondaryAttack();

        void SecondaryAttack()
        {
            shared.m_secondaryAttack.m_attackAnimation = "sword_secondary";
            //TODO: secondary attack will make trolls go in its direction or target enemy attack is pointed
        }

        void MainAttack()
        {
            var spawnAbility = shared.m_attack.m_attackProjectile?.GetComponent<SpawnAbility>();
            if (!spawnAbility) return;
            if (!spawnAbility.m_levelUpSettings.Exists(x => x.m_skillLevel >= 99))
                spawnAbility.m_levelUpSettings.Add(new() { m_skillLevel = 99, m_skill = Skills.SkillType.BloodMagic });

            var upSettings = spawnAbility.m_levelUpSettings.Find(x => x.m_skillLevel >= 99);
            upSettings.m_setLevel = maxlevelBloodMagicTrollLevel.Value;
            upSettings.m_maxSpawns = maxlevelBloodMagicTrollCount.Value;
            spawnAbility.m_maxSpawned = maxlevelBloodMagicTrollCount.Value;

            spawnAbility.m_commandOnSpawn = true;
            spawnAbility.m_setMaxInstancesFromWeaponLevel = false;

            var troll = spawnAbility.m_spawnPrefab.FirstOrDefault()?.GetComponent<Humanoid>();
            if (troll) troll.gameObject.GetOrAddComponent<Tameable>();
        }
    }

    public static int GetCurrentFriendlyTrollChance()
    {
        return Clamp(CeilToInt(Lerp(startChanceFriendlyTroll.Value, 100,
            Player.m_localPlayer.m_skills.GetSkillLevel(Skills.SkillType.BloodMagic) / 100f) + 0.08f), 0, 100);
    }
}

// public static class Do
// {
//     public static void Main()
//     {
//         var staff = ZNetScene.instance.GetPrefab("StaffRedTroll").GetComponent<ItemDrop>();
//         if (!staff) return;
//         var shared = staff.m_itemData.m_shared;
//         var spawnAbility = shared.m_attack.m_attackProjectile.GetComponent<SpawnAbility>();
//         if (!spawnAbility) return;
// 		
//         System.Console.WriteLine(string.Join("\n", spawnAbility.m_levelUpSettings.Select(x => $"m_skill={x.m_skill} m_skillLevel={x.m_skillLevel} m_maxSpawns={x.m_maxSpawns} m_setLevel={x.m_setLevel}")));
//     }
// }