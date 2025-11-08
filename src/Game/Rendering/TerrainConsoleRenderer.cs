using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Rendering;

/// <summary>
/// Console renderer that displays terrain and entities
/// </summary>
public class TerrainConsoleRenderer
{
    private const int MapWidth = 80;
    private const int MapHeight = 24;
    private readonly char[,] _buffer = new char[MapWidth, MapHeight];
    private readonly ConsoleColor[,] _colorBuffer = new ConsoleColor[MapWidth, MapHeight];
    
    public void Render(World world, ChunkManager chunkManager, float fps)
    {
        // Clear buffer
        ClearBuffer();
        
        // Get active camera
        var camera = CameraSystem.GetActiveCamera(world);
        
        if (camera != null)
        {
            // Draw terrain
            DrawTerrain(world, chunkManager, camera);
            
            // Draw entities on top of terrain
            DrawEntities(world, camera);
        }
        
        // Draw boundaries
        DrawBoundaries();
        
        // Draw to console
        Console.SetCursorPosition(0, 0);
        DrawBuffer();
        
        // Draw info panel
        DrawInfoPanel(world, chunkManager, fps);
    }
    
    private void ClearBuffer()
    {
        for (int x = 0; x < MapWidth; x++)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                _buffer[x, y] = ' ';
                _colorBuffer[x, y] = ConsoleColor.Black;
            }
        }
    }
    
    private void DrawTerrain(World world, ChunkManager chunkManager, CameraComponent camera)
    {
        // Calculate visible world bounds based on camera
        // Map console buffer (1-78 x 1-22 inside borders) to world coordinates
        int viewWidth = MapWidth - 2;
        int viewHeight = MapHeight - 2;
        
        // Convert screen pixels to world blocks (assuming each block is ~24 pixels)
        int blocksPerScreenX = (int)(camera.ViewportWidth / (24 * camera.Zoom));
        int blocksPerScreenY = (int)(camera.ViewportHeight / (24 * camera.Zoom));
        
        int startWorldX = (int)camera.X - blocksPerScreenX / 2;
        int startWorldY = (int)camera.Y - blocksPerScreenY / 2;
        
        // Draw visible tiles
        for (int screenX = 0; screenX < viewWidth; screenX++)
        {
            for (int screenY = 0; screenY < viewHeight; screenY++)
            {
                // Map screen position to world tile coordinate
                int worldX = startWorldX + (screenX * blocksPerScreenX / viewWidth);
                int worldY = startWorldY + (screenY * blocksPerScreenY / viewHeight);
                
                // Convert world Y to chunk Y (0-29 range)
                int chunkY = worldY;
                if (chunkY >= 0 && chunkY < Chunk.CHUNK_HEIGHT)
                {
                    var tileType = chunkManager.GetTile(worldX, chunkY);
                    
                    // Draw tile to buffer
                    int bufferX = screenX + 1; // +1 for border
                    int bufferY = screenY + 1; // +1 for border
                    
                    // Check if there's vegetation at this position (only on surface)
                    // Display vegetation on top of terrain if it exists at surface level
                    var vegetation = chunkManager.GetVegetation(worldX);
                    
                    if (vegetation.HasValue && chunkY <= 9) // Vegetation only visible in surface area
                    {
                        // Find surface tile Y for this X column
                        int surfaceY = FindSurfaceY(chunkManager, worldX);
                        
                        // If we're rendering the surface or just above it, show vegetation
                        if (chunkY <= surfaceY + 1 && chunkY >= surfaceY - 1)
                        {
                            _buffer[bufferX, bufferY] = vegetation.Value.GetChar();
                            _colorBuffer[bufferX, bufferY] = ApplyLighting(vegetation.Value.GetColor(), worldX, worldY, world);
                            continue;
                        }
                    }
                    
                    // Normal tile rendering with lighting applied
                    _buffer[bufferX, bufferY] = tileType.GetChar();
                    _colorBuffer[bufferX, bufferY] = ApplyLighting(tileType.GetColor(), worldX, worldY, world);
                }
            }
        }
    }
    
    /// <summary>
    /// Apply lighting effect to a color based on the tile's light level
    /// </summary>
    private ConsoleColor ApplyLighting(ConsoleColor originalColor, int worldX, int worldY, World world)
    {
        // Get ambient light level for this depth
        float lightLevel = GetAmbientLightForDepth(worldY);
        
        // Check for nearby light sources (player, torches, etc.)
        float maxLightFromSources = GetLightFromNearbyLightSources(worldX, worldY, world);
        lightLevel = Math.Max(lightLevel, maxLightFromSources);
        
        // Apply lighting to color
        return AdjustColorBrightness(originalColor, lightLevel);
    }
    
    /// <summary>
    /// Get the ambient light level for a given Y coordinate (depth)
    /// </summary>
    private float GetAmbientLightForDepth(int y)
    {
        const int SURFACE_Y_THRESHOLD = 10;
        const int SHALLOW_UNDERGROUND_Y = 19;
        
        if (y < SURFACE_Y_THRESHOLD)
        {
            // Surface: Full daylight
            return 1.0f;
        }
        else if (y < SHALLOW_UNDERGROUND_Y)
        {
            // Shallow underground: Dim light (0.3 to 0.0)
            float depth = y - SURFACE_Y_THRESHOLD;
            float maxDepth = SHALLOW_UNDERGROUND_Y - SURFACE_Y_THRESHOLD;
            return Math.Max(0.3f - (depth / maxDepth) * 0.3f, 0f);
        }
        else
        {
            // Deep underground: Pitch black
            return 0f;
        }
    }
    
    /// <summary>
    /// Get light level from nearby light sources
    /// </summary>
    private float GetLightFromNearbyLightSources(int worldX, int worldY, World world)
    {
        float maxLight = 0f;
        
        // Check player position for player light
        var playerEntities = world.GetEntitiesWithComponent<PlayerComponent>();
        foreach (var entity in playerEntities)
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var lightSource = world.GetComponent<LightSourceComponent>(entity);
            
            if (position != null && lightSource != null && lightSource.IsActive)
            {
                float distance = MathF.Sqrt(
                    MathF.Pow(position.X - worldX, 2) +
                    MathF.Pow(position.Y - worldY, 2)
                );
                
                if (distance <= lightSource.Radius)
                {
                    float light = lightSource.Intensity * (1.0f - (distance / lightSource.Radius));
                    maxLight = Math.Max(maxLight, light);
                }
            }
        }
        
        // Check for torch entities (placed torches)
        var lightSources = world.GetEntitiesWithComponent<LightSourceComponent>();
        foreach (var entity in lightSources)
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var lightSource = world.GetComponent<LightSourceComponent>(entity);
            
            if (position != null && lightSource != null && lightSource.IsActive)
            {
                float distance = MathF.Sqrt(
                    MathF.Pow(position.X - worldX, 2) +
                    MathF.Pow(position.Y - worldY, 2)
                );
                
                if (distance <= lightSource.Radius)
                {
                    float light = lightSource.Intensity * (1.0f - (distance / lightSource.Radius));
                    maxLight = Math.Max(maxLight, light);
                }
            }
        }
        
        return maxLight;
    }
    
    /// <summary>
    /// Adjust color brightness based on light level
    /// </summary>
    private ConsoleColor AdjustColorBrightness(ConsoleColor color, float lightLevel)
    {
        // Clamp light level
        lightLevel = Math.Clamp(lightLevel, 0f, 1f);
        
        // If very dark (< 0.1), return black
        if (lightLevel < 0.1f)
        {
            return ConsoleColor.Black;
        }
        
        // If dim (< 0.5), convert to darker version
        if (lightLevel < 0.5f)
        {
            return color switch
            {
                ConsoleColor.White => ConsoleColor.Gray,
                ConsoleColor.Gray => ConsoleColor.DarkGray,
                ConsoleColor.Yellow => ConsoleColor.DarkYellow,
                ConsoleColor.Green => ConsoleColor.DarkGreen,
                ConsoleColor.Cyan => ConsoleColor.DarkCyan,
                ConsoleColor.Red => ConsoleColor.DarkRed,
                ConsoleColor.Magenta => ConsoleColor.DarkMagenta,
                ConsoleColor.Blue => ConsoleColor.DarkBlue,
                // Already dark colors stay as is
                _ => color
            };
        }
        
        // Normal or bright lighting - return original color
        return color;
    }
    
    /// <summary>
    /// Finds the surface Y coordinate for a given world X position
    /// </summary>
    private int FindSurfaceY(ChunkManager chunkManager, int worldX)
    {
        for (int y = 0; y < Chunk.CHUNK_HEIGHT; y++)
        {
            var tile = chunkManager.GetTile(worldX, y);
            if (tile.IsSolid())
            {
                return y;
            }
        }
        return 0;
    }
    
    private void DrawEntities(World world, CameraComponent camera)
    {
        int viewWidth = MapWidth - 2;
        int viewHeight = MapHeight - 2;
        
        // Draw all entities with position and sprite components
        foreach (var entity in world.GetEntitiesWithComponent<PositionComponent>())
        {
            var position = world.GetComponent<PositionComponent>(entity);
            var sprite = world.GetComponent<SpriteComponent>(entity);
            var playerComp = world.GetComponent<PlayerComponent>(entity);
            
            if (position != null && sprite != null)
            {
                // Use camera to transform world coordinates to screen coordinates
                var (camScreenX, camScreenY) = camera.WorldToScreen(position.X, position.Y);
                
                // Map camera screen space to console buffer
                int screenX = (int)((camScreenX / camera.ViewportWidth) * viewWidth) + 1;
                int screenY = (int)((camScreenY / camera.ViewportHeight) * viewHeight) + 1;
                
                // Draw if in bounds
                if (screenX >= 1 && screenX < MapWidth - 1 && 
                    screenY >= 1 && screenY < MapHeight - 1)
                {
                    // Use @ for player, G for other entities
                    char displayChar = playerComp != null ? '@' : 'G';
                    ConsoleColor displayColor = playerComp != null ? ConsoleColor.Cyan : ConsoleColor.Red;
                    
                    _buffer[screenX, screenY] = displayChar;
                    _colorBuffer[screenX, screenY] = displayColor;
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
    
    private void DrawBuffer()
    {
        for (int y = 0; y < MapHeight; y++)
        {
            for (int x = 0; x < MapWidth; x++)
            {
                Console.ForegroundColor = _colorBuffer[x, y];
                Console.Write(_buffer[x, y]);
            }
            Console.WriteLine();
        }
        Console.ResetColor();
    }
    
    private void DrawInfoPanel(World world, ChunkManager chunkManager, float fps)
    {
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════════════════════");
        Console.WriteLine($" FPS: {fps:F1} | Chunks: {chunkManager.GetLoadedChunkCount()}");
        
        // Get player info
        var playerEntities = world.GetEntitiesWithComponent<PlayerComponent>().ToList();
        if (playerEntities.Count > 0)
        {
            var entity = playerEntities[0];
            var position = world.GetComponent<PositionComponent>(entity);
            var health = world.GetComponent<HealthComponent>(entity);
            
            if (position != null && health != null)
            {
                int chunkX = Chunk.WorldToChunkCoord((int)position.X);
                int chunkY = (int)position.Y;
                var tileType = chunkManager.GetTile((int)position.X, chunkY);
                
                Console.WriteLine($" Player: ({position.X:F1}, {position.Y:F1}) | Chunk: {chunkX} | " +
                                $"Health: {health.CurrentHealth}/{health.MaxHealth}");
                Console.WriteLine($" Tile: {tileType}");
            }
        }
        
        Console.WriteLine(" Controls: WASD/Arrows=Move | Q/ESC=Quit");
        Console.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
    }
    
    public static void InitializeConsole()
    {
        Console.Clear();
        Console.CursorVisible = false;
        
        // Only set window size on Windows
        if (OperatingSystem.IsWindows())
        {
            try
            {
                Console.SetWindowSize(Math.Min(80, Console.LargestWindowWidth), 
                                    Math.Min(32, Console.LargestWindowHeight));
            }
            catch
            {
                // Ignore if console window size cannot be set
            }
        }
    }
}
