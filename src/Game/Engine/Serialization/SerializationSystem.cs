using System.Runtime.InteropServices;
using System.Text;

namespace ChroniclesOfADrifter.Engine.Serialization;

/// <summary>
/// P/Invoke wrapper for C++ Serialization API
/// </summary>
public static class SerializationInterop
{
    private const string DllName = "ChroniclesEngine";
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Serialization_ToJson(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] StringBuilder buffer,
        int bufferSize);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Serialization_FromJson(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string json);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Serialization_SaveToFile(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string filePath);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Serialization_LoadFromFile(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string filePath);
}

/// <summary>
/// High-level managed wrapper for serialization system
/// </summary>
public class SerializationSystem
{
    /// <summary>
    /// Serialize an object to JSON string
    /// </summary>
    public static string? ToJson(string typeName, IntPtr instance)
    {
        if (instance == IntPtr.Zero) return null;
        
        var buffer = new StringBuilder(4096);
        int length = SerializationInterop.Serialization_ToJson(typeName, instance, buffer, buffer.Capacity);
        
        return length > 0 ? buffer.ToString() : null;
    }
    
    /// <summary>
    /// Deserialize an object from JSON string
    /// </summary>
    public static bool FromJson(string typeName, IntPtr instance, string json)
    {
        if (instance == IntPtr.Zero || string.IsNullOrEmpty(json)) return false;
        
        return SerializationInterop.Serialization_FromJson(typeName, instance, json);
    }
    
    /// <summary>
    /// Save object to JSON file
    /// </summary>
    public static bool SaveToFile(string typeName, IntPtr instance, string filePath)
    {
        if (instance == IntPtr.Zero || string.IsNullOrEmpty(filePath)) return false;
        
        return SerializationInterop.Serialization_SaveToFile(typeName, instance, filePath);
    }
    
    /// <summary>
    /// Load object from JSON file
    /// </summary>
    public static bool LoadFromFile(string typeName, IntPtr instance, string filePath)
    {
        if (instance == IntPtr.Zero || string.IsNullOrEmpty(filePath)) return false;
        
        return SerializationInterop.Serialization_LoadFromFile(typeName, instance, filePath);
    }
}
