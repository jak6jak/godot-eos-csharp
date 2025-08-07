Epic Developer [Resources](https://dev.epicgames.com/docs) Platform Interface

# Platform Interface

Interface that grants access to all other interfaces.

Integrate the Platform Interface into your game (or other software application) to use the features of the Epic Online Services (EOS) Platform service. The Platform Interface sits at the heart of the EOS SDK and holds the handles you need to access every other interface and keep them all running. When your application starts up, you initialize the SDK and get a handle to the Platform Interface. This handle is usable for the lifetime of the SDK.

# Initializing the SDK

The first step to using the Epic Online Services (EOS) SDK is to initialize it. During initialization, your code will identify your **product** and have the opportunity to set up custom memory allocation functions.

## Configuring and Creating the SDK

Titles are in control of their presence information. Epic recommends setting presence information to codenames during development as an additional safeguard for the most sensitive games. The ProductName field in [EOS\\_Initiaize](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-initialize) is where the default game name comes from.

Initialize the EOS SDK by calling [EOS\\_Initiaize](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-initialize) with an EOS\_InitiaizeOptions data structure. Populate the structure as follows:

| Property                 | Value                                                                                                                                                                                                                                                                                                          |
|--------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ApiVersion               | EOS_INITIALIZE_API_LATEST                                                                                                                                                                                                                                                                                      |
| ProductName              | The name of the product using the SDK. The name string must not be empty and supports a maximum<br>of 64 characters in length. The name can consist of any of the following readable ANSI characters<br>(from 32-127): A-Z, a-z, 0-9, space, ! " # \$ % & ' ( ) * + , - . / : ; < = > ? @ [ \ ] ^ _ ` {   } ~. |
| ProductVersion           | The product version of the application using the SDK                                                                                                                                                                                                                                                           |
| Reserved                 | NULL                                                                                                                                                                                                                                                                                                           |
| AllocateMemoryFunction   | Your custom malloc function, of type EOS_AllocateMemoryFunc , or NULL                                                                                                                                                                                                                                          |
|                          | This function must return pointers that honor memory alignment.                                                                                                                                                                                                                                                |
| ReallocateMemoryFunction | Your custom realloc function, of type EOS_ReallocateMemoryFunc , or NULL                                                                                                                                                                                                                                       |
| ReleaseMemoryFunction    | Your custom free function, of type EOS_ReleaseMemoryFunc , or NULL                                                                                                                                                                                                                                             |
| SystemInitializeOptions  | A field for any system-specific initialization. If provided then the information will be passed to the<br>EOS_\<System>_InitializeOptions structure, where \<System> is the system being initialized.                                                                                                          |
| OverrideThreadAffinity   | A field for any thread affinity initialization that is EOS_Initialize_ThreadAffinity type. The<br>information, if it is provided, will be used when creating any threads during the operation of the EOS                                                                                                       |

Property Value

Value

SDK. When set to null, the EOS SDK will use a default scheme for determining thread affinity. The EOS\_Initiaize\_ThreadAffinity structure is a set of affinity masks which identify categories of threads to use.

EOS\_Initiaize returns an EOS\_EResut to indicate success or failure. The value will be EOS\_Success if the SDK initialized successfully; otherwise, the value will indicate an error, such as EOS\_AreadyConfigured . After initializing the SDK, you can create a **Platform Interface**.

# Logging Callbacks

The SDK logs useful information at various levels of verbosity. Registering a callback with EOS\_Logging\_SetCaback will allow access to this output. Implement a function of type EOS\_LogMessageFunc to receive the EOS\_LogMessage data structure.

- **Category**: A string corresponding to the category of the log message (see the [EOS\\_ELogCategory](https://dev.epicgames.com/docs/en-US/api-ref/enums/eos-e-log-category) API reference)
- **Message**: The message itself, as a string.
- **Level**: The verbosity level of the log message (see the [EOS\\_ELogLevel](https://dev.epicgames.com/docs/en-US/api-ref/enums/eos-e-log-level) API reference)

You may use EOS\_Logging\_SetLogLeve to adjust logging detail level.

# Platform Interface

The Platform Interface provides access to all other EOS SDK interfaces, and keeps them running. Once you have created the platform interface, you can use it to retrieve handles to other interfaces, or tell it to run its per-frame update code, known as ticking.

## Creating the Platform Interface

Create the Platform Interface by calling the EOS\_Patform\_Create function with an EOS\_Patform\_Options structure containing the following information:

| Property            | Value                                                                                                                                                                                                           |
|---------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| ApiVersion          | EOS_PLATFORM_OPTIONS_API_LATEST                                                                                                                                                                                 |
| Reserved            | NULL                                                                                                                                                                                                            |
| ProductId           | The Product ID of the game, provided by Epic Games                                                                                                                                                              |
| SandboxId           | The Sandbox ID of the game, provided by Epic Games                                                                                                                                                              |
| ClientCredentials   | The Client ID and Client Secret pair assigned to the host application<br>Publicly exposed applications, such as the end-user game client, will use<br>different credentials than a trusted game server backend. |
| bIsServer           | Set to EOS_False if the application is running as a client with a local user. Set to<br>EOS_True for a dedicated game server.                                                                                   |
| EncryptionKey       | 256-bit Encryption Key for file encryption in hexadecimal format (64 hex chars)                                                                                                                                 |
| OverrideCountryCode | The override country code for the logged-in user                                                                                                                                                                |

5/26/25, 7:06 AM Platform Interface | Epic Online Services Developer

| Property                                 | Value                                                                                                                                                                                                                                                                                                                                                                              |
|------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| OverrideLocaleCode                       | The override local code for the logged-in user                                                                                                                                                                                                                                                                                                                                     |
| DeploymentId                             | The Deployment ID of the game, provided by Epic Games                                                                                                                                                                                                                                                                                                                              |
| Flags                                    | Platform creation flags. This is a bitwise OR union of flags that start with EOS_PF_ .<br>For more information, see documentation: EOS Overlay Overview–EOS Platform Flags<br>for the EOS Overlay.                                                                                                                                                                                 |
| CacheDirectory                           | Absolute path to the folder that is going to be used for caching temporary data                                                                                                                                                                                                                                                                                                    |
| TickBudgetInMilliseconds                 | A budget, measured in milliseconds, for EOS_Platform_Tick to do its work. When<br>the budget is met or exceeded (or if no work is available), EOS_Platform_Tick will<br>return. This allows your game to amortize the cost of SDK work across multiple<br>frames in the event that a lot of work is queued for processing. Zero is interpreted as<br>"perform all available work". |
| RTCOptions                               | Pointer to EOS_Platform_RTCOptions structure for using Real Time<br>Communication features. Use NULL to disable the Real Time Communications (RTC)<br>features, such as Voice.                                                                                                                                                                                                     |
| IntegratedPlatformOptionsContainerHandle | A handle that contains all the options for setting up integrated platforms. When set<br>to NULL, the EOS Integrated Platform behavior for the host platform will be disabled.                                                                                                                                                                                                      |

Upon success, EOS\_Patform\_Create will return a handle to the Platform Interface, of type EOS\_HPatform .

In applications that support multiple views, such as editors, you may want to create multiple Platform Interface handles. This is not necessary to support multiple users playing on the same device, such as splitscreen games. The SDK supports multiple Platform Interface instances, each with its own internal state. Other interfaces that you retrieve will be unique to the Platform Interface from which you retrieve them. Do not attempt to initialize more than one instance of the SDK itself.

# Using the Platform Interface

Once you have an EOS\_HPatform handle, you can use it to gain access to the other EOS SDK interfaces through their handle access functions:

| Interface                     | Access Function                              |
|-------------------------------|----------------------------------------------|
| Achievements                  | EOS_Platform_GetAchievementsInterface        |
| Anti-Cheat                    | EOS_Platform_GetAntiCheatClientInterface     |
|                               | EOS_Platform_GetAntiCheatServerInterface     |
| Authentication                | EOS_Platform_GetAuthInterface                |
| Connect                       | EOS_Platform_GetConnectInterface             |
| Custom Invites                | EOS_Platform_GetCustomInvitesInterface       |
| Ecommerce                     | EOS_Platform_GetEcomInterface                |
| Interface                     | Access Function                              |
| Friends                       | EOS_Platform_GetFriendsInterface             |
| Leaderboards                  | EOS_Platform_GetLeaderboardsInterface        |
| Lobby                         | EOS_Platform_GetLobbyInterface               |
| Metrics                       | EOS_Platform_GetMetricsInterface             |
| Mods                          | EOS_Platforms_GetModsInterface               |
| P2P                           | EOS_Platform_GetP2PInterface                 |
| Player Data Storage           | EOS_Platform_GetPlayerDataStorageInterface   |
| Presence                      | EOS_Platform_GetPresenceInterface            |
| Progression Snapshot          | EOS_Platform_GetProgressionSnapshotInterface |
| Real-time Communication (RTC) | EOS_Platform_GetRTCInterface                 |
|                               | EOS_Platform_GetRTCAdminInterface            |
| Reports                       | EOS_Platform_GetReportsInterface             |
| Sanctions                     | EOS_Platform_GetSanctionsInterface           |
| Sessions                      | EOS_Platform_GetSessionsInfoInterface        |
| Stats                         | EOS_Platform_GetStatsInterface               |
| Title Storage                 | EOS_Platform_GetTitleStorageInterface        |
| User Info                     | EOS_Platform_GetUserInfoInterface            |

In addition to gaining access to the other interfaces, the Platform Interface keeps them all running. Call EOS\_Patform\_Tick from your game's main loop every frame to make sure that asynchronous functions continue updating.

## Restarting the App with the Launcher

When you pass an EOS\_HPatform hande into EOS\_Patform\_CheckForLauncherAndRestart , EOS checks whether the Epic Games Launcher launches the app. If the Epic Games Launcher doesn't launch the app, EOS\_Patform\_CheckForLauncherAndRestart restarts the app with the Epic Games Launcher.

EOS\_Patform\_CheckForLauncherAndRestart is only relevant for apps that are published on the store and, therefore, are already accessible through the Launcher.

**Note**: During the call to EOS\_Patform\_Create , the command line used to launch the app is inspected. If it is recognized as coming from the Epic Games Launcher, the environment variable, EOS\_PLATFORM\_CHECKFORLAUNCHERANDRESTART\_ENV\_VAR , is set to 1 .

You can force the EOS\_Patform\_CheckForLauncherAndRestart API to relaunch the app by explicitly unsetting EOS\_PLATFORM\_CHECKFORLAUNCHERANDRESTART\_ENV\_VAR before calling EOS\_Patform\_CheckForLauncherAndRestart .

**Note**: The APIs you use to interact with the environment variable, EOS\_PLATFORM\_CHECKFORLAUNCHERANDRESTART\_ENV\_VAR , are dependent on the operating system:

- On Windows, you must use SetEnvironmentVariabe and GetEnvironmentVariabe .
- On other platforms, you must use setenv and getenv .

This returns an EOS\_EResut with the following codes:

- EOS\_Success : The Epic Games Launcher is relaunching the app. You must terminate the current app process as soon as possible to make way for the newly launched process.
- EOS\_NoChange : The Epic Games Launcher has already launched the app. You don't need to do anything.
- EOS\_UnexpectedError : The LauncherCheck module failed to initialize, or the module tried and failed to restart the app.

# Shutting Down the SDK

To close the game, you must release the memory held by the Platform Interface as well as the global state held by the SDK. First, pass your EOS\_HPatform handle to the EOS\_Patform\_Reease function to shut it down. After that, you can call EOS\_Shutdown to complete the process and shut down.

Once you call EOS\_Shutdown, you will not be able to reinitialize the EOS SDK, and any further calls to it will fail.

## Application and Network Status

You must set your players' application (game) status and network status when you change either status. Setting these will prompt the EOS SDK, ensuring that RTC transitions properly for your game.

## Application Status

The application status notifies the EOS SDK whether the game is currently suspended.

EOS\_Patform\_SetAppicationStatus must be called when the application status changes. The application status can be set using the NewStatus parameter.

The following application states are defined with the EOS\_EAppicationStatus structure:

| EOS_AS_BackgroundSuspended | Notifies the SDK that the application has been put into a suspended state by the platform. This is<br>sometimes called "background mode." |
|----------------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| EOS_AS_Foreground          | Notifies the SDK that the application has been resumed from a suspended state. This is the default<br>active state on all platforms.      |

EOS\_Patform\_GetAppicationStatus can also be used to get the current application status.

The nature of these application status changes depends on the platform (see the platforms-specific documentation for more information).

You can only access console documentation if you have the appropriate permissions. See the Get Started Steps: EOS SDK [Download](https://dev.epicgames.com/docs/en-US/epic-online-services/eos-get-started/services-quick-start#eos-sdk-download-types) Types documentation for more information on how to access the EOS SDKs for consoles and their associated documentation.

What your game does for RTC rooms when your game transitions between states is up to you. The common practice is for all platforms to leave all RTC rooms and disallow joining back while in a suspended state (background mode). You can control this default behavior with the BackgroundMode option. See the [Background](https://dev.epicgames.com/docs/en-US/game-services/real-time-communication-interface/voice/using-voice#background-mode) Mode section of the Using the Voice Interface documentation for more information on how to set BackgroundMode .

### Network Status

The network status sets the status of the player's network connection.

You must call EOS\_Patform\_SetNetworkStatus when the network status changes. The network status can be set using the NewStatus parameter.

The following network states are defined with the EOS\_ENetworkStatus structure:

| EOS_NS_Disabled | Network cannot be used.                                                                                |
|-----------------|--------------------------------------------------------------------------------------------------------|
| EOS_NS_Offline  | The player may not be connected to the internet. The network can still be used, but is likely to fail. |
| EOS_NS_Online   | The player thinks they are connected to the internet.                                                  |

EOS\_Patform\_GetNetworkStatus can also be used to get the current network status.

The nature of these network status changes depends on the platform (see the platforms-specific documents for more information).

You can only access console documentation if you have the appropriate permissions. See the Get Started Steps: EOS SDK [Download](https://dev.epicgames.com/docs/en-US/epic-online-services/eos-get-started/services-quick-start#eos-sdk-download-types) Types documentation for more information on how to access the EOS SDKs for consoles and their associated documentation.

The common pattern is to clear resources and prevent further room joins when you change the network status to disabled ( EOS\_NS\_Disabed ) or offline ( EOS\_NS\_Offine ). The EOS SDK does this by default. This is the same for each platform. Lobby RTC restarts any paused RTC rooms when foregrounded and online. It does not try to reconnect otherwise.

**Note**: When you turn the network on and off, RTC can run into network interruptions, as it has to reconfigure its webRTC threads.

For PlayStation 4, PlayStation 5, Nintendo Switch and Xbox, the network status defaults to EOS\_NS\_Disabed . When the network is online, you must update the network status to EOS\_NS\_Onine with EOS\_Patform\_SetNetworkStatus . See the relevant [platform-specific](https://dev.epicgames.com/docs/en-US/epic-online-services/platforms) [documentation](https://dev.epicgames.com/docs/en-US/epic-online-services/platforms) for more information.

### **Configure Network Tasks Time-out**

By default, the EOS SDK times out network tasks after 30 seconds if the network is not available. If a task times out, the EOS SDK returns the error EOS\_TimeOut . You can override the default time-out (in seconds) by setting the TaskNetworkTimeoutSeconds property in EOS\_Patform\_Options when you create the Platform Interface. To override the default time-out, pass a pointer to a double containing the number of seconds that you want tasks to wait for the network to time out.

### **Notes**:

- The time-out applies when the network status is not EOS\_NS\_Onine .
- Tasks that need the network queue for up to this time, until your game calls EOS\_Patform\_SetNetworkStatus to set the network status to online.
- To use the default time-out of 30 seconds, pass a null pointer.

# Native Platform Integration

The SDK provides functionality to integrate automatically with the native platform. These integrations allow the game to:

- include platform friends in the EOS overlay's friends list
- mirror presence state of the local Epic user with their native platform presence
- replicate game invites of EOS Lobbies and EOS Sessions with the native platform system
- enable join-via-presence to the local user's EOS lobby or session for platform friends
- control system used to show game invite notifications (Game UI, Platform UI, **EOS Overlay** )

To configure the platform integrations to use:

- 1. Call EOS\_IntegratedPatform\_CreateIntegratedPatformOptionsContainer to create a new temporary container for the platform options.
- 2. Create the platform specific EOS\_IntegratedPatform\_<Patform>\_Options struct to specify the desired options to use. You can find the available options in the EOS SDK download for your platform in the /SDK/Incude/PLATFORM\_NAME/eos\_PLATFORM\_NAME.h file.

3. Call EOS\_IntegratedPatformOptionsContainer\_Add to register the options for SDK initialization.

4. After a successful call to EOS\_Patform\_Create , release the temporary container by calling EOS\_IntegratedPatformOptionsContainer\_Reease .

See the documentation for more information:

- To find the file that contains your platform's options, see your platform's documentation for the filename and location. To do this, go to the Native Platform Integration section of your platform in EOS SDK for [Platforms.](https://dev.epicgames.com/docs/en-US/epic-online-services/platforms)
- How to download the EOS SDK for your platform, see the Get Started Steps: [Download](https://dev.epicgames.com/docs/en-US/epic-online-services/eos-get-started/services-quick-start#step-2---download-the-eos-sdk) the EOS SDK documentation.
- How to integrate the EOS SDK with the platform, see the Platform [Interface](https://dev.epicgames.com/docs/en-US/game-services/eos-platform-interface) documentation.

#### **Note**:

You can only access console documentation if you have the appropriate permissions. See the Get Started Steps: EOS SDK [Download](https://dev.epicgames.com/docs/en-US/epic-online-services/eos-get-started/services-quick-start#eos-sdk-download-types) Types documentation for more information on how to access the EOS SDKs for consoles and their associated documentation.

## Sample Code

```
static EOS_EResut CreateIntegratedPatform(EOS_Patform_Options& PatformOptions)
{
 // Create the generic container.
 const EOS_IntegratedPatform_CreateIntegratedPatformOptionsContainerOptions CreateOptions =
 {
  EOS_INTEGRATEDPLATFORM_CREATEINTEGRATEDPLATFORMOPTIONSCONTAINER_API_LATEST
 };
 const EOS_EResut Resut = EOS_IntegratedPatform_CreateIntegratedPatformOptionsContainer(&CreateOptions,
&PatformOptions.IntegratedPatformOptionsContainerHande);
 if (Resut != EOS_EResut::EOS_Success)
 {
  return Resut;
 }
 // Configure patform-specific options.
 const EOS_IntegratedPatform_Steam_Options PatformSpecificOptions =
 {
   EOS_INTEGRATEDPLATFORM_STEAM_OPTIONS_API_LATEST,
   nuptr
 };
 // Add the configuration to the SDK initiaization options.
 const EOS_IntegratedPatform_Options Options =
 {
  EOS_INTEGRATEDPLATFORM_OPTIONS_API_LATEST,
  EOS_IPT_Steam,
  EOS_EIntegratedPatformManagementFags::EOS_IPMF_LibraryManagedByAppication |
EOS_EIntegratedPatformManagementFags::EOS_IPMF_DisabeSDKManagedSessions,
  &PatformSpecificOptions
 };
 const EOS_IntegratedPatformOptionsContainer_AddOptions AddOptions =
 {
  EOS_INTEGRATEDPLATFORMOPTIONSCONTAINER_ADD_API_LATEST,
  &Options
 };
 return EOS_IntegratedPatformOptionsContainer_Add(PatformOptions.IntegratedPatformOptionsContainerHande, &AddOptions);
}
static void FreeIntegratedPatform(EOS_Patform_Options& PatformOptions)
{
 // Free the created container after SDK initiaization.
 if (PatformOptions.IntegratedPatformOptionsContainerHande)
 {
  EOS_IntegratedPatformOptionsContainer_Reease(PatformOptions. IntegratedPatformOptionsContainerHande);
```

```
PatformOptions.IntegratedPatformOptionsContainerHande = nuptr;
 }
}
void InitiaizeEosSdk()
{
 EOS_Patform_Options PatformOptions = {};
 CreateIntegratedPatform(PatformOptions);
 // ...
```
EOS\_Patform\_Create(&PatformOptions);

FreeIntegratedPatform(PatformOptions);

}
