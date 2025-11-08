using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;

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
        // Draw all entities with position and sprite components
        foreach (var entity in world.GetEntitiesWithComponent<PositionComponent>())
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var sprite = world.GetComponent<SpriteComponent>(entity);
            var health = world.GetComponent<HealthComponent>(entity);
            
            if (position != null && sprite != null)
            {
                // Convert world coordinates (0-1920 x 0-1080) to screen coordinates (1-78 x 1-22)
                // Map world space to screen space
                int screenX = (int)((position.X / 1920.0f) * (MapWidth - 2)) + 1;
                int screenY = (int)((position.Y / 1080.0f) * (MapHeight - 2)) + 1;
                
                // Clamp to buffer bounds
                screenX = Math.Clamp(screenX, 1, MapWidth - 2);
                screenY = Math.Clamp(screenY, 1, MapHeight - 2);
                
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
