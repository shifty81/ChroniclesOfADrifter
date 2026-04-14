using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Manages the achievement catalogue and tracks player progress.
/// Other systems call the static Award* helpers to record events.
/// </summary>
public class AchievementSystem : ISystem
{
    // --- Achievement catalogue ---
    private static readonly List<AchievementDefinition> _allAchievements = new()
    {
        // Combat
        new("first_blood",     "First Blood",       "Defeat your first enemy",               AchievementCategory.Combat,      1,   50),
        new("warrior",         "Warrior",            "Defeat 50 enemies",                     AchievementCategory.Combat,     50,  200),
        new("conqueror",       "Conqueror",          "Defeat 250 enemies",                    AchievementCategory.Combat,    250,  500),
        new("boss_slayer",     "Boss Slayer",        "Defeat your first boss",                AchievementCategory.Combat,      1,  300),
        new("boss_master",     "Boss Master",        "Defeat 5 bosses",                       AchievementCategory.Combat,      5,  750),

        // Exploration
        new("explorer",        "Explorer",           "Explore 10 chunks",                     AchievementCategory.Exploration, 10,  100),
        new("adventurer",      "Adventurer",         "Explore 50 chunks",                     AchievementCategory.Exploration, 50,  250),
        new("world_traveler",  "World Traveler",     "Explore 200 chunks",                    AchievementCategory.Exploration,200,  500),

        // Crafting
        new("apprentice_smith","Apprentice Smith",   "Complete your first craft",             AchievementCategory.Crafting,    1,   75),
        new("journeyman_smith","Journeyman Smith",   "Complete 25 crafts",                    AchievementCategory.Crafting,   25,  200),
        new("master_smith",    "Master Smith",       "Complete 100 crafts",                   AchievementCategory.Crafting,  100,  500),

        // Farming
        new("green_thumb",     "Green Thumb",        "Harvest your first crop",               AchievementCategory.Farming,     1,   75),
        new("farmer",          "Farmer",             "Harvest 50 crops",                      AchievementCategory.Farming,    50,  200),
        new("master_farmer",   "Master Farmer",      "Harvest 200 crops",                     AchievementCategory.Farming,   200,  500),

        // Social
        new("socialite",       "Socialite",          "Speak to 5 different NPCs",             AchievementCategory.Social,      5,  100),
        new("diplomat",        "Diplomat",           "Speak to 20 different NPCs",            AchievementCategory.Social,     20,  300),

        // Survival
        new("survivor",        "Survivor",           "Die and respawn for the first time",    AchievementCategory.Survival,    1,   50),
        new("undying",         "Undying",            "Die and respawn 10 times",              AchievementCategory.Survival,   10,  150),

        // Collection
        new("hoarder",         "Hoarder",            "Collect 100 items",                     AchievementCategory.Collection, 100,  200),
        new("pack_rat",        "Pack Rat",           "Collect 500 items",                     AchievementCategory.Collection, 500,  500),

        // Progression
        new("level5",          "Seasoned",           "Reach level 5",                         AchievementCategory.Progression,  5,  150),
        new("level10",         "Veteran",            "Reach level 10",                        AchievementCategory.Progression, 10,  400),
        new("level20",         "Legend",             "Reach level 20",                        AchievementCategory.Progression, 20, 1000),
    };

    public static IReadOnlyList<AchievementDefinition> AllAchievements => _allAchievements;

    // -----------------------------------------------------------------------

    public void Initialize(World world)
    {
        Console.WriteLine("[Achievement] Achievement system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        foreach (var entity in world.GetEntitiesWithComponent<AchievementComponent>())
        {
            var ach = world.GetComponent<AchievementComponent>(entity);
            if (ach == null) continue;

            // Display any recently unlocked achievements
            if (ach.RecentlyUnlocked.Count > 0)
            {
                foreach (var id in ach.RecentlyUnlocked)
                {
                    var def = _allAchievements.FirstOrDefault(a => a.Id == id);
                    if (def != null)
                    {
                        Console.WriteLine($"[Achievement] 🏆 UNLOCKED: \"{def.Name}\" - {def.Description} (+{def.XPReward} XP)");
                        ach.AddXPEarned(def.XPReward);
                    }
                }
                ach.ClearRecentlyUnlocked();
            }
        }
    }

    // -----------------------------------------------------------------------
    // Static helpers for other systems to award achievement progress
    // -----------------------------------------------------------------------

    /// <summary>Initialize all achievement slots for a player entity</summary>
    public static void InitializePlayer(World world, Entity playerEntity)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        foreach (var def in _allAchievements)
            ach.InitializeAchievement(def);
    }

    public static void OnEnemyDefeated(World world, Entity playerEntity)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        ach.EnemiesDefeated++;
        CheckMilestones(ach, "first_blood",  ach.EnemiesDefeated,  1);
        CheckMilestones(ach, "warrior",      ach.EnemiesDefeated,  50);
        CheckMilestones(ach, "conqueror",    ach.EnemiesDefeated,  250);
    }

    public static void OnBossDefeated(World world, Entity playerEntity)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        ach.BossesDefeated++;
        CheckMilestones(ach, "boss_slayer",  ach.BossesDefeated, 1);
        CheckMilestones(ach, "boss_master",  ach.BossesDefeated, 5);
    }

    public static void OnChunkExplored(World world, Entity playerEntity)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        ach.ChunksExplored++;
        CheckMilestones(ach, "explorer",      ach.ChunksExplored, 10);
        CheckMilestones(ach, "adventurer",    ach.ChunksExplored, 50);
        CheckMilestones(ach, "world_traveler",ach.ChunksExplored, 200);
    }

    public static void OnCraftCompleted(World world, Entity playerEntity)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        ach.CraftsCompleted++;
        CheckMilestones(ach, "apprentice_smith", ach.CraftsCompleted, 1);
        CheckMilestones(ach, "journeyman_smith", ach.CraftsCompleted, 25);
        CheckMilestones(ach, "master_smith",     ach.CraftsCompleted, 100);
    }

    public static void OnCropHarvested(World world, Entity playerEntity)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        ach.CropsHarvested++;
        CheckMilestones(ach, "green_thumb",   ach.CropsHarvested, 1);
        CheckMilestones(ach, "farmer",        ach.CropsHarvested, 50);
        CheckMilestones(ach, "master_farmer", ach.CropsHarvested, 200);
    }

    public static void OnNPCSpokenTo(World world, Entity playerEntity)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        ach.NPCsSpokenTo++;
        CheckMilestones(ach, "socialite", ach.NPCsSpokenTo, 5);
        CheckMilestones(ach, "diplomat",  ach.NPCsSpokenTo, 20);
    }

    public static void OnPlayerDied(World world, Entity playerEntity)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        ach.DeathCount++;
        CheckMilestones(ach, "survivor", ach.DeathCount, 1);
        CheckMilestones(ach, "undying",  ach.DeathCount, 10);
    }

    public static void OnItemCollected(World world, Entity playerEntity, int count = 1)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        ach.ItemsCollected += count;
        CheckMilestones(ach, "hoarder",  ach.ItemsCollected, 100);
        CheckMilestones(ach, "pack_rat", ach.ItemsCollected, 500);
    }

    public static void OnLevelReached(World world, Entity playerEntity, int level)
    {
        var ach = world.GetComponent<AchievementComponent>(playerEntity);
        if (ach == null) return;
        if (level >= 5)  CheckMilestones(ach, "level5",  level, 5);
        if (level >= 10) CheckMilestones(ach, "level10", level, 10);
        if (level >= 20) CheckMilestones(ach, "level20", level, 20);
    }

    // -----------------------------------------------------------------------

    private static void CheckMilestones(AchievementComponent ach, string id, int current, int required)
    {
        if (!ach.Progress.TryGetValue(id, out var prog)) return;
        if (prog.IsUnlocked) return;
        // Set progress to current (cap at required)
        int delta = Math.Max(0, current - prog.CurrentCount);
        if (delta > 0)
            ach.IncrementProgress(id, delta, required);
    }
}
