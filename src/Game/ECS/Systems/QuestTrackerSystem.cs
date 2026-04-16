using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Tracks quest events fired from other systems and updates quest progress
/// accordingly. Unlike the base <see cref="QuestSystem"/> (which contains the
/// display/UI logic), this system contains the event-driven progress backbone.
///
/// Other systems call the static Notify* helpers (e.g. NotifyEnemyKilled).
/// The tracker queues the events and drains them each frame.
/// </summary>
public class QuestTrackerSystem : ISystem
{
    // -----------------------------------------------------------------------
    // Event definitions
    // -----------------------------------------------------------------------

    public enum EventType
    {
        EnemyKilled,
        ItemCollected,
        LocationDiscovered,
        ItemCrafted,
        ItemDelivered,
        CropHarvested,
        BossDefeated,
        NpcInteracted
    }

    private record QuestEvent(EventType Type, string? Tag, int Count);

    // Thread-safe queue: other systems post to this queue, we drain it each Update
    private static readonly Queue<QuestEvent> _pendingEvents = new();
    private static readonly object _lock = new();

    // -----------------------------------------------------------------------
    // Static notification API (called by other systems)
    // -----------------------------------------------------------------------

    /// <summary>Notify that an enemy of the given creature type was killed.</summary>
    public static void NotifyEnemyKilled(CreatureType type, int count = 1)
        => Enqueue(new QuestEvent(EventType.EnemyKilled, type.ToString(), count));

    /// <summary>Notify that the player collected an item.</summary>
    public static void NotifyItemCollected(TileType item, int count = 1)
        => Enqueue(new QuestEvent(EventType.ItemCollected, item.ToString(), count));

    /// <summary>Notify that the player discovered a named location or biome.</summary>
    public static void NotifyLocationDiscovered(string locationName)
        => Enqueue(new QuestEvent(EventType.LocationDiscovered, locationName, 1));

    /// <summary>Notify that the player crafted an item.</summary>
    public static void NotifyItemCrafted(TileType item, int count = 1)
        => Enqueue(new QuestEvent(EventType.ItemCrafted, item.ToString(), count));

    /// <summary>Notify that the player delivered an item to an NPC.</summary>
    public static void NotifyItemDelivered(TileType item, int count = 1)
        => Enqueue(new QuestEvent(EventType.ItemDelivered, item.ToString(), count));

    /// <summary>Notify that the player harvested a crop.</summary>
    public static void NotifyCropHarvested(TileType crop, int count = 1)
        => Enqueue(new QuestEvent(EventType.CropHarvested, crop.ToString(), count));

    /// <summary>Notify that a boss was defeated.</summary>
    public static void NotifyBossDefeated(string bossName)
        => Enqueue(new QuestEvent(EventType.BossDefeated, bossName, 1));

    /// <summary>Notify that the player interacted with a named NPC.</summary>
    public static void NotifyNpcInteracted(string npcName)
        => Enqueue(new QuestEvent(EventType.NpcInteracted, npcName, 1));

    // -----------------------------------------------------------------------
    // ISystem implementation
    // -----------------------------------------------------------------------

    public void Initialize(World world)
    {
        Console.WriteLine("[QuestTracker] Enhanced quest tracker initialized");
    }

    public void Update(World world, float deltaTime)
    {
        // Drain event queue
        List<QuestEvent> events;
        lock (_lock)
        {
            if (_pendingEvents.Count == 0) return;
            events = new List<QuestEvent>(_pendingEvents);
            _pendingEvents.Clear();
        }

        // Find player quest components
        foreach (var playerEntity in world.GetEntitiesWithComponent<QuestComponent>())
        {
            var questComp = world.GetComponent<QuestComponent>(playerEntity);
            if (questComp == null) continue;

            foreach (var evt in events)
                ProcessEvent(world, playerEntity, questComp, evt);
        }
    }

    // -----------------------------------------------------------------------
    // Event processing
    // -----------------------------------------------------------------------

    private static void ProcessEvent(World world, Entity playerEntity, QuestComponent quests, QuestEvent evt)
    {
        foreach (var quest in quests.ActiveQuests.ToList())
        {
            if (quest.Status != QuestStatus.Active) continue;

            bool progressed = false;
            switch (evt.Type)
            {
                case EventType.EnemyKilled when quest.Type == QuestType.Combat:
                    progressed = MatchTag(quest, evt.Tag);
                    break;

                case EventType.ItemCollected when quest.Type == QuestType.Gathering:
                    progressed = MatchTag(quest, evt.Tag);
                    break;

                case EventType.LocationDiscovered when quest.Type == QuestType.Exploration:
                    progressed = MatchTag(quest, evt.Tag);
                    break;

                case EventType.ItemCrafted when quest.Type == QuestType.Crafting:
                    progressed = MatchTag(quest, evt.Tag);
                    break;

                case EventType.ItemDelivered when quest.Type == QuestType.Delivery:
                    progressed = MatchTag(quest, evt.Tag);
                    break;

                case EventType.CropHarvested when quest.Type == QuestType.Farming:
                    progressed = MatchTag(quest, evt.Tag);
                    break;

                case EventType.BossDefeated when quest.Type == QuestType.Combat:
                    progressed = MatchTag(quest, evt.Tag);
                    break;

                case EventType.NpcInteracted when quest.Type == QuestType.Social:
                    progressed = MatchTag(quest, evt.Tag);
                    break;
            }

            if (progressed)
            {
                int before = quest.CurrentProgress;
                quest.UpdateProgress(evt.Count);

                if (quest.CurrentProgress != before)
                    Console.WriteLine($"[QuestTracker] {quest.Name}: {quest.CurrentProgress}/{quest.RequiredProgress}");

                if (quest.IsComplete())
                {
                    Console.WriteLine($"[QuestTracker] Quest complete: {quest.Name}! Press C to claim reward.");
                    AutoCompleteRewards(world, playerEntity, quests, quest);
                }
            }
        }
    }

    /// <summary>
    /// Returns true if the quest has no specific target tag, or if its description
    /// contains the tag (case-insensitive). This allows quests like
    /// "Kill 5 Goblins" to match EnemyKilled events where Tag == "Goblin".
    /// </summary>
    private static bool MatchTag(Quest quest, string? tag)
    {
        if (string.IsNullOrEmpty(tag)) return true;
        // Match if quest description or name mentions the tag
        return quest.Description.Contains(tag, StringComparison.OrdinalIgnoreCase)
            || quest.Name.Contains(tag, StringComparison.OrdinalIgnoreCase)
            || tag.Contains(quest.Name, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Automatically grant rewards when a quest completes, then move it to the
    /// completed list (mirrors QuestSystem.CompleteQuest logic).
    /// </summary>
    private static void AutoCompleteRewards(World world, Entity playerEntity, QuestComponent quests, Quest quest)
    {
        var completed = quests.CompleteQuest(quest.Id);
        if (completed != null)
            QuestSystem.CompleteQuest(world, playerEntity, completed);
    }

    // -----------------------------------------------------------------------
    // Helper
    // -----------------------------------------------------------------------

    private static void Enqueue(QuestEvent evt)
    {
        lock (_lock)
            _pendingEvents.Enqueue(evt);
    }

    // -----------------------------------------------------------------------
    // Pre-built quest factory
    // -----------------------------------------------------------------------

    /// <summary>
    /// Returns a set of starter quests suitable for a new player.
    /// </summary>
    public static List<Quest> CreateStarterQuests()
    {
        return new List<Quest>
        {
            new Quest("starter_gather_wood", "Gather Wood", "Collect 10 pieces of wood from oak trees.", QuestType.Gathering)
            {
                RequiredProgress = 10,
                GoldReward = 20,
                ExperienceReward = 50
            },
            new Quest("starter_kill_goblins", "Goblin Trouble", "Defeat 5 Goblins threatening the village.", QuestType.Combat)
            {
                RequiredProgress = 5,
                GoldReward = 50,
                ExperienceReward = 100
            },
            new Quest("starter_explore", "Explore the World", "Discover 3 different biome locations.", QuestType.Exploration)
            {
                RequiredProgress = 3,
                GoldReward = 30,
                ExperienceReward = 75
            },
            new Quest("starter_farm", "Green Thumb", "Harvest 5 crops from your farm.", QuestType.Farming)
            {
                RequiredProgress = 5,
                GoldReward = 25,
                ExperienceReward = 60
            },
            new Quest("starter_craft", "Crafting Basics", "Craft 3 items at a workbench.", QuestType.Crafting)
            {
                RequiredProgress = 3,
                GoldReward = 15,
                ExperienceReward = 40
            }
        };
    }
}
