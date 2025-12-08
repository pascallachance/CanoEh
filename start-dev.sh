#!/bin/bash

# CanoEh Application Startup Script
# This script starts both the API backend and frontend development server

set -e

echo "🍁 Starting CanoEh Application..."
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get the directory of this script
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Check prerequisites
echo "Checking prerequisites..."

if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed. Please install .NET 8.0 SDK or later."
    exit 1
fi

if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js v20 or later."
    exit 1
fi

echo "✅ Prerequisites check passed"
echo ""

# First-time setup
if [ ! -d "Store/store.client/node_modules" ]; then
    echo "${YELLOW}Running first-time setup...${NC}"
    echo "Installing frontend dependencies..."
    cd Store/store.client
    npm install
    cd "$SCRIPT_DIR"
    echo "✅ Frontend dependencies installed"
    echo ""
fi

# Check if already running
if lsof -Pi :7182 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo "${YELLOW}⚠️  Port 7182 is already in use. API might already be running.${NC}"
    read -p "Continue anyway? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

if lsof -Pi :64941 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo "${YELLOW}⚠️  Port 64941 is already in use. Frontend might already be running.${NC}"
    read -p "Continue anyway? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

echo "${BLUE}Starting API Backend...${NC}"
cd API
dotnet run --launch-profile https > /tmp/canoeh-api.log 2>&1 &
API_PID=$!
cd "$SCRIPT_DIR"

# Wait for API to start
echo "Waiting for API to start..."
for i in {1..30}; do
    if curl -k -s -o /dev/null -w "%{http_code}" https://localhost:7182/swagger/index.html | grep -q "200"; then
        echo "${GREEN}✅ API Backend started successfully${NC}"
        echo "   - HTTPS: https://localhost:7182"
        echo "   - HTTP: http://localhost:5269"
        echo "   - Swagger: https://localhost:7182/swagger"
        echo ""
        break
    fi
    if [ $i -eq 30 ]; then
        echo "❌ API failed to start. Check logs at /tmp/canoeh-api.log"
        kill $API_PID 2>/dev/null || true
        exit 1
    fi
    sleep 1
done

echo "${BLUE}Starting Frontend Development Server...${NC}"
cd Store/store.client
npm run dev > /tmp/canoeh-frontend.log 2>&1 &
FRONTEND_PID=$!
cd "$SCRIPT_DIR"

# Wait for frontend to start
echo "Waiting for frontend to start..."
for i in {1..30}; do
    if curl -k -s -o /dev/null -w "%{http_code}" https://localhost:64941/ | grep -q "200"; then
        echo "${GREEN}✅ Frontend Development Server started successfully${NC}"
        echo "   - URL: https://localhost:64941"
        echo ""
        break
    fi
    if [ $i -eq 30 ]; then
        echo "❌ Frontend failed to start. Check logs at /tmp/canoeh-frontend.log"
        kill $API_PID 2>/dev/null || true
        kill $FRONTEND_PID 2>/dev/null || true
        exit 1
    fi
    sleep 1
done

echo ""
echo "${GREEN}🎉 CanoEh is now running!${NC}"
echo ""
echo "Access the application:"
echo "  • Frontend: ${BLUE}https://localhost:64941${NC}"
echo "  • Login: ${BLUE}https://localhost:64941/login${NC}"
echo "  • API: ${BLUE}https://localhost:7182${NC}"
echo "  • Swagger: ${BLUE}https://localhost:7182/swagger${NC}"
echo ""
echo "${YELLOW}Note: Accept the certificate warning in your browser when prompted.${NC}"
echo ""
echo "Process IDs:"
echo "  • API Backend: $API_PID"
echo "  • Frontend: $FRONTEND_PID"
echo ""
echo "Logs:"
echo "  • API: /tmp/canoeh-api.log"
echo "  • Frontend: /tmp/canoeh-frontend.log"
echo ""
echo "To stop the application, run: kill $API_PID $FRONTEND_PID"
echo "Or press Ctrl+C and manually stop the processes."
echo ""

# Keep script running and wait for interrupt
trap "echo ''; echo 'Shutting down...'; kill $API_PID $FRONTEND_PID 2>/dev/null; exit 0" INT TERM

# Follow logs
echo "Following logs (Ctrl+C to exit)..."
tail -f /tmp/canoeh-api.log /tmp/canoeh-frontend.log
