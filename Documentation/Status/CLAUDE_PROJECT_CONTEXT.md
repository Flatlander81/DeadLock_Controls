# DeadLock Controls - Claude Project Context

**Last Updated:** 2025-12-14
**Purpose:** Upload this file to Claude Projects to provide context about the current codebase state.

---

## Project Summary

**Name:** DeadLock Controls
**Type:** Turn-based tactical space combat game (Unity)
**Inspiration:** Battlestar Galactica: Deadlock, Homeworld
**Engine:** Unity 6000.2.10f1
**Language:** C#
**Current Phase:** 2.1 Complete (Weapon Systems)

---

## Implemented Systems

| System | Status | Tests |
|--------|--------|-------|
| Movement | Complete | 3 editor test files |
| Heat | Complete | 10 tests |
| Abilities | Complete | 13 tests |
| Weapons | Complete | 12 tests |
| Projectiles | Complete | 12 tests |
| Targeting UI | Complete | 12 tests |
| **Total** | **Phase 2.1 Done** | **69 tests passing** |

---

## File Structure (Scripts Only)

```
Assets/Scripts/
├── Movement/
│   ├── Ship.cs                    # Ship state, movement, combat properties
│   ├── TurnManager.cs             # Phase management (Command/Simulation)
│   ├── MovementController.cs      # Player input, ship selection
│   └── OrbitCamera.cs             # Homeworld-style 3D camera
├── Combat/
│   ├── HeatManager.cs             # Heat tracking, tiers, penalties
│   ├── ProjectileManager.cs       # Object pooling, projectile spawning
│   ├── Abilities/
│   │   ├── AbilityData.cs         # Abstract base (ScriptableObject)
│   │   ├── AbilityDataCreator.cs  # Editor utility
│   │   ├── AbilitySystem.cs       # Manager component
│   │   ├── EmergencyCoolingData.cs
│   │   ├── EvasiveManeuverData.cs
│   │   ├── OverchargeWeaponsData.cs
│   │   ├── PDOverrideData.cs
│   │   ├── SensorBurstData.cs
│   │   └── ShieldBoostData.cs
│   ├── Projectiles/
│   │   ├── Projectile.cs          # Abstract base class
│   │   ├── BallisticProjectile.cs # Straight-line (cannons)
│   │   ├── HomingProjectile.cs    # Seeking (missiles)
│   │   └── InstantHitEffect.cs    # Beam effect (railguns)
│   ├── Targeting/
│   │   ├── ITargetingSystem.cs    # Interface
│   │   └── TargetingController.cs # Selection, firing coordination
│   └── Weapons/
│       ├── WeaponSystem.cs        # Abstract base class
│       ├── WeaponManager.cs       # Ship-level management
│       ├── RailGun.cs             # Instant-hit, 360 arc
│       ├── NewtonianCannon.cs     # Ballistic, 180 arc
│       └── HardpointGizmo.cs      # Debug visualization
├── UI/
│   ├── UIManager.cs               # Panel state management
│   ├── DebugUI.cs                 # Heat/ability debug display
│   ├── WeaponConfigPanel.cs       # Group assignment (left panel)
│   ├── WeaponGroupPanel.cs        # Firing interface (right panel)
│   ├── SelectionIndicator.cs      # Rotating ring visual
│   └── TargetingLineRenderer.cs   # Color-coded targeting lines
├── Editor/
│   ├── ProjectilePrefabSetup.cs
│   ├── ProjectileTestSceneSetup.cs
│   ├── TargetingTestSceneSetup.cs
│   ├── WeaponHardpointSetup.cs
│   └── WeaponTestSceneSetup.cs
├── ProjectileTester.cs            # Runtime test controller
└── WeaponTester.cs                # Runtime test controller
```

---

## Test Files

```
Assets/Tests/
├── Editor/
│   ├── ShipMovementTests.cs
│   ├── MovementExecutionTests.cs
│   └── MovementPerformanceTests.cs
└── PlayModeTests/
    ├── HeatSystemTests.cs         # 10 tests
    ├── AbilitySystemTests.cs      # 13 tests
    ├── Phase1IntegrationTests.cs  # 10 tests
    ├── WeaponSystemTests.cs       # 12 tests
    ├── ProjectileSystemTests.cs   # 12 tests
    └── TargetingSystemTests.cs    # 12 tests
```

---

## Documentation Structure

```
Documentation/
├── Status/
│   ├── PROJECT_STATUS.md          # High-level overview
│   ├── IMPLEMENTATION_STATUS.md   # Detailed implementation log
│   └── CLAUDE_PROJECT_CONTEXT.md  # This file
├── hephaestus_guide_part1.md      # Phases 0-1 implementation guide
├── hephaestus_guide_part2.md      # Phase 2 implementation guide
├── hephaestus_guide_part3.md      # Phases 3-4 implementation guide
├── hephaestus_guide_part4.md      # Phases 5-6 implementation guide
├── hephaestus_implementation_guide.md  # Master roadmap (203KB)
├── INTEGRATION_GUIDE.md           # System architecture
└── MovementSystem_Guide.md        # Movement system details

TestingDocumentation/
├── ABILITY_SYSTEM_TESTING_GUIDE.md
├── PROJECTILE_QUICK_START.md
├── PROJECTILE_TEST_GUIDE.md
├── TARGETING_QUICK_START.md
├── TARGETING_TEST_GUIDE.md
├── TARGETING_CONTROLS_REFERENCE.md
├── TRACK_B_SUMMARY.md
└── WEAPON_TEST_GUIDE.md
```

---

## Key Technical Details

### Turn System
- **Command Phase**: Plan movements, queue abilities, assign targets
- **Simulation Phase**: Execute all actions simultaneously
- Managed by `TurnManager.cs` singleton

### Heat System (HeatManager.cs)
- Tiers: Safe(0-59), Minor(60-79), Moderate(80-99), Severe(100-119), Critical(120-149), Catastrophic(150+)
- Passive cooling: 20 heat/turn
- Critical/Catastrophic causes hull damage

### Abilities (6 total)
| Ability | Heat | Cooldown | Effect |
|---------|------|----------|--------|
| Emergency Cooling | 0 | 4 | -50 heat |
| Shield Boost | 25 | 3 | +100 shields (2 turns) |
| Evasive Maneuver | 35 | 2 | 90° turn, 30u move |
| Overcharge Weapons | 20 | 3 | 1.5x damage, 2x heat |
| Sensor Burst | 15 | 2 | Reveal enemies |
| PD Override | 30 | 3 | 2x PD capacity |

### Weapons (2 implemented)
| Weapon | Type | Arc | Range | Damage | Heat |
|--------|------|-----|-------|--------|------|
| RailGun | Instant-hit | 360° | 30 | 20 | 15 |
| Newtonian Cannon | Ballistic | 180° | 20 | 40 | 30 |

### Projectiles (3 types)
- **Ballistic**: Straight-line, no tracking
- **Homing**: 90°/sec turn rate, seeks target
- **InstantHit**: Line renderer beam effect

### Controls
- **1-4**: Fire weapon groups (enemy targeted) / Abilities
- **5-6**: Abilities only
- **A**: Alpha Strike
- **E/R**: Elevation/Rotation mode
- **Q/E, WASD, R/F**: Camera orbit/pan/zoom

---

## Not Yet Implemented

- Phase 3: Point Defense System
- Phase 4: Enemy AI
- Phase 5: Polish & VFX
- Phase 6: Balance & MVP

---

## Verification Checklist

To verify Claude has correct context, ask:

1. "What phase is the project in?" → Should answer: Phase 2.1 Complete
2. "How many tests are passing?" → Should answer: 69
3. "What are the 6 abilities?" → Should list all 6 with stats
4. "What weapons are implemented?" → Should answer: RailGun, Newtonian Cannon
5. "Where is the targeting code?" → Should answer: `Assets/Scripts/Combat/Targeting/TargetingController.cs`

---

## Recent Changes

- Documentation reorganized into `Documentation/` and `TestingDocumentation/` folders
- Status files moved to `Documentation/Status/`
- Movement README renamed to `MovementSystem_Guide.md`
- All status documentation synced with actual codebase
