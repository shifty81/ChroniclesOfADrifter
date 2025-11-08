using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;

namespace ChroniclesOfADrifter.Rendering;

/// <summary>
/// Simple console-based renderer for displaying game state
/// </summary>
public class ConsoleRenderer
{
    private const int MapWidth = 80;
    private const int MapHeight = 24;
    private const int InfoHeight = 6;
    private readonly char[,] _buffer = new char[MapWidth, MapHeight];
    private readonly ConsoleColor[,] _colorBuffer = new ConsoleColor[MapWidth, MapHeight];
    
    public void Render(World world, float fps)
    {
        // Clear buffer
        ClearBuffer();
        
        // Draw parallax background layers (sorted by ZOrder)
        DrawParallaxLayers(world);
        
        // Draw boundaries
        DrawBoundaries();
        
        // Draw entities
        DrawEntities(world);
        
        // Draw to console
        Console.SetCursorPosition(0, 0);
        DrawBuffer();
        
        // Draw info panel
        DrawInfoPanel(world, fps);
    }
    
    private void ClearBuffer()
    {
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                _buffer[x, y] = ' ';
                _colorBuffer[x, y] = ConsoleColor.White;
            }
        }
    }
    
    private void DrawParallaxLayers(World world)
    {
        // Get active camera
        var camera = CameraSystem.GetActiveCamera(world);
        if (camera == null)
            return;
        
        // Get all parallax layers and sort by ZOrder (lower = rendered first/behind)
        var parallaxLayers = new List<(Entity entity, ParallaxLayerComponent layer, PositionComponent position)>();
        
        foreach (var entity in world.GetEntitiesWithComponent<ParallaxLayerComponent>())
        {
            var layer = world.GetComponent<ParallaxLayerComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);
            
            if (layer != null && position != null && layer.IsVisible)
            {
                parallaxLayers.Add((entity, layer, position));
            }
        }
        
        // Sort by ZOrder (ascending)
        parallaxLayers.Sort((a, b) => a.layer.ZOrder.CompareTo(b.layer.ZOrder));
        
        // Render each layer
        foreach (var (entity, layer, position) in parallaxLayers)
        {
            DrawParallaxLayer(layer, position, camera);
        }
    }
    
    private void DrawParallaxLayer(ParallaxLayerComponent layer, PositionComponent position, CameraComponent camera)
    {
        // Calculate offset based on layer position and parallax factor
        float offsetX = position.X;
        float offsetY = position.Y;
        
        // Draw based on visual type
        switch (layer.VisualType)
        {
            case ParallaxVisualType.Sky:
                DrawSkyLayer(layer, offsetX, offsetY);
                break;
                
            case ParallaxVisualType.Clouds:
                DrawCloudsLayer(layer, offsetX, offsetY);
                break;
                
            case ParallaxVisualType.Mountains:
                DrawMountainsLayer(layer, offsetX, offsetY);
                break;
                
            case ParallaxVisualType.Stars:
                DrawStarsLayer(layer, offsetX, offsetY);
                break;
                
            case ParallaxVisualType.Mist:
                DrawMistLayer(layer, offsetX, offsetY);
                break;
        }
    }
    
    private void DrawSkyLayer(ParallaxLayerComponent layer, float offsetX, float offsetY)
    {
        // Fill entire background with sky color
        for (int x = 1; x < MapWidth - 1; x++)
        {
            for (int y = 1; y < MapHeight - 1; y++)
            {
                if (_buffer[x, y] == ' ')
                {
                    _buffer[x, y] = '·';
                    _colorBuffer[x, y] = layer.Color;
                }
            }
        }
    }
    
    private void DrawCloudsLayer(ParallaxLayerComponent layer, float offsetX, float offsetY)
    {
        // Draw cloud patterns that move with auto-scroll
        int cloudOffset = (int)(offsetX * 0.1f) % MapWidth;
        
        for (int x = 1; x < MapWidth - 1; x++)
        {
            for (int y = 1; y < MapHeight / 3; y++) // Clouds in upper portion
            {
                // Use noise-like pattern for clouds
                int worldX = (x + cloudOffset) % MapWidth;
                float noise = (MathF.Sin(worldX * 0.3f + y * 0.5f) + MathF.Cos(worldX * 0.2f)) * 0.5f + 0.5f;
                
                if (noise > (1.0f - layer.Density))
                {
                    if (_buffer[x, y] == ' ' || _buffer[x, y] == '·')
                    {
                        _buffer[x, y] = '░';
                        _colorBuffer[x, y] = layer.Color;
                    }
                }
            }
        }
    }
    
    private void DrawMountainsLayer(ParallaxLayerComponent layer, float offsetX, float offsetY)
    {
        // Draw mountain silhouette at bottom of layer
        int mountainOffset = (int)(offsetX * 0.05f) % MapWidth;
        
        for (int x = 1; x < MapWidth - 1; x++)
        {
            int worldX = (x + mountainOffset) % MapWidth;
            
            // Create mountain profile using sine waves
            float height = MathF.Abs(MathF.Sin(worldX * 0.15f)) * 6 + 
                          MathF.Abs(MathF.Sin(worldX * 0.08f + 2.5f)) * 4;
            int peakY = MapHeight - 2 - (int)height;
            
            for (int y = peakY; y < MapHeight - 1; y++)
            {
                if (_buffer[x, y] == ' ' || _buffer[x, y] == '·')
                {
                    _buffer[x, y] = '▲';
                    _colorBuffer[x, y] = layer.Color;
                }
            }
        }
    }
    
    private void DrawStarsLayer(ParallaxLayerComponent layer, float offsetX, float offsetY)
    {
        // Draw stars scattered across the sky
        int starSeed = (int)(offsetX * 0.01f);
        var random = new Random(42 + starSeed); // Consistent seed for stable stars
        
        int starCount = (int)(MapWidth * MapHeight * layer.Density * 0.01f);
        
        for (int i = 0; i < starCount; i++)
        {
            int x = random.Next(1, MapWidth - 1);
            int y = random.Next(1, MapHeight / 2); // Stars in upper half
            
            if (_buffer[x, y] == ' ' || _buffer[x, y] == '·')
            {
                _buffer[x, y] = random.Next(4) switch
                {
                    0 => '·',
                    1 => '*',
                    2 => '✦',
                    _ => '+'
                };
                _colorBuffer[x, y] = layer.Color;
            }
        }
    }
    
    private void DrawMistLayer(ParallaxLayerComponent layer, float offsetX, float offsetY)
    {
        // Draw mist effect in lower portion
        int mistOffset = (int)(offsetX * 0.2f) % MapWidth;
        
        for (int x = 1; x < MapWidth - 1; x++)
        {
            for (int y = MapHeight * 2 / 3; y < MapHeight - 1; y++) // Mist in lower portion
            {
                int worldX = (x + mistOffset) % MapWidth;
                float noise = (MathF.Sin(worldX * 0.4f + y * 0.3f) + 1.0f) * 0.5f;
                
                if (noise > (1.0f - layer.Density))
                {
                    if (_buffer[x, y] == ' ')
                    {
                        _buffer[x, y] = '░';
                        _colorBuffer[x, y] = layer.Color;
                    }
                }
            }
        }
    }
    
    private void DrawBoundaries()
    {
        // Top and bottom borders
        for (int x = 0; x < MapWidth; x++)
        {
            _buffer[x, 0] = '═';
            _buffer[x, MapHeight - 1] = '═';
            _colorBuffer[x, 0] = ConsoleColor.DarkGray;
            _colorBuffer[x, MapHeight - 1] = ConsoleColor.DarkGray;
        }
        
        // Left and right borders
        for (int y = 0; y < MapHeight; y++)
        {
            _buffer[0, y] = '║';
            _buffer[MapWidth - 1, y] = '║';
            _colorBuffer[0, y] = ConsoleColor.DarkGray;
            _colorBuffer[MapWidth - 1, y] = ConsoleColor.DarkGray;
        }
        
        // Corners
        _buffer[0, 0] = '╔';
        _buffer[MapWidth - 1, 0] = '╗';
        _buffer[0, MapHeight - 1] = '╚';
        _buffer[MapWidth - 1, MapHeight - 1] = '╝';
    }
    
    private void DrawEntities(World world)
    {
        // Get active camera
        var camera = CameraSystem.GetActiveCamera(world);
        
        // Draw all entities with position and sprite components
        foreach (var entity in world.GetEntitiesWithComponent<PositionComponent>())
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var sprite = world.GetComponent<SpriteComponent>(entity);
            var health = world.GetComponent<HealthComponent>(entity);
            
            if (position != null && sprite != null)
            {
                int screenX, screenY;
                
                if (camera != null)
                {
                    // Use camera to transform world coordinates to screen coordinates
                    var (camScreenX, camScreenY) = camera.WorldToScreen(position.X, position.Y);
                    
                    // Map camera screen space (0-1920 x 0-1080) to console buffer (1-78 x 1-22)
                    screenX = (int)((camScreenX / camera.ViewportWidth) * (MapWidth - 2)) + 1;
                    screenY = (int)((camScreenY / camera.ViewportHeight) * (MapHeight - 2)) + 1;
                }
                else
                {
                    // Fallback: Convert world coordinates (0-1920 x 0-1080) to screen coordinates (1-78 x 1-22)
                    screenX = (int)((position.X / 1920.0f) * (MapWidth - 2)) + 1;
                    screenY = (int)((position.Y / 1080.0f) * (MapHeight - 2)) + 1;
                }
                
                // Check if entity is visible in viewport
                if (screenX < 1 || screenX >= MapWidth - 1 || screenY < 1 || screenY >= MapHeight - 1)
                    continue;
                
                // Determine character and color
                char displayChar = '?';
                ConsoleColor color = ConsoleColor.White;
                
                if (world.HasComponent<PlayerComponent>(entity))
                {
                    displayChar = '@';
                    color = ConsoleColor.Green;
                }
                else if (world.HasComponent<ScriptComponent>(entity))
                {
                    displayChar = 'G'; // Goblin
                    color = health != null && health.CurrentHealth <= 0 ? ConsoleColor.DarkRed : ConsoleColor.Red;
                }
                
                _buffer[screenX, screenY] = displayChar;
                _colorBuffer[screenX, screenY] = color;
            }
        }
    }
    
    private void DrawBuffer()
    {
        var prevColor = Console.ForegroundColor;
        
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                if (_colorBuffer[x, y] != prevColor)
                {
                    Console.ForegroundColor = _colorBuffer[x, y];
                    prevColor = _colorBuffer[x, y];
                }
                Console.Write(_buffer[x, y]);
            }
            Console.WriteLine();
        }
        
        Console.ForegroundColor = ConsoleColor.White;
    }
    
    private void DrawInfoPanel(World world, float fps)
    {
        Console.WriteLine("════════════════════════════════════════════════════════════════════════════════");
        Console.WriteLine($" FPS: {fps:F1}  |  Controls: WASD/Arrows=Move  Space=Attack  Q=Quit");
        Console.WriteLine("────────────────────────────────────────────────────────────────────────────────");
        
        // Find player
        foreach (var entity in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var health = world.GetComponent<HealthComponent>(entity);
            
            if (position != null && health != null)
            {
                Console.Write($" Player: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"@ ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Position: ({position.X:F0}, {position.Y:F0})  ");
                
                // Health bar
                Console.Write("Health: [");
                int healthBars = (int)(health.CurrentHealth / 10);
                Console.ForegroundColor = health.CurrentHealth > 50 ? ConsoleColor.Green : 
                                        health.CurrentHealth > 25 ? ConsoleColor.Yellow : ConsoleColor.Red;
                Console.Write(new string('█', healthBars));
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write(new string('░', 10 - healthBars));
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"] {health.CurrentHealth:F0}/{health.MaxHealth}");
            }
        }
        
        // Count enemies
        int enemyCount = 0;
        int aliveEnemies = 0;
        foreach (var entity in world.GetEntitiesWithComponent<ScriptComponent>())
        {
            enemyCount++;
            var health = world.GetComponent<HealthComponent>(entity);
            if (health != null && health.CurrentHealth > 0)
                aliveEnemies++;
        }
        
        Console.Write($" Enemies: ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"G ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Active: {aliveEnemies} / {enemyCount}");
        
        Console.WriteLine("════════════════════════════════════════════════════════════════════════════════");
    }
    
    public static void InitializeConsole()
    {
        Console.Clear();
        Console.CursorVisible = false;
        
        // Try to resize console (may not work in all environments)
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Console.SetWindowSize(Math.Min(85, Console.LargestWindowWidth), 
                                    Math.Min(35, Console.LargestWindowHeight));
                Console.SetBufferSize(85, 35);
            }
        }
        catch
        {
            // Ignore if we can't resize
        }
    }
}
