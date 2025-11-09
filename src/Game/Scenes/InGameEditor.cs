using ChroniclesOfADrifter.ECS;
using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Engine;
using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.Scenes;

/// <summary>
/// Helper class to add in-game editor capabilities to any scene
/// Allows toggling editor mode with F1 or Tilde key
/// </summary>
public class InGameEditor
{
    private World world;
    private ChunkManager? chunkManager;
    private bool editorEnabled = false;
    private int selectedTileIndex = 0;
    private List<TileType> availableTiles;
    
    // Key codes
    private const int KEY_F1 = 1073741882;
    private const int KEY_TILDE = 96;
    private const int KEY_LEFT_BRACKET = 91;
    private const int KEY_RIGHT_BRACKET = 93;
    private const int KEY_SPACE = 32;
    private const int KEY_DELETE = 127;
    
    public InGameEditor(World world, ChunkManager? chunkManager)
    {
        this.world = world;
        this.chunkManager = chunkManager;
        
        // Initialize available tiles
        availableTiles = new List<TileType>
        {
            TileType.Grass,
            TileType.Dirt,
            TileType.Stone,
            TileType.Sand,
            TileType.Snow,
            TileType.Water,
            TileType.Wood,
            TileType.WoodPlank,
            TileType.Cobblestone,
            TileType.Brick,
            TileType.Torch
        };
    }
    
    /// <summary>
    /// Update the editor - call this in your scene's Update method
    /// </summary>
    public void Update(float deltaTime)
    {
        HandleEditorToggle();
        
        if (editorEnabled)
        {
            HandleEditorInput();
        }
    }
    
    private void HandleEditorToggle()
    {
        if (EngineInterop.Input_IsKeyPressed(KEY_F1) || EngineInterop.Input_IsKeyPressed(KEY_TILDE))
        {
            editorEnabled = !editorEnabled;
            string status = editorEnabled ? "ENABLED" : "DISABLED";
            Console.WriteLine($"\n[InGameEditor] Editor mode: {status}");
            
            if (editorEnabled)
            {
                Console.WriteLine("[InGameEditor] Controls:");
                Console.WriteLine("  [ / ] - Previous/Next tile");
                Console.WriteLine("  Space - Place tile at camera center");
                Console.WriteLine("  Delete - Remove tile at camera center");
                Console.WriteLine("  F1/~ - Toggle editor\n");
            }
        }
    }
    
    private void HandleEditorInput()
    {
        // Tile selection
        if (EngineInterop.Input_IsKeyPressed(KEY_LEFT_BRACKET))
        {
            selectedTileIndex = (selectedTileIndex - 1 + availableTiles.Count) % availableTiles.Count;
            Console.WriteLine($"[InGameEditor] Selected: {availableTiles[selectedTileIndex]}");
        }
        if (EngineInterop.Input_IsKeyPressed(KEY_RIGHT_BRACKET))
        {
            selectedTileIndex = (selectedTileIndex + 1) % availableTiles.Count;
            Console.WriteLine($"[InGameEditor] Selected: {availableTiles[selectedTileIndex]}");
        }
        
        // Tile placement
        if (EngineInterop.Input_IsKeyPressed(KEY_SPACE))
        {
            PlaceTileAtCamera();
        }
        
        // Tile removal
        if (EngineInterop.Input_IsKeyPressed(KEY_DELETE))
        {
            RemoveTileAtCamera();
        }
    }
    
    private void PlaceTileAtCamera()
    {
        if (chunkManager == null) return;
        
        // Find camera position
        foreach (var entity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var cameraPos = world.GetComponent<PositionComponent>(entity);
            if (cameraPos != null)
            {
                int tileX = (int)(cameraPos.X / 32);
                int tileY = (int)(cameraPos.Y / 32);
                
                int chunkX = tileX / 32;
                int localX = tileX % 32;
                
                var chunk = chunkManager.GetChunk(chunkX);
                if (chunk != null && tileY >= 0 && tileY < 30)
                {
                    TileType selectedTile = availableTiles[selectedTileIndex];
                    chunk.SetTile(localX, tileY, selectedTile);
                    Console.WriteLine($"[InGameEditor] Placed {selectedTile} at ({tileX}, {tileY})");
                }
                break;
            }
        }
    }
    
    private void RemoveTileAtCamera()
    {
        if (chunkManager == null) return;
        
        // Find camera position
        foreach (var entity in world.GetEntitiesWithComponent<CameraComponent>())
        {
            var cameraPos = world.GetComponent<PositionComponent>(entity);
            if (cameraPos != null)
            {
                int tileX = (int)(cameraPos.X / 32);
                int tileY = (int)(cameraPos.Y / 32);
                
                int chunkX = tileX / 32;
                int localX = tileX % 32;
                
                var chunk = chunkManager.GetChunk(chunkX);
                if (chunk != null && tileY >= 0 && tileY < 30)
                {
                    chunk.SetTile(localX, tileY, TileType.Air);
                    Console.WriteLine($"[InGameEditor] Removed tile at ({tileX}, {tileY})");
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// Check if editor is currently enabled
    /// </summary>
    public bool IsEnabled() => editorEnabled;
    
    /// <summary>
    /// Get the currently selected tile type
    /// </summary>
    public TileType GetSelectedTile() => availableTiles[selectedTileIndex];
}
