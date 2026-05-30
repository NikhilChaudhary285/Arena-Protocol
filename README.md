# Arena Protocol

> **2-Player Co-op Multiplayer Survival Arena** — Built with Unity 2022 LTS & Netcode for GameObjects

---

## Demo Video

[![Arena Protocol Demo](https://img.shields.io/badge/Watch%20Demo-Google%20Drive-blue?style=for-the-badge&logo=google-drive)](https://drive.google.com/file/d/1G69NQzM6XEuHGmR2MY8vK8lOAvrgYZQ4/view?usp=sharing)

**[Click here to watch the full gameplay demo](https://drive.google.com/file/d/1G69NQzM6XEuHGmR2MY8vK8lOAvrgYZQ4/view?usp=sharing)**

---

## GDD Requirements Checklist

All requirements from the Game Design Document have been implemented and verified:

| Requirement | Status |
|---|---|
| WASD / Arrow key movement — snappy, responsive | ✅ Done |
| Movement synced across both clients with no jitter | ✅ Done |
| Dash ability (Q) with independent cooldown | ✅ Done |
| Projectile ability (E) — fires toward cursor, damages enemies | ✅ Done |
| Heal ability (R) — restores player health | ✅ Done |
| Modular ability system (BaseAbility abstract class) | ✅ Done |
| Visual cooldown overlays + countdown timers for all 3 abilities | ✅ Done |
| Enemy AI — Patrol → Chase → Attack → Return to Patrol | ✅ Done |
| Enemy positions and actions synced across both clients | ✅ Done |
| Wave-based enemy spawning | ✅ Done |
| Energy orbs — collection, score increase, visual effect | ✅ Done |
| Orb respawn after set delay | ✅ Done |
| Orb collection and respawn timer synced across both clients | ✅ Done |
| Health bar — own player health display | ✅ Done |
| Health bar — partner player health display (optional) | ✅ Done |
| Score display — prominent, real-time, synced | ✅ Done |
| Latency / ping indicator (ms) | ✅ Done |
| Graceful client reconnection to active session | ✅ Done |
| Health restored on reconnect | ✅ Done |
| Score preserved on reconnect | ✅ Done |
| Ability cooldown states restored on reconnect | ✅ Done |
| Unity 2022 LTS | ✅ Done |
| Primitive meshes only — no paid assets | ✅ Done |
| README.md with all 4 required sections | ✅ Done |

---

## 1. Setup & Run Instructions

### Requirements

- Unity Hub installed
- Unity **2022.3.x LTS** (any patch release)
- Windows PC (x86_64)
- No additional software required — all packages install automatically

### How to Open the Project

1. Download and unzip `ArenaProtocol.zip`
2. Open **Unity Hub**
3. Click **"Add"** → **"Add project from disk"**
4. Navigate to the unzipped `ArenaProtocol` folder → click **"Select Folder"**
5. Unity 2022.3 LTS will appear in the list — click **"Open"**
6. Wait for Unity to import assets and install packages automatically (2–5 minutes first time)
7. In the Project window → navigate to `Assets/Scenes/` → double-click **ArenaScene**
8. Verify no compile errors in the Console window

### How to Run — Two Instances (Multiplayer)

**Option A — Editor + Build (Recommended for testing)**

1. Build the project: `File → Build Settings → Add Open Scenes → Build`
2. Save build to any folder (e.g. `Builds/ArenaProtocol.exe`)
3. Run the built `.exe` → click **"Host"** — this starts the server + Player 1
4. Press **Play** in Unity Editor → click **"Join"** — this connects as Player 2
5. Both players appear in the arena — game starts immediately

**Option B — Two Builds**

1. Build the project twice into separate folders
2. Run both `.exe` files
3. First instance: click **"Host"**
4. Second instance: click **"Join"**

### Controls

| Key | Action |
|---|---|
| W A S D / Arrow Keys | Move player |
| Q | Dash (dodge/close distance) |
| E | Fire projectile toward cursor |
| R | Heal self |
| Mouse | Aim projectile direction |

### Testing Reconnection

1. Start Host + Join with client
2. Play for a minute — take damage, use abilities
3. Close the client window (Alt+F4)
4. Reopen client `.exe` → click **"Join"**
5. Client rejoins with health, score, and cooldowns restored

---

## 2. Technical Approach

### Networking Model — Host-Client (NGO)

The game uses **Unity Netcode for GameObjects (NGO) v1.15.1** with **Unity Transport** as the underlying protocol.

- The **Host** acts as both server and first player. All authoritative game logic runs on the host.
- The **Client** sends input to the server, receives state updates, and renders results.
- **NetworkVariable\<T\>** is used for all replicated state: health, score, cooldown timers. Changes propagate automatically to all connected clients.
- **NetworkTransform** handles position synchronization for players and enemies with built-in interpolation.
- **ServerRpc** is used for client-to-server actions (firing projectiles, ability activation).
- **ClientRpc** is used for server-to-all-clients broadcasts (projectile velocity, orb collection).

### Architecture Overview

**Player System**
- `PlayerController` — CharacterController-based movement, ability input handling, owner-only execution
- `PlayerHealth` — NetworkVariable health with OnValueChanged UI callbacks
- `PlayerSpawnManager` — Coroutine-based spawn point assignment using `NetworkTransform.Teleport()` via PlayerController

**Ability System**
- `BaseAbility` (abstract, extends NetworkBehaviour) — networked cooldown via `NetworkVariable<float>`, server-authoritative tick, `TryActivate()` pattern
- `DashAbility`, `ProjectileAbility`, `HealAbility` — concrete implementations
- Fully modular — new abilities extend `BaseAbility` and implement `Activate()`

**Enemy AI**
- `EnemyAI` — server-only state machine (Patrol / Chase / Attack) using `Vector3.MoveTowards`
- `EnemySpawner` — wave-based spawning via server coroutine
- All AI logic runs exclusively on server; positions replicated to clients via `NetworkTransform`

**Resource & Scoring**
- `EnergyOrb` — scene-placed NetworkObjects, trigger-based collection, `ClientRpc` for hide/show sync
- `ScoreManager` — `NetworkVariable<int>` team score, auto-synced to both clients

**Session Management**
- `SessionManager` — static dictionary keyed by slot (not clientId) to survive clientId changes on reconnect
- Saves health + cooldown state on disconnect, restores after player object spawns on rejoin
- Handles NGO's clientId increment behaviour (clientId changes each reconnect session)

**UI**
- `AbilityUI` — per-frame cooldown overlay fill + countdown text
- `PingDisplay` — `UnityTransport.GetCurrentRtt()` with smoothed display
- `PlayerHealth` — runtime `GameObject.Find()` wiring (scene object → spawned prefab bridge)

### Git Commit History

| Commit | Description |
|---|---|
| `629d92f` | feat: implement 2-player co-op arena with NGO networking, ability system, enemy AI, orb collection, UI, and session reconnection with state persistence |
| `b866e00` | Refactored Unity NGO multiplayer architecture |
| `c76e46884` | Implemented polished multiplayer HUD/UI icons and enhanced gameplay UI/UX flow |
| `5a218ed` | Implemented synchronized multiplayer partner health UI with dynamic visibility |
| `c9cbda6` | Implemented NGO multiplayer fixes: server-authoritative player movement, reliable network spawning |
| `3a4d4b4` | fix: resolve multiplayer movement authority, CharacterController sync issues |
| `eb13da1` | feat: finalize Arena Protocol with visual polish, camera follow system, multiplayer build setup |
| `fcf4ff5` | feat: implement energy orb collection, synchronized score/UI systems, cooldown indicators |
| `8082ad2` | feat: implement networked projectile combat, enemy AI state machine, health system |
| `75b82a2` | feat: implement modular ability system, networked projectile combat, synchronized health |
| `1d4a9ac` | feat: implement networked player prefab, multiplayer movement, connection UI |
| `47685086` | feat: create ArenaScene with arena environment, top-down camera setup, NGO configuration |
| `88278e3` | setup: initialize ArenaProtocol Unity 2022 LTS project with NGO packages |
| `edbe0cb` | project: initialize Arena Protocol project setup with repository configuration |

---

## 3. Engineering Tradeoffs

| Decision | Reasoning | Tradeoff |
|---|---|---|
| **Host-Client over Dedicated Server** | Simplest topology for 2-player scope. No server infrastructure needed. | Host has ~0ms latency advantage. Host crash ends session for both players. |
| **Server-authoritative AI** | Eliminates any possibility of enemy desync between clients. Both clients always see identical enemy state. | Client sees slight positional lag on fast-moving enemies due to NetworkTransform interpolation. |
| **NetworkVariable for health/score** | Auto-replicates on change with zero manual sync code. OnValueChanged drives UI updates cleanly. | Write permission restricted to server only — all damage/heal must go through server. |
| **CharacterController over Rigidbody** | Snappier, more responsive movement. No physics lag or momentum accumulation. Matches GDD "responsive, snappy" requirement precisely. | No physics-based interactions between players (pushing, collision response). |
| **No client-side prediction** | Significantly simpler codebase. Appropriate for LAN/local multiplayer scope of this assignment. | Movement may feel slightly delayed on high-latency connections (100ms+). |
| **Slot-based session state (not clientId-based)** | NGO increments clientId on every reconnect. Slot-based approach correctly identifies returning players regardless of new clientId. | Assumes only one non-host client. Extending to 3+ players would need slot expansion logic. |
| **Primitive meshes only** | Keeps project size minimal, load times fast, and complies with GDD asset requirements. | Purely functional visuals — no artistic polish. |
| **No NavMesh / crowd avoidance** | GDD specifies only Patrol/Chase/Attack state machine behaviour. NavMesh was outside the stated scope. | Multiple enemies targeting the same player can visually overlap/stack. |
| **`GameObject.Find()` for UI wiring** | Necessary bridge between scene UI objects and dynamically spawned player prefabs. Prefab Inspector cannot reference scene objects directly. | Called once on spawn — no runtime performance impact. |

---

## 4. Known Issues & Limitations

| Issue | Details |
|---|---|
| **Enemy visual stacking** | Enemies have no physical separation or crowd avoidance. Multiple enemies targeting the same player can overlap visually. NavMesh and steering behaviours were outside the GDD scope. |
| **No dedicated lobby** | Players connect manually via Host/Join buttons. There is no matchmaking, lobby browser, or player discovery system. |
| **Ping display approximation** | Ping is calculated from `UnityTransport.GetCurrentRtt()` divided by 2. Accuracy depends on transport layer measurement — may not match OS-level ping exactly. |
| **No player respawn** | When a player's health reaches 0 they remain in the scene at 0 health. A death/respawn cycle was not specified in the GDD and was not implemented. |
| **Projectile tunnel risk** | Fast-moving projectiles use discrete collision detection. At very high speeds a projectile may pass through a thin enemy without registering a hit. Setting Rigidbody Collision Detection to Continuous reduces this. |
| **Session state is memory-only** | Reconnection state (health, cooldowns) is stored in host process RAM. If the host crashes or closes the application, reconnection state is permanently lost. A production system would persist state to disk or a remote database. |
| **clientId increments on rejoin** | NGO assigns a new clientId each time a client reconnects. The SessionManager handles this via slot-based mapping, but this is a known NGO behaviour documented here for transparency. |
| **Single non-host client only** | The session management system is designed for exactly one non-host client (2-player total). Extending beyond 2 players would require slot map expansion. |

---

## Project Structure

```
ArenaProtocol/
├── Assets/
│   ├── Materials/          # Primitive materials (floor, walls, players, enemies, orbs)
│   ├── Prefabs/            # Player, Enemy, Projectile, EnergyOrb network prefabs
│   ├── Scenes/
│   │   └── ArenaScene.unity
│   └── Scripts/
│       ├── Abilities/
│       │   ├── BaseAbility.cs
│       │   ├── DashAbility.cs
│       │   ├── ProjectileAbility.cs
│       │   └── HealAbility.cs
│       ├── UI/
│       │   ├── AbilityUI.cs
│       │   └── PingDisplay.cs
│       ├── PlayerController.cs
│       ├── PlayerHealth.cs
│       ├── PlayerSpawnManager.cs
│       ├── EnemyAI.cs
│       ├── EnemyHealth.cs
│       ├── EnemySpawner.cs
│       ├── EnergyOrb.cs
│       ├── ScoreManager.cs
│       ├── SessionManager.cs
│       ├── GameManager.cs
│       ├── CameraFollow.cs
│       └── Projectile.cs
├── Packages/
│   └── manifest.json       # NGO, Unity Transport, Multiplayer Tools — auto-installs
├── ProjectSettings/
└── README.md
```

---

*Built by Nikhil Chaudhary — Arena Protocol - **2-Player Co-op Multiplayer Survival Arena** — Built with Unity 2022 LTS & Netcode for GameObjects*
