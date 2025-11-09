using System.Text.Json;

namespace ChroniclesOfADrifter.Engine;

/// <summary>
/// Manages game settings including renderer configuration
/// Settings can be changed in the game settings menu and will trigger a restart
/// </summary>
public class SettingsManager
{
    private const string SettingsFilePath = "settings.json";
    
    public class GameSettings
    {
        public RendererSettings Renderer { get; set; } = new();
        public GraphicsSettings Graphics { get; set; } = new();
        public AudioSettings Audio { get; set; } = new();
        public GameplaySettings Gameplay { get; set; } = new();
        public string Note { get; set; } = "This configuration file can be modified in the game settings menu. Changing renderer settings will restart the game.";
    }
    
    public class RendererSettings
    {
        public string Backend { get; set; } = "dx11"; // Default to DirectX 11 on Windows
        public string Description { get; set; } = "Renderer backend: dx11 (default), dx12 (high-performance), or sdl2 (cross-platform)";
        public int WindowWidth { get; set; } = 1920;
        public int WindowHeight { get; set; } = 1080;
        public bool Vsync { get; set; } = true;
        public bool Fullscreen { get; set; } = false;
    }
    
    public class GraphicsSettings
    {
        public string Quality { get; set; } = "high";
        public bool Antialiasing { get; set; } = true;
        public bool Shadows { get; set; } = true;
    }
    
    public class AudioSettings
    {
        public float MasterVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 0.8f;
        public float SfxVolume { get; set; } = 1.0f;
    }
    
    public class GameplaySettings
    {
        public string Difficulty { get; set; } = "normal";
        public bool ShowTutorials { get; set; } = true;
    }
    
    /// <summary>
    /// Load settings from file or create default settings if file doesn't exist
    /// </summary>
    public static GameSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<GameSettings>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                });
                
                if (settings != null)
                {
                    Console.WriteLine($"[Settings] Loaded settings from {SettingsFilePath}");
                    Console.WriteLine($"[Settings] Renderer backend: {settings.Renderer.Backend}");
                    return settings;
                }
            }
            
            // Create default settings if file doesn't exist or failed to deserialize
            Console.WriteLine($"[Settings] Creating default settings");
            var defaultSettings = new GameSettings();
            SaveSettings(defaultSettings);
            return defaultSettings;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Settings] Error loading settings: {ex.Message}");
            Console.WriteLine($"[Settings] Using default settings");
            return new GameSettings();
        }
    }
    
    /// <summary>
    /// Save settings to file
    /// </summary>
    public static void SaveSettings(GameSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(SettingsFilePath, json);
            Console.WriteLine($"[Settings] Settings saved to {SettingsFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Settings] Error saving settings: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Apply renderer settings to environment for the engine to pick up
    /// </summary>
    public static void ApplyRendererSettings(GameSettings settings)
    {
        Environment.SetEnvironmentVariable("CHRONICLES_RENDERER", settings.Renderer.Backend);
        Console.WriteLine($"[Settings] Applied renderer backend: {settings.Renderer.Backend}");
    }
    
    /// <summary>
    /// Change renderer backend and trigger game restart
    /// This would be called from the in-game settings menu
    /// </summary>
    public static void ChangeRendererBackend(string newBackend)
    {
        var settings = LoadSettings();
        settings.Renderer.Backend = newBackend;
        SaveSettings(settings);
        
        Console.WriteLine($"[Settings] Renderer changed to {newBackend}");
        Console.WriteLine($"[Settings] Game will restart with new renderer...");
        
        // In a real implementation, this would:
        // 1. Save current game state
        // 2. Shutdown current engine instance
        // 3. Restart the game with new renderer settings
        // For now, we just notify the user
        Console.WriteLine($"[Settings] Please restart the game to apply changes");
    }
}
