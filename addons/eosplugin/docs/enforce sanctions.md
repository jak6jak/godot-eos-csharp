| Overview                 | Account Services                                         | Game Services                                             | Get                                                       | j                                                        | jak6jak | Dev Portal |
|--------------------------|----------------------------------------------------------|-----------------------------------------------------------|-----------------------------------------------------------|----------------------------------------------------------|---------|------------|
| Epic Developer Resources | <span style="font-size: 1em;">➔</span> EOS Game Services | <span style="font-size: 1em;">➔</span> Lobbies & Sessions | <span style="font-size: 1em;">➔</span> Sessions Interface | <span style="font-size: 1em;">➔</span> Enforce Sanctions |         |            |

## Enforce Sanctions

Overview of the Session Matchmaking Sample.

If you are using EOS Sanctions, then you can enforce sanctions when players attempt to join or register with the session. See the [Sanctions](https://dev.epicgames.com/docs/en-US/game-services/sanctions-interface) Interface documentation for more information on sanctions.

Sanctioned players may neither join nor register with a session that has sanctions enabled. This includes sanctioned sessions that the player creates themselves; they are still not able to join those sessions if they are a sanctioned player.

Sanctions are enforced through the Sessions Interface. Sanctions enforcement is enabled at session creation based on the value of the bSanctionsEnabed member in the EOS\_Sessions\_CreateSessionModificationOptions structure. If bSanctionsEnabed is true, then the created session enforces sanctions. For more information, see the section on: [Create](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/sessions-intro#create-a-session) a [Session](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/sessions-intro#create-a-session) in the SessionsIntroductionpage.

If a player has a RESTRICT\_GAME\_ACCESS sanction, they are automatically kicked from multiplayer sessions in games protected by Anti-Cheat. See the documentation on [Anti-cheat](https://dev.epicgames.com/docs/en-US/game-services/anti-cheat) for more information.

The [EOS\\_Sessions\\_JoinSession](https://dev.epicgames.com/docs/en-US/api-ref/functions/eos-sessions-join-session) function does not join a player with a RESTRICT\_MATCHMAKING sanction, and it returns the error EOS\_Sessions\_PayerSanctioned on any attempt to do so. However, the function EOS\_Sessions\_RegisterPayers supports multiple players, and, unlike EOS\_Sessions\_JoinSession , it does not return an error when one or more sanctioned players attempts to register with a session that has sanctions enabled. Instead, you must check the response object to ensure that all players are registered. If the session has sanctions enabled, any sanctioned players who failed to register are listed in the SanctionedPayers member of the callback result struct EOS\_Sessions\_RegisterPayersCabackInfo . For more information, see the section on: [Register](https://dev.epicgames.com/docs/en-US/game-services/lobbies-and-sessions/sessions/sessions-intro#register-a-player) a Player in the SessionsIntroduction page.
