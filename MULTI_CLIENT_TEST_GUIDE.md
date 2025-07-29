# Testing Two EOS Clients - Multi-Instance Guide

## Method 1: Export and Run Multiple Instances (Recommended)

### Step 1: Export the Project
1. In Godot, go to **Project → Export**
2. Add export template for your platform (Mac/Windows/Linux)
3. Set **Main Scene** to `SimpleConnectionTest.tscn`
4. Export to a folder (e.g., `build/EOSConnectionTest`)

### Step 2: Run Multiple Instances
1. **Run first instance**: Double-click the exported executable
2. **Run second instance**: Open terminal and run:
   ```bash
   # Mac example:
   ./build/EOSConnectionTest.app/Contents/MacOS/EOSConnectionTest
   
   # Windows example:
   ./build/EOSConnectionTest.exe
   
   # Linux example:
   ./build/EOSConnectionTest
   ```

### Step 3: Monitor Both Windows
- Each instance will automatically log into EOS with the same user
- Watch the console output in both windows
- Both should show successful P2P connections

## Method 2: Command Line with Different Arguments

### Run from terminal with different instance IDs:
```bash
# Instance 1
godot --main-pack project.pck -- --client-id=1

# Instance 2 
godot --main-pack project.pck -- --client-id=2
```

## Method 3: Modified Test for Single Instance (Current Approach)

The current `SimpleConnectionTest.cs` creates:
- **1 Server** (accepting connections)
- **2 Clients** (both connecting to the same server via localhost)

This tests the P2P transport layer but both clients run in the same process with the same EOS user.

## Method 4: Create Dual-Instance Test Script

I can create a script that manages two separate connection contexts within one Godot instance.

## Which method would you prefer?

1. **Export and run multiple instances** - Most realistic test
2. **Modify current test** to better simulate separate clients
3. **Create a new dual-context test** - Two separate EOS contexts in one instance
4. **Command line approach** - Run with different parameters

Let me know which approach you'd like me to implement!