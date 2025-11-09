using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.World;

/// <summary>
/// Room type in a dungeon
/// </summary>
public enum RoomType
{
    Entrance,
    Normal,
    Treasure,
    Boss,
    Puzzle,
    Secret
}

/// <summary>
/// Dungeon room definition
/// </summary>
public class DungeonRoom
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public RoomType Type { get; set; }
    public List<DungeonRoom> ConnectedRooms { get; private set; }
    public bool IsCleared { get; set; }
    
    public DungeonRoom(int x, int y, int width, int height, RoomType type = RoomType.Normal)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
        Type = type;
        ConnectedRooms = new List<DungeonRoom>();
        IsCleared = false;
    }
    
    /// <summary>
    /// Check if this room overlaps with another
    /// </summary>
    public bool Overlaps(DungeonRoom other, int padding = 2)
    {
        return !(X + Width + padding <= other.X ||
                 other.X + other.Width + padding <= X ||
                 Y + Height + padding <= other.Y ||
                 other.Y + other.Height + padding <= Y);
    }
    
    /// <summary>
    /// Get center point of room
    /// </summary>
    public (int x, int y) GetCenter()
    {
        return (X + Width / 2, Y + Height / 2);
    }
    
    /// <summary>
    /// Check if a point is inside this room
    /// </summary>
    public bool Contains(int px, int py)
    {
        return px >= X && px < X + Width && py >= Y && py < Y + Height;
    }
}

/// <summary>
/// Corridor connecting two rooms
/// </summary>
public class Corridor
{
    public List<(int x, int y)> Path { get; private set; }
    public DungeonRoom Room1 { get; set; }
    public DungeonRoom Room2 { get; set; }
    
    public Corridor(DungeonRoom room1, DungeonRoom room2)
    {
        Room1 = room1;
        Room2 = room2;
        Path = new List<(int x, int y)>();
    }
}

/// <summary>
/// Complete dungeon structure
/// </summary>
public class Dungeon
{
    public string Name { get; set; }
    public string Id { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<DungeonRoom> Rooms { get; private set; }
    public List<Corridor> Corridors { get; private set; }
    public DungeonRoom? EntranceRoom { get; set; }
    public DungeonRoom? BossRoom { get; set; }
    public int EntranceWorldX { get; set; }
    public int EntranceWorldY { get; set; }
    
    public Dungeon(string id, string name, int width, int height)
    {
        Id = id;
        Name = name;
        Width = width;
        Height = height;
        Rooms = new List<DungeonRoom>();
        Corridors = new List<Corridor>();
    }
    
    /// <summary>
    /// Get room at world position
    /// </summary>
    public DungeonRoom? GetRoomAt(int x, int y)
    {
        return Rooms.FirstOrDefault(r => r.Contains(x, y));
    }
}

/// <summary>
/// Generates dungeons using BSP (Binary Space Partitioning) algorithm
/// </summary>
public class DungeonGenerator
{
    private Random random;
    private int minRoomSize = 8;
    private int maxRoomSize = 15;
    
    public DungeonGenerator(int seed)
    {
        random = new Random(seed);
    }
    
    /// <summary>
    /// Generate a complete dungeon
    /// </summary>
    public Dungeon Generate(string id, string name, int width, int height, int numRooms = 10)
    {
        var dungeon = new Dungeon(id, name, width, height);
        
        // Generate rooms using random placement
        GenerateRooms(dungeon, numRooms);
        
        // Connect rooms with corridors
        ConnectRooms(dungeon);
        
        // Designate special rooms
        DesignateSpecialRooms(dungeon);
        
        return dungeon;
    }
    
    /// <summary>
    /// Generate rooms in the dungeon space
    /// </summary>
    private void GenerateRooms(Dungeon dungeon, int numRooms)
    {
        int attempts = 0;
        int maxAttempts = numRooms * 10;
        
        while (dungeon.Rooms.Count < numRooms && attempts < maxAttempts)
        {
            attempts++;
            
            // Random room size
            int width = random.Next(minRoomSize, maxRoomSize);
            int height = random.Next(minRoomSize, maxRoomSize);
            
            // Random position
            int x = random.Next(2, dungeon.Width - width - 2);
            int y = random.Next(2, dungeon.Height - height - 2);
            
            var newRoom = new DungeonRoom(x, y, width, height);
            
            // Check if room overlaps with existing rooms
            bool overlaps = false;
            foreach (var room in dungeon.Rooms)
            {
                if (newRoom.Overlaps(room))
                {
                    overlaps = true;
                    break;
                }
            }
            
            if (!overlaps)
            {
                dungeon.Rooms.Add(newRoom);
            }
        }
    }
    
    /// <summary>
    /// Connect rooms with corridors using MST-like approach
    /// </summary>
    private void ConnectRooms(Dungeon dungeon)
    {
        if (dungeon.Rooms.Count < 2)
            return;
            
        // Connect each room to its nearest unconnected neighbor
        var connected = new HashSet<DungeonRoom> { dungeon.Rooms[0] };
        var unconnected = new HashSet<DungeonRoom>(dungeon.Rooms.Skip(1));
        
        while (unconnected.Count > 0)
        {
            DungeonRoom? bestFrom = null;
            DungeonRoom? bestTo = null;
            float bestDistance = float.MaxValue;
            
            // Find closest pair of connected and unconnected rooms
            foreach (var connectedRoom in connected)
            {
                foreach (var unconnectedRoom in unconnected)
                {
                    float distance = GetDistance(connectedRoom, unconnectedRoom);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestFrom = connectedRoom;
                        bestTo = unconnectedRoom;
                    }
                }
            }
            
            if (bestFrom != null && bestTo != null)
            {
                // Create corridor
                var corridor = CreateCorridor(bestFrom, bestTo);
                dungeon.Corridors.Add(corridor);
                
                // Mark as connected
                bestFrom.ConnectedRooms.Add(bestTo);
                bestTo.ConnectedRooms.Add(bestFrom);
                connected.Add(bestTo);
                unconnected.Remove(bestTo);
            }
            else
            {
                break; // Safety break
            }
        }
        
        // Add some extra connections for loops (20% chance per room pair)
        for (int i = 0; i < dungeon.Rooms.Count - 1; i++)
        {
            for (int j = i + 1; j < dungeon.Rooms.Count; j++)
            {
                if (!dungeon.Rooms[i].ConnectedRooms.Contains(dungeon.Rooms[j]))
                {
                    if (random.NextDouble() < 0.2)
                    {
                        var corridor = CreateCorridor(dungeon.Rooms[i], dungeon.Rooms[j]);
                        dungeon.Corridors.Add(corridor);
                        dungeon.Rooms[i].ConnectedRooms.Add(dungeon.Rooms[j]);
                        dungeon.Rooms[j].ConnectedRooms.Add(dungeon.Rooms[i]);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Create a corridor between two rooms
    /// </summary>
    private Corridor CreateCorridor(DungeonRoom room1, DungeonRoom room2)
    {
        var corridor = new Corridor(room1, room2);
        var (x1, y1) = room1.GetCenter();
        var (x2, y2) = room2.GetCenter();
        
        // Create L-shaped corridor
        int currentX = x1;
        int currentY = y1;
        
        // Horizontal first
        while (currentX != x2)
        {
            corridor.Path.Add((currentX, currentY));
            currentX += (x2 > currentX) ? 1 : -1;
        }
        
        // Then vertical
        while (currentY != y2)
        {
            corridor.Path.Add((currentX, currentY));
            currentY += (y2 > currentY) ? 1 : -1;
        }
        
        corridor.Path.Add((x2, y2));
        
        return corridor;
    }
    
    /// <summary>
    /// Designate entrance, boss, treasure rooms
    /// </summary>
    private void DesignateSpecialRooms(Dungeon dungeon)
    {
        if (dungeon.Rooms.Count == 0)
            return;
            
        // First room is entrance
        dungeon.EntranceRoom = dungeon.Rooms[0];
        dungeon.EntranceRoom.Type = RoomType.Entrance;
        
        // Last/farthest room is boss room
        if (dungeon.Rooms.Count > 1)
        {
            var bossRoom = dungeon.Rooms.Last();
            bossRoom.Type = RoomType.Boss;
            dungeon.BossRoom = bossRoom;
        }
        
        // Random treasure and puzzle rooms
        for (int i = 1; i < dungeon.Rooms.Count - 1; i++)
        {
            double roll = random.NextDouble();
            if (roll < 0.2)
            {
                dungeon.Rooms[i].Type = RoomType.Treasure;
            }
            else if (roll < 0.35)
            {
                dungeon.Rooms[i].Type = RoomType.Puzzle;
            }
        }
    }
    
    /// <summary>
    /// Get distance between two rooms (center to center)
    /// </summary>
    private float GetDistance(DungeonRoom room1, DungeonRoom room2)
    {
        var (x1, y1) = room1.GetCenter();
        var (x2, y2) = room2.GetCenter();
        int dx = x2 - x1;
        int dy = y2 - y1;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
    
    /// <summary>
    /// Place the dungeon data into the chunk system
    /// </summary>
    public void PlaceDungeonInWorld(Dungeon dungeon, Terrain.ChunkManager chunkManager, int startDepth = 5)
    {
        // Place rooms
        foreach (var room in dungeon.Rooms)
        {
            PlaceRoom(room, chunkManager, startDepth);
        }
        
        // Place corridors
        foreach (var corridor in dungeon.Corridors)
        {
            PlaceCorridor(corridor, chunkManager, startDepth);
        }
    }
    
    /// <summary>
    /// Place a room in the world
    /// </summary>
    private void PlaceRoom(DungeonRoom room, Terrain.ChunkManager chunkManager, int depth)
    {
        for (int y = room.Y; y < room.Y + room.Height; y++)
        {
            for (int x = room.X; x < room.X + room.Width; x++)
            {
                // Create air space inside room
                chunkManager.SetBlock(x, depth + (y - room.Y), TileType.Air);
                
                // Place walls on edges
                if (x == room.X || x == room.X + room.Width - 1 ||
                    y == room.Y || y == room.Y + room.Height - 1)
                {
                    chunkManager.SetBlock(x, depth + (y - room.Y), TileType.Stone);
                }
            }
        }
    }
    
    /// <summary>
    /// Place a corridor in the world
    /// </summary>
    private void PlaceCorridor(Corridor corridor, Terrain.ChunkManager chunkManager, int depth)
    {
        foreach (var (x, y) in corridor.Path)
        {
            // Create 2-wide corridor
            chunkManager.SetBlock(x, depth + y, TileType.Air);
            chunkManager.SetBlock(x + 1, depth + y, TileType.Air);
            chunkManager.SetBlock(x, depth + y + 1, TileType.Air);
            chunkManager.SetBlock(x + 1, depth + y + 1, TileType.Air);
        }
    }
}
