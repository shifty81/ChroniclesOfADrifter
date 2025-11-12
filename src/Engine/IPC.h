#pragma once

#include <string>
#include <functional>
#include <map>
#include <vector>

#ifdef _WIN32
    #ifdef ENGINE_EXPORTS
        #define ENGINE_API __declspec(dllexport)
    #else
        #define ENGINE_API __declspec(dllimport)
    #endif
#else
    #define ENGINE_API
#endif

// Chronicles of a Drifter - IPC System
// Inter-Process Communication for Editor-Engine communication

namespace Chronicles {
namespace IPC {

/// <summary>
/// IPC message types
/// </summary>
enum class MessageType {
    // Query messages
    GetTypes,
    GetTypeInfo,
    GetSceneObjects,
    GetObjectProperties,
    
    // Command messages
    SetProperty,
    CreateObject,
    DeleteObject,
    LoadScene,
    SaveScene,
    
    // Response messages
    Response,
    Error,
    
    // Events
    ObjectSelected,
    ObjectModified,
    SceneChanged
};

/// <summary>
/// IPC message structure
/// </summary>
struct Message {
    MessageType type;
    std::string payload;  // JSON-encoded data
    int requestId;        // For matching requests/responses
};

/// <summary>
/// Message handler callback
/// </summary>
using MessageHandler = std::function<std::string(const std::string& payload)>;

/// <summary>
/// IPC server for engine side
/// Listens for editor commands and sends events
/// </summary>
class ENGINE_API IPCServer {
public:
    IPCServer();
    ~IPCServer();
    
    /// <summary>
    /// Start the IPC server
    /// </summary>
    bool Start(const std::string& pipeName = "ChroniclesEngine");
    
    /// <summary>
    /// Stop the IPC server
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Check if server is running
    /// </summary>
    bool IsRunning() const { return m_running; }
    
    /// <summary>
    /// Process incoming messages (call from main loop)
    /// </summary>
    void Update();
    
    /// <summary>
    /// Register a message handler
    /// </summary>
    void RegisterHandler(MessageType type, MessageHandler handler);
    
    /// <summary>
    /// Send an event to connected clients
    /// </summary>
    void SendEvent(MessageType type, const std::string& payload);
    
private:
    bool m_running;
    std::string m_pipeName;
    std::map<MessageType, MessageHandler> m_handlers;
    
    // Platform-specific implementation
    void* m_platformData;
    
    void InitializeDefaultHandlers();
    std::string HandleMessage(const Message& msg);
};

/// <summary>
/// IPC client for editor side
/// Sends commands to engine and receives events
/// </summary>
class ENGINE_API IPCClient {
public:
    IPCClient();
    ~IPCClient();
    
    /// <summary>
    /// Connect to the engine
    /// </summary>
    bool Connect(const std::string& pipeName = "ChroniclesEngine");
    
    /// <summary>
    /// Disconnect from the engine
    /// </summary>
    void Disconnect();
    
    /// <summary>
    /// Check if connected
    /// </summary>
    bool IsConnected() const { return m_connected; }
    
    /// <summary>
    /// Send a command and wait for response
    /// </summary>
    std::string SendCommand(MessageType type, const std::string& payload);
    
    /// <summary>
    /// Poll for events from engine
    /// </summary>
    std::vector<Message> PollEvents();
    
private:
    bool m_connected;
    std::string m_pipeName;
    int m_nextRequestId;
    
    // Platform-specific implementation
    void* m_platformData;
};

} // namespace IPC
} // namespace Chronicles

// C API for cross-language access
extern "C" {
    // Server API
    ENGINE_API void* IPC_CreateServer();
    ENGINE_API void IPC_DestroyServer(void* server);
    ENGINE_API bool IPC_ServerStart(void* server, const char* pipeName);
    ENGINE_API void IPC_ServerStop(void* server);
    ENGINE_API void IPC_ServerUpdate(void* server);
    ENGINE_API void IPC_ServerSendEvent(void* server, int eventType, const char* payload);
    
    // Client API
    ENGINE_API void* IPC_CreateClient();
    ENGINE_API void IPC_DestroyClient(void* client);
    ENGINE_API bool IPC_ClientConnect(void* client, const char* pipeName);
    ENGINE_API void IPC_ClientDisconnect(void* client);
    ENGINE_API bool IPC_ClientSendCommand(void* client, int commandType, 
                                          const char* payload, char* response, int responseSize);
}
