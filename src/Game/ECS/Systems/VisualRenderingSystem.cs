using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Visual rendering system using SDL2 for graphical output
/// Demonstrates Zelda: A Link to the Past style tile-based rendering
/// </summary>
public class VisualRenderingSystem : ISystem
{
    private const int TileSize = 32; // Zelda ALTTP used 16x16, we use 32x32 for visibility
    
    public void Initialize(World world)
    {
        // No initialization needed
    }
    
    public void Update(World world, float deltaTime)
    {
        // Clear screen with Zelda-style bright sky blue color
        EngineInterop.Renderer_Clear(0.53f, 0.81f, 0.98f, 1.0f);
        
        // Get active camera
        CameraComponent? camera = null;
        
        foreach (var entity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            camera = world.GetComponent<CameraComponent>(entity);
            var cameraPos = world.GetComponent<PositionComponent>(entity);
            if (camera != null && cameraPos != null)
            {
                camera.X = cameraPos.X;
                camera.Y = cameraPos.Y;
                break;
            }
        }
        
        if (camera == null) return;
        
        // Render tile-based background (Zelda-style overworld)
        RenderTiledBackground(camera);
        
        // Render all entities with sprites
        RenderEntities(world, camera);
        
        // Present the frame
        EngineInterop.Renderer_Present();
    }
    
    private void RenderTiledBackground(CameraComponent camera)
    {
        // Calculate visible tile range based on camera position
        float screenLeft = camera.X - (camera.ViewportWidth / (2.0f * camera.Zoom));
        float screenRight = camera.X + (camera.ViewportWidth / (2.0f * camera.Zoom));
        float screenTop = camera.Y - (camera.ViewportHeight / (2.0f * camera.Zoom));
        float screenBottom = camera.Y + (camera.ViewportHeight / (2.0f * camera.Zoom));
        
        int minTileX = Math.Max(0, (int)(screenLeft / TileSize));
        int maxTileX = (int)(screenRight / TileSize) + 1;
        int minTileY = Math.Max(0, (int)(screenTop / TileSize));
        int maxTileY = (int)(screenBottom / TileSize) + 1;
        
        // Draw tiles with varied terrain (Zelda-style vibrant colors)
        for (int y = minTileY; y < maxTileY; y++)
        {
            for (int x = minTileX; x < maxTileX; x++)
            {
                float worldX = x * TileSize;
                float worldY = y * TileSize;
                
                var (screenX, screenY) = camera.WorldToScreen(worldX, worldY);
                float size = TileSize * camera.Zoom;
                
                // Get tile color based on position (pseudo-random terrain)
                var (r, g, b) = GetTileColor(x, y);
                EngineInterop.Renderer_DrawRect(screenX, screenY, size, size, r, g, b, 1.0f);
            }
        }
    }
    
    private (float r, float g, float b) GetTileColor(int x, int y)
    {
        // Create varied, Zelda-style colorful landscape using pseudo-random pattern
        // This simulates different terrain types: grass, water, dirt paths, rocks
        int pattern = (x * 7 + y * 11) % 10;
        
        return pattern switch
        {
            0 or 1 or 2 => (0.13f, 0.65f, 0.13f),  // Bright green grass (most common)
            3 or 4 => (0.18f, 0.70f, 0.18f),        // Light green grass variation
            5 => (0.55f, 0.47f, 0.25f),              // Light dirt path
            6 => (0.25f, 0.55f, 0.25f),              // Dark green grass
            7 => (0.20f, 0.60f, 0.85f),              // Water (vibrant blue)
            8 => (0.75f, 0.75f, 0.75f),              // Stone/rock (light gray)
            _ => (0.15f, 0.60f, 0.15f)               // Default grass
        };
    }
    
    private void RenderEntities(World world, CameraComponent camera)
    {
        // Render all entities with sprite components
        foreach (var entity in world.GetEntitiesWithComponent<SpriteComponent>())
        {
            var pos = world.GetComponent<PositionComponent>(entity);
            var sprite = world.GetComponent<SpriteComponent>(entity);
            
            if (pos != null && sprite != null)
            {
                var (screenX, screenY) = camera.WorldToScreen(pos.X, pos.Y);
                
                // Center the sprite on the position
                float width = sprite.Width * camera.Zoom;
                float height = sprite.Height * camera.Zoom;
                float renderX = screenX - width / 2;
                float renderY = screenY - height / 2;
                
                // Get entity color (vibrant Zelda-style)
                var (r, g, b) = GetEntityColor(entity, world);
                
                // Draw entity as colored rectangle
                EngineInterop.Renderer_DrawRect(renderX, renderY, width, height, r, g, b, 1.0f);
                
                // Draw black outline for better visibility (Zelda-style sprites had outlines)
                DrawOutline(renderX, renderY, width, height);
            }
        }
    }
    
    private (float r, float g, float b) GetEntityColor(Entity entity, World world)
    {
        // Player is bright golden yellow (like Link)
        if (world.GetComponent<PlayerComponent>(entity) != null)
        {
            return (1.0f, 0.85f, 0.0f); // Bright golden yellow
        }
        
        // Enemies are red
        if (world.GetComponent<ScriptComponent>(entity) != null)
        {
            return (0.95f, 0.0f, 0.0f); // Bright red
        }
        
        // Default white
        return (1.0f, 1.0f, 1.0f);
    }
    
    private void DrawOutline(float x, float y, float width, float height)
    {
        float outlineWidth = 2.0f;
        
        // Top
        EngineInterop.Renderer_DrawRect(x, y, width, outlineWidth, 0.0f, 0.0f, 0.0f, 1.0f);
        // Bottom
        EngineInterop.Renderer_DrawRect(x, y + height - outlineWidth, width, outlineWidth, 0.0f, 0.0f, 0.0f, 1.0f);
        // Left
        EngineInterop.Renderer_DrawRect(x, y, outlineWidth, height, 0.0f, 0.0f, 0.0f, 1.0f);
        // Right
        EngineInterop.Renderer_DrawRect(x + width - outlineWidth, y, outlineWidth, height, 0.0f, 0.0f, 0.0f, 1.0f);
    }
}
