namespace ChroniclesOfADrifter.ECS.Components;

/// <summary>
/// Component that holds UI elements for an entity
/// </summary>
public class UIComponent : IComponent
{
    public List<ChroniclesOfADrifter.UI.UIElement> Elements { get; } = new();
    public bool IsVisible { get; set; } = true;
    
    /// <summary>
    /// Add a UI element
    /// </summary>
    public void AddElement(ChroniclesOfADrifter.UI.UIElement element)
    {
        Elements.Add(element);
    }
    
    /// <summary>
    /// Remove a UI element
    /// </summary>
    public void RemoveElement(ChroniclesOfADrifter.UI.UIElement element)
    {
        Elements.Remove(element);
    }
    
    /// <summary>
    /// Clear all UI elements
    /// </summary>
    public void Clear()
    {
        Elements.Clear();
    }
}
