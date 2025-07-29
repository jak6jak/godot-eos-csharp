#!/bin/bash

# Script to export and run multiple instances for EOS P2P testing

echo "=== EOS P2P Multi-Instance Test Script ==="
echo

# Check if Godot is available
if ! command -v godot &> /dev/null; then
    echo "ERROR: Godot not found in PATH"
    echo "Please install Godot or add it to your PATH"
    exit 1
fi

# Create build directory
BUILD_DIR="build"
APP_NAME="EOSConnectionTest"

echo "1. Creating build directory..."
mkdir -p "$BUILD_DIR"

echo "2. Exporting project..."
# Export for current platform
godot --headless --export-debug "Mac OSX" "$BUILD_DIR/$APP_NAME.app" --quit

if [ $? -ne 0 ]; then
    echo "ERROR: Export failed"
    echo "Make sure you have export templates installed"
    echo "Go to Editor -> Manage Export Templates in Godot"
    exit 1
fi

echo "3. Export completed successfully!"
echo

# Ask user if they want to run multiple instances
read -p "Do you want to run two instances now? (y/n): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "4. Starting first instance..."
    "$BUILD_DIR/$APP_NAME.app/Contents/MacOS/$APP_NAME" &
    INSTANCE1_PID=$!
    
    echo "5. Waiting 3 seconds..."
    sleep 3
    
    echo "6. Starting second instance..."
    "$BUILD_DIR/$APP_NAME.app/Contents/MacOS/$APP_NAME" &
    INSTANCE2_PID=$!
    
    echo
    echo "Two instances are now running!"
    echo "Instance 1 PID: $INSTANCE1_PID"
    echo "Instance 2 PID: $INSTANCE2_PID"
    echo
    echo "Watch both windows for connection test results."
    echo "Press Ctrl+C to stop both instances."
    
    # Wait for user to stop
    trap "echo 'Stopping instances...'; kill $INSTANCE1_PID $INSTANCE2_PID 2>/dev/null; exit" INT
    wait
else
    echo "4. Skipping instance launch."
    echo "You can manually run: $BUILD_DIR/$APP_NAME.app/Contents/MacOS/$APP_NAME"
fi

echo "Done!"