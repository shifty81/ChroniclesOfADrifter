using ChroniclesOfADrifter.ECS.Components;

namespace ChroniclesOfADrifter.ECS.Systems;

/// <summary>
/// Audio system - manages sound effect playback and background music.
/// This is a framework-level implementation that logs audio events to console.
/// When a real audio engine is integrated, the PlaySound/PlayMusic methods
/// should be connected to the actual audio backend.
/// </summary>
public class AudioSystem : ISystem
{
    private MusicTrack? _currentMusic;
    private float _masterVolume;
    private float _sfxVolume;
    private float _musicVolume;
    private bool _enabled;

    public AudioSystem(float masterVolume = 1.0f)
    {
        _masterVolume = masterVolume;
        _sfxVolume = 1.0f;
        _musicVolume = 0.7f;
        _enabled = true;
        _currentMusic = null;
    }

    public void Initialize(World world)
    {
        Console.WriteLine("[Audio] Audio system initialized (framework mode)");
    }

    public void Update(World world, float deltaTime)
    {
        if (!_enabled) return;

        // Process pending sounds from all AudioComponents
        foreach (var entity in world.GetEntitiesWithComponent<AudioComponent>())
        {
            var audio = world.GetComponent<AudioComponent>(entity);
            if (audio == null) continue;

            // Process and clear pending sounds
            foreach (var soundEvent in audio.PendingSounds)
            {
                PlaySound(soundEvent.Sound, soundEvent.Volume * _sfxVolume * _masterVolume);
            }
            audio.PendingSounds.Clear();
        }
    }

    /// <summary>
    /// Play a sound effect (framework: logs to console)
    /// </summary>
    public static void PlaySound(SoundEffect sound, float volume = 1.0f)
    {
        // Framework implementation - log the sound event
        // When real audio is integrated, this should call the audio backend
        Console.WriteLine($"[Audio] SFX: {sound} (vol: {volume:F1})");
    }

    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayMusic(MusicTrack track)
    {
        if (_currentMusic == track) return;

        _currentMusic = track;
        Console.WriteLine($"[Audio] Music: Now playing {track} (vol: {_musicVolume * _masterVolume:F1})");
    }

    /// <summary>
    /// Stop background music
    /// </summary>
    public void StopMusic()
    {
        if (_currentMusic != null)
        {
            Console.WriteLine($"[Audio] Music: Stopped {_currentMusic}");
            _currentMusic = null;
        }
    }

    /// <summary>
    /// Get the currently playing music track
    /// </summary>
    public MusicTrack? CurrentMusic => _currentMusic;

    /// <summary>
    /// Set master volume (0.0 to 1.0)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        _masterVolume = Math.Clamp(volume, 0f, 1f);
    }

    /// <summary>
    /// Set sound effects volume (0.0 to 1.0)
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        _sfxVolume = Math.Clamp(volume, 0f, 1f);
    }

    /// <summary>
    /// Set music volume (0.0 to 1.0)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        _musicVolume = Math.Clamp(volume, 0f, 1f);
    }

    /// <summary>
    /// Enable or disable audio
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        _enabled = enabled;
        if (!enabled)
        {
            StopMusic();
        }
    }
}
