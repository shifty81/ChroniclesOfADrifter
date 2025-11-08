namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component for entities that can attack
/// </summary>
public class CombatComponent : IComponent
{
    public float AttackDamage { get; set; }
    public float AttackRange { get; set; }
    public float AttackCooldown { get; set; }
    public float TimeSinceLastAttack { get; set; }
    
    public CombatComponent(float damage = 10f, float range = 50f, float cooldown = 0.5f)
    {
        AttackDamage = damage;
        AttackRange = range;
        AttackCooldown = cooldown;
        TimeSinceLastAttack = cooldown; // Ready to attack immediately
    }
}
