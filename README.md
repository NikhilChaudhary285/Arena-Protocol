\# Arena Protocol



\## Setup \& Run Instructions



\### Requirements

\- Unity 2022.3 LTS

\- Unity Netcode for GameObjects (installed via Package Manager)



\### How to Open

1\. Clone/unzip the project

2\. Open Unity Hub → Add → select the project folder

3\. Open ArenaScene from Assets/Scenes/



\### How to Run (Multiplayer)

1\. Build the project: File → Build Settings → Build

2\. Run TWO instances of the build (or one build + Unity editor)

3\. First instance: click \*\*"Host"\*\* — starts the game as host+player

4\. Second instance: click \*\*"Join"\*\* — connects as client player

5\. Both players now appear in the arena



\---



\## Technical Approach



\### Networking Model: Host-Client (NGO)

\- Used \*\*Unity Netcode for GameObjects (NGO)\*\* with Unity Transport

\- \*\*Host\*\* acts as both server and player — authoritative on all game state

\- \*\*Client\*\* sends inputs, receives state updates

\- `NetworkVariable<T>` used for health, score — auto-synced to all clients

\- \*\*Server-side AI\*\*: All enemy logic runs exclusively on the server; positions replicated via `NetworkTransform`



\### Architecture

\- \*\*Modular Ability System\*\*: `BaseAbility` abstract class allows easy addition of new ability types

\- \*\*State Machine AI\*\*: Clean Patrol/Chase/Attack state machine on server

\- \*\*Score System\*\*: Centralized `ScoreManager` with `NetworkVariable<int>`

\- \*\*Session Persistence\*\*: `SessionManager` listens to connect/disconnect callbacks and saves/restores player state



\---



\## Engineering Tradeoffs



| Decision | Tradeoff |

|---|---|

| Host-Client over Dedicated Server | Simpler setup for 2-player; host has slight advantage (no latency) |

| Server-authoritative AI | Eliminates desync; client sees slight lag on enemy movement |

| NetworkVariable for health/score | Easy sync but limited to server writes; good for this scale |

| Primitives-only art | Keeps project clean and focused; not visually rich |

| Client-side prediction skipped | Simpler code; may feel slightly laggy on high-latency connections |



\---



\## Known Issues \& Limitations

\- No dedicated lobby system; both players must manually start host/join

\- Ping display is approximate; full RTT measurement needs deeper transport integration

\- No respawn system after player death (player stays at 0 health)

\- Projectile may occasionally pass through enemies at high speed (no continuous collision)

\- Reconnect works within the same session; full server crash is not recoverable

