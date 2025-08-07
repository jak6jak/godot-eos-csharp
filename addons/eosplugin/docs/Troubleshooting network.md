| Overview | Account Services | Game Services | Get | j | jak6jak | Dev Portal |
|----------|------------------|---------------|-----|---|---------|------------|
|----------|------------------|---------------|-----|---|---------|------------|

Epic Developer [Resources](https://dev.epicgames.com/docs) EOS Game [Services](https://dev.epicgames.com/docs/game-services) Lobbies & [Sessions](https://dev.epicgames.com/docs/game-services/lobbies-and-sessions) Troubleshooting Network Connection Issues

## Troubleshooting Network Connection Issues

## Recommendations for troubleshooting network connection issues for lobbies and player-hosted sessions.

Sometimes a player's network is restrictive and does not allow all traffic through their firewall. To help overcome this restriction, we recommend that you use the EOS Peer-to-Peer (P2P) Interface for player-hosted sessions and lobbies. The EOS P2P interface uses the following methods to try to establish the peer-to-peer connection between players:

- Direct local network connection:
	- EOS P2P attempts to connect players to each other using the local IP address of each player's device. If the players are on the same subnet of the local network, or if the players are directly connected to the internet, this connection is likely to succeed. A direct local network connection has the least amount of latency. However, this type of connection is not common, since players are usually on different networks and behind networking devices (such as a router) that provide a firewall and a private IP subnet.
- Direct public internet connection:
	- If the EOS P2P Interface cannot establish a direct local network connection between players, EOS P2P communicates with a STUN (Session Traversal Utilities for NAT) server to determine the public IP address and port of each player's device. EOS P2P uses this information to attempt to connect both players to each other. A direct public internet connection is the most common type of peer-to-peer connection. It doesn't have the latency issues that exist on a relayed internet connection.
- Relayed internet connection:
	- If a player's firewall is too restrictive, the EOS P2P interface uses a secure relay to communicate data between players. Each player sends their data to a TURN (Traversal Using Relays around NAT) server, which then forwards that data to the other player's game client. Since a TURN server must receive data and then relay that data, it can introduce additional latency when it relays the data. The amount of latency depends on how close both players are to the relay server.

For more information on how the EOS P2P interface uses relays to establish the peer-to-peer connection between players, see the section on: Relay [Control](https://dev.epicgames.com/docs/en-US/game-services/p-2-p#relay-control) in the NATP2PInterfacepage.

**Note**: If you use the EOS P2P Interface, Epic Games handles the peer-to-peer connection for you, so you do not need to host your own relay servers. If you don't use the EOS P2P Interface and instead choose to handle your game's networking yourself, we still recommend that you implement STUN and or TURN for player-hosted sessions and lobbies yourself. However, due to the bandwidth costs and latency associated with TURN, we recommend you only use TURN as a last resort.
