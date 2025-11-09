using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.WorldManagement;
using World = ChroniclesOfADrifter.ECS.World;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System managing NPC behavior, schedules, and interactions
/// </summary>
public class NPCSystem : ISystem
{
    private TimeSystem? timeSystem;
    
    public void Initialize(World world)
    {
        Console.WriteLine("[NPC] NPC system initialized");
    }
    
    public void Update(World world, float deltaTime)
    {
        // Get current time from TimeSystem
        if (timeSystem == null)
        {
            // Find TimeSystem (would be passed in or stored globally in real implementation)
            return;
        }
        
        float currentHour = timeSystem.GetCurrentHour();
        
        // Update each NPC's position and activity based on schedule
        foreach (var entity in world.GetEntitiesWithComponent<NPCComponent>())
        {
            var npc = world.GetComponent<NPCComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            
            if (npc != null && position != null)
            {
                UpdateNPCSchedule(npc, position, currentHour);
            }
        }
    }
    
    /// <summary>
    /// Update NPC position and activity based on schedule
    /// </summary>
    private void UpdateNPCSchedule(NPCComponent npc, PositionComponent position, float currentHour)
    {
        var schedule = npc.GetScheduleAt(currentHour);
        
        if (schedule != null)
        {
            // Move NPC to scheduled location (simplified - would use pathfinding)
            float targetX = schedule.LocationX;
            float targetY = schedule.LocationY;
            
            // Lerp towards target location
            float speed = 50f; // pixels per second
            float dx = targetX - position.X;
            float dy = targetY - position.Y;
            float distance = MathF.Sqrt(dx * dx + dy * dy);
            
            if (distance > 5f) // If not at target
            {
                position.X += (dx / distance) * speed * 0.016f; // Approximate deltaTime
                position.Y += (dy / distance) * speed * 0.016f;
            }
            
            npc.CurrentActivity = schedule.Activity;
        }
    }
    
    /// <summary>
    /// Interact with an NPC
    /// </summary>
    public static void InteractWithNPC(World world, Entity playerEntity, Entity npcEntity)
    {
        var npc = world.GetComponent<NPCComponent>(npcEntity);
        if (npc == null) return;
        
        Console.WriteLine($"\n[NPC] {npc.Name}: {npc.GetGreeting()}");
        
        // Handle different NPC roles
        switch (npc.Role)
        {
            case NPCRole.Merchant:
                ShowMerchantMenu(world, playerEntity, npc);
                break;
                
            case NPCRole.Questgiver:
                ShowQuestMenu(world, playerEntity, npc);
                break;
                
            case NPCRole.Healer:
                HealPlayer(world, playerEntity);
                break;
                
            default:
                Console.WriteLine(npc.GetRandomDialogue());
                break;
        }
        
        // Update relationship
        var relationships = world.GetComponent<RelationshipComponent>(playerEntity);
        if (relationships != null)
        {
            relationships.Interact(npc.Name, 5);
        }
    }
    
    /// <summary>
    /// Show merchant shop menu
    /// </summary>
    private static void ShowMerchantMenu(World world, Entity playerEntity, NPCComponent merchant)
    {
        Console.WriteLine($"\n=== {merchant.Name}'s Shop ===");
        
        if (merchant.ShopInventory.Count == 0)
        {
            Console.WriteLine("No items for sale right now.");
            return;
        }
        
        foreach (var (item, quantity) in merchant.ShopInventory)
        {
            int price = merchant.ShopPrices.GetValueOrDefault(item, 10);
            Console.WriteLine($"  {item} x{quantity} - {price} gold");
        }
        
        Console.WriteLine("========================\n");
    }
    
    /// <summary>
    /// Show available quests from questgiver
    /// </summary>
    private static void ShowQuestMenu(World world, Entity playerEntity, NPCComponent questgiver)
    {
        var playerQuests = world.GetComponent<QuestComponent>(playerEntity);
        if (playerQuests == null) return;
        
        Console.WriteLine($"\n=== Quests from {questgiver.Name} ===");
        
        if (questgiver.AvailableQuests.Count == 0)
        {
            Console.WriteLine("No quests available right now.");
            return;
        }
        
        foreach (var quest in questgiver.AvailableQuests)
        {
            if (!playerQuests.HasCompletedQuest(quest.Id))
            {
                Console.WriteLine($"  [{quest.Type}] {quest.Name}");
                Console.WriteLine($"    {quest.Description}");
                Console.WriteLine($"    Reward: {quest.GoldReward} gold");
            }
        }
        
        Console.WriteLine("==============================\n");
    }
    
    /// <summary>
    /// Heal the player
    /// </summary>
    private static void HealPlayer(World world, Entity playerEntity)
    {
        var health = world.GetComponent<HealthComponent>(playerEntity);
        if (health != null)
        {
            health.CurrentHealth = health.MaxHealth;
            Console.WriteLine("[NPC] You have been healed!");
        }
    }
    
    /// <summary>
    /// Buy an item from a merchant
    /// </summary>
    public static bool BuyFromMerchant(World world, Entity playerEntity, NPCComponent merchant, TileType item)
    {
        var currency = world.GetComponent<CurrencyComponent>(playerEntity);
        var inventory = world.GetComponent<InventoryComponent>(playerEntity);
        
        if (currency == null || inventory == null)
            return false;
            
        if (!merchant.ShopInventory.ContainsKey(item) || merchant.ShopInventory[item] <= 0)
        {
            Console.WriteLine("[Shop] Item not in stock!");
            return false;
        }
        
        int price = merchant.ShopPrices.GetValueOrDefault(item, 10);
        
        if (!currency.HasGold(price))
        {
            Console.WriteLine("[Shop] Not enough gold!");
            return false;
        }
        
        // Complete transaction
        currency.RemoveGold(price);
        inventory.AddItem(item, 1);
        merchant.ShopInventory[item]--;
        
        Console.WriteLine($"[Shop] Purchased {item} for {price} gold!");
        return true;
    }
    
    /// <summary>
    /// Sell an item to a merchant
    /// </summary>
    public static bool SellToMerchant(World world, Entity playerEntity, TileType item, int sellPrice)
    {
        var currency = world.GetComponent<CurrencyComponent>(playerEntity);
        var inventory = world.GetComponent<InventoryComponent>(playerEntity);
        
        if (currency == null || inventory == null)
            return false;
            
        if (!inventory.HasItem(item, 1))
        {
            Console.WriteLine("[Shop] You don't have that item!");
            return false;
        }
        
        // Complete transaction
        inventory.RemoveItem(item, 1);
        currency.AddGold(sellPrice);
        
        Console.WriteLine($"[Shop] Sold {item} for {sellPrice} gold!");
        return true;
    }
}
