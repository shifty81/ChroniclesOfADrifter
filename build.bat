@echo off
REM Chronicles of a Drifter - Build Script for Windows
REM This script builds the C++ engine and C# game

setlocal enabledelayedexpansion

echo ==========================================
echo   Chronicles of a Drifter - Build Script
echo ==========================================
echo.

REM Check prerequisites
echo [1/4] Checking prerequisites...

where cmake >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: CMake is not installed. Please install CMake 3.20 or later.
    exit /b 1
)

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK is not installed. Please install .NET 9 SDK.
    exit /b 1
)

for /f "tokens=*" %%i in ('cmake --version ^| findstr /R "^cmake"') do set CMAKE_VER=%%i
for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VER=%%i

echo ✓ CMake found: %CMAKE_VER%
echo ✓ .NET found: %DOTNET_VER%
echo.

REM Build C++ Engine
echo [2/4] Building C++ engine...
if not exist build mkdir build
cd build

REM Configure
echo   Configuring CMake...
cmake .. -G "Visual Studio 17 2022" -A x64 >nul 2>&1
if %errorlevel% neq 0 (
    cmake .. >nul 2>&1
    if %errorlevel% neq 0 (
        echo ERROR: CMake configuration failed
        exit /b 1
    )
)

REM Build
echo   Compiling C++ code...
cmake --build . --config Release >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: C++ build failed
    exit /b 1
)

echo ✓ C++ engine built successfully
echo.

REM Build C# Game
echo [3/4] Building C# game...
cd ..\src\Game

dotnet build -c Release --nologo -v quiet >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: C# build failed
    exit /b 1
)

echo ✓ C# game built successfully
echo.

REM Verify build
echo [4/4] Verifying build...
cd ..\..

if exist "build\bin\ChroniclesEngine.dll" (
    echo ✓ Found: build\bin\ChroniclesEngine.dll
) else if exist "build\lib\libChroniclesEngine.so" (
    echo ✓ Found: build\lib\libChroniclesEngine.so
) else (
    echo ERROR: Native engine library not found!
    exit /b 1
)

if exist "src\Game\bin\Release\net9.0\ChroniclesOfADrifter.dll" (
    echo ✓ Found: src\Game\bin\Release\net9.0\ChroniclesOfADrifter.dll
) else (
    echo ERROR: C# game assembly not found!
    exit /b 1
)

echo.
echo ==========================================
echo   Build completed successfully!
echo ==========================================
echo.
echo To run the game:
echo   cd src\Game
echo   dotnet run -c Release
echo.
echo To run tests:
echo   cd src\Game
echo   dotnet run -c Release -- test
echo.

endlocal
