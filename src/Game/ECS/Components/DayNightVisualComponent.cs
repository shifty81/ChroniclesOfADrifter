namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Represents an RGBA color for ambient lighting
/// </summary>
public struct AmbientColor
{
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public AmbientColor(float r, float g, float b, float a = 1f)
    {
        R = r; G = g; B = b; A = a;
    }

    public static AmbientColor Lerp(AmbientColor a, AmbientColor b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new AmbientColor(
            a.R + (b.R - a.R) * t,
            a.G + (b.G - a.G) * t,
            a.B + (b.B - a.B) * t,
            a.A + (b.A - a.A) * t
        );
    }

    public override string ToString() =>
        $"({R:F2},{G:F2},{B:F2},{A:F2})";
}

/// <summary>
/// Named time-of-day lighting preset
/// </summary>
public class DayNightPreset
{
    public string Name { get; set; }
    public float StartHour { get; set; }
    public AmbientColor AmbientLight { get; set; }
    public float Brightness { get; set; }
    public float FogDensity { get; set; }

    public DayNightPreset(string name, float startHour,
        AmbientColor ambientLight, float brightness, float fogDensity)
    {
        Name = name;
        StartHour = startHour;
        AmbientLight = ambientLight;
        Brightness = brightness;
        FogDensity = fogDensity;
    }
}

/// <summary>
/// Component that drives the visual day/night cycle:
/// ambient color, brightness, and fog density that blend based on time of day.
/// </summary>
public class DayNightVisualComponent : IComponent
{
    /// <summary>Current ambient color (interpolated each frame)</summary>
    public AmbientColor CurrentAmbient { get; set; }

    /// <summary>Current scene brightness multiplier [0,1]</summary>
    public float CurrentBrightness { get; set; }

    /// <summary>Current fog density [0,1]</summary>
    public float CurrentFogDensity { get; set; }

    /// <summary>Ordered list of day/night presets (sorted by StartHour)</summary>
    public List<DayNightPreset> Presets { get; private set; }

    /// <summary>Name of the phase currently active</summary>
    public string CurrentPhaseName { get; set; }

    public DayNightVisualComponent()
    {
        CurrentPhaseName = "Day";
        CurrentBrightness = 1f;
        CurrentFogDensity = 0f;
        CurrentAmbient = new AmbientColor(1f, 1f, 1f);
        Presets = BuildDefaultPresets();
    }

    /// <summary>
    /// Default presets covering midnight through the following midnight
    /// </summary>
    private static List<DayNightPreset> BuildDefaultPresets()
    {
        return new List<DayNightPreset>
        {
            new("Midnight",  0f,  new AmbientColor(0.05f, 0.05f, 0.15f), 0.10f, 0.20f),
            new("Dawn",      5f,  new AmbientColor(0.90f, 0.55f, 0.30f), 0.55f, 0.10f),
            new("Morning",   7f,  new AmbientColor(1.00f, 0.95f, 0.80f), 0.90f, 0.00f),
            new("Noon",     12f,  new AmbientColor(1.00f, 1.00f, 1.00f), 1.00f, 0.00f),
            new("Afternoon",15f,  new AmbientColor(1.00f, 0.95f, 0.75f), 0.90f, 0.00f),
            new("Dusk",     17f,  new AmbientColor(0.90f, 0.45f, 0.20f), 0.55f, 0.08f),
            new("Evening",  19f,  new AmbientColor(0.15f, 0.10f, 0.25f), 0.20f, 0.15f),
            new("Night",    21f,  new AmbientColor(0.05f, 0.05f, 0.15f), 0.10f, 0.20f),
        };
    }

    /// <summary>
    /// Return the two surrounding presets and blend factor t for a given hour.
    /// </summary>
    public (DayNightPreset from, DayNightPreset to, float t) GetBlendFactors(float hour)
    {
        var sorted = Presets;
        for (int i = sorted.Count - 1; i >= 0; i--)
        {
            if (hour >= sorted[i].StartHour)
            {
                var from = sorted[i];
                var to   = sorted[(i + 1) % sorted.Count];
                float next = to.StartHour <= from.StartHour ? to.StartHour + 24f : to.StartHour;
                float t = (hour - from.StartHour) / (next - from.StartHour);
                return (from, to, Math.Clamp(t, 0f, 1f));
            }
        }
        // Fallback: wrap around (late night before first preset)
        var last = sorted[sorted.Count - 1];
        var first = sorted[0];
        float span = (first.StartHour + 24f) - last.StartHour;
        float tt = (hour + 24f - last.StartHour) / span;
        return (last, first, Math.Clamp(tt, 0f, 1f));
    }
}
