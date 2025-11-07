# Procedural Generation Guide

## Overview

This document details the procedural generation algorithms suitable for creating Zelda-like dungeons and worlds in Chronicles of a Drifter.

## Dungeon Generation Algorithms

### 1. Binary Space Partitioning (BSP)

**Best For**: Structured dungeons with distinct rooms

**Algorithm**:
```
1. Start with a rectangle representing the entire dungeon
2. Recursively split the space:
   - Choose random orientation (horizontal/vertical)
   - Choose random split position (with min/max constraints)
   - Split into two sub-rectangles
3. Stop when rectangles reach minimum size
4. Create rooms within leaf nodes
5. Connect rooms with corridors
```

**Implementation Pseudocode**:
```csharp
public class BSPNode
{
    public Rectangle Bounds { get; set; }
    public BSPNode Left { get; set; }
    public BSPNode Right { get; set; }
    public Room Room { get; set; }
    
    public void Split(int minSize)
    {
        if (Bounds.Width < minSize * 2 && Bounds.Height < minSize * 2)
            return;
            
        bool splitHorizontal = Random.value > 0.5f;
        
        if (Bounds.Width > Bounds.Height && Bounds.Width / Bounds.Height >= 1.25f)
            splitHorizontal = false;
        else if (Bounds.Height > Bounds.Width && Bounds.Height / Bounds.Width >= 1.25f)
            splitHorizontal = true;
            
        int max = (splitHorizontal ? Bounds.Height : Bounds.Width) - minSize;
        int split = Random.Range(minSize, max);
        
        if (splitHorizontal)
        {
            Left = new BSPNode(new Rectangle(Bounds.X, Bounds.Y, Bounds.Width, split));
            Right = new BSPNode(new Rectangle(Bounds.X, Bounds.Y + split, Bounds.Width, Bounds.Height - split));
        }
        else
        {
            Left = new BSPNode(new Rectangle(Bounds.X, Bounds.Y, split, Bounds.Height));
            Right = new BSPNode(new Rectangle(Bounds.X + split, Bounds.Y, Bounds.Width - split, Bounds.Height));
        }
        
        Left.Split(minSize);
        Right.Split(minSize);
    }
    
    public void CreateRooms()
    {
        if (Left != null || Right != null)
        {
            if (Left != null) Left.CreateRooms();
            if (Right != null) Right.CreateRooms();
            
            if (Left != null && Right != null)
                CreateCorridor(Left.GetRoom(), Right.GetRoom());
        }
        else
        {
            // Leaf node - create a room
            int roomWidth = Random.Range(Bounds.Width / 2, Bounds.Width - 2);
            int roomHeight = Random.Range(Bounds.Height / 2, Bounds.Height - 2);
            int roomX = Bounds.X + Random.Range(1, Bounds.Width - roomWidth - 1);
            int roomY = Bounds.Y + Random.Range(1, Bounds.Height - roomHeight - 1);
            
            Room = new Room(roomX, roomY, roomWidth, roomHeight);
        }
    }
}
```

**Advantages**:
- Guaranteed no overlapping rooms
- Natural corridor placement
- Controllable room sizes
- Good for multi-level dungeons

**Parameters to Tune**:
- `minRoomSize`: 5-10 tiles
- `maxRoomSize`: 15-25 tiles
- `splitRatio`: 0.45-0.55 for balance
- `corridorWidth`: 2-3 tiles

### 2. Random Walker Algorithm

**Best For**: Organic caves, winding paths, natural-looking dungeons

**Algorithm**:
```
1. Start at a random or central position
2. Mark current tile as floor
3. Choose random direction (up, down, left, right)
4. Move in that direction
5. Repeat until desired floor count reached
```

**Implementation Pseudocode**:
```csharp
public class RandomWalker
{
    private Vector2Int position;
    private HashSet<Vector2Int> floorTiles = new HashSet<Vector2Int>();
    
    public void Generate(int targetFloorCount, float changeDirectionChance = 0.3f)
    {
        Vector2Int currentDirection = GetRandomDirection();
        
        while (floorTiles.Count < targetFloorCount)
        {
            floorTiles.Add(position);
            
            if (Random.value < changeDirectionChance)
                currentDirection = GetRandomDirection();
                
            position += currentDirection;
            
            // Keep within bounds
            position.x = Mathf.Clamp(position.x, 0, mapWidth - 1);
            position.y = Mathf.Clamp(position.y, 0, mapHeight - 1);
        }
    }
    
    private Vector2Int GetRandomDirection()
    {
        int choice = Random.Range(0, 4);
        return choice switch
        {
            0 => Vector2Int.up,
            1 => Vector2Int.down,
            2 => Vector2Int.left,
            3 => Vector2Int.right,
            _ => Vector2Int.up
        };
    }
}
```

**Enhancements**:
- **Multiple Walkers**: Run several walkers simultaneously for branching paths
- **Weighted Directions**: Bias toward certain directions for flow
- **Room Carving**: Occasionally carve out larger rooms
- **Post-Processing**: Smooth jagged edges, remove isolated tiles

**Advantages**:
- Very natural, organic appearance
- Fast and simple to implement
- Good for caves and outdoor areas
- Easy to add variations

**Parameters to Tune**:
- `changeDirectionChance`: 0.2-0.5
- `numberOfWalkers`: 3-7 for branching
- `stepCount`: Based on desired size
- `roomCarveChance`: 0.05-0.15

### 3. Cellular Automata

**Best For**: Cave systems, natural terrain, erosion effects

**Algorithm**:
```
1. Initialize grid with random walls/floors (50/50 ratio)
2. For each generation:
   - Count neighboring walls for each tile
   - Apply rules: if neighbors > threshold, become wall, else floor
3. Repeat for several generations
4. Clean up isolated regions
```

**Implementation Pseudocode**:
```csharp
public class CellularAutomata
{
    private bool[,] map; // true = wall, false = floor
    
    public void Generate(int width, int height, float wallChance = 0.5f, int iterations = 4)
    {
        map = new bool[width, height];
        
        // Initialize randomly
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = Random.value < wallChance;
        
        // Run cellular automata
        for (int i = 0; i < iterations; i++)
            map = DoSimulationStep(map);
            
        CleanUpMap();
    }
    
    private bool[,] DoSimulationStep(bool[,] oldMap)
    {
        bool[,] newMap = new bool[oldMap.GetLength(0), oldMap.GetLength(1)];
        
        for (int x = 0; x < oldMap.GetLength(0); x++)
        {
            for (int y = 0; y < oldMap.GetLength(1); y++)
            {
                int wallCount = GetAdjacentWallCount(oldMap, x, y);
                
                // Birth/death rules
                if (oldMap[x, y]) // Currently wall
                    newMap[x, y] = wallCount >= 4;
                else // Currently floor
                    newMap[x, y] = wallCount >= 5;
            }
        }
        
        return newMap;
    }
    
    private int GetAdjacentWallCount(bool[,] map, int x, int y, int range = 1)
    {
        int count = 0;
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int nx = x + dx;
                int ny = y + dy;
                
                if (nx < 0 || nx >= map.GetLength(0) || ny < 0 || ny >= map.GetLength(1))
                    count++; // Treat out of bounds as wall
                else if (map[nx, ny])
                    count++;
            }
        }
        return count;
    }
}
```

**Advantages**:
- Extremely natural-looking caves
- Self-organizing patterns
- Good for outdoor rocky areas
- Can simulate erosion

**Parameters to Tune**:
- `initialWallChance`: 0.45-0.55
- `iterations`: 3-6
- `birthThreshold`: 4-5
- `deathThreshold`: 3-4

### 4. Hybrid Approach (Recommended for Zelda-like)

**Combine Multiple Algorithms**:
```
1. Use BSP to create main structure and large rooms
2. Use Random Walker to connect rooms with organic corridors
3. Use Cellular Automata for special cave areas
4. Place key items and enemies strategically
```

**Example Hybrid**:
```csharp
public class HybridDungeonGenerator
{
    public Dungeon Generate()
    {
        var dungeon = new Dungeon();
        
        // 1. BSP for main structure
        var bsp = new BSPNode(new Rectangle(0, 0, 100, 100));
        bsp.Split(minSize: 8);
        var rooms = bsp.GetAllRooms();
        
        // 2. Select key rooms
        var entranceRoom = rooms[0];
        var bossRoom = rooms[rooms.Count - 1];
        var treasureRooms = rooms.Where(r => r.Size > threshold).ToList();
        
        // 3. Random Walker for secret passages
        foreach (var room in treasureRooms)
        {
            if (Random.value < 0.3f) // 30% chance of secret path
            {
                var walker = new RandomWalker(room.Center);
                walker.GenerateToTarget(FindNearbyRoom(rooms, room).Center);
            }
        }
        
        // 4. Cellular Automata for cave section
        if (Random.value < 0.5f)
        {
            var caveArea = SelectAreaForCave(dungeon);
            var ca = new CellularAutomata();
            ca.Generate(caveArea.Width, caveArea.Height);
        }
        
        // 5. Place content
        PlaceEnemies(dungeon, rooms);
        PlaceTreasure(dungeon, treasureRooms);
        PlaceKeys(dungeon, rooms);
        PlaceDoors(dungeon, rooms);
        
        return dungeon;
    }
}
```

## World Generation

### Overworld Structure
```
1. Generate biome map using Perlin noise
2. Place major landmarks (towns, dungeons)
3. Create paths between landmarks using A* or Random Walker
4. Populate with resources and enemies based on biome
5. Hide dungeon entrances with environmental puzzles
```

### Scene Transitions
```csharp
public class SceneTransitionManager
{
    public void GenerateAdjacentScene(Scene currentScene, Direction exitDirection)
    {
        // Ensure consistent border tiles for seamless transition
        var seed = GetDeterministicSeed(currentScene.Coordinates, exitDirection);
        Random.InitState(seed);
        
        var newScene = GenerateScene(currentScene.Biome);
        
        // Match entrance to previous exit
        newScene.CreateEntrance(OppositeDirection(exitDirection));
    }
}
```

## Dungeon Entrance Concealment

### Overgrowth System
```csharp
public class DungeonEntranceConcealer
{
    public void HideEntrance(DungeonEntrance entrance)
    {
        switch (entrance.ConcealmentType)
        {
            case ConcealmentType.Overgrowth:
                PlaceVegetation(entrance.Position, density: High);
                entrance.RequiredTool = Tool.Machete;
                break;
                
            case ConcealmentType.RockSlide:
                PlaceBoulders(entrance.Position);
                entrance.RequiredTool = Tool.Bomb;
                break;
                
            case ConcealmentType.Puzzle:
                CreateEnvironmentalPuzzle(entrance.Position);
                break;
                
            case ConcealmentType.Hidden:
                entrance.VisibleOnlyWithItem = Item.RevealingLens;
                break;
        }
    }
}
```

## Performance Considerations

### Optimization Techniques
1. **Generate in Chunks**: Don't generate entire world at once
2. **Async Generation**: Use background threads
3. **Caching**: Store generated scenes in memory/disk
4. **Seeded Random**: Allow reproduction of same world
5. **Progressive Generation**: Generate as player explores

### Recommended Generation Times
- Small room: < 10ms
- Medium dungeon: < 100ms
- Large scene: < 500ms
- World chunk: < 1s (async)

## Testing and Iteration

### Validation Checks
1. **Connectivity**: All rooms reachable
2. **Dead Ends**: Minimum number for pacing
3. **Size**: Within target range
4. **Playability**: No impossible sections
5. **Balance**: Fair enemy/reward distribution

### Debug Visualization
```csharp
public class GenerationDebugger
{
    public void VisualizeSteps(DungeonGenerator generator)
    {
        // Show step-by-step generation
        // Color code different algorithm outputs
        // Highlight problems (disconnected areas, etc.)
    }
}
```

## Conclusion

For Chronicles of a Drifter, a **hybrid approach** combining BSP for structure, Random Walkers for organic flow, and Cellular Automata for special areas will create the best Zelda-like dungeon experience. Each algorithm brings unique strengths that complement the others.
