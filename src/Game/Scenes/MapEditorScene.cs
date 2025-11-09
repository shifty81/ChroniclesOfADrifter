using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.ECS.Systems;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Terrain;
using ChroniclesOfADrifter.Rendering;
using System.Text.Json;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// In-game map editor for real-time scene editing
/// Allows placing/removing tiles, changing tilesets, and saving/loading maps
/// </summary>
public class MapEditorScene : Scene
{
    private Entity _cameraEntity;
    private ChunkManager? chunkManager;
    private TerrainGenerator? terrainGenerator;
    private TilesetManager tilesetManager;
    
    private bool editorEnabled = true;
    private int selectedTileIndex = 0;
    private List<string> availableTiles = new();
    private TileType selectedTileType = TileType.Grass;
    
    // Editor state
    private bool isPainting = false;
    private bool isErasing = false;
    private float cameraMoveSpeed = 400.0f;
    
    // Key codes
    private const int KEY_F1 = 1073741882;
    private const int KEY_TILDE = 96;
    private const int KEY_LEFT_BRACKET = 91;
    private const int KEY_RIGHT_BRACKET = 93;
    private const int KEY_1 = 49;
    private const int KEY_2 = 50;
    private const int KEY_3 = 51;
    private const int KEY_4 = 52;
    private const int KEY_5 = 53;
    private const int KEY_6 = 54;
    private const int KEY_7 = 55;
    private const int KEY_8 = 56;
    private const int KEY_9 = 57;
    private const int KEY_0 = 48;
    private const int KEY_S = 115;
    private const int KEY_L = 108;
    private const int KEY_N = 110;
    private const int KEY_G = 103;
    private const int KEY_SPACE = 32;
    private const int KEY_W = 119;
    private const int KEY_A = 97;
    private const int KEY_D = 100;
    private const int KEY_UP = 1073741906;
    private const int KEY_DOWN = 1073741905;
    private const int KEY_LEFT = 1073741904;
    private const int KEY_RIGHT = 1073741903;
    
    public MapEditorScene()
    {
        tilesetManager = new TilesetManager();
    }
    
    public override void OnLoad()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         MAP EDITOR - Real-Time Scene Editing            ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝\n");
        
        // Initialize tileset manager
        tilesetManager.CreateDefaultTileset();
        
        // Try to load additional tilesets
        string tilesetDir = "assets/tilesets";
        if (Directory.Exists(tilesetDir))
        {
            tilesetManager.LoadTilesetsFromDirectory(tilesetDir);
        }
        
        // Initialize terrain generation (optional)
        terrainGenerator = new TerrainGenerator(seed: 12345);
        chunkManager = new ChunkManager();
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Store chunk manager as shared resource
        World.SetSharedResource("ChunkManager", chunkManager);
        
        // Add systems
        World.AddSystem(new CameraInputSystem());
        World.AddSystem(new CameraSystem());
        World.AddSystem(new TerrainRenderingSystem());
        
        // Create camera entity
        _cameraEntity = World.CreateEntity();
        var camera = new CameraComponent(1280, 720)
        {
            Zoom = 1.0f,
            FollowSpeed = 0 // No follow, manual control
        };
        World.AddComponent(_cameraEntity, camera);
        World.AddComponent(_cameraEntity, new PositionComponent(640, 360));
        
        // Pre-generate initial chunks
        if (chunkManager != null)
        {
            chunkManager.UpdateChunks(640);
            Console.WriteLine($"[MapEditor] Generated {chunkManager.GetLoadedChunkCount()} initial chunks");
        }
        
        // Build available tiles list
        BuildAvailableTilesList();
        
        PrintInstructions();
    }
    
    private void BuildAvailableTilesList()
    {
        availableTiles = new List<string>
        {
            "Grass", "Dirt", "Stone", "Sand", "Snow", "Water",
            "Wood", "WoodPlank", "Cobblestone", "Brick",
            "CoalOre", "IronOre", "GoldOre", "Torch"
        };
        
        Console.WriteLine("[MapEditor] Available tiles:");
        for (int i = 0; i < availableTiles.Count; i++)
        {
            Console.WriteLine($"  [{i}] {availableTiles[i]}");
        }
        Console.WriteLine();
    }
    
    private void PrintInstructions()
    {
        Console.WriteLine("═══════════════ EDITOR CONTROLS ═══════════════");
        Console.WriteLine("  Camera Movement:");
        Console.WriteLine("    WASD or Arrow Keys - Move camera");
        Console.WriteLine("    +/-                - Zoom in/out");
        Console.WriteLine();
        Console.WriteLine("  Tile Editing:");
        Console.WriteLine("    Left Click / Space - Place selected tile");
        Console.WriteLine("    Right Click        - Erase tile");
        Console.WriteLine("    [ / ]              - Previous/Next tile");
        Console.WriteLine("    0-9                - Quick select tile");
        Console.WriteLine();
        Console.WriteLine("  Map Management:");
        Console.WriteLine("    Ctrl+S             - Save current map");
        Console.WriteLine("    Ctrl+L             - Load map");
        Console.WriteLine("    Ctrl+N             - New map (clear all)");
        Console.WriteLine("    G                  - Generate new terrain");
        Console.WriteLine();
        Console.WriteLine("  Other:");
        Console.WriteLine("    F1 / ~             - Toggle editor UI");
        Console.WriteLine("    Q / ESC            - Exit editor");
        Console.WriteLine("═══════════════════════════════════════════════\n");
    }
    
    public override void Update(float deltaTime)
    {
        HandleEditorInput();
        HandleCameraMovement(deltaTime);
        UpdateChunks();
        
        World.Update(deltaTime);
        
        // Display editor info on screen
        if (editorEnabled)
        {
            DisplayEditorInfo();
        }
    }
    
    private void HandleEditorInput()
    {
        // Toggle editor
        if (EngineInterop.Input_IsKeyPressed(KEY_F1) || EngineInterop.Input_IsKeyPressed(KEY_TILDE))
        {
            editorEnabled = !editorEnabled;
            Console.WriteLine($"[MapEditor] Editor UI: {(editorEnabled ? "Enabled" : "Disabled")}");
        }
        
        if (!editorEnabled) return;
        
        // Tile selection with [ and ]
        if (EngineInterop.Input_IsKeyPressed(KEY_LEFT_BRACKET))
        {
            selectedTileIndex = (selectedTileIndex - 1 + availableTiles.Count) % availableTiles.Count;
            UpdateSelectedTile();
        }
        if (EngineInterop.Input_IsKeyPressed(KEY_RIGHT_BRACKET))
        {
            selectedTileIndex = (selectedTileIndex + 1) % availableTiles.Count;
            UpdateSelectedTile();
        }
        
        // Quick select with number keys
        HandleQuickSelect();
        
        // Tile placement/removal
        if (EngineInterop.Input_IsKeyDown(KEY_SPACE))
        {
            PlaceTileAtCursor();
        }
        
        // Map management
        if (EngineInterop.Input_IsKeyPressed(KEY_S))
        {
            SaveCurrentMap();
        }
        if (EngineInterop.Input_IsKeyPressed(KEY_L))
        {
            LoadMap();
        }
        if (EngineInterop.Input_IsKeyPressed(KEY_N))
        {
            ClearMap();
        }
        if (EngineInterop.Input_IsKeyPressed(KEY_G))
        {
            GenerateNewTerrain();
        }
    }
    
    private void HandleQuickSelect()
    {
        int[] numberKeys = { KEY_0, KEY_1, KEY_2, KEY_3, KEY_4, KEY_5, KEY_6, KEY_7, KEY_8, KEY_9 };
        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (EngineInterop.Input_IsKeyPressed(numberKeys[i]))
            {
                int index = i;
                if (index < availableTiles.Count)
                {
                    selectedTileIndex = index;
                    UpdateSelectedTile();
                }
            }
        }
    }
    
    private void UpdateSelectedTile()
    {
        string tileName = availableTiles[selectedTileIndex];
        if (Enum.TryParse<TileType>(tileName, out var tileType))
        {
            selectedTileType = tileType;
            Console.WriteLine($"[MapEditor] Selected tile: {tileName}");
        }
    }
    
    private void HandleCameraMovement(float deltaTime)
    {
        var cameraPos = World.GetComponent<PositionComponent>(_cameraEntity);
        if (cameraPos == null) return;
        
        float moveAmount = cameraMoveSpeed * deltaTime;
        
        if (EngineInterop.Input_IsKeyDown(KEY_W) || EngineInterop.Input_IsKeyDown(KEY_UP))
        {
            cameraPos.Y -= moveAmount;
        }
        if (EngineInterop.Input_IsKeyDown(KEY_S) || EngineInterop.Input_IsKeyDown(KEY_DOWN))
        {
            cameraPos.Y += moveAmount;
        }
        if (EngineInterop.Input_IsKeyDown(KEY_A) || EngineInterop.Input_IsKeyDown(KEY_LEFT))
        {
            cameraPos.X -= moveAmount;
        }
        if (EngineInterop.Input_IsKeyDown(KEY_D) || EngineInterop.Input_IsKeyDown(KEY_RIGHT))
        {
            cameraPos.X += moveAmount;
        }
    }
    
    private void PlaceTileAtCursor()
    {
        if (chunkManager == null) return;
        
        var cameraPos = World.GetComponent<PositionComponent>(_cameraEntity);
        var camera = World.GetComponent<CameraComponent>(_cameraEntity);
        if (cameraPos == null || camera == null) return;
        
        // Get world position at center of screen (cursor position)
        float worldX = cameraPos.X;
        float worldY = cameraPos.Y;
        
        // Convert to tile coordinates
        int tileX = (int)(worldX / 32);
        int tileY = (int)(worldY / 32);
        
        // Place the tile
        int chunkX = tileX / 32;
        int localX = tileX % 32;
        
        var chunk = chunkManager.GetChunk(chunkX);
        if (chunk != null && tileY >= 0 && tileY < 30)
        {
            chunk.SetTile(localX, tileY, selectedTileType);
            Console.WriteLine($"[MapEditor] Placed {selectedTileType} at ({tileX}, {tileY})");
        }
    }
    
    private void UpdateChunks()
    {
        if (chunkManager == null) return;
        
        var cameraPos = World.GetComponent<PositionComponent>(_cameraEntity);
        if (cameraPos != null)
        {
            chunkManager.UpdateChunks(cameraPos.X);
        }
    }
    
    private void SaveCurrentMap()
    {
        if (chunkManager == null)
        {
            Console.WriteLine("[MapEditor] Cannot save: ChunkManager not initialized");
            return;
        }
        
        string savePath = $"assets/maps/map_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        Directory.CreateDirectory("assets/maps");
        
        try
        {
            var mapData = new MapData
            {
                Name = $"Map_{DateTime.Now:yyyyMMdd_HHmmss}",
                Width = chunkManager.GetLoadedChunkCount() * 32,
                Height = 30,
                TileSize = 32,
                Tiles = new List<TileData>()
            };
            
            // Save all loaded chunks
            foreach (int chunkX in Enumerable.Range(0, chunkManager.GetLoadedChunkCount()))
            {
                var chunk = chunkManager.GetChunk(chunkX);
                if (chunk != null)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        for (int y = 0; y < 30; y++)
                        {
                            var tile = chunk.GetTile(x, y);
                            if (tile != TileType.Air)
                            {
                                mapData.Tiles.Add(new TileData
                                {
                                    X = chunkX * 32 + x,
                                    Y = y,
                                    Type = tile.ToString()
                                });
                            }
                        }
                    }
                }
            }
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(mapData, options);
            File.WriteAllText(savePath, json);
            
            Console.WriteLine($"[MapEditor] Map saved to: {savePath}");
            Console.WriteLine($"[MapEditor] Saved {mapData.Tiles.Count} tiles");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapEditor] Failed to save map: {ex.Message}");
        }
    }
    
    private void LoadMap()
    {
        Console.WriteLine("[MapEditor] Load map feature - Select a map file from assets/maps/");
        
        string mapsDir = "assets/maps";
        if (!Directory.Exists(mapsDir))
        {
            Console.WriteLine("[MapEditor] No maps directory found");
            return;
        }
        
        var mapFiles = Directory.GetFiles(mapsDir, "*.json");
        if (mapFiles.Length == 0)
        {
            Console.WriteLine("[MapEditor] No saved maps found");
            return;
        }
        
        // Load the most recent map
        string latestMap = mapFiles.OrderByDescending(f => File.GetLastWriteTime(f)).First();
        LoadMapFromFile(latestMap);
    }
    
    private void LoadMapFromFile(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            var mapData = JsonSerializer.Deserialize<MapData>(json);
            
            if (mapData == null || chunkManager == null)
            {
                Console.WriteLine("[MapEditor] Failed to load map data");
                return;
            }
            
            // Clear existing chunks and reload
            ClearMap();
            
            // Place all tiles
            foreach (var tileData in mapData.Tiles)
            {
                if (Enum.TryParse<TileType>(tileData.Type, out var tileType))
                {
                    int chunkX = tileData.X / 32;
                    int localX = tileData.X % 32;
                    
                    var chunk = chunkManager.GetChunk(chunkX);
                    if (chunk != null)
                    {
                        chunk.SetTile(localX, tileData.Y, tileType);
                    }
                }
            }
            
            Console.WriteLine($"[MapEditor] Loaded map from: {filePath}");
            Console.WriteLine($"[MapEditor] Placed {mapData.Tiles.Count} tiles");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MapEditor] Failed to load map: {ex.Message}");
        }
    }
    
    private void ClearMap()
    {
        if (chunkManager == null) return;
        
        // Clear all chunks by filling with air
        foreach (int chunkX in Enumerable.Range(0, chunkManager.GetLoadedChunkCount()))
        {
            var chunk = chunkManager.GetChunk(chunkX);
            if (chunk != null)
            {
                for (int x = 0; x < 32; x++)
                {
                    for (int y = 0; y < 30; y++)
                    {
                        chunk.SetTile(x, y, TileType.Air);
                    }
                }
            }
        }
        
        Console.WriteLine("[MapEditor] Map cleared");
    }
    
    private void GenerateNewTerrain()
    {
        if (terrainGenerator == null || chunkManager == null)
        {
            Console.WriteLine("[MapEditor] Cannot generate: TerrainGenerator not initialized");
            return;
        }
        
        // Generate new terrain with random seed
        int newSeed = new Random().Next();
        terrainGenerator = new TerrainGenerator(seed: newSeed);
        chunkManager.SetTerrainGenerator(terrainGenerator);
        
        // Regenerate chunks
        var cameraPos = World.GetComponent<PositionComponent>(_cameraEntity);
        if (cameraPos != null)
        {
            chunkManager.UpdateChunks(cameraPos.X);
        }
        
        Console.WriteLine($"[MapEditor] Generated new terrain with seed: {newSeed}");
    }
    
    private void DisplayEditorInfo()
    {
        // Display editor info in console (could be rendered on screen in full implementation)
        Console.Write($"\r[Editor] Tile: {availableTiles[selectedTileIndex]} | Camera: {GetCameraPosition()}     ");
    }
    
    private string GetCameraPosition()
    {
        var cameraPos = World.GetComponent<PositionComponent>(_cameraEntity);
        if (cameraPos != null)
        {
            return $"({cameraPos.X:F0}, {cameraPos.Y:F0})";
        }
        return "(0, 0)";
    }
    
    public override void OnUnload()
    {
        Console.WriteLine("\n[MapEditor] Exiting map editor...");
    }
}

/// <summary>
/// Map data structure for saving/loading
/// </summary>
public class MapData
{
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int TileSize { get; set; } = 32;
    public List<TileData> Tiles { get; set; } = new();
}

/// <summary>
/// Individual tile data for saving
/// </summary>
public class TileData
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Type { get; set; } = string.Empty;
}
