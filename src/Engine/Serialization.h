#pragma once

#include "Reflection.h"
#include <string>
#include <sstream>
#include <iomanip>

// Chronicles of a Drifter - Simple JSON Serialization
// Minimal JSON serializer/deserializer for reflection system
// Note: For production, consider using nlohmann/json library

namespace Chronicles {
namespace Serialization {

/// <summary>
/// Simple JSON value types
/// </summary>
enum class JsonType {
    Null,
    Bool,
    Number,
    String,
    Object,
    Array
};

/// <summary>
/// Simple JSON writer
/// </summary>
class JsonWriter {
public:
    JsonWriter() : m_indent(0) {}
    
    void BeginObject() {
        m_ss << "{\n";
        m_indent++;
    }
    
    void EndObject() {
        m_indent--;
        WriteIndent();
        m_ss << "}";
    }
    
    void BeginArray(const std::string& key) {
        WriteIndent();
        m_ss << "\"" << key << "\": [\n";
        m_indent++;
    }
    
    void EndArray() {
        m_indent--;
        m_ss << "\n";
        WriteIndent();
        m_ss << "]";
    }
    
    void WriteField(const std::string& key, bool value, bool comma = true) {
        WriteIndent();
        m_ss << "\"" << key << "\": " << (value ? "true" : "false");
        if (comma) m_ss << ",";
        m_ss << "\n";
    }
    
    void WriteField(const std::string& key, int value, bool comma = true) {
        WriteIndent();
        m_ss << "\"" << key << "\": " << value;
        if (comma) m_ss << ",";
        m_ss << "\n";
    }
    
    void WriteField(const std::string& key, float value, bool comma = true) {
        WriteIndent();
        m_ss << "\"" << key << "\": " << std::fixed << std::setprecision(6) << value;
        if (comma) m_ss << ",";
        m_ss << "\n";
    }
    
    void WriteField(const std::string& key, double value, bool comma = true) {
        WriteIndent();
        m_ss << "\"" << key << "\": " << std::fixed << std::setprecision(6) << value;
        if (comma) m_ss << ",";
        m_ss << "\n";
    }
    
    void WriteField(const std::string& key, const std::string& value, bool comma = true) {
        WriteIndent();
        m_ss << "\"" << key << "\": \"" << EscapeString(value) << "\"";
        if (comma) m_ss << ",";
        m_ss << "\n";
    }
    
    std::string GetJson() const {
        return m_ss.str();
    }
    
private:
    void WriteIndent() {
        for (int i = 0; i < m_indent; i++) {
            m_ss << "  ";
        }
    }
    
    std::string EscapeString(const std::string& str) {
        std::string result;
        for (char c : str) {
            switch (c) {
                case '"': result += "\\\""; break;
                case '\\': result += "\\\\"; break;
                case '\n': result += "\\n"; break;
                case '\r': result += "\\r"; break;
                case '\t': result += "\\t"; break;
                default: result += c;
            }
        }
        return result;
    }
    
    std::stringstream m_ss;
    int m_indent;
};

/// <summary>
/// Serialize an object to JSON using reflection
/// </summary>
inline std::string SerializeObject(const std::string& typeName, void* instance) {
    using namespace Reflection;
    
    auto typeInfo = ReflectionRegistry::Instance().GetType(typeName);
    if (!typeInfo || !instance) {
        return "{}";
    }
    
    JsonWriter writer;
    writer.BeginObject();
    
    const auto& fields = typeInfo->GetFields();
    for (size_t i = 0; i < fields.size(); i++) {
        const auto& field = fields[i];
        bool isLast = (i == fields.size() - 1);
        
        switch (field.GetType()) {
            case PropertyType::Bool:
                writer.WriteField(field.GetName(), field.GetValue<bool>(instance), !isLast);
                break;
            case PropertyType::Int:
                writer.WriteField(field.GetName(), field.GetValue<int>(instance), !isLast);
                break;
            case PropertyType::Float:
                writer.WriteField(field.GetName(), field.GetValue<float>(instance), !isLast);
                break;
            case PropertyType::Double:
                writer.WriteField(field.GetName(), field.GetValue<double>(instance), !isLast);
                break;
            case PropertyType::String:
                writer.WriteField(field.GetName(), field.GetValue<std::string>(instance), !isLast);
                break;
            default:
                // Skip unsupported types
                break;
        }
    }
    
    writer.EndObject();
    return writer.GetJson();
}

/// <summary>
/// Simple JSON value for deserialization
/// </summary>
class JsonValue {
public:
    JsonValue() : m_type(JsonType::Null) {}
    
    JsonType GetType() const { return m_type; }
    
    bool AsBool() const { return m_boolValue; }
    int AsInt() const { return m_intValue; }
    float AsFloat() const { return m_floatValue; }
    const std::string& AsString() const { return m_stringValue; }
    
    static JsonValue FromBool(bool value) {
        JsonValue v;
        v.m_type = JsonType::Bool;
        v.m_boolValue = value;
        return v;
    }
    
    static JsonValue FromInt(int value) {
        JsonValue v;
        v.m_type = JsonType::Number;
        v.m_intValue = value;
        v.m_floatValue = static_cast<float>(value);
        return v;
    }
    
    static JsonValue FromFloat(float value) {
        JsonValue v;
        v.m_type = JsonType::Number;
        v.m_floatValue = value;
        v.m_intValue = static_cast<int>(value);
        return v;
    }
    
    static JsonValue FromString(const std::string& value) {
        JsonValue v;
        v.m_type = JsonType::String;
        v.m_stringValue = value;
        return v;
    }
    
private:
    JsonType m_type;
    bool m_boolValue = false;
    int m_intValue = 0;
    float m_floatValue = 0.0f;
    std::string m_stringValue;
};

// Note: Full JSON parsing is complex. For production use, integrate nlohmann/json.
// This is a minimal implementation for demonstration purposes.

} // namespace Serialization
} // namespace Chronicles
