using ChroniclesOfADrifter.Terrain;

namespace ChroniclesOfADrifter.WorldManagement;

/// <summary>
/// Types of weather that can occur in the game
/// </summary>
public enum WeatherType
{
    Clear,
    Rain,
    Snow,
    Fog,
    Storm,
    Sandstorm
}

/// <summary>
/// Intensity of weather effects
/// </summary>
public enum WeatherIntensity
{
    Light,
    Moderate,
    Heavy
}

/// <summary>
/// Manages weather and atmospheric effects in the game world.
/// Weather changes based on biome, time, and random events.
/// </summary>
public class WeatherSystem
{
    private WeatherType currentWeather;
    private WeatherIntensity currentIntensity;
    private float weatherTimer;
    private float transitionTimer;
    private float weatherDuration;
    private bool isTransitioning;
    private WeatherType targetWeather;
    
    private readonly Random random;
    private readonly int worldSeed;
    
    // Weather configuration
    private const float MIN_WEATHER_DURATION = 300f;  // 5 minutes
    private const float MAX_WEATHER_DURATION = 900f;  // 15 minutes
    private const float TRANSITION_DURATION = 30f;    // 30 seconds
    private const float WEATHER_CHECK_INTERVAL = 60f; // Check every minute
    
    /// <summary>
    /// Probability of weather change for each biome
    /// </summary>
    private readonly Dictionary<BiomeType, float> biomeWeatherProbability = new()
    {
        { BiomeType.Plains, 0.3f },
        { BiomeType.Desert, 0.1f },
        { BiomeType.Forest, 0.4f },
        { BiomeType.Snow, 0.6f },
        { BiomeType.Swamp, 0.5f },
        { BiomeType.Rocky, 0.2f },
        { BiomeType.Jungle, 0.7f },
        { BiomeType.Beach, 0.3f }
    };
    
    /// <summary>
    /// Possible weather types for each biome
    /// </summary>
    private readonly Dictionary<BiomeType, List<WeatherType>> biomeWeatherTypes = new()
    {
        { BiomeType.Plains, new List<WeatherType> { WeatherType.Clear, WeatherType.Rain, WeatherType.Fog } },
        { BiomeType.Desert, new List<WeatherType> { WeatherType.Clear, WeatherType.Sandstorm } },
        { BiomeType.Forest, new List<WeatherType> { WeatherType.Clear, WeatherType.Rain, WeatherType.Fog } },
        { BiomeType.Snow, new List<WeatherType> { WeatherType.Clear, WeatherType.Snow, WeatherType.Fog } },
        { BiomeType.Swamp, new List<WeatherType> { WeatherType.Clear, WeatherType.Rain, WeatherType.Fog } },
        { BiomeType.Rocky, new List<WeatherType> { WeatherType.Clear, WeatherType.Fog } },
        { BiomeType.Jungle, new List<WeatherType> { WeatherType.Clear, WeatherType.Rain, WeatherType.Storm } },
        { BiomeType.Beach, new List<WeatherType> { WeatherType.Clear, WeatherType.Rain, WeatherType.Fog } }
    };
    
    public WeatherType CurrentWeather => currentWeather;
    public WeatherIntensity CurrentIntensity => currentIntensity;
    public bool IsTransitioning => isTransitioning;
    public float TransitionProgress => isTransitioning ? (transitionTimer / TRANSITION_DURATION) : 1.0f;
    
    public WeatherSystem(int seed)
    {
        worldSeed = seed;
        random = new Random(seed);
        currentWeather = WeatherType.Clear;
        currentIntensity = WeatherIntensity.Light;
        weatherTimer = 0f;
        weatherDuration = random.Next((int)MIN_WEATHER_DURATION, (int)MAX_WEATHER_DURATION);
        isTransitioning = false;
    }
    
    /// <summary>
    /// Updates the weather system
    /// </summary>
    public void Update(BiomeType currentBiome, float deltaTime)
    {
        if (isTransitioning)
        {
            UpdateTransition(deltaTime);
        }
        else
        {
            UpdateWeather(currentBiome, deltaTime);
        }
    }
    
    /// <summary>
    /// Updates weather transitions
    /// </summary>
    private void UpdateTransition(float deltaTime)
    {
        transitionTimer += deltaTime;
        
        if (transitionTimer >= TRANSITION_DURATION)
        {
            // Transition complete
            currentWeather = targetWeather;
            isTransitioning = false;
            weatherTimer = 0f;
            weatherDuration = random.Next((int)MIN_WEATHER_DURATION, (int)MAX_WEATHER_DURATION);
        }
    }
    
    /// <summary>
    /// Updates the current weather
    /// </summary>
    private void UpdateWeather(BiomeType currentBiome, float deltaTime)
    {
        weatherTimer += deltaTime;
        
        // Check if it's time to potentially change weather
        if (weatherTimer >= weatherDuration)
        {
            TryChangeWeather(currentBiome);
        }
    }
    
    /// <summary>
    /// Attempts to change the weather based on biome
    /// </summary>
    private void TryChangeWeather(BiomeType biome)
    {
        float changeProbability = biomeWeatherProbability.GetValueOrDefault(biome, 0.3f);
        
        // Should weather change?
        if (random.NextDouble() < changeProbability)
        {
            var possibleWeathers = biomeWeatherTypes.GetValueOrDefault(biome, 
                new List<WeatherType> { WeatherType.Clear });
            
            // Select new weather (different from current)
            var availableWeathers = possibleWeathers.Where(w => w != currentWeather).ToList();
            if (availableWeathers.Count > 0)
            {
                targetWeather = availableWeathers[random.Next(availableWeathers.Count)];
                StartWeatherTransition();
            }
            else
            {
                // No other weather available, extend current weather
                weatherTimer = 0f;
                weatherDuration = random.Next((int)MIN_WEATHER_DURATION, (int)MAX_WEATHER_DURATION);
            }
        }
        else
        {
            // No change, extend current weather
            weatherTimer = 0f;
            weatherDuration = random.Next((int)MIN_WEATHER_DURATION, (int)MAX_WEATHER_DURATION);
        }
    }
    
    /// <summary>
    /// Starts a weather transition
    /// </summary>
    private void StartWeatherTransition()
    {
        isTransitioning = true;
        transitionTimer = 0f;
        
        // Randomly select intensity for new weather
        if (targetWeather != WeatherType.Clear)
        {
            currentIntensity = (WeatherIntensity)random.Next(0, 3);
        }
        else
        {
            currentIntensity = WeatherIntensity.Light;
        }
    }
    
    /// <summary>
    /// Forces an immediate weather change (for testing or scripted events)
    /// </summary>
    public void SetWeather(WeatherType weather, WeatherIntensity intensity = WeatherIntensity.Moderate)
    {
        targetWeather = weather;
        currentIntensity = intensity;
        StartWeatherTransition();
    }
    
    /// <summary>
    /// Restores weather state from saved data (used by SaveSystem)
    /// </summary>
    /// <param name="weather">The weather type to restore</param>
    /// <param name="intensity">The weather intensity to restore</param>
    /// <param name="timer">Time elapsed in current weather</param>
    /// <param name="duration">Total duration of current weather</param>
    public void RestoreWeatherState(WeatherType weather, WeatherIntensity intensity, float timer, float duration)
    {
        currentWeather = weather;
        currentIntensity = intensity;
        weatherTimer = Math.Max(0f, timer);
        weatherDuration = Math.Clamp(duration, MIN_WEATHER_DURATION, MAX_WEATHER_DURATION);
        isTransitioning = false;
    }
    
    /// <summary>
    /// Gets the current weather effect multiplier (affects visibility, speed, etc.)
    /// Returns a value between 0.0 and 1.0, where 1.0 is no effect
    /// </summary>
    public float GetVisibilityMultiplier()
    {
        float baseMultiplier = currentWeather switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Rain => 0.8f,
            WeatherType.Snow => 0.7f,
            WeatherType.Fog => 0.6f,
            WeatherType.Storm => 0.5f,
            WeatherType.Sandstorm => 0.4f,
            _ => 1.0f
        };
        
        // Adjust based on intensity
        float intensityMultiplier = currentIntensity switch
        {
            WeatherIntensity.Light => 0.9f,
            WeatherIntensity.Moderate => 0.75f,
            WeatherIntensity.Heavy => 0.5f,
            _ => 1.0f
        };
        
        return baseMultiplier * intensityMultiplier;
    }
    
    /// <summary>
    /// Gets the movement speed multiplier for current weather
    /// </summary>
    public float GetMovementSpeedMultiplier()
    {
        return currentWeather switch
        {
            WeatherType.Clear => 1.0f,
            WeatherType.Rain => 0.95f,
            WeatherType.Snow => 0.85f,
            WeatherType.Fog => 0.95f,
            WeatherType.Storm => 0.8f,
            WeatherType.Sandstorm => 0.7f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Checks if current weather damages entities
    /// </summary>
    public bool IsDamagingWeather()
    {
        return (currentWeather == WeatherType.Storm || currentWeather == WeatherType.Sandstorm) 
               && currentIntensity == WeatherIntensity.Heavy;
    }
    
    /// <summary>
    /// Gets the damage per second from weather (if applicable)
    /// </summary>
    public float GetWeatherDamagePerSecond()
    {
        if (!IsDamagingWeather()) return 0f;
        
        return currentWeather switch
        {
            WeatherType.Storm => 2f,
            WeatherType.Sandstorm => 1.5f,
            _ => 0f
        };
    }
    
    /// <summary>
    /// Gets atmospheric color tint for current weather
    /// Returns RGB values normalized (0.0 to 1.0)
    /// </summary>
    public (float r, float g, float b, float a) GetWeatherTint()
    {
        // Base tint for weather type
        var (r, g, b) = currentWeather switch
        {
            WeatherType.Clear => (1.0f, 1.0f, 1.0f),
            WeatherType.Rain => (0.7f, 0.75f, 0.8f),
            WeatherType.Snow => (0.9f, 0.95f, 1.0f),
            WeatherType.Fog => (0.85f, 0.85f, 0.85f),
            WeatherType.Storm => (0.5f, 0.55f, 0.6f),
            WeatherType.Sandstorm => (0.9f, 0.75f, 0.5f),
            _ => (1.0f, 1.0f, 1.0f)
        };
        
        // Adjust alpha based on intensity and transition
        float alpha = currentIntensity switch
        {
            WeatherIntensity.Light => 0.3f,
            WeatherIntensity.Moderate => 0.5f,
            WeatherIntensity.Heavy => 0.7f,
            _ => 0.3f
        };
        
        // Fade during transition
        if (isTransitioning)
        {
            alpha *= TransitionProgress;
        }
        
        return (r, g, b, alpha);
    }
    
    /// <summary>
    /// Gets statistics about the weather system (for debugging/UI)
    /// </summary>
    public string GetWeatherInfo()
    {
        string status = isTransitioning ? 
            $"Transitioning to {targetWeather} ({TransitionProgress * 100:F0}%)" :
            $"{currentWeather} ({currentIntensity})";
        
        float timeRemaining = weatherDuration - weatherTimer;
        int minutes = (int)(timeRemaining / 60f);
        int seconds = (int)(timeRemaining % 60f);
        
        return $"Weather: {status} | Time remaining: {minutes}m {seconds}s";
    }
}
