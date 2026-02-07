namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Types of sound effects in the game
/// </summary>
public enum SoundEffect
{
    // Combat
    SwordSwing,
    SwordHit,
    BowShoot,
    ArrowHit,
    MagicCast,
    PlayerHurt,
    EnemyHurt,
    EnemyDeath,
    PlayerDeath,

    // Environment
    FootstepGrass,
    FootstepStone,
    FootstepSand,
    FootstepWater,
    WaterSplash,
    Mining,
    BlockBreak,
    BlockPlace,

    // UI/Feedback
    ItemPickup,
    InventoryOpen,
    InventoryClose,
    CraftingSuccess,
    LevelUp,
    QuestComplete,

    // Farming
    TillGround,
    PlantSeed,
    WaterCrop,
    HarvestCrop,

    // Weather
    RainAmbient,
    ThunderClap,
    WindGust
}

/// <summary>
/// Types of background music tracks
/// </summary>
public enum MusicTrack
{
    MainTheme,
    Overworld,
    Underground,
    Combat,
    Boss,
    Village,
    Night,
    Victory
}

/// <summary>
/// A queued sound event
/// </summary>
public class SoundEvent
{
    public SoundEffect Sound { get; set; }
    public float Volume { get; set; }
    public bool Loop { get; set; }
    public float Delay { get; set; }

    public SoundEvent(SoundEffect sound, float volume = 1.0f, bool loop = false, float delay = 0f)
    {
        Sound = sound;
        Volume = volume;
        Loop = loop;
        Delay = delay;
    }
}

/// <summary>
/// Component for entities that can produce sounds
/// </summary>
public class AudioComponent : IComponent
{
    public List<SoundEvent> PendingSounds { get; set; }
    public SoundEffect? LoopingSound { get; set; }
    public float Volume { get; set; }

    public AudioComponent(float volume = 1.0f)
    {
        PendingSounds = new List<SoundEvent>();
        LoopingSound = null;
        Volume = volume;
    }

    /// <summary>
    /// Queue a sound effect to be played
    /// </summary>
    public void PlaySound(SoundEffect sound, float volume = 1.0f)
    {
        PendingSounds.Add(new SoundEvent(sound, volume * Volume));
    }

    /// <summary>
    /// Start a looping sound
    /// </summary>
    public void StartLoop(SoundEffect sound)
    {
        LoopingSound = sound;
    }

    /// <summary>
    /// Stop looping sound
    /// </summary>
    public void StopLoop()
    {
        LoopingSound = null;
    }
}
