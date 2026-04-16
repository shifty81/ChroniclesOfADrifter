using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// A* grid-based pathfinding system used by NPCs to navigate the world.
///
/// The world is divided into a uniform grid of <see cref="CELL_SIZE"/> pixels.
/// Solid tiles (CollisionComponent.IsSolid or PositionComponent inside a known
/// obstacle) block movement; open cells are walkable.
///
/// The system drives NPC movement every <see cref="PATH_UPDATE_INTERVAL"/> seconds
/// to avoid recalculating every frame.
/// </summary>
public class PathfindingSystem : ISystem
{
    // World cell size in game pixels (matches the collision tile size)
    public const int CELL_SIZE = 32;

    // How often (seconds) NPCs recalculate their paths
    private const float PATH_UPDATE_INTERVAL = 0.5f;

    // Maximum A* iterations before giving up (prevents frame spikes)
    private const int MAX_ITERATIONS = 500;

    // Per-NPC path cache: entity → remaining waypoints
    private readonly Dictionary<Entity, Queue<(int cx, int cy)>> _npcPaths = new();
    private float _updateTimer = 0f;

    public void Initialize(World world)
    {
        Console.WriteLine("[Pathfinding] A* pathfinding system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        _updateTimer += deltaTime;

        if (_updateTimer >= PATH_UPDATE_INTERVAL)
        {
            _updateTimer = 0f;
            RefreshNPCPaths(world);
        }

        // Advance NPCs along their cached paths every frame
        foreach (var entity in world.GetEntitiesWithComponent<NPCComponent>())
        {
            if (!_npcPaths.TryGetValue(entity, out var path) || path.Count == 0) continue;

            var pos = world.GetComponent<PositionComponent>(entity);
            if (pos == null) continue;

            var (targetCx, targetCy) = path.Peek();
            float targetX = targetCx * CELL_SIZE + CELL_SIZE / 2f;
            float targetY = targetCy * CELL_SIZE + CELL_SIZE / 2f;

            float dx = targetX - pos.X;
            float dy = targetY - pos.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist <= 4f)
            {
                path.Dequeue(); // Reached this waypoint
            }
            else
            {
                const float NPC_SPEED = 60f;
                pos.X += (dx / dist) * NPC_SPEED * deltaTime;
                pos.Y += (dy / dist) * NPC_SPEED * deltaTime;
            }
        }
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Compute and return the A* path from <paramref name="startPos"/> to
    /// <paramref name="goalPos"/> within <paramref name="world"/>.
    /// Returns an empty list if no path is found.
    /// </summary>
    public static List<(float x, float y)> FindPath(
        World world,
        (float x, float y) startPos,
        (float x, float y) goalPos)
    {
        var grid = BuildObstacleGrid(world);

        int startCx = (int)(startPos.x / CELL_SIZE);
        int startCy = (int)(startPos.y / CELL_SIZE);
        int goalCx  = (int)(goalPos.x  / CELL_SIZE);
        int goalCy  = (int)(goalPos.y  / CELL_SIZE);

        var rawPath = AStar(grid, (startCx, startCy), (goalCx, goalCy));

        return rawPath
            .Select(cell => (x: cell.cx * CELL_SIZE + CELL_SIZE / 2f,
                             y: cell.cy * CELL_SIZE + CELL_SIZE / 2f))
            .ToList();
    }

    // -----------------------------------------------------------------------
    // A* implementation
    // -----------------------------------------------------------------------

    private record Cell(int cx, int cy);

    private static List<(int cx, int cy)> AStar(
        HashSet<(int, int)> obstacles,
        (int cx, int cy) start,
        (int cx, int cy) goal)
    {
        if (obstacles.Contains(goal))
            return new List<(int, int)>(); // Goal is blocked

        var openSet = new SortedSet<(float f, int id, int cx, int cy)>(
            Comparer<(float f, int id, int cx, int cy)>.Create((a, b) =>
            {
                int cmp = a.f.CompareTo(b.f);
                return cmp != 0 ? cmp : a.id.CompareTo(b.id);
            }));

        var gScore = new Dictionary<(int, int), float>();
        var cameFrom = new Dictionary<(int, int), (int, int)>();

        gScore[start] = 0f;
        openSet.Add((Heuristic(start, goal), 0, start.cx, start.cy));
        int idCounter = 1;
        int iterations = 0;

        while (openSet.Count > 0 && iterations < MAX_ITERATIONS)
        {
            iterations++;
            var current = openSet.Min;
            openSet.Remove(current);
            var node = (current.cx, current.cy);

            if (node == goal)
                return ReconstructPath(cameFrom, goal);

            foreach (var neighbor in GetNeighbors(node))
            {
                if (obstacles.Contains(neighbor)) continue;

                float tentativeG = gScore.GetValueOrDefault(node, float.MaxValue) + 1f;

                if (tentativeG < gScore.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = node;
                    gScore[neighbor] = tentativeG;
                    openSet.Add((tentativeG + Heuristic(neighbor, goal), idCounter++, neighbor.Item1, neighbor.Item2));
                }
            }
        }

        return new List<(int, int)>(); // No path found
    }

    private static float Heuristic((int cx, int cy) a, (int cx, int cy) goal)
    {
        return MathF.Abs(a.cx - goal.cx) + MathF.Abs(a.cy - goal.cy); // Manhattan
    }

    private static List<(int, int)> ReconstructPath(
        Dictionary<(int, int), (int, int)> cameFrom,
        (int, int) current)
    {
        var path = new List<(int, int)> { current };
        while (cameFrom.TryGetValue(current, out var prev))
        {
            path.Insert(0, prev);
            current = prev;
        }
        return path;
    }

    private static IEnumerable<(int, int)> GetNeighbors((int cx, int cy) node)
    {
        yield return (node.cx - 1, node.cy);
        yield return (node.cx + 1, node.cy);
        yield return (node.cx, node.cy - 1);
        yield return (node.cx, node.cy + 1);
        // Diagonal movement
        yield return (node.cx - 1, node.cy - 1);
        yield return (node.cx + 1, node.cy - 1);
        yield return (node.cx - 1, node.cy + 1);
        yield return (node.cx + 1, node.cy + 1);
    }

    // -----------------------------------------------------------------------
    // Obstacle grid construction
    // -----------------------------------------------------------------------

    /// <summary>
    /// Build the set of blocked grid cells from solid collision entities in the world.
    /// </summary>
    private static HashSet<(int, int)> BuildObstacleGrid(World world)
    {
        var obstacles = new HashSet<(int, int)>();

        foreach (var entity in world.GetEntitiesWithComponent<CollisionComponent>())
        {
            var collision = world.GetComponent<CollisionComponent>(entity);
            var pos = world.GetComponent<PositionComponent>(entity);
            if (collision == null || pos == null || !collision.IsStatic) continue;

            // Mark the grid cell(s) the collision box covers
            int minCx = (int)(pos.X / CELL_SIZE);
            int minCy = (int)(pos.Y / CELL_SIZE);
            int maxCx = (int)((pos.X + collision.Width) / CELL_SIZE);
            int maxCy = (int)((pos.Y + collision.Height) / CELL_SIZE);

            for (int cx = minCx; cx <= maxCx; cx++)
                for (int cy = minCy; cy <= maxCy; cy++)
                    obstacles.Add((cx, cy));
        }

        return obstacles;
    }

    // -----------------------------------------------------------------------
    // Path refresh for all NPCs
    // -----------------------------------------------------------------------

    private void RefreshNPCPaths(World world)
    {
        // Find the first player entity to use as target for hostile AI
        Entity? playerEntity = null;
        PositionComponent? playerPos = null;

        foreach (var e in world.GetEntitiesWithComponent<PlayerComponent>())
        {
            playerEntity = e;
            playerPos = world.GetComponent<PositionComponent>(e);
            break;
        }

        var obstacles = BuildObstacleGrid(world);

        foreach (var entity in world.GetEntitiesWithComponent<NPCComponent>())
        {
            var npc = world.GetComponent<NPCComponent>(entity);
            var pos = world.GetComponent<PositionComponent>(entity);
            if (npc == null || pos == null) continue;

            // Determine destination
            (float x, float y) destination = GetNPCDestination(npc, playerPos);
            if (destination == (pos.X, pos.Y)) continue;

            int startCx = (int)(pos.X / CELL_SIZE);
            int startCy = (int)(pos.Y / CELL_SIZE);
            int goalCx  = (int)(destination.x / CELL_SIZE);
            int goalCy  = (int)(destination.y / CELL_SIZE);

            var rawPath = AStar(obstacles, (startCx, startCy), (goalCx, goalCy));

            if (!_npcPaths.TryGetValue(entity, out var pathQueue))
            {
                pathQueue = new Queue<(int, int)>();
                _npcPaths[entity] = pathQueue;
            }
            else
            {
                pathQueue.Clear();
            }

            foreach (var cell in rawPath.Skip(1)) // Skip the start cell
                pathQueue.Enqueue(cell);
        }
    }

    private static (float x, float y) GetNPCDestination(NPCComponent npc, PositionComponent? playerPos)
    {
        // Use the NPC's current scheduled location if available
        // (would need TimeSystem here; use static placeholder for now)
        if (npc.Schedule.Count > 0)
        {
            var firstEntry = npc.Schedule[0];
            return (firstEntry.LocationX, firstEntry.LocationY);
        }

        return (0f, 0f); // Default destination
    }
}
