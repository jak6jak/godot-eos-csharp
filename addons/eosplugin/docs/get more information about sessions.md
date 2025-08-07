| Overview | Account Services | Game Services | Get | j | jak6jak | Dev Portal |
|----------|------------------|---------------|-----|---|---------|------------|
|          |                  |               |     |   |         |            |

Epic Developer [Resources](https://dev.epicgames.com/docs) EOS Game [Services](https://dev.epicgames.com/docs/game-services) Lobbies & [Sessions](https://dev.epicgames.com/docs/game-services/lobbies-and-sessions) Sessions [Interface](https://dev.epicgames.com/docs/game-services/lobbies-and-sessions/sessions) Get Information About a Session

# Get Information About a Session

#### How to access session information.

You can access both session details and player details from the active session. For more information on active sessions, see the section on: Active [sessions](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/sessions-intro#active-sessions) in the SessionsIntroductionpage.

# Session Details

Active sessions that you create locally or discover through searches, invitations, or other users' [presence](https://dev.epicgames.com/docs/en-US/epic-account-services/eos-presence-interface) data store an internal data structure called EOS\_SessionDetais\_Info , which contains basic details about the session:

- Session ID
- Host address
- Number of open slots in the session

The structure also contains a pointer to another structure, EOS\_SessionDetais\_Settings , which provides more detail about the state of a session, including:

- Top-level filtering criteria, called the **Bucket ID**, which is specific to your game; often formatted like "GameMode:Region:MapName"
- Total number of connections allowed on the session
- Join-in-progress settings
- Privacy setting

### Access Session Details

If you have an EOS\_ActiveSession\_Info data structure, its SessionDetais variable gives you access to the EOS\_SessionDetais\_Info for that session. If not, you can use an EOS\_HSessionDetais handle to call EOS\_SessionDetais\_CopyInfo to acquire a copy of the EOS\_SessionDetais\_Info data. Call EOS\_SessionDetais\_CopyInfo with an EOS\_SessionDetais\_CopyInfoOptions structure containing the following information:

| Property   | Value                                  |
|------------|----------------------------------------|
| ApiVersion | EOS_SESSIONDETAILS_COPYINFO_API_LATEST |

On success, this will return a copy of the session's EOS\_SessionDetais\_Info , which contains the session's ID, the address of the host, and the number of open slots in the session. When you no longer need this information, call

EOS\_SessionDetais\_Info\_Reease to free it.

## Player Details

You can retrieve the following player data from the active session:

- The total number of registered players.
- The product user ID (PUID) of each registered player.

### Access Player Details

#### **Total Number of Registered Players**

You can call EOS\_ActiveSession\_GetRegisteredPayerCount to get the total number of registered players associated with the active session. To do this, call EOS\_ActiveSession\_GetRegisteredPayerCount with an ActiveSessionGetRegisteredPayerCountOptions structure that contains the following information:

| Property   | Value                                                 |
|------------|-------------------------------------------------------|
| ApiVersion | EOS_ACTIVESESSION_GETREGISTEREDPLAYERCOUNT_API_LATEST |

#### **Product User ID of Each Registered Player**

You can iterate through the registered players for the active session to get the product user ID (PUID) of each registered player. To do this, call EOS\_ActiveSession\_GetRegisteredPayerByIndex with an

ActiveSessionGetRegisteredPayerByIndexOptions structure that contains the following information:

| Property    | Value                                                   |
|-------------|---------------------------------------------------------|
| ApiVersion  | EOS_ACTIVESESSION_GETREGISTEREDPLAYERBYINDEX_API_LATEST |
| PlayerIndex | Index of the registered player to retrieve.             |

On success, this returns the product user ID (PUID) of the player at the specified index for the active session.
