using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Manages the minimap: tracks which chunks the player has explored,
/// updates POI markers, and provides render-ready data for the HUD.
/// </summary>
public class MinimapSystem : ISystem
{
    private const int CHUNK_SIZE = 16; // world units per chunk (matches WorldManagement)

    public void Initialize(World world)
    {
        Console.WriteLine("[Minimap] Minimap system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        // Find player entity and minimap component
        foreach (var entity in world.GetEntitiesWithComponent<MinimapComponent>())
        {
            var minimap = world.GetComponent<MinimapComponent>(entity);
            var position = world.GetComponent<PositionComponent>(entity);

            if (minimap == null || position == null) continue;

            minimap.UpdatePlayerPosition(position.X, position.Y);

            // Determine which chunk the player is in
            int playerChunkX = (int)MathF.Floor(position.X / CHUNK_SIZE);
            int playerChunkY = (int)MathF.Floor(position.Y / CHUNK_SIZE);

            // Reveal chunks in visibility radius
            bool anyNew = false;
            for (int dx = -minimap.VisibilityRadius; dx <= minimap.VisibilityRadius; dx++)
            {
                for (int dy = -minimap.VisibilityRadius; dy <= minimap.VisibilityRadius; dy++)
                {
                    if (minimap.ExploreChunk(playerChunkX + dx, playerChunkY + dy))
                        anyNew = true;
                }
            }

            if (anyNew)
            {
                Console.WriteLine($"[Minimap] Player at chunk ({playerChunkX},{playerChunkY}), " +
                                  $"total explored: {minimap.ExploredChunks.Count}");
            }
        }
    }

    // -----------------------------------------------------------------------
    // Static helpers
    // -----------------------------------------------------------------------

    /// <summary>Add a POI marker (e.g. boss arena, chest, village)</summary>
    public static void AddPOI(World world, Entity mapEntity, float worldX, float worldY,
        MinimapTileType type)
    {
        var minimap = world.GetComponent<MinimapComponent>(mapEntity);
        minimap?.AddPOI((int)worldX, (int)worldY, type);
    }

    /// <summary>Remove a POI marker</summary>
    public static void RemovePOI(World world, Entity mapEntity, float worldX, float worldY)
    {
        var minimap = world.GetComponent<MinimapComponent>(mapEntity);
        minimap?.RemovePOI((int)worldX, (int)worldY);
    }

    /// <summary>
    /// Return a text-mode render of the minimap centered on the player (for console/debug).
    /// Each character represents one chunk; explored = '.', unexplored = ' ', player = '@'.
    /// </summary>
    public static string RenderASCII(MinimapComponent minimap, int radius = 8)
    {
        int playerChunkX = (int)MathF.Floor(minimap.PlayerWorldX / CHUNK_SIZE);
        int playerChunkY = (int)MathF.Floor(minimap.PlayerWorldY / CHUNK_SIZE);

        int size = radius * 2 + 1;
        var lines = new System.Text.StringBuilder();
        lines.AppendLine("+" + new string('-', size) + "+");

        for (int dy = -radius; dy <= radius; dy++)
        {
            lines.Append("|");
            for (int dx = -radius; dx <= radius; dx++)
            {
                int cx = playerChunkX + dx;
                int cy = playerChunkY + dy;
                if (dx == 0 && dy == 0)
                    lines.Append('@');
                else
                {
                    // Check POI first
                    bool isPOI = minimap.POIMarkers.Any(p =>
                        (int)MathF.Floor(p.WorldX / (float)CHUNK_SIZE) == cx &&
                        (int)MathF.Floor(p.WorldY / (float)CHUNK_SIZE) == cy);

                    if (isPOI)
                        lines.Append('!');
                    else if (minimap.IsChunkExplored(cx, cy))
                        lines.Append('.');
                    else
                        lines.Append(' ');
                }
            }
            lines.AppendLine("|");
        }

        lines.AppendLine("+" + new string('-', size) + "+");
        return lines.ToString();
    }

    /// <summary>Toggle minimap visibility</summary>
    public static void ToggleVisibility(World world, Entity mapEntity)
    {
        var minimap = world.GetComponent<MinimapComponent>(mapEntity);
        if (minimap != null)
            minimap.IsVisible = !minimap.IsVisible;
    }
}
