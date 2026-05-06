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

## Status snapshot (2026-05-06)
- Git: INITIALIZED — github.com/Alphiex/lumen-game (main branch, push confirmed)
- Stack: Unity (URP) on mobile + WebGL (URP scales to both)
- Existing assets: Quaternius low-poly nature pack at `unity/Assets/ThirdParty/`
- Existing art reference: 7 captured screenshots + scene.html in `art-reference/`
- Note: `artifacts/commercial-scene/` still present alongside `art-reference/` (git
  deduplicates by content hash; to tidy, a follow-up `git rm -r artifacts/commercial-scene/`
  commit suffices — not blocking)

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

## Next 3 deliverables (in order)
1. **Fox model swap + animations** — Replace capsule placeholder with Quaternius
   animal pack fox mesh (or Mixamo fox FBX). Wire idle/walk/run animation clips to
   FoxAnimator states. Tag fox collider as "Fox". Test NavMesh walk in batch mode.
   Commit + push.
2. **First TestFlight + internal-track Play build** — full biome loop:
   spawn → memory motes scattered → biome gate at end → reaching gate ends
   the loop with simple fade-out. Apple Developer + Google Play Console
   uploads. Mike opens build on physical device, plays through one loop,
   ships feedback in Telegram topic 400.
3. **Ambient audio wiring** — CC0 ambient music loop (Pixabay Music / Free Music
   Archive) + 3-5 spatial SFX (wind, birds, mote-collect chime, gate-reach chime,
   sigh). Wire to AudioSource / DaylightManager. Commit + push.

## Blocked / decisions needed from Mike
- Apple Developer account access for TestFlight upload (needed for deliverable #2)
- Google Play Console access (one-time $25 dev account fee if not yet paid; needed for deliverable #2)
- Music: ambient soundtrack OK to use Royalty-Free / CC0 sources (plan: CC0 via
  Pixabay Music / Free Music Archive), OR Mike has a composer in mind?
- **NavMesh bake**: one-time manual step — open Unity 2022.3 LTS → open TheHush scene
  → Window → AI → Navigation → Bake. Takes ~10 seconds. Required for fox movement.
  Fox will compile and run without it but won't pathfind until baked.
- **Fox model**: Quaternius animal pack may not include a fox. If not found at
  unity/Assets/ThirdParty/Quaternius/, next cycle will fall back to a free Mixamo fox FBX
  (rigged, with idle/walk/run clips). Confirm preference or let cron decide.
- ~~Game name confirmation~~ — RESOLVED: "Lumen", repo = lumen-game, bundle id to be
  set in Unity Player Settings during deliverable #1 Unity setup

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
