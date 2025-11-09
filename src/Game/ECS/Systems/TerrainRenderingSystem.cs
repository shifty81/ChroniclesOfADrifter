using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Renders terrain from ChunkManager using the C++ rendering engine.
/// Integrates procedurally generated world with visual output.
/// </summary>
public class TerrainRenderingSystem : ISystem
{
    private const int BlockSize = 32; // Size of each block in pixels
    private ChunkManager? chunkManager;
    private bool lightingEnabled = true;
    
    public void Initialize(World world)
    {
        // Get chunk manager from world shared resources
        chunkManager = world.GetSharedResource<ChunkManager>("ChunkManager");
        
        if (chunkManager == null)
        {
            Console.WriteLine("[TerrainRendering] WARNING: ChunkManager not found in world resources!");
            Console.WriteLine("[TerrainRendering] Terrain will not be rendered. Add ChunkManager to world.SetSharedResource()");
        }
    }
    
    public void Update(World world, float deltaTime)
    {
        if (chunkManager == null)
        {
            return;
        }
        
        // Get active camera
        CameraComponent? camera = null;
        Entity? playerEntity = null;
        
        foreach (var entity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            camera = world.GetComponent<CameraComponent>(entity);
            if (camera != null)
            {
                var cameraPos = world.GetComponent<PositionComponent>(entity);
                if (cameraPos != null)
                {
                    camera.X = cameraPos.X;
                    camera.Y = cameraPos.Y;
                }
                break;
            }
        }
        
        // Get player entity for lighting calculations
        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            playerEntity = entity;
            break;
        }
        
        if (camera == null)
        {
            return;
        }
        
        // Clear screen with sky color
        EngineInterop.Renderer_Clear(0.53f, 0.81f, 0.98f, 1.0f);
        
        // Render terrain
        RenderTerrain(world, camera, playerEntity);
        
        // Render entities (player, enemies, items)
        RenderEntities(world, camera);
        
        // Present frame
        EngineInterop.Renderer_Present();
    }
    
    private void RenderTerrain(World world, CameraComponent camera, Entity? playerEntity)
    {
        if (chunkManager == null)
        {
            return;
        }
        
        // Calculate visible area in world coordinates
        float screenLeft = camera.X - (camera.ViewportWidth / (2.0f * camera.Zoom));
        float screenRight = camera.X + (camera.ViewportWidth / (2.0f * camera.Zoom));
        float screenTop = camera.Y - (camera.ViewportHeight / (2.0f * camera.Zoom));
        float screenBottom = camera.Y + (camera.ViewportHeight / (2.0f * camera.Zoom));
        
        // Calculate visible tile range
        int minTileX = Math.Max(0, (int)(screenLeft / BlockSize));
        int maxTileX = (int)(screenRight / BlockSize) + 1;
        int minTileY = Math.Max(0, (int)(screenTop / BlockSize));
        int maxTileY = Math.Min(Chunk.ChunkHeight - 1, (int)(screenBottom / BlockSize) + 1);
        
        // Get player position for lighting
        float playerWorldX = 0;
        float playerWorldY = 0;
        if (playerEntity.HasValue)
        {
            var playerPos = world.GetComponent<PositionComponent>(playerEntity.Value);
            if (playerPos != null)
            {
                playerWorldX = playerPos.X;
                playerWorldY = playerPos.Y;
            }
        }
        
        // Get lighting system if available
        LightingSystem? lightingSystem = null;
        if (lightingEnabled)
        {
            lightingSystem = world.GetSystem<LightingSystem>();
        }
        
        // Render visible chunks
        for (int worldX = minTileX; worldX <= maxTileX; worldX++)
        {
            for (int worldY = minTileY; worldY <= maxTileY; worldY++)
            {
                // Calculate chunk coordinate
                int chunkX = worldX / Chunk.ChunkWidth;
                int localX = worldX % Chunk.ChunkWidth;
                
                // Get chunk
                var chunk = chunkManager.GetChunk(chunkX);
                if (chunk == null)
                {
                    continue; // Chunk not loaded yet
                }
                
                // Get tile
                var tile = chunk.GetTile(localX, worldY);
                if (tile == TileType.Air)
                {
                    continue; // Don't render air
                }
                
                // Calculate screen position
                float worldPosX = worldX * BlockSize;
                float worldPosY = worldY * BlockSize;
                var (screenX, screenY) = camera.WorldToScreen(worldPosX, worldPosY);
                float size = BlockSize * camera.Zoom;
                
                // Get tile color
                var (r, g, b) = GetTileColor(tile);
                
                // Apply lighting if enabled
                if (lightingSystem != null)
                {
                    float lightLevel = lightingSystem.GetLightLevel(worldX, worldY);
                    r *= lightLevel;
                    g *= lightLevel;
                    b *= lightLevel;
                }
                
                // Draw tile
                EngineInterop.Renderer_DrawRect(screenX, screenY, size, size, r, g, b, 1.0f);
                
                // Draw biome-specific decorations (grass, flowers) on surface blocks
                if (worldY <= 10 && (tile == TileType.Grass || tile == TileType.Dirt))
                {
                    DrawSurfaceDecoration(worldX, worldY, screenX, screenY, size, chunk);
                }
            }
        }
    }
    
    private void DrawSurfaceDecoration(int worldX, int worldY, float screenX, float screenY, float size, Chunk chunk)
    {
        // Get vegetation at this position
        var vegetation = chunk.GetVegetation(worldX % Chunk.ChunkWidth, worldY);
        
        if (vegetation != VegetationType.None)
        {
            var (r, g, b) = GetVegetationColor(vegetation);
            
            // Draw vegetation on top of tile (smaller, offset)
            float vegSize = size * 0.6f;
            float offsetX = size * 0.2f;
            float offsetY = size * 0.2f;
            
            EngineInterop.Renderer_DrawRect(
                screenX + offsetX, 
                screenY + offsetY, 
                vegSize, 
                vegSize, 
                r, g, b, 1.0f);
        }
    }
    
    private void RenderEntities(World world, CameraComponent camera)
    {
        // Render all entities with sprites
        foreach (var entity in world.GetEntitiesWithComponent<SpriteComponent>())
        {
            var sprite = world.GetComponent<SpriteComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            
            if (sprite != null && position != null)
            {
                // Convert world position to screen position
                var (screenX, screenY) = camera.WorldToScreen(position.X, position.Y);
                
                float width = sprite.Width * camera.Zoom;
                float height = sprite.Height * camera.Zoom;
                
                // Center the sprite
                screenX -= width / 2.0f;
                screenY -= height / 2.0f;
                
                // Get entity color based on type
                var (r, g, b) = GetEntityColor(world, entity);
                
                // Draw entity (for now, as colored rectangle)
                EngineInterop.Renderer_DrawRect(screenX, screenY, width, height, r, g, b, 1.0f);
                
                // Draw black outline (Zelda style)
                float outlineThickness = 2.0f * camera.Zoom;
                EngineInterop.Renderer_DrawRect(screenX - outlineThickness, screenY - outlineThickness, 
                    width + 2 * outlineThickness, outlineThickness, 0, 0, 0, 1.0f); // Top
                EngineInterop.Renderer_DrawRect(screenX - outlineThickness, screenY + height, 
                    width + 2 * outlineThickness, outlineThickness, 0, 0, 0, 1.0f); // Bottom
                EngineInterop.Renderer_DrawRect(screenX - outlineThickness, screenY, 
                    outlineThickness, height, 0, 0, 0, 1.0f); // Left
                EngineInterop.Renderer_DrawRect(screenX + width, screenY, 
                    outlineThickness, height, 0, 0, 0, 1.0f); // Right
            }
        }
    }
    
    private (float r, float g, float b) GetTileColor(TileType tile)
    {
        return tile switch
        {
            // Surface blocks
            TileType.Grass => (0.13f, 0.65f, 0.13f),      // Bright green
            TileType.Dirt => (0.55f, 0.47f, 0.25f),        // Brown dirt
            TileType.Sand => (0.93f, 0.87f, 0.51f),        // Sandy yellow
            TileType.Snow => (0.95f, 0.95f, 1.0f),         // White snow
            
            // Stone blocks
            TileType.Stone => (0.50f, 0.50f, 0.50f),       // Gray stone
            TileType.Sandstone => (0.76f, 0.70f, 0.50f),   // Light tan
            TileType.Limestone => (0.85f, 0.85f, 0.80f),   // Off-white
            
            // Ores
            TileType.CoalOre => (0.20f, 0.20f, 0.20f),     // Dark gray
            TileType.CopperOre => (0.72f, 0.45f, 0.20f),   // Copper orange
            TileType.IronOre => (0.65f, 0.50f, 0.39f),     // Rusty brown
            TileType.GoldOre => (1.0f, 0.84f, 0.0f),       // Gold
            TileType.SilverOre => (0.75f, 0.75f, 0.75f),   // Silver
            TileType.DiamondOre => (0.0f, 0.75f, 1.0f),    // Light blue
            
            // Water
            TileType.Water => (0.20f, 0.60f, 0.85f),       // Blue water
            TileType.DeepWater => (0.10f, 0.40f, 0.70f),   // Dark blue
            
            // Special
            TileType.Torch => (1.0f, 0.80f, 0.20f),        // Warm yellow
            TileType.Wood => (0.55f, 0.35f, 0.16f),        // Brown wood
            TileType.Planks => (0.65f, 0.45f, 0.26f),      // Light brown
            TileType.Bricks => (0.70f, 0.35f, 0.25f),      // Red bricks
            
            // Default
            _ => (0.60f, 0.60f, 0.60f)                     // Gray default
        };
    }
    
    private (float r, float g, float b) GetVegetationColor(VegetationType vegetation)
    {
        return vegetation switch
        {
            VegetationType.Grass => (0.18f, 0.70f, 0.18f),       // Light green
            VegetationType.TallGrass => (0.15f, 0.60f, 0.15f),   // Medium green
            VegetationType.Flower => (1.0f, 0.20f, 0.40f),       // Pink/red
            VegetationType.Bush => (0.10f, 0.50f, 0.10f),        // Dark green
            VegetationType.Cactus => (0.20f, 0.60f, 0.30f),      // Green cactus
            VegetationType.DeadBush => (0.65f, 0.55f, 0.40f),    // Brown
            VegetationType.Reed => (0.30f, 0.70f, 0.30f),        // Swamp green
            _ => (0.20f, 0.60f, 0.20f)                           // Default green
        };
    }
    
    private (float r, float g, float b) GetEntityColor(World world, Entity entity)
    {
        // Check if player
        if (world.HasComponent<PlayerComponent>(entity))
        {
            return (1.0f, 0.84f, 0.0f); // Golden yellow (like Link)
        }
        
        // Check if has combat component (enemy)
        if (world.HasComponent<CombatComponent>(entity) && !world.HasComponent<PlayerComponent>(entity))
        {
            return (0.8f, 0.2f, 0.2f); // Red (enemy)
        }
        
        // Default entity color
        return (0.5f, 0.5f, 0.5f); // Gray
    }
    
    /// <summary>
    /// Enable or disable lighting calculations
    /// </summary>
    public void SetLightingEnabled(bool enabled)
    {
        lightingEnabled = enabled;
    }
}
