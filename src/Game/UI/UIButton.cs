using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.UI;

/// <summary>
/// A clickable button UI element
/// </summary>
public class UIButton : UIElement
{
    public float NormalR { get; set; } = 0.3f;
    public float NormalG { get; set; } = 0.3f;
    public float NormalB { get; set; } = 0.3f;
    public float NormalA { get; set; } = 1.0f;
    
    public float HoverR { get; set; } = 0.4f;
    public float HoverG { get; set; } = 0.4f;
    public float HoverB { get; set; } = 0.4f;
    public float HoverA { get; set; } = 1.0f;
    
    public float PressedR { get; set; } = 0.2f;
    public float PressedG { get; set; } = 0.2f;
    public float PressedB { get; set; } = 0.2f;
    public float PressedA { get; set; } = 1.0f;
    
    public float DisabledR { get; set; } = 0.15f;
    public float DisabledG { get; set; } = 0.15f;
    public float DisabledB { get; set; } = 0.15f;
    public float DisabledA { get; set; } = 0.5f;
    
    public string Text { get; set; } = "";
    public Action? OnClickAction { get; set; }
    
    private bool _isHovered = false;
    private bool _isPressed = false;
    
    public override void OnMouseEnter()
    {
        _isHovered = true;
    }
    
    public override void OnMouseExit()
    {
        _isHovered = false;
        _isPressed = false;
    }
    
    public override void OnClick(float mouseX, float mouseY)
    {
        if (IsEnabled)
        {
            _isPressed = true;
            OnClickAction?.Invoke();
        }
    }
    
    protected override void OnRender()
    {
        float absX = GetAbsoluteX();
        float absY = GetAbsoluteY();
        
        // Determine color based on state
        float r, g, b, a;
        if (!IsEnabled)
        {
            r = DisabledR; g = DisabledG; b = DisabledB; a = DisabledA;
        }
        else if (_isPressed)
        {
            r = PressedR; g = PressedG; b = PressedB; a = PressedA;
        }
        else if (_isHovered)
        {
            r = HoverR; g = HoverG; b = HoverB; a = HoverA;
        }
        else
        {
            r = NormalR; g = NormalG; b = NormalB; a = NormalA;
        }
        
        // Draw button background
        EngineInterop.Renderer_DrawRect(absX, absY, Width, Height, r, g, b, a);
        
        // Draw border
        float borderThickness = 2.0f;
        EngineInterop.Renderer_DrawRect(absX, absY, Width, borderThickness, 0.5f, 0.5f, 0.5f, 1.0f);
        EngineInterop.Renderer_DrawRect(absX + Width - borderThickness, absY, borderThickness, Height, 0.5f, 0.5f, 0.5f, 1.0f);
        EngineInterop.Renderer_DrawRect(absX, absY + Height - borderThickness, Width, borderThickness, 0.5f, 0.5f, 0.5f, 1.0f);
        EngineInterop.Renderer_DrawRect(absX, absY, borderThickness, Height, 0.5f, 0.5f, 0.5f, 1.0f);
        
        // TODO: Draw text when text rendering is available
        // For now, the button is just a colored rectangle
    }
    
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        
        // Reset pressed state after a frame
        if (_isPressed)
        {
            _isPressed = false;
        }
    }
}
