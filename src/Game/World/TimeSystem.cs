namespace ChroniclesOfADrifter.WorldManagement;

/// <summary>
/// Represents the different phases of the day
/// </summary>
public enum DayPhase
{
    Dawn,      // 5:00 - 7:00 (sun rising)
    Day,       // 7:00 - 17:00 (full daylight)
    Dusk,      // 17:00 - 19:00 (sun setting)
    Night      // 19:00 - 5:00 (darkness)
}

/// <summary>
/// Manages the day/night cycle and time progression
/// Integrates with lighting, weather, and creature spawning systems
/// </summary>
public class TimeSystem
{
    // Constants
    private const float DEFAULT_TIME_SCALE = 60f; // 1 real second = 1 game minute (24 real minutes = 1 game day)
    private const float SECONDS_PER_GAME_DAY = 86400f; // 24 hours in seconds
    
    // Time state
    private float _currentTime; // Time in seconds since start of day (0-86400)
    private float _timeScale;
    private int _dayCount;
    
    // Phase thresholds (in hours)
    private const float DAWN_START = 5f;
    private const float DAY_START = 7f;
    private const float DUSK_START = 17f;
    private const float NIGHT_START = 19f;
    
    /// <summary>
    /// Gets the current time in seconds since midnight (0-86400)
    /// </summary>
    public float CurrentTime => _currentTime;
    
    /// <summary>
    /// Gets the current hour (0-23)
    /// </summary>
    public int CurrentHour => (int)(_currentTime / 3600f) % 24;
    
    /// <summary>
    /// Gets the current minute (0-59)
    /// </summary>
    public int CurrentMinute => (int)((_currentTime % 3600f) / 60f);
    
    /// <summary>
    /// Gets the current day count (starts at 0)
    /// </summary>
    public int DayCount => _dayCount;
    
    /// <summary>
    /// Gets or sets the time scale (how fast time passes)
    /// 1.0 = real-time, 60.0 = 1 real second = 1 game minute
    /// </summary>
    public float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Math.Max(0f, value);
    }
    
    /// <summary>
    /// Gets the current phase of the day
    /// </summary>
    public DayPhase CurrentPhase
    {
        get
        {
            float hour = _currentTime / 3600f;
            
            if (hour >= DAWN_START && hour < DAY_START)
                return DayPhase.Dawn;
            else if (hour >= DAY_START && hour < DUSK_START)
                return DayPhase.Day;
            else if (hour >= DUSK_START && hour < NIGHT_START)
                return DayPhase.Dusk;
            else
                return DayPhase.Night;
        }
    }
    
    /// <summary>
    /// Gets a normalized value (0-1) representing how far through the current phase we are
    /// </summary>
    public float PhaseProgress
    {
        get
        {
            float hour = _currentTime / 3600f;
            
            return CurrentPhase switch
            {
                DayPhase.Dawn => (hour - DAWN_START) / (DAY_START - DAWN_START),
                DayPhase.Day => (hour - DAY_START) / (DUSK_START - DAY_START),
                DayPhase.Dusk => (hour - DUSK_START) / (NIGHT_START - DUSK_START),
                DayPhase.Night => hour >= NIGHT_START 
                    ? (hour - NIGHT_START) / (24f + DAWN_START - NIGHT_START)
                    : (24f - NIGHT_START + hour) / (24f + DAWN_START - NIGHT_START),
                _ => 0f
            };
        }
    }
    
    /// <summary>
    /// Creates a new time system
    /// </summary>
    /// <param name="startHour">The hour to start at (0-23), default is 8 AM</param>
    /// <param name="timeScale">How fast time passes (default 60x)</param>
    public TimeSystem(int startHour = 8, float timeScale = DEFAULT_TIME_SCALE)
    {
        _currentTime = startHour * 3600f;
        _timeScale = timeScale;
        _dayCount = 0;
    }
    
    /// <summary>
    /// Updates the time system
    /// </summary>
    /// <param name="deltaTime">Time elapsed in seconds (real-time)</param>
    public void Update(float deltaTime)
    {
        // Advance time based on time scale
        _currentTime += deltaTime * _timeScale;
        
        // Handle day rollover
        if (_currentTime >= SECONDS_PER_GAME_DAY)
        {
            _currentTime -= SECONDS_PER_GAME_DAY;
            _dayCount++;
        }
    }
    
    /// <summary>
    /// Gets the ambient light level for the current time of day
    /// Returns a value between 0.0 (pitch black) and 1.0 (full daylight)
    /// </summary>
    public float GetAmbientLightLevel()
    {
        float hour = _currentTime / 3600f;
        
        // Dawn (5:00 - 7:00): Light increases from 0.2 to 1.0
        if (hour >= DAWN_START && hour < DAY_START)
        {
            float progress = (hour - DAWN_START) / (DAY_START - DAWN_START);
            return 0.2f + progress * 0.8f; // 0.2 -> 1.0
        }
        // Day (7:00 - 17:00): Full daylight
        else if (hour >= DAY_START && hour < DUSK_START)
        {
            return 1.0f;
        }
        // Dusk (17:00 - 19:00): Light decreases from 1.0 to 0.2
        else if (hour >= DUSK_START && hour < NIGHT_START)
        {
            float progress = (hour - DUSK_START) / (NIGHT_START - DUSK_START);
            return 1.0f - progress * 0.8f; // 1.0 -> 0.2
        }
        // Night (19:00 - 5:00): Minimal moonlight
        else
        {
            return 0.2f;
        }
    }
    
    /// <summary>
    /// Gets the atmospheric color tint for the current time of day
    /// Returns RGBA values (0-1 range)
    /// </summary>
    public (float r, float g, float b, float a) GetAtmosphereTint()
    {
        float hour = _currentTime / 3600f;
        
        // Dawn: Orange/pink sunrise colors
        if (hour >= DAWN_START && hour < DAY_START)
        {
            float progress = (hour - DAWN_START) / (DAY_START - DAWN_START);
            float r = 1.0f - progress * 0.1f; // 1.0 -> 0.9
            float g = 0.6f + progress * 0.4f; // 0.6 -> 1.0
            float b = 0.4f + progress * 0.6f; // 0.4 -> 1.0
            return (r, g, b, 0.3f);
        }
        // Day: Neutral/bright
        else if (hour >= DAY_START && hour < DUSK_START)
        {
            return (1.0f, 1.0f, 1.0f, 0.1f);
        }
        // Dusk: Orange/red sunset colors
        else if (hour >= DUSK_START && hour < NIGHT_START)
        {
            float progress = (hour - DUSK_START) / (NIGHT_START - DUSK_START);
            float r = 1.0f;
            float g = 0.6f - progress * 0.1f; // 0.6 -> 0.5
            float b = 0.4f - progress * 0.2f; // 0.4 -> 0.2
            return (r, g, b, 0.4f);
        }
        // Night: Dark blue
        else
        {
            return (0.3f, 0.3f, 0.5f, 0.6f);
        }
    }
    
    /// <summary>
    /// Gets the creature spawn rate multiplier for the current time
    /// Night has more hostile creatures, day has more peaceful ones
    /// </summary>
    public float GetHostileSpawnMultiplier()
    {
        return CurrentPhase switch
        {
            DayPhase.Dawn => 0.8f,   // Some lingering night creatures
            DayPhase.Day => 0.5f,    // Fewer hostile creatures during day
            DayPhase.Dusk => 0.8f,   // Hostiles starting to emerge
            DayPhase.Night => 1.5f,  // More hostile creatures at night
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Gets the peaceful creature spawn rate multiplier for the current time
    /// </summary>
    public float GetPeacefulSpawnMultiplier()
    {
        return CurrentPhase switch
        {
            DayPhase.Dawn => 1.0f,
            DayPhase.Day => 1.2f,    // More peaceful creatures during day
            DayPhase.Dusk => 1.0f,
            DayPhase.Night => 0.3f,  // Fewer peaceful creatures at night
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Checks if it's currently daytime (between 7:00 and 19:00)
    /// </summary>
    public bool IsDaytime()
    {
        return CurrentPhase == DayPhase.Day;
    }
    
    /// <summary>
    /// Checks if it's currently nighttime (between 19:00 and 5:00)
    /// </summary>
    public bool IsNighttime()
    {
        return CurrentPhase == DayPhase.Night;
    }
    
    /// <summary>
    /// Sets the current time to a specific hour
    /// </summary>
    /// <param name="hour">Hour to set (0-23)</param>
    public void SetTime(int hour)
    {
        hour = Math.Clamp(hour, 0, 23);
        _currentTime = hour * 3600f;
    }
    
    /// <summary>
    /// Restores time state from saved data (used by SaveSystem)
    /// </summary>
    /// <param name="currentTime">Time in seconds since midnight (0-86400)</param>
    /// <param name="dayCount">Number of days elapsed</param>
    public void RestoreTimeState(float currentTime, int dayCount)
    {
        _currentTime = Math.Clamp(currentTime, 0f, SECONDS_PER_GAME_DAY);
        _dayCount = Math.Max(0, dayCount);
    }
    
    /// <summary>
    /// Gets a formatted time string (HH:MM format)
    /// </summary>
    public string GetTimeString()
    {
        return $"{CurrentHour:D2}:{CurrentMinute:D2}";
    }
    
    /// <summary>
    /// Gets a formatted time string with phase (e.g., "08:30 (Day)")
    /// </summary>
    public string GetTimeStringWithPhase()
    {
        return $"{GetTimeString()} ({CurrentPhase})";
    }
}
