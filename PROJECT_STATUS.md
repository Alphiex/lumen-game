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

## Status snapshot (2026-05-05)
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

## Next 3 deliverables (in order)
1. **First playable Unity scene (mobile)** — Unity 2022.3 LTS project properly
   initialized (ProjectSettings, Packages with URP, Input System). Third-person fox
   controller (use Unity Starter Asset Third Person Controller, swap character mesh
   for a low-poly fox from Quaternius or free Mixamo asset). One-thumb touch input:
   hold-anywhere-on-screen → fox runs toward finger; release → fox stops.
   Quaternius nature props placed in a small fog-shrouded scene. URP volume
   with bloom + color grading + depth of field + vignette + atmospheric fog
   tuned to match the art-reference cinematic feel. iOS + Android build
   targets verified. Commit + push to main.
2. **First TestFlight + internal-track Play build** — full biome loop:
   spawn → memory motes scattered → biome gate at end → reaching gate ends
   the loop with simple fade-out. Apple Developer + Google Play Console
   uploads. Mike opens build on physical device, plays through one loop,
   ships feedback in Telegram topic 400.

## Blocked / decisions needed from Mike
- Apple Developer account access for TestFlight upload (needed for deliverable #2)
- Google Play Console access (one-time $25 dev account fee if not yet paid; needed for deliverable #2)
- Music: ambient soundtrack OK to use Royalty-Free / CC0 sources (plan: CC0 via
  Pixabay Music / Free Music Archive), OR Mike has a composer in mind?
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
