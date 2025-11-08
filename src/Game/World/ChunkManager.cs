namespace ChroniclesOfADrifter.Terrain;

/// <summary>
/// Manages chunks, handles loading/unloading based on player position
/// </summary>
public class ChunkManager
{
    private Dictionary<int, Chunk> loadedChunks;
    private int renderDistance = 2; // Load chunks within 2 chunks of player
    private TerrainGenerator? terrainGenerator;
    
    public ChunkManager()
    {
        loadedChunks = new Dictionary<int, Chunk>();
    }
    
    /// <summary>
    /// Sets the terrain generator for this chunk manager
    /// </summary>
    public void SetTerrainGenerator(TerrainGenerator generator)
    {
        terrainGenerator = generator;
    }
    
    /// <summary>
    /// Gets a chunk at the given chunk coordinate, loading it if necessary
    /// </summary>
    public Chunk GetChunk(int chunkX)
    {
        if (loadedChunks.TryGetValue(chunkX, out var chunk))
        {
            return chunk;
        }
        
        // Load/generate new chunk
        chunk = new Chunk(chunkX);
        
        if (terrainGenerator != null)
        {
            terrainGenerator.GenerateChunk(chunk);
        }
        else
        {
            // Fallback: fill with air
            chunk.Fill(ECS.Components.TileType.Air);
        }
        
        loadedChunks[chunkX] = chunk;
        return chunk;
    }
    
    /// <summary>
    /// Gets the tile at a world coordinate
    /// </summary>
    public ECS.Components.TileType GetTile(int worldX, int worldY)
    {
        if (worldY < 0 || worldY >= Chunk.CHUNK_HEIGHT)
        {
            return ECS.Components.TileType.Air;
        }
        
        int chunkX = Chunk.WorldToChunkCoord(worldX);
        int localX = Chunk.WorldToLocalCoord(worldX);
        
        var chunk = GetChunk(chunkX);
        return chunk.GetTile(localX, worldY);
    }
    
    /// <summary>
    /// Sets the tile at a world coordinate
    /// </summary>
    public void SetTile(int worldX, int worldY, ECS.Components.TileType type)
    {
        if (worldY < 0 || worldY >= Chunk.CHUNK_HEIGHT)
        {
            return;
        }
        
        int chunkX = Chunk.WorldToChunkCoord(worldX);
        int localX = Chunk.WorldToLocalCoord(worldX);
        
        var chunk = GetChunk(chunkX);
        chunk.SetTile(localX, worldY, type);
    }
    
    /// <summary>
    /// Updates chunks based on player position, loading nearby and unloading distant chunks
    /// </summary>
    public void UpdateChunks(float playerWorldX)
    {
        int playerChunkX = Chunk.WorldToChunkCoord((int)playerWorldX);
        
        // Load chunks around player
        for (int offsetX = -renderDistance; offsetX <= renderDistance; offsetX++)
        {
            int chunkX = playerChunkX + offsetX;
            GetChunk(chunkX); // This will load the chunk if not already loaded
        }
        
        // Unload far chunks (simple approach: keep all for now, can optimize later)
        // In a real implementation, you'd unload chunks beyond renderDistance + buffer
    }
    
    /// <summary>
    /// Gets all loaded chunks
    /// </summary>
    public IEnumerable<Chunk> GetLoadedChunks()
    {
        return loadedChunks.Values;
    }
    
    /// <summary>
    /// Gets the number of loaded chunks
    /// </summary>
    public int GetLoadedChunkCount()
    {
        return loadedChunks.Count;
    }
    
    /// <summary>
    /// Gets the vegetation at a world X coordinate (surface only)
    /// </summary>
    public ECS.Components.TileType? GetVegetation(int worldX)
    {
        int chunkX = Chunk.WorldToChunkCoord(worldX);
        int localX = Chunk.WorldToLocalCoord(worldX);
        
        var chunk = GetChunk(chunkX);
        return chunk.GetVegetation(localX);
    }
    
    /// <summary>
    /// Sets the vegetation at a world X coordinate (surface only)
    /// </summary>
    public void SetVegetation(int worldX, ECS.Components.TileType? type)
    {
        int chunkX = Chunk.WorldToChunkCoord(worldX);
        int localX = Chunk.WorldToLocalCoord(worldX);
        
        var chunk = GetChunk(chunkX);
        chunk.SetVegetation(localX, type);
    }
}
