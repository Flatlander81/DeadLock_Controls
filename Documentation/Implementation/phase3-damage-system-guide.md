# Hephaestus Implementation Guide - Phase 3: Damage System

---

## Document Overview

**Phase 3** implements the Battletech-inspired damage system that creates attrition warfare where ships degrade over time rather than simply losing HP.

**Prerequisites**: Phase 2.2 Complete (104 tests passing)

**Estimated Time**: 25-35 hours

**New Tests**: ~55 automated tests

---

## Phase 3 Architecture Summary

```
Incoming Damage
      ↓
[Shield System] ─── Single bubble pool, no regen
      ↓
[Section Hit Detection] ─── Attack vector determines zone
      ↓
[Section Armor] ─── Ablative outer layer
      ↓
[Section Structure] ─── Triggers critical rolls
      ↓
[Critical Hit System] ─── Size-based slot targeting
      ↓
[System Degradation] ─── Operational → Damaged → Destroyed
      ↓
[Ship Death] ─── Core Breach or Combat Ineffective
```

---

## Step 3.0: Establish Phase 3 Standards

**Time**: 15 minutes  
**Prerequisites**: Phase 2.2 complete

### CLAUDE CODE PROMPT 3.0

```
CONTEXT:
Beginning Phase 3: Damage System. Before implementation, we need to establish coding standards in CLAUDE.md that all Phase 3 work will follow.

OBJECTIVE:
Add a Phase 3 standards section to CLAUDE.md that will be referenced by all subsequent implementation prompts.

TASK:
Append the following section to CLAUDE.md (create file if it doesn't exist):

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
- **Debug visualization**: Gizmos and runtime visualizers for all spatial systems. Color-coded by state.
- **Unit tests**: Every new system needs comprehensive unit tests. Follow existing test patterns in Assets/Tests/PlayModeTests/.

### Folder Structure for Phase 3
```
Assets/Scripts/Damage/           # Core damage system
Assets/Scripts/Damage/Sections/  # Section-related components
Assets/Scripts/Damage/Systems/   # Mounted systems (weapons, engines, etc.)
Assets/Scripts/Damage/Debug/     # Debug visualizers and test controllers
Assets/Editor/DamageSystem/      # Editor automation scripts
Assets/Tests/PlayModeTests/DamageSystem/  # Unit tests
Assets/Scenes/Testing/           # Test scenes (auto-generated)
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

---

Also create the folder structure listed above if directories don't exist.

STATUS UPDATE:
After completion, note in IMPLEMENTATION_STATUS.md:
- ✅ Step 3.0 Complete - Phase 3 Standards Established
- 📁 Modified: CLAUDE.md
- 📁 Created: Folder structure for Phase 3
- ⏭️ Next: Step 3.1 - Section Infrastructure
```

### VERIFICATION 3.0

- [ ] CLAUDE.md contains Phase 3 standards section
- [ ] All folder paths exist in Assets/

---

## Step 3.1: Section Infrastructure

**Time**: 4-5 hours  
**Prerequisites**: Step 3.0 complete

### CLAUDE CODE PROMPT 3.1

```
CONTEXT:
Phase 3 standards established in CLAUDE.md. Beginning section infrastructure implementation.

The GDD specifies 7 ship sections (Fore, Aft, Port, Starboard, Dorsal, Ventral, Core), each with armor and structure values. Damage flows through armor first, then structure. Section breach occurs when structure hits 0.

OBJECTIVE:
Create the foundational section infrastructure for the damage system.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3 section.

ARCHITECTURE:

1. SectionType.cs
   - Enum defining all 7 section types
   - Serializable

2. SectionDefinitions.cs
   - Static data class with Hephaestus section configuration
   - Struct for per-section data: type, armor, structure, collider position/size
   - Dictionary mapping SectionType → config
   - Default collider positions:
     * Fore: +Z (front)
     * Aft: -Z (rear)
     * Port: -X (left)
     * Starboard: +X (right)
     * Dorsal: +Y (top)
     * Ventral: -Y (bottom)
     * Core: center (sphere collider)

3. ShipSection.cs (MonoBehaviour)
   - Attach to section trigger colliders
   - Properties: sectionType, maxArmor, currentArmor, maxStructure, currentStructure, isBreached, parentShip
   - Events: OnArmorDamaged, OnStructureDamaged, OnSectionBreached
   - Methods: Initialize, ApplyDamage→DamageResult, GetArmorPercentage, GetStructurePercentage, IsOperational, Reset
   - Damage flow: armor absorbs first, overflow to structure, breach on structure=0

4. DamageResult.cs (struct)
   - Fields: damageToArmor, damageToStructure, overflowDamage, armorBroken, sectionBreached, wasAlreadyBreached
   - Computed: TotalDamageApplied

5. SectionManager.cs (MonoBehaviour)
   - Attach to Ship, manages all sections
   - Methods: RegisterSection, AutoRegisterChildSections, GetSection(type), GetAllSections, GetBreachedSections, GetOperationalSections, GetTotalArmorRemaining, GetTotalStructureRemaining, ResetAllSections

6. SectionHitDetector.cs (MonoBehaviour)
   - Attach to section colliders
   - Auto-finds parent ShipSection
   - OnTriggerEnter for projectile detection

7. Modify Ship.cs
   - Add SectionManager property
   - Initialize in Awake

EDITOR AUTOMATION:

8. SectionSetupEditor.cs
   - Menu: "Hephaestus/Setup/Add Sections to Selected Ship"
     * Creates Sections parent object
     * Generates all 7 section colliders with components
     * Configures from SectionDefinitions
     * Creates ShipSection layer if missing
   - Menu: "Hephaestus/Setup/Remove Sections from Selected Ship"
     * Cleanup utility

9. SectionTestSceneSetup.cs
   - Menu: "Hephaestus/Testing/Create Section Test Scene"
   - Creates:
     * Test ship with mesh placeholder
     * All sections via SectionSetupEditor
     * SectionDebugVisualizer
     * SectionTestController
     * UI Canvas with health display
   - Saves to Assets/Scenes/Testing/SectionTestScene.unity

10. SectionDebugVisualizer.cs
    - Gizmos: wireframe per section, color-coded by health
    - Green >66%, Yellow 33-66%, Red <33%, Black = breached
    - Labels with name and values

11. SectionTestController.cs
    - OnGUI test interface
    - Per-section: health bars, damage buttons (25/50/100), breach button
    - Global: Reset All, Damage Random
    - Damage event log

UNIT TESTS (SectionTests.cs):

1. Test_SectionInitialization
2. Test_SectionArmorDamage
3. Test_SectionArmorBreakthrough
4. Test_SectionStructureDamage
5. Test_SectionBreach
6. Test_SectionAlreadyBreached
7. Test_SectionManagerRegistration
8. Test_SectionManagerBreachedList
9. Test_DamageResultAccuracy
10. Test_SectionPercentages
11. Test_SectionReset
12. Test_AutoRegisterChildSections

STATUS UPDATE:
- ✅ Step 3.1 Complete - Section Infrastructure
- List new/modified files
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 116/116 passing
- ⏭️ Next: Step 3.2 - Shield System Refactor
```

### VERIFICATION 3.1

1. **Run Setup**: Menu → Hephaestus → Testing → Create Section Test Scene
2. **Run Tests**: Test Runner → Verify 116/116 passing
3. **Play Mode Verification**:
   - [ ] 7 wireframe sections visible
   - [ ] Health UI shows all sections
   - [ ] Damage buttons reduce armor/structure correctly
   - [ ] Breach button sets section to breached state
   - [ ] Reset restores all values
   - [ ] Colors update based on health

---

## Step 3.2: Shield System Refactor

**Time**: 2-3 hours  
**Prerequisites**: Step 3.1 complete (116 tests passing)

### CLAUDE CODE PROMPT 3.2

```
CONTEXT:
Step 3.1 complete. Section infrastructure in place.

The GDD specifies shields as a single bubble pool (200 HP) with no regeneration. Shield Boost ability (+100) only works when shields are at 0.

OBJECTIVE:
Create shield system and damage routing that integrates shields with sections.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3 section.

ARCHITECTURE:

1. ShieldSystem.cs (MonoBehaviour)
   - Single bubble shield pool
   - Properties: maxShields (200), currentShields, IsShieldActive (currentShields > 0)
   - Events: OnShieldDamaged, OnShieldDepleted, OnShieldRestored
   - Methods:
     * AbsorbDamage(damage) → overflow
     * CanRestoreShields() → true only if shields = 0
     * RestoreShields(amount) → only if CanRestoreShields
     * GetShieldPercentage, Reset
   - NO regeneration

2. DamageRouter.cs (MonoBehaviour)
   - Central damage routing on Ship
   - Auto-finds ShieldSystem, SectionManager
   - Methods:
     * ProcessDamage(damage, targetSection) → DamageReport
     * ProcessDamageAtPoint(damage, worldPoint) → DamageReport
   - Flow: Shields → Section Armor → Section Structure

3. DamageReport.cs (struct)
   - Fields: totalIncomingDamage, shieldDamage, armorDamage, structureDamage
   - Flags: shieldsDepleted, armorBroken, sectionBreached
   - Reference: sectionHit (type), section (reference)

4. Modify AbilityData.cs
   - Add virtual: CanActivate(Ship) → bool
   - Add virtual: GetActivationBlockedReason(Ship) → string

5. Modify ShieldBoostData.cs
   - Override CanActivate: return shields = 0
   - Override GetActivationBlockedReason: explain if blocked
   - Update Execute to use ShieldSystem.RestoreShields

6. Modify AbilitySystem.cs
   - Check CanActivate before allowing activation
   - Display blocked reason if applicable

7. Modify Ship.cs
   - Add ShieldSystem property
   - Add DamageRouter property

EDITOR AUTOMATION:

8. ShieldTestSceneSetup.cs
   - Menu: "Hephaestus/Testing/Create Shield Test Scene"
   - Creates ship with ShieldSystem, sections, DamageRouter
   - ShieldTestController with damage routing tests
   - UI showing shield bar and section health

9. ShieldTestController.cs
   - Shield bar display
   - Damage buttons that route through DamageRouter
   - Shield Boost test button (shows blocked reason)
   - Toggle to target specific sections

UNIT TESTS (ShieldSystemTests.cs):

1. Test_ShieldInitialization
2. Test_ShieldAbsorbsFullDamage
3. Test_ShieldOverflow
4. Test_ShieldDepletedEvent
5. Test_ShieldNoRegeneration
6. Test_ShieldRestore
7. Test_ShieldRestoreBlockedWhenActive
8. Test_ShieldBoostCanActivateCheck
9. Test_DamageRouterShieldsFirst
10. Test_DamageRouterOverflowToSection
11. Test_DamageReportAccuracy
12. Test_ShieldPercentage

STATUS UPDATE:
- ✅ Step 3.2 Complete - Shield System Refactor
- List new/modified files
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 128/128 passing
- ⏭️ Next: Step 3.3 - Projectile Damage Integration
```

### VERIFICATION 3.2

1. **Run Setup**: Menu → Hephaestus → Testing → Create Shield Test Scene
2. **Run Tests**: Verify 128/128 passing
3. **Play Mode Verification**:
   - [ ] Shield bar shows 200/200
   - [ ] Damage reduces shields first
   - [ ] Overflow passes to section
   - [ ] Shield Boost blocked when shields > 0
   - [ ] Shield Boost works when shields = 0
   - [ ] No passive regeneration occurs

---

## Step 3.3: Projectile Damage Integration

**Time**: 2-3 hours  
**Prerequisites**: Step 3.2 complete (128 tests passing)

### CLAUDE CODE PROMPT 3.3

```
CONTEXT:
Step 3.2 complete. Shield system and damage routing in place.

Projectiles currently exist (Projectile.cs, BallisticProjectile.cs, HomingProjectile.cs) but need to route damage through the new DamageRouter system using section hit detection.

OBJECTIVE:
Integrate projectile hits with section-based damage routing.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3 section.

ARCHITECTURE:

1. Modify Projectile.cs (base class)
   - On hit, get SectionHitDetector from collider
   - If found, route damage through target ship's DamageRouter
   - Pass impact point for section determination
   - Store DamageReport for potential use (effects, logging)

2. Modify BallisticProjectile.cs
   - Ensure OnTriggerEnter uses new damage routing
   - Impact point = collision point

3. Modify HomingProjectile.cs
   - Same integration as ballistic
   - Impact point from collision

4. Modify SectionHitDetector.cs
   - Ensure projectiles can query the section
   - Method: HandleProjectileHit(Projectile) → DamageReport

5. ProjectileDamageIntegration.cs (optional helper)
   - Static utility methods if shared logic needed
   - Keep DRY between projectile types

EDITOR AUTOMATION:

6. ProjectileDamageTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Projectile Damage Test Scene"
   - Creates:
     * Target ship with shields and sections
     * Projectile spawn points at various angles
     * Test controller to fire projectiles at specific sections
     * Real-time damage log showing routing results

7. ProjectileDamageTestController.cs
   - Buttons to spawn projectiles aimed at each section
   - Toggle shields on/off for testing
   - Damage log showing: projectile type, section hit, shield/armor/structure damage
   - Visual tracer showing projectile path

UNIT TESTS (ProjectileDamageIntegrationTests.cs):

1. Test_ProjectileHitsSection - Projectile hits, correct section identified
2. Test_ProjectileDamageRoutedThroughShields - Shields absorb first
3. Test_ProjectileOverflowToArmor - Shield overflow hits armor
4. Test_ProjectileSectionBreach - Enough damage breaches section
5. Test_BallisticProjectileDamage - Ballistic specific
6. Test_HomingProjectileDamage - Homing specific
7. Test_ProjectileHitsDifferentSections - Angle determines section
8. Test_ProjectileDamageReport - Report contains correct data
9. Test_MultipleProjectileSameSection - Cumulative damage
10. Test_ProjectileAgainstBreachedSection - Damage to already breached

STATUS UPDATE:
- ✅ Step 3.3 Complete - Projectile Damage Integration
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 138/138 passing
- ⏭️ Next: Step 3.4 - Critical Hit System
```

### VERIFICATION 3.3

1. **Run Setup**: Menu → Hephaestus → Testing → Create Projectile Damage Test Scene
2. **Run Tests**: Verify 138/138 passing
3. **Play Mode Verification**:
   - [ ] Projectile fired at Fore hits Fore section
   - [ ] Projectile fired at Port hits Port section
   - [ ] Shields absorb damage first
   - [ ] Overflow damages correct section
   - [ ] Damage log shows accurate routing
   - [ ] Multiple hits accumulate correctly

---

## Step 3.4: Critical Hit System

**Time**: 4-5 hours  
**Prerequisites**: Step 3.3 complete (138 tests passing)

### CLAUDE CODE PROMPT 3.4

```
CONTEXT:
Step 3.3 complete. Projectiles route damage through shields and sections.

The GDD specifies a critical hit system: when structure takes damage, roll against section's slot layout. Systems occupy slots based on size. Hits on occupied slots damage that system.

REFERENCE (from GDD):
- Each section has structure value = total slots
- Systems have sizes (e.g., Rail Gun = 5, Engine = 15, Reactor = 20)
- Empty slots = safe hits (no critical)
- Critical roll: random 1 to structure_value, check what's in that slot

System sizes from GDD:
| System | Size |
|--------|------|
| Reactor Core | 20 |
| Main Engine | 15 |
| Newtonian Cannon | 8 |
| Torpedo Launcher | 8 |
| Torpedo Magazine | 8 |
| Missile Battery | 6 |
| Missile Magazine | 6 |
| Sensors | 6 |
| Rail Gun | 5 |
| Radiator | 5 |
| PD Turret | 4 |

OBJECTIVE:
Implement critical hit system with slot-based targeting.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3 section.

ARCHITECTURE:

1. ShipSystemType.cs (enum)
   - All mountable system types from GDD

2. ShipSystemData.cs (static definitions)
   - System sizes, default stats per type

3. MountedSystem.cs (MonoBehaviour)
   - Base class for systems mounted in sections
   - Properties: systemType, size, slotStart, slotEnd, health state
   - States: Operational, Damaged, Destroyed (enum)
   - Events: OnSystemDamaged, OnSystemDestroyed
   - Virtual: ApplyDegradation() - subclass effects

4. CriticalHitSystem.cs
   - Handles critical roll logic
   - Method: RollCritical(ShipSection) → CriticalHitResult
   - Determines slot range for section
   - Rolls random slot
   - Finds system at that slot (or empty)

5. CriticalHitResult.cs (struct)
   - Fields: rolled slot, system hit (or null), was empty slot, damage applied

6. SlotLayout.cs
   - Per-section slot configuration
   - List of mounted systems with slot ranges
   - Method: GetSystemAtSlot(int) → MountedSystem or null
   - Method: GetOccupiedSlotCount, GetEmptySlotCount

7. Modify ShipSection.cs
   - Add SlotLayout property
   - On structure damage, trigger CriticalHitSystem.RollCritical
   - Report critical result

8. Modify DamageResult.cs / DamageReport.cs
   - Add critical hit information
   - System damaged/destroyed flags

EDITOR AUTOMATION:

9. CriticalHitTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Critical Hit Test Scene"
   - Ship with configured slot layouts matching GDD Hephaestus
   - Visualizer showing slot layout per section
   - Test controller to force criticals

10. CriticalHitTestController.cs
    - Section selector dropdown
    - Shows slot layout visually (filled vs empty slots)
    - "Roll Critical" button (shows result)
    - "Force Hit System X" button
    - Damage specific systems directly
    - Log of critical hits

11. SlotLayoutVisualizer.cs
    - Debug display of slot layout
    - Color-coded by system type
    - Shows system health state

UNIT TESTS (CriticalHitTests.cs):

1. Test_CriticalRollInRange - Roll always within 1 to structure
2. Test_CriticalHitsSystem - Roll on occupied slot damages system
3. Test_CriticalMissesEmpty - Roll on empty slot, no system damage
4. Test_SystemDamagedState - System hit once → Damaged
5. Test_SystemDestroyedState - System hit twice → Destroyed
6. Test_DestroyedSystemAbsorbsHits - Further hits on destroyed = no new damage
7. Test_SlotLayoutConfiguration - Verify slot assignments correct
8. Test_MultipleSystemsInSection - Different slots hit different systems
9. Test_CriticalResultAccuracy - Result struct populated correctly
10. Test_CriticalProbabilityDistribution - Statistical test over many rolls

STATUS UPDATE:
- ✅ Step 3.4 Complete - Critical Hit System
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 148/148 passing
- ⏭️ Next: Step 3.5 - System Degradation Effects
```

### VERIFICATION 3.4

1. **Run Setup**: Menu → Hephaestus → Testing → Create Critical Hit Test Scene
2. **Run Tests**: Verify 148/148 passing
3. **Play Mode Verification**:
   - [ ] Slot layout visualizer shows correct configuration
   - [ ] Roll Critical produces results in valid range
   - [ ] Hits on occupied slots damage systems
   - [ ] Hits on empty slots show "no critical"
   - [ ] Systems transition: Operational → Damaged → Destroyed
   - [ ] Destroyed systems still occupy slots (absorb future hits)

---

## Step 3.5: System Degradation Effects

**Time**: 3-4 hours  
**Prerequisites**: Step 3.4 complete (148 tests passing)

### CLAUDE CODE PROMPT 3.5

```
CONTEXT:
Step 3.4 complete. Critical hit system identifies which systems take damage.

Now implementing the gameplay effects of damaged/destroyed systems.

REFERENCE (from GDD - Damaged Effects):
| System | Damaged Effect |
|--------|----------------|
| Weapon | Cooldown doubled OR damage halved |
| Main Engine | Max speed halved, turn rate halved |
| Radiator | Passive cooling halved |
| Sensors | Targeting range halved |
| PD Turret | Engagement rate halved |
| Reactor Core | Heat capacity reduced OR generates passive heat |

Destroyed = system non-functional.

OBJECTIVE:
Implement degradation effects that integrate with existing systems.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3 section.

ARCHITECTURE:

1. Create specific MountedSystem subclasses:
   - MountedWeapon.cs - Links to WeaponSystem, applies damage/cooldown penalty
   - MountedEngine.cs - Modifies Ship speed/turn rate
   - MountedRadiator.cs - Modifies HeatManager passive cooling
   - MountedSensors.cs - Modifies targeting range
   - MountedPDTurret.cs - Modifies PD engagement (placeholder for Phase 4)
   - MountedReactor.cs - Modifies heat capacity, catastrophic if destroyed

2. SystemDegradationManager.cs
   - Tracks all mounted systems on ship
   - Applies/removes degradation effects based on system state
   - Recalculates ship stats when systems change state

3. Modify existing systems to support degradation:
   - WeaponSystem.cs: Add degradation multipliers for damage, cooldown
   - Ship.cs: Add degradation multipliers for speed, turn rate
   - HeatManager.cs: Add degradation multiplier for passive cooling

4. Ammo Magazine Systems:
   - MountedMagazine.cs for Torpedo/Missile magazines
   - On Destroyed: Internal explosion (damage to section structure)
   - Reference GDD: Torpedo Magazine = 40 internal damage, Missile = 25

5. Core Breach Logic:
   - If Reactor Core destroyed → Ship destruction
   - Trigger ship death sequence

EDITOR AUTOMATION:

6. DegradationTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Degradation Test Scene"
   - Ship with all system types mounted
   - Real-time stat display showing current values
   - Test controller to damage specific systems

7. DegradationTestController.cs
   - List all mounted systems with state
   - Buttons: Damage System, Destroy System, Repair System
   - Live stat display: speed, turn rate, cooling rate, weapon stats
   - Compare current vs base values

UNIT TESTS (SystemDegradationTests.cs):

1. Test_WeaponDamagedCooldown - Damaged weapon has doubled cooldown
2. Test_WeaponDestroyedNonFunctional - Destroyed weapon cannot fire
3. Test_EngineDamagedSpeed - Speed halved when engine damaged
4. Test_EngineDestroyedImmobile - No movement when destroyed
5. Test_RadiatorDamagedCooling - Passive cooling halved
6. Test_ReactorDamagedHeatCapacity - Heat capacity reduced
7. Test_ReactorDestroyedShipDeath - Core breach triggers death
8. Test_MagazineDestroyedExplosion - Internal damage applied
9. Test_MultipleDegradationsStack - Multiple damaged systems compound
10. Test_RepairRestoresFunction - Repairing system restores stats

STATUS UPDATE:
- ✅ Step 3.5 Complete - System Degradation Effects
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 158/158 passing
- ⏭️ Next: Step 3.6 - Core Protection & Ship Death
```

### VERIFICATION 3.5

1. **Run Setup**: Menu → Hephaestus → Testing → Create Degradation Test Scene
2. **Run Tests**: Verify 158/158 passing
3. **Play Mode Verification**:
   - [ ] Damaging engine reduces displayed speed/turn rate
   - [ ] Damaging radiator reduces cooling rate shown
   - [ ] Damaging weapon shows increased cooldown
   - [ ] Destroying weapon disables it completely
   - [ ] Destroying magazine causes internal explosion
   - [ ] Destroying reactor triggers ship death

---

## Step 3.6: Core Protection & Ship Death

**Time**: 3-4 hours  
**Prerequisites**: Step 3.5 complete (158 tests passing)

### CLAUDE CODE PROMPT 3.6

```
CONTEXT:
Step 3.5 complete. System degradation functional.

The GDD specifies Core section has special protection rules and ship death conditions.

REFERENCE (from GDD):
Core Access Rules:
- Normal attacks cannot hit Core directly
- If adjacent section breached → attacks from that angle CAN hit Core
- 5% "lucky shot" chance: any structure hit can punch through to Core

Ship Death Conditions:
- Core Breach: Reactor destroyed = instant death
- Combat Ineffective: All weapons AND engines destroyed = disabled

OBJECTIVE:
Implement Core protection rules and ship death conditions.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3 section.

ARCHITECTURE:

1. CoreProtectionSystem.cs
   - Determines if Core can be hit from attack angle
   - Methods:
     * CanHitCore(Vector3 attackDirection) → bool
     * GetAdjacentSection(Vector3 direction) → SectionType
     * IsAdjacentSectionBreached(Vector3 direction) → bool
   - Lucky shot roll (5% on structure damage)

2. Modify DamageRouter.cs
   - Before routing to section, check Core access rules
   - If attack would hit Core but Core protected, redirect to outer section
   - Lucky shot: 5% chance structure damage punches to Core

3. ShipDeathController.cs
   - Monitors ship state for death conditions
   - Events: OnShipDestroyed, OnShipDisabled
   - Methods:
     * CheckDeathConditions() - Called after damage
     * IsCoreBreached() → bool
     * IsCombatIneffective() → bool (all weapons + engines destroyed)
   - Triggers appropriate death sequence

4. Modify Ship.cs
   - Add ShipDeathController reference
   - Add IsDestroyed, IsDisabled properties
   - Death state prevents further actions

5. Adjacent Section Mapping:
   - Define which sections are "adjacent" to Core from each angle
   - Fore breach → attacks from front can hit Core
   - Aft breach → attacks from rear can hit Core
   - etc.

EDITOR AUTOMATION:

6. CoreProtectionTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Core Protection Test Scene"
   - Ship with breachable sections
   - Attack angle visualizer
   - Core access indicator

7. CoreProtectionTestController.cs
   - Buttons to breach specific sections
   - Attack angle selector (front, rear, left, right, top, bottom)
   - "Fire at Core" button - shows if blocked or hits
   - Lucky shot toggle (force 5% roll)
   - Ship death state display

8. ShipDeathTestController.cs
   - Destroy all weapons button
   - Destroy all engines button
   - Destroy reactor button
   - Shows death condition status

UNIT TESTS (CoreProtectionTests.cs):

1. Test_CoreProtectedByDefault - Attack at Core blocked
2. Test_CoreAccessibleAfterForeBreach - Breach Fore, front attacks hit Core
3. Test_CoreAccessibleAfterAftBreach - Breach Aft, rear attacks hit Core
4. Test_CoreStillProtectedFromOtherAngles - Breach Fore, side attacks still blocked
5. Test_LuckyShotCanHitCore - 5% chance verified
6. Test_CoreBreachCausesShipDeath - Reactor destroyed = dead
7. Test_CombatIneffectiveCondition - All weapons + engines = disabled
8. Test_PartialDisableNotDeath - Some weapons remain = still active
9. Test_DeathPreventsActions - Dead ship cannot act
10. Test_DisabledCanStillTakeDamage - Disabled ship can be finished off

STATUS UPDATE:
- ✅ Step 3.6 Complete - Core Protection & Ship Death
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 168/168 passing
- ⏭️ Next: Step 3.7 - Damage System UI
```

### VERIFICATION 3.6

1. **Run Setup**: Menu → Hephaestus → Testing → Create Core Protection Test Scene
2. **Run Tests**: Verify 168/168 passing
3. **Play Mode Verification**:
   - [ ] Attacks at Core blocked when all sections intact
   - [ ] Breach Fore section, front attacks now hit Core
   - [ ] Side attacks still blocked after Fore breach
   - [ ] Destroying reactor triggers ship death
   - [ ] Destroying all weapons + engines triggers disabled state
   - [ ] Dead ship shows destroyed state

---

## Step 3.7: Damage System UI

**Time**: 3-4 hours  
**Prerequisites**: Step 3.6 complete (168 tests passing)

### CLAUDE CODE PROMPT 3.7

```
CONTEXT:
Step 3.6 complete. All damage mechanics functional.

Now creating UI to display damage state to player.

REFERENCE (from GDD UI section):
- Section status diagram showing all 7 sections
- Color-coded: Green (armor intact) → Yellow (armor damaged) → Orange (armor breached) → Red (structure critical) → Black (breached)
- System icons within sections showing damage state
- Combat log with hits, criticals, breaches
- Core exposure indicator

OBJECTIVE:
Create comprehensive damage UI integrated with existing HUD.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3 section.

ARCHITECTURE:

1. SectionStatusPanel.cs (UI component)
   - Visual ship diagram with 7 clickable sections
   - Each section shows:
     * Color based on damage state
     * Armor bar
     * Structure bar
     * Mounted system icons with state indicators
   - Core highlighted when exposed
   - Click section for detailed breakdown

2. SectionDetailPopup.cs
   - Detailed view of single section
   - Lists all mounted systems with health
   - Shows slot layout
   - Armor/structure exact values

3. CombatLogPanel.cs
   - Scrolling log of combat events
   - Categories: Hits, Criticals, Breaches, System Damage
   - Color-coded by severity
   - Timestamp per entry
   - Filter toggles

4. ShieldStatusBar.cs
   - Prominent shield display
   - Shows current/max
   - Flash effect on damage
   - "DEPLETED" indicator when at 0

5. DamageUIManager.cs
   - Coordinates all damage UI elements
   - Subscribes to damage events
   - Updates UI in response to:
     * Shield damage
     * Section damage
     * Critical hits
     * System state changes
     * Ship death

6. Modify existing UIManager.cs
   - Integrate damage UI panels
   - Handle visibility states

EDITOR AUTOMATION:

7. DamageUITestSetup.cs
   - Menu: "Hephaestus/Testing/Create Damage UI Test Scene"
   - Full UI layout with all panels
   - Ship that can take damage
   - Controller to trigger various damage events

8. DamageUITestController.cs
   - Buttons to simulate:
     * Shield hit (various amounts)
     * Section damage (each section)
     * Critical hit (each system type)
     * Section breach
     * Ship death
   - Verify UI updates correctly

UNIT TESTS (DamageUITests.cs):

1. Test_ShieldBarUpdatesOnDamage
2. Test_SectionColorChangesWithDamage
3. Test_CombatLogRecordsHits
4. Test_CombatLogRecordsCriticals
5. Test_SystemIconShowsDamagedState
6. Test_CoreIndicatorWhenExposed
7. Test_SectionDetailShowsCorrectData
8. Test_UIUpdatesOnBreach
9. Test_DeathStateUIDisplay
10. Test_UIPerformanceWithManyEvents

STATUS UPDATE:
- ✅ Step 3.7 Complete - Damage System UI
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 178/178 passing
- ⏭️ Next: Step 3.8 - Phase 3 Integration Testing
```

### VERIFICATION 3.7

1. **Run Setup**: Menu → Hephaestus → Testing → Create Damage UI Test Scene
2. **Run Tests**: Verify 178/178 passing
3. **Play Mode Verification**:
   - [ ] Shield bar displays and updates on damage
   - [ ] Section diagram shows all 7 sections
   - [ ] Section colors change with damage
   - [ ] Combat log shows hit events
   - [ ] Critical hits appear in log with system name
   - [ ] Section detail popup shows systems
   - [ ] Core indicator appears when adjacent breached

---

## Step 3.8: Phase 3 Integration Testing

**Time**: 2-3 hours  
**Prerequisites**: Step 3.7 complete (178 tests passing)

### CLAUDE CODE PROMPT 3.8

```
CONTEXT:
All Phase 3 systems implemented. Now creating comprehensive integration tests and a complete test scenario.

OBJECTIVE:
Create integration tests verifying complete damage flow and a full combat test scene.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3 section.

ARCHITECTURE:

1. Phase3IntegrationTests.cs
   - End-to-end tests of complete damage scenarios
   - Tests that span multiple systems

2. DamageSystemIntegrationTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Full Damage Test Scene"
   - Complete combat scenario:
     * Player ship (Hephaestus) with full loadout
     * Target dummy ship that can receive damage
     * All UI panels active
     * Weapon firing functional
     * Full damage routing

3. FullCombatTestController.cs
   - Master test controller combining all systems
   - Combat scenario presets:
     * "Light Damage" - shields take hits
     * "Shield Break" - overflow to armor
     * "Section Damage" - multiple section hits
     * "Critical Cascade" - multiple criticals
     * "Near Death" - ship almost destroyed
     * "Kill Shot" - core breach
   - Step-by-step verification checklist
   - Reset between scenarios

INTEGRATION TESTS:

1. Test_FullDamageFlow_ShieldsToSectionToSystem
   - Fire weapon at ship
   - Verify: shields absorb → overflow to section → critical roll → system damage
   
2. Test_ProjectileHitRoutesCorrectly
   - Fire projectile at specific angle
   - Verify correct section takes damage

3. Test_ShieldDepletionEnablesShieldBoost
   - Deplete shields via damage
   - Verify Shield Boost now activatable

4. Test_CriticalDamagesWeaponAffectsCooldown
   - Damage a weapon system
   - Verify cooldown doubled

5. Test_EngineDamageAffectsMovement
   - Damage engine
   - Verify speed reduced in movement planning

6. Test_CoreAccessAfterBreach
   - Breach Fore section
   - Fire from front
   - Verify Core takes damage

7. Test_ShipDeathOnReactorDestruction
   - Destroy reactor
   - Verify ship death triggered

8. Test_CombatIneffectiveCondition
   - Destroy all weapons and engines
   - Verify disabled state

9. Test_UIReflectsDamageState
   - Apply damage
   - Verify all UI elements updated

10. Test_MultipleHitsSameSection
    - Multiple projectiles to same section
    - Verify cumulative damage and multiple critical rolls

STATUS UPDATE:
- ✅ Step 3.8 Complete - Phase 3 Integration Testing
- ✅ PHASE 3 COMPLETE
- List new/modified files
- 🧪 Integration Tests: 10/10 passing
- 🧪 Total Tests: 188/188 passing
- 📊 Phase 3 Summary: [List all major systems implemented]
- ⏭️ Next Phase: Phase 4 - Point Defense System
```

### VERIFICATION 3.8

1. **Run Setup**: Menu → Hephaestus → Testing → Create Full Damage Test Scene
2. **Run All Tests**: Verify 188/188 passing
3. **Manual Combat Scenario**:
   Run through each preset in FullCombatTestController:
   
   **Light Damage**:
   - [ ] Shields take damage, no overflow
   - [ ] UI shows shield reduction
   
   **Shield Break**:
   - [ ] Shields deplete
   - [ ] Overflow hits section armor
   - [ ] Combat log shows shield depleted
   
   **Section Damage**:
   - [ ] Multiple sections take hits
   - [ ] Armor breaks on at least one
   - [ ] Structure damage triggers criticals
   
   **Critical Cascade**:
   - [ ] Multiple systems damaged
   - [ ] Degradation effects visible (speed, cooldowns)
   - [ ] Combat log shows critical hits
   
   **Near Death**:
   - [ ] Multiple sections breached
   - [ ] Core exposed from some angles
   - [ ] Ship still functional
   
   **Kill Shot**:
   - [ ] Attack hits Core
   - [ ] Reactor destroyed
   - [ ] Ship death triggered
   - [ ] UI shows destroyed state

---

## Phase 3 Summary

### Systems Implemented
| System | Description |
|--------|-------------|
| Ship Sections | 7 sections with armor/structure |
| Shield System | Bubble shields, no regen |
| Damage Routing | Shields → Armor → Structure |
| Critical Hits | Slot-based system targeting |
| System Degradation | Damaged/Destroyed effects |
| Core Protection | Access rules and lucky shots |
| Ship Death | Core breach and combat ineffective |
| Damage UI | Section diagram, combat log |

### Test Coverage
| Step | Tests Added | Running Total |
|------|-------------|---------------|
| 3.1 Sections | 12 | 116 |
| 3.2 Shields | 12 | 128 |
| 3.3 Projectiles | 10 | 138 |
| 3.4 Criticals | 10 | 148 |
| 3.5 Degradation | 10 | 158 |
| 3.6 Core/Death | 10 | 168 |
| 3.7 UI | 10 | 178 |
| 3.8 Integration | 10 | 188 |

### Files Created
- ~25 new script files
- ~8 new Editor scripts
- ~8 new test files
- ~8 test scenes

### Next Phase
Phase 4: Point Defense System
- PD turrets with arc coverage
- Threat detection for incoming projectiles
- Interception mechanics
- Integration with damage system

---

## Appendix: Quick Reference

### Menu Items Created
```
Hephaestus/Setup/Add Sections to Selected Ship
Hephaestus/Setup/Remove Sections from Selected Ship
Hephaestus/Setup/Add ShipSection Layer
Hephaestus/Testing/Create Section Test Scene
Hephaestus/Testing/Create Shield Test Scene
Hephaestus/Testing/Create Projectile Damage Test Scene
Hephaestus/Testing/Create Critical Hit Test Scene
Hephaestus/Testing/Create Degradation Test Scene
Hephaestus/Testing/Create Core Protection Test Scene
Hephaestus/Testing/Create Damage UI Test Scene
Hephaestus/Testing/Create Full Damage Test Scene
```

### Key Classes
```
SectionType (enum)
ShipSection (MonoBehaviour)
SectionManager (MonoBehaviour)
ShieldSystem (MonoBehaviour)
DamageRouter (MonoBehaviour)
DamageResult (struct)
DamageReport (struct)
CriticalHitSystem
MountedSystem (base class)
CoreProtectionSystem
ShipDeathController
```