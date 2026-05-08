# Project: Lumen (Game Development)

## Vision
A playable mobile game (iOS + Android) — atmospheric one-thumb exploration where
the player guides a small spirit-fox through fog-shrouded biomes by trailing it
with light. Hold finger anywhere on screen → fox runs toward your finger.
Release → fox stops to listen. Collect "memory motes" (glowing orbs) to extend
the journey; reach the next biome's gate before daylight fades. 5-10 minute
sessions, no win/lose pressure, just a meditative chase-the-light loop.

Shipped = TestFlight build (iOS) + internal-track Google Play build (Android)
that Mike can open and play through one full biome loop on a physical device.

## Reference vibes
Alto's Odyssey + Sky: Children of the Light + GRIS — but with a fox instead of
a human and a follow-the-light pull mechanic instead of jumping. Atmospheric
post-processing IS the gameplay feel.

## ⚠️ Honest reorientation from prior project state
For the past 60+ days the cron-driven loop produced **280 shader passes on a
single non-interactive Three.js cinematic scene** (`scene.html`, ~948KB / 22,004
lines). This was visual-quality iteration without a playable loop, no input
handling, no state machine. The shader work is preserved as art reference
under `art-reference/` — no longer the deliverable.

## Status snapshot (2026-05-07)
- Git: INITIALIZED — github.com/Alphiex/lumen-game (main branch, push confirmed)
- Stack: Unity (URP) on mobile + WebGL (URP scales to both)
- Existing assets: Quaternius low-poly nature pack at `unity/Assets/ThirdParty/`
- Existing art reference: 7 captured screenshots + scene.html in `art-reference/`
- GitHub Pages: LIVE at https://alphiex.github.io/lumen-game/ (source: main /docs)
- Unity WebGL Build Support module: NOT YET INSTALLED (needs Unity Hub → 2022.3.62f3 → Add modules → WebGL Build Support ~700MB). Build infra ready; just needs the module.

## Done
- [x] Three.js commercial scene with 280-pass post-processing pipeline (now art reference)
- [x] 7 captured screenshots (baseline, crane, dolly, flythrough, hero-reveal, latest)
- [x] Quaternius nature asset pack imported into Unity Assets/ThirdParty
- [x] Capture workflow (Playwright headless Chromium → 1920x1080 PNG)
- [x] **Init lumen-game repo** — git init, .gitignore (Unity), README.md, art-reference/
      moved from artifacts/commercial-scene/, pushed to github.com/Alphiex/lumen-game
      main branch. 425 files, 63,524 insertions. **commit: 2c78996**
- [x] **Unity project structure** — Packages/manifest.json (URP 14.0.10, Input System 1.7,
      Cinemachine 2.9.7, TextMeshPro), ProjectSettings/ (bundle id com.alphiex.lumen,
      iOS 13+, Android API 24+, URP graphics, 3 quality tiers), Assets/Settings/
      (LumenURPAsset + LumenURPRenderer + LumenVolumeProfile with Bloom/ACES/Vignette/
      DoF/FilmGrain), Assets/Scripts/ (FoxController, WispController, DaylightManager,
      MemoryMote, BiomeGate), Assets/Scenes/TheHush.unity (scene stub — directional
      light, camera, ground plane, GlobalPostProcessVolume, DaylightManager).
      **commit: 2120311**
- [x] **Scene wired — Fox, WispLight, MemoryMotes, BiomeGate, UI, AnimatorController**
      TheHush.unity fully wired: Fox capsule (NavMeshAgent + FoxController + Animator
      tagged "Fox"), WispLight (PointLight + WispController with mainCamera+fox refs),
      10 MemoryMotes scattered z=15→70, BiomeGate at z=85 with trigger + warm gold light,
      UI Canvas (DaylightSlider top-center, OutcomeText centered hidden, FadeOverlay
      full-screen black CanvasGroup alpha=0), DaylightManager refs wired, AudioSource
      added, FoxAnimator.controller (Speed float + LookAround trigger) created.
      com.unity.ai.navigation 1.1.5 added to manifest for NavMeshSurface.
      NavMesh bake: open Unity → Window → AI → Navigation → Bake (Ground is already
      NavigationStatic). **commit: c61d7ac**
- [x] **URP Volume tuning + scene dressing** — 11 Quaternius FBX .meta files created
      (Pine_1/3, TwistedTree_1/3, CommonTree_1/3, Fern_1, Rock_Medium_1/3, Bush_Common,
      Mushroom_Common) + 4 directory .meta files. 19 PrefabInstance objects placed in
      TheHush.unity (10 trees, 4 ferns/bushes, 3 rocks, 2 mushrooms) spanning z=8→80,
      framing the corridor without blocking the fox path. NavMeshSurface component added
      to Ground GameObject (bake still required in Unity Editor). LumenVolumeProfile
      tuned: Bloom threshold 0.85→0.8, Vignette intensity 0.32→0.35, ACES confirmed.
      **commit: d425434**
- [x] **Fox model v1 + animations** — No Quaternius fox in pack (nature-only); procedural
      fox fallback authorized + executed. FoxMeshBuilder.cs: [ExecuteAlways] script builds
      11-child primitive rig (Body capsule, Head sphere, Snout cube, EarL/R cubes, TailBase/
      TailTip spheres, 4 leg capsules) with amber URP material. ProceduralFoxAnimator.cs:
      transform-driven idle/walk/run/lookAround animation (breathing bob, trot gait at 90°
      leg phase offset, tail sway, ear flick/perk). FoxController updated to drive proc
      animator. FoxAnimator.controller upgraded to 1D blend tree (Idle@0/Walk@1/Run@2) +
      LookAround state. **commit: 24b0ec3**
- [x] **Scene component wiring + NavMesh bake** — FoxMeshBuilder + ProceduralFoxAnimator
      wired to Fox GameObject in TheHush.unity. NavMeshBaker.cs Editor script created
      (Assets/Editor/); Unity batch-mode bake executed and **SUCCEEDED**: 1 NavMeshSurface
      found on Ground, BuildNavMesh() called, scene saved with baked NavMeshData embedded
      (scene 77KB → 256KB). Fox can now pathfind at runtime without any manual Unity Editor
      step. 62 additional Quaternius .meta files + ProjectSettings finalized.
      NavMesh bake blocked item RESOLVED. **commit: 99484c2**
- [x] **TheHush.unity YAML fix** — Converted scene back to text YAML (NavMeshData stored
      externally). Scene now has 33 GameObjects including 11 procedural fox parts persisted
      by FoxMeshBuilder [ExecuteAlways] during bake. **commit: eae54d8**
- [x] **Touch input polish + wisp visual orb** — WispMeshBuilder.cs: [ExecuteAlways]
      builds emissive sphere child ("WispOrb") with HDR pale-cyan URP emissive material
      (baseColor × 2.5 emission). WispController.cs refactored: tap-vs-hold via 0.15s
      threshold (tap = scale-pop pulse, fox stays; hold = wisp follows + fox tracks),
      smooth light-dim coroutine on release (0.3s fade), mesh visibility synced, initial
      snap-to-finger on press-down. TheHush.unity: WispMeshBuilder component wired to
      WispLight. Inspector knobs: tapPulseDuration, tapPulseScalePeak, tapPulseIntensity.
      **commit: 146b702**
- [x] **MemoryMote visual orb + WebGL build configuration** — MemoryMoteVisualBuilder.cs:
      [ExecuteAlways], builds "MoteOrb" child sphere (pale-gold HDR emissive, R=1.0 G=0.85
      B=0.3 × 3.5 emission, scale 0.3f), bobs on local Y (Sin × 0.08f at 1.2Hz), OnDestroy
      cleanup, ContextMenu rebuild. Wired to all 10 MemoryMote GOs in TheHush.unity
      (fileIDs 7000000015…7000000105). WebGL: ProjectSettings updated (webGLMemorySize
      512MB, webGLExceptionSupport 0, webGLPowerPreference 2=HighPerformance). New:
      Assets/Editor/WebGLBuilder.cs (MenuItem + batch-mode executeMethod), scripts/
      build-webgl.sh (auto-discovers Unity 2022.3.x binary, outputs to _builds/webgl/,
      logs to build-webgl.log). .gitignore: added `_builds/`. **commit: 174b1d2**
- [x] **GitHub Pages deploy + branded landing page** — docs/index.html: amber/cream
      branded landing page for Lumen with project status, iframed Three.js art reference
      scene, info cards (Game / Tech / Status), GitHub link. docs/scene.html: full
      1.1MB Three.js atmospheric scene. GitHub Pages enabled via API (source: main /docs).
      Site live at https://alphiex.github.io/lumen-game/ (HTTP 200 confirmed).
      Unity WebGL Build Support module not installed — build infra (WebGLBuilder.cs,
      build-webgl.sh) ready; swap with `cp -r _builds/webgl/* docs/` once module installed.
      **commit: 0a91c0f**
- [x] **Mote animation polish** — Phase-staggered 10 MemoryMotes' bob using
      `_phaseOffset = transform.GetSiblingIndex() * 0.41f` (golden-angle-ish spread
      across 10 siblings). Detuned MemoryMoteVisualBuilder.bobHz 1.2→0.7f to eliminate
      resonant double-bob with parent MemoryMote.cs. Motes now bob organically, each
      at its own rhythm. 1 file changed, 6 insertions. **commit: 6672ac3**
- [x] **BiomeGate visual builder** — BiomeGateVisualBuilder.cs [ExecuteAlways]: 4 warm-gold
      HDR emissive sphere pillars arranged in rectangular henge (x=±4, y=±1, scale 0.4f,
      R=1.0 G=0.7 B=0.2 × 4.0 emission), GateLight PointLight child (warm gold, range=12,
      intensity=1.8). ProximityPulse coroutine fires when fox enters 20m radius: intensity
      spikes 1.8→5.4 over 0.2s then fades back over 0.6s. Hysteresis at 1.3× radius
      prevents flicker. Wired to BiomeGate GO in TheHush.unity. Coexists with existing
      attention-pulse light on BiomeGate root. **commit: ed1c5ca**

## Next 3 deliverables (in order)
1. **Unity WebGL Build Support install + real WebGL build** — Install WebGL Build
   Support module via Unity Hub (2022.3.62f3 → ⋮ → Add modules → WebGL Build Support,
   ~700MB download). Then run `scripts/build-webgl.sh`, copy output to docs/, commit
   + push. Pages auto-redeploys. Mike gets a real playable browser build.
   ⚠️ BLOCKED: Requires manual Unity Hub GUI action to install the module. Can Claude
   automate via Unity Hub CLI? Worth checking — otherwise needs Mike to run it once.
2. **Ambient audio wiring** — CC0 ambient music loop (Pixabay Music / Free Music
   Archive) + 3-5 spatial SFX (wind, birds, mote-collect chime, gate-reach chime,
   sigh). Wire to AudioSource / DaylightManager. Commit + push.
   ⚠️ BLOCKED: Needs Mike's OK on CC0 sources — confirm or name a preferred composer.
3. **Daylight UI polish + outcome sequence** — DaylightSlider gradient upgrade:
   amber (#F5A623) at full → cool blue (#4A90D9) when depleted, updated each frame
   via Slider's fillRect Image.color. Add urgency pulse (CanvasGroup alpha oscillation
   at 2Hz) when daylight < 20%. Polish outcome sequence timing: fade-to-black 0.8s,
   text fade-in 0.5s (alpha tween on OutcomeText), hold 3s, fade-out + scene restart.
   DaylightManager.cs changes only. No external blockers.

## Blocked / decisions needed from Mike
- Apple Developer account access for TestFlight upload (needed for deliverable: First TestFlight build)
- Google Play Console access (one-time $25 dev account fee if not yet paid; needed for First Play build)
- Music: ambient soundtrack OK to use Royalty-Free / CC0 sources (plan: CC0 via
  Pixabay Music / Free Music Archive), OR Mike has a composer in mind?
  (Needed for ambient audio wiring — Next 3 item #2)
- **Unity WebGL Build Support module**: needs Unity Hub GUI install (2022.3.62f3 → Add modules
  → WebGL Build Support). One-time ~700MB download. All build infra is ready; this is the only
  missing piece to get a real playable browser build live. Can Mike install this once?
- **Fox model**: ~~Quaternius animal pack may not include a fox~~ — RESOLVED: Quaternius
  is nature-only. Procedural fox shipped (commit 24b0ec3). Real fox FBX can be swapped
  in later by replacing FoxMeshBuilder with an FBX + SkinnedMeshRenderer — not blocking.
- ~~Game name confirmation~~ — RESOLVED: "Lumen", repo = lumen-game, bundle id to be
  set in Unity Player Settings during deliverable #1 Unity setup
- ~~NavMesh bake~~ — RESOLVED: batch-mode bake succeeded (commit 99484c2). No manual
  Unity Editor step required.

## Out of scope for v1 (ruled out to prevent scope creep)
- Multiple biomes — v1 ships ONE biome loop
- Procedural generation — v1 is one hand-placed scene
- Multiplayer / leaderboards / accounts — v1 is single-player local
- IAP / monetization — v1 is free, no in-app purchases
- Story / cutscenes — v1 is wordless atmospheric

Shader-fidelity equivalence to the 280-pass Three.js reference is also out of
scope. URP can approximate the cinematic feel via Volume effects; pixel-perfect
match is not required and would block delivery indefinitely.

## Delivery rules
- Use Claude Code via ACPX with `frontend-design` skill for any UI/menu work
- Commit per deliverable; PR to main; push to origin
- Update this file's "Done" + "Next 3" each cycle as the source of truth
- Each cycle ships ONE meaningful step — not a perfect deliverable, a real one

## Progress log
| Date | Commit | What shipped |
|------|--------|-------------|
| 2026-05-05 | 2c78996 | Repo init, .gitignore, README, art-reference moved |
| 2026-05-05 | 2120311 | Unity project structure: Packages, ProjectSettings, URP pipeline assets, all 5 core C# scripts, TheHush scene stub |
| 2026-05-06 | c61d7ac | TheHush scene wired: Fox+NavMeshAgent, WispLight, 10 MemoryMotes, BiomeGate, UI canvas, FoxAnimator controller, DaylightManager refs |
| 2026-05-06 | d425434 | Scene dressing: 19 Quaternius props placed (trees/ferns/rocks/mushrooms), 11 FBX .meta files, NavMeshSurface on Ground, LumenVolumeProfile tuned (Bloom 0.8, vignette 0.35) |
| 2026-05-06 | 24b0ec3 | Fox model v1: FoxMeshBuilder (11-part primitive rig), ProceduralFoxAnimator (trot gait/breathing/tail sway), FoxController wired, FoxAnimator.controller 1D blend tree |
| 2026-05-06 | 99484c2 | Scene wiring complete: FoxMeshBuilder+ProceduralFoxAnimator on Fox GameObject, NavMesh batch bake SUCCEEDED (1 surface, scene saved), 62 Quaternius .meta files, ProjectSettings finalized |
| 2026-05-06 | eae54d8 | TheHush.unity YAML fix: NavMeshData stored externally, scene 33 GOs confirmed, text YAML verified |
| 2026-05-06 | 146b702 | Touch input polish + wisp visual: WispMeshBuilder emissive orb, tap-vs-hold (0.15s), smooth dim on release |
| 2026-05-07 | 174b1d2 | MemoryMote emissive orb (pale-gold HDR × 3.5, 0.3f scale, bob) wired to 10 motes + WebGL build config: ProjectSettings 512MB, WebGLBuilder.cs, build-webgl.sh |
| 2026-05-07 | 0a91c0f | GitHub Pages deploy: docs/index.html (branded landing, iframed Three.js scene), GitHub Pages enabled via API, live at https://alphiex.github.io/lumen-game/ |
| 2026-05-08 | 6672ac3 | Mote animation polish: phase-stagger 10 orbs (siblingIndex × 0.41 rad), bobHz 1.2→0.7f — organic feel, no resonant sync |
| 2026-05-08 | ed1c5ca | BiomeGate visual builder: 4 warm-gold HDR emissive pillar spheres + GateLight PointLight + ProximityPulse coroutine (1.8→5.4 intensity, 20m radius, hysteresis 1.3×) wired to BiomeGate in TheHush.unity |
