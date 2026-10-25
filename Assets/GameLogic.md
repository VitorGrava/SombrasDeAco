# Game Logic Overview

This document summarizes all game‑logic C# scripts in the project, grouped by folder. For each script it lists:
- **Class** – primary MonoBehaviour
- **Purpose** – high level description
- **Key public fields / properties**
- **Public methods** (notable actions)
- **Other noteworthy behaviour**

## Assets/Scripts

### CampoDeVisao.cs
| Field | Description |
|---|---|
| `viewRadius` | Max distance to detect the player. |
| `viewAngle` | Cone angle in degrees. |
| `playerLayer` | Layer mask for the player. |
| `obstacleLayer` | Layers that block vision (walls, platforms). |
| `eyePosition` | Transform used as the origin of the FOV check. |
| `checkInterval` | Seconds between each FOV scan. |

**Purpose:** Provides a reusable field‑of‑view component for enemies.

**Key methods:**
- `FOVCheck()` – performs sphere overlap, angle test and raycast to set `playerInSight`.  
- Coroutine `FOVCoroutine()` runs `FOVCheck()` every `checkInterval` seconds.  
- Gizmos draw the radius and cone in the editor.

### CampovisaoTank.cs
| Field | Description |
|---|---|
| Same fields as **CampoDeVisão** – used by tank enemies. |

**Purpose:** Similar to *CampoDeVisão* but tailored for tanks (uses `eyePosition.forward`).  
**Key methods:** identical to CampoDeVisão.

### Bala.cs
| Field | Description |
|---|---|
| `player` | Transform of the player, assigned in Awake. |
| `destroyBala` | LayerMask specifying objects that destroy the bullet on collision. |
| `velocidade` | Speed at which the bullet travels toward the player. |

**Purpose:** Projectile that homes toward the player and triggers loss on collision.

**Key methods:**
- `Atirar()` – sets linear velocity toward the player.  
- `OnTriggerEnter(Collider)` – destroys itself, kills the player and loads the *Perdeu* scene.

### BossScript.cs
| Field | Description |
|---|---|
| `velocidadeMovimento` | Speed factor for sine‑wave movement. |
| `distanciaPatrulha` | Range of X oscillation. |

**Purpose:** Boss patrol movement using a sine wave along the X axis.

**Key method:** `FixedUpdate()` – calculates new X position with `Mathf.Sin(Time.time * velocidadeMovimento)` and moves the rigidbody.

### ControleCenas.cs
Empty script – placeholder for future scene‑control logic.

### JogoFinal.cs
| Field | Description |
|---|---|
| (none) |

**Purpose:** Triggers the win condition when the player collides with a trigger.

**Key method:** `OnTriggerEnter(Collider)` loads *Ganhou* scene once.

### MovimentoCamera.cs
| Field | Purpose |
|---|---|
| `player` | Reference to the player transform. |
| `limiteEsquerda`, `limiteDireita` | Horizontal bounds for camera movement. |
| `suavizacaoX`, `suavizacaoY` | Smoothing factors per axis. |
| `distanciaLookAhead`, `suavizacaoLookAhead` | Forward look‑ahead distance and smoothing. |
| `deadZoneX`, `usarDeadZone` | Dead zone threshold to stop camera jitter. |
| `seguirY`, `offsetY`, `offsetX` | Vertical follow toggle and offsets. |

**Purpose:** Smoothly follows the player with optional dead zone, look‑ahead and vertical offset.

**Key method:** `FixedUpdate()` calculates target position, applies dead zone, clamps within limits, then smoothly interpolates using `Mathf.SmoothDamp`.

### MovimentoInimigo.cs
| Field | Description |
|---|---|
| Movement speeds (`velocidadePatrulha`, `velocidadeSeguir`). |
| Rotation speed and smoothing. |
| References: `player`, `CampoDeVisão`. |

**Purpose:** Enemy AI that patrols and chases the player when detected.

**Key methods:**
- `FixedUpdate()` – chooses between patrol, chase or idle based on visibility and hidden state.  
- `MovimentarPatrulha()`, `SeguirPlayer()`, `AtualizarRotacao()` implement the movement logic.

### SniperController.cs
| Field | Description |
|---|---|
| Movement parameters (`velocidade`, `eixoMovimento`). |
| Rotation and FOV references (`campoDeVisaoObjeto`, `campoDeVisão`). |
| Bullet prefab, shooting interval. |

**Purpose:** Enemy sniper that patrols, detects the player, and shoots when in sight.

**Key methods:**
- `Update()` – handles patrolling, limit checks, shooting logic via `ChecarPlayerParaAtirar()`.  
- `ChecarPlayerParaAtirar()` – fires a bullet when the player is visible and reload time elapsed.

### Tank_movimentacao.cs
| Field | Description |
|---|---|
| Movement settings per axis (`velocidadePatrulha`, `distanciaPatrulha`, etc.). |
| Pursuit parameters (speed, minimum distance, shooting range). |
| Detection (`CampovisaoTank`). |
| Bullet prefab, shooting interval. |

**Purpose:** Tank AI with patrol, optional boss mode, chase, and shooting.

**Key methods:**
- `FixedUpdate()` – decides between boss mode, chase or patrol based on visibility.  
- `MovimentarPatrulha()`, `AndarParaFrente()`, `SeguirPlayer()`, `TentarAtirar()` implement movement and attack.

### TankSpawnTrigger.cs
| Field | Description |
|---|---|
| `tankPrefab` | Prefab of the tank to instantiate. |
| `tankNaCena` | Optional existing tank in scene to activate and reposition instead of instantiating. |
| `direcaoDoAvanco` | Direction player moves; tank spawns on opposite side. |
| `distanciaMinima` | Minimum distance behind the player before spawning. |
| `margemForaDaCamera` | Extra margin outside camera view to ensure off‑screen spawn. |
| `camadaChao` | Layer mask of ground for raycast placement. |
| `offsetY` | Vertical offset if tank pivot not at base. |
| `tagPlayer` | Tag used to detect player trigger entry. |

**Purpose:** Spawns a tank behind the player when they enter the trigger, positioning it off‑screen and starting pursuit.

**Key methods:**
- `OnTriggerEnter(Collider)` – detects player and initiates spawn.  
- `SpawnarTank(Transform player)` – calculates spawn position, instantiates or activates tank, calls `IniciarPerseguicao` on `Tank_movimentacao`.

## Assets/Scripts/ComportamentoPlayer

### ControleStamina.cs
| Field | Description |
|---|---|
| `stamina`, `staminaMax` | Current and maximum stamina values. |
| `estaCorrendo` | Indicates if the player is running. |
| Consumption and regeneration rates, delay before regen starts. |
| UI references (`Image`, `CanvasGroup`). |

**Purpose:** Manages the player's stamina resource during running.

**Key methods:**
- `Update()` – triggers regeneration when not running.  
- `Correndo()` – reduces stamina and starts a coroutine to delay regen.

### GerenciadorEstadoJogador.cs
| Field | Description |
|---|---|
| Singleton instance (`Instancia`). |
| `estaEscondido` flag. |

**Purpose:** Singleton that tracks whether the player is hidden across scenes.

**Key methods:**
- `SetEscondido(bool)` – sets internal flag and persists across scenes.  
- `EstaEscondido()` – returns current state.

### SistemaDeVida.cs
| Field | Description |
|---|---|
| `estaMorto` flag to prevent repeated death logic. |

**Purpose:** Handles player death when colliding with enemies, loading the *Perdeu* scene.

**Key method:** `OnCollisionEnter(Collision)` destroys the player and loads loss scene.

### PlayerController.cs
| Field | Description |
|---|---|
| Movement speeds (`velocidadeAndando`, `velocidadeAgachando`, `velocidadeCorrendo`). |
| Rotation speed, jump force, coyote time, jump buffer, gravity multipliers. |
| State enum for movement (walking, crouching, running). |

**Purpose:** Handles player movement, rotation, jumping and stamina integration.

**Key methods:**
- `Start()` – gets components.  
- `Update()` – handles input for jumping (jump buffer).  
- `FixedUpdate()` – moves based on current movement state, updates animator, applies gravity and jump logic.  
- `MovimentacaoPlayer()`, `AtualizarEstadoMovimento()`, `Pulo()`, `AplicarGravidade()`, `Rotacao(float)` implement core gameplay.

## Assets/Scripts/ComportamentoPlayer/Menu

### Menu.cs
| Field | Description |
|---|---|
| None (UI callbacks). |

**Purpose:** Provides UI buttons to start the game or quit.

**Key methods:**
- `PlayGame()` – loads *Alpha* scene.  
- `QuitGame()` – exits application.

## Assets/Scripts/ComportamentoPlayer/SistemaInteracao

### Interactor.cs (abstract)
| Field | Description |
|---|---|
| Collider size and offset for interaction zone. |

**Purpose:** Base class for objects the player can interact with (e.g., doors, pickups). Handles collider setup and input detection.

**Key methods:** `OnTriggerEnter`, `OnTriggerExit`, `Update()` (checks E key), abstract `Interagir()`.

### Porta.cs
| Field | Description |
|---|---|
| Reference to player transform. |
| Offset for hiding mechanic (`deslocamentoZ`). |
| Hidden state flag. |

**Purpose:** Implements a hidden room mechanic – moves the player forward/backward and toggles hidden state via `GerenciadorEstadoJogador`.

**Key methods:** `Esconder()`, `SairEsconderijo()`.
