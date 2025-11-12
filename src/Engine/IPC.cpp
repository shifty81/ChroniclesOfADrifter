#include "IPC.h"
#include "Reflection.h"
#include "Serialization.h"
#include <cstring>
#include <sstream>

using namespace Chronicles::IPC;
using namespace Chronicles::Reflection;
using namespace Chronicles::Serialization;

// ===== IPCServer Implementation =====

IPCServer::IPCServer() 
    : m_running(false), m_platformData(nullptr) {
    InitializeDefaultHandlers();
}

IPCServer::~IPCServer() {
    Stop();
}

bool IPCServer::Start(const std::string& pipeName) {
    if (m_running) return true;
    
    m_pipeName = pipeName;
    
    // Note: Platform-specific implementation would go here
    // For Windows: Create named pipe with CreateNamedPipe
    // For Unix: Create Unix domain socket
    // For now, this is a stub
    
    m_running = false; // Set to true when actually implemented
    return m_running;
}

void IPCServer::Stop() {
    if (!m_running) return;
    
    // Close platform-specific handles
    m_running = false;
}

void IPCServer::Update() {
    if (!m_running) return;
    
    // Poll for incoming messages
    // Process each message with registered handlers
    // Send responses
}

void IPCServer::RegisterHandler(MessageType type, MessageHandler handler) {
    m_handlers[type] = handler;
}

void IPCServer::SendEvent(MessageType type, const std::string& payload) {
    if (!m_running) return;
    
    // Send event to all connected clients
    // Platform-specific implementation
}

void IPCServer::InitializeDefaultHandlers() {
    // Register default handlers for common operations
    
    // GetTypes handler
    RegisterHandler(MessageType::GetTypes, [](const std::string&) -> std::string {
        auto types = ReflectionRegistry::Instance().GetAllTypeNames();
        
        std::stringstream ss;
        ss << "[";
        for (size_t i = 0; i < types.size(); i++) {
            if (i > 0) ss << ",";
            ss << "\"" << types[i] << "\"";
        }
        ss << "]";
        
        return ss.str();
    });
    
    // GetTypeInfo handler
    RegisterHandler(MessageType::GetTypeInfo, [](const std::string& payload) -> std::string {
        // payload should be: {"typeName":"Transform"}
        // Response: {"name":"Transform","size":16,"fields":[...]}
        
        // Simple parsing - in production use a JSON library
        size_t start = payload.find("\":\"") + 3;
        size_t end = payload.find("\"", start);
        if (start == std::string::npos || end == std::string::npos) {
            return "{\"error\":\"Invalid request\"}";
        }
        
        std::string typeName = payload.substr(start, end - start);
        auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
        
        if (!typeInfo) {
            return "{\"error\":\"Type not found\"}";
        }
        
        std::stringstream ss;
        ss << "{";
        ss << "\"name\":\"" << typeInfo->GetName() << "\",";
        ss << "\"size\":" << typeInfo->GetSize() << ",";
        ss << "\"fields\":[";
        
        const auto& fields = typeInfo->GetFields();
        for (size_t i = 0; i < fields.size(); i++) {
            if (i > 0) ss << ",";
            ss << "{\"name\":\"" << fields[i].GetName() << "\",";
            ss << "\"type\":" << static_cast<int>(fields[i].GetType()) << "}";
        }
        
        ss << "]}";
        return ss.str();
    });
}

std::string IPCServer::HandleMessage(const Message& msg) {
    auto it = m_handlers.find(msg.type);
    if (it == m_handlers.end()) {
        return "{\"error\":\"Unknown message type\"}";
    }
    
    return it->second(msg.payload);
}

// ===== IPCClient Implementation =====

IPCClient::IPCClient()
    : m_connected(false), m_nextRequestId(1), m_platformData(nullptr) {
}

IPCClient::~IPCClient() {
    Disconnect();
}

bool IPCClient::Connect(const std::string& pipeName) {
    if (m_connected) return true;
    
    m_pipeName = pipeName;
    
    // Platform-specific implementation
    // For Windows: Connect to named pipe with CreateFile
    // For Unix: Connect to Unix domain socket
    
    m_connected = false; // Set to true when actually implemented
    return m_connected;
}

void IPCClient::Disconnect() {
    if (!m_connected) return;
    
    // Close platform-specific handles
    m_connected = false;
}

std::string IPCClient::SendCommand(MessageType type, const std::string& payload) {
    if (!m_connected) {
        return "{\"error\":\"Not connected\"}";
    }
    
    // Create message with unique request ID
    Message msg;
    msg.type = type;
    msg.payload = payload;
    msg.requestId = m_nextRequestId++;
    
    // Send message and wait for response
    // Platform-specific implementation
    
    return "{\"error\":\"Not implemented\"}";
}

std::vector<Message> IPCClient::PollEvents() {
    std::vector<Message> events;
    
    if (!m_connected) return events;
    
    // Poll for events from server
    // Platform-specific implementation
    
    return events;
}

// ===== C API Implementation =====

extern "C" ENGINE_API void* IPC_CreateServer() {
    return new IPCServer();
}

extern "C" ENGINE_API void IPC_DestroyServer(void* server) {
    delete static_cast<IPCServer*>(server);
}

extern "C" ENGINE_API bool IPC_ServerStart(void* server, const char* pipeName) {
    if (!server) return false;
    return static_cast<IPCServer*>(server)->Start(pipeName ? pipeName : "ChroniclesEngine");
}

extern "C" ENGINE_API void IPC_ServerStop(void* server) {
    if (server) {
        static_cast<IPCServer*>(server)->Stop();
    }
}

extern "C" ENGINE_API void IPC_ServerUpdate(void* server) {
    if (server) {
        static_cast<IPCServer*>(server)->Update();
    }
}

extern "C" ENGINE_API void IPC_ServerSendEvent(void* server, int eventType, const char* payload) {
    if (server && payload) {
        static_cast<IPCServer*>(server)->SendEvent(static_cast<MessageType>(eventType), payload);
    }
}

extern "C" ENGINE_API void* IPC_CreateClient() {
    return new IPCClient();
}

extern "C" ENGINE_API void IPC_DestroyClient(void* client) {
    delete static_cast<IPCClient*>(client);
}

extern "C" ENGINE_API bool IPC_ClientConnect(void* client, const char* pipeName) {
    if (!client) return false;
    return static_cast<IPCClient*>(client)->Connect(pipeName ? pipeName : "ChroniclesEngine");
}

extern "C" ENGINE_API void IPC_ClientDisconnect(void* client) {
    if (client) {
        static_cast<IPCClient*>(client)->Disconnect();
    }
}

extern "C" ENGINE_API bool IPC_ClientSendCommand(void* client, int commandType,
                                                 const char* payload, char* response, int responseSize) {
    if (!client || !payload || !response || responseSize <= 0) {
        return false;
    }
    
    auto result = static_cast<IPCClient*>(client)->SendCommand(
        static_cast<MessageType>(commandType), payload);
    
    std::strncpy(response, result.c_str(), responseSize - 1);
    response[responseSize - 1] = '\0';
    
    return !result.empty();
}
