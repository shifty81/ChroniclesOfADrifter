using System.Runtime.InteropServices;

namespace ChroniclesOfADrifter.Engine;

/// <summary>
/// P/Invoke wrapper for the native C++ Chronicles Engine
/// </summary>
public static class EngineInterop
{
    private const string DllName = "ChroniclesEngine.dll";
    
    // ===== Engine Initialization =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Engine_Initialize(
        int width, 
        int height, 
        [MarshalAs(UnmanagedType.LPStr)] string title);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Engine_Shutdown();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Engine_IsRunning();
    
    // ===== Game Loop =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Engine_BeginFrame();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Engine_EndFrame();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float Engine_GetDeltaTime();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float Engine_GetTotalTime();
    
    // ===== Rendering =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Renderer_LoadTexture(
        [MarshalAs(UnmanagedType.LPStr)] string filePath);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Renderer_UnloadTexture(int textureId);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Renderer_DrawSprite(
        int textureId, 
        float x, 
        float y, 
        float width, 
        float height, 
        float rotation);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Renderer_Clear(float r, float g, float b, float a);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Renderer_Present();
    
    // ===== Input =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Input_IsKeyPressed(int keyCode);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Input_IsKeyDown(int keyCode);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Input_IsKeyReleased(int keyCode);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Input_GetMousePosition(out float x, out float y);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Input_IsMouseButtonPressed(int button);
    
    // ===== Audio =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Audio_LoadSound(
        [MarshalAs(UnmanagedType.LPStr)] string filePath);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Audio_PlaySound(int soundId, float volume);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Audio_PlayMusic(
        [MarshalAs(UnmanagedType.LPStr)] string filePath, 
        float volume, 
        [MarshalAs(UnmanagedType.I1)] bool loop);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Audio_StopMusic();
    
    // ===== Physics =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Physics_SetGravity(float x, float y);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Physics_CheckCollision(
        float x1, float y1, float w1, float h1,
        float x2, float y2, float w2, float h2);
    
    // ===== Callbacks =====
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void InputCallbackDelegate(
        int keyCode, 
        [MarshalAs(UnmanagedType.I1)] bool isPressed);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CollisionCallbackDelegate(int entity1, int entity2);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Engine_RegisterInputCallback(InputCallbackDelegate callback);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Engine_RegisterCollisionCallback(CollisionCallbackDelegate callback);
    
    // ===== Error Handling =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Engine_GetLastError();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    public static extern string Engine_GetErrorMessage();
}
