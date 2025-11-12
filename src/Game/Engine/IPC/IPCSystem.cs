using System.Runtime.InteropServices;

namespace ChroniclesOfADrifter.Engine.IPC;

/// <summary>
/// IPC message types
/// </summary>
public enum MessageType
{
    // Query messages
    GetTypes = 0,
    GetTypeInfo = 1,
    GetSceneObjects = 2,
    GetObjectProperties = 3,
    
    // Command messages
    SetProperty = 4,
    CreateObject = 5,
    DeleteObject = 6,
    LoadScene = 7,
    SaveScene = 8,
    
    // Response messages
    Response = 9,
    Error = 10,
    
    // Events
    ObjectSelected = 11,
    ObjectModified = 12,
    SceneChanged = 13
}

/// <summary>
/// P/Invoke wrapper for IPC API
/// </summary>
public static class IPCInterop
{
    private const string DllName = "ChroniclesEngine";
    
    // Server API
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr IPC_CreateServer();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void IPC_DestroyServer(IntPtr server);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool IPC_ServerStart(IntPtr server,
        [MarshalAs(UnmanagedType.LPStr)] string pipeName);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void IPC_ServerStop(IntPtr server);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void IPC_ServerUpdate(IntPtr server);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void IPC_ServerSendEvent(IntPtr server, int eventType,
        [MarshalAs(UnmanagedType.LPStr)] string payload);
    
    // Client API
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr IPC_CreateClient();
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void IPC_DestroyClient(IntPtr client);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool IPC_ClientConnect(IntPtr client,
        [MarshalAs(UnmanagedType.LPStr)] string pipeName);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern void IPC_ClientDisconnect(IntPtr client);
    
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool IPC_ClientSendCommand(IntPtr client, int commandType,
        [MarshalAs(UnmanagedType.LPStr)] string payload,
        [MarshalAs(UnmanagedType.LPStr)] System.Text.StringBuilder response,
        int responseSize);
}

/// <summary>
/// High-level IPC server wrapper
/// </summary>
public class IPCServer : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    
    public IPCServer()
    {
        _handle = IPCInterop.IPC_CreateServer();
    }
    
    public bool Start(string pipeName = "ChroniclesEngine")
    {
        return IPCInterop.IPC_ServerStart(_handle, pipeName);
    }
    
    public void Stop()
    {
        IPCInterop.IPC_ServerStop(_handle);
    }
    
    public void Update()
    {
        IPCInterop.IPC_ServerUpdate(_handle);
    }
    
    public void SendEvent(MessageType type, string payload)
    {
        IPCInterop.IPC_ServerSendEvent(_handle, (int)type, payload);
    }
    
    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            IPCInterop.IPC_DestroyServer(_handle);
            _handle = IntPtr.Zero;
            _disposed = true;
        }
    }
}

/// <summary>
/// High-level IPC client wrapper
/// </summary>
public class IPCClient : IDisposable
{
    private IntPtr _handle;
    private bool _disposed;
    
    public IPCClient()
    {
        _handle = IPCInterop.IPC_CreateClient();
    }
    
    public bool Connect(string pipeName = "ChroniclesEngine")
    {
        return IPCInterop.IPC_ClientConnect(_handle, pipeName);
    }
    
    public void Disconnect()
    {
        IPCInterop.IPC_ClientDisconnect(_handle);
    }
    
    public string? SendCommand(MessageType type, string payload)
    {
        var response = new System.Text.StringBuilder(4096);
        bool success = IPCInterop.IPC_ClientSendCommand(_handle, (int)type, payload, response, response.Capacity);
        
        return success ? response.ToString() : null;
    }
    
    public void Dispose()
    {
        if (!_disposed && _handle != IntPtr.Zero)
        {
            IPCInterop.IPC_DestroyClient(_handle);
            _handle = IntPtr.Zero;
            _disposed = true;
        }
    }
}
