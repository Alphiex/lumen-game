# Lumen — Game Design Spec

Brainstormed and approved 2026-05-04. This spec is the source of truth for the
Lumen game implementation. Implementation plan to follow via `writing-plans`.

## Pitch

Atmospheric one-thumb mobile exploration. You are a small light-being, a wisp.
Your task: guide a spirit-fox through fog-shrouded biomes by being the light it
follows. Hold your finger anywhere on screen → the wisp moves to that point and
the fox runs toward the wisp. Release → wisp dims, fox stops to listen. Collect
memory motes (glowing orbs) to extend the journey before daylight fades; reach
the next biome's gate to complete the loop.

Reference: Alto's Odyssey (mood + auto-progress), Sky: Children of the Light
(atmospheric beauty + simple controls), GRIS (color-as-narrative).

Differentiator: the *follow-the-light* pull mechanic is novel for mobile, and
the cinematic post-processing aesthetic is the project's existing strength
(carried over from the Three.js art-reference work) — distinctive in a market
crowded with auto-runners and idle clickers.

## Core loop (5-10 minutes per session)

1. Player opens app → biome scene loads (low-poly nature, fog, soft hour-of-day lighting)
2. Fox stands at the start, looking around, ears moving
3. Player touches screen → wisp appears at finger position, fox runs toward wisp
4. As fox moves, daylight meter (top of screen) ticks down slowly
5. Memory motes (glowing orbs scattered through scene) refill daylight when
   the fox passes through them
6. Player guides fox toward the biome gate (visible on horizon) before daylight
   runs out
7. Reaching the gate → screen fades out, soft chime, "First biome — completed"
   text appears, return to start (loop)
8. Daylight runs out before gate → screen fades out, soft sigh, "The light
   was not enough yet" text appears, return to start (no penalty, no Game Over)

No score. No fail state. The loop replays itself with subtle variation in mote
placement and weather (rain / mist / dusk) to keep replays fresh.

## Controls (single mechanic)

| Input | Behavior |
|---|---|
| Touch screen and hold | Wisp appears at finger, fox runs toward wisp at base speed |
| Drag finger | Wisp follows finger; fox keeps tracking wisp |
| Release | Wisp dims, fox decelerates to stop and look around |
| Tap (no hold) | Wisp pulse — gentle attractor, no movement command |

That's it. No buttons, no menus during play, no inventory.

## Scene architecture (v1: one biome)

**Biome 1 — "The Hush" (fog-shrouded forest)**
- Quaternius nature pack assets: trees (pine + oak variants), ferns, moss
  rocks, fallen logs, mushroom clusters
- Ground: soft grass-textured terrain with ankle-deep fog
- Lighting: soft directional light (low sun angle), volumetric fog, depth-graded
  color (cool blues at distance, warm amber in foreground)
- Skybox: stylized cloud cover with subtle motion
- Length: ~60 seconds at base fox speed, gate visible from start
- 8-12 memory motes scattered, some on direct path, some require detour
- Ambient audio: forest sounds (wind in leaves, distant birds), one ambient
  music loop (CC0 source, e.g., Kevin MacLeod or Tasos Sioukas)

## Visual direction

URP (Universal Render Pipeline) Volume stack approximating the Three.js
art-reference aesthetic:

| URP effect | Purpose | Reference (Three.js art-ref) |
|---|---|---|
| Bloom | Glow on motes, wisp, dawn light through trees | UnrealBloomPass |
| Color Adjustments + Color Curves | Cinematic color grading; cool-to-warm depth gradient | Color temperature grading shaders |
| Depth of Field | Soft far-field blur for atmospheric haze | Depth-keyed luminance hierarchy |
| Vignette | Frame the action toward center; gentle | Vignette shader |
| Film Grain | Texture overlay, subtle | Film grain shader |
| Lens Distortion | Very mild, for cinematic feel | (no direct equivalent) |
| Ambient Fog (URP volumetric) | Core atmospheric effect | Atmospheric haze shaders |
| Tonemapping (ACES) | Standard cinematic tonemapper | (replaces 280-pass tonal logic) |

Pixel-perfect equivalence to the 280-pass Three.js scene is **explicitly out of
scope.** The look-and-feel target is "feels like the Three.js scene" not "is
the Three.js scene." The shader work is reference, not specification.

## Tech stack

- **Engine**: Unity 2022.3 LTS (current LTS, mobile mature)
- **Render pipeline**: URP (handles iOS + Android + WebGL)
- **Character controller**: Unity Starter Assets Third Person Controller (free,
  Asset Store), swap character mesh for fox
- **Fox model**: Quaternius animal pack OR free Mixamo fox; rigged with idle,
  walk, run, look-around animations
- **Input**: Unity Input System with touch handler
- **Audio**: Unity built-in audio with one ambient music loop + 3-5 spatial
  ambient sounds (wind, birds, mote-collect chime, gate-reach chime, sigh)
- **Build targets**: iOS (TestFlight) + Android (Play Console internal track)
- **Optional v1.5**: WebGL build for shareable URL link

## What ships in v1 (concrete)

- One biome ("The Hush"), playable end-to-end
- Single mechanic (hold-to-pull, release-to-stop)
- 8-12 memory motes, one biome gate, one daylight timer
- Cinematic URP post-processing tuned to art-reference vibe
- iOS + Android internal-track builds installed on Mike's device(s)
- README with build instructions + screenshots

## What's explicitly NOT in v1

- Multiple biomes (defer to v2 once v1 is shipping)
- Procedural generation (v1 is hand-placed)
- Multiplayer, accounts, leaderboards
- IAP, monetization, ads
- Cutscenes, dialogue, words on screen during gameplay
- Settings menu (defer; use sensible defaults)
- Localization (English text only; mostly iconographic anyway)
- Pixel-perfect art-reference shader fidelity

## 30-day milestone breakdown

**Week 1 — Foundation**
- [ ] Init github.com/Alphiex/lumen-game with .gitignore + README
- [ ] Unity 2022.3 LTS project upgraded with URP installed
- [ ] Starter Asset Third Person Controller imported, working with
  default character on PC
- [ ] iOS + Android build targets configured (Player Settings, bundle id)
- [ ] First "hello fog" build deployed to device (just URP fog + a cube)

**Week 2 — Mechanic**
- [ ] Fox model imported, rigged, animations wired (idle/walk/run/look)
- [ ] One-thumb touch input wired: hold = wisp appears, fox follows wisp
- [ ] Wisp visual: small glowing orb that follows finger
- [ ] Fox state machine: idle / running-to-wisp / decelerating / looking
- [ ] Verified on device — input works, fox tracks wisp

**Week 3 — Scene + visuals**
- [ ] Biome 1 scene built with Quaternius assets (trees, fog, terrain)
- [ ] URP Volume stack tuned (bloom, color grading, DoF, vignette, fog)
- [ ] Memory motes placed (8-12) with collect logic
- [ ] Daylight meter UI + countdown logic
- [ ] Biome gate placed; reaching it triggers loop-end
- [ ] Ambient audio + sound effects wired

**Week 4 — Ship**
- [ ] iOS TestFlight build uploaded; Mike installs
- [ ] Android internal-track build uploaded; Mike installs
- [ ] Mike plays through one full loop on each device
- [ ] Bug fixes from Mike's feedback
- [ ] First public-shareable artifact: gameplay video / WebGL URL

## Risks & mitigations

| Risk | Likelihood | Mitigation |
|---|---|---|
| Apple Developer account not ready | Medium | Confirm in Week 1; if missing, ship Android-first |
| Fox animation gaps (Quaternius may not have full animal rig) | Medium | Fall back to Unity-Chan or free Mixamo asset |
| URP performance on low-end Android | Medium | Profile early on lowest-target device; reduce post-processing if needed |
| Scope creep ("just one more shader") | High | Spec explicitly rules out art-reference fidelity; weekly milestone gates |
| Music licensing | Low | CC0 sources only (Pixabay Music, Free Music Archive) |

## Implementation rule

The cron-driven Claude worker (`deliver-game-4h`) reads this spec + the
PROJECT_STATUS.md file, picks the next un-done item from the milestone
breakdown, ships it via Unity edits + commits, and updates PROJECT_STATUS.md.

Each cycle ships ONE meaningful step — committed code, not status updates.
