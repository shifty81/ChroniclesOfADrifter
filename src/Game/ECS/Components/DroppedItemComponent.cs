namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component for items dropped on the ground that can be picked up
/// </summary>
public class DroppedItemComponent : IComponent
{
    public TileType ItemType { get; set; }
    public int Quantity { get; set; }
    public float ExpirationTimer { get; set; }
    public int GoldAmount { get; set; }
    
    public const float DEFAULT_EXPIRATION_TIME = 60f; // 60 seconds
    
    public DroppedItemComponent(TileType itemType, int quantity)
    {
        ItemType = itemType;
        Quantity = quantity;
        GoldAmount = 0;
        ExpirationTimer = DEFAULT_EXPIRATION_TIME;
    }
    
    public DroppedItemComponent(int goldAmount)
    {
        ItemType = TileType.Air;
        Quantity = 0;
        GoldAmount = goldAmount;
        ExpirationTimer = DEFAULT_EXPIRATION_TIME;
    }
    
    /// <summary>
    /// Checks if this is a gold drop
    /// </summary>
    public bool IsGold => GoldAmount > 0;
    
    /// <summary>
    /// Checks if the item has expired
    /// </summary>
    public bool HasExpired => ExpirationTimer <= 0f;
}
