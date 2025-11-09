#!/bin/bash
# Chronicles of a Drifter - Build Script
# NOTE: This project is Windows-only with DirectX 11/12 support
# This script is NOT SUPPORTED and will fail on non-Windows platforms

set -e  # Exit on error

echo "=========================================="
echo "  Chronicles of a Drifter - Build Script"
echo "  ERROR: Windows-Only Project"
echo "=========================================="
echo ""
echo "This project is configured for Windows ONLY with DirectX 11/12."
echo "Cross-platform support (SDL2) has been removed."
echo ""
echo "Please use build.bat on Windows or Visual Studio 2022."
echo ""
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
