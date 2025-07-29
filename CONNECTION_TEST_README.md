# EOS P2P Connection Test

This test automatically logs into EOS and tests P2P connections between two clients.

## How to Run

1. **Open Godot** and load this project
2. **Load the SimpleConnectionTest.tscn scene** in the editor
3. **Run the scene** (F6 or click the play scene button)

## What the Test Does

The test will automatically:

1. **Check for existing EOS login** - If already logged in, it will use the existing session
2. **Attempt automatic login** - If not logged in, it will call `SmartLogin()` and `ConnectService.Login()`
3. **Start EOS server** - Creates a server listening on port 7777
4. **Connect Client 1** - Attempts localhost P2P connection
5. **Connect Client 2** - Attempts second localhost P2P connection
6. **Report results** - Shows success/failure status

## Expected Output

You should see console output like:
```
=== Simple EOS P2P Connection Test ===
[ConnectionTest] Checking EOS login status...
[ConnectionTest] Attempting automatic EOS login...
[ConnectionTest] Login successful! User ID: [your-user-id]
[ConnectionTest] Starting EOS server...
[ConnectionTest] Server started successfully
[ConnectionTest] Connecting Client 1...
[ConnectionTest] Client 1 connected successfully
[ConnectionTest] Connecting Client 2...
[ConnectionTest] Client 2 connected successfully
[ConnectionTest] SUCCESS: Both clients connected!
[ConnectionTest] === CONNECTION TEST COMPLETE ===
```

## Enhanced Logging

The test now includes detailed logging from:
- **EOSServer**: Server startup, notification setup, connection handling
- **EOSClient**: Connection attempts, localhost handling, connection creation
- **SimpleConnectionTest**: Overall test flow and status updates

## Troubleshooting

- If login fails, check your EOS configuration in the EOSConfiguration
- If connections fail, check the console for detailed error messages
- The test automatically handles cleanup on exit

## Files

- **SimpleConnectionTest.cs** - Main test script with automatic login
- **SimpleConnectionTest.tscn** - Scene file to run the test
- Enhanced logging in **EOSServer.cs** and **EOSClient.cs**