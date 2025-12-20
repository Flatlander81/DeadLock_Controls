# Hephaestus Implementation Guide - Phase 4: Point Defense System

---

## Document Overview

**Phase 4** implements the Point Defense system - automated turrets that intercept incoming missiles and torpedoes, creating tactical depth around saturation attacks and arc exploitation.

**Prerequisites**: Phase 3.5 Complete (250 tests passing)

**Estimated Time**: 20-28 hours

**New Tests**: ~56 automated tests

---

## Phase 3.5 Integration Dependencies

Phase 4 builds on the integrated combat loop from Phase 3.5:

- **TurnManager Events**: PD subscribes to `OnSimulationPhaseStart` to begin interception
- **CombatCoordinator**: PD interception happens AFTER movement, BEFORE damage resolution
- **WeaponFiringQueue**: Missiles/torpedoes are queued weapons that PD can intercept
- **Projectile Timing**: PD intercepts during projectile travel phase of simulation

**Combat Flow with PD:**
```
Simulation Phase:
  1. Ships move (0-1s)
  2. Weapons fire, projectiles spawn (1-2s)
  3. PD INTERCEPTS incoming homing projectiles (2-2.5s)  ← NEW
  4. Surviving projectiles hit targets (2.5-3s)
  5. Damage resolves
```

---

## Phase 4 Architecture Summary

```
Incoming Projectile (Homing)
      ↓
[Threat Detection] ─── Identify projectiles entering PD range
      ↓
[Coverage Check] ─── Is projectile in any turret's arc?
      ↓
[Turret Assignment] ─── Which turret(s) engage?
      ↓
[Intercept Roll] ─── Base 60% ± modifiers
      ↓
[Result] ─── Destroyed OR continues to target
```

---

## GDD Reference Data

**PD Turret Stats:**
| Property | Value |
|----------|-------|
| Base Intercept | 60% |
| Targets/Turn | 2 per turret (4 with PD Override) |
| Heat Cost | None (independent system) |
| Coverage | Arc-based per turret |

**Intercept Modifiers:**
| Target Type | Modifier |
|-------------|----------|
| Torpedo (slow) | +20% (80% total) |
| Missile | Base (60%) |
| Cannon Shell | -30% (30% total) |

**Hephaestus PD Layout:**
| Section | Turrets |
|---------|---------|
| Aft | 2 |
| Port | 2 |
| Starboard | 2 |
| Dorsal | 2 |
| Ventral | 2 |
| **Total** | **10** |

**Degradation:**
- Damaged: Engagement rate halved (1 target/turn)
- Destroyed: Gap in coverage

---

## Step 4.0: Establish Phase 4 Standards

**Time**: 15 minutes
**Prerequisites**: Phase 3.5 complete (250 tests passing)

### CLAUDE CODE PROMPT 4.0

```
CONTEXT:
Phase 3.5 complete with 250 tests passing. Beginning Phase 4: Point Defense System.

OBJECTIVE:
Add Phase 4 standards section to CLAUDE.md.

TASK:
Append the following section to CLAUDE.md:

---

## Phase 4: Point Defense System Standards

### Architecture Principles
- PD turrets are **reactive and automatic** - no player control during simulation
- PD operates independently of heat system (no heat cost)
- Each turret has arc-based coverage (not full 360°)
- Interception is probabilistic with modifiers based on target type
- PD integrates with Phase 3 damage system (turrets can be damaged/destroyed)

### Key Classes to Create
- PDTurret: Individual turret with arc, range, intercept logic
- PDSystem: Ship-level manager for all turrets
- ThreatDetector: Identifies incoming projectiles
- InterceptionResolver: Handles intercept rolls and results

### Integration Points
- MountedSystem (Phase 3): PDTurret is a mountable system that can be damaged
- Projectile system: PD targets homing projectiles (missiles, torpedoes)
- AbilitySystem: PD Override ability doubles engagement capacity
- DamageRouter: Projectiles that survive PD continue to damage routing

### Folder Structure for Phase 4
```
Assets/Scripts/Combat/PointDefense/  # PD system scripts
Assets/Editor/PointDefense/          # Editor automation
Assets/Tests/PlayModeTests/PointDefense/  # Unit tests
```

### GDD Reference (Hephaestus PD)
- 10 turrets: Aft(2), Port(2), Starboard(2), Dorsal(2), Ventral(2)
- Base intercept: 60%
- Torpedo modifier: +20%
- Missile modifier: +0%
- Cannon shell modifier: -30%
- Targets per turret: 2 (4 with PD Override)
- Turret size: 4 slots (for critical hit system)

---

Create the folder structure listed above if directories don't exist.

STATUS UPDATE:
After completion, note in IMPLEMENTATION_STATUS.md:
- ✅ Step 4.0 Complete - Phase 4 Standards Established
- 📁 Modified: CLAUDE.md
- 📁 Created: Folder structure for Phase 4
- ⏭️ Next: Step 4.1 - PD Turret Infrastructure
```

### VERIFICATION 4.0

- [ ] CLAUDE.md contains Phase 4 standards section
- [ ] All folder paths exist

---

## Step 4.1: PD Turret Infrastructure

**Time**: 4-5 hours
**Prerequisites**: Step 4.0 complete (250 tests passing)

### CLAUDE CODE PROMPT 4.1

```
CONTEXT:
Phase 4 standards established. Beginning PD turret implementation.

PD turrets are automatic defensive weapons that intercept incoming homing projectiles. Each turret has limited arc coverage and can engage a set number of targets per turn.

OBJECTIVE:
Create the foundational PD turret infrastructure.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 4 section.

ARCHITECTURE:

1. PDTurretData.cs (static definitions)
   - Default turret stats: range, arc angle, base intercept chance
   - Intercept modifiers by projectile type
   - Targets per turn (base and with override)

2. PDTurret.cs (MonoBehaviour)
   - Individual turret component
   - Properties:
     * turretId: string (unique identifier)
     * mountedSection: SectionType (where it's mounted)
     * arcAngle: float (coverage cone, default 90°)
     * arcDirection: Vector3 (forward direction of arc)
     * range: float (intercept range, default 15 units)
     * baseInterceptChance: float (0.6)
     * targetsPerTurn: int (2, doubled by PD Override)
     * currentTargetsThisTurn: int
   - Methods:
     * CanEngageTarget(Vector3 targetPosition) → bool (in arc and range)
     * GetInterceptChance(ProjectileType type) → float (with modifiers)
     * TryIntercept(Projectile projectile) → InterceptResult
     * ResetTurnState() - Called at turn start
     * HasCapacity() → bool (currentTargets < targetsPerTurn)

3. InterceptResult.cs (struct)
   - Fields: success, turretId, projectileId, interceptChance, roll

4. PDTurret inherits from MountedSystem (Phase 3)
   - Degradation affects engagement rate
   - Damaged: targetsPerTurn halved
   - Destroyed: cannot engage

5. PDSystem.cs (MonoBehaviour)
   - Ship-level PD manager
   - Properties:
     * turrets: List<PDTurret>
     * isOverrideActive: bool (from PD Override ability)
   - Methods:
     * RegisterTurret(PDTurret)
     * GetTurretsInArc(Vector3 threatDirection) → List<PDTurret>
     * GetAvailableTurrets() → turrets with remaining capacity
     * SetOverrideActive(bool) - Doubles all turret capacity
     * ResetAllTurrets() - Turn start reset
     * GetTotalInterceptCapacity() → int

6. Modify Ship.cs
   - Add PDSystem property

EDITOR AUTOMATION:

7. PDTurretSetupEditor.cs
   - Menu: "Hephaestus/Setup/Add PD Turrets to Selected Ship"
     * Creates PDSystem component
     * Adds 10 turrets in correct sections per GDD
     * Configures arc directions based on mount location:
       - Aft turrets: rear-facing arcs
       - Port turrets: left-facing arcs
       - Starboard turrets: right-facing arcs
       - Dorsal turrets: upward-facing arcs
       - Ventral turrets: downward-facing arcs
     * Links turrets to MountedSystem slots in sections

8. PDTurretTestSceneSetup.cs
   - Menu: "Hephaestus/Testing/Create PD Turret Test Scene"
   - Creates:
     * Ship with all PD turrets configured
     * Visual arc display for each turret
     * Test target objects at various positions
     * PDTurretTestController

9. PDTurretDebugVisualizer.cs
   - Gizmos showing turret arcs as cones
   - Color by state: Green (ready), Yellow (engaged), Red (destroyed)
   - Range sphere visualization
   - Toggle on/off per turret

10. PDTurretTestController.cs
    - OnGUI interface showing:
      * All 10 turrets with capacity status
      * Arc coverage diagram (top-down view)
      * Test position selector
      * "Check Coverage" button - shows which turrets can engage
      * "Simulate Intercept" button - rolls intercept for selected turret
      * Intercept log

UNIT TESTS (PDTurretTests.cs):

1. Test_TurretInitialization - Verify default properties
2. Test_TurretArcCoverage - Target in arc returns true
3. Test_TurretArcExclusion - Target outside arc returns false
4. Test_TurretRangeCheck - Target beyond range excluded
5. Test_TurretCapacityTracking - Engagements reduce capacity
6. Test_TurretCapacityReset - Reset restores capacity
7. Test_TurretInterceptChance_Torpedo - 80% (60 + 20)
8. Test_TurretInterceptChance_Missile - 60%
9. Test_TurretInterceptChance_Cannon - 30% (60 - 30)
10. Test_PDSystemRegistration - All 10 turrets registered
11. Test_PDSystemGetTurretsInArc - Correct turrets for direction
12. Test_TurretDegradation - Damaged turret has halved capacity

STATUS UPDATE:
- ✅ Step 4.1 Complete - PD Turret Infrastructure
- List new/modified files
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 262/262 passing
- ⏭️ Next: Step 4.2 - Threat Detection System
```

### VERIFICATION 4.1

1. **Run Setup**: Menu → Hephaestus → Testing → Create PD Turret Test Scene
2. **Run Tests**: Verify 200/200 passing
3. **Play Mode Verification**:
   - [ ] 10 turret arc cones visible around ship
   - [ ] Arcs face correct directions per mount location
   - [ ] Test controller shows all turrets
   - [ ] "Check Coverage" correctly identifies turrets for test positions
   - [ ] Turrets outside arc return false for coverage
   - [ ] Capacity tracking works (2 per turret default)

---

## Step 4.2: Threat Detection System

**Time**: 3-4 hours
**Prerequisites**: Step 4.1 complete (262 tests passing)

### CLAUDE CODE PROMPT 4.2

```
CONTEXT:
Step 4.1 complete. PD turrets exist with arc coverage and capacity.

Now implementing threat detection - the system that identifies incoming projectiles and queues them for interception.

OBJECTIVE:
Create threat detection that identifies homing projectiles entering PD range.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 4 section.

ARCHITECTURE:

1. ThreatDetector.cs (MonoBehaviour)
   - Monitors for incoming projectiles
   - Attached to Ship
   - Properties:
     * detectionRange: float (slightly larger than PD range for early warning)
     * trackedThreats: List<TrackedThreat>
     * ownerShip: Ship
   - Methods:
     * void Update() - Scan for new threats during Simulation phase
     * void RegisterThreat(Projectile projectile)
     * void UnregisterThreat(Projectile projectile)
     * List<TrackedThreat> GetActiveThreats()
     * List<TrackedThreat> GetThreatsInPDRange()
     * void ClearAllThreats() - Turn cleanup

2. TrackedThreat.cs (class)
   - Wrapper for tracked projectile
   - Properties:
     * projectile: Projectile reference
     * detectedTime: float
     * estimatedImpactTime: float
     * targetSection: SectionType (predicted)
     * hasBeenEngaged: bool
     * interceptAttempts: int
   - Methods:
     * UpdateTracking() - Recalculate ETA, target section
     * GetThreatDirection() → Vector3 (from ship to projectile)

3. ProjectileType Classification
   - Extend Projectile.cs or create helper:
     * GetProjectileType() → enum (Torpedo, Missile, CannonShell, Other)
   - Used for intercept chance modifiers

4. ThreatPrioritizer.cs
   - Determines engagement order
   - Priority factors:
     * Time to impact (closer = higher priority)
     * Damage potential (torpedo > missile)
     * Already engaged (skip if being handled)
   - Method: PrioritizeThreats(List<TrackedThreat>) → sorted list

5. Modify Projectile.cs / HomingProjectile.cs
   - Add event: OnEnteredPDRange(Ship targetShip)
   - Add property: isBeingIntercepted
   - Track which ship's PD is engaging

6. Integration with TurnManager
   - Threat detection active during Simulation phase
   - Clear threats at turn end

EDITOR AUTOMATION:

7. ThreatDetectionTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Threat Detection Test Scene"
   - Creates:
     * Ship with PD system and threat detector
     * Projectile spawners at various angles
     * Spawn buttons for each projectile type
     * Threat tracking display

8. ThreatDetectionTestController.cs
   - Spawn projectile buttons (Torpedo, Missile, Cannon Shell)
   - Spawn angle selector (front, rear, sides, above, below)
   - Threat list display showing:
     * Projectile type
     * Distance
     * ETA
     * Engaged status
   - Priority queue visualization
   - Clear threats button

9. ThreatVisualizer.cs
   - Runtime visualization of threats
   - Line from projectile to ship
   - Color by type: Red (torpedo), Orange (missile), Yellow (shell)
   - ETA text display

UNIT TESTS (ThreatDetectionTests.cs):

1. Test_ThreatRegistration - Projectile enters range, registered
2. Test_ThreatUnregistration - Projectile destroyed, unregistered
3. Test_ThreatTracking - ETA and direction update correctly
4. Test_ThreatPrioritization - Closer threats higher priority
5. Test_TorpedoPriorityOverMissile - Damage potential factor
6. Test_MultipleThreatTracking - Handle many simultaneous threats
7. Test_ThreatDirectionCalculation - Correct vector from ship
8. Test_ProjectileTypeClassification - Correct type identification
9. Test_ThreatClearOnTurnEnd - Threats cleared appropriately
10. Test_AlreadyEngagedSkipped - Engaged threats deprioritized

STATUS UPDATE:
- ✅ Step 4.2 Complete - Threat Detection System
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 272/272 passing
- ⏭️ Next: Step 4.3 - Interception Mechanics
```

### VERIFICATION 4.2

1. **Run Setup**: Menu → Hephaestus → Testing → Create Threat Detection Test Scene
2. **Run Tests**: Verify 210/210 passing
3. **Play Mode Verification**:
   - [ ] Spawn torpedo - appears in threat list
   - [ ] ETA counts down as projectile approaches
   - [ ] Priority list shows torpedo above missile
   - [ ] Threat direction line renders correctly
   - [ ] Multiple threats tracked simultaneously
   - [ ] Destroying projectile removes from list

---

## Step 4.3: Interception Mechanics

**Time**: 4-5 hours
**Prerequisites**: Step 4.2 complete (272 tests passing)

### CLAUDE CODE PROMPT 4.3

```
CONTEXT:
Step 4.2 complete. Threats are detected and prioritized.

Now implementing the interception logic - turrets engaging threats and rolling for intercept success.

OBJECTIVE:
Create interception system that assigns turrets to threats and resolves intercept attempts.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 4 section.

ARCHITECTURE:

1. InterceptionManager.cs (MonoBehaviour)
   - Coordinates turret-to-threat assignments
   - Attached to Ship
   - Properties:
     * pdSystem: PDSystem
     * threatDetector: ThreatDetector
     * pendingInterceptions: Queue<InterceptionAttempt>
     * resolvedThisPhase: List<InterceptionResult>
   - Methods:
     * void ProcessInterceptions() - Main loop during Simulation
     * void AssignTurretsToThreats() - Match turrets to prioritized threats
     * InterceptionResult ResolveInterception(PDTurret, TrackedThreat)
     * void OnInterceptionSuccess(Projectile) - Destroy projectile
     * void OnInterceptionFailure(Projectile) - Projectile continues

2. InterceptionAttempt.cs (struct)
   - Pending interception data:
     * turret: PDTurret
     * threat: TrackedThreat
     * interceptChance: float (pre-calculated)

3. InterceptionResult.cs (expand from 4.1)
   - Full result data:
     * success: bool
     * turretId: string
     * projectileType: ProjectileType
     * interceptChance: float
     * roll: float (0-1)
     * timestamp: float

4. Turret Assignment Logic:
   - For each threat in priority order:
     * Find turrets with coverage AND capacity
     * Assign closest/best turret
     * Reduce turret capacity
     * Mark threat as engaged
   - Multiple turrets can engage same threat if capacity allows

5. Intercept Resolution:
   - Roll random 0-1
   - Compare to intercept chance (with modifiers)
   - Success: Destroy projectile, log result
   - Failure: Projectile continues, log result
   - Turret capacity consumed either way

6. Timing Integration:
   - Interceptions resolve during Simulation phase
   - After projectile movement update, before damage application
   - Visual: Brief delay for PD "firing" effect

7. Modify Projectile.cs
   - Method: Intercept() - Called on successful intercept
   - Triggers destruction with PD-specific effect

EDITOR AUTOMATION:

8. InterceptionTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Interception Test Scene"
   - Creates:
     * Ship with full PD
     * Projectile spawner
     * Interception resolver
     * Result logging UI

9. InterceptionTestController.cs
   - "Spawn & Intercept" buttons for each type
   - Manual intercept chance override (for testing)
   - Force success/failure toggles
   - Resolution log showing:
     * Turret ID
     * Target type
     * Chance
     * Roll
     * Result
   - Statistics: Total attempts, successes, failures, by type

10. InterceptionVisualizer.cs
    - Visual feedback for interceptions
    - Line from turret to intercept point
    - Success: Green flash, projectile destroyed
    - Failure: Red flash, projectile continues
    - Brief turret "firing" animation placeholder

UNIT TESTS (InterceptionMechanicsTests.cs):

1. Test_TurretAssignment - Threat gets assigned available turret
2. Test_TurretCapacityRespected - No over-assignment
3. Test_MultiTurretSameThreat - Multiple turrets can engage one threat
4. Test_InterceptSuccess - Roll under chance = success
5. Test_InterceptFailure - Roll over chance = failure
6. Test_TorpedoInterceptChance - 80% applied correctly
7. Test_MissileInterceptChance - 60% applied correctly
8. Test_CannonShellInterceptChance - 30% applied correctly
9. Test_SuccessDestroysProjectile - Projectile removed on success
10. Test_FailureContinuesProjectile - Projectile survives failure
11. Test_DamagedTurretReducedCapacity - Half engagements
12. Test_DestroyedTurretNoEngagement - Cannot engage

STATUS UPDATE:
- ✅ Step 4.3 Complete - Interception Mechanics
- List new/modified files
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 284/284 passing
- ⏭️ Next: Step 4.4 - PD Override Ability Integration
```

### VERIFICATION 4.3

1. **Run Setup**: Menu → Hephaestus → Testing → Create Interception Test Scene
2. **Run Tests**: Verify 222/222 passing
3. **Play Mode Verification**:
   - [ ] Spawned missile gets assigned to turret
   - [ ] Intercept roll displayed with chance and result
   - [ ] Successful intercept destroys projectile
   - [ ] Failed intercept lets projectile continue
   - [ ] Torpedo has higher intercept chance than missile
   - [ ] Cannon shell has lowest intercept chance
   - [ ] Turret capacity prevents over-engagement

---

## Step 4.4: PD Override Ability Integration

**Time**: 2-3 hours
**Prerequisites**: Step 4.3 complete (284 tests passing)

### CLAUDE CODE PROMPT 4.4

```
CONTEXT:
Step 4.3 complete. Interception mechanics functional.

The GDD specifies PD Override ability: doubles PD engagement rate (4 targets per turret instead of 2) for one turn. Heat cost 30, cooldown 3 turns.

OBJECTIVE:
Integrate existing PDOverrideData ability with the PD system.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 4 section.

ARCHITECTURE:

1. Modify PDOverrideData.cs
   - Update Execute() to:
     * Find PDSystem on ship
     * Call PDSystem.SetOverrideActive(true)
     * Set duration (1 turn)
   - Ensure proper turn-based expiration

2. Modify PDSystem.cs
   - Add: overrideActive property
   - Add: overrideTurnsRemaining
   - SetOverrideActive(bool) method
   - On turn end: Decrement override duration, deactivate if expired
   - When override active: All turrets have doubled targetsPerTurn

3. Modify PDTurret.cs
   - GetEffectiveCapacity() method
   - Returns base capacity × 2 if override active
   - Respects degradation (damaged = base/2, so override = base)

4. Turn Lifecycle Integration:
   - Override activated during Command phase (ability use)
   - Effect applies during Simulation phase
   - Duration tracked, expires at turn end

5. UI Feedback:
   - PDSystem override status visible
   - Turret capacity display updates when override active

EDITOR AUTOMATION:

6. PDOverrideTestSetup.cs
   - Menu: "Hephaestus/Testing/Create PD Override Test Scene"
   - Creates:
     * Ship with PD and ability system
     * Ability activation UI
     * Threat spawner
     * Capacity display

7. PDOverrideTestController.cs
   - "Activate PD Override" button
   - Heat cost display
   - Cooldown status
   - Per-turret capacity display (shows 2 vs 4)
   - Spawn multiple threats to test capacity
   - Turn advance button to test duration

UNIT TESTS (PDOverrideTests.cs):

1. Test_PDOverrideActivation - Ability activates override
2. Test_PDOverrideDoublesCapacity - 4 instead of 2
3. Test_PDOverrideHeatCost - 30 heat consumed
4. Test_PDOverrideCooldown - 3 turn cooldown
5. Test_PDOverrideDuration - Lasts 1 turn
6. Test_PDOverrideExpires - Returns to normal after duration
7. Test_DamagedTurretWithOverride - 1 × 2 = 2 capacity
8. Test_PDOverrideStacksCorrectly - No double-activation exploit

STATUS UPDATE:
- ✅ Step 4.4 Complete - PD Override Ability Integration
- List new/modified files
- 🧪 Unit Tests: 8/8 passing
- 🧪 Total Tests: 292/292 passing
- ⏭️ Next: Step 4.5 - PD Damage Integration
```

### VERIFICATION 4.4

1. **Run Setup**: Menu → Hephaestus → Testing → Create PD Override Test Scene
2. **Run Tests**: Verify 230/230 passing
3. **Play Mode Verification**:
   - [ ] PD Override ability visible in ability bar
   - [ ] Activation costs 30 heat
   - [ ] Turret capacity shows 4 instead of 2
   - [ ] Can engage more threats with override active
   - [ ] Override expires after 1 turn
   - [ ] 3 turn cooldown enforced

---

## Step 4.5: PD Damage Integration

**Time**: 2-3 hours
**Prerequisites**: Step 4.4 complete (292 tests passing)

### CLAUDE CODE PROMPT 4.5

```
CONTEXT:
Step 4.4 complete. PD Override working.

PD turrets are MountedSystems (Phase 3) that can be damaged by enemy fire. Need to integrate turret damage with PD functionality.

REFERENCE (from GDD):
- PD Turret size: 4 slots
- Damaged: Engagement rate halved
- Destroyed: Gap in coverage (cannot engage)

OBJECTIVE:
Integrate PD turrets with Phase 3 damage/degradation system.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 4 section.

ARCHITECTURE:

1. Ensure PDTurret extends MountedSystem
   - systemType = PDTurret
   - size = 4 slots
   - Inherits: Operational, Damaged, Destroyed states

2. Modify PDTurret.cs degradation effects:
   - Operational: Full capacity (2 targets, 4 with override)
   - Damaged: Half capacity (1 target, 2 with override)
   - Destroyed: Zero capacity, cannot engage

3. PDTurretDegradation.cs (or integrate into PDTurret)
   - Override ApplyDegradation() from MountedSystem
   - Update capacity based on state
   - Fire events for UI updates

4. Coverage Gap Handling:
   - When turret destroyed, that arc has reduced/no coverage
   - InterceptionManager skips destroyed turrets
   - Tactical implication: enemies can exploit gaps

5. Repair Integration (future-proofing):
   - Method stubs for repair mechanics
   - Restoring damaged turret restores capacity

6. Visual Feedback:
   - Damaged turret: Yellow arc visualization
   - Destroyed turret: No arc (or red/gray)
   - Section detail popup shows turret status

EDITOR AUTOMATION:

7. PDDamageTestSetup.cs
   - Menu: "Hephaestus/Testing/Create PD Damage Test Scene"
   - Creates:
     * Ship with PD turrets in sections
     * Damage controls per turret
     * Threat spawner
     * Coverage gap visualization

8. PDDamageTestController.cs
   - Per-turret controls:
     * Current state display
     * "Damage" button (Operational → Damaged)
     * "Destroy" button (→ Destroyed)
     * "Repair" button (restore)
   - Coverage map showing gaps
   - Spawn threat in gap to verify no interception

UNIT TESTS (PDDamageIntegrationTests.cs):

1. Test_PDTurretIsMountedSystem - Correct base class
2. Test_PDTurretSlotSize - Size = 4
3. Test_DamagedTurretHalvedCapacity - 1 instead of 2
4. Test_DestroyedTurretZeroCapacity - Cannot engage
5. Test_DestroyedTurretSkipped - InterceptionManager skips it
6. Test_CoverageGapExploitable - Threat in gap not intercepted
7. Test_CriticalHitCanDamageTurret - Phase 3 integration
8. Test_MultipleTurretsInSection - Independent damage

STATUS UPDATE:
- ✅ Step 4.5 Complete - PD Damage Integration
- List new/modified files
- 🧪 Unit Tests: 8/8 passing
- 🧪 Total Tests: 300/300 passing
- ⏭️ Next: Step 4.6 - PD UI and Feedback
```

### VERIFICATION 4.5

1. **Run Setup**: Menu → Hephaestus → Testing → Create PD Damage Test Scene
2. **Run Tests**: Verify 238/238 passing
3. **Play Mode Verification**:
   - [ ] Damaging turret changes capacity to 1
   - [ ] Destroying turret removes arc visualization
   - [ ] Destroyed turret cannot engage threats
   - [ ] Threat in coverage gap reaches ship
   - [ ] Section detail shows turret damage state
   - [ ] Critical hit from Phase 3 can damage turret

---

## Step 4.6: PD UI and Feedback

**Time**: 3-4 hours
**Prerequisites**: Step 4.5 complete (300 tests passing)

### CLAUDE CODE PROMPT 4.6

```
CONTEXT:
Step 4.5 complete. PD system fully functional.

Now creating UI to display PD status, coverage, and interception events to player.

OBJECTIVE:
Create comprehensive PD UI integrated with existing HUD.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 4 section.

ARCHITECTURE:

1. PDStatusPanel.cs (UI component)
   - Shows overall PD readiness
   - Total turrets: Operational / Damaged / Destroyed
   - Current capacity available
   - Override status indicator
   - Compact view for HUD

2. PDCoverageDisplay.cs
   - Visual representation of PD coverage
   - Top-down or 3D arc visualization
   - Color-coded by turret state
   - Highlights gaps in coverage
   - Optional toggle (can clutter screen)

3. InterceptionFeedback.cs
   - Real-time feedback during Simulation
   - Shows interception attempts as they happen
   - "PD ENGAGED" flash with target type
   - Success: "INTERCEPTED" + projectile explosion
   - Failure: "MISSED" + projectile continues
   - Brief, non-intrusive notifications

4. Modify CombatLogPanel.cs (Phase 3)
   - Add interception events to combat log
   - Format: "[PD] Turret #3 intercepted Missile"
   - Format: "[PD] Turret #5 missed Torpedo"
   - Category filter for PD events

5. Modify SectionDetailPopup.cs (Phase 3)
   - Show PD turrets mounted in section
   - Display turret state and capacity

6. PDThreatIndicator.cs
   - Warning when threats incoming
   - Shows count of tracked threats
   - Flashes when PD engaging
   - "SATURATED" warning when threats exceed capacity

EDITOR AUTOMATION:

7. PDUITestSetup.cs
   - Menu: "Hephaestus/Testing/Create PD UI Test Scene"
   - Creates:
     * Ship with full PD and UI
     * Threat spawner
     * All UI panels active
     * Various threat scenarios

8. PDUITestController.cs
   - Scenario buttons:
     * "Single Threat" - one missile
     * "Multiple Threats" - several missiles
     * "Saturation Attack" - exceed PD capacity
     * "Mixed Threats" - torpedoes and missiles
   - UI state verification checklist
   - Force intercept results for UI testing

UNIT TESTS (PDUITests.cs):

1. Test_PDStatusPanelUpdates - Shows correct counts
2. Test_CoverageDisplayAccurate - Arcs match turret state
3. Test_InterceptionFeedbackOnSuccess - Shows intercepted
4. Test_InterceptionFeedbackOnFailure - Shows missed
5. Test_CombatLogRecordsInterceptions - Events logged
6. Test_ThreatIndicatorWarning - Shows threat count
7. Test_SaturationWarning - Displays when overwhelmed
8. Test_OverrideStatusVisible - Shows when active

STATUS UPDATE:
- ✅ Step 4.6 Complete - PD UI and Feedback
- List new/modified files
- 🧪 Unit Tests: 8/8 passing
- 🧪 Total Tests: 308/308 passing
- ⏭️ Next: Step 4.7 - Phase 4 Integration Testing
```

### VERIFICATION 4.6

1. **Run Setup**: Menu → Hephaestus → Testing → Create PD UI Test Scene
2. **Run Tests**: Verify 246/246 passing
3. **Play Mode Verification**:
   - [ ] PD status panel shows turret counts
   - [ ] Coverage display shows arcs
   - [ ] Spawning threat shows threat indicator
   - [ ] Interception shows feedback (success/failure)
   - [ ] Combat log records PD events
   - [ ] Saturation warning when overwhelmed
   - [ ] PD Override status visible when active

---

## Step 4.7: Phase 4 Integration Testing

**Time**: 2-3 hours
**Prerequisites**: Step 4.6 complete (308 tests passing)

### CLAUDE CODE PROMPT 4.7

```
CONTEXT:
All Phase 4 systems implemented. Creating comprehensive integration tests.

OBJECTIVE:
Create integration tests verifying complete PD flow and combat scenarios.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 4 section.

ARCHITECTURE:

1. Phase4IntegrationTests.cs
   - End-to-end PD scenario tests

2. PDIntegrationTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Full PD Test Scene"
   - Complete combat scenario:
     * Player ship with full PD
     * Enemy ship (or spawner) that fires missiles/torpedoes
     * Full UI
     * Turn system active

3. PDCombatTestController.cs
   - Combat scenario presets:
     * "Light Attack" - 1-2 missiles, easily handled
     * "Standard Attack" - matches PD capacity
     * "Saturation Attack" - exceeds capacity, some get through
     * "Torpedo Strike" - slow but deadly
     * "Mixed Salvo" - torpedoes + missiles
     * "Exploit Gap" - attack from destroyed turret arc
   - Step-by-step result verification
   - Statistics tracking

INTEGRATION TESTS:

1. Test_FullPDFlow_DetectToIntercept
   - Fire missile at ship
   - Verify: detected → prioritized → assigned → intercepted

2. Test_SaturationAttack_SomeGetThrough
   - Fire more missiles than PD capacity
   - Verify: some intercepted, some hit ship

3. Test_TorpedoHigherInterceptChance
   - Fire torpedo
   - Verify: 80% chance applied

4. Test_PDOverrideHandlesSaturation
   - Activate PD Override
   - Fire saturation attack
   - Verify: more intercepted than without override

5. Test_DamagedTurretReducedEffectiveness
   - Damage turrets
   - Fire missiles
   - Verify: fewer intercepted

6. Test_DestroyedTurretCoverageGap
   - Destroy all turrets on one side
   - Fire from that side
   - Verify: missiles hit ship unimpeded

7. Test_PDIntegrationWithDamageSystem
   - Missiles that get through
   - Verify: proper damage routing (shields → sections)

8. Test_CombatLogRecordsAllEvents
   - Full combat sequence
   - Verify: all events logged correctly

9. Test_UIReflectsPDState
   - Various PD states
   - Verify: UI accurate throughout

10. Test_TurnCycleResetsCapacity
    - Exhaust PD capacity
    - Advance turn
    - Verify: capacity reset

STATUS UPDATE:
- ✅ Step 4.7 Complete - Phase 4 Integration Testing
- ✅ PHASE 4 COMPLETE
- List new/modified files
- 🧪 Integration Tests: 10/10 passing
- 🧪 Total Tests: 318/318 passing
- 📊 Phase 4 Summary: [List all major systems implemented]
- ⏭️ Next Phase: Phase 5 - Enemy AI
```

### VERIFICATION 4.7

1. **Run Setup**: Menu → Hephaestus → Testing → Create Full PD Test Scene
2. **Run All Tests**: Verify 256/256 passing
3. **Manual Combat Scenarios**:

   **Light Attack**:
   - [ ] 1-2 missiles fired
   - [ ] PD detects and intercepts all
   - [ ] No damage to ship
   
   **Standard Attack**:
   - [ ] Missiles match PD capacity
   - [ ] Most or all intercepted
   - [ ] Turrets show engaged state
   
   **Saturation Attack**:
   - [ ] More missiles than capacity
   - [ ] Some get through
   - [ ] Ship takes damage
   - [ ] "SATURATED" warning appears
   
   **Torpedo Strike**:
   - [ ] Slower projectiles
   - [ ] Higher intercept chance visible
   - [ ] More likely intercepted
   
   **Mixed Salvo**:
   - [ ] Both types tracked
   - [ ] Torpedoes prioritized
   - [ ] Appropriate intercept chances
   
   **Exploit Gap**:
   - [ ] Destroy port turrets
   - [ ] Attack from port
   - [ ] Missiles hit without interception

---

## Phase 4 Summary

### Systems Implemented
| System | Description |
|--------|-------------|
| PD Turrets | Individual turrets with arc coverage |
| Threat Detection | Tracking incoming projectiles |
| Interception | Probabilistic intercept rolls |
| PD Override | Ability integration (2x capacity) |
| PD Damage | Turret degradation from combat |
| PD UI | Status, coverage, feedback |

### Test Coverage
| Step | Tests Added | Running Total |
|------|-------------|---------------|
| Phase 3.5 (Integration) | 52 | 250 |
| 4.1 Turrets | 12 | 262 |
| 4.2 Detection | 10 | 272 |
| 4.3 Interception | 12 | 284 |
| 4.4 Override | 8 | 292 |
| 4.5 Damage | 8 | 300 |
| 4.6 UI | 8 | 308 |
| 4.7 Integration | 10 | 318 |

### Files Created
- ~15 new script files
- ~7 Editor scripts
- ~7 test files
- ~7 test scenes

### Key Tactical Implications
- Saturation attacks can overwhelm PD
- Arc gaps are exploitable
- Torpedo/missile mix creates dilemmas
- PD Override is valuable against salvos
- Damaged turrets create vulnerabilities

### Next Phase
Phase 5: Enemy AI
- AI decision making
- Threat assessment
- Movement and weapon planning
- Exploitation of player weaknesses

---

## Appendix: Quick Reference

### Menu Items Created
```
Hephaestus/Setup/Add PD Turrets to Selected Ship
Hephaestus/Testing/Create PD Turret Test Scene
Hephaestus/Testing/Create Threat Detection Test Scene
Hephaestus/Testing/Create Interception Test Scene
Hephaestus/Testing/Create PD Override Test Scene
Hephaestus/Testing/Create PD Damage Test Scene
Hephaestus/Testing/Create PD UI Test Scene
Hephaestus/Testing/Create Full PD Test Scene
```

### Key Classes
```
PDTurret (MonoBehaviour, extends MountedSystem)
PDSystem (MonoBehaviour)
ThreatDetector (MonoBehaviour)
TrackedThreat (class)
InterceptionManager (MonoBehaviour)
InterceptionResult (struct)
ThreatPrioritizer
PDStatusPanel (UI)
PDCoverageDisplay (UI)
InterceptionFeedback (UI)
```

### Intercept Chances
```
Torpedo: 60% + 20% = 80%
Missile: 60% + 0% = 60%
Cannon Shell: 60% - 30% = 30%
```

### Capacity Reference
```
Normal: 2 targets/turret
With Override: 4 targets/turret
Damaged: Half normal
Destroyed: 0
```