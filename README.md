# Lumen

An atmospheric, one-thumb mobile exploration game. You guide a small spirit-fox through fog-shrouded biomes, lighting lanterns and chasing memories that dissolve at the edge of sight. Designed for short, calm sessions on the bus or before bed: hold to glide, tap to leap, drift through wind-borne mist that parts as you move. There is no fail state — only quieter and louder moments of light.

## Tech Stack

- **Engine:** Unity 2022.3 LTS
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Platforms:** iOS and Android
- **Input:** Single-thumb (hold/drag/tap)

## Repository Layout

| Path | Contents |
|------|----------|
| `unity/` | Unity project (Assets, ProjectSettings) |
| `art-reference/` | Three.js prototype scene + reference screenshots — the visual target the Unity build is matching |
| `asset-sources/` | Source asset packs (Quaternius nature pack, etc.) — imported into Unity from here |
| `docs/` | Game design docs and spec |

## Art Reference

`art-reference/scene.html` is a self-contained Three.js scene used as the lighting / atmosphere benchmark. Open it in any modern browser. The accompanying PNGs (`baseline-scene.png`, `flythrough-scene.png`, `hero-reveal-scene.png`, etc.) are the reference frames the Unity URP build is calibrated against.
