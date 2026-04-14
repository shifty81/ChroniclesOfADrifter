using ChroniclesOfADrifter.ECS.Components;
using ChroniclesOfADrifter.WorldManagement;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Updates the visual day/night cycle each frame by blending between
/// DayNightPreset values according to the current in-game hour.
/// Entities that own a DayNightVisualComponent receive updated ambient
/// color, brightness, and fog density values.
/// </summary>
public class DayNightVisualSystem : ISystem
{
    private TimeSystem? _timeSystem;

    public void Initialize(World world)
    {
        _timeSystem = world.GetSharedResource<TimeSystem>("TimeSystem");
        Console.WriteLine("[DayNight] Day/Night visual system initialized");
    }

    public void Update(World world, float deltaTime)
    {
        float hour = GetCurrentHour();

        foreach (var entity in world.GetEntitiesWithComponent<DayNightVisualComponent>())
        {
            var dnv = world.GetComponent<DayNightVisualComponent>(entity);
            if (dnv == null) continue;

            var (from, to, t) = dnv.GetBlendFactors(hour);

            dnv.CurrentAmbient   = AmbientColor.Lerp(from.AmbientLight, to.AmbientLight, t);
            dnv.CurrentBrightness = Lerp(from.Brightness,  to.Brightness,  t);
            dnv.CurrentFogDensity = Lerp(from.FogDensity,  to.FogDensity,  t);
            dnv.CurrentPhaseName  = from.Name;
        }
    }

    // -----------------------------------------------------------------------

    private float GetCurrentHour()
    {
        if (_timeSystem != null)
            return _timeSystem.CurrentHour + (_timeSystem.CurrentMinute / 60f);

        // Fallback: use wall clock for demo purposes
        var now = DateTime.Now;
        return now.Hour + now.Minute / 60f;
    }

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    // -----------------------------------------------------------------------
    // Static helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Return a console-friendly description of the current lighting state.
    /// </summary>
    public static string DescribeState(DayNightVisualComponent dnv)
    {
        return $"Phase: {dnv.CurrentPhaseName} | " +
               $"Ambient: {dnv.CurrentAmbient} | " +
               $"Brightness: {dnv.CurrentBrightness:F2} | " +
               $"Fog: {dnv.CurrentFogDensity:F2}";
    }

    /// <summary>
    /// Create a standalone DayNightVisualComponent entity in the world.
    /// </summary>
    public static Entity CreateDayNightEntity(World world)
    {
        var entity = world.CreateEntity();
        world.AddComponent(entity, new DayNightVisualComponent());
        return entity;
    }
}
