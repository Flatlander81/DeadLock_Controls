# Hephaestus Implementation Guide - Phase 6: Polish & VFX

---

## Document Overview

**Phase 6** adds visual effects, audio, and polish to transform the functional prototype into an engaging experience.

**Prerequisites**: Phase 5 Complete (342 tests passing)

**Estimated Time**: 25-35 hours

**New Tests**: ~35 automated tests

---

## Phase 6 Architecture Summary

```
┌─────────────────────────────────────────────────────────────┐
│                      POLISH SYSTEMS                          │
├─────────────────────────────────────────────────────────────┤
│  [Weapon VFX] ─── Muzzle flash, tracers, impacts            │
│  [Damage VFX] ─── Explosions, debris, hull damage           │
│  [Heat VFX] ─── Ship glow, venting, distortion              │
│  [Shield VFX] ─── Bubble visualization, hit ripples         │
│  [Audio System] ─── Weapons, impacts, ambience, music       │
│  [Screen Effects] ─── Shake, flash, post-processing         │
│  [UI Polish] ─── Animations, transitions, feedback          │
└─────────────────────────────────────────────────────────────┘
```

---

## GDD Reference

**Heat Visualization (from GDD):**
| Heat Range | Visual Effect |
|------------|---------------|
| 0-59 | Normal (gray/white) |
| 60-79 | Slight yellow glow on vents |
| 80-99 | Orange glow, heat distortion |
| 100-119 | Red glow, heavy distortion, sparks |
| 120+ | Flashing red, fire effects |

**Weapon Spin-Up Times:**
| Weapon/Ability | Spin-Up |
|----------------|---------|
| Rail Gun | 0.2s |
| Newtonian Cannon | 0.5s |
| Missile Battery | 0.4s |
| Torpedo Launcher | 1.0s |

---

## Step 6.0: Establish Phase 6 Standards

**Time**: 15 minutes  
**Prerequisites**: Phase 5 complete

### CLAUDE CODE PROMPT 6.0

```
CONTEXT:
Phase 5 complete with 342 tests passing. Beginning Phase 6: Polish & VFX.

OBJECTIVE:
Add Phase 6 standards section to CLAUDE.md.

TASK:
Append the following section to CLAUDE.md:

---

## Phase 6: Polish & VFX Standards

### Visual Design Principles
- Effects should enhance readability, not obscure gameplay
- Color-coding must be consistent with UI (group colors, heat colors, etc.)
- Performance matters: Use particle pooling, LOD for effects
- Effects should have clear start/middle/end states
- Everything should feel "weighty" - no instant transitions

### Audio Design Principles
- Every action should have audio feedback
- Spatial audio for 3D positioning
- Layer: Ambience (constant) + Events (triggered) + Music (adaptive)
- Audio should never be jarring or too loud
- Provide volume controls per category

### Performance Targets
- Maintain 60 FPS during heavy combat (10+ ships, many projectiles)
- Particle system budgets: Max 500 particles per system
- Audio: Max 32 simultaneous sounds
- Use object pooling for all VFX

### Folder Structure for Phase 6
```
Assets/Scripts/VFX/              # VFX controller scripts
Assets/Scripts/Audio/            # Audio system scripts
Assets/Scripts/Polish/           # Screen effects, juice
Assets/VFX/                      # Particle systems, materials
Assets/Audio/                    # Audio clips, mixers
Assets/Editor/Polish/            # Editor automation
Assets/Tests/PlayModeTests/Polish/  # Tests
```

### VFX Reference (from GDD)

**Weapon Effects:**
- Rail Gun: Bright flash, fast tracer, small impact spark
- Newtonian Cannon: Power-up glow, slower visible shell, explosion on hit
- Missile: Contrail, homing trail, medium explosion
- Torpedo: Heavy contrail, slower trail, large explosion

**Damage Effects:**
- Shield hit: Blue ripple on shield surface
- Armor hit: Sparks, metallic debris
- Structure hit: Debris, hull darkening
- Critical hit: Larger explosion, system sparks
- Section breach: Major structural failure, fire
- Ship death: Massive explosion, debris field

**Heat Effects:**
- 0-59: Normal ship appearance
- 60-79: Yellow glow on radiator vents
- 80-99: Orange glow, heat shimmer effect
- 100-119: Red glow, sparks from vents, distortion
- 120+: Flashing red, fire on hull, heavy distortion

---

Create the folder structure listed above if directories don't exist.

STATUS UPDATE:
- ✅ Step 6.0 Complete - Phase 6 Standards Established
- 📁 Modified: CLAUDE.md
- 📁 Created: Folder structure for Phase 6
- ⏭️ Next: Step 6.1 - Weapon Visual Effects
```

### VERIFICATION 6.0

- [ ] CLAUDE.md contains Phase 6 standards section
- [ ] All folder paths exist

---

## Step 6.1: Weapon Visual Effects

**Time**: 5-6 hours  
**Prerequisites**: Step 6.0 complete

### CLAUDE CODE PROMPT 6.1

```
CONTEXT:
Phase 6 standards established. Beginning weapon VFX implementation.

Each weapon type needs distinct visual identity: muzzle flash, projectile trail, impact effect.

OBJECTIVE:
Create visual effects for all weapon types.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 6 section.

ARCHITECTURE:

1. WeaponVFXManager.cs (singleton)
   - Manages all weapon VFX
   - Object pooling for particle systems
   - Methods:
     * void PlayMuzzleFlash(WeaponType type, Vector3 position, Quaternion rotation)
     * void PlayImpact(WeaponType type, Vector3 position, Vector3 normal)
     * ParticleSystem GetPooledEffect(string effectName)
     * void ReturnToPool(ParticleSystem effect)

2. ProjectileTrailController.cs
   - Attached to projectiles
   - Manages trail renderer or particle trail
   - Different trail types per weapon:
     * Rail Gun: Thin, fast-fading line
     * Newtonian Cannon: Thicker, slower fade
     * Missile: Smoke contrail with particles
     * Torpedo: Heavy engine glow trail

3. Weapon-Specific Effects:

   **Rail Gun:**
   - Muzzle: Bright blue-white flash, quick (0.1s)
   - Trail: Thin cyan tracer line
   - Impact: Small blue spark burst
   
   **Newtonian Cannon:**
   - Muzzle: Orange charge-up glow (0.5s), then flash
   - Trail: Visible shell with heat distortion
   - Impact: Orange explosion, debris particles
   
   **Missile Battery:**
   - Muzzle: Small smoke puff, launch flash
   - Trail: White smoke contrail, engine glow
   - Impact: Yellow-orange explosion, shrapnel
   
   **Torpedo:**
   - Muzzle: Heavy launch blast, smoke
   - Trail: Thick blue engine trail, pulsing glow
   - Impact: Large blue-white explosion, debris field

4. Spin-Up Visual Effects:
   - WeaponSpinUpVFX.cs component on weapons
   - Glow intensifies during spin-up time
   - Energy gathering particle effect
   - Integrate with existing spin-up timing

5. Modify WeaponSystem.cs (base class)
   - Add: OnWeaponFired event
   - Add: OnWeaponSpinUpStart event
   - VFX manager subscribes to these events

6. Modify Projectile classes
   - Add trail reference
   - Initialize trail on spawn
   - Clean up trail on destroy/pool

EDITOR AUTOMATION:

7. WeaponVFXSetupEditor.cs
   - Menu: "Hephaestus/Setup/Create Weapon VFX Prefabs"
     * Creates particle system prefabs for each weapon
     * Configures pooling quantities
   - Menu: "Hephaestus/Setup/Assign VFX to Weapons"
     * Attaches VFX references to weapon prefabs

8. WeaponVFXTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Weapon VFX Test Scene"
   - Creates:
     * All weapon types ready to fire
     * Target dummy
     * VFX toggle controls
     * Slow-motion option

9. WeaponVFXTestController.cs
   - Fire buttons per weapon type
   - Slow motion slider (0.1x to 1x)
   - VFX quality toggle (Low/Medium/High)
   - Particle count display
   - Performance metrics

UNIT TESTS (WeaponVFXTests.cs):

1. Test_MuzzleFlashPlays - Effect instantiates
2. Test_TrailAttachesToProjectile - Trail visible
3. Test_ImpactPlaysOnHit - Impact effect triggers
4. Test_VFXPooling - Effects reused, not destroyed
5. Test_SpinUpGlowVisible - Glow during charge
6. Test_DifferentEffectsPerWeapon - Visual distinction
7. Test_PerformanceUnder500Particles - Budget respected
8. Test_VFXCleanupOnProjectileDestroy - No orphaned effects

STATUS UPDATE:
- ✅ Step 6.1 Complete - Weapon Visual Effects
- List new/modified files
- 📁 Created: VFX prefabs for 4 weapon types
- 🧪 Unit Tests: 8/8 passing
- 🧪 Total Tests: 350/350 passing
- ⏭️ Next: Step 6.2 - Damage Visual Effects
```

### VERIFICATION 6.1

1. **Run Setup**: Menu → Hephaestus → Setup → Create Weapon VFX Prefabs
2. **Run Setup**: Menu → Hephaestus → Testing → Create Weapon VFX Test Scene
3. **Run Tests**: Verify 350/350 passing
4. **Play Mode Verification**:
   - [ ] Rail Gun: Blue flash, fast tracer, spark impact
   - [ ] Cannon: Orange charge, visible shell, explosion
   - [ ] Missile: Smoke trail, explosion
   - [ ] Torpedo: Heavy trail, large explosion
   - [ ] Spin-up glow visible
   - [ ] Performance stable (check particle count)

---

## Step 6.2: Damage Visual Effects

**Time**: 4-5 hours  
**Prerequisites**: Step 6.1 complete (350 tests passing)

### CLAUDE CODE PROMPT 6.2

```
CONTEXT:
Step 6.1 complete. Weapon VFX functional.

Implementing damage visualization - shield hits, armor impacts, hull damage, explosions.

OBJECTIVE:
Create visual feedback for all damage types.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 6 section.

ARCHITECTURE:

1. DamageVFXManager.cs (singleton)
   - Manages damage visualization
   - Methods:
     * void PlayShieldHit(Vector3 position, float damage)
     * void PlayArmorHit(Vector3 position, Vector3 normal, float damage)
     * void PlayStructureHit(Vector3 position, float damage)
     * void PlayCriticalHit(Vector3 position, SystemType system)
     * void PlaySectionBreach(ShipSection section)
     * void PlayShipDestruction(Ship ship)

2. ShieldVFXController.cs
   - Attached to ships with shields
   - Properties:
     * shieldMesh: MeshRenderer (bubble around ship)
     * shieldMaterial: Material (with ripple shader)
   - Methods:
     * void ShowShieldBubble(bool visible)
     * void PlayHitRipple(Vector3 hitPoint, float intensity)
     * void PlayShieldCollapse() - Dramatic effect when shields hit 0

3. HullDamageController.cs
   - Tracks visible damage on ship hull
   - Per-section damage decals/darkening
   - Methods:
     * void UpdateSectionDamage(SectionType section, float percentage)
     * void ShowBreach(SectionType section)
     * void AddDamageDecal(Vector3 position, float size)

4. Damage-Specific Effects:

   **Shield Hit:**
   - Blue ripple emanating from hit point
   - Shield bubble briefly visible
   - Intensity scales with damage
   
   **Armor Hit:**
   - Metallic sparks
   - Small debris particles
   - Scorch mark decal
   
   **Structure Hit:**
   - Larger debris chunks
   - Fire particles (brief)
   - Hull material darkens in area
   
   **Critical Hit:**
   - Localized explosion
   - System-specific sparks (electrical for weapons, etc.)
   - Warning particle burst
   
   **Section Breach:**
   - Major explosion
   - Structural debris
   - Persistent fire/smoke in breached area
   - Hull visibly damaged
   
   **Ship Destruction:**
   - Massive explosion sequence
   - Ship breaks apart (if possible)
   - Debris field spawns
   - Shockwave effect

5. Subscribe to damage events:
   - DamageRouter.OnDamageApplied
   - ShipSection.OnSectionBreached
   - Ship.OnShipDestroyed
   - MountedSystem.OnSystemDamaged

EDITOR AUTOMATION:

6. DamageVFXSetupEditor.cs
   - Menu: "Hephaestus/Setup/Create Damage VFX Prefabs"
   - Menu: "Hephaestus/Setup/Add Damage VFX to Ship"

7. DamageVFXTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Damage VFX Test Scene"
   - Creates:
     * Ship with all damage VFX components
     * Damage trigger buttons
     * Cumulative damage display

8. DamageVFXTestController.cs
   - Buttons:
     * "Shield Hit (Light/Medium/Heavy)"
     * "Armor Hit"
     * "Structure Hit"
     * "Critical Hit (per system)"
     * "Breach Section (dropdown)"
     * "Destroy Ship"
   - Damage accumulation visible
   - Reset button

UNIT TESTS (DamageVFXTests.cs):

1. Test_ShieldRipplePlays - Ripple on shield hit
2. Test_ShieldCollapseEffect - Dramatic collapse
3. Test_ArmorSparkEffect - Sparks on armor hit
4. Test_StructureDebrisEffect - Debris on structure
5. Test_CriticalHitExplosion - Explosion plays
6. Test_SectionBreachFire - Persistent fire
7. Test_ShipDestructionSequence - Full explosion
8. Test_DamageDecalsAccumulate - Multiple hits show

STATUS UPDATE:
- ✅ Step 6.2 Complete - Damage Visual Effects
- List new/modified files
- 🧪 Unit Tests: 8/8 passing
- 🧪 Total Tests: 358/358 passing
- ⏭️ Next: Step 6.3 - Heat Visualization
```

### VERIFICATION 6.2

1. **Run Setup**: Menu → Hephaestus → Testing → Create Damage VFX Test Scene
2. **Run Tests**: Verify 358/358 passing
3. **Play Mode Verification**:
   - [ ] Shield hit shows blue ripple
   - [ ] Shield collapse is dramatic
   - [ ] Armor hits show sparks
   - [ ] Structure hits show debris
   - [ ] Critical hits have explosion
   - [ ] Breached sections have fire
   - [ ] Ship destruction is spectacular

---

## Step 6.3: Heat Visualization

**Time**: 4-5 hours  
**Prerequisites**: Step 6.2 complete (358 tests passing)

### CLAUDE CODE PROMPT 6.3

```
CONTEXT:
Step 6.2 complete. Damage VFX functional.

Implementing heat visualization - the ship should visually show its heat state.

REFERENCE (from GDD):
- 0-59: Normal
- 60-79: Yellow glow on vents
- 80-99: Orange glow, heat shimmer
- 100-119: Red glow, sparks, distortion
- 120+: Flashing red, fire, heavy distortion

OBJECTIVE:
Create visual heat feedback on ship model.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 6 section.

ARCHITECTURE:

1. HeatVisualizationController.cs (MonoBehaviour)
   - Attached to ships
   - Properties:
     * heatManager: HeatManager reference
     * shipMaterials: List<Material> (materials to affect)
     * ventPositions: List<Transform> (where vents/radiators are)
     * currentHeatTier: HeatTier
   - Methods:
     * void UpdateVisualization() - Called when heat changes
     * void SetHeatTier(HeatTier tier) - Apply tier-specific effects
     * void TransitionBetweenTiers(HeatTier from, HeatTier to)

2. Heat Tier Effects:

   **Safe (0-59):**
   - Normal ship materials
   - No glow
   - No particles
   
   **Minor (60-79):**
   - Slight yellow emission on vent areas
   - Subtle heat shimmer near radiators
   - Occasional steam particle
   
   **Moderate (80-99):**
   - Orange glow on vents and edges
   - Visible heat distortion shader
   - Regular steam/heat particles
   - Material color shift toward warm
   
   **Severe (100-119):**
   - Red glow spreading across hull
   - Heavy heat distortion
   - Spark particles from vents
   - Hull material stress patterns
   - Warning particle bursts
   
   **Critical/Catastrophic (120+):**
   - Flashing red glow
   - Fire particles on hull
   - Extreme distortion
   - Smoke trailing
   - Audible sizzling (integrate with audio)

3. HeatShaderController.cs
   - Controls heat-related shader parameters
   - Properties:
     * emissionColor: Color (changes with heat)
     * emissionIntensity: float
     * distortionStrength: float
   - Smooth interpolation between states

4. HeatParticleController.cs
   - Manages heat-related particle effects
   - Steam venting
   - Sparks
   - Fire
   - Smoke
   - Scales with heat tier

5. Integrate with HeatManager:
   - Subscribe to heat change events
   - Update visualization in real-time
   - Smooth transitions between tiers

EDITOR AUTOMATION:

6. HeatVFXSetupEditor.cs
   - Menu: "Hephaestus/Setup/Create Heat VFX Materials"
   - Menu: "Hephaestus/Setup/Add Heat Visualization to Ship"
     * Adds HeatVisualizationController
     * Identifies vent positions
     * Assigns materials

7. HeatVFXTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Heat VFX Test Scene"
   - Creates:
     * Ship with heat visualization
     * Heat slider control
     * Tier indicator

8. HeatVFXTestController.cs
   - Heat slider (0-180)
   - Tier display
   - "Cycle Through Tiers" button
   - Individual effect toggles
   - Material property display

UNIT TESTS (HeatVFXTests.cs):

1. Test_NormalHeatNoGlow - Safe tier looks normal
2. Test_MinorHeatYellowGlow - Yellow at 60+
3. Test_ModerateHeatOrangeDistortion - Orange at 80+
4. Test_SevereHeatRedSparks - Red and sparks at 100+
5. Test_CriticalHeatFireEffect - Fire at 120+
6. Test_HeatTierTransition - Smooth transitions
7. Test_HeatParticlesScale - More particles at higher heat
8. Test_HeatVisualizationPerformance - Stays within budget

STATUS UPDATE:
- ✅ Step 6.3 Complete - Heat Visualization
- List new/modified files
- 🧪 Unit Tests: 8/8 passing
- 🧪 Total Tests: 366/366 passing
- ⏭️ Next: Step 6.4 - Audio System
```

### VERIFICATION 6.3

1. **Run Setup**: Menu → Hephaestus → Testing → Create Heat VFX Test Scene
2. **Run Tests**: Verify 366/366 passing
3. **Play Mode Verification**:
   - [ ] Normal (0-59): Ship looks normal
   - [ ] Minor (60-79): Yellow glow on vents
   - [ ] Moderate (80-99): Orange glow, shimmer visible
   - [ ] Severe (100-119): Red glow, sparks
   - [ ] Critical (120+): Flashing, fire, smoke
   - [ ] Transitions are smooth

---

## Step 6.4: Audio System

**Time**: 5-6 hours  
**Prerequisites**: Step 6.3 complete (366 tests passing)

### CLAUDE CODE PROMPT 6.4

```
CONTEXT:
Step 6.3 complete. Heat VFX functional.

Implementing audio system - weapons, impacts, damage, ambience, music.

OBJECTIVE:
Create comprehensive audio system for all game events.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 6 section.

ARCHITECTURE:

1. AudioManager.cs (singleton)
   - Central audio management
   - Properties:
     * sfxVolume, musicVolume, ambienceVolume: float
     * audioMixer: AudioMixer
     * sfxSources: Pool of AudioSources
   - Methods:
     * void PlaySFX(AudioClip clip, Vector3 position, float volume = 1)
     * void PlaySFX2D(AudioClip clip, float volume = 1) - Non-positional
     * void PlayMusic(AudioClip music, bool loop = true)
     * void PlayAmbience(AudioClip ambience)
     * void StopMusic()
     * void SetVolume(AudioCategory category, float volume)

2. WeaponAudioController.cs
   - Per-weapon audio
   - Sounds:
     * Spin-up sound (charge)
     * Fire sound
     * Projectile travel sound (looping for missiles/torpedoes)
     * Impact sound
   - Different sounds per weapon type

3. DamageAudioController.cs
   - Damage event sounds
   - Sounds:
     * Shield hit (energy impact)
     * Armor hit (metallic clang)
     * Structure hit (crunch/tear)
     * Critical hit (explosion + alarm)
     * Section breach (large explosion)
     * Ship destruction (massive explosion sequence)

4. HeatAudioController.cs
   - Heat-related sounds
   - Sounds:
     * Venting steam (when cooling)
     * Warning beeps per tier
     * Emergency alarm at critical
     * Fire crackling at catastrophic

5. UIAudioController.cs
   - UI interaction sounds
   - Sounds:
     * Button click
     * Selection change
     * Ability activation
     * Error/blocked action
     * Victory fanfare
     * Defeat sound

6. AmbienceController.cs
   - Background audio
   - Space ambience (low hum, distant effects)
   - Combat intensity layer (increases during action)
   - Engine hum (varies with ship speed)

7. MusicController.cs
   - Adaptive music system
   - Tracks:
     * Menu theme
     * Combat - calm
     * Combat - intense
     * Victory
     * Defeat
   - Crossfade between tracks
   - Intensity based on combat state

8. Spatial Audio Setup:
   - 3D audio for weapon fire, impacts
   - Distance attenuation
   - Listener on camera

EDITOR AUTOMATION:

9. AudioSetupEditor.cs
   - Menu: "Hephaestus/Setup/Create Audio Manager"
   - Menu: "Hephaestus/Setup/Create Audio Mixer"
   - Menu: "Hephaestus/Setup/Generate Placeholder Audio"
     * Creates placeholder sounds for testing

10. AudioTestSetup.cs
    - Menu: "Hephaestus/Testing/Create Audio Test Scene"
    - Creates:
      * Audio manager
      * Sound trigger buttons
      * Volume controls
      * 3D audio test positions

11. AudioTestController.cs
    - Category volume sliders
    - Sound trigger buttons (all weapon types, damage types)
    - Music track selector
    - Ambience toggle
    - Active sounds counter

UNIT TESTS (AudioSystemTests.cs):

1. Test_SFXPlays - Sound effect plays
2. Test_3DAudioPositional - Volume varies with distance
3. Test_MusicPlays - Music starts
4. Test_MusicCrossfade - Smooth transition
5. Test_VolumeControl - Volume adjustable
6. Test_MaxSimultaneousSounds - Respects limit
7. Test_AudioPooling - Sources reused
8. Test_AudioCleanup - Sounds stop when needed

STATUS UPDATE:
- ✅ Step 6.4 Complete - Audio System
- List new/modified files
- 🧪 Unit Tests: 8/8 passing
- 🧪 Total Tests: 374/374 passing
- ⏭️ Next: Step 6.5 - Screen Effects
```

### VERIFICATION 6.4

1. **Run Setup**: Menu → Hephaestus → Setup → Create Audio Manager
2. **Run Setup**: Menu → Hephaestus → Testing → Create Audio Test Scene
3. **Run Tests**: Verify 374/374 passing
4. **Play Mode Verification**:
   - [ ] Weapon sounds play (distinct per type)
   - [ ] Impact sounds vary by damage type
   - [ ] 3D positioning works
   - [ ] Music plays and crossfades
   - [ ] Volume controls work
   - [ ] No audio overload in combat

---

## Step 6.5: Screen Effects

**Time**: 3-4 hours  
**Prerequisites**: Step 6.4 complete (374 tests passing)

### CLAUDE CODE PROMPT 6.5

```
CONTEXT:
Step 6.4 complete. Audio system functional.

Implementing screen effects - shake, flash, post-processing feedback.

OBJECTIVE:
Create screen-level feedback effects.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 6 section.

ARCHITECTURE:

1. ScreenEffectsManager.cs (singleton)
   - Manages camera/screen effects
   - Methods:
     * void ShakeCamera(float intensity, float duration)
     * void FlashScreen(Color color, float duration)
     * void SetVignette(float intensity)
     * void SetChromaticAberration(float intensity)
     * void PulseEffect(EffectType type)

2. CameraShakeController.cs
   - Attached to main camera
   - Properties:
     * shakeIntensity: float
     * shakeDuration: float
     * shakeDecay: float
   - Methods:
     * void Shake(float intensity, float duration)
     * void Update() - Apply shake offset
   - Different shake profiles:
     * Light (weapon fire)
     * Medium (hit taken)
     * Heavy (critical/breach)
     * Extreme (ship destruction)

3. ScreenFlashController.cs
   - Full-screen color flash
   - Uses UI Image overlay or post-processing
   - Flash types:
     * White flash (weapon fire)
     * Red flash (damage taken)
     * Blue flash (shield hit)
     * Orange flash (explosion)

4. DamagePostProcessing.cs
   - Post-processing effects for damage feedback
   - Effects:
     * Vignette intensifies as hull drops
     * Chromatic aberration on hits
     * Desaturation at low health
     * Screen crack overlay at critical

5. HeatPostProcessing.cs
   - Heat-related screen effects
   - Effects:
     * Heat distortion overlay
     * Red tint at high heat
     * Warning vignette at critical

6. Event Integration:
   - Subscribe to:
     * WeaponSystem.OnFired → Light shake
     * DamageRouter.OnDamageReceived → Medium shake + flash
     * ShipSection.OnBreach → Heavy shake
     * Ship.OnDestroyed → Extreme shake
     * HeatManager.OnTierChanged → Heat post-processing

7. Effect Stacking:
   - Multiple effects can combine
   - Limits to prevent overdoing it
   - Smooth recovery to normal

EDITOR AUTOMATION:

8. ScreenEffectsSetupEditor.cs
   - Menu: "Hephaestus/Setup/Add Screen Effects"
     * Adds post-processing volume
     * Configures effects

9. ScreenEffectsTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Screen Effects Test Scene"
   - Creates:
     * Camera with effects
     * Effect trigger buttons
     * Intensity sliders

10. ScreenEffectsTestController.cs
    - Shake buttons (Light/Medium/Heavy/Extreme)
    - Flash buttons (per color)
    - Damage percentage slider (vignette test)
    - Heat tier selector (heat effects test)
    - All effects toggle

UNIT TESTS (ScreenEffectsTests.cs):

1. Test_CameraShakePlays - Shake applies to camera
2. Test_ShakeDecays - Returns to normal
3. Test_ScreenFlashPlays - Flash visible
4. Test_FlashFades - Flash fades out
5. Test_VignetteScalesWithDamage - Intensity matches
6. Test_HeatDistortionAtHighHeat - Effect at 100+
7. Test_EffectsStack - Multiple effects combine
8. Test_EffectsToggleable - Can disable

STATUS UPDATE:
- ✅ Step 6.5 Complete - Screen Effects
- List new/modified files
- 🧪 Unit Tests: 8/8 passing
- 🧪 Total Tests: 382/382 passing
- ⏭️ Next: Step 6.6 - UI Polish
```

### VERIFICATION 6.5

1. **Run Setup**: Menu → Hephaestus → Testing → Create Screen Effects Test Scene
2. **Run Tests**: Verify 382/382 passing
3. **Play Mode Verification**:
   - [ ] Camera shake works (various intensities)
   - [ ] Screen flash works (various colors)
   - [ ] Vignette increases with damage
   - [ ] Heat effects show at high heat
   - [ ] Effects combine properly
   - [ ] Can toggle effects off

---

## Step 6.6: UI Polish

**Time**: 3-4 hours  
**Prerequisites**: Step 6.5 complete (382 tests passing)

### CLAUDE CODE PROMPT 6.6

```
CONTEXT:
Step 6.5 complete. Screen effects functional.

Implementing UI polish - animations, transitions, micro-feedback.

OBJECTIVE:
Add polish to all UI elements.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 6 section.

ARCHITECTURE:

1. UIAnimationController.cs
   - Manages UI animations
   - Methods:
     * void AnimateIn(RectTransform element, AnimationType type)
     * void AnimateOut(RectTransform element, AnimationType type)
     * void Pulse(RectTransform element)
     * void Shake(RectTransform element)

2. AnimationType enum:
   - FadeIn, FadeOut
   - SlideFromLeft, SlideFromRight
   - ScaleUp, ScaleDown
   - Bounce

3. ButtonFeedback.cs
   - Enhanced button interactions
   - Effects:
     * Hover: Scale up slightly, color shift
     * Click: Quick scale down then up (punch)
     * Disabled: Grayed out, no hover effect

4. BarAnimations.cs
   - Smooth bar value changes
   - Shield/Health/Heat bars animate to new values
   - Flash on significant change
   - Color pulse at thresholds

5. PanelTransitions.cs
   - Smooth panel show/hide
   - Staggered element animation
   - Context-sensitive positioning

6. TooltipSystem.cs (if not exists)
   - Hover tooltips for UI elements
   - Smooth fade in/out
   - Smart positioning (stays on screen)
   - Rich content support

7. NotificationSystem.cs
   - Non-blocking notifications
   - Types: Info, Warning, Critical
   - Stack/queue management
   - Auto-dismiss with animation

8. CombatTextController.cs
   - Floating damage numbers (if enabled)
   - Color-coded by damage type
   - Rise and fade animation
   - Optional (player preference)

9. UIStateTransitions.cs
   - Smooth transitions between UI states
   - Selection → targeting → firing flow
   - Phase indicator animation

EDITOR AUTOMATION:

10. UIPolishSetupEditor.cs
    - Menu: "Hephaestus/Setup/Add UI Animations"
    - Menu: "Hephaestus/Setup/Configure UI Theme"

11. UIPolishTestSetup.cs
    - Menu: "Hephaestus/Testing/Create UI Polish Test Scene"
    - Creates:
      * Sample UI with all elements
      * Animation trigger buttons
      * State transition demo

12. UIPolishTestController.cs
    - Panel animation buttons
    - Bar value sliders
    - Notification trigger
    - Combat text toggle
    - State flow demo

UNIT TESTS (UIPolishTests.cs):

1. Test_PanelAnimationPlays - Animation triggers
2. Test_BarValueAnimates - Smooth transitions
3. Test_ButtonFeedbackOnHover - Visual response
4. Test_NotificationAppears - Shows correctly
5. Test_NotificationAutoDismisses - Goes away
6. Test_TooltipPositions - Stays on screen

STATUS UPDATE:
- ✅ Step 6.6 Complete - UI Polish
- List new/modified files
- 🧪 Unit Tests: 6/6 passing
- 🧪 Total Tests: 388/388 passing
- ⏭️ Next: Step 6.7 - Phase 6 Integration Testing
```

### VERIFICATION 6.6

1. **Run Setup**: Menu → Hephaestus → Testing → Create UI Polish Test Scene
2. **Run Tests**: Verify 388/388 passing
3. **Play Mode Verification**:
   - [ ] Panels animate in/out smoothly
   - [ ] Health/shield bars animate to values
   - [ ] Button hover effects work
   - [ ] Notifications appear and dismiss
   - [ ] Tooltips position correctly
   - [ ] Overall feel is polished

---

## Step 6.7: Phase 6 Integration Testing

**Time**: 2-3 hours  
**Prerequisites**: Step 6.6 complete (388 tests passing)

### CLAUDE CODE PROMPT 6.7

```
CONTEXT:
All Phase 6 systems implemented. Creating integration tests.

OBJECTIVE:
Verify all polish systems work together in combat.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 6 section.

ARCHITECTURE:

1. Phase6IntegrationTests.cs
   - Full polish integration tests

2. PolishIntegrationTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Full Polish Test Scene"
   - Complete combat with all VFX/audio/effects:
     * Player ship
     * Enemy ship
     * Full VFX
     * Full audio
     * All screen effects
     * Polished UI

3. PolishTestController.cs
   - Combat scenario with observation mode
   - Effect intensity controls
   - Performance monitoring
   - A/B toggle (polish on/off)
   - Checklist of effects to verify

INTEGRATION TESTS:

1. Test_WeaponFireTriggersAllFeedback
   - VFX, audio, screen shake all fire together

2. Test_DamageTriggersAllFeedback
   - Shield hit: VFX + audio + flash
   - Hull hit: VFX + audio + shake

3. Test_HeatVisualizationWithAudio
   - Heat VFX matches audio cues

4. Test_CombatAudioLayers
   - Weapon sounds + ambience + music together

5. Test_ScreenEffectsNotOverwhelming
   - Multiple effects don't obscure gameplay

6. Test_UIResponsiveDuringCombat
   - UI animations don't lag during action

7. Test_PerformanceWithAllEffects
   - Maintain 60 FPS with full polish

8. Test_PolishCanBeDisabled
   - Players can turn off effects for performance

STATUS UPDATE:
- ✅ Step 6.7 Complete - Phase 6 Integration Testing
- ✅ PHASE 6 COMPLETE
- List new/modified files
- 🧪 Integration Tests: 8/8 passing
- 🧪 Total Tests: 396/396 passing
- 📊 Phase 6 Summary: [List all systems]
- ⏭️ Next Phase: Phase 7 - Balance & MVP
```

### VERIFICATION 6.7

1. **Run Setup**: Menu → Hephaestus → Testing → Create Full Polish Test Scene
2. **Run All Tests**: Verify 396/396 passing
3. **Manual Combat Playthrough**:
   - [ ] Weapons have full VFX + audio
   - [ ] Damage shows visual + audio feedback
   - [ ] Heat builds with visual progression
   - [ ] Shield hits look and sound distinct
   - [ ] Screen shake on impacts
   - [ ] UI animations are smooth
   - [ ] Music responds to combat intensity
   - [ ] 60 FPS maintained
   - [ ] Overall "game feel" is satisfying

---

## Phase 6 Summary

### Systems Implemented
| System | Description |
|--------|-------------|
| Weapon VFX | Muzzle flash, trails, impacts |
| Damage VFX | Shield ripples, explosions, debris |
| Heat VFX | Glow, distortion, fire effects |
| Audio | SFX, music, ambience |
| Screen Effects | Shake, flash, post-processing |
| UI Polish | Animations, transitions, feedback |

### Test Coverage
| Step | Tests Added | Running Total |
|------|-------------|---------------|
| 6.1 Weapon VFX | 8 | 350 |
| 6.2 Damage VFX | 8 | 358 |
| 6.3 Heat VFX | 8 | 366 |
| 6.4 Audio | 8 | 374 |
| 6.5 Screen Effects | 8 | 382 |
| 6.6 UI Polish | 6 | 388 |
| 6.7 Integration | 8 | 396 |

### Performance Targets
- 60 FPS with all effects
- Max 500 particles per system
- Max 32 simultaneous sounds
- All effects pooled/reused

### Next Phase
Phase 7: Balance & MVP
- Value tuning
- Playtesting
- Bug fixing
- Final validation