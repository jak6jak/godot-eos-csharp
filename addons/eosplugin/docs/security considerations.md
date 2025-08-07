|                          | Overview |                   | Account Services | Game Services      | Get | j                       | jak6jak | Dev Portal |
|--------------------------|----------|-------------------|------------------|--------------------|-----|-------------------------|---------|------------|
| Epic Developer Resources | >        | EOS Game Services | >                | Lobbies & Sessions | >   | Security Considerations |         |            |

# Security Considerations

Security considerations for lobbies and sessions.

A player can use different methods to join a lobby or session. You can control which players can find the lobby or session and the methods they can use to find it by setting the permission level for the lobby or session. There are three different permission levels that you can set:

- **Public advertised**: Any player can search for and find the lobby or session.
- **Join via presence**: A friend can join their friend in the lobby or session through the Social Overlay. Also, any player or game client that knows the lobby ID or the session ID can join it. Note that lobbies that have the Join via presence permission level set do not appear in searches.
- **Invite only**: The lobby or session does not appear in searches. A player or game client can only join the lobby or session if an existing player invites them to join.

For more information, see the documentation:

- How to set permission levels for lobbies: [Permission](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/lobbies/modify-a-lobby#permission-levels) Levels in the LobbyInterfacepage.
- How to set permission levels for sessions: [Permission](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session#permission-levels) Levels in the SessionsInterfacepage.

There are types of data that you might expose for a lobby or session to allow players to join it. However, you can follow best practices to make sure that you don't expose too much data. For more information, see the sections below.

# Types of Data That You Might Expose

When you set the permission level for a lobby or session to public advertised, you might expose the following information:

- Player IP addresses (for peer-to-peer sessions only).
- Host IP address (dedicated servers for sessions only).
- User ID of the owner of the lobby or session.
- User ID of each member of the lobby or session.
- Any public attributes that the owner of the lobby or session has added.
- Any public lobby member attributes (lobbies only).

### Best Practices

Here are some best practices that you should follow for lobbies and sessions to ensure that you don't expose too much data:

Set the permission level so that the lobby or session is visible to the least number of players necessary. For example, if you want all players to be able to find your lobby or session and access the data for it, you should set the permission level to

#### 5/23/25, 8:50 PM Security Considerations | Epic Online Services Developer

public advertised. However, if you don't require that level of exposure, you should set the permission level to a level that provides less exposure, such as the invite only permission level.

- Don't add information to attributes with public visibility that you don't want players to be able to use as search criteria to find the lobby or session.
- If you set the permission level to Join via presence, make sure you don't include the lobby ID or session ID as an attribute with public visibility. If you expose the lobby ID or session ID, someone can use it to access the lobby data or session data.
- If your game doesn't allow other players to join the session after it has already started (also known as "join in progress"), make sure your server starts the session with [EOS\\_Sessions\\_StartSession](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-sessions-start-session) . This removes the session from search results while the game is in progress.
- If you have security concerns about exposing the host IP address, you can use [EOS\\_SessionModification\\_SetHostAddress](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-session-modification-set-host-address) to set the host address to something else. For example, you can set the host address to the P2P socket ID if you use the P2P Interface. For more information, see the section on: [Managing](https://dev.epicgames.com/docs/en-US/game-services/p-2-p#managing-p2p-connections) P2P [Connections](https://dev.epicgames.com/docs/en-US/game-services/p-2-p#managing-p2p-connections) in the NATP2PInterfacepage.

# More Information

See the documentation for more information:

- How to set permission levels for lobbies: [Permission](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/lobbies/modify-a-lobby#permission-levels) Levels in the ModifyaLobbypage.
- How to set permission levels for sessions: [Permission](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session#permission-levels) Levels in the ModifyaSessionpage.
- How to add attributes with public visibility to lobbies: Lobby and Lobby Member [Properties](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/lobbies/modify-a-lobby#lobby-and-lobby-member-properties) in the ModifyaLobbypage.
- How to add attributes with public visibility to sessions: Custom [Attributes](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session#custom-attributes) in the SessionsInterfacepage.
- Join via presence: Allow Friends to Join Via [Presence](https://dev.epicgames.com/docs/en-US/epic-account-services/social-overlay-overview/social-overlay-for-crossplay#allow-friends-to-join-via-presence) in the SocialOverlayforCrossplaypage.
- Join in progress: Modify a [Session](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/modify-a-session). See the function [EOS\\_SessionModification\\_SetJoinInProgressAowed](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-session-modification-set-join-in-progress-allowed) .
