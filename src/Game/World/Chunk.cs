namespace ChroniclesOfADrifter.Terrain;

/// <summary>
/// Represents a chunk of the world, containing a 2D grid of tiles
/// Size: 32 blocks wide × 30 blocks tall (10 surface + 20 underground)
/// </summary>
public class Chunk
{
    public const int CHUNK_WIDTH = 32;
    public const int CHUNK_HEIGHT = 30;
    public const int SURFACE_HEIGHT = 10;
    public const int UNDERGROUND_DEPTH = 20;
    
    /// <summary>
    /// Chunk coordinate in the world (not world block coordinate)
    /// </summary>
    public int ChunkX { get; private set; }
    
    /// <summary>
    /// 2D array of tile types [x, y]
    /// Y coordinate: 0-9 = surface (0 is highest), 10-29 = underground
    /// </summary>
    private ECS.Components.TileType[,] tiles;
    
    /// <summary>
    /// Whether this chunk has been generated
    /// </summary>
    public bool IsGenerated { get; private set; }
    
    /// <summary>
    /// Whether this chunk has been modified by the player
    /// </summary>
    public bool IsModified { get; private set; }
    
    public Chunk(int chunkX)
    {
        ChunkX = chunkX;
        tiles = new ECS.Components.TileType[CHUNK_WIDTH, CHUNK_HEIGHT];
        IsGenerated = false;
        IsModified = false;
    }
    
    /// <summary>
    /// Gets the tile type at local chunk coordinates
    /// </summary>
    /// <param name="localX">X coordinate within chunk (0-31)</param>
    /// <param name="localY">Y coordinate within chunk (0-29)</param>
    public ECS.Components.TileType GetTile(int localX, int localY)
    {
        if (localX < 0 || localX >= CHUNK_WIDTH || localY < 0 || localY >= CHUNK_HEIGHT)
        {
            return ECS.Components.TileType.Air;
        }
        
        return tiles[localX, localY];
    }
    
    /// <summary>
    /// Sets the tile type at local chunk coordinates
    /// </summary>
    public void SetTile(int localX, int localY, ECS.Components.TileType type)
    {
        if (localX < 0 || localX >= CHUNK_WIDTH || localY < 0 || localY >= CHUNK_HEIGHT)
        {
            return;
        }
        
        tiles[localX, localY] = type;
        IsModified = true;
    }
    
    /// <summary>
    /// Converts world X coordinate to chunk coordinate
    /// </summary>
    public static int WorldToChunkCoord(int worldX)
    {
        return worldX >= 0 ? worldX / CHUNK_WIDTH : (worldX - CHUNK_WIDTH + 1) / CHUNK_WIDTH;
    }
    
    /// <summary>
    /// Converts world X coordinate to local chunk coordinate (0-31)
    /// </summary>
    public static int WorldToLocalCoord(int worldX)
    {
        int local = worldX % CHUNK_WIDTH;
        return local >= 0 ? local : local + CHUNK_WIDTH;
    }
    
    /// <summary>
    /// Gets the world X coordinate of the leftmost block in this chunk
    /// </summary>
    public int GetWorldStartX()
    {
        return ChunkX * CHUNK_WIDTH;
    }
    
    /// <summary>
    /// Marks the chunk as generated
    /// </summary>
    public void SetGenerated()
    {
        IsGenerated = true;
    }
    
    /// <summary>
    /// Fills the entire chunk with a specific tile type (for testing)
    /// </summary>
    public void Fill(ECS.Components.TileType type)
    {
        for (int x = 0; x < CHUNK_WIDTH; x++)
        {
            for (int y = 0; y < CHUNK_HEIGHT; y++)
            {
                tiles[x, y] = type;
            }
        }
        IsGenerated = true;
    }
}
