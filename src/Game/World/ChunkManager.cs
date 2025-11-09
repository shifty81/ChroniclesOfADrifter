namespace ChroniclesOfADrifter.Terrain;

/// <summary>
/// Manages chunks, handles loading/unloading based on player position.
/// Supports both synchronous and asynchronous chunk generation.
/// </summary>
public class ChunkManager : IDisposable
{
    private Dictionary<int, Chunk> loadedChunks;
    private int renderDistance = 2; // Load chunks within 2 chunks of player
    private int unloadDistance = 4; // Unload chunks beyond this distance
    private TerrainGenerator? terrainGenerator;
    private AsyncChunkGenerator? asyncGenerator;
    private bool useAsyncGeneration;
    private bool isDisposed;
    
    public ChunkManager(bool useAsyncGeneration = false)
    {
        loadedChunks = new Dictionary<int, Chunk>();
        this.useAsyncGeneration = useAsyncGeneration;
    }
    
    /// <summary>
    /// Sets the terrain generator for this chunk manager
    /// </summary>
    public void SetTerrainGenerator(TerrainGenerator generator)
    {
        terrainGenerator = generator;
        
        // Initialize async generator if needed
        if (useAsyncGeneration && asyncGenerator == null && terrainGenerator != null)
        {
            asyncGenerator = new AsyncChunkGenerator(terrainGenerator);
        }
    }
    
    /// <summary>
    /// Enables or disables async chunk generation
    /// </summary>
    public void SetAsyncGeneration(bool enabled)
    {
        if (useAsyncGeneration == enabled)
        {
            return;
        }
        
        useAsyncGeneration = enabled;
        
        if (enabled && asyncGenerator == null && terrainGenerator != null)
        {
            asyncGenerator = new AsyncChunkGenerator(terrainGenerator);
        }
        else if (!enabled && asyncGenerator != null)
        {
            asyncGenerator.Dispose();
            asyncGenerator = null;
        }
    }
    
    /// <summary>
    /// Gets a chunk at the given chunk coordinate, loading it if necessary.
    /// Returns null if using async generation and chunk is not ready yet.
    /// </summary>
    public Chunk? GetChunk(int chunkX)
    {
        if (loadedChunks.TryGetValue(chunkX, out var chunk))
        {
            return chunk;
        }
        
        // Check if async generator has completed this chunk
        if (useAsyncGeneration && asyncGenerator != null)
        {
            var generatedChunk = asyncGenerator.TryGetGeneratedChunk(chunkX);
            if (generatedChunk != null)
            {
                loadedChunks[chunkX] = generatedChunk;
                return generatedChunk;
            }
            
            // Chunk not ready yet
            return null;
        }
        
        // Synchronous generation
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
    /// Gets a chunk synchronously, waiting if necessary (always returns a chunk).
    /// Use this when you need to guarantee a chunk is available.
    /// </summary>
    public Chunk GetChunkSync(int chunkX)
    {
        if (loadedChunks.TryGetValue(chunkX, out var chunk))
        {
            return chunk;
        }
        
        // Generate synchronously regardless of async setting
        chunk = new Chunk(chunkX);
        
        if (terrainGenerator != null)
        {
            terrainGenerator.GenerateChunk(chunk);
        }
        else
        {
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
        if (chunk == null)
        {
            return ECS.Components.TileType.Air;  // Chunk not loaded yet
        }
        
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
        
        // Always use sync get for modifications
        var chunk = GetChunkSync(chunkX);
        chunk.SetTile(localX, worldY, type);
    }
    
    /// <summary>
    /// Updates chunks based on player position, loading nearby and unloading distant chunks
    /// </summary>
    public void UpdateChunks(float playerWorldX)
    {
        int playerChunkX = Chunk.WorldToChunkCoord((int)playerWorldX);
        
        if (useAsyncGeneration && asyncGenerator != null)
        {
            // Async mode: request chunks in order of priority
            for (int offsetX = -renderDistance; offsetX <= renderDistance; offsetX++)
            {
                int chunkX = playerChunkX + offsetX;
                asyncGenerator.RequestChunkGeneration(chunkX, playerWorldX);
                
                // Check if chunk is ready and load it
                GetChunk(chunkX);
            }
            
            // Pre-request chunks slightly beyond render distance for smoother experience
            for (int offsetX = -(renderDistance + 1); offsetX <= (renderDistance + 1); offsetX++)
            {
                if (Math.Abs(offsetX) > renderDistance)
                {
                    int chunkX = playerChunkX + offsetX;
                    asyncGenerator.RequestChunkGeneration(chunkX, playerWorldX);
                }
            }
        }
        else
        {
            // Sync mode: load chunks immediately
            for (int offsetX = -renderDistance; offsetX <= renderDistance; offsetX++)
            {
                int chunkX = playerChunkX + offsetX;
                GetChunk(chunkX); // This will load the chunk if not already loaded
            }
        }
        
        // Unload far chunks
        var chunksToUnload = new List<int>();
        foreach (var chunkX in loadedChunks.Keys)
        {
            if (Math.Abs(chunkX - playerChunkX) > unloadDistance)
            {
                chunksToUnload.Add(chunkX);
            }
        }
        
        foreach (var chunkX in chunksToUnload)
        {
            loadedChunks.Remove(chunkX);
        }
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
    /// Gets async generation statistics (for debugging/UI)
    /// </summary>
    public (int queued, int inProgress, int completed) GetAsyncStats()
    {
        if (asyncGenerator == null)
        {
            return (0, 0, 0);
        }
        
        return (
            asyncGenerator.GetQueuedChunkCount(),
            asyncGenerator.GetInProgressChunkCount(),
            asyncGenerator.GetCompletedChunkCount()
        );
    }
    
    /// <summary>
    /// Gets the vegetation at a world X coordinate (surface only)
    /// </summary>
    public ECS.Components.TileType? GetVegetation(int worldX)
    {
        int chunkX = Chunk.WorldToChunkCoord(worldX);
        int localX = Chunk.WorldToLocalCoord(worldX);
        
        var chunk = GetChunk(chunkX);
        if (chunk == null)
        {
            return null;  // Chunk not loaded yet
        }
        
        return chunk.GetVegetation(localX);
    }
    
    /// <summary>
    /// Sets the vegetation at a world X coordinate (surface only)
    /// </summary>
    public void SetVegetation(int worldX, ECS.Components.TileType? type)
    {
        int chunkX = Chunk.WorldToChunkCoord(worldX);
        int localX = Chunk.WorldToLocalCoord(worldX);
        
        // Always use sync get for modifications
        var chunk = GetChunkSync(chunkX);
        chunk.SetVegetation(localX, type);
    }
    
    /// <summary>
    /// Disposes the chunk manager and any async resources
    /// </summary>
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }
        
        isDisposed = true;
        
        if (asyncGenerator != null)
        {
            asyncGenerator.Dispose();
            asyncGenerator = null;
        }
    }
}
