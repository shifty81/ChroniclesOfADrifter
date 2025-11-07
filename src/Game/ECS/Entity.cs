namespace ChroniclesOfADrifter.ECS;

/// <summary>
/// Represents a game entity (just an ID)
/// </summary>
public readonly struct Entity : IEquatable<Entity>
{
    public readonly int Id;
    
    public Entity(int id)
    {
        Id = id;
    }
    
    public bool Equals(Entity other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is Entity entity && Equals(entity);
    public override int GetHashCode() => Id;
    public override string ToString() => $"Entity({Id})";
    
    public static bool operator ==(Entity left, Entity right) => left.Equals(right);
    public static bool operator !=(Entity left, Entity right) => !left.Equals(right);
}
