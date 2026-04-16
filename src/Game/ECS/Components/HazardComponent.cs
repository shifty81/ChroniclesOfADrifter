namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Type of environmental hazard
/// </summary>
public enum HazardType
{
    SpikeTrap,    // Floor spikes that pop up periodically
    LavaPool,     // Persistent area damage from lava
    AcidPool,     // Corroding damage that also degrades equipment
    GasVent,      // Periodic poison gas bursts
    BearTrap,     // Point trap that immobilises on contact
    ElectricFence // Arc damage in a line
}

/// <summary>
/// Component that marks an entity as an environmental hazard
/// </summary>
public class HazardComponent : IComponent
{
    public HazardType Type { get; set; }

    /// <summary>Damage dealt per second while inside the hazard area</summary>
    public float DamagePerSecond { get; set; }

    /// <summary>Radius of effect around the hazard entity's position</summary>
    public float TriggerRadius { get; set; }

    /// <summary>Minimum seconds between successive triggers for non-continuous hazards</summary>
    public float TriggerCooldown { get; set; }

    /// <summary>Elapsed time since the last trigger</summary>
    public float TimeSinceLastTrigger { get; set; }

    /// <summary>Whether the hazard is currently active</summary>
    public bool IsActive { get; set; }

    /// <summary>For periodic hazards (SpikeTrap, GasVent): seconds the hazard stays on</summary>
    public float ActiveDuration { get; set; }

    /// <summary>For periodic hazards: seconds the hazard stays off between bursts</summary>
    public float InactiveDuration { get; set; }

    /// <summary>Seconds into the current on/off phase</summary>
    public float PhaseTimer { get; set; }

    /// <summary>Optional status effect applied on contact</summary>
    public StatusEffectType? AppliedStatusEffect { get; set; }

    /// <summary>Duration of applied status effect in seconds</summary>
    public float StatusEffectDuration { get; set; }

    /// <summary>True once a BearTrap-type hazard has been sprung and is waiting to reset</summary>
    public bool IsTriggered { get; set; }

    /// <summary>Seconds until a triggered BearTrap resets</summary>
    public float ResetTimer { get; set; }

    public HazardComponent(HazardType type, float dps, float radius)
    {
        Type = type;
        DamagePerSecond = dps;
        TriggerRadius = radius;
        IsActive = true;
        TriggerCooldown = 0.5f;
        TimeSinceLastTrigger = 0f;
        ActiveDuration = 2f;
        InactiveDuration = 3f;
        PhaseTimer = 0f;
        StatusEffectDuration = 3f;

        // Set defaults based on type
        switch (type)
        {
            case HazardType.LavaPool:
            case HazardType.AcidPool:
                // Continuous — always active
                ActiveDuration = float.MaxValue;
                InactiveDuration = 0f;
                AppliedStatusEffect = type == HazardType.AcidPool ? StatusEffectType.Poison : StatusEffectType.Burning;
                break;

            case HazardType.GasVent:
                AppliedStatusEffect = StatusEffectType.Poison;
                ActiveDuration = 2f;
                InactiveDuration = 5f;
                IsActive = false;
                break;

            case HazardType.SpikeTrap:
                ActiveDuration = 1f;
                InactiveDuration = 2f;
                IsActive = false;
                break;

            case HazardType.BearTrap:
                AppliedStatusEffect = StatusEffectType.Stunned;
                StatusEffectDuration = 5f;
                ActiveDuration = float.MaxValue;
                InactiveDuration = 10f; // reset time after springing
                break;

            case HazardType.ElectricFence:
                AppliedStatusEffect = StatusEffectType.Stunned;
                TriggerCooldown = 1f;
                break;
        }
    }

    /// <summary>Returns a short player-facing description of this hazard</summary>
    public string GetDescription() => Type switch
    {
        HazardType.SpikeTrap    => "Spike trap! Watch your step.",
        HazardType.LavaPool     => "Scorching lava! Avoid at all costs.",
        HazardType.AcidPool     => "Corrosive acid! Damages you and your gear.",
        HazardType.GasVent      => "Poison gas vent! Hold your breath.",
        HazardType.BearTrap     => "Bear trap! It will hold you in place.",
        HazardType.ElectricFence => "Electric fence! Shocking.",
        _                       => "Hazard area."
    };
}
