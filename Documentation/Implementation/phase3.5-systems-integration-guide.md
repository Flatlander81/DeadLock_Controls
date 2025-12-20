# Hephaestus Implementation Guide - Phase 3.5: Systems Integration

---

## Document Overview

**Phase 3.5** integrates all existing systems (Movement, Turn Management, Weapons, Damage) into a cohesive combat loop. This phase ensures that all systems work together seamlessly before adding new features.

**Prerequisites**: Phase 3 Complete (198 tests passing)

**Estimated Time**: 15-20 hours

**New Tests**: ~40 automated tests

---

## Why This Phase Exists

Phases 1-3 built individual systems in isolation:
- **Phase 1**: Movement planning and execution
- **Phase 2**: Weapon systems and projectiles
- **Phase 2.2**: Weapon firing and projectile spawning
- **Phase 3**: Damage routing, shields, sections, criticals

These systems were tested individually but need explicit integration to form a complete combat loop:

```
┌─────────────────────────────────────────────────────────────┐
│                    COMPLETE COMBAT LOOP                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────┐                                           │
│  │ COMMAND      │  Player plans:                            │
│  │ PHASE        │  - Movement (drag projection)             │
│  │              │  - Weapon targets (select enemies)        │
│  │              │  - Weapon groups to fire                  │
│  │              │  - Abilities to use                       │
│  └──────┬───────┘                                           │
│         ↓                                                    │
│  ┌──────────────┐                                           │
│  │ END TURN     │  "End Turn" button commits all plans      │
│  └──────┬───────┘                                           │
│         ↓                                                    │
│  ┌──────────────┐                                           │
│  │ SIMULATION   │  Simultaneous execution:                  │
│  │ PHASE        │  1. Ships move to planned positions       │
│  │              │  2. Weapons fire (with spin-up delays)    │
│  │              │  3. Projectiles travel toward targets     │
│  │              │  4. PD attempts interception (Phase 4)    │
│  │              │  5. Projectiles hit → Damage routing      │
│  │              │  6. Criticals resolve                     │
│  │              │  7. Ship destruction checks               │
│  └──────┬───────┘                                           │
│         ↓                                                    │
│  ┌──────────────┐                                           │
│  │ TURN END     │  Cleanup:                                 │
│  │              │  - Reset weapon cooldowns (tick down)     │
│  │              │  - Apply heat dissipation                 │
│  │              │  - Check victory/defeat                   │
│  │              │  - Increment turn counter                 │
│  └──────┬───────┘                                           │
│         ↓                                                    │
│  ┌──────────────┐                                           │
│  │ NEXT TURN    │  Return to Command Phase                  │
│  └──────────────┘                                           │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## GDD Reference Data

**Turn Structure (from GDD):**
- Command Phase: Unlimited time for planning
- Simulation Phase: ~3 seconds real-time execution
- All actions resolve simultaneously (WEGO system)

**Heat Per Turn:**
- Base dissipation: 10 heat/turn
- Radiator bonus: +5 heat/turn per operational radiator
- Damaged radiator: Half effectiveness

**Cooldown Mechanics:**
- Weapon cooldowns tick down by 1 each turn
- Ability cooldowns tick down by 1 each turn
- Cooldown of 0 = ready to use

**Projectile Timing:**
- Instant-hit weapons (RailGun): Resolve immediately after spin-up
- Ballistic projectiles: Travel during simulation, may take multiple turns
- Homing projectiles: Track target, may take multiple turns

---

## Step 3.5.0: Establish Integration Standards

**Time**: 15 minutes
**Prerequisites**: Phase 3 complete

### CLAUDE CODE PROMPT 3.5.0

```
CONTEXT:
Phase 3 complete with 198 tests passing. Beginning Phase 3.5: Systems Integration.

OBJECTIVE:
Add Phase 3.5 standards section to CLAUDE.md.

TASK:
Append the following section to CLAUDE.md:

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
Assets/Scripts/Combat/Integration/   # Integration coordinator scripts
Assets/Editor/Integration/           # Editor automation
Assets/Tests/PlayModeTests/Integration/  # Integration tests
```

---

Create the folder structure listed above if directories don't exist.

STATUS UPDATE:
After completion, note in IMPLEMENTATION_STATUS.md:
- ✅ Step 3.5.0 Complete - Phase 3.5 Standards Established
- 📁 Modified: CLAUDE.md
- 📁 Created: Folder structure for Phase 3.5
- ⏭️ Next: Step 3.5.1 - Turn System Event Infrastructure
```

### VERIFICATION 3.5.0

- [ ] CLAUDE.md contains Phase 3.5 standards section
- [ ] All folder paths exist

---

## Step 3.5.1: Turn System Event Infrastructure

**Time**: 3-4 hours
**Prerequisites**: Step 3.5.0 complete

### CLAUDE CODE PROMPT 3.5.1

```
CONTEXT:
Phase 3.5 standards established. Beginning turn system event infrastructure.

The TurnManager exists but needs proper events for other systems to subscribe to.

OBJECTIVE:
Enhance TurnManager with comprehensive event system for combat coordination.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3.5 section.

ARCHITECTURE:

1. Modify TurnManager.cs
   - Add events:
     * OnCommandPhaseStart (Action)
     * OnSimulationPhaseStart (Action)
     * OnSimulationPhaseEnd (Action)
     * OnTurnEnd (Action<int> turnNumber)
     * OnTurnStart (Action<int> turnNumber)
   - Add properties:
     * CurrentTurn: int
     * CurrentPhase: TurnPhase enum
     * SimulationProgress: float (0-1)
   - Ensure events fire at correct times
   - Add turn counter that increments each cycle

2. TurnPhase.cs (enum)
   - Command
   - Simulation
   - TurnEnd
   - (matches existing Phase enum if present)

3. TurnEventSubscriber.cs (helper base class)
   - Virtual methods for each event
   - Auto-subscribes in OnEnable, unsubscribes in OnDisable
   - Subclasses override only the events they care about

4. Modify Ship.cs
   - Subscribe to OnSimulationPhaseStart for movement execution
   - Subscribe to OnTurnEnd for any per-turn cleanup

5. CombatCoordinator.cs (new MonoBehaviour)
   - Central coordinator for combat during simulation
   - Subscribes to TurnManager events
   - Orchestrates execution order:
     1. Movement first (ships reach new positions)
     2. Weapons fire (after movement settles)
     3. Projectiles update (travel toward targets)
     4. Damage resolves (hits process through DamageRouter)
   - Properties:
     * movementDuration: float (portion of simulation for movement)
     * weaponsDuration: float (portion for weapon firing)
   - Methods:
     * void OnSimulationStart()
     * IEnumerator ExecuteSimulation()
     * void OnSimulationEnd()

EDITOR AUTOMATION:

6. TurnSystemTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Turn System Test Scene"
   - Creates:
     * TurnManager with all events
     * CombatCoordinator
     * Test ships with movement planned
     * Event logging display

7. TurnSystemTestController.cs
   - OnGUI showing:
     * Current phase
     * Current turn number
     * Event log (timestamped)
     * Manual phase transition buttons
     * Simulation progress bar
   - Buttons: "Start Command", "Start Simulation", "End Turn"

UNIT TESTS (TurnSystemIntegrationTests.cs):

1. Test_TurnManagerEventsExist - All events accessible
2. Test_CommandPhaseEventFires - Event fires on command phase
3. Test_SimulationPhaseEventFires - Event fires on simulation start
4. Test_TurnEndEventFires - Event fires with turn number
5. Test_TurnCounterIncrements - Increases each turn
6. Test_PhaseTransitionOrder - Correct sequence
7. Test_SubscriberReceivesEvents - TurnEventSubscriber works
8. Test_CombatCoordinatorOrchestrates - Correct execution order
9. Test_SimulationProgressUpdates - Progress 0→1 during simulation
10. Test_MultipleSubscribers - All subscribers notified

STATUS UPDATE:
- ✅ Step 3.5.1 Complete - Turn System Event Infrastructure
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 208/208 passing
- ⏭️ Next: Step 3.5.2 - Weapon Firing Integration
```

### VERIFICATION 3.5.1

1. **Run Setup**: Menu → Hephaestus → Testing → Create Turn System Test Scene
2. **Run Tests**: Verify 208/208 passing
3. **Play Mode Verification**:
   - [ ] Events fire in correct order
   - [ ] Turn counter increments
   - [ ] Phase transitions work
   - [ ] Event log shows all events
   - [ ] Multiple systems receive events

---

## Step 3.5.2: Weapon Firing Integration

**Time**: 4-5 hours
**Prerequisites**: Step 3.5.1 complete (208 tests passing)

### CLAUDE CODE PROMPT 3.5.2

```
CONTEXT:
Step 3.5.1 complete. Turn system events functional.

Now integrating weapon firing with the turn system. Weapons should queue during Command Phase and fire during Simulation Phase.

OBJECTIVE:
Create weapon firing queue that integrates with turn phases.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3.5 section.

ARCHITECTURE:

1. WeaponFiringQueue.cs (MonoBehaviour)
   - Queues weapon firing commands during Command Phase
   - Executes queue during Simulation Phase
   - Properties:
     * queuedCommands: List<WeaponFireCommand>
     * isExecuting: bool
   - Methods:
     * void QueueFire(WeaponSystem weapon, Ship target)
     * void QueueGroupFire(int groupNumber, Ship target)
     * void QueueAlphaStrike(Ship target)
     * void ExecuteQueue() - Called by CombatCoordinator
     * void ClearQueue() - Called on turn end
     * int GetQueuedHeatCost() - Preview total heat

2. WeaponFireCommand.cs (struct)
   - weapon: WeaponSystem
   - target: Ship
   - groupNumber: int (optional)
   - queueTime: float

3. Modify WeaponManager.cs
   - Add reference to WeaponFiringQueue
   - QueueFireGroup(int group, Ship target) → queues instead of immediate fire
   - QueueAlphaStrike(Ship target) → queues all weapons
   - GetQueuedCommands() → returns pending fire commands
   - CancelQueuedCommand(WeaponSystem weapon)

4. Modify WeaponSystem.cs
   - Add: isQueuedToFire property
   - Visual indicator when queued (e.g., pulsing glow)
   - CanQueueFire() → checks cooldown, ammo, but not arc (arc checked at execution)

5. Integration with CombatCoordinator:
   - After movement completes:
     * Recalculate weapon arcs (ship may have moved)
     * Execute weapon queue
     * Weapons that are now out of arc: Skip with warning
   - Stagger weapon firing based on spin-up times

6. Heat Preview System:
   - During Command Phase, show projected heat after firing
   - WeaponFiringQueue.GetQueuedHeatCost() returns total
   - UI shows: Current Heat + Queued = Projected Heat
   - Warning if projected heat > safe threshold

7. Modify existing weapon UI:
   - Show "QUEUED" status on queued weapons
   - Show projected heat bar
   - Allow cancel of queued commands

EDITOR AUTOMATION:

8. WeaponFiringTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Weapon Firing Integration Test Scene"
   - Creates:
     * Player ship with weapons
     * Enemy ship as target
     * WeaponFiringQueue
     * CombatCoordinator
     * Queue visualization UI

9. WeaponFiringTestController.cs
   - Weapon queue display
   - "Queue Group 1-4" buttons
   - "Queue Alpha Strike" button
   - "Clear Queue" button
   - Heat preview display
   - "Execute Turn" button
   - Result log showing what fired

UNIT TESTS (WeaponFiringIntegrationTests.cs):

1. Test_WeaponQueuesDuringCommand - Commands queue, don't fire
2. Test_WeaponExecutesDuringSimulation - Queue fires on simulation
3. Test_QueuedHeatPreview - Correct heat preview
4. Test_OutOfArcSkipped - Weapon skipped if arc invalid after move
5. Test_SpinUpTimingRespected - Weapons fire in spin-up order
6. Test_CooldownBlocksQueue - Can't queue weapon on cooldown
7. Test_AmmoBlocksQueue - Can't queue empty weapon
8. Test_QueueClearedOnTurnEnd - Queue resets each turn
9. Test_CancelQueuedWeapon - Can remove from queue
10. Test_AlphaStrikeQueuesAll - All weapons queued
11. Test_GroupFireQueuesGroup - Only group weapons queued
12. Test_MultipleTargetsSupported - Different weapons, different targets

STATUS UPDATE:
- ✅ Step 3.5.2 Complete - Weapon Firing Integration
- List new/modified files
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 220/220 passing
- ⏭️ Next: Step 3.5.3 - Heat and Cooldown Turn Integration
```

### VERIFICATION 3.5.2

1. **Run Setup**: Menu → Hephaestus → Testing → Create Weapon Firing Integration Test Scene
2. **Run Tests**: Verify 220/220 passing
3. **Play Mode Verification**:
   - [ ] Weapons queue during Command Phase (don't fire immediately)
   - [ ] Queued weapons show visual indicator
   - [ ] Heat preview shows projected total
   - [ ] "End Turn" executes all queued weapons
   - [ ] Weapons fire with proper spin-up timing
   - [ ] Out-of-arc weapons skipped with warning

---

## Step 3.5.3: Heat and Cooldown Turn Integration

**Time**: 3-4 hours
**Prerequisites**: Step 3.5.2 complete (220 tests passing)

### CLAUDE CODE PROMPT 3.5.3

```
CONTEXT:
Step 3.5.2 complete. Weapon firing integrated with turns.

Now integrating heat dissipation and cooldown tick-down with turn end.

REFERENCE (from GDD):
- Heat dissipates at turn end: Base 10 + radiator bonus
- Weapon cooldowns tick down by 1 at turn end
- Ability cooldowns tick down by 1 at turn end

OBJECTIVE:
Create turn-end processor that handles heat and cooldowns.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3.5 section.

ARCHITECTURE:

1. TurnEndProcessor.cs (MonoBehaviour)
   - Handles all turn-end cleanup
   - Subscribes to TurnManager.OnTurnEnd
   - Methods:
     * void ProcessTurnEnd(int turnNumber)
     * void ProcessHeatDissipation(Ship ship)
     * void ProcessCooldowns(Ship ship)
     * void ProcessAllShips()

2. Modify HeatManager.cs
   - Add: baseDissipationRate (default 10)
   - Add: GetRadiatorBonus() - checks operational radiators
   - Add: DissipateHeat() - called at turn end
   - Dissipation formula: baseDissipation + (radiatorCount * 5)
   - Damaged radiator: Half bonus
   - Destroyed radiator: No bonus

3. Modify WeaponSystem.cs
   - Add: TickCooldown() - reduces cooldown by 1
   - Called at turn end
   - Cooldown can't go below 0

4. Modify AbilitySystem.cs (if exists)
   - Add: TickAllCooldowns() - reduces all ability cooldowns
   - Called at turn end

5. Heat Dissipation Events:
   - OnHeatDissipated(float amount)
   - UI can show "-10 Heat" feedback

6. Cooldown Ready Events:
   - OnWeaponReady(WeaponSystem weapon) - when cooldown reaches 0
   - OnAbilityReady(AbilityType ability) - when cooldown reaches 0
   - UI can show "Weapon Ready!" notification

7. Turn Summary Display:
   - At turn end, show summary:
     * Heat dissipated: X
     * Weapons now ready: [list]
     * Abilities now ready: [list]

EDITOR AUTOMATION:

8. HeatCooldownTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Heat Cooldown Test Scene"
   - Creates:
     * Ship with heat manager and weapons
     * TurnEndProcessor
     * Radiators (some damaged)
     * Heat/cooldown display UI

9. HeatCooldownTestController.cs
   - Heat controls: "Add 50 Heat", "Add 100 Heat"
   - Cooldown controls: "Set Weapon Cooldown to 3"
   - "Advance Turn" button
   - Display: Heat before/after, Cooldowns before/after
   - Radiator status controls

UNIT TESTS (HeatCooldownIntegrationTests.cs):

1. Test_HeatDissipatesOnTurnEnd - Heat reduces by base amount
2. Test_RadiatorBonusApplied - Extra dissipation with radiators
3. Test_DamagedRadiatorHalfBonus - Half effectiveness
4. Test_DestroyedRadiatorNoBonus - No contribution
5. Test_WeaponCooldownTicks - Reduces by 1
6. Test_CooldownStopsAtZero - Doesn't go negative
7. Test_AbilityCooldownTicks - Abilities also tick
8. Test_WeaponReadyEventFires - Event when cooldown hits 0
9. Test_HeatDissipationEvent - Event fires with amount
10. Test_MultipleShipsProcessed - All ships handled

STATUS UPDATE:
- ✅ Step 3.5.3 Complete - Heat and Cooldown Turn Integration
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 230/230 passing
- ⏭️ Next: Step 3.5.4 - Movement and Weapon Arc Integration
```

### VERIFICATION 3.5.3

1. **Run Setup**: Menu → Hephaestus → Testing → Create Heat Cooldown Test Scene
2. **Run Tests**: Verify 230/230 passing
3. **Play Mode Verification**:
   - [ ] Heat dissipates at turn end
   - [ ] Radiators provide bonus dissipation
   - [ ] Damaged radiators give half bonus
   - [ ] Weapon cooldowns tick down each turn
   - [ ] "Ready" notification when cooldown reaches 0
   - [ ] Ability cooldowns also tick

---

## Step 3.5.4: Movement and Weapon Arc Integration

**Time**: 3-4 hours
**Prerequisites**: Step 3.5.3 complete (230 tests passing)

### CLAUDE CODE PROMPT 3.5.4

```
CONTEXT:
Step 3.5.3 complete. Heat and cooldowns integrated.

Now integrating movement planning with weapon arc visualization. Players need to see where their weapons will be able to fire AFTER they move.

OBJECTIVE:
Create weapon arc preview that updates based on planned movement.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3.5 section.

ARCHITECTURE:

1. WeaponArcPreview.cs (MonoBehaviour)
   - Visualizes weapon firing arcs from planned position
   - Shows which enemies will be in arc after move
   - Properties:
     * showArcs: bool
     * arcColor: Color (green for in-arc targets, red for out)
     * previewFromPlannedPosition: bool
   - Methods:
     * void UpdateArcPreview()
     * List<Ship> GetTargetsInArc(WeaponSystem weapon, Vector3 fromPosition)
     * void DrawArcGizmos()

2. Modify Ship.cs
   - Add: GetPlannedForward() - direction ship will face after move
   - Add: GetPlannedPosition() - where ship will be (already exists as PlannedPosition)
   - Projection should show facing direction

3. Modify WeaponSystem.cs
   - Add: CanFireFromPosition(Vector3 position, Quaternion rotation, Ship target)
   - Checks if weapon could fire from hypothetical position
   - Used for arc preview

4. Arc Visualization:
   - Draw cone/wedge showing weapon's firing arc
   - Cone originates from planned position, not current
   - Enemies inside cone: Highlighted green
   - Enemies outside cone: Highlighted red/gray
   - Toggle per weapon or all weapons

5. Target Reachability Indicator:
   - On enemy ships, show indicator:
     * Green: In arc of at least one weapon after move
     * Yellow: In arc of some weapons but not all
     * Red: Out of arc of all weapons
   - Updates as player drags movement projection

6. Broadside Weapon Preview:
   - For TorpedoLauncher and MissileBattery (broadside weapons)
   - Show both port and starboard arcs
   - Indicate which side will fire based on target position

7. Integration with MovementController:
   - When dragging projection, update arc preview
   - Real-time feedback on weapon coverage

EDITOR AUTOMATION:

8. WeaponArcTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Weapon Arc Preview Test Scene"
   - Creates:
     * Player ship with various weapons
     * Multiple enemy ships at different positions
     * Arc visualization enabled
     * Movement projection

9. WeaponArcTestController.cs
   - Toggle arc display per weapon type
   - Toggle preview from current vs planned position
   - Show in-arc target list
   - Drag projection to see arc updates

UNIT TESTS (WeaponArcIntegrationTests.cs):

1. Test_ArcPreviewFromPlannedPosition - Uses planned position
2. Test_TargetInArcDetection - Correctly identifies targets
3. Test_TargetOutOfArcDetection - Correctly excludes targets
4. Test_ArcUpdatesOnProjectionMove - Real-time update
5. Test_BroadsideArcPreview - Shows both sides
6. Test_MultipleWeaponArcs - All weapons shown
7. Test_PlannedForwardDirection - Correct facing after move
8. Test_TurretWeapon360Arc - Full coverage shown
9. Test_NarrowArcVisualization - Correct cone size
10. Test_ReachabilityIndicators - Enemy indicators correct

STATUS UPDATE:
- ✅ Step 3.5.4 Complete - Movement and Weapon Arc Integration
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 240/240 passing
- ⏭️ Next: Step 3.5.5 - Unified Combat Test Level
```

### VERIFICATION 3.5.4

1. **Run Setup**: Menu → Hephaestus → Testing → Create Weapon Arc Preview Test Scene
2. **Run Tests**: Verify 240/240 passing
3. **Play Mode Verification**:
   - [ ] Weapon arcs visible as cones/wedges
   - [ ] Arcs originate from planned position
   - [ ] Enemies in arc highlighted green
   - [ ] Enemies out of arc highlighted red
   - [ ] Arcs update as projection is dragged
   - [ ] Broadside weapons show both sides

---

## Step 3.5.5: Unified Combat Test Level

**Time**: 3-4 hours
**Prerequisites**: Step 3.5.4 complete (240 tests passing)

### CLAUDE CODE PROMPT 3.5.5

```
CONTEXT:
Step 3.5.4 complete. All individual integrations functional.

Now creating unified test level that demonstrates the complete combat loop.

OBJECTIVE:
Create comprehensive test level showing full turn-based combat flow.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3.5 section.

ARCHITECTURE:

1. UnifiedCombatTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Unified Combat Test Level"
   - Creates complete combat scenario:
     * Player ship (Hephaestus) with full loadout
     * 2 Enemy ships at strategic positions
     * TurnManager, CombatCoordinator, TurnEndProcessor
     * WeaponFiringQueue
     * All UI panels (damage, weapons, status)
     * Arc visualization
     * Full combat loop enabled

2. UnifiedCombatTestController.cs
   - Master test controller combining all features
   - Tabs:
     * Overview: Turn counter, phase indicator, quick actions
     * Movement: Projection controls, arc preview toggle
     * Weapons: Queue display, fire buttons, heat preview
     * Damage: Section status, shield status, combat log
     * Debug: Event log, force phase transitions
   - "Complete Turn" button: Plans move → Queues weapons → Executes

3. Combat Flow Walkthrough Mode:
   - Step-by-step guided mode
   - Instructions appear for each action
   - Validates player performed action correctly
   - Steps:
     1. "Select your ship and plan movement"
     2. "Check weapon arcs from planned position"
     3. "Queue weapons to fire at enemy"
     4. "Review heat projection"
     5. "Click End Turn to execute"
     6. "Watch simulation resolve"
     7. "Review turn summary"

4. Combat Statistics Tracking:
   - Turns played
   - Damage dealt/taken
   - Weapons fired
   - Heat generated/dissipated
   - Cooldowns used

5. Quick Scenario Buttons:
   - "Reset to Start"
   - "Skip to Combat" (pre-positioned for immediate fighting)
   - "Low Health Scenario" (test near-death situations)
   - "Overheat Scenario" (high heat situation)

6. Validation Checklist (in-game):
   Display checkmarks as features are tested:
   - [ ] Movement planned and executed
   - [ ] Weapon queued and fired
   - [ ] Damage dealt to enemy
   - [ ] Heat accumulated and dissipated
   - [ ] Cooldown applied and ticked
   - [ ] Turn counter incremented
   - [ ] All phases executed

EDITOR AUTOMATION:

7. Update Phase3UnifiedTestSetup.cs
   - Rename to legacy or merge functionality
   - New unified setup includes all Phase 3.5 integration

8. CombatLoopValidator.cs
   - Validates all systems are properly connected
   - Reports missing components or broken links
   - Menu: "Hephaestus/Validate/Check Combat Loop Integration"

INTEGRATION TESTS (UnifiedCombatTests.cs):

1. Test_FullTurnCycle - Command → Simulation → TurnEnd → Command
2. Test_MovementThenWeaponFire - Move executes before weapons
3. Test_WeaponArcRecalculatedAfterMove - Arc check uses new position
4. Test_HeatAccumulatesFromFiring - Heat increases when weapons fire
5. Test_HeatDissipatesAtTurnEnd - Heat reduces at turn end
6. Test_CooldownsTickAtTurnEnd - Cooldowns reduce
7. Test_ProjectileHitsDealsDamage - Full damage pipeline
8. Test_MultiTurnCombat - Multiple turns work correctly
9. Test_BroadsideWeaponSelectsSide - Correct side fires
10. Test_QueuedWeaponOutOfArcSkipped - Skips if arc invalid after move

STATUS UPDATE:
- ✅ Step 3.5.5 Complete - Unified Combat Test Level
- List new/modified files
- 🧪 Integration Tests: 10/10 passing
- 🧪 Total Tests: 250/250 passing
- ⏭️ Next: Step 3.5.6 - Phase 3.5 Final Validation
```

### VERIFICATION 3.5.5

1. **Run Setup**: Menu → Hephaestus → Testing → Create Unified Combat Test Level
2. **Run Tests**: Verify 250/250 passing
3. **Play Mode - Full Combat Loop**:

   **Turn 1**:
   - [ ] Select player ship, drag projection to new position
   - [ ] See weapon arcs update from planned position
   - [ ] Queue RailGuns to fire at enemy
   - [ ] See heat preview update
   - [ ] Click "End Turn"
   - [ ] Watch ship move, then weapons fire
   - [ ] See projectiles hit enemy, damage applied
   - [ ] Turn ends, heat dissipates

   **Turn 2**:
   - [ ] Turn counter shows 2
   - [ ] Weapon cooldowns have ticked
   - [ ] Can plan new movement
   - [ ] Can queue weapons again
   - [ ] Repeat combat loop

---

## Step 3.5.6: Phase 3.5 Final Validation

**Time**: 2-3 hours
**Prerequisites**: Step 3.5.5 complete (250 tests passing)

### CLAUDE CODE PROMPT 3.5.6

```
CONTEXT:
All Phase 3.5 systems implemented. Final validation and documentation.

OBJECTIVE:
Validate all integrations work together and update documentation.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 3.5 section.

TASKS:

1. Run Full Test Suite
   - Verify all 250 tests pass
   - Document any flaky tests
   - Fix any failures

2. Update IMPLEMENTATION_STATUS.md
   - Mark Phase 3.5 complete
   - Document test counts
   - List all new files

3. Update Phase3_DamageSystem_TestingGuide.md
   - Add section for integrated combat testing
   - Include turn-based combat walkthrough
   - Update keyboard shortcuts

4. Create Phase3.5_Integration_TestingGuide.md
   - Complete testing guide for integrated systems
   - Turn-by-turn test scenarios
   - Troubleshooting guide

5. Verify Documentation Accuracy:
   - All menu items work
   - All keyboard shortcuts work
   - All described features functional

6. Performance Validation:
   - Run unified combat for 20 turns
   - Monitor frame rate (should stay 60+)
   - Check for memory leaks
   - Document any performance issues

7. Final Integration Checklist:
   - [ ] TurnManager fires all events correctly
   - [ ] Weapons queue during Command, fire during Simulation
   - [ ] Movement executes before weapons fire
   - [ ] Weapon arcs checked from post-move position
   - [ ] Heat dissipates at turn end
   - [ ] Cooldowns tick at turn end
   - [ ] Damage flows through full pipeline
   - [ ] Multiple turns work correctly
   - [ ] No console errors during normal play

STATUS UPDATE:
- ✅ Step 3.5.6 Complete - Phase 3.5 Final Validation
- ✅ PHASE 3.5 COMPLETE
- List new/modified files
- 🧪 Total Tests: 250/250 passing
- 📊 Phase 3.5 Summary: [List all systems integrated]
- ⏭️ Next Phase: Phase 4 - Point Defense System
```

### VERIFICATION 3.5.6

1. **Run All Tests**: Verify 250/250 passing
2. **Complete Combat Session**:
   - Play 10 full turns of combat
   - Verify no errors in console
   - Verify frame rate stable
3. **Documentation Review**:
   - All guides accurate
   - All menu items functional

---

## Phase 3.5 Summary

### Systems Integrated
| System | Integration |
|--------|-------------|
| Turn Manager | Event system for phase coordination |
| Weapon Firing | Queue during Command, execute during Simulation |
| Heat Management | Dissipates at turn end with radiator bonus |
| Cooldowns | Tick down at turn end for weapons and abilities |
| Movement + Arcs | Arc preview from planned position |
| Combat Loop | Full turn cycle validated |

### Test Coverage
| Step | Tests Added | Running Total |
|------|-------------|---------------|
| 3.5.1 Turn Events | 10 | 208 |
| 3.5.2 Weapon Firing | 12 | 220 |
| 3.5.3 Heat/Cooldowns | 10 | 230 |
| 3.5.4 Movement/Arcs | 10 | 240 |
| 3.5.5 Unified Level | 10 | 250 |

### Key Classes Created
```
CombatCoordinator (orchestrates simulation)
WeaponFiringQueue (queues fire commands)
TurnEndProcessor (handles turn-end cleanup)
WeaponArcPreview (visualizes arcs from planned position)
TurnEventSubscriber (base class for event subscription)
```

### Combat Loop Flow
```
Command Phase:
  - Player plans movement
  - Player queues weapons
  - Player selects abilities
  ↓
End Turn:
  - Commit all plans
  ↓
Simulation Phase:
  1. Ships move (3 seconds)
  2. Weapons fire (spin-up delays)
  3. Projectiles travel
  4. Damage resolves
  ↓
Turn End:
  - Heat dissipates
  - Cooldowns tick
  - Turn counter increments
  ↓
Next Turn (Command Phase)
```

### Next Phase
Phase 4: Point Defense System
- Now properly integrated with turn system
- PD will intercept during Simulation Phase
- After movement, before damage resolution

---

## Appendix: Quick Reference

### Menu Items Created
```
Hephaestus/Testing/Create Turn System Test Scene
Hephaestus/Testing/Create Weapon Firing Integration Test Scene
Hephaestus/Testing/Create Heat Cooldown Test Scene
Hephaestus/Testing/Create Weapon Arc Preview Test Scene
Hephaestus/Testing/Create Unified Combat Test Level
Hephaestus/Validate/Check Combat Loop Integration
```

### Key Events
```
TurnManager.OnCommandPhaseStart
TurnManager.OnSimulationPhaseStart
TurnManager.OnSimulationPhaseEnd
TurnManager.OnTurnEnd(int turnNumber)
TurnManager.OnTurnStart(int turnNumber)
HeatManager.OnHeatDissipated(float amount)
WeaponSystem.OnWeaponReady
```

### Combat Timing (Simulation Phase)
```
0.0s - 1.0s: Movement execution
1.0s - 2.5s: Weapon firing (staggered by spin-up)
2.5s - 3.0s: Projectile resolution / cleanup
```
