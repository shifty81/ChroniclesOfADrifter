using ChroniclesOfADrifter.Engine;

namespace ChroniclesOfADrifter.UI;

/// <summary>
/// A simple rectangular panel UI element
/// </summary>
public class UIPanel : UIElement
{
    public float BackgroundR { get; set; } = 0.2f;
    public float BackgroundG { get; set; } = 0.2f;
    public float BackgroundB { get; set; } = 0.2f;
    public float BackgroundA { get; set; } = 0.9f;
    
    public float BorderR { get; set; } = 0.4f;
    public float BorderG { get; set; } = 0.4f;
    public float BorderB { get; set; } = 0.4f;
    public float BorderA { get; set; } = 1.0f;
    
    public float BorderThickness { get; set; } = 2.0f;
    
    protected override void OnRender()
    {
        float absX = GetAbsoluteX();
        float absY = GetAbsoluteY();
        
        // Draw background
        EngineInterop.Renderer_DrawRect(absX, absY, Width, Height, 
                                       BackgroundR, BackgroundG, BackgroundB, BackgroundA);
        
        // Draw border (top, right, bottom, left)
        if (BorderThickness > 0)
        {
            // Top border
            EngineInterop.Renderer_DrawRect(absX, absY, Width, BorderThickness,
                                           BorderR, BorderG, BorderB, BorderA);
            // Right border
            EngineInterop.Renderer_DrawRect(absX + Width - BorderThickness, absY, 
                                           BorderThickness, Height,
                                           BorderR, BorderG, BorderB, BorderA);
            // Bottom border
            EngineInterop.Renderer_DrawRect(absX, absY + Height - BorderThickness, 
                                           Width, BorderThickness,
                                           BorderR, BorderG, BorderB, BorderA);
            // Left border
            EngineInterop.Renderer_DrawRect(absX, absY, BorderThickness, Height,
                                           BorderR, BorderG, BorderB, BorderA);
        }
    }
}
