#!/bin/bash

# CanoEh Seller Development Startup Script
# This script starts both the API server and Seller client for development

set -e

echo "🚀 Starting CanoEh Seller Development Environment"
echo "=================================================="

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

echo "📁 Project root: $ROOT_DIR"

# Function to cleanup background processes
cleanup() {
    echo ""
    echo "🧹 Cleaning up..."
    
    # Kill background jobs
    jobs -p | xargs -r kill 2>/dev/null || true
    
    echo "✅ Cleanup complete"
    exit 0
}

# Set trap to cleanup on script exit
trap cleanup SIGINT SIGTERM EXIT

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK is not installed or not in PATH"
    echo "   Please install .NET 8.0 SDK from https://dotnet.microsoft.com/download"
    exit 1
fi

# Check if Node.js is installed
if ! command -v npm &> /dev/null; then
    echo "❌ Error: Node.js/npm is not installed or not in PATH"
    echo "   Please install Node.js from https://nodejs.org/"
    exit 1
fi

# Step 1: Build the solution
echo "🔨 Building .NET solution..."
cd "$ROOT_DIR"
dotnet build --verbosity quiet

if [ $? -ne 0 ]; then
    echo "❌ Error: Failed to build .NET solution"
    exit 1
fi

# Step 2: Install npm dependencies
echo "📦 Installing npm dependencies..."
cd "$ROOT_DIR/Seller/seller.client"
npm install --silent

if [ $? -ne 0 ]; then
    echo "❌ Error: Failed to install npm dependencies"
    exit 1
fi

# Step 3: Start API server in background
echo "🌐 Starting API server on https://localhost:7182..."
cd "$ROOT_DIR/API"
dotnet run --launch-profile https &
API_PID=$!

# Wait for API server to start
echo "⏳ Waiting for API server to start..."
for i in {1..30}; do
    if curl -k -s https://localhost:7182/api/Category/GetAllCategories >/dev/null 2>&1; then
        echo "✅ API server is ready!"
        break
    fi
    if [ $i -eq 30 ]; then
        echo "❌ Error: API server failed to start within 30 seconds"
        exit 1
    fi
    sleep 1
done

# Step 4: Start Seller client
echo "⚛️  Starting Seller client on https://localhost:62209..."
cd "$ROOT_DIR/Seller/seller.client"

echo ""
echo "🎉 Both services are starting!"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📱 Seller Client:  https://localhost:62209"
echo "🔧 API Server:     https://localhost:7182"
echo "📚 Swagger UI:     https://localhost:7182/swagger"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""
echo "💡 Click 'Advanced' → 'Proceed to localhost (unsafe)' if you see certificate warnings"
echo "🛑 Press Ctrl+C to stop both services"
echo ""

# Start the seller client (this will run in foreground)
npm run dev

# This line should never be reached, but just in case
wait