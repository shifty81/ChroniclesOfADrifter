using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.UI;

/// <summary>
/// Base class for all UI elements
/// </summary>
public abstract class UIElement
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public UIElement? Parent { get; set; }
    public List<UIElement> Children { get; } = new();
    
    /// <summary>
    /// Get absolute X position in screen coordinates
    /// </summary>
    public float GetAbsoluteX()
    {
        return Parent != null ? Parent.GetAbsoluteX() + X : X;
    }
    
    /// <summary>
    /// Get absolute Y position in screen coordinates
    /// </summary>
    public float GetAbsoluteY()
    {
        return Parent != null ? Parent.GetAbsoluteY() + Y : Y;
    }
    
    /// <summary>
    /// Check if a point is inside this UI element
    /// </summary>
    public bool Contains(float x, float y)
    {
        float absX = GetAbsoluteX();
        float absY = GetAbsoluteY();
        return x >= absX && x <= absX + Width && y >= absY && y <= absY + Height;
    }
    
    /// <summary>
    /// Add a child UI element
    /// </summary>
    public void AddChild(UIElement child)
    {
        child.Parent = this;
        Children.Add(child);
    }
    
    /// <summary>
    /// Remove a child UI element
    /// </summary>
    public void RemoveChild(UIElement child)
    {
        child.Parent = null;
        Children.Remove(child);
    }
    
    /// <summary>
    /// Update this element and all children
    /// </summary>
    public virtual void Update(float deltaTime)
    {
        if (!IsVisible) return;
        
        foreach (var child in Children)
        {
            child.Update(deltaTime);
        }
    }
    
    /// <summary>
    /// Render this element and all children
    /// </summary>
    public virtual void Render()
    {
        if (!IsVisible) return;
        
        // Render self
        OnRender();
        
        // Render children
        foreach (var child in Children)
        {
            child.Render();
        }
    }
    
    /// <summary>
    /// Called when this element needs to be rendered
    /// </summary>
    protected abstract void OnRender();
    
    /// <summary>
    /// Called when mouse button is clicked on this element
    /// </summary>
    public virtual void OnClick(float mouseX, float mouseY)
    {
        // Override in derived classes
    }
    
    /// <summary>
    /// Called when mouse enters this element
    /// </summary>
    public virtual void OnMouseEnter()
    {
        // Override in derived classes
    }
    
    /// <summary>
    /// Called when mouse exits this element
    /// </summary>
    public virtual void OnMouseExit()
    {
        // Override in derived classes
    }
}
