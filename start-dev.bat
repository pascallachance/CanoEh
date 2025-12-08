@echo off
REM CanoEh Application Startup Script for Windows
REM This script starts both the API backend and frontend development server

setlocal enabledelayedexpansion

echo Starting CanoEh Application...
echo.

REM Check prerequisites
echo Checking prerequisites...

where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK is not installed. Please install .NET 8.0 SDK or later.
    exit /b 1
)

where node >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] Node.js is not installed. Please install Node.js v20 or later.
    exit /b 1
)

echo [OK] Prerequisites check passed
echo.

REM First-time setup
if not exist "Store\store.client\node_modules" (
    echo Running first-time setup...
    echo Installing frontend dependencies...
    cd Store\store.client
    call npm install
    cd ..\..
    echo [OK] Frontend dependencies installed
    echo.
)

REM Start API Backend
echo Starting API Backend...
cd API
start "CanoEh API Backend" /MIN cmd /c "dotnet run --launch-profile https > ..\api.log 2>&1"
cd ..

REM Wait for API to start
echo Waiting for API to start...
timeout /t 5 /nobreak >nul

:check_api
curl -k -s -o nul -w "%%{http_code}" https://localhost:7182/swagger/index.html > temp_status.txt 2>nul
set /p API_STATUS=<temp_status.txt
del temp_status.txt

if "%API_STATUS%"=="200" (
    echo [OK] API Backend started successfully
    echo    - HTTPS: https://localhost:7182
    echo    - HTTP: http://localhost:5269
    echo    - Swagger: https://localhost:7182/swagger
    echo.
) else (
    timeout /t 2 /nobreak >nul
    goto check_api
)

REM Start Frontend
echo Starting Frontend Development Server...
cd Store\store.client
start "CanoEh Frontend" /MIN cmd /c "npm run dev > ..\..\frontend.log 2>&1"
cd ..\..

REM Wait for frontend to start
echo Waiting for frontend to start...
timeout /t 5 /nobreak >nul

:check_frontend
curl -k -s -o nul -w "%%{http_code}" https://localhost:64941/ > temp_status.txt 2>nul
set /p FRONTEND_STATUS=<temp_status.txt
del temp_status.txt

if "%FRONTEND_STATUS%"=="200" (
    echo [OK] Frontend Development Server started successfully
    echo    - URL: https://localhost:64941
    echo.
) else (
    timeout /t 2 /nobreak >nul
    goto check_frontend
)

echo.
echo ============================================
echo    CanoEh is now running!
echo ============================================
echo.
echo Access the application:
echo   * Frontend: https://localhost:64941
echo   * Login: https://localhost:64941/login
echo   * API: https://localhost:7182
echo   * Swagger: https://localhost:7182/swagger
echo.
echo Note: Accept the certificate warning in your browser when prompted.
echo.
echo Logs:
echo   * API: api.log
echo   * Frontend: frontend.log
echo.
echo To stop the application, close the terminal windows or use Task Manager.
echo.
echo Press any key to open the application in your default browser...
pause >nul

start https://localhost:64941/login

echo.
echo Application is running. Keep this window open.
echo Press Ctrl+C to see this message again, or close the window to stop monitoring.
echo.
pause
