# CanoEh Seller Development Startup Script (PowerShell)
# This script starts both the API server and Seller client for development

$ErrorActionPreference = "Stop"

Write-Host "🚀 Starting CanoEh Seller Development Environment" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green

# Get the script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir = Split-Path -Parent $ScriptDir

Write-Host "📁 Project root: $RootDir" -ForegroundColor Cyan

# Function to cleanup background processes
function Cleanup {
    Write-Host ""
    Write-Host "🧹 Cleaning up..." -ForegroundColor Yellow
    
    # Stop any background jobs
    Get-Job | Stop-Job
    Get-Job | Remove-Job
    
    Write-Host "✅ Cleanup complete" -ForegroundColor Green
}

# Set trap to cleanup on script exit
Register-EngineEvent PowerShell.Exiting -Action { Cleanup }

# Check if .NET is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ .NET SDK version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: .NET SDK is not installed or not in PATH" -ForegroundColor Red
    Write-Host "   Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# Check if Node.js is installed
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js version: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Error: Node.js is not installed or not in PATH" -ForegroundColor Red
    Write-Host "   Please install Node.js from https://nodejs.org/" -ForegroundColor Yellow
    exit 1
}

# Step 1: Build the solution
Write-Host "🔨 Building .NET solution..." -ForegroundColor Cyan
Set-Location $RootDir
$buildResult = dotnet build --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error: Failed to build .NET solution" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build successful" -ForegroundColor Green

# Step 2: Install npm dependencies
Write-Host "📦 Installing npm dependencies..." -ForegroundColor Cyan
Set-Location "$RootDir\Seller\seller.client"
$npmResult = npm install --silent
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error: Failed to install npm dependencies" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Dependencies installed" -ForegroundColor Green

# Step 3: Start API server in background
Write-Host "🌐 Starting API server on https://localhost:7182..." -ForegroundColor Cyan
Set-Location "$RootDir\API"

# Start API server as a background job
$apiJob = Start-Job -ScriptBlock {
    Set-Location $using:RootDir\API
    dotnet run --launch-profile https
}

# Wait for API server to start
Write-Host "⏳ Waiting for API server to start..." -ForegroundColor Yellow
$timeout = 30
$elapsed = 0
$apiReady = $false

while ($elapsed -lt $timeout -and -not $apiReady) {
    Start-Sleep -Seconds 1
    $elapsed++
    
    try {
        $response = Invoke-WebRequest -Uri "https://localhost:7182/api/Category/GetAllCategories" -SkipCertificateCheck -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $apiReady = $true
        }
    } catch {
        # Continue waiting
    }
}

if (-not $apiReady) {
    Write-Host "❌ Error: API server failed to start within 30 seconds" -ForegroundColor Red
    Cleanup
    exit 1
}

Write-Host "✅ API server is ready!" -ForegroundColor Green

# Step 4: Start Seller client
Write-Host "⚛️  Starting Seller client on https://localhost:62209..." -ForegroundColor Cyan
Set-Location "$RootDir\Seller\seller.client"

Write-Host ""
Write-Host "🎉 Both services are starting!" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue
Write-Host "📱 Seller Client:  https://localhost:62209" -ForegroundColor Yellow
Write-Host "🔧 API Server:     https://localhost:7182" -ForegroundColor Yellow  
Write-Host "📚 Swagger UI:     https://localhost:7182/swagger" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Blue
Write-Host ""
Write-Host "💡 Click 'Advanced' → 'Proceed to localhost (unsafe)' if you see certificate warnings" -ForegroundColor Cyan
Write-Host "🛑 Press Ctrl+C to stop both services" -ForegroundColor Cyan
Write-Host ""

try {
    # Start the seller client (this will run in foreground)
    npm run dev
} finally {
    Cleanup
}