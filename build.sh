#!/bin/bash
# Chronicles of a Drifter - Build Script
# Supports Windows (DirectX 11 default) and Linux/Unix (SDL2)

set -e  # Exit on error

echo "=========================================="
echo "  Chronicles of a Drifter - Build Script"
echo "=========================================="
echo ""

# Detect platform
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" ]]; then
    PLATFORM="Windows"
    RENDERER_DEFAULT="DirectX 11"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    PLATFORM="Linux"
    RENDERER_DEFAULT="SDL2"
elif [[ "$OSTYPE" == "darwin"* ]]; then
    PLATFORM="macOS"
    RENDERER_DEFAULT="SDL2"
else
    PLATFORM="Unix"
    RENDERER_DEFAULT="SDL2"
fi

echo "Platform detected: $PLATFORM"
echo "Default renderer: $RENDERER_DEFAULT"
echo ""

# Check prerequisites
echo "[1/4] Checking prerequisites..."

if ! command -v cmake &> /dev/null; then
    echo "ERROR: CMake is not installed. Please install CMake 3.20 or later."
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK is not installed. Please install .NET 9 SDK."
    exit 1
fi

# Check for SDL2 on non-Windows platforms
if [[ "$PLATFORM" != "Windows" ]]; then
    if ! pkg-config --exists sdl2 2>/dev/null; then
        echo "ERROR: SDL2 is required on $PLATFORM."
        echo "Please install SDL2 development libraries:"
        echo "  Ubuntu/Debian: sudo apt-get install libsdl2-dev"
        echo "  Fedora: sudo dnf install SDL2-devel"
        echo "  macOS: brew install sdl2"
        exit 1
    fi
    echo "✓ SDL2 found: $(pkg-config --modversion sdl2)"
fi

echo "✓ CMake found: $(cmake --version | head -n1)"
echo "✓ .NET found: $(dotnet --version)"
echo ""

# Build C++ Engine
echo "[2/4] Building C++ engine..."
mkdir -p build
cd build

# Configure
echo "  Configuring CMake..."
cmake .. > /dev/null 2>&1 || {
    echo "ERROR: CMake configuration failed"
    exit 1
}

# Build
echo "  Compiling C++ code..."
cmake --build . --config Release > /dev/null 2>&1 || {
    echo "ERROR: C++ build failed"
    exit 1
}

echo "✓ C++ engine built successfully"
echo ""

# Build C# Game
echo "[3/4] Building C# game..."
cd ../src/Game

dotnet build -c Release --nologo -v quiet || {
    echo "ERROR: C# build failed"
    exit 1
}

echo "✓ C# game built successfully"
echo ""

# Verify build
echo "[4/4] Verifying build..."
cd ../../

if [ -f "build/lib/libChroniclesEngine.so" ]; then
    echo "✓ Found: build/lib/libChroniclesEngine.so"
elif [ -f "build/bin/ChroniclesEngine.dll" ]; then
    echo "✓ Found: build/bin/ChroniclesEngine.dll"
else
    echo "ERROR: Native engine library not found!"
    exit 1
fi

if [ -f "src/Game/bin/Release/net9.0/ChroniclesOfADrifter.dll" ]; then
    echo "✓ Found: src/Game/bin/Release/net9.0/ChroniclesOfADrifter.dll"
else
    echo "ERROR: C# game assembly not found!"
    exit 1
fi

echo ""
echo "=========================================="
echo "  Build completed successfully!"
echo "=========================================="
echo ""
echo "To run the game:"
echo "  cd src/Game"
echo "  dotnet run -c Release"
echo ""
echo "To run tests:"
echo "  cd src/Game"
echo "  dotnet run -c Release -- test"
echo ""
