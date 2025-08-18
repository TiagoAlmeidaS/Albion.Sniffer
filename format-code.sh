#!/bin/bash

# Script to format and lint C# code

echo "🔧 Code Formatting and Linting Tool"
echo "===================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ Error: dotnet CLI is not installed${NC}"
    echo "Please install .NET SDK 8.0 or later"
    exit 1
fi

# Navigate to workspace root
cd "$(dirname "$0")"

# Restore tools
echo -e "${YELLOW}📦 Restoring dotnet tools...${NC}"
dotnet tool restore

if [ $? -ne 0 ]; then
    echo -e "${YELLOW}⚠️  Tools not found, installing...${NC}"
    dotnet new tool-manifest --force
    dotnet tool install dotnet-format
    dotnet tool install dotnet-reportgenerator-globaltool
fi

# Run format
echo -e "${YELLOW}🎨 Running code formatter...${NC}"
dotnet format --verbosity diagnostic

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Code formatting completed successfully${NC}"
else
    echo -e "${RED}❌ Code formatting failed${NC}"
    exit 1
fi

# Build to check for errors
echo -e "${YELLOW}🔨 Building solution...${NC}"
dotnet build --no-restore

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Build successful${NC}"
else
    echo -e "${RED}❌ Build failed${NC}"
    exit 1
fi

# Run analyzers
echo -e "${YELLOW}🔍 Running code analyzers...${NC}"
dotnet build --no-restore /p:RunAnalyzers=true /p:TreatWarningsAsErrors=false

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✅ Code analysis completed${NC}"
else
    echo -e "${YELLOW}⚠️  Code analysis found issues${NC}"
fi

echo ""
echo -e "${GREEN}🎉 Code formatting and linting completed!${NC}"