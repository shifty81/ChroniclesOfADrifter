"""
Chronicles of a Drifter - Engine Bindings
Python bindings for the ChroniclesEngine using ctypes
"""

import ctypes
import sys
import os
from pathlib import Path

# Locate the engine DLL/SO
def find_engine_library():
    """Find the ChroniclesEngine library"""
    if sys.platform == "win32":
        lib_name = "ChroniclesEngine.dll"
        search_paths = [
            Path("build/bin"),
            Path("../../build/bin"),
            Path(".")
        ]
    else:
        lib_name = "libChroniclesEngine.so"
        search_paths = [
            Path("build/lib"),
            Path("../../build/lib"),
            Path(".")
        ]
    
    for path in search_paths:
        lib_path = path / lib_name
        if lib_path.exists():
            return str(lib_path.absolute())
    
    raise FileNotFoundError(f"Could not find {lib_name} in search paths")

# Load the engine library
try:
    _engine = ctypes.CDLL(find_engine_library())
except Exception as e:
    print(f"Warning: Could not load ChroniclesEngine: {e}")
    print("Python tooling will have limited functionality.")
    _engine = None

# ===== Reflection API Bindings =====

if _engine:
    # Type query functions
    _engine.Reflection_GetTypeCount.restype = ctypes.c_int
    _engine.Reflection_GetTypeCount.argtypes = []
    
    _engine.Reflection_GetTypeName.restype = None
    _engine.Reflection_GetTypeName.argtypes = [ctypes.c_int, ctypes.c_char_p, ctypes.c_int]
    
    _engine.Reflection_GetTypeSize.restype = ctypes.c_int
    _engine.Reflection_GetTypeSize.argtypes = [ctypes.c_char_p]
    
    _engine.Reflection_GetFieldCount.restype = ctypes.c_int
    _engine.Reflection_GetFieldCount.argtypes = [ctypes.c_char_p]
    
    _engine.Reflection_GetFieldName.restype = None
    _engine.Reflection_GetFieldName.argtypes = [ctypes.c_char_p, ctypes.c_int, ctypes.c_char_p, ctypes.c_int]
    
    _engine.Reflection_GetFieldType.restype = ctypes.c_int
    _engine.Reflection_GetFieldType.argtypes = [ctypes.c_char_p, ctypes.c_char_p]
    
    # Value access functions
    _engine.Reflection_GetFloatValue.restype = ctypes.c_float
    _engine.Reflection_GetFloatValue.argtypes = [ctypes.c_char_p, ctypes.c_char_p, ctypes.c_void_p]
    
    _engine.Reflection_SetFloatValue.restype = None
    _engine.Reflection_SetFloatValue.argtypes = [ctypes.c_char_p, ctypes.c_char_p, ctypes.c_void_p, ctypes.c_float]
    
    # Serialization API
    _engine.Serialization_ToJson.restype = ctypes.c_int
    _engine.Serialization_ToJson.argtypes = [ctypes.c_char_p, ctypes.c_void_p, ctypes.c_char_p, ctypes.c_int]
    
    _engine.Serialization_SaveToFile.restype = ctypes.c_bool
    _engine.Serialization_SaveToFile.argtypes = [ctypes.c_char_p, ctypes.c_void_p, ctypes.c_char_p]

# ===== High-Level Python API =====

class PropertyType:
    """Property type enumeration"""
    BOOL = 0
    INT = 1
    FLOAT = 2
    DOUBLE = 3
    STRING = 4
    VECTOR2 = 5
    VECTOR3 = 6
    COLOR = 7
    CUSTOM = 8

class ReflectionSystem:
    """High-level reflection system wrapper"""
    
    @staticmethod
    def get_all_types():
        """Get all registered type names"""
        if not _engine:
            return []
        
        count = _engine.Reflection_GetTypeCount()
        types = []
        
        for i in range(count):
            buffer = ctypes.create_string_buffer(256)
            _engine.Reflection_GetTypeName(i, buffer, 256)
            types.append(buffer.value.decode('utf-8'))
        
        return types
    
    @staticmethod
    def get_type_info(type_name):
        """Get information about a type"""
        if not _engine:
            return None
        
        type_name_bytes = type_name.encode('utf-8')
        size = _engine.Reflection_GetTypeSize(type_name_bytes)
        if size <= 0:
            return None
        
        field_count = _engine.Reflection_GetFieldCount(type_name_bytes)
        fields = []
        
        for i in range(field_count):
            name_buffer = ctypes.create_string_buffer(256)
            _engine.Reflection_GetFieldName(type_name_bytes, i, name_buffer, 256)
            field_name = name_buffer.value.decode('utf-8')
            
            field_type = _engine.Reflection_GetFieldType(type_name_bytes, field_name.encode('utf-8'))
            
            fields.append({
                'name': field_name,
                'type': field_type
            })
        
        return {
            'name': type_name,
            'size': size,
            'fields': fields
        }
    
    @staticmethod
    def get_value(type_name, field_name, instance_ptr):
        """Get field value from instance"""
        if not _engine or not instance_ptr:
            return None
        
        type_name_bytes = type_name.encode('utf-8')
        field_name_bytes = field_name.encode('utf-8')
        
        field_type = _engine.Reflection_GetFieldType(type_name_bytes, field_name_bytes)
        
        if field_type == PropertyType.FLOAT:
            return _engine.Reflection_GetFloatValue(type_name_bytes, field_name_bytes, instance_ptr)
        # Add other types as needed
        
        return None
    
    @staticmethod
    def set_value(type_name, field_name, instance_ptr, value):
        """Set field value on instance"""
        if not _engine or not instance_ptr:
            return False
        
        type_name_bytes = type_name.encode('utf-8')
        field_name_bytes = field_name.encode('utf-8')
        
        field_type = _engine.Reflection_GetFieldType(type_name_bytes, field_name_bytes)
        
        if field_type == PropertyType.FLOAT:
            _engine.Reflection_SetFloatValue(type_name_bytes, field_name_bytes, instance_ptr, float(value))
            return True
        # Add other types as needed
        
        return False

class SerializationSystem:
    """High-level serialization system wrapper"""
    
    @staticmethod
    def to_json(type_name, instance_ptr):
        """Serialize object to JSON"""
        if not _engine or not instance_ptr:
            return None
        
        type_name_bytes = type_name.encode('utf-8')
        buffer = ctypes.create_string_buffer(4096)
        
        length = _engine.Serialization_ToJson(type_name_bytes, instance_ptr, buffer, 4096)
        
        if length > 0:
            return buffer.value.decode('utf-8')
        return None
    
    @staticmethod
    def save_to_file(type_name, instance_ptr, file_path):
        """Save object to JSON file"""
        if not _engine or not instance_ptr:
            return False
        
        type_name_bytes = type_name.encode('utf-8')
        file_path_bytes = file_path.encode('utf-8')
        
        return _engine.Serialization_SaveToFile(type_name_bytes, instance_ptr, file_path_bytes)

# ===== Example Usage =====

def print_all_types():
    """Example: Print all registered types"""
    print("Registered Types:")
    for type_name in ReflectionSystem.get_all_types():
        type_info = ReflectionSystem.get_type_info(type_name)
        print(f"\n  {type_name} ({type_info['size']} bytes)")
        for field in type_info['fields']:
            print(f"    - {field['name']}: {field['type']}")

if __name__ == "__main__":
    if _engine:
        print_all_types()
    else:
        print("Engine not loaded. Please build the C++ engine first.")
