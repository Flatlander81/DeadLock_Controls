# Hephaestus - Claude Code Standards

This document defines coding standards and conventions for Claude Code to follow during implementation.

---

## Phase 3: Damage System Standards

### Code Standards
- **DRY**: Extract shared logic into base classes or utility methods. No copy-paste code.
- **Naming**: Follow existing project conventions. Review Ship.cs, WeaponSystem.cs, HeatManager.cs for patterns before creating new files.
- **Events**: Use System.Action for events. Follow the event patterns already established in the codebase.
- **SerializeField**: Expose configuration values in Inspector. Keep runtime state visible for debugging with [Header] attributes to organize.
- **Null Safety**: Defensive null checks in all public methods and Awake/Start. Use TryGetComponent where appropriate.
- **Documentation**: XML summary comments on all public classes and methods.

### Editor Automation Standards
- **All Unity setup must be automated via Editor scripts.** If a human would need to click through menus or drag references, write a script instead.
- **Menu items**: Use "Hephaestus/Setup/" for configuration scripts, "Hephaestus/Testing/" for test scene creation.
- **Layer/Tag creation**: Editor scripts must create required layers/tags automatically if missing.
- **Prefab modification**: Editor scripts should work on selected GameObject OR create new test objects as appropriate.

### Testing Standards
- **Test scenes**: Every test scene must be creatable via menu item. Zero manual setup.
- **Test controllers**: Runtime test UI using OnGUI for quick verification. Include buttons for common test actions.
- **Human-usable UI required**: Every test scene MUST have visible, interactive buttons during Play mode. Do not rely solely on Inspector checkboxes or keyboard shortcuts. Use OnGUI to create a panel with clearly labeled buttons for all test actions.
- **SerializedObject wiring**: When creating test scenes via Editor scripts, use SerializedObject/SerializedProperty to wire component references. Never rely on runtime property access (like `ship.DamageRouter`) at edit time - components haven't initialized yet.
- **Debug visualization**: Gizmos and runtime visualizers for all spatial systems. Color-coded by state.
- **Unit tests**: Every new system needs comprehensive unit tests. Follow existing test patterns in Assets/Tests/PlayModeTests/.

### Folder Structure for Phase 3
```
Assets/Scripts/Damage/              # Core damage system
Assets/Scripts/Damage/Sections/     # Section-related components
Assets/Scripts/Damage/Systems/      # Mounted systems (weapons, engines, etc.)
Assets/Scripts/Damage/Debug/        # Debug visualizers and test controllers
Assets/Editor/DamageSystem/         # Editor automation scripts
Assets/Tests/PlayModeTests/DamageSystem/  # Unit tests
Assets/Scenes/Testing/              # Test scenes (auto-generated)
```

### GDD Reference Data (Hephaestus)

**Section Stats:**
| Section    | Armor | Structure | Notes |
|------------|-------|-----------|-------|
| Fore       | 100   | 50        | Front, forward weapons |
| Aft        | 60    | 40        | Rear, engines |
| Port       | 80    | 50        | Left side |
| Starboard  | 80    | 50        | Right side |
| Dorsal     | 70    | 40        | Top |
| Ventral    | 70    | 40        | Bottom |
| Core       | 0     | 30        | Protected center, special access rules |

**Shield Stats:**
- Max Shields: 200 (single bubble)
- Regeneration: None
- Restoration: Shield Boost ability only (+100, requires shields = 0)

**System Slot Sizes (for Critical Hit System):**
| System            | Slot Size |
|-------------------|-----------|
| Reactor Core      | 20        |
| Main Engine       | 15        |
| Newtonian Cannon  | 8         |
| Torpedo Launcher  | 8         |
| Torpedo Magazine  | 8         |
| Missile Battery   | 6         |
| Missile Magazine  | 6         |
| Rail Gun          | 5         |
| PD Turret         | 4         |
| Sensors           | 6         |
| Radiator          | 5         |

---

## Implementation Progress

### Phase 3: Damage System
- [x] **Step 3.1** - Section Infrastructure (SectionType, ShipSection, SectionManager, SectionDefinitions)
- [x] **Step 3.2** - Shield System (ShieldSystem, DamageRouter, DamageResult, DamageReport)
- [x] **Step 3.3** - Projectile Damage Integration (SectionHitDetector, Projectile routing)
- [x] **Step 3.4** - Critical Hit System (SlotLayout, MountedSystem, CriticalHitSystem, ShipSystemType/Data)
- [x] **Step 3.5** - System Degradation Effects (MountedSystem subclasses, SystemDegradationManager)
- [x] **Step 3.6** - Core Protection & Ship Death (CoreProtectionSystem, ShipDeathController)
- [x] **Step 3.7** - Damage System UI (SectionStatusPanel, CombatLogPanel, ShieldStatusBar, DamageUIManager)
- [x] **Step 3.8** - Phase 3 Integration Testing (Phase3IntegrationTests, FullCombatTestController)

🧪 **Total Tests: 188/188 passing** ✅

### Step 3.5 Details - System Degradation
New files created:
- `MountedWeapon.cs` - Weapon degradation (damage/cooldown penalties)
- `MountedEngine.cs` - Engine degradation (speed/turn rate penalties)
- `MountedRadiator.cs` - Radiator degradation (cooling penalties)
- `MountedSensors.cs` - Sensor degradation (targeting range penalties)
- `MountedPDTurret.cs` - PD turret degradation (engagement rate penalties)
- `MountedReactor.cs` - Reactor degradation (heat capacity/passive heat, core breach on destruction)
- `MountedMagazine.cs` - Magazine degradation (reload penalties, internal explosion on destruction)
- `SystemDegradationManager.cs` - Central manager for tracking all degradation effects
- `DegradationTestController.cs` - Runtime test controller for manual testing
- `DegradationTestSetup.cs` - Editor script for creating test scenes
- `SystemDegradationTests.cs` - 10 unit tests for degradation system

Modified files:
- `WeaponSystem.cs` - Added EffectiveDamage/EffectiveCooldown properties, degradation checks
- `Ship.cs` - Added DegradationManager reference, degradation multipliers for movement
- `HeatManager.cs` - Added EffectiveMaxHeat/EffectivePassiveCooling, passive heat from reactors

Degradation effects per GDD:
- **Engine Damaged**: Speed halved, turn rate halved
- **Engine Destroyed**: Cannot move
- **Radiator Damaged**: Passive cooling halved
- **Radiator Destroyed**: No passive cooling
- **Sensor Damaged**: Targeting range halved
- **Sensor Destroyed**: Minimal targeting (10%)
- **Weapon Damaged**: Cooldown doubled OR damage halved (random)
- **Weapon Destroyed**: Cannot fire
- **Reactor Damaged**: Heat capacity -30% OR +10 heat/turn (random)
- **Reactor Destroyed**: CORE BREACH - Ship destroyed
- **Magazine Destroyed**: Internal explosion (Torpedo: 40 dmg, Missile: 25 dmg)

### Step 3.6 Details - Core Protection & Ship Death
New files created:
- `CoreProtectionSystem.cs` - Core access rules (protected unless adjacent section breached, 5% lucky shot)
- `ShipDeathController.cs` - Death condition monitoring (Core Breach, Combat Ineffective, Core/All Sections Breached)
- `CoreProtectionTestController.cs` - Runtime test controller for Core protection mechanics
- `ShipDeathTestController.cs` - Runtime test controller for Ship death mechanics
- `CoreProtectionTestSetup.cs` - Editor script for creating Core Protection test scenes
- `CoreProtectionTests.cs` - 10 unit tests for Core protection and ship death

Modified files:
- `DamageRouter.cs` - Added Core access check, redirects damage when Core is protected
- `DamageReport.cs` - Added CoreWasProtected and WasLuckyShot flags
- `Ship.cs` - Added IsDestroyed, IsDisabled, CanMove, CanAct, SetDeathController properties/methods

Core Protection Rules:
- Core cannot be hit directly unless adjacent section is breached
- Adjacent section mapping uses dot product (Fore=forward, Aft=back, Port=left, Starboard=right, Dorsal=up, Ventral=down)
- 5% "lucky shot" chance on structure damage bypasses Core protection

Ship Death Conditions:
- **Core Breach**: Reactor destroyed = instant death
- **Core Breached**: Core section structure depleted = death
- **All Sections Breached**: All 7 sections breached = death
- **Combat Ineffective**: All weapons AND all engines destroyed = disabled (not dead)

### Step 3.7 Details - Damage System UI
New files created:
- `SectionStatusPanel.cs` - Visual ship diagram showing all 7 sections with damage state
- `SectionDetailPopup.cs` - Detailed view of single section with mounted systems
- `CombatLogPanel.cs` - Scrolling log of combat events with category filters
- `ShieldStatusBar.cs` - Prominent shield display with flash effect
- `DamageUIManager.cs` - Coordinates all damage UI elements, subscribes to damage events
- `DamageUITestSetup.cs` - Editor script for creating Damage UI test scenes
- `DamageUITestController.cs` - Runtime test controller for UI testing
- `DamageUITests.cs` - 10 unit tests for Damage UI system

Modified files:
- `UIManager.cs` - Added DamageUIManager integration, ProcessDamageReport method

UI Features:
- **Section Status Panel**: 7-section ship diagram, color-coded by damage state (Green→Yellow→Orange→Red→Black)
- **Section Detail Popup**: Click section for detailed view, shows mounted systems with state
- **Combat Log**: Scrolling log with categories (Hits, Criticals, Breaches, System Damage, Shield)
- **Shield Status Bar**: Current/max display, flash on damage, "DEPLETED" indicator
- **Core Exposure Indicator**: Warning when Core is accessible via breached section

### Step 3.8 Details - Phase 3 Integration Testing
New files created:
- `Phase3IntegrationTests.cs` - 10 end-to-end integration tests validating complete damage flow
- `FullCombatTestController.cs` - Runtime test controller for full combat simulation
- `DamageSystemIntegrationTestSetup.cs` - Editor script for creating full integration test scenes

Integration tests validate:
1. Shield damage absorption and depletion flow
2. Armor and structure damage progression
3. Critical hit triggering on structure damage
4. System damage and degradation effects
5. Section breach progression
6. Core exposure when adjacent section breached
7. Ship death on reactor destruction (Core Breach)
8. Combat ineffective condition (all weapons/engines destroyed)
9. UI updates reflect damage state (colors, shield bar)
10. Multiple hits accumulate correctly

**Phase 3 Complete!** All damage system components working together end-to-end.

---

## Phase 3.5: Systems Integration Standards

### Architecture Principles
- TurnManager is the central coordinator for all phase transitions
- Systems subscribe to TurnManager events (OnCommandPhaseStart, OnSimulationPhaseStart, OnTurnEnd)
- No system should independently trigger phase changes
- All combat actions queue during Command Phase, execute during Simulation Phase

### Event Flow
1. TurnManager.OnCommandPhaseStart → UI enables planning, weapons show ready status
2. Player commits plans → TurnManager.StartSimulation()
3. TurnManager.OnSimulationPhaseStart → Movement executes, weapons fire
4. Simulation completes → TurnManager.OnTurnEnd → Cooldowns tick, heat dissipates
5. TurnManager.OnCommandPhaseStart → Next turn begins

### Integration Points
- Ship.cs: Responds to turn events for movement execution
- WeaponManager.cs: Queues firing commands, executes on simulation start
- HeatManager.cs: Dissipates heat on turn end
- WeaponSystem.cs: Cooldowns tick on turn end
- AbilitySystem.cs: Cooldowns tick on turn end
- ProjectileManager.cs: Updates projectiles during simulation

### Key Classes to Create/Modify
- CombatCoordinator: Orchestrates combat flow during simulation
- WeaponFiringQueue: Queues weapon fire commands for simulation
- TurnEndProcessor: Handles all turn-end cleanup

### Folder Structure for Phase 3.5
```
Assets/Scripts/Combat/Integration/    # Integration coordinator scripts
Assets/Editor/Integration/            # Editor automation
Assets/Tests/PlayModeTests/Integration/  # Integration tests
```

---
