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
- **All tests must pass**: A feature is not complete until ALL unit tests pass (0 failures). Fix any failing tests before marking a step as complete.

### Unified Test Scene Philosophy
- **Incremental test scenes**: Each phase's test scene EXTENDS the previous phase's unified test. This allows testing all features together.
- **Phase 3 Unified Test** is the base - it includes full ships with all damage systems, sections, weapons, and UI.
- **Phase 3.5+ test scenes** must build on Phase 3 Unified by adding new systems (TurnManager, TurnEndProcessor, etc.) to the existing ships.
- **No standalone test levels**: Never create a test scene with minimal ships that lack damage system, sections, etc. Always start from the unified base.
- **Test controllers are additive**: New test controllers add tabs/sections to test new features while existing Phase 3 features remain testable.
- **Menu organization**: `Hephaestus/Testing/Phase X.X - FeatureName/Create Test Scene` for feature-specific scenes, `Hephaestus/Testing/Create Unified Phase X Test Level` for the comprehensive unified level.

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

🧪 **Phase 3 Tests: 188/188 passing** ✅

### Phase 3.5: Systems Integration
- [x] **Step 3.5.0** - Phase 3.5 Standards Established
- [x] **Step 3.5.1** - Turn System Event Infrastructure (TurnPhase, TurnEventSubscriber, CombatCoordinator)
- [x] **Step 3.5.2** - Weapon Firing Integration (WeaponFireCommand, WeaponFiringQueue)
- [x] **Step 3.5.3** - Heat and Cooldown Turn Integration (TurnEndProcessor)
- [x] **Step 3.5.4** - Movement and Weapon Arc Integration (MovementExecutor, WeaponArcValidator)
- [x] **Step 3.5.5** - Unified Combat Test Level (UnifiedCombatFlowTests - 15 end-to-end tests)
- [ ] **Step 3.5.6** - Phase 3.5 Final Validation

🧪 **Total Tests: 260/260 passing** ✅

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

### Step 3.5.3 Details - Heat and Cooldown Turn Integration
New files created:
- `TurnEndProcessor.cs` - Central turn-end processing (heat dissipation, cooldown ticking)
- `HeatCooldownTestSetup.cs` - Editor script for test scene creation
- `HeatCooldownTestController.cs` - Runtime test controller
- `HeatCooldownIntegrationTests.cs` - 10 unit tests for heat/cooldown integration

Modified files:
- `HeatManager.cs` - Added DissipateHeat() method and OnHeatDissipated event, Reset() method

Turn-End Processing:
- Heat dissipation: Base 10 + radiator bonus (5 per operational radiator)
- Damaged radiators: Half bonus (2.5)
- Destroyed radiators: No bonus
- Weapon cooldowns: Tick down by 1 each turn
- Ability cooldowns: Tick down by 1 each turn
- Events: OnHeatDissipated, OnWeaponReady, OnAbilityReady

### Phase 3.5 Unified Test Infrastructure
New files created:
- `Phase35UnifiedTestSetup.cs` - Creates unified test level with all Phase 3 + Phase 3.5 features
- `Phase35UnifiedTestController.cs` - Runtime controller with tabs for all features (10 tabs total)

Modified files:
- `TurnSystemTestSetup.cs` - Now builds on Phase35UnifiedTestSetup
- `WeaponFiringTestSetup.cs` - Now builds on Phase35UnifiedTestSetup
- `HeatCooldownTestSetup.cs` - Now builds on Phase35UnifiedTestSetup

Test Scene Hierarchy:
- **Phase 3 Unified Test** (`Hephaestus/Testing/Phase 3 - Damage/Create Unified Test Level`)
  - Full ships with damage system, sections, shields, weapons, degradation, death
- **Phase 3.5 Unified Test** (`Hephaestus/Testing/Create Unified Phase 3.5 Test Level`)
  - Extends Phase 3 + TurnManager, CombatCoordinator, WeaponFiringQueue, TurnEndProcessor
- **Feature-Specific Tests** (`Hephaestus/Testing/Phase 3.5 - Integration/...`)
  - Turn System Test - Focuses on phase transitions and turn events
  - Weapon Firing Test - Focuses on firing queue and weapon groups
  - Heat Cooldown Test - Focuses on dissipation and cooldown ticking
  - Movement Test - Uses Movement tab in Phase 3.5 Unified Test

### Step 3.5.4 Details - Movement and Weapon Arc Integration
New files created:
- `MovementExecutor.cs` - Coordinates ship movement during Simulation phase
- `WeaponArcValidator.cs` - Validates weapon arcs accounting for ship movement
- `MovementIntegrationTests.cs` - 15 unit tests for movement and arc validation

Modified files:
- `Ship.cs` - Added GetPositionAtTime(), GetRotationAtTime(), GetMoveProgress(), IsExecutingMove, SetMoveDuration()
- `Phase35UnifiedTestController.cs` - Added Movement tab (11th tab)
- `Phase35UnifiedTestSetup.cs` - Creates MovementExecutor and WeaponArcValidator

Movement Features:
- Position-at-time queries along Bezier curve for retroactive arc checking
- Rotation-at-time queries based on curve tangent
- MovementExecutor subscribes to TurnManager events
- Automatic movement execution on simulation start
- Movement progress tracking (0-1 normalized time)

Arc Validation Features:
- Static arc checking (current positions)
- Movement-aware arc checking (samples positions during movement)
- Firing window detection (start/end times when weapon is in arc)
- Optimal firing time calculation (best angle to target)
- Cache system for performance

---

## Manual Testing Guide

Each completed phase/step should be manually tested using the Unity Editor. This section describes how to create and run test scenes for each major system.

### How to Create Test Scenes

1. Open Unity Editor
2. Go to menu: `Hephaestus/Testing/...`
3. Select the appropriate test scene creation option
4. Press Play to enter Play Mode
5. Use the on-screen controls and keyboard shortcuts

### Phase 3: Damage System Testing

**Create Scene:** `Hephaestus/Testing/Phase 3 - Damage/Create Unified Test Level`

**Keyboard Controls:**
- `J` - Toggle test panel UI
- `K` - Cycle target ships
- `Tab` - Cycle through tabs

**Test Panel Tabs:**
1. **Combat** - Shield damage, section selection, quick damage buttons
2. **Sections** - Per-section damage and breach controls
3. **Systems** - Mounted system damage, degradation multipliers
4. **Core/Death** - Core protection, attack directions, death conditions
5. **Projectiles** - Fire ballistic/homing projectiles
6. **Weapons** - Fire individual weapons
7. **Status** - All ships overview

**Manual Tests to Verify:**
- [ ] Hit shields and verify they absorb damage (Combat tab)
- [ ] Deplete shields and verify damage passes to sections
- [ ] Apply damage to sections, verify armor depletes before structure
- [ ] Breach a section, verify it shows red/black
- [ ] Damage systems (engines, radiators), verify degradation multipliers update
- [ ] Breach fore section, then attack core - verify core is accessible
- [ ] Destroy reactor - verify Core Breach death

### Phase 3.5: Systems Integration Testing

**Create Scene:** `Hephaestus/Testing/Create Unified Phase 3.5 Test Level`

**Movement Controls:**
- `M` - Toggle movement mode
- Click+Drag on projection to plan move
- `E` - Elevation adjustment mode (scroll wheel)
- `R` - Rotation adjustment mode (arrow keys)
- `Enter/Space` - Confirm move
- `Escape` - Cancel move / Exit movement mode

**Combat Controls:**
- `J` - Toggle test panel UI
- `K` - Cycle target ships
- `T` - Advance turn (force end current turn)
- `1,2,3,4` - Fire weapon groups
- `A` - Alpha Strike (fire all weapons)
- `Space` - Toggle weapon config panel
- `R` - Reset all cooldowns (cheat)
- `L` - Reload all ammo (cheat)

**Test Panel Tabs (extends Phase 3):**
8. **Turns** - Turn system control, phase transitions, event log
9. **Heat/CD** - Heat controls, cooldown management, radiator status
10. **FiringQ** - Weapon firing queue, queue controls
11. **Movement** - Movement planning, execution, arc validation

### Step 3.5.1: Turn System Testing

**Create Scene:** `Hephaestus/Testing/Phase 3.5 - Integration/Create Turn System Test Scene`

**Manual Tests to Verify:**
- [ ] Verify turn counter increments after each turn
- [ ] Verify Command Phase → Simulation Phase → Turn End flow
- [ ] Use "Start Simulation" button and observe phase change
- [ ] Press `T` to advance turns, verify event log updates
- [ ] Verify OnCommandPhaseStart, OnSimulationPhaseStart, OnTurnEnd events fire

### Step 3.5.2: Weapon Firing Queue Testing

**Create Scene:** `Hephaestus/Testing/Phase 3.5 - Integration/Create Weapon Firing Test Scene`

**Manual Tests to Verify:**
- [ ] Queue individual weapons using Queue button
- [ ] Queue all ready weapons using "Queue All Ready Weapons"
- [ ] Clear queue and verify count goes to 0
- [ ] Execute queue and verify weapons fire
- [ ] Fire weapon groups (1-4), verify correct weapons fire
- [ ] Alpha Strike (`A`), verify all weapons fire

### Step 3.5.3: Heat and Cooldown Testing

**Create Scene:** `Hephaestus/Testing/Phase 3.5 - Integration/Create Heat Cooldown Test Scene`

**Manual Tests to Verify:**
- [ ] Add heat (+25, +50, +100 buttons), verify heat increases
- [ ] Advance turn (`T`), verify heat dissipates
- [ ] Check dissipation rate shows base 10 + radiator bonus
- [ ] Damage radiators (Heat/CD tab), verify dissipation decreases
- [ ] Set weapon cooldowns (CD=3), advance turns, verify they tick down
- [ ] Verify OnHeatDissipated events appear in log
- [ ] Verify OnWeaponReady events when cooldowns reach 0

### Step 3.5.4: Movement and Arc Integration Testing

**Create Scene:** `Hephaestus/Testing/Create Unified Phase 3.5 Test Level`
(Use Movement tab OR mouse-based movement controls)

**Mouse-Based Movement Planning:**
- [ ] Press `M` to enter movement mode
- [ ] Click and drag on ship projection to plan movement
- [ ] Use scroll wheel with `E` mode for elevation adjustment
- [ ] Use arrow keys with `R` mode for rotation adjustment
- [ ] Press `Enter` or `Space` to confirm move
- [ ] Press `Escape` to cancel

**Movement Tab Manual Tests:**
- [ ] Check "Can Move" status (should be green if engines operational)
- [ ] Plan forward move (+10 Forward button), verify "Has Planned Move: True"
- [ ] Execute move, verify ship moves along Bezier curve
- [ ] Check arc validation - verify weapons show IN ARC / OUT status
- [ ] Plan a turning move (Turn Right), check weapons that come into arc
- [ ] Verify optimal firing time (@t=X.XX) for weapons that will be in arc during movement
- [ ] Check position queries show interpolated positions along curve
- [ ] Damage engines, verify max move distance decreases

### Feature-Specific Test Scenes

| Feature | Menu Path | Focus |
|---------|-----------|-------|
| Turn System | Phase 3.5 - Integration/Create Turn System Test Scene | Phase transitions, events |
| Weapon Firing | Phase 3.5 - Integration/Create Weapon Firing Test Scene | Queue, groups, alpha strike |
| Heat/Cooldown | Phase 3.5 - Integration/Create Heat Cooldown Test Scene | Dissipation, radiators, cooldowns |

### Step 3.5.5 Details - Unified Combat Test Level
New files created:
- `UnifiedCombatFlowTests.cs` - 15 end-to-end integration tests for complete combat flow

Test Coverage:
1. **Complete Turn Cycle** - Command → Simulation → Turn End → Next Command
2. **Movement During Simulation** - Ship moves along planned path
3. **Weapon Queue Execution** - Queued weapons fire during simulation
4. **Heat Dissipation** - Heat dissipates at turn end
5. **Cooldown Ticking** - Weapon cooldowns tick down each turn
6. **Shield Absorption** - Shields absorb damage correctly
7. **Section Damage** - Sections take damage after shields deplete
8. **Arc Validation** - Weapon arcs validate with movement
9. **Degradation Effects** - Damaged systems affect combat
10. **Full Combat Round** - Complete movement + firing + damage
11. **Multiple Turns** - Several consecutive turns work correctly
12. **Event Firing Order** - Turn events fire in correct sequence
13. **Radiator Bonus** - Damaged radiators reduce heat dissipation
14. **Engine Destruction** - Destroyed engines prevent movement
15. **Position Queries** - Position-at-time queries work during execution

Modified files:
- `Phase35UnifiedTestSetup.cs` - Added MovementController and InputManager creation
- Fixed test setup to register ships with TurnEndProcessor

Key API Patterns Validated:
- HeatManager: AddPlannedHeat() → CommitPlannedHeat() for heat addition
- WeaponSystem: StartCooldown() sets cooldown, TickCooldown() decrements
- TurnEndProcessor: RegisterShip() to track ships for turn-end processing
- DamageRouter: ProcessDamage() for damage routing

### Quick Validation Checklist

After each step completion, verify:
1. [ ] All unit tests pass (run via Test Runner or command line)
2. [ ] Test scene creates without errors
3. [ ] Play mode starts without console errors
4. [ ] Basic functionality works per manual tests above
5. [ ] UI displays correct values
