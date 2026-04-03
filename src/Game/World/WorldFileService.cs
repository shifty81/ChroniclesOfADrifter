using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.Terrain;
using System.Text.Json;

namespace ChroniclesOfADrifter.WorldManagement;

/// <summary>
/// Handles saving and loading the world grid (chunks) to/from a JSON file.
/// All edits are assumed to have gone through EditorCommandStack before this
/// is called, so no mutation happens here.
/// </summary>
public class WorldFileService
{
    private readonly ChunkManager _chunkManager;

    public WorldFileService(ChunkManager chunkManager)
    {
        _chunkManager = chunkManager;
    }

    // ── Save ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Saves all loaded chunks to <paramref name="filePath"/>.
    /// The directory is created automatically if it does not exist.
    /// </summary>
    public void Save(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");

        var worldFile = new WorldFile
        {
            SavedAt  = DateTime.UtcNow,
            TileSize = GridCoordUtility.TileSize,
            Tiles    = CollectTiles()
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(filePath, JsonSerializer.Serialize(worldFile, options));

        Console.WriteLine($"[WorldFileService] Saved {worldFile.Tiles.Count} tiles → {filePath}");
    }

    // ── Load ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads a previously saved world file into the chunk manager.
    /// Returns true on success, false if the file was not found or invalid.
    /// </summary>
    public bool Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[WorldFileService] File not found: {filePath}");
            return false;
        }

        try
        {
            var worldFile = JsonSerializer.Deserialize<WorldFile>(File.ReadAllText(filePath));
            if (worldFile?.Tiles == null)
            {
                Console.WriteLine("[WorldFileService] Invalid world file.");
                return false;
            }

            ApplyTiles(worldFile.Tiles);
            Console.WriteLine($"[WorldFileService] Loaded {worldFile.Tiles.Count} tiles ← {filePath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WorldFileService] Load failed: {ex.Message}");
            return false;
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private List<TileEntry> CollectTiles()
    {
        var tiles = new List<TileEntry>();

        for (int chunkIndex = 0; chunkIndex < _chunkManager.GetLoadedChunkCount(); chunkIndex++)
        {
            var chunk = _chunkManager.GetChunk(chunkIndex);
            if (chunk == null) continue;

            for (int lx = 0; lx < GridCoordUtility.ChunkWidth; lx++)
            {
                for (int ly = 0; ly < GridCoordUtility.ChunkHeight; ly++)
                {
                    var type = chunk.GetTile(lx, ly);
                    if (type != TileType.Air)
                    {
                        tiles.Add(new TileEntry
                        {
                            X    = chunkIndex * GridCoordUtility.ChunkWidth + lx,
                            Y    = ly,
                            Type = type.ToString()
                        });
                    }
                }
            }
        }

        return tiles;
    }

    private void ApplyTiles(List<TileEntry> tiles)
    {
        foreach (var entry in tiles)
        {
            if (!Enum.TryParse<TileType>(entry.Type, out var tileType)) continue;

            int chunkX = GridCoordUtility.TileToChunkX(entry.X);
            int localX = GridCoordUtility.TileToLocalX(entry.X);

            var chunk = _chunkManager.GetChunk(chunkX);
            if (chunk != null && GridCoordUtility.IsValidLocal(localX, entry.Y))
            {
                chunk.SetTile(localX, entry.Y, tileType);
            }
        }
    }

    // ── DTO types ────────────────────────────────────────────────────────────

    private class WorldFile
    {
        public DateTime SavedAt  { get; set; }
        public int TileSize      { get; set; }
        public List<TileEntry> Tiles { get; set; } = new();
    }

    private class TileEntry
    {
        public int X     { get; set; }
        public int Y     { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
