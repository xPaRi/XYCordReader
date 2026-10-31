@echo off
setlocal

rem Publikuje aplikaci jako jediný exe (win-x64) do složky "publish". Na cílovém počítači musí být .NET 10 Desktop Runtime.

set "OUT=%~dp0publish"

if exist "%OUT%" rmdir /s /q "%OUT%"

dotnet publish "%~dp0XYCordReader.csproj" ^
    -c Release ^
    -r win-x64 ^
    --self-contained false ^
    -p:PublishSingleFile=true ^
    -p:DebugType=None ^
    -p:DebugSymbols=false ^
    -o "%OUT%"

if errorlevel 1 (
    echo.
    echo Publikace selhala.
    exit /b 1
)

echo.
echo Hotovo: %OUT%\XYCordReader.exe
