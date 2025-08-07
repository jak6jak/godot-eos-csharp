|                          | Overview          | Account Services | Game Services      | Get                | j | jak6jak               | Dev Portal |
|--------------------------|-------------------|------------------|--------------------|--------------------|---|-----------------------|------------|
| Epic Developer Resources | EOS Game Services |                  | Lobbies & Sessions | Sessions Interface |   | Sessions Introduction |            |

# Sessions Introduction

Interface to handle session-based matchmaking.

**Epic Online Services** (EOS) gives players the ability to host, find, and interact with online gaming sessions through the **Sessions Interface**. A session can be short, like filling a certain number of player slots before starting a game, then disbanding after the game ends, or it could be longer, like keeping track of a game that cycles through matches on multiple maps or levels. The Sessions Interface also manages game-specific data that supports the back-end service searching and matchmaking functionality. For more information on the considerations you should take for matchmaking, see the documentation: [Security](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/security-considerations) [Considerations](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/security-considerations) section below. This document covers some of the definitions and headers for the Sessions Interface. See the Sessions Interface API [Reference](https://dev.epicgames.com/docs/en-US/api-ref/interfaces/sessions) documentation for a complete list.

Integrate the Sessions Interface into your game (or other software application) to use the features of the EOS Sessions service. To use the Sessions Interface, acquire an EOS\_HSessions handle through the Platform Interface (documentation: [Platform](https://dev.epicgames.com/docs/en-US/game-services/eos-platform-interface) [Interface](https://dev.epicgames.com/docs/en-US/game-services/eos-platform-interface)) function, EOS\_Patform\_GetSessionsInterface . All Sessions Interface functions require this handle as their first parameter. You must ensure that the EOS\_HPatform handle is ticking for callbacks to trigger when requests are completed. See the EOS [SDK](https://dev.epicgames.com/docs/en-US/epic-online-services/eos-get-started/working-with-the-eos-sdk/eossdkc-sharp-getting-started#ticking) in C# documentation for more information on ticking.

## Session Lifecycle

A session's life cycle is not rigid; for example, sessions can modify their data at any time, and can start and end multiple matches. A session generally includes these events:

- A user or a dedicated server creates the session and sets up the session's initial state.
- The owner might invite users to join.
- Other users join and leave the session.
	- The owner registers users when they join the session and unregisters users when they leave the session.
- The session owner can update data specific to the state of the game.
	- Users can play multiple matches without leaving or destroying the session. Your game's logic determines the lifetime of a session.
- Finally, the owner destroys the session.

# Active Sessions

Active sessions are at the core of everything the Sessions Interface does. An application can have multiple active sessions at the same time, each identified by a unique, local name. For example, there might be a session called "Party" with the local player's friends, keeping them together as they play matches against other teams, and another called "Game" that includes some or all of those friends as well as other players in the match currently in progress. Each session has its own EOS\_HActiveSession handle on each participating player's system. An active session forms on a player's machine whenever that player creates a session, or joins a session found in an online search or through an invitation. Since active sessions exist locally, the local application must destroy them when they are no longer needed. If a host fails to do this, the back-end service server will delay destruction of the session, which can lead to other players falsely discovering sessions in their online searches.

To get a copy of the high-level information (type EOS\_ActiveSession\_Info ), for an active session, including its name, the ID of the local user who created or joined it, its [current](https://dev.epicgames.com/docs/en-US/api-ref/enums/eos-e-online-session-state) state, a reference to the session details (a const pointer to EOS\_SessionDetais\_Info ), and any user-defined data attributes, you must first get its active session handle (type EOS\_HActiveSession ) by calling the local function EOS\_Sessions\_CopyActiveSessionHande with an EOS\_Sessions\_CopyActiveSessionHandeOptions initialized as follows:

| Property    | Value                                                      |
|-------------|------------------------------------------------------------|
| ApiVersion  | EOS_SESSIONS_COPYACTIVESESSIONHANDLE_API_LATEST            |
| SessionName | Name of the session for which to retrieve a session handle |

With this handle, you can call EOS\_ActiveSession\_CopyInfo . You will also need to initialize and pass in an EOS\_ActiveSession\_CopyInfoOptions as follows:

| Property   | Value                                 |
|------------|---------------------------------------|
| ApiVersion | EOS_ACTIVESESSION_COPYINFO_API_LATEST |

This function also runs locally and, on success, makes a copy of the session's EOS\_ActiveSession\_Info data. You are responsible for releasing the copy with EOS\_ActiveSession\_Info\_Reease when you no longer need it.

## Create a Session

Creating a session is a three-step process:.

- Set the initial state and settings for the session locally.
- Make any additional modifications to the session that you require.
- Complete the Setup.

### Set the Initial State and Settings

First, call the EOS\_Sessions\_CreateSessionModification function to set the initial state and settings for the session locally. You must pass in an EOS\_Sessions\_CreateSessionModificationOptions structure. See the [EOS\\_Sessions\\_CreateSessionsModificationOptions](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-sessions-create-session-modification-options) API reference documentation for details.

If the EOS\_Sessions\_CreateSessionModification call succeeds, it will return EOS\_Success and the default EOS\_HSessionModification you provided will contain a valid handle.

### Make Additional Modifications

Second, continue to modify the session's initial setup (see the documentation: Modify a [Session\)](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session) until you have made all the modifications you need.

### Complete the Setup

After you have finished setting up the session, you can complete the creation process by calling [EOS\\_Sessions\\_UpdateSession](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-sessions-update-session) with an [EOS\\_Sessions\\_UpdateSessionOptions](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-sessions-update-session-options) structure initialized as follows:

| Property                  | Value                                                                                   |
|---------------------------|-----------------------------------------------------------------------------------------|
| ApiVersion                | EOS_SESSIONS_UPDATESESSION_API_LATEST                                                   |
| SessionModificationHandle | The handle ( EOS_HSessionModification ) for the session you want to create<br>or update |

EOS\_Sessions\_UpdateSession is asynchronous, and will call your delegate (of type

EOS\_Sessions\_OnUpdateSessionCaback ) upon completion with an EOS\_Sessions\_UpdateSessionCabackInfo data structure. On success, the local name that you provided will be paired with a searchable, unique ID string from the server.

## Invite Users to a Session

<span id="page-2-0"></span>To invite another user to join an active session, a [registered](#page-5-0) member of the session can call EOS\_Sessions\_SendInvite with an EOS\_Sessions\_SendInviteOptions structure containing the following data:

| Property     | Value                                                  |
|--------------|--------------------------------------------------------|
| ApiVersion   | EOS_SESSIONS_SENDINVITE_API_LATEST                     |
| SessionName  | The name of the session to which the player is invited |
| LocalUserId  | The local user sending the invitation                  |
| TargetUserId | The remote user being invited                          |

Once the server has processed the invitation request, your callback, of type EOS\_Sessions\_OnSendInviteCaback , will run with an EOS\_Sessions\_SendInviteCabackInfo structure containing a result code. This result indicates success if there was no error in the process of sending the invitation; success does not mean that the remote user has accepted, or even seen, the invitation.

For invite functionality with the Epic Games Launcher, be sure to map your deployments to your [artifacts](https://dev.epicgames.com/docs/en-US/epic-games-store/services/launcher-invites) as well.

The remote user will receive notification of the invitation when it arrives, and the payload will provide the user ID that has been invited as well as the ID of the invitation itself. Upon receipt, use EOS\_Sessions\_CopySessionHandeByInviteId to retrieve the EOS\_HSessionDetais handle from the invitation. You can use this handle to gain access to the [session](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/get-information-about-a-session#session-details) details data for the associated session, or to accept or reject the invitation. Once you are finished with the handle, call EOS\_SessionDetais\_Reease to release it.

In order to receive this notification, you must register a callback with EOS\_Sessions\_AddNotifySessionInviteReceived . You only need to do this once, typically at startup, after which your callback will run as each invitation is received. When you no longer need notification, call EOS\_Sessions\_RemoveNotifySessionInviteReceived to remove your callback.

### Accept an Invitation

The Sessions Interface does not feature a dedicated function for accepting an invitation. You can [join](#page-4-0) the session using the standard method with the EOS\_HSessionDetais handle that you retrieved from the invitation. To request a list of all pending invitations, call EOS\_Sessions\_QueryInvites with an EOS\_Sessions\_QueryInvitesOptions data structure initialized as follows:

| ApiVersion  | EOS_SESSIONS_QUERYINVITES_API_LATEST                 |
|-------------|------------------------------------------------------|
| LocalUserId | The local user whose invitations are being requested |

This operation is asynchronous. When it finishes, it will call your callback function, of type

EOS\_Sessions\_OnQueryInvitesCaback , with an EOS\_Sessions\_QueryInvitesCabackInfo data structure. On success, EOS will have all of the user's pending invitations cached locally. You can use EOS\_Sessions\_GetInviteCount to determine the number of invitations in the cache. Pass in an EOS\_Sessions\_GetInviteCountOptions structure with the following information:

| ApiVersion  | EOS_SESSIONS_GETINVITECOUNT_API_LATEST    |
|-------------|-------------------------------------------|
| LocalUserId | The local user who has cached invitations |

This function runs locally and will return a uint32\_t that represents the number of invitations currently in the cache. To get the ID of any cached invitation, call EOS\_Sessions\_GetInviteIdByIndex with an EOS\_Sessions\_GetInviteIdByIndexOptions containing the following information:

| ApiVersion  | EOS_SESSIONS_GETINVITEIDBYINDEX_API_LATEST                     |
|-------------|----------------------------------------------------------------|
| LocalUserId | The local user who has cached invitations                      |
| Index       | The cache index of the invitation whose ID we want to retrieve |

If EOS\_Sessions\_GetInviteIdByIndex returns EOS\_Success , the output parameters you passed to it contain the invitation's ID as a null-terminated character string and the length of that string.

The max length of an invitation's ID string is EOS\_SESSIONS\_INVITEID\_MAX\_LENGTH (currently 64).

As described [above,](#page-2-0) the EOS\_Sessions\_CopySessionHandeByInviteId function will provide an EOS\_HSession handle which gives you access to the [session](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/get-information-about-a-session#session-details) details data for the associated session. You can then choose to accept the invitation by [joining](#page-4-0) the session, ignore the invitation, or [reject](#page-4-1) it. Once you are finished with the EOS\_HSession handle, call EOS\_SessionDetais\_Reease to release it.

### <span id="page-4-1"></span>Reject an Invitation

To reject an invitation, call EOS\_Sessions\_RejectInvite with an EOS\_Sessions\_RejectInviteOptions initialized as follows:

Upon completion, you will receive a call to your EOS\_Sessions\_OnRejectInviteCaback callback function with an EOS\_Sessions\_RejectInviteCabackInfo data structure indicating success or failure. Successfully rejecting an invitation permanently deletes it from the system.

## Join a Session

You can join an existing session if you have a valid EOS\_HSessionDetais handle to it by calling EOS\_Sessions\_JoinSession and providing an [EOS\\_Sessions\\_JoinSessionOptions](https://dev.epicgames.com/docs/en-US/api-ref/structs/eos-sessions-join-session-options) structure with the following information:

<span id="page-4-0"></span>

| Property         | Value                                                                                                                                                |
|------------------|------------------------------------------------------------------------------------------------------------------------------------------------------|
| ApiVersion       | EOS_SESSIONS_JOINSESSION_API_LATEST                                                                                                                  |
| SessionName      | The unique name that the local system will use to refer to the session                                                                               |
| SessionHandle    | The EOS_HSessionDetails handle for the session you want to join                                                                                      |
| LocalUserId      | The local user joining the session                                                                                                                   |
| bPresenceEnabled | Whether or not this session should be the one associated with the local user's presence<br>information (see the Presence Interface for more details) |

When the operation finishes, your callback function, of type EOS\_Sessions\_OnJoinSessionCaback , will receive an EOS\_Sessions\_JoinSessionCabackInfo indicating success or failure. If the operation succeeds, EOS will create an active

session on the joining client's system. As this new session is locally-owned, the joining user is responsible for [destroying](#page-7-0) it once it is no longer needed.

### <span id="page-5-0"></span>Register a Player

When a player joins the session, the session owner is responsible for registering the player with the session. This keeps the backend service aware of the number of players so that it may stop advertising the session publicly when it is full. EOS accepts registration of multiple players at once through the EOS\_Sessions\_RegisterPayers function. Call this function from the owning client with an EOS\_Sessions\_RegisterPayersOptions structure containing the following data:

| Property               | Value                                       |
|------------------------|---------------------------------------------|
| ApiVersion             | EOS_SESSIONS_REGISTERPLAYERS_API_LATEST     |
| SessionName            | The local name of the session               |
| PlayersToRegister      | An array of IDs for the joining players     |
| PlayersToRegisterCount | The number of elements in PlayersToRegister |

On completion, your callback of type EOS\_Sessions\_OnRegisterPayersCaback will run with an EOS\_Sessions\_RegisterPayersCabackInfo parameter that contains the following data:

| Property                 | Value                                                       |
|--------------------------|-------------------------------------------------------------|
| ResultCode               | Result code indicating success or failure                   |
| Registered Players       | A list of players successfully registered                   |
| Registered Players Count | The number of registered players                            |
| Sanctioned Players       | A list of players that were not registered due to sanctions |
| Sanctioned Players Count | The number of players not registered due to sanctions       |

If the call succeeds, the newly-registered players will receive access to some session management functionality, such as the ability to invite other players to the session. Note that no errors are returned when one or more sanctioned players are denied registration; theSanctionedPayerslistmustbecheckedmanually. See Enforcing [Sanctions](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/enforce-sanctions) for more details. EOS does not provide notification when a player joins a session, so you must notify the owner when you join, or provide the owner with a way to detect when a player has successfully joined the session.

After a user registers with a session configured with the EOS\_OSPF\_PubicAdvertised permission level, other users can find the session with EOS\_SessionSearch\_SetTargetUserId . For more information on the permission levels that you can set for a session, see the section on: [Permission](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session#permission-levels) Levels in the page ModifyaSession.

This information is publicly visible, including to applications outside of the EOS ecosystem. These applications could use session data to discover the IP address of the server or P2P host, and potentially attempt to cause disruption.

## Leave a Session

The Sessions Interface does not feature a dedicated function for leaving a session. To leave a session, [destroy](#page-7-0) your local session through the standard method, using its local name.

### Unregister a Player

When a player leaves the session, the session's owner is responsible for unregistering the player. This enables the server to free up player slots so that future players can join. EOS accepts unregistration of multiple players at once through the EOS\_Sessions\_UnregisterPayers function. Call this function from the owning client with an EOS\_Sessions\_UnregisterPayersOptions structure containing the following data:

| Property                 | Value                                         |
|--------------------------|-----------------------------------------------|
| ApiVersion               | EOS_SESSIONS_UNREGISTERPLAYERS_API_LATEST     |
| SessionName              | The local name of the session                 |
| PlayersToUnregister      | An array of IDs for the departing players     |
| PlayersToUnregisterCount | The number of elements in PlayersToUnregister |

On completion, your callback of type EOS\_Sessions\_OnUnregisterPayersCaback will run with an

EOS\_Sessions\_UnregisterPayersCabackInfo parameter that indicates success or failure. If the call succeeds, the back-end service will revoke the session management access that those players received when they were originally registered. For example, players who are not registered with the session cannot invite others to join it. EOS does not provide notification of a player leaving a session, so you must notify the owner when you leave a session, or provide the owner with a way to detect that a player has left the game or disconnected.

Host migration is not supported for sessions. If the owner of the session leaves or they lose their connection to the network, the session is orphaned, and no one else can manage the session on the back-end.

## Start and End Play

A player can declare that a match has started or ended for a local active session. If that session maps to a session on the backend service that the local player owns, the back-end version will also start or end play. While playing, the back-end service will automatically reject attempts to join a session if that session has **Join in Progress** disabled (see the documentation: [Modify](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session) a [Session\)](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session). Although starting a session typically implies that a match has begun, the specific usage of this functionality is up to the game's developers.

To start play, call EOS\_Sessions\_StartSession with an EOS\_Sessions\_StartSessionOptions initialized as follows:

| Property    | Value                                |
|-------------|--------------------------------------|
| ApiVersion  | EOS_SESSIONS_STARTSESSION_API_LATEST |
| SessionName | The local name of the session        |
|             |                                      |

When the operation completes, your callback, of type EOS\_Sessions\_OnStartSessionCaback , will run with an EOS\_Sessions\_StartSessionCabackInfo data structure indicating success or failure. If the operation succeeds, the session is considered "in progress" until you end play by calling EOS\_Sessions\_EndSession with an EOS\_Sessions\_EndSessionOptions containing the following information:

Upon completion, your EOS\_Sessions\_OnEndSessionCaback function will receive a call with an

EOS\_Sessions\_EndSessionCabackInfo data structure indicating success or failure. On success, the session is no longer considered "in progress" and will once again permit players to join. Ending a session does not remove players or destroy the session; it effectively returns to its pre-start state and remains usable, including the ability to start play again. If you intend to destroy the session, you do not need to call EOS\_Sessions\_EndSession first.

# Destroy a Session

When you no longer need a session, you must destroy it using EOS\_Sessions\_DestroySession . Call this function with an EOS\_Sessions\_DestroySessionOptions data structure containing the following information:

<span id="page-7-0"></span>

| ApiVersion  | EOS_SESSIONS_DESTROYSESSION_API_LATEST |
|-------------|----------------------------------------|
| SessionName | The name of the session to destroy     |

When the destruction operation completes, you will receive a callback of type EOS\_Sessions\_OnDestroySessionCaback with an EOS\_Sessions\_DestroySessionCabackInfo data structure. Upon success, the session will cease to exist and its name will be available for reuse. However, due to the asynchronous nature of this system, it is possible for a player to make a request after you have called EOS\_Sessions\_DestroySession but before the back-end service has destroyed the session. In this case, you may receive requests to join the session after having started the destruction operation. It is important to reject these players, or shut down the network to prevent them from joining the defunct session.

## Mirror Session Management on Remote Clients

Through the Sessions Interface, the session owner (the user who created the session) can manage the session's state as it exists on the back-end service.

In general, only the owner of the session can make modifications to session data on the back-end service. However, remote clients who [join](#page-4-0) a session will have their own local view of the session, and this view does not automatically receive data updates about the back-end version. It is not required, but can be beneficial for remote clients to keep their local view in sync with the back-end session by mirroring the following function calls:

- EOS\_Sessions\_StartSession
- EOS\_Sessions\_EndSession
- EOS\_Sessions\_RegisterPayers
- EOS\_Sessions\_UnregisterPayers

These functions will modify the local state of the session on remote (non-owner) clients without affecting the back-end service's version.

## Usage Limitations

Users can host, find, and interact with online gaming sessions through the Sessions Interface. A session can be short, like filling a certain number of player slots before starting a game, then disbanding after the game ends. Or it can be longer, like keeping track of a game that cycles through matches on multiple maps or levels. The Sessions Interface also manages game-specific data that supports the back-end service searching and matchmaking functionality.

For general information about throttling, usage quotas, and best practices, s ee the section on service usage limitations in the EOS SDK - [Conventions](https://dev.epicgames.com/docs/en-US/epic-online-services/eos-get-started/working-with-the-eos-sdk/conventions-and-limitations#service-usage-limitations) and Limitations page.

The following general limitations apply to sessions:

| Feature               | Limitation       |
|-----------------------|------------------|
| Concurrent players    | 1000 per session |
| Session attributes    | 100 per session  |
| Attribute name length | 1000 characters  |

User interaction with the Sessions Interface must respect the following limitations:

| Feature          | Limitation             | Per-Deployment Limitation       |
|------------------|------------------------|---------------------------------|
| Create a session | 30 requests per minute | 30 request per 1 CCU per minute |
| Delete a session | 30 requests per minute | 30 request per 1 CCU per minute |

| Feature                 | Limitation              | Per-Deployment Limitation       |
|-------------------------|-------------------------|---------------------------------|
| Update a session        | 30 requests per minute  | 30 request per 1 CCU per minute |
| Add or remove players   | 100 requests per minute | 30 request per 1 CCU per minute |
| Start or stop a session | 30 requests per minute  | 30 request per 1 CCU per minute |
| Invite a user           | 100 requests per minute | 30 request per 1 CCU per minute |
| Filter sessions         | 30 requests per minute  | 30 request per 1 CCU per minute |

Additionally, there are per-user rate limitations. Consider the following limitations to avoid throttling:

| Feature                                           | User Limit |
|---------------------------------------------------|------------|
| Amount of sessions a single user can join at once | 16         |

# FAQ

### **Q: Why can't I find the server that I just created?**

A: Indexing takes two seconds to refresh, so you might not be able to find your server immediately.

### **Q: Why can't I see the same list of servers as my co-worker or friend?**

A: Matchmaking has a two-second delay to ensure that no two people get the same list of servers at the same time. This deliberate jittering avoids race conditions that can affect the efficiency of the servers.

### **Q: I want to see the sessions that are set up in my game. How can I see all the sessions in my game?**

A: You can find the list of sessions in your game in the Developer Portal. Sign in to the Developer Portal (at [dev.epicgames.com/portal\)](https://dev.epicgames.com/portal/), and select your product under the **Products** section in the left sidebar. Go to **Game Services** > **MULTIPLAYER** > **Matchmaking**.
