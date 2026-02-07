using System;
using System.Collections.Generic;
using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that handles loot drops from defeated enemies and item pickup
/// </summary>
public class LootDropSystem : ISystem
{
    private Random random;
    private const float PICKUP_RADIUS = 64f; // 2 blocks * 32 pixels per block
    private Queue<PendingLootDrop> pendingDrops;
    
    private class PendingLootDrop
    {
        public float X { get; set; }
        public float Y { get; set; }
        public LootDropComponent LootTable { get; set; }
        
        public PendingLootDrop(float x, float y, LootDropComponent lootTable)
        {
            X = x;
            Y = y;
            LootTable = lootTable;
        }
    }
    
    public LootDropSystem(int seed = 0)
    {
        random = seed == 0 ? new Random() : new Random(seed);
        pendingDrops = new Queue<PendingLootDrop>();
    }
    
    public void Initialize(World world)
    {
        Console.WriteLine("[LootDropSystem] Initialized");
    }
    
    public void Update(World world, float deltaTime)
    {
        // Process any pending drops
        ProcessPendingDrops(world);
        
        // Update dropped item timers and handle pickup
        UpdateDroppedItems(world, deltaTime);
    }
    
    /// <summary>
    /// Queues a loot drop at the specified position
    /// </summary>
    public void QueueLootDrop(float x, float y, LootDropComponent lootTable)
    {
        pendingDrops.Enqueue(new PendingLootDrop(x, y, lootTable));
    }
    
    private void ProcessPendingDrops(World world)
    {
        while (pendingDrops.Count > 0)
        {
            var drop = pendingDrops.Dequeue();
            GenerateLoot(world, drop.X, drop.Y, drop.LootTable);
        }
    }
    
    private void GenerateLoot(World world, float x, float y, LootDropComponent lootTable)
    {
        var droppedItems = new List<string>();
        int totalGold = 0;
        
        // Generate gold drop
        if (lootTable.GoldDropChance > 0 && RollChance(lootTable.GoldDropChance))
        {
            int goldAmount = random.Next(lootTable.MinGold, lootTable.MaxGold + 1);
            if (goldAmount > 0)
            {
                SpawnDroppedGold(world, x, y, goldAmount);
                totalGold = goldAmount;
                droppedItems.Add($"{goldAmount} Gold");
            }
        }
        
        // Generate item drops
        foreach (var lootItem in lootTable.PossibleLoot)
        {
            if (RollChance(lootItem.DropChance))
            {
                int quantity = random.Next(lootItem.MinQuantity, lootItem.MaxQuantity + 1);
                if (quantity > 0)
                {
                    SpawnDroppedItem(world, x, y, lootItem.ItemType, quantity);
                    droppedItems.Add($"{quantity}x {lootItem.ItemType}");
                }
            }
        }
        
        // Display drop message
        if (droppedItems.Count > 0)
        {
            Console.WriteLine($"[LootDropSystem] Enemy dropped: {string.Join(", ", droppedItems)}");
        }
    }
    
    private bool RollChance(float chance)
    {
        return random.NextDouble() * 100.0 < chance;
    }
    
    private void SpawnDroppedItem(World world, float x, float y, TileType itemType, int quantity)
    {
        var itemEntity = world.CreateEntity();
        world.AddComponent(itemEntity, new DroppedItemComponent(itemType, quantity));
        world.AddComponent(itemEntity, new PositionComponent(x, y));
        world.AddComponent(itemEntity, new SpriteComponent(4, 16, 16)); // Small item sprite
    }
    
    private void SpawnDroppedGold(World world, float x, float y, int amount)
    {
        var goldEntity = world.CreateEntity();
        world.AddComponent(goldEntity, new DroppedItemComponent(amount));
        world.AddComponent(goldEntity, new PositionComponent(x, y));
        world.AddComponent(goldEntity, new SpriteComponent(5, 16, 16)); // Gold sprite
    }
    
    private void UpdateDroppedItems(World world, float deltaTime)
    {
        var itemsToRemove = new List<Entity>();
        
        foreach (var itemEntity in world.GetEntitiesWithComponent<DroppedItemComponent>())
        {
            var droppedItem = world.GetComponent<DroppedItemComponent>(itemEntity);
            var itemPosition = world.GetComponent<PositionComponent>(itemEntity);
            
            if (droppedItem == null || itemPosition == null)
                continue;
            
            // Update expiration timer
            droppedItem.ExpirationTimer -= deltaTime;
            
            if (droppedItem.HasExpired)
            {
                itemsToRemove.Add(itemEntity);
                continue;
            }
            
            // Check for player pickup
            foreach (var playerEntity in world.GetEntitiesWithComponent<PlayerComponent>())
            {
                var playerPosition = world.GetComponent<PositionComponent>(playerEntity);
                if (playerPosition == null)
                    continue;
                
                float distance = MathF.Sqrt(
                    MathF.Pow(playerPosition.X - itemPosition.X, 2) +
                    MathF.Pow(playerPosition.Y - itemPosition.Y, 2)
                );
                
                if (distance <= PICKUP_RADIUS)
                {
                    if (TryPickupItem(world, playerEntity, droppedItem))
                    {
                        itemsToRemove.Add(itemEntity);
                    }
                }
            }
        }
        
        // Remove picked up or expired items
        foreach (var item in itemsToRemove)
        {
            world.DestroyEntity(item);
        }
    }
    
    private bool TryPickupItem(World world, Entity playerEntity, DroppedItemComponent droppedItem)
    {
        var pickupMessages = new List<string>();
        
        // Handle gold pickup
        if (droppedItem.IsGold)
        {
            var currency = world.GetComponent<CurrencyComponent>(playerEntity);
            if (currency != null)
            {
                currency.AddGold(droppedItem.GoldAmount);
                pickupMessages.Add($"+{droppedItem.GoldAmount} Gold");
            }
        }
        else
        {
            // Handle item pickup
            var inventory = world.GetComponent<InventoryComponent>(playerEntity);
            if (inventory != null)
            {
                if (inventory.AddItem(droppedItem.ItemType, droppedItem.Quantity))
                {
                    pickupMessages.Add($"{droppedItem.Quantity}x {droppedItem.ItemType}");
                }
                else
                {
                    Console.WriteLine("[LootDropSystem] Inventory full! Cannot pick up item.");
                    return false;
                }
            }
        }
        
        // Display pickup message
        if (pickupMessages.Count > 0)
        {
            Console.WriteLine($"[LootDropSystem] Picked up: {string.Join(", ", pickupMessages)}");
        }
        
        return true;
    }
}
