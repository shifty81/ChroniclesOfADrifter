using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that handles block interactions: mining, placing, and building
/// </summary>
public class BlockInteractionSystem : ISystem
{
    private const float INTERACTION_REACH = 3.0f; // How far the player can interact (in blocks)
    private const int KEY_M = 77;  // Mine/break blocks
    private const int KEY_P = 80;  // Place blocks
    
    private Entity? currentMiningEntity = null;
    private int currentMiningX = 0;
    private int currentMiningY = 0;
    private float miningProgress = 0f;
    private float requiredMiningTime = 0f;
    
    private TileType selectedBlockType = TileType.Dirt; // Block type to place
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        var chunkManager = world.GetSharedResource<ChunkManager>("ChunkManager");
        if (chunkManager == null)
            return;
        
        // Find the player entity
        Entity? playerEntity = null;
        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            playerEntity = entity;
            break;
        }
        
        if (playerEntity == null)
            return;
        
        var playerPos = world.GetComponent<PositionComponent>(playerEntity.Value);
        var inventory = world.GetComponent<InventoryComponent>(playerEntity.Value);
        var tool = world.GetComponent<ToolComponent>(playerEntity.Value);
        
        if (playerPos == null || inventory == null)
            return;
        
        ToolComponent currentTool = tool ?? ToolComponent.BareHands();
        
        // Handle block selection (1-9 keys to select block type from inventory)
        HandleBlockSelection(inventory);
        
        // Mining logic
        if (EngineInterop.Input_IsKeyDown(KEY_M))
        {
            HandleMining(world, chunkManager, playerEntity.Value, playerPos, inventory, currentTool, deltaTime);
        }
        else
        {
            ResetMining();
        }
        
        // Placing logic
        if (EngineInterop.Input_IsKeyPressed(KEY_P))
        {
            HandlePlacement(chunkManager, playerPos, inventory);
        }
    }
    
    private void HandleBlockSelection(InventoryComponent inventory)
    {
        // Keys 1-9 to select block types from inventory
        for (int i = 1; i <= 9; i++)
        {
            int keyCode = 48 + i; // ASCII codes for '1' to '9'
            if (EngineInterop.Input_IsKeyPressed(keyCode))
            {
                // Get the i-th item from inventory
                var items = inventory.GetAllItems();
                if (items.Count >= i)
                {
                    var itemList = new List<TileType>(items.Keys);
                    selectedBlockType = itemList[i - 1];
                    Console.WriteLine($"[BlockInteraction] Selected: {selectedBlockType}");
                }
            }
        }
    }
    
    private void HandleMining(World world, ChunkManager chunkManager, Entity playerEntity,
                             PositionComponent playerPos, InventoryComponent inventory,
                             ToolComponent tool, float deltaTime)
    {
        // Convert player position to block coordinates
        int playerBlockX = (int)(playerPos.X / 32);
        int playerBlockY = (int)(playerPos.Y / 32);
        
        // For simplicity, mine the block below the player
        int targetX = playerBlockX;
        int targetY = playerBlockY + 1;
        
        // Check if we can reach this block
        float distance = MathF.Sqrt(
            MathF.Pow(targetX * 32 - playerPos.X, 2) + 
            MathF.Pow(targetY * 32 - playerPos.Y, 2)
        );
        
        if (distance > INTERACTION_REACH * 32)
        {
            ResetMining();
            return;
        }
        
        var block = GetBlockAt(chunkManager, targetX, targetY);
        
        if (!block.IsMineable())
        {
            ResetMining();
            return;
        }
        
        // Check if this is a new block or continuing mining
        if (currentMiningEntity == null || currentMiningX != targetX || currentMiningY != targetY)
        {
            StartMining(playerEntity, targetX, targetY, block, tool);
        }
        
        // Continue mining
        miningProgress += deltaTime;
        
        // Check if mining is complete
        if (miningProgress >= requiredMiningTime)
        {
            CompleteMining(world, chunkManager, inventory, block, targetX, targetY);
            ResetMining();
        }
    }
    
    private void HandlePlacement(ChunkManager chunkManager, PositionComponent playerPos, InventoryComponent inventory)
    {
        // Check if player has the selected block in inventory
        if (!inventory.HasItem(selectedBlockType, 1))
        {
            Console.WriteLine($"[BlockInteraction] No {selectedBlockType} in inventory!");
            return;
        }
        
        // Convert player position to block coordinates
        int playerBlockX = (int)(playerPos.X / 32);
        int playerBlockY = (int)(playerPos.Y / 32);
        
        // Place block below the player (could be extended to support directional placement)
        int targetX = playerBlockX;
        int targetY = playerBlockY + 1;
        
        // Check if target position is within reach
        float distance = MathF.Sqrt(
            MathF.Pow(targetX * 32 - playerPos.X, 2) + 
            MathF.Pow(targetY * 32 - playerPos.Y, 2)
        );
        
        if (distance > INTERACTION_REACH * 32)
        {
            Console.WriteLine("[BlockInteraction] Too far to place block!");
            return;
        }
        
        // Check if target location is empty (air)
        var existingBlock = GetBlockAt(chunkManager, targetX, targetY);
        if (existingBlock != TileType.Air)
        {
            Console.WriteLine("[BlockInteraction] Cannot place block - space occupied!");
            return;
        }
        
        // Place the block
        SetBlockAt(chunkManager, targetX, targetY, selectedBlockType);
        
        // Remove from inventory
        inventory.RemoveItem(selectedBlockType, 1);
        
        Console.WriteLine($"[BlockInteraction] Placed {selectedBlockType} at ({targetX}, {targetY})");
    }
    
    private void StartMining(Entity entity, int x, int y, TileType block, ToolComponent tool)
    {
        currentMiningEntity = entity;
        currentMiningX = x;
        currentMiningY = y;
        miningProgress = 0f;
        
        float blockHardness = block.GetHardness();
        bool hasCorrectTool = tool.Type == block.GetRequiredToolType() || 
                             block.GetRequiredToolType() == ToolType.None;
        bool hasStrongEnoughTool = tool.Material >= block.GetMinimumToolMaterial();
        
        float miningSpeed = 1.0f;
        
        if (hasCorrectTool && hasStrongEnoughTool)
        {
            miningSpeed = tool.MiningPower;
        }
        else if (!hasStrongEnoughTool)
        {
            miningSpeed = 0.1f; // Tool not strong enough
        }
        else
        {
            miningSpeed = 0.5f; // Wrong tool type
        }
        
        requiredMiningTime = blockHardness / miningSpeed;
    }
    
    private void CompleteMining(World world, ChunkManager chunkManager, 
                               InventoryComponent inventory, TileType block, int x, int y)
    {
        TileType droppedItem = block.GetDroppedItem();
        int quantity = block.GetDropQuantity();
        
        if (quantity > 0 && droppedItem != TileType.Air)
        {
            bool added = inventory.AddItem(droppedItem, quantity);
            if (added)
            {
                Console.WriteLine($"[BlockInteraction] Collected {quantity}x {droppedItem}");
            }
            else
            {
                Console.WriteLine("[BlockInteraction] Inventory full!");
            }
        }
        
        SetBlockAt(chunkManager, x, y, TileType.Air);
    }
    
    private void ResetMining()
    {
        currentMiningEntity = null;
        miningProgress = 0f;
        requiredMiningTime = 0f;
    }
    
    private TileType GetBlockAt(ChunkManager chunkManager, int worldX, int worldY)
    {
        int chunkX = Chunk.WorldToChunkCoord(worldX);
        int localX = Chunk.WorldToLocalCoord(worldX);
        
        var chunk = chunkManager.GetChunk(chunkX);
        if (chunk == null || worldY < 0 || worldY >= Chunk.CHUNK_HEIGHT)
        {
            return TileType.Air;
        }
        
        if (worldY < Chunk.SURFACE_HEIGHT)
        {
            var vegetation = chunk.GetVegetation(localX);
            if (vegetation.HasValue && vegetation.Value != TileType.Air)
            {
                return vegetation.Value;
            }
        }
        
        return chunk.GetTile(localX, worldY);
    }
    
    private void SetBlockAt(ChunkManager chunkManager, int worldX, int worldY, TileType type)
    {
        int chunkX = Chunk.WorldToChunkCoord(worldX);
        int localX = Chunk.WorldToLocalCoord(worldX);
        
        var chunk = chunkManager.GetChunk(chunkX);
        if (chunk == null || worldY < 0 || worldY >= Chunk.CHUNK_HEIGHT)
        {
            return;
        }
        
        if (worldY < Chunk.SURFACE_HEIGHT)
        {
            var vegetation = chunk.GetVegetation(localX);
            if (vegetation.HasValue && vegetation.Value != TileType.Air)
            {
                chunk.SetVegetation(localX, null);
                return;
            }
        }
        
        chunk.SetTile(localX, worldY, type);
    }
    
    /// <summary>
    /// Gets the current mining progress (0.0 to 1.0)
    /// </summary>
    public float GetMiningProgress()
    {
        if (requiredMiningTime <= 0f)
            return 0f;
        return Math.Min(miningProgress / requiredMiningTime, 1.0f);
    }
    
    /// <summary>
    /// Checks if player is currently mining
    /// </summary>
    public bool IsMining()
    {
        return currentMiningEntity != null;
    }
    
    /// <summary>
    /// Gets the currently selected block type for placement
    /// </summary>
    public TileType GetSelectedBlockType()
    {
        return selectedBlockType;
    }
}
