using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Renders terrain blocks visually using the engine's rendering backend
/// Supports both surface and underground rendering with proper lighting
/// </summary>
public class TerrainRenderingSystem : ISystem
{
    private ChunkManager? _chunkManager;
    
    public void Initialize(World world)
    {
        // Get chunk manager from world resources
        _chunkManager = world.GetSharedResource<ChunkManager>("ChunkManager");
    }
    
    public void Update(World world, float deltaTime)
    {
        if (_chunkManager == null) return;
        
        // Get active camera
        CameraComponent? camera = null;
        PositionComponent? cameraPos = null;
        
        foreach (var entity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            camera = world.GetComponent<CameraComponent>(entity);
            cameraPos = world.GetComponent<PositionComponent>(entity);
            if (camera != null && cameraPos != null)
            {
                camera.X = cameraPos.X;
                camera.Y = cameraPos.Y;
                break;
            }
        }
        
        if (camera == null) return;
        
        // Clear screen with sky color
        EngineInterop.Renderer_Clear(0.53f, 0.81f, 0.98f, 1.0f);
        
        // Render visible terrain
        RenderTerrain(camera);
    }
    
    private void RenderTerrain(CameraComponent camera)
    {
        if (_chunkManager == null) return;
        
        // Calculate visible block range
        float screenLeft = camera.X - (camera.ViewportWidth / (2.0f * camera.Zoom));
        float screenRight = camera.X + (camera.ViewportWidth / (2.0f * camera.Zoom));
        float screenTop = camera.Y - (camera.ViewportHeight / (2.0f * camera.Zoom));
        float screenBottom = camera.Y + (camera.ViewportHeight / (2.0f * camera.Zoom));
        
        int minBlockX = Math.Max(0, (int)(screenLeft / GameConstants.BlockSize));
        int maxBlockX = (int)(screenRight / GameConstants.BlockSize) + 1;
        int minBlockY = Math.Max(0, (int)(screenTop / GameConstants.BlockSize));
        int maxBlockY = Math.Min(Chunk.CHUNK_HEIGHT, (int)(screenBottom / GameConstants.BlockSize) + 1);
        
        // Render each visible block
        for (int by = minBlockY; by < maxBlockY; by++)
        {
            for (int bx = minBlockX; bx < maxBlockX; bx++)
            {
                var tileType = _chunkManager.GetTile(bx, by);
                if (tileType == TileType.Air) continue; // Skip air blocks
                
                float worldX = bx * GameConstants.BlockSize;
                float worldY = by * GameConstants.BlockSize;
                
                var (screenX, screenY) = camera.WorldToScreen(worldX, worldY);
                float size = GameConstants.BlockSize * camera.Zoom;
                
                // Get tile color
                var (r, g, b) = GetTileColor(tileType);
                
                // Draw the block
                EngineInterop.Renderer_DrawRect(screenX, screenY, size, size, r, g, b, 1.0f);
            }
        }
    }
    
    private (float r, float g, float b) GetTileColor(TileType tileType)
    {
        return tileType switch
        {
            // Surface blocks
            TileType.Grass => (0.13f, 0.65f, 0.13f),           // Bright green
            TileType.Sand => (0.93f, 0.87f, 0.51f),            // Sandy yellow
            TileType.Snow => (0.97f, 0.97f, 1.00f),            // White snow
            TileType.Dirt => (0.55f, 0.40f, 0.28f),            // Brown dirt
            
            // Water
            TileType.Water => (0.20f, 0.60f, 0.85f),           // Water blue
            
            // Stone and underground
            TileType.Stone => (0.50f, 0.50f, 0.50f),           // Gray stone
            TileType.Bedrock => (0.15f, 0.15f, 0.15f),         // Almost black
            
            // Ores
            TileType.IronOre => (0.65f, 0.50f, 0.39f),         // Rusty brown
            TileType.CopperOre => (0.72f, 0.45f, 0.20f),       // Copper orange
            TileType.GoldOre => (1.00f, 0.84f, 0.00f),         // Gold
            
            // Wood and plants
            TileType.Wood => (0.40f, 0.26f, 0.13f),            // Brown wood
            TileType.WoodPlank => (0.65f, 0.50f, 0.35f),       // Light wood
            
            // Building materials
            TileType.Brick => (0.70f, 0.30f, 0.20f),           // Red brick
            TileType.Glass => (0.90f, 0.95f, 1.00f),           // Clear glass
            TileType.Torch => (1.00f, 0.90f, 0.60f),           // Torch light
            
            // Default
            _ => (0.50f, 0.50f, 0.50f)                         // Gray
        };
    }
}
