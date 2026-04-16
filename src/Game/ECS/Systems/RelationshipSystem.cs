using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Manages NPC relationships for all player entities:
/// - Resets daily gift counters when the in-game day rolls over.
/// - Applies a passive friendship bonus for each NPC the player has interacted with today.
/// - Exposes relationship level as a reputation modifier fed into TradingSystem price calculations.
/// - Unlocks dialogue tiers based on relationship threshold.
/// </summary>
public class RelationshipSystem : ISystem
{
    private TimeSystem? _timeSystem;
    private int _lastDay = -1;

    // Relationship-level → reputation multiplier fed to TradingSystem
    private static readonly Dictionary<RelationshipLevel, float> ReputationModifier = new()
    {
        { RelationshipLevel.Stranger,     0.0f  },
        { RelationshipLevel.Acquaintance, 0.1f  },
        { RelationshipLevel.Friend,       0.3f  },
        { RelationshipLevel.GoodFriend,   0.6f  },
        { RelationshipLevel.BestFriend,   1.0f  }
    };

    // Passive points earned per real-world second while relationship is active
    private const float PASSIVE_POINTS_PER_SECOND = 0.05f;

    public void Initialize(World world)
    {
        _timeSystem = world.GetSharedResource<TimeSystem>("TimeSystem");
        Console.WriteLine("[Relationship] Relationship system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        int currentDay = GetCurrentDay();

        // Detect day rollover and reset daily gifts
        if (currentDay != _lastDay && _lastDay != -1)
        {
            OnNewDay(world);
        }
        _lastDay = currentDay;

        // Passive relationship point accumulation for entities with active relationships
        foreach (var entity in world.GetEntitiesWithComponent<RelationshipComponent>())
        {
            var rel = world.GetComponent<RelationshipComponent>(entity);
            if (rel == null) continue;

            AccumulatePassivePoints(rel, deltaTime);
        }
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Give a gift to an NPC. Returns true on success.
    /// Daily gift limit enforced by RelationshipComponent.
    /// </summary>
    public static bool GiveGift(World world, Entity playerEntity, string npcName,
        TileType item, int giftValue)
    {
        var rel = world.GetComponent<RelationshipComponent>(playerEntity);
        var inventory = world.GetComponent<InventoryComponent>(playerEntity);

        if (rel == null || inventory == null) return false;
        if (!inventory.HasItem(item, 1)) return false;

        bool success = rel.GiveGift(npcName, item, giftValue);
        if (success)
        {
            inventory.RemoveItem(item, 1);
            var level = rel.GetLevel(npcName);
            Console.WriteLine($"[Relationship] {npcName}: {giftValue} gift points (+{giftValue}pts) → {level}");

            // Notify quest tracker (Social quests)
            QuestTrackerSystem.NotifyNpcInteracted(npcName);
        }
        else
        {
            Console.WriteLine($"[Relationship] Daily gift limit reached for {npcName}.");
        }

        return success;
    }

    /// <summary>
    /// Talk to an NPC, granting a small relationship boost.
    /// </summary>
    public static void TalkToNPC(World world, Entity playerEntity, string npcName, int points = 5)
    {
        var rel = world.GetComponent<RelationshipComponent>(playerEntity);
        if (rel == null) return;

        rel.Interact(npcName, points);
        Console.WriteLine($"[Relationship] Talked to {npcName}: +{points}pts → {rel.GetLevel(npcName)}");
        QuestTrackerSystem.NotifyNpcInteracted(npcName);
    }

    /// <summary>
    /// Returns the TradingSystem reputation modifier [-1, 1] for the given NPC.
    /// Stranger = 0.0, BestFriend = 1.0. Neutral default is 0.
    /// </summary>
    public static float GetTradeReputation(World world, Entity playerEntity, string npcName)
    {
        var rel = world.GetComponent<RelationshipComponent>(playerEntity);
        if (rel == null) return 0f;

        var level = rel.GetLevel(npcName);
        return ReputationModifier.TryGetValue(level, out float mod) ? mod : 0f;
    }

    /// <summary>
    /// Returns a description of the unlock state for dialogue tiers.
    /// </summary>
    public static string GetDialogueTier(World world, Entity playerEntity, string npcName)
    {
        var rel = world.GetComponent<RelationshipComponent>(playerEntity);
        if (rel == null) return "Stranger";

        return rel.GetLevel(npcName) switch
        {
            RelationshipLevel.Stranger     => "Basic greeting only",
            RelationshipLevel.Acquaintance => "Small talk unlocked",
            RelationshipLevel.Friend       => "Personal topics unlocked",
            RelationshipLevel.GoodFriend   => "Backstory revealed",
            RelationshipLevel.BestFriend   => "Secret quest unlocked",
            _                              => "Unknown"
        };
    }

    /// <summary>
    /// Print a summary of all relationships for the given player entity.
    /// </summary>
    public static void DisplayRelationships(World world, Entity playerEntity)
    {
        var rel = world.GetComponent<RelationshipComponent>(playerEntity);
        if (rel == null)
        {
            Console.WriteLine("[Relationship] No relationship data.");
            return;
        }

        var all = rel.GetAllRelationships();
        if (all.Count == 0)
        {
            Console.WriteLine("[Relationship] No NPC relationships yet.");
            return;
        }

        Console.WriteLine("\n=== NPC Relationships ===");
        foreach (var (name, relationship) in all)
        {
            Console.WriteLine($"  {name,-20} {relationship.Level,-14} ({relationship.Points} pts) " +
                              $" Gifts today: {relationship.GiftsGivenToday}/{2}");
        }
        Console.WriteLine("=========================\n");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private int GetCurrentDay()
    {
        if (_timeSystem != null)
            return _timeSystem.DayCount;
        return DateTime.Now.DayOfYear;
    }

    private static void OnNewDay(World world)
    {
        Console.WriteLine("[Relationship] New day — resetting daily gift counters.");
        foreach (var entity in world.GetEntitiesWithComponent<RelationshipComponent>())
        {
            var rel = world.GetComponent<RelationshipComponent>(entity);
            rel?.ResetDailyGifts();
        }
    }

    private static void AccumulatePassivePoints(RelationshipComponent rel, float deltaTime)
    {
        float points = PASSIVE_POINTS_PER_SECOND * deltaTime;
        if (points < 0.01f) return; // Avoid tiny fractional updates

        foreach (var relationship in rel.GetAllRelationships().Values)
        {
            if (relationship.Level == RelationshipLevel.Stranger) continue; // No passive for strangers
            relationship.AddPoints((int)MathF.Round(points));
        }
    }
}
