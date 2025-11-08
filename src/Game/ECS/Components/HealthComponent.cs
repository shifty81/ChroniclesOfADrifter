namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Health component - represents entity health
/// </summary>
public class HealthComponent : IComponent
{
    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }
    
    public bool IsAlive => CurrentHealth > 0;
    public float HealthPercentage => CurrentHealth / MaxHealth;
    
    public HealthComponent(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }
    
    public void Damage(float amount)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - amount);
    }
    
    public void Heal(float amount)
    {
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
    }
}
