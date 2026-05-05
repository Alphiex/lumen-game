# Topic 400 — Game Design Prototype Milestone Status — 2026-04-22

## Done

- **Commercial scene**: Three.js r170 scene with EffectComposer post-processing pipeline. 277 custom ShaderPass definitions + RenderPass + UnrealBloomPass + OutputPass = 280 total passes.
- **Scene source restored**: `scene.html` (947,958 bytes, 22,004 lines) and `capture.mjs` (2,207 bytes) are on disk and functional.
- **Camera presets**: 7 captured screenshots — baseline, crane, dolly, flythrough, flythrough-polish, hero-reveal, and latest-scene.
- **Shader pass categories shipped**: depth-keyed luminance hierarchy, atmospheric haze, film grain, vignette, midtone clarity, color temperature grading, depth separation, hero-path legibility, silhouette grounding, cinematic composition, zone system refinement, frequency separation, perceptual crispness, fog veil path separation, and 250+ more.
- **Unity asset pipeline**: Quaternius nature pack (FBX models + textures) imported under `unity/Assets/ThirdParty/`.
- **Asset sources**: Quaternius low-poly asset references catalogued in `asset-sources/quaternius/`.
- **Capture workflow**: Playwright headless Chromium via `node capture.mjs` (1920x1080 PNG output).

## Latest Milestone — 2026-04-23 00:08 UTC

**Replaced**: Pass 280 — FgMgDepthReadabilityShader (v5) → FinalDepthPolishShader. New three-mechanism architecture:

| Mechanism | Description | Key Parameters |
|-----------|-------------|----------------|
| Depth-gradient micro-contrast | Sobel on depth, 4% darkening at depth boundaries, hero corridor 140% | edgeNorm smoothstep(0.25,1.6), fgMgBand 0.05–0.60 |
| Zone-adaptive tone curve | Per-zone S-curves: FG 3% from pivot 0.46, MG 5% from pivot 0.40 | Separates luminance planes without haze anchoring |
| Unified chroma management | Tree-sky rim 4% dark + combined desaturation: sky 5% + edge 6% + MG 3%, capped at 12% | Single pass replaces separate silhouette + edge paths |

**Architectural changes vs v5**:
- Replaced per-pixel haze anchoring (10% toward 0.34) with zone-adaptive S-curves — more natural tonal separation
- Replaced 2px wide-kernel edge sharpening with 1px Sobel micro-contrast — crisper without ringing
- Merged three separate desaturation paths into unified chroma management with 12% hard cap
- Consistent 88% luminance locks across all mechanisms (v5 had mixed 82%/84%)

**Pipeline pass count**: 280 (unchanged — replaced existing pass)

**Latest render**: `latest-scene.png`
- UTC mtime: 2026-04-23T00:08:03Z
- Byte size: 2,641,009
- SHA-256: `c8d7b3f639b46c382b1c4622f755c7d1c161444bfb3d013a22bb651989e31cfd`
- Resolution: 1920x1080 PNG

### Previous Milestone — 2026-04-22 22:04 UTC

**Tuned**: FgSubjectLiftShader (Pass 29) — tightened shadow bias band, widened edge guard sensitivity, and increased luminance preservation to reduce foreground washout while keeping dark-area readability.

| Parameter | Before | After | Effect |
|-----------|--------|-------|--------|
| Shadow bias onset | smoothstep(0.05,0.45) | smoothstep(0.05,0.38) | Lift ramps up faster — targets darker pixels more precisely |
| Shadow bias ceiling | smoothstep(0.45,0.75) | smoothstep(0.42,0.68) | Lift drops off sooner — excludes near-midtone pixels from lift |
| Edge guard range | smoothstep(0.03,0.10) | smoothstep(0.025,0.09) | Catches finer edges — preserves detail at lower gradient thresholds |
| Luminance preservation | 0.25 | 0.32 | +28% — stronger pull-back to original luminance after lift |

**Pipeline pass count**: 280 (unchanged — no new passes added)

**Latest render**: `latest-scene.png`
- UTC mtime: 2026-04-22T22:04:57Z
- Byte size: 2,617,234
- SHA-256: `610ce13f8195ec2a318140c496ab7155b20c2e2b9604af40059019b565ecac75`
- Resolution: 1920x1080 PNG

### Previous Milestone — 2026-04-22 20:06 UTC

**Tuned**: ForegroundBackgroundSeparationShader (Pass 35) — reduced foreground washout by pulling back warmth lift, widening the FG transition, boosting halo separation, and increasing luminance preservation.

| Parameter | Before | After | Effect |
|-----------|--------|-------|--------|
| FG warmth zone | smoothstep(0.12,0.28) | smoothstep(0.14,0.32) | Gentler transition — less abrupt brightness at FG edge |
| FG warmth strength | 0.015 | 0.010 | -33% warm lift — reduces luminance washout in near objects |
| Halo separation strength | 0.035 | 0.042 | +20% depth-edge halo — compensates for reduced warmth with edge-based separation |
| Luminance preservation | 0.22 | 0.30 | +36% preservation ratio — pulls post-warmth brightness back toward original values |

**Pipeline pass count**: 280 (unchanged — no new passes added)

**Latest render**: `latest-scene.png`
- UTC mtime: 2026-04-22T20:06:55Z
- Byte size: 2,832,891
- SHA-256: `74d2375f562c66b54bfaf2f8369ef808d015c9026439a7de744232cf563d6a60`
- Resolution: 1920x1080 PNG

### Previous Milestone — 2026-04-22 18:05 UTC

**Tuned**: DepthReadabilityContrastShader (Pass 39) — strengthened all three readability mechanisms: Sobel edge darkening, foreground unsharp mask, and midground value separation.

| Parameter | Before | After | Effect |
|-----------|--------|-------|--------|
| Edge darkening strength | 0.18 | 0.22 | +22% Sobel depth-edge darkening for crisper depth boundaries |
| Unsharp mask amount | 0.35 | 0.42 | +20% foreground clarity sharpening |
| MG value zone onset | smoothstep(0.15,0.30) | smoothstep(0.12,0.26) | Zone starts closer — more near-MG pixels get value push |
| MG value zone ceiling | smoothstep(0.50,0.65) | smoothstep(0.55,0.70) | Zone extends deeper — wider midground coverage |
| MG value shift strength | 0.06 | 0.08 | +33% luminance push toward screen midtone for MG separation |

**Pipeline pass count**: 280 (unchanged — no new passes added)

**Latest render**: `latest-scene.png`
- UTC mtime: 2026-04-22T18:05:52Z
- Byte size: 2,737,352
- SHA-256: `0c115a25bdff1ba129b46310568aa0d0634dbcfa79ba8ac5cf21d9f7e27c6d10`
- Resolution: 1920x1080 PNG

### Previous Milestone — 2026-04-22 16:07 UTC

**Tuned**: SilhouetteGroundingShader (Pass 30) — strengthened all three FG/BG separation mechanisms: silhouette edge darkening, contact-shadow grounding, and midground rim light.

| Parameter | Before | After | Effect |
|-----------|--------|-------|--------|
| Silhouette zone | smoothstep(0.50,0.70) | smoothstep(0.55,0.75) | Extended silhouette treatment 10% deeper into scene |
| Silhouette strength | 0.06 | 0.075 | +25% edge contour darkening for crisper tree/foliage outlines |
| Contact shadow strength | 0.04 | 0.052 | +30% foreground object grounding at base |
| MG rim zone onset | smoothstep(0.20,0.30) | smoothstep(0.18,0.28) | Rim light starts 10% closer — more objects get backlight |
| MG rim zone ceiling | smoothstep(0.50,0.60) | smoothstep(0.55,0.65) | Rim light extends 10% deeper into midground |
| Rim strength | 0.10 | 0.12 | +20% warm rim-light intensity for MG separation |

**Pipeline pass count**: 280 (unchanged — no new passes added)

**Latest render**: `latest-scene.png`
- UTC mtime: 2026-04-22T16:07:30Z
- Byte size: 2,934,365
- SHA-256: `19b139b90eb00929f632d7dd6e87d3ee41738989e301a311c939ced07a805fea`
- Resolution: 1920x1080 PNG

### Previous Milestone — 2026-04-22 14:04 UTC

**Tuned**: AtmosphericHazeShader (Pass 8) — strengthened aerial perspective haze to push midground/background further back from foreground.

| Parameter | Before | After | Effect |
|-----------|--------|-------|--------|
| `strength` | 0.12 | 0.15 | +25% haze intensity for deeper aerial perspective |
| `lumFloor` | 0.55 | 0.50 | Lowered threshold — haze reaches more midtone pixels |
| `vertBias` | 0.35 | 0.42 | +20% vertical position weighting — stronger top-of-frame recession |

**Pipeline pass count**: 280 (unchanged — no new passes added)

**Latest render**: `latest-scene.png`
- UTC mtime: 2026-04-22T14:04:36Z
- Byte size: 2,582,602
- SHA-256: `ec7e013e2b856428e584a2befbfaa73a3a68036b99617a89e235dfece322ac21`
- Resolution: 1920x1080 PNG

### Previous Milestone — 2026-04-22 12:04 UTC

**Tuned**: FgSubjectLiftShader (Pass 29) — increased foreground subject luminance lift and extended coverage ceiling for stronger subject/ground separation.

| Parameter | Before | After | Effect |
|-----------|--------|-------|--------|
| `liftAmt` | 0.045 | 0.058 | +29% additive luminance lift for FG subjects |
| `fgCeil` | 0.42 | 0.48 | Extended FG treatment 14% higher on screen |

**Pipeline pass count**: 280 (unchanged — no new passes added)

**Latest render**: `latest-scene.png`
- UTC mtime: 2026-04-22T12:04:37Z
- Byte size: 2,388,177
- SHA-256: `71dab27d4c9527b8cc96a39a8f5f03e73eba8bd52def40ac3854dbfc20f09250`
- Resolution: 1920x1080 PNG

### Previous Milestone — 2026-04-22 10:05 UTC

**Tuned**: MidtoneClarityShader (Pass 10) — widened midtone contrast band and increased strength for subject/background separation without blowing highlights.

| Parameter | Before | After | Effect |
|-----------|--------|-------|--------|
| `strength` | 0.28 | 0.34 | +21% local detail push in midtone band |
| `lumLow` | 0.18 | 0.14 | Lowered shadow boundary — more dark-midtones included |
| `lumHigh` | 0.72 | 0.78 | Raised highlight boundary — more bright-midtones included |

**Pipeline pass count**: 280 (unchanged — no new passes added)

**Latest render**: `latest-scene.png`
- UTC mtime: 2026-04-22T10:05:12Z
- Byte size: 2,682,830
- SHA-256: `854012ce9eafe27e0490b2f54753a92367963e77f50267049fc7c1b828e42e60`
- Resolution: 1920x1080 PNG

### Previous Milestone — 2026-04-22 08:05 UTC

**Tuned**: HeroPathLegibilityShader (Pass 32) — strengthened hero-path corridor detail, aerial perspective, and edge framing.

| Parameter | Before | After | Effect |
|-----------|--------|-------|--------|
| Path detail push | 0.22 | 0.28 | +27% local contrast in hero path corridor |
| Warm-cool gradient | 0.018 | 0.024 | +33% aerial perspective warm/cool separation |
| Path-edge darkening | 0.025 | 0.035 | +40% corridor edge framing depth |

### Previous Milestone — 2026-04-22 06:11 UTC

**Tuned**: DepthSeparationShader (Pass 27) — improved hero-path readability and foreground/midground separation.

| Parameter | Before | After | Effect |
|-----------|--------|-------|--------|
| `fgContrastBoost` | 0.22 | 0.30 | +36% foreground midtone contrast pop |
| `bgDesatStrength` | 0.18 | 0.25 | +39% background desaturation for depth recession |
| `bgBlurRadius` | 1.5 | 2.0 | +33% background softness for perceptual depth |
| `fgZone` | 0.40 | 0.46 | Extended FG treatment 15% higher to cover hero path |
| `bgZone` | 0.65 | 0.62 | Brought BG onset 5% lower for tighter MG band |

## Queued

1. **Continue shader tuning** — further passes targeting color grading refinement and final output polish.
2. **Unity scene integration** — port Three.js commercial scene lighting/materials into Unity project using imported Quaternius assets.
