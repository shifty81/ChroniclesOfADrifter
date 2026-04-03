namespace ChroniclesOfADrifter.WorldManagement;

/// <summary>
/// Single authoritative source for all grid / world-coordinate conversions.
/// Tile size is 32 px. Chunks are 32 tiles wide × 30 tiles tall.
/// </summary>
public static class GridCoordUtility
{
    public const int TileSize   = 32;
    public const int ChunkWidth = 32;
    public const int ChunkHeight = 30;

    // ── World → Tile ────────────────────────────────────────────────────────

    /// <summary>Converts a world-pixel X position to a tile X index.</summary>
    public static int WorldToTileX(float worldX) => (int)Math.Floor(worldX / TileSize);

    /// <summary>Converts a world-pixel Y position to a tile Y index.</summary>
    public static int WorldToTileY(float worldY) => (int)Math.Floor(worldY / TileSize);

    // ── Tile → World ────────────────────────────────────────────────────────

    /// <summary>Returns the world-pixel X of the left edge of a tile.</summary>
    public static float TileToWorldX(int tileX) => tileX * TileSize;

    /// <summary>Returns the world-pixel Y of the top edge of a tile.</summary>
    public static float TileToWorldY(int tileY) => tileY * TileSize;

    // ── Tile → Chunk ────────────────────────────────────────────────────────

    /// <summary>Returns the chunk X index for a given tile X.</summary>
    public static int TileToChunkX(int tileX)
    {
        // Floor division so negative tile coords map to the correct chunk.
        if (tileX >= 0) return tileX / ChunkWidth;
        return (tileX - (ChunkWidth - 1)) / ChunkWidth;
    }

    /// <summary>Returns the local (within-chunk) X for a given tile X.</summary>
    public static int TileToLocalX(int tileX)
    {
        int local = tileX % ChunkWidth;
        return local < 0 ? local + ChunkWidth : local;
    }

    // ── World → Chunk ───────────────────────────────────────────────────────

    /// <summary>Returns the chunk X index for a world-pixel X position.</summary>
    public static int WorldToChunkX(float worldX) => TileToChunkX(WorldToTileX(worldX));

    // ── Bounds helpers ──────────────────────────────────────────────────────

    /// <summary>Returns true when localX / localY are valid within a chunk.</summary>
    public static bool IsValidLocal(int localX, int localY)
        => localX >= 0 && localX < ChunkWidth
        && localY >= 0 && localY < ChunkHeight;
}
