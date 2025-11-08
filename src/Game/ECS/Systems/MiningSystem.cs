using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// System that handles block mining/digging and resource collection
/// </summary>
public class MiningSystem : ISystem
{
    private const float MINING_REACH = 3.0f; // How far the player can mine (in blocks)
    
    private Entity? currentMiningEntity = null;
    private int currentMiningX = 0;
    private int currentMiningY = 0;
    private float miningProgress = 0f;
    private float requiredMiningTime = 0f;
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Get the chunk manager from the world (assumes it's stored somewhere accessible)
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
        
        // Get tool (default to bare hands if no tool component)
        ToolComponent currentTool = tool ?? ToolComponent.BareHands();
        
        // For now, we'll use keyboard input instead of mouse until we implement mouse handling
        // Press 'M' key to mine
        const int KEY_M = 77;
        if (EngineInterop.Input_IsKeyDown(KEY_M))
        {
            // Get mouse position in world coordinates
            // For now, we'll use a simplified approach - mine the block the player is standing on/near
            // In a real implementation, you'd convert mouse screen coordinates to world coordinates
            
            // Convert player position to block coordinates
            int playerBlockX = (int)(playerPos.X / 32); // Assuming 32 pixels per block
            int playerBlockY = (int)(playerPos.Y / 32);
            
            // For simplicity, try to mine the block below the player
            int targetX = playerBlockX;
            int targetY = playerBlockY + 1; // Block below player
            
            // Check if we can reach this block
            float distance = MathF.Sqrt(
                MathF.Pow(targetX * 32 - playerPos.X, 2) + 
                MathF.Pow(targetY * 32 - playerPos.Y, 2)
            );
            
            if (distance > MINING_REACH * 32)
            {
                // Block is too far away
                ResetMining();
                return;
            }
            
            // Get the block at this position
            var block = GetBlockAt(chunkManager, targetX, targetY);
            
            if (!block.IsMineable())
            {
                // Can't mine this block
                ResetMining();
                return;
            }
            
            // Check if this is a new block or continuing mining
            if (currentMiningEntity == null || currentMiningX != targetX || currentMiningY != targetY)
            {
                // Starting to mine a new block
                StartMining(playerEntity.Value, targetX, targetY, block, currentTool);
            }
            
            // Continue mining
            miningProgress += deltaTime;
            
            // Check if mining is complete
            if (miningProgress >= requiredMiningTime)
            {
                // Mine the block!
                CompleteMining(world, chunkManager, inventory, block, targetX, targetY);
                ResetMining();
            }
        }
        else
        {
            // Player released key, reset mining
            ResetMining();
        }
    }
    
    private void StartMining(Entity entity, int x, int y, TileType block, ToolComponent tool)
    {
        currentMiningEntity = entity;
        currentMiningX = x;
        currentMiningY = y;
        miningProgress = 0f;
        
        // Calculate required mining time based on block hardness and tool
        float blockHardness = block.GetHardness();
        
        // Check if player has the right tool type
        bool hasCorrectTool = tool.Type == block.GetRequiredToolType() || 
                             block.GetRequiredToolType() == ToolType.None;
        
        // Check if tool material is sufficient
        bool hasStrongEnoughTool = tool.Material >= block.GetMinimumToolMaterial();
        
        float miningSpeed = 1.0f;
        
        if (hasCorrectTool && hasStrongEnoughTool)
        {
            // Use tool mining power
            miningSpeed = tool.MiningPower;
        }
        else if (!hasStrongEnoughTool)
        {
            // Tool not strong enough - mining is much slower or impossible
            miningSpeed = 0.1f;
        }
        else
        {
            // Wrong tool type - mining is slower
            miningSpeed = 0.5f;
        }
        
        requiredMiningTime = blockHardness / miningSpeed;
    }
    
    private void CompleteMining(World world, ChunkManager chunkManager, 
                               InventoryComponent inventory, TileType block, int x, int y)
    {
        // Get the dropped item
        TileType droppedItem = block.GetDroppedItem();
        int quantity = block.GetDropQuantity();
        
        // Add to inventory
        if (quantity > 0 && droppedItem != TileType.Air)
        {
            bool added = inventory.AddItem(droppedItem, quantity);
            if (added)
            {
                Console.WriteLine($"[Mining] Collected {quantity}x {droppedItem}");
            }
            else
            {
                Console.WriteLine("[Mining] Inventory full!");
            }
        }
        
        // Remove the block from the world
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
        
        // Check vegetation first (if on surface)
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
        
        // Check if we're removing vegetation
        if (worldY < Chunk.SURFACE_HEIGHT)
        {
            var vegetation = chunk.GetVegetation(localX);
            if (vegetation.HasValue && vegetation.Value != TileType.Air)
            {
                // Clear vegetation
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
}
