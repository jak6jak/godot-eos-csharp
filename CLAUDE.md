# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Godot 4.4 C# project that implements Epic Online Services (EOS) integration with networking capabilities using RiptideNetworking. The project includes Steam integration as an optional third-party authentication provider.

**Key Components:**
- **EOS Plugin** (`addons/eosplugin/`) - Custom Godot plugin for EOS integration
- **Networking Layer** - Built on RiptideNetworking for P2P connections
- **Authentication Services** - Supports EOS Auth and EOS Connect with Steam fallback
- **Demo/Test Scenes** - Various connection test scenarios

## Development Commands

### Building and Running
```bash
# Build the project (use MSBuild or dotnet)
dotnet build EOSPluign.sln

# Run in Godot editor (opens main scene)
godot

# Run specific test scene directly
godot --main-pack project.pck res://SimpleConnectionTest.tscn

# Export and test multiple instances
bash export_and_test.sh
```

### Testing
- **Single Instance Test**: Run `SimpleConnectionTest.tscn` in editor (F6)
- **Multi-Instance Test**: Export project and run multiple executables
- **Dual Client Test**: Run `DualClientTest.tscn` for in-process testing

## Architecture

### EOS Plugin Structure
```
addons/eosplugin/
├── Core/
│   ├── EOSInterfaceManager.cs    # Main EOS platform manager (autoload)
│   ├── EOSConfiguration.cs      # Configuration system with reflection-based UI
│   └── BaseEOSService.cs        # Base class for EOS services
├── EOS Service Layer/
│   ├── Authentication/
│   │   ├── AuthService.cs       # EOS Auth interface
│   │   └── ConnectService.cs    # EOS Connect interface
│   └── NATP2P/                  # Network transport layer
│       ├── EOSServer.cs         # Server implementation
│       ├── EOSClient.cs         # Client implementation
│       └── EOSConnection.cs     # Connection management
└── thirdparty/                  # Native libraries (Windows/Mac/Linux)
```

### Configuration System
- Configuration stored in `EOSconfig.cfg`
- `EOSConfiguration.cs` uses reflection with `ConfigFieldAttribute` for dynamic UI generation
- Categories: Basic, Authentication, EOS Settings, Development
- Automatic validation and type conversion

### Authentication Flow
1. **EOS Auth** - Primary authentication via Epic account or DevAuth
2. **EOS Connect** - Creates product user ID, supports external auth (Steam)
3. **Steam Integration** - Optional third-party login via Facepunch.Steamworks

### Networking Architecture
- **Transport**: RiptideNetworking (UDP-based) with EOS P2P overlay
- **Server**: `EOSServer.cs` - Handles incoming connections, broadcasts
- **Client**: `EOSClient.cs` - Manages outbound connections to servers
- **Extensions**: `RiptideExtensions.cs` - Helper methods for Riptide integration

## Platform Support

### Dependencies by Platform
**Windows:**
- `EOSSDK-Win64-Shipping.dll`
- `steam_api64.dll`
- `Facepunch.Steamworks.Win64.dll`

**macOS:**
- `libEOSSDK-Mac-Shipping.dylib`
- `libsteam_api.dylib` (commented out in csproj)
- `Facepunch.Steamworks.Posix.dll`

**Linux:**
- `libEOSSDK-Linux-Shipping.so`
- `libsteam_api.so`

### Native Library Loading
- Custom `DllImportResolver` in `EOSInterfaceManager.cs`
- Automatic path resolution for different platforms
- Build targets copy libraries to output directory

## Key Configuration

### EOS Settings (EOSconfig.cfg)
```ini
[EOS]
ProductName="netpong"
ProductVersion="1.0"
DefaultCredentialType="ExternalAuth"
DefaultExternalCredentialType="SteamSessionTicket"
EosProductId="[your-product-id]"
EosSandboxId="[your-sandbox-id]"
EosDeploymentId="[your-deployment-id]"
EosClientId="[your-client-id]"
EosClientSecret="[your-client-secret]"
```

### Project Configuration
- **Target Framework**: .NET 8.0
- **Godot Version**: 4.4.1
- **Features**: C#, Mobile rendering
- **Autoload**: `EOSInterfaceManager` at startup

## Testing Scenarios

### Demo
The main demo scene (`Demo/MainWorld.tscn`) provides a complete multiplayer experience:

**Features:**
- **NetworkManager**: Handles EOS authentication and P2P networking setup
- **DemoUI**: Control panel for starting servers and connecting clients
- **Player System**: Synchronized 3D player movement with physics
- **Input Handling**: WASD movement + Space for jumping

**Usage:**
1. Run `Demo/MainWorld.tscn` in Godot editor
2. EOS authentication happens automatically 
3. Use UI buttons to start server and connect clients
4. Export project to test multiple instances

### SimpleConnectionTest
Automated test that:


### Multi-Instance Testing
1. Export project with `SimpleConnectionTest.tscn` as main scene
2. Run multiple executable instances
3. Each instance performs automatic EOS login and P2P connection tests

## Steam Integration

- **Optional Component**: Can be disabled by removing Steam references
- **Version**: Facepunch.Steamworks 1.61
- **File**: `steam_appid.txt` contains Steam App ID
- **Usage**: Third-party authentication for EOS Connect service

## Common Issues

### Native Library Loading
- Libraries must be in `addons/eosplugin/thirdparty/`
- Build process copies to output directory
- Custom resolver handles cross-platform loading

### EOS Configuration
- Missing required fields cause initialization failures
- Configuration UI validates all required fields
- Dev credentials available for testing without Epic account

### Networking
- P2P connections require successful EOS authentication
- Localhost connections supported for testing
- Port 7777 used by default for server instances