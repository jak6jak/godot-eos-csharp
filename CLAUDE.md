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

## Comprehensive Project Issues Analysis (Updated 2025-08-07)

### 1. Critical Architectural and Implementation Flaws

#### **1.1 Incomplete Remote Connection System** (`EOSClient.cs:129-142`)
- **Issue**: Remote ProductUserId resolution is completely unimplemented
- **Code Problem**: Method `ResolveTargetUserId()` returns null for all remote connections
- **Impact**: Only localhost connections work; no actual P2P networking possible
- **Root Cause**: Missing EOS Friends API, lobby system, or session management integration

#### **1.2 Broken Connection Handshake Protocol** (`EOSClient.cs:98-119`)
- **Issue**: Connection establishment is fake - just updates local state without proper P2P handshake
- **Code Problem**: No actual EOS P2P connection request is sent; relies on manual server.AddConnection()
- **Impact**: Connections appear successful but data transfer will fail
- **Missing**: Proper EOS P2P connection negotiation flow

#### **1.3 Hardcoded Socket Management**
- **Issue**: Single hardcoded socket name "RiptideSocket" throughout networking layer
- **Code Problem**: No dynamic socket management, conflicts with multiple connections
- **Impact**: Cannot handle multiple connection types, no isolation
- **Missing**: Dynamic socket management and cleanup

### 2. Networking Protocol and Reliability Issues

#### **2.1 Inadequate Message Processing** (`EOSPeer.cs:92-135`)
- **Issue**: Synchronous polling with arbitrary packet limits
- **Code Problem**: Rate limiting can drop legitimate packets, no message ordering guarantees
- **Impact**: Unreliable message delivery under load
- **Missing**: Proper message queuing, rate limiting, and flow control

#### **2.2 Unsafe Memory Management** (`EOSPeer.cs:46-87`)
- **Issue**: Array copying and inefficient buffer allocation on every send
- **Code Problem**: `Array.Copy()` calls cause high GC pressure
- **Impact**: Performance degradation under load
- **Missing**: Buffer pooling (MessageBuffer.cs is disabled)

#### **2.3 Missing NAT Traversal Implementation**
- **Issue**: No use of EOS NAT-P2P capabilities
- **Code Problem**: Only uses basic P2P send/receive, no connection quality monitoring
- **Impact**: Connections will fail behind NAT/firewalls
- **Missing**: Relay fallback, hole punching, connection quality management

### 3. Authentication and Session Management Problems

#### **3.1 Fragile Authentication Dependencies** (`ConnectService.cs`, `AuthService.cs`)
- **Issue**: Hard dependency on specific authentication order and Steam availability
- **Code Problem**: Authentication failures cascade without graceful fallbacks
- **Impact**: Entire system breaks if any auth component fails
- **Missing**: Robust retry mechanisms, authentication state recovery

#### **3.2 Unsafe Steam Integration** (`ConnectService.cs:325-396`)
- **Issue**: Steam initialization can fail silently, blocking ticket generation
- **Code Problem**: No exception handling for Steam API calls
- **Impact**: Authentication flow breaks if Steam is unavailable
- **Missing**: Async Steam operations, proper error propagation

#### **3.3 Account Linking Race Conditions** (`ConnectService.cs:481-528`)
- **Issue**: Multiple simultaneous login attempts interfere with global state
- **Code Problem**: Instance-level `TaskCompletionSource<AccountChoice>`
- **Impact**: Authentication corruption with concurrent users
- **Missing**: Per-request state isolation, timeout handling

### 4. Build System and Dependency Issues

#### **4.1 Fragile SDK Path Dependencies** (`EOSPluign.csproj:22-31`)
- **Issue**: Hardcoded relative path to SDK outside project directory
- **Code Problem**: Build breaks if SDK isn't in exact expected location
- **Impact**: Non-portable builds, difficult setup for new developers
- **Missing**: Flexible SDK path resolution, NuGet packaging

#### **4.2 Platform-Specific Library Loading Problems** (`EOSInterfaceManager.cs:143-192`)
- **Issue**: Complex native library resolver with potential security holes
- **Code Problem**: Searches multiple directories without validation
- **Impact**: DLL hijacking vulnerabilities, inconsistent loading behavior
- **Missing**: Secure library loading, digital signature verification

#### **4.3 Configuration System Vulnerabilities** (`EOSConfiguration.cs`)
- **Issue**: Configuration secrets stored in plain text
- **Code Problem**: EOS credentials in `EOSconfig.cfg` without encryption
- **Impact**: API keys exposed in version control and deployments
- **Missing**: Secure credential storage, environment variable support

### 5. Error Handling and Debugging Problems

#### **5.1 Insufficient Error Propagation**
- **Issue**: Errors are logged but not properly propagated to calling code
- **Code Problem**: `GD.PushError()` calls without return value checks
- **Impact**: Silent failures, difficult debugging, poor user experience
- **Missing**: Structured error handling, error recovery strategies

#### **5.2 Inadequate Connection State Management**
- **Issue**: No connection state machine or lifecycle management
- **Code Problem**: Connections can be in undefined states
- **Impact**: Resource leaks, zombie connections, unpredictable behavior
- **Missing**: Formal state machine, connection timeouts, cleanup procedures

### 6. Security and Production Readiness Issues

#### **6.1 Missing Input Validation**
- **Issue**: No validation of remote data, ProductUserIds, or configuration values
- **Impact**: Potential crashes, security vulnerabilities
- **Missing**: Data sanitization, bounds checking, format validation

#### **6.2 No Rate Limiting or Abuse Protection**
- **Issue**: No protection against flooding, large packets, or rapid connections
- **Impact**: DoS vulnerabilities, resource exhaustion
- **Missing**: Connection throttling, packet size limits, flood protection

#### **6.3 Insecure Development Defaults** (`EOSConfiguration.cs:58-62`)
- **Issue**: Development auth credentials hardcoded
- **Code Problem**: `DevAuthToken = "DevUser1"` in configuration
- **Impact**: Development backdoors in production builds
- **Missing**: Production/development configuration separation

### 7. Performance and Scalability Issues

#### **7.1 Inefficient Polling Architecture**
- **Issue**: Manual polling in every frame across multiple systems
- **Impact**: CPU waste, inconsistent timing, poor scaling
- **Missing**: Event-driven architecture, proper threading

#### **7.2 Lack of Connection Pooling**
- **Issue**: Resources are created/destroyed continuously (MessageBuffer.cs disabled)
- **Impact**: GC pressure, allocation overhead
- **Missing**: Connection pooling, buffer reuse

### 8. Missing Production Features

#### **8.1 No Session Discovery/Matchmaking**
- **Issue**: No way to discover or connect to remote players
- **Missing**: Lobby integration, session browser, friend invitation system

#### **8.2 No Connection Migration**
- **Issue**: No host migration or connection quality management
- **Missing**: Host failover, reconnection logic, quality monitoring

#### **8.3 No Metrics or Monitoring**
- **Issue**: No networking statistics or performance monitoring
- **Missing**: Connection metrics, bandwidth monitoring, error reporting

### Recommendations for Production Use

1. **Complete Remote Connection Implementation**: Implement proper EOS Friends API or lobby-based user discovery
2. **Implement Proper P2P Handshake**: Use EOS connection request/accept flow instead of fake local connections
3. **Add NAT Traversal Support**: Integrate EOS relay system and connection quality monitoring
4. **Secure Configuration Management**: Move credentials to secure storage, support environment variables
5. **Add Comprehensive Error Handling**: Implement retry logic, graceful degradation, and proper error reporting
6. **Implement Connection State Management**: Add proper lifecycle management and resource cleanup
7. **Add Security Measures**: Input validation, rate limiting, secure library loading
8. **Replace Polling with Events**: Use async/await patterns and event-driven networking
9. **Add Production Logging**: Implement structured logging and metrics collection
10. **Comprehensive Testing**: Add unit tests, integration tests, and multi-client scenarios

**Current Status**: This implementation is only suitable for local development and testing. Significant architectural changes would be required to make it production-ready for real multiplayer gaming.