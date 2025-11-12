#include "SerializationAPI.h"
#include "Serialization.h"
#include <cstring>
#include <fstream>
#include <sstream>

using namespace Chronicles::Serialization;

extern "C" ENGINE_API int Serialization_ToJson(const char* typeName, void* instance, 
                                               char* buffer, int bufferSize) {
    if (!typeName || !instance || !buffer || bufferSize <= 0) {
        return -1;
    }
    
    try {
        std::string json = SerializeObject(typeName, instance);
        
        if (static_cast<int>(json.length()) >= bufferSize) {
            // Buffer too small
            return -1;
        }
        
        std::strncpy(buffer, json.c_str(), bufferSize - 1);
        buffer[bufferSize - 1] = '\0';
        
        return static_cast<int>(json.length());
    }
    catch (...) {
        return -1;
    }
}

extern "C" ENGINE_API bool Serialization_FromJson(const char* typeName, void* instance, 
                                                  const char* json) {
    if (!typeName || !instance || !json) {
        return false;
    }
    
    // Note: Full JSON parsing not implemented in this minimal version.
    // For production, integrate nlohmann/json or similar library.
    // This would parse the JSON and use reflection to set field values.
    
    return false; // Not implemented yet
}

extern "C" ENGINE_API bool Serialization_SaveToFile(const char* typeName, void* instance, 
                                                    const char* filePath) {
    if (!typeName || !instance || !filePath) {
        return false;
    }
    
    try {
        std::string json = SerializeObject(typeName, instance);
        
        std::ofstream file(filePath);
        if (!file.is_open()) {
            return false;
        }
        
        file << json;
        file.close();
        
        return true;
    }
    catch (...) {
        return false;
    }
}

extern "C" ENGINE_API bool Serialization_LoadFromFile(const char* typeName, void* instance,
                                                      const char* filePath) {
    if (!typeName || !instance || !filePath) {
        return false;
    }
    
    try {
        std::ifstream file(filePath);
        if (!file.is_open()) {
            return false;
        }
        
        std::stringstream buffer;
        buffer << file.rdbuf();
        file.close();
        
        std::string json = buffer.str();
        
        // Note: Would call Serialization_FromJson here
        return false; // Not fully implemented yet
    }
    catch (...) {
        return false;
    }
}
