namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component representing an entity's currency
/// </summary>
public class CurrencyComponent : IComponent
{
    public int Gold { get; set; }
    
    public CurrencyComponent(int initialGold = 0)
    {
        Gold = initialGold;
    }
    
    /// <summary>
    /// Add gold
    /// </summary>
    public void AddGold(int amount)
    {
        Gold += amount;
        Gold = Math.Max(0, Gold);
    }
    
    /// <summary>
    /// Remove gold (returns true if had enough)
    /// </summary>
    public bool RemoveGold(int amount)
    {
        if (Gold >= amount)
        {
            Gold -= amount;
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Check if has enough gold
    /// </summary>
    public bool HasGold(int amount)
    {
        return Gold >= amount;
    }
}
