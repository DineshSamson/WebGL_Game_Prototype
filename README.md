# Stealth Infiltration: Unity Stealth Prototype

A top-down / third-person stealth game built in **Unity 6.0 (6000.0.83f1 LTS)** and shipped as a **WebGL** build. Sneak past patrolling guards, pick up the item, and reach the exit without being caught.

**▶ Play in browser:**
**Build size:** 17.8 MB (Gzip compression) | **Target:** 60 FPS in WebGL

---

## Features

- Rigidbody-based player movement with mouse aiming
- Guard AI with a 3-state machine: **Patrol → Chase → Suspicious**
- Vision cone built from scratch and rendered live on the floor, clipped by walls
- Cone colour changes with guard state (yellow / orange / red)
- Custom object pool for bullets (no runtime `Instantiate`/`Destroy`)
- Pre-allocated mesh buffers in the vision cone
- All gameplay C# written from scratch

---

## Controls

| Input | Action |
|-------|--------|
| **W** or **↑** | Move forward |
| **Mouse pointer** | Set the direction the player faces |

---

## Guard AI

Each guard runs a simple state machine in `FixedUpdate`:

| State | Cone colour | Behaviour | Transition |
|-------|-------------|-----------|------------|
| **Patrol** | Yellow | Walks between waypoints in a loop at `patrolSpeed` | Sees player → **Chase** |
| **Chase** | Red | Runs to the player's last seen position at `chaseSpeed` | Player out of sight for `loseSightTime` (3 s) → **Suspicious**. Within `catchDistance` → player caught |
| **Suspicious** | Orange | Walks to the last seen position, stops and searches for `searchTime` (3 s) | Sees player → **Chase**. Search time over → **Patrol** |

Guards can also be alerted by other systems through the public `Investigate(Vector3 position)` method, which sends them to a position in **Suspicious** state (ignored while chasing).

**Movement:** guards steer with `Quaternion.RotateTowards` and move by setting Rigidbody velocity. A forward raycast (`wallCheckDistance`) detects walls ahead, and the guard sidesteps along its right vector to get around them.

| Setting | Default |
|---------|---------|
| `viewRange` | 8 |
| `viewAngle` | 90° |
| `patrolSpeed` / `chaseSpeed` | 2 / 4 |
| `loseSightTime` | 3 s |
| `searchTime` | 3 s |
| `catchDistance` | 1 |

---

## Vision Cone

The cone has two parts: **detection** (can the guard see the player?) and **rendering** (the cone drawn on the floor). Both use the same guard settings: `viewAngle`, `viewRange` and `wallMask`.

### Detection math

`CanSeePlayer()` runs three checks, cheapest first, and exits early on any failure:

1. **Range:** flat (XZ) squared distance compared to `viewRange²`, so there is no `sqrt`.
2. **Angle:** `Vector3.Dot(guard.forward, dirToPlayer)` compared to a `cos(viewAngle / 2)` that is cached once in `Awake`, so no `acos` call is needed.
3. **Line of sight:** `Physics.Linecast` from the guard's eye to the player's chest against `wallMask`. If a wall is hit, the player is hidden.

```csharp
Vector3 toPlayer = player.position - transform.position;
toPlayer.y = 0;

if (toPlayer.sqrMagnitude > viewRange * viewRange) return false;                 // 1. range
if (Vector3.Dot(transform.forward, toPlayer.normalized) < cosHalfAngle) return false; // 2. angle

Vector3 eye    = transform.position + Vector3.up;
Vector3 target = player.position + Vector3.up;
return !Physics.Linecast(eye, target, wallMask);                                  // 3. line of sight
```

### Rendering the cone as a mesh

The visible cone is one dynamic mesh, rebuilt every `LateUpdate`:

1. **Cast a fan of rays.** `rayCount` rays (default 40) are spread evenly across `viewAngle`, starting at the guard's position raised by `rayHeight`. Each is a `Physics.Raycast` limited to `viewRange`, checking only `wallMask`.
2. **Pick the point for each ray.** On a wall hit, the point is the hit position pulled back by `wallPadding` (stops the mesh clipping into the wall). With no hit, it is at full `viewRange`.
3. **Refine the corners.** Between two neighbouring rays, if one hits and the other misses, or their hit distances differ by more than `edgeDistanceThreshold`, there is a wall corner between them. `FindEdge` runs a **binary search** (`edgeIterations`, default 8) on the angle between the two rays to find the corner. It adds a near and a far point, so the cone has sharp edges at corners instead of a jagged fan.
4. **Build a triangle fan.** Vertex 0 is the guard's position, and every other vertex is a cast point converted to local space and flattened to `floorOffset` so the cone lies on the ground. Triangles are `(0, i+1, i+2)`.

### Allocation-free mesh updates

- `points`, `vertices` and `triangles` are allocated **once** in `Awake`, sized for the worst case of `rayCount * 3 + 3` points (each ray can add up to two edge points).
- The triangle index buffer is built once and never changes.
- Unused vertex slots collapse onto the last point, creating degenerate (zero-area) triangles that never render, so the mesh never needs resizing.
- The mesh uses `MarkDynamic()`, has shadows off, and only vertices and bounds are updated each frame.

| Setting | Default | Effect |
|---------|---------|--------|
| `rayCount` | 40 | Base cone resolution |
| `edgeIterations` | 8 | Corner accuracy (binary search steps) |
| `edgeDistanceThreshold` | 1 | Distance jump that counts as an edge |
| `wallPadding` | 0.02 | Pulls the cone off walls to avoid clipping |
| `floorOffset` | 0.05 | Height above the floor, avoids z-fighting |

---

## Object Pooling

Bullets are pooled. **10 bullets are created up front** and reused for the whole session. When a bullet is fired, an inactive one is taken from the pool, positioned and activated. When it hits something or expires, it is deactivated and returned to the pool.

This means there is **no `Instantiate` or `Destroy` during gameplay**, so no garbage-collection spikes in the WebGL build.

---

## WebGL Build Notes

- Build size: **17.8 MB** (under the 20 MB target)
- Compression: **Gzip**
- Pooled bullets and pre-allocated vision cone buffers keep frame times stable at 60 FPS

---

## Run Locally

1. Install **Unity 6.0 (6000.0.83f1)** through Unity Hub.
2. Open the project folder in Unity Hub.
3. Open `Assets/Scenes/<!-- TODO: SampleScene -->.unity` and press Play.

To build: **File → Build Profiles → Web → Build**.

---

## Art & Asset Credits

| Asset | Source |
|-------|--------|
| Guard and player characters, animations | [Mixamo](https://www.mixamo.com) |


---

## Author

**Dinesh Samson**, Senior XR / Unity Developer
