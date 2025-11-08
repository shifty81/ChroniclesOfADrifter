using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.World;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Visual demo scene showing graphical rendering with SDL2
/// </summary>
public class VisualDemoScene : Scene
{
    private Entity _playerEntity;
    private Entity _cameraEntity;
    private ChunkManager? _chunkManager;
    
    public override void OnLoad()
    {
        Console.WriteLine("[VisualDemo] Loading visual demo scene...");
        
        // Register all systems
        World.RegisterSystem(new MovementSystem());
        World.RegisterSystem(new InputSystem());
        World.RegisterSystem(new CameraSystem());
        World.RegisterSystem(new CollisionSystem());
        
        // Create chunk manager for terrain
        _chunkManager = new ChunkManager(seed: 12345, chunkLoadDistance: 4);
        
        // Create player entity
        _playerEntity = World.CreateEntity();
        World.AddComponent(_playerEntity, new PositionComponent { X = 400, Y = 300 });
        World.AddComponent(_playerEntity, new VelocityComponent { VX = 0, VY = 0 });
        World.AddComponent(_playerEntity, new SpriteComponent 
        { 
            Character = '@',
            Color = ConsoleColor.Yellow,
            Width = 32,
            Height = 32
        });
        World.AddComponent(_playerEntity, new PlayerComponent());
        World.AddComponent(_playerEntity, new AABBComponent 
        { 
            Width = 28, 
            Height = 28,
            OffsetX = 2,
            OffsetY = 2
        });
        
        // Create camera entity
        _cameraEntity = World.CreateEntity();
        World.AddComponent(_cameraEntity, new CameraComponent
        {
            X = 400,
            Y = 300,
            Zoom = 1.0f,
            ViewportWidth = 1920,
            ViewportHeight = 1080,
            FollowSpeed = 5.0f
        });
        
        // Set camera to follow player
        CameraSystem.SetFollowTarget(World, _cameraEntity, _playerEntity);
        
        // Set world bounds
        CameraSystem.SetBounds(World, _cameraEntity,
            minX: 0,
            maxX: 2000,
            minY: 0,
            maxY: 2000
        );
        
        Console.WriteLine("[VisualDemo] Visual demo scene loaded!");
        Console.WriteLine("[VisualDemo] Use WASD or Arrow keys to move");
        Console.WriteLine("[VisualDemo] Press Q or ESC to quit");
    }
    
    public override void Update(float deltaTime)
    {
        // Update all systems
        foreach (var system in World.GetSystems())
        {
            system.Update(World, deltaTime);
        }
        
        // Render the scene
        RenderScene();
    }
    
    private void RenderScene()
    {
        // Clear screen with sky blue color
        EngineInterop.Renderer_Clear(0.53f, 0.81f, 0.92f, 1.0f);
        
        // Get camera for world-to-screen transformation
        var camera = World.GetComponent<CameraComponent>(_cameraEntity);
        if (camera == null) return;
        
        // Render terrain chunks
        RenderTerrain(camera);
        
        // Render entities
        RenderEntities(camera);
    }
    
    private void RenderTerrain(CameraComponent camera)
    {
        if (_chunkManager == null) return;
        
        // Calculate visible chunk range based on camera position
        int centerChunkX = (int)(camera.X / (GameConstants.ChunkWidth * GameConstants.BlockSize));
        int centerChunkY = (int)(camera.Y / (GameConstants.ChunkHeight * GameConstants.BlockSize));
        
        // Load chunks around camera
        for (int dy = -2; dy <= 2; dy++)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                int chunkX = centerChunkX + dx;
                int chunkY = centerChunkY + dy;
                _chunkManager.LoadChunk(chunkX, chunkY);
            }
        }
        
        // Render visible terrain blocks
        float screenLeft = camera.X - (camera.ViewportWidth / (2.0f * camera.Zoom));
        float screenRight = camera.X + (camera.ViewportWidth / (2.0f * camera.Zoom));
        float screenTop = camera.Y - (camera.ViewportHeight / (2.0f * camera.Zoom));
        float screenBottom = camera.Y + (camera.ViewportHeight / (2.0f * camera.Zoom));
        
        int minBlockX = Math.Max(0, (int)(screenLeft / GameConstants.BlockSize));
        int maxBlockX = (int)(screenRight / GameConstants.BlockSize) + 1;
        int minBlockY = Math.Max(0, (int)(screenTop / GameConstants.BlockSize));
        int maxBlockY = (int)(screenBottom / GameConstants.BlockSize) + 1;
        
        // Draw blocks as colored rectangles (using SDL's primitive drawing)
        for (int y = minBlockY; y < maxBlockY; y++)
        {
            for (int x = minBlockX; x < maxBlockX; x++)
            {
                var block = _chunkManager.GetBlock(x, y);
                if (block.Type == BlockType.Air) continue;
                
                float worldX = x * GameConstants.BlockSize;
                float worldY = y * GameConstants.BlockSize;
                
                var (screenX, screenY) = camera.WorldToScreen(worldX, worldY);
                float size = GameConstants.BlockSize * camera.Zoom;
                
                // Draw block with color based on type
                DrawColoredRect(screenX, screenY, size, size, GetBlockColor(block.Type));
            }
        }
    }
    
    private void RenderEntities(CameraComponent camera)
    {
        // Render all entities with sprite components
        foreach (var entity in World.GetAllEntities())
        {
            var pos = World.GetComponent<PositionComponent>(entity);
            var sprite = World.GetComponent<SpriteComponent>(entity);
            
            if (pos != null && sprite != null)
            {
                var (screenX, screenY) = camera.WorldToScreen(pos.X, pos.Y);
                float width = sprite.Width * camera.Zoom;
                float height = sprite.Height * camera.Zoom;
                
                // Draw entity as colored rectangle
                DrawColoredRect(screenX - width/2, screenY - height/2, width, height, 
                    GetConsoleColor(sprite.Color));
            }
        }
    }
    
    private void DrawColoredRect(float x, float y, float w, float h, (float r, float g, float b) color)
    {
        // This is a helper method that would ideally use SDL_RenderFillRect
        // For now, we'll add a new engine API call for drawing rectangles
        EngineInterop.Renderer_DrawRect(x, y, w, h, color.r, color.g, color.b, 1.0f);
    }
    
    private (float r, float g, float b) GetBlockColor(BlockType type)
    {
        return type switch
        {
            BlockType.Grass => (0.13f, 0.55f, 0.13f),      // Forest green
            BlockType.Dirt => (0.55f, 0.27f, 0.07f),       // Saddle brown
            BlockType.Stone => (0.66f, 0.66f, 0.66f),      // Gray
            BlockType.Sand => (0.96f, 0.96f, 0.86f),       // Beige
            BlockType.Water => (0.0f, 0.5f, 1.0f),         // Blue
            BlockType.Wood => (0.55f, 0.27f, 0.07f),       // Brown
            BlockType.Coal => (0.2f, 0.2f, 0.2f),          // Dark gray
            BlockType.Iron => (0.75f, 0.75f, 0.75f),       // Light gray
            BlockType.Gold => (1.0f, 0.84f, 0.0f),         // Gold
            _ => (0.5f, 0.5f, 0.5f)                         // Default gray
        };
    }
    
    private (float r, float g, float b) GetConsoleColor(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Yellow => (1.0f, 1.0f, 0.0f),
            ConsoleColor.Green => (0.0f, 1.0f, 0.0f),
            ConsoleColor.Red => (1.0f, 0.0f, 0.0f),
            ConsoleColor.Blue => (0.0f, 0.0f, 1.0f),
            ConsoleColor.White => (1.0f, 1.0f, 1.0f),
            ConsoleColor.Cyan => (0.0f, 1.0f, 1.0f),
            ConsoleColor.Magenta => (1.0f, 0.0f, 1.0f),
            _ => (0.7f, 0.7f, 0.7f)
        };
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("[VisualDemo] Unloading visual demo scene...");
        _chunkManager = null;
    }
}
