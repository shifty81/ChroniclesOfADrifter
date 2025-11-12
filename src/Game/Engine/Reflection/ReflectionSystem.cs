using System.Runtime.InteropServices;
using System.Text;

namespace ChroniclesOfADrifter.Engine.Reflection;

/// <summary>
/// Property type enumeration matching C++ PropertyType
/// </summary>
public enum PropertyType
{
    Bool = 0,
    Int = 1,
    Float = 2,
    Double = 3,
    String = 4,
    Vector2 = 5,
    Vector3 = 6,
    Color = 7,
    Custom = 8
}

/// <summary>
/// P/Invoke wrapper for C++ Reflection API
/// </summary>
public static class ReflectionInterop
{
    private const string DllName = "ChroniclesEngine";
    
    // ===== Type Query Functions =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Reflection_GetTypeCount();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Reflection_GetTypeName(int index, 
        [MarshalAs(UnmanagedType.LPStr)] StringBuilder buffer, int bufferSize);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Reflection_GetTypeSize(
        [MarshalAs(UnmanagedType.LPStr)] string typeName);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Reflection_GetFieldCount(
        [MarshalAs(UnmanagedType.LPStr)] string typeName);
    
    // ===== Field Query Functions =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Reflection_GetFieldName(
        [MarshalAs(UnmanagedType.LPStr)] string typeName, 
        int fieldIndex,
        [MarshalAs(UnmanagedType.LPStr)] StringBuilder buffer, 
        int bufferSize);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Reflection_GetFieldType(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Reflection_GetFieldOffset(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName);
    
    // ===== Value Access Functions =====
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern float Reflection_GetFloatValue(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName,
        IntPtr instance);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Reflection_SetFloatValue(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName,
        IntPtr instance,
        float value);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int Reflection_GetIntValue(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName,
        IntPtr instance);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Reflection_SetIntValue(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName,
        IntPtr instance,
        int value);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Reflection_GetBoolValue(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName,
        IntPtr instance);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Reflection_SetBoolValue(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName,
        IntPtr instance,
        [MarshalAs(UnmanagedType.I1)] bool value);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Reflection_GetStringValue(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName,
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] StringBuilder buffer,
        int bufferSize);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Reflection_SetStringValue(
        [MarshalAs(UnmanagedType.LPStr)] string typeName,
        [MarshalAs(UnmanagedType.LPStr)] string fieldName,
        IntPtr instance,
        [MarshalAs(UnmanagedType.LPStr)] string value);
}

/// <summary>
/// High-level managed wrapper for reflection system
/// </summary>
public class ReflectionSystem
{
    /// <summary>
    /// Get all registered type names
    /// </summary>
    public static List<string> GetAllTypes()
    {
        var types = new List<string>();
        int count = ReflectionInterop.Reflection_GetTypeCount();
        
        for (int i = 0; i < count; i++)
        {
            var buffer = new StringBuilder(256);
            ReflectionInterop.Reflection_GetTypeName(i, buffer, buffer.Capacity);
            types.Add(buffer.ToString());
        }
        
        return types;
    }
    
    /// <summary>
    /// Get type information
    /// </summary>
    public static TypeInfo? GetTypeInfo(string typeName)
    {
        int size = ReflectionInterop.Reflection_GetTypeSize(typeName);
        if (size <= 0) return null;
        
        int fieldCount = ReflectionInterop.Reflection_GetFieldCount(typeName);
        var fields = new List<FieldInfo>();
        
        for (int i = 0; i < fieldCount; i++)
        {
            var nameBuffer = new StringBuilder(256);
            ReflectionInterop.Reflection_GetFieldName(typeName, i, nameBuffer, nameBuffer.Capacity);
            string fieldName = nameBuffer.ToString();
            
            int typeValue = ReflectionInterop.Reflection_GetFieldType(typeName, fieldName);
            int offset = ReflectionInterop.Reflection_GetFieldOffset(typeName, fieldName);
            
            fields.Add(new FieldInfo
            {
                Name = fieldName,
                Type = (PropertyType)typeValue,
                Offset = offset
            });
        }
        
        return new TypeInfo
        {
            Name = typeName,
            Size = size,
            Fields = fields
        };
    }
    
    /// <summary>
    /// Get field value from instance
    /// </summary>
    public static object? GetValue(string typeName, string fieldName, IntPtr instance)
    {
        int typeValue = ReflectionInterop.Reflection_GetFieldType(typeName, fieldName);
        if (typeValue < 0) return null;
        
        var type = (PropertyType)typeValue;
        return type switch
        {
            PropertyType.Bool => ReflectionInterop.Reflection_GetBoolValue(typeName, fieldName, instance),
            PropertyType.Int => ReflectionInterop.Reflection_GetIntValue(typeName, fieldName, instance),
            PropertyType.Float => ReflectionInterop.Reflection_GetFloatValue(typeName, fieldName, instance),
            PropertyType.String => GetStringValue(typeName, fieldName, instance),
            _ => null
        };
    }
    
    /// <summary>
    /// Set field value on instance
    /// </summary>
    public static void SetValue(string typeName, string fieldName, IntPtr instance, object value)
    {
        int typeValue = ReflectionInterop.Reflection_GetFieldType(typeName, fieldName);
        if (typeValue < 0) return;
        
        var type = (PropertyType)typeValue;
        switch (type)
        {
            case PropertyType.Bool when value is bool boolVal:
                ReflectionInterop.Reflection_SetBoolValue(typeName, fieldName, instance, boolVal);
                break;
            case PropertyType.Int when value is int intVal:
                ReflectionInterop.Reflection_SetIntValue(typeName, fieldName, instance, intVal);
                break;
            case PropertyType.Float when value is float floatVal:
                ReflectionInterop.Reflection_SetFloatValue(typeName, fieldName, instance, floatVal);
                break;
            case PropertyType.String when value is string strVal:
                ReflectionInterop.Reflection_SetStringValue(typeName, fieldName, instance, strVal);
                break;
        }
    }
    
    private static string GetStringValue(string typeName, string fieldName, IntPtr instance)
    {
        var buffer = new StringBuilder(1024);
        ReflectionInterop.Reflection_GetStringValue(typeName, fieldName, instance, buffer, buffer.Capacity);
        return buffer.ToString();
    }
}

/// <summary>
/// Type information
/// </summary>
public class TypeInfo
{
    public string Name { get; set; } = "";
    public int Size { get; set; }
    public List<FieldInfo> Fields { get; set; } = new();
}

/// <summary>
/// Field information
/// </summary>
public class FieldInfo
{
    public string Name { get; set; } = "";
    public PropertyType Type { get; set; }
    public int Offset { get; set; }
}
