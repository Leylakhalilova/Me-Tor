#!/bin/bash

# ANSI Color codes for colorful terminal output (Kitty friendly)
RED='\033[0;31m'
GREEN='\033[1;32m'
YELLOW='\033[1;33m'
CYAN='\033[1;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}=== MiniNotepad Bundling & Packaging Tool ===${NC}"

# Find repository root directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"
cd "$REPO_ROOT"

SUBMISSION_DIR="240201092-240201016"

echo -e "${YELLOW}[1/5] Re-creating submission directory: ${SUBMISSION_DIR}...${NC}"
rm -rf "$SUBMISSION_DIR"
mkdir -p "$SUBMISSION_DIR"

echo -e "${YELLOW}[2/5] Copying source code files...${NC}"
mkdir -p "$SUBMISSION_DIR/Core"
mkdir -p "$SUBMISSION_DIR/IO"
mkdir -p "$SUBMISSION_DIR/UI"
mkdir -p "$SUBMISSION_DIR/Utils"

cp src/MiniNotepad/Program.cs "$SUBMISSION_DIR/"
cp src/MiniNotepad/Core/*.cs "$SUBMISSION_DIR/Core/"
cp src/MiniNotepad/IO/*.cs "$SUBMISSION_DIR/IO/"
cp src/MiniNotepad/UI/*.cs "$SUBMISSION_DIR/UI/"
cp src/MiniNotepad/Utils/*.cs "$SUBMISSION_DIR/Utils/"

echo -e "${YELLOW}[3/5] Compiling self-contained executables (SingleFile)...${NC}"

# 1. Linux x64 publish
echo -e "${CYAN}Building for Linux x64...${NC}"
dotnet publish src/MiniNotepad/MiniNotepad.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -p:DebugSymbols=false -o ./publish/linux > /dev/null

if [ $? -eq 0 ]; then
    cp ./publish/linux/main "$SUBMISSION_DIR/main"
    echo -e "${GREEN}Linux build success -> $SUBMISSION_DIR/main${NC}"
else
    echo -e "${RED}Linux build failed!${NC}"
fi

# 2. Windows x64 publish
echo -e "${CYAN}Building for Windows x64...${NC}"
dotnet publish src/MiniNotepad/MiniNotepad.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -p:DebugSymbols=false -o ./publish/windows > /dev/null

if [ $? -eq 0 ]; then
    cp ./publish/windows/main.exe "$SUBMISSION_DIR/main.exe"
    echo -e "${GREEN}Windows build success -> $SUBMISSION_DIR/main.exe${NC}"
else
    echo -e "${RED}Windows build failed!${NC}"
fi

# Clean temp publish directory
rm -rf publish

echo -e "${YELLOW}[4/5] Re-creating Release/ folder and copying binaries...${NC}"
rm -rf Release
mkdir -p Release
cp "$SUBMISSION_DIR/main" Release/
cp "$SUBMISSION_DIR/main.exe" Release/

echo -e "${YELLOW}[5/5] Automatically generating concatenated Kodlar.txt...${NC}"
KODLAR_FILE="Kodlar.txt"
rm -f "$KODLAR_FILE"

# CS Files in order
CS_FILES=(
    "src/MiniNotepad/Program.cs"
    "src/MiniNotepad/Core/Cursor.cs"
    "src/MiniNotepad/Core/EditorBuffer.cs"
    "src/MiniNotepad/Core/UndoManager.cs"
    "src/MiniNotepad/IO/FileExplorer.cs"
    "src/MiniNotepad/UI/Renderer.cs"
    "src/MiniNotepad/UI/Toolbox.cs"
    "src/MiniNotepad/Utils/SearchAlgorithms.cs"
)

for file in "${CS_FILES[@]}"; do
    if [ -f "$file" ]; then
        echo -e "${CYAN}Adding $file to Kodlar.txt...${NC}"
        cat "$file" >> "$KODLAR_FILE"
        echo "" >> "$KODLAR_FILE"
    else
        echo -e "${RED}Warning: File $file not found!${NC}"
    fi
done

echo -e "${GREEN}=== Bundling completed successfully! ===${NC}"
echo -e "${CYAN}Submission folder: $SUBMISSION_DIR/${NC}"
echo -e "${CYAN}Release folder: Release/${NC}"
echo -e "${CYAN}Concatenated code file: Kodlar.txt${NC}"
