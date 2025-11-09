using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System managing quest progression and completion
/// </summary>
public class QuestSystem : ISystem
{
    private const int KEY_Q = 113; // 'q' key for quest log
    private float lastKeyPressTime = 0;
    private float keyPressCooldown = 0.3f;
    
    public void Initialize(World world)
    {
        Console.WriteLine("[Quest] Quest system initialized");
    }
    
    public void Update(World world, float deltaTime)
    {
        lastKeyPressTime += deltaTime;
        
        // Check for quest log display
        if (EngineInterop.Input_IsKeyPressed(KEY_Q) && lastKeyPressTime >= keyPressCooldown)
        {
            DisplayQuestLog(world);
            lastKeyPressTime = 0;
        }
        
        // Update quest progress based on game events
        UpdateQuestProgress(world, deltaTime);
    }
    
    /// <summary>
    /// Display active quests
    /// </summary>
    private void DisplayQuestLog(World world)
    {
        foreach (var entity in world.GetEntitiesWithComponent<QuestComponent>())
        {
            var quests = world.GetComponent<QuestComponent>(entity);
            if (quests != null && quests.ActiveQuests.Count > 0)
            {
                Console.WriteLine("\n=== QUEST LOG ===");
                foreach (var quest in quests.ActiveQuests)
                {
                    Console.WriteLine($"[{quest.Type}] {quest.Name}");
                    Console.WriteLine($"  {quest.Description}");
                    Console.WriteLine($"  Progress: {quest.CurrentProgress}/{quest.RequiredProgress}");
                    if (quest.IsComplete())
                    {
                        Console.WriteLine("  STATUS: READY TO COMPLETE!");
                    }
                }
                Console.WriteLine("=================\n");
            }
        }
    }
    
    /// <summary>
    /// Update quest progress based on player actions
    /// </summary>
    private void UpdateQuestProgress(World world, float deltaTime)
    {
        // Check for combat quest completion
        CheckCombatQuests(world);
        
        // Check for gathering quest completion
        CheckGatheringQuests(world);
        
        // Check for exploration quest completion
        CheckExplorationQuests(world);
    }
    
    /// <summary>
    /// Check and update combat-related quests
    /// </summary>
    private void CheckCombatQuests(World world)
    {
        foreach (var entity in world.GetEntitiesWithComponent<QuestComponent>())
        {
            var quests = world.GetComponent<QuestComponent>(entity);
            if (quests == null) continue;
            
            foreach (var quest in quests.ActiveQuests.Where(q => q.Type == QuestType.Combat))
            {
                // Count defeated enemies (would need enemy tracking in real implementation)
                // For now, just placeholder logic
            }
        }
    }
    
    /// <summary>
    /// Check and update gathering-related quests
    /// </summary>
    private void CheckGatheringQuests(World world)
    {
        foreach (var entity in world.GetEntitiesWithComponent<QuestComponent>())
        {
            var quests = world.GetComponent<QuestComponent>(entity);
            var inventory = world.GetComponent<InventoryComponent>(entity);
            
            if (quests == null || inventory == null) continue;
            
            foreach (var quest in quests.ActiveQuests.Where(q => q.Type == QuestType.Gathering))
            {
                // Check if player has required items in inventory
                // Update progress based on item counts
            }
        }
    }
    
    /// <summary>
    /// Check and update exploration-related quests
    /// </summary>
    private void CheckExplorationQuests(World world)
    {
        foreach (var entity in world.GetEntitiesWithComponent<QuestComponent>())
        {
            var quests = world.GetComponent<QuestComponent>(entity);
            if (quests == null) continue;
            
            foreach (var quest in quests.ActiveQuests.Where(q => q.Type == QuestType.Exploration))
            {
                // Check if player has discovered required locations
                // Would integrate with world exploration tracking
            }
        }
    }
    
    /// <summary>
    /// Award quest completion rewards to player
    /// </summary>
    public static void CompleteQuest(World world, Entity playerEntity, Quest quest)
    {
        // Award gold
        var currency = world.GetComponent<CurrencyComponent>(playerEntity);
        if (currency != null && quest.GoldReward > 0)
        {
            currency.AddGold(quest.GoldReward);
            Console.WriteLine($"[Quest] Received {quest.GoldReward} gold!");
        }
        
        // Award items
        var inventory = world.GetComponent<InventoryComponent>(playerEntity);
        if (inventory != null && quest.ItemRewards.Count > 0)
        {
            foreach (var (item, quantity) in quest.ItemRewards)
            {
                inventory.AddItem(item, quantity);
                Console.WriteLine($"[Quest] Received {quantity}x {item}!");
            }
        }
        
        // Unlock abilities
        var abilities = world.GetComponent<AbilityComponent>(playerEntity);
        if (abilities != null && quest.UnlockAbility != null)
        {
            // Parse ability name and unlock
            if (Enum.TryParse<AbilityType>(quest.UnlockAbility, out var abilityType))
            {
                abilities.UnlockAbility(abilityType);
                Console.WriteLine($"[Quest] Unlocked ability: {quest.UnlockAbility}!");
            }
        }
        
        // Unlock areas would be handled by world progression system
        if (quest.UnlockArea != null)
        {
            Console.WriteLine($"[Quest] New area unlocked: {quest.UnlockArea}!");
        }
        
        Console.WriteLine($"[Quest] Quest completed: {quest.Name}");
    }
}
