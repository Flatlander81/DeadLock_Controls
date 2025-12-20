# Hephaestus Implementation Guide - Phase 5: Enemy AI

---

## Document Overview

**Phase 5** implements enemy AI that controls ships using the exact same systems as the player. No cheating - AI uses identical heat management, weapons, abilities, and constraints.

**Prerequisites**: Phase 4 Complete (318 tests passing)

**Estimated Time**: 30-40 hours

**New Tests**: ~68 automated tests

---

## Phase 3.5/4 Integration Dependencies

Phase 5 builds heavily on the integrated combat loop:

- **TurnManager Events**: AI subscribes to `OnCommandPhaseStart` to make decisions
- **WeaponFiringQueue**: AI queues weapons just like player
- **CombatCoordinator**: AI commits plans before simulation
- **Heat/Cooldown Integration**: AI manages heat budget using same turn-end processing

**AI Decision Timing:**
```
Command Phase Start:
  1. TurnManager fires OnCommandPhaseStart
  2. AIController.MakeDecisions() called
  3. AI evaluates state (AIContext)
  4. AI plans movement via Ship.PlanMove()
  5. AI queues weapons via WeaponFiringQueue
  6. AI commits abilities via AbilitySystem
  ↓
End Turn:
  AI plans execute alongside player plans
```

---

## Phase 5 Architecture Summary

```
┌─────────────────────────────────────────────────────────────┐
│                      AI DECISION LOOP                        │
├─────────────────────────────────────────────────────────────┤
│  Command Phase Start                                         │
│       ↓                                                      │
│  [Evaluate State] ─── Own ship, target ship, battlefield    │
│       ↓                                                      │
│  [Select Behavior] ─── Basic / Intermediate / Advanced      │
│       ↓                                                      │
│  [Plan Movement] ─── Position for optimal engagement        │
│       ↓                                                      │
│  [Plan Weapons] ─── Assign targets, manage heat budget      │
│       ↓                                                      │
│  [Plan Abilities] ─── Tactical ability usage                │
│       ↓                                                      │
│  [Commit Plans] ─── Same systems as player                  │
│       ↓                                                      │
│  Simulation Phase ─── Execute alongside player              │
└─────────────────────────────────────────────────────────────┘
```

---

## GDD Reference Data

**Critical Design Rule**: AI operates under identical constraints as player.

**AI Difficulty Tiers:**

| Tier | Movement | Weapons | Heat Management | Abilities |
|------|----------|---------|-----------------|-----------|
| Basic | Move toward target | Fire all weapons | Emergency Cooling at 100+ | Minimal |
| Intermediate | Maintain optimal range | Weapon groups by range | Stop at 80 heat | Defensive abilities |
| Advanced | Predict player movement | Coordinated fire | Multi-turn planning | Full tactical usage |

**Enemy Ship Types (from GDD):**

| Ship | Hull | Shields | Heat Cap | Speed | Turn | Weapons |
|------|------|---------|----------|-------|------|---------|
| Frigate | 200 | 100 | 120 | 25 | 60° | 2 Rail, 1 Missile |
| Destroyer | 350 | 150 | 140 | 18 | 45° | 1 Rail, 2 Cannon, 1 Missile |
| Battleship | 600 | 250 | 180 | 15 | 30° | 3 Rail, 2 Cannon, 2 Torp, 2 Missile |

---

## Step 5.0: Establish Phase 5 Standards

**Time**: 15 minutes
**Prerequisites**: Phase 4 complete (318 tests passing)

### CLAUDE CODE PROMPT 5.0

```
CONTEXT:
Phase 4 complete with 318 tests passing. Beginning Phase 5: Enemy AI.

OBJECTIVE:
Add Phase 5 standards section to CLAUDE.md.

TASK:
Append the following section to CLAUDE.md:

---

## Phase 5: Enemy AI Standards

### Critical Design Principle
**AI must use the exact same systems as the player. No cheating.**
- AI plans movement using Ship.PlanMove()
- AI fires weapons using WeaponManager
- AI activates abilities using AbilitySystem
- AI accumulates heat and suffers penalties
- AI turrets can be damaged/destroyed
- AI respects cooldowns, ammo limits, arcs

### Architecture Principles
- Strategy pattern for AI behaviors (swap difficulty levels)
- AI makes decisions during Command phase, not before
- AI can read visible game state (positions, health)
- Advanced AI can read player's planned movement (ghost position)
- AI decisions are deterministic given same state (for testing)

### Key Classes to Create
- AIController: Main AI component on enemy ships
- AIBehavior: Abstract base for difficulty strategies
- BasicAIBehavior, IntermediateAIBehavior, AdvancedAIBehavior
- AICoordinator: Multi-ship coordination (Advanced only)
- EnemySpawner: Spawn and configure enemy ships
- GameManager: Victory/defeat conditions

### Integration Points
- Ship.cs: AI controls same ship component as player
- WeaponManager: AI assigns weapon groups and targets
- AbilitySystem: AI activates abilities
- HeatManager: AI manages heat budget
- TurnManager: AI acts during Command phase
- DamageRouter: AI ships take damage same as player

### Folder Structure for Phase 5
```
Assets/Scripts/AI/                    # AI behavior scripts
Assets/Scripts/AI/Behaviors/          # Difficulty-specific behaviors
Assets/Scripts/Management/            # Game flow management
Assets/Editor/AI/                     # Editor automation
Assets/Tests/PlayModeTests/AI/        # Unit tests
Assets/Prefabs/Ships/Enemies/         # Enemy ship prefabs
```

### Enemy Ship Configuration (from GDD)

**Frigate (Light)**
- Stats: 200 HP, 100 shields, 120 heat cap, 25 speed, 60° turn
- Weapons: 2x Rail Gun, 1x Missile Battery
- PD: 4 turrets
- Abilities: Emergency Cooling, Evasive Maneuver
- Default AI: Basic or Intermediate

**Destroyer (Medium)**
- Stats: 350 HP, 150 shields, 140 heat cap, 18 speed, 45° turn
- Weapons: 1x Rail Gun, 2x Newtonian Cannon, 1x Missile Battery
- PD: 6 turrets
- Abilities: Emergency Cooling, Shield Boost, Overcharge Weapons
- Default AI: Intermediate

**Battleship (Heavy)**
- Stats: 600 HP, 250 shields, 180 heat cap, 15 speed, 30° turn
- Weapons: 3x Rail Gun, 2x Newtonian Cannon, 2x Torpedo, 2x Missile
- PD: 12 turrets
- Abilities: All 6 abilities
- Default AI: Advanced

---

Create the folder structure listed above if directories don't exist.

STATUS UPDATE:
After completion, note in IMPLEMENTATION_STATUS.md:
- ✅ Step 5.0 Complete - Phase 5 Standards Established
- 📁 Modified: CLAUDE.md
- 📁 Created: Folder structure for Phase 5
- ⏭️ Next: Step 5.1 - AI Controller Foundation
```

### VERIFICATION 5.0

- [ ] CLAUDE.md contains Phase 5 standards section
- [ ] All folder paths exist

---

## Step 5.1: AI Controller Foundation

**Time**: 4-5 hours
**Prerequisites**: Step 5.0 complete (318 tests passing)

### CLAUDE CODE PROMPT 5.1

```
CONTEXT:
Phase 5 standards established. Beginning AI controller implementation.

The AI controller is the main component that makes decisions for enemy ships during Command phase, using the same systems the player uses.

OBJECTIVE:
Create the foundational AI controller and behavior system.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 5 section.

ARCHITECTURE:

1. AIController.cs (MonoBehaviour)
   - Main AI component attached to enemy ships
   - Properties:
     * controlledShip: Ship
     * currentBehavior: AIBehavior
     * targetShip: Ship (primary target, usually player)
     * decisionInterval: float (think delay for realism)
     * hasCommittedThisTurn: bool
   - Methods:
     * void Initialize(Ship ship, AIBehavior behavior)
     * void SetTarget(Ship target)
     * void MakeDecisions() - Main decision loop
     * void CommitPlans() - Finalize and commit all plans
     * void OnCommandPhaseStart() - Reset state, begin thinking
     * void OnSimulationPhaseStart() - Ensure committed

2. AIBehavior.cs (abstract base class)
   - Strategy pattern for different AI difficulties
   - Abstract methods:
     * Vector3 DetermineMovementTarget(AIContext context)
     * WeaponPlan DetermineWeaponUsage(AIContext context)
     * List<AbilityType> DetermineAbilityUsage(AIContext context)
   - Virtual helper methods:
     * float GetIdealRange(Ship self) - Optimal engagement range
     * bool ShouldRetreat(AIContext context) - Disengage check
     * float EvaluateThreat(Ship target) - Threat assessment

3. AIContext.cs (struct/class)
   - Snapshot of decision-relevant state
   - Properties:
     * selfShip: Ship (the AI's ship)
     * targetShip: Ship (primary target)
     * distanceToTarget: float
     * selfHeat: float, selfHeatPercentage: float
     * selfShields: float, selfShieldsPercentage: float
     * targetShields: float
     * selfBreachedSections: List<SectionType>
     * targetBreachedSections: List<SectionType>
     * availableAbilities: List<AbilityData>
     * weaponsReady: List<WeaponSystem>
     * weaponsOnCooldown: List<WeaponSystem>
     * incomingThreats: int (missiles/torpedoes targeting self)

4. WeaponPlan.cs (struct)
   - AI's weapon firing plan
   - Properties:
     * weaponGroupAssignments: Dictionary<WeaponSystem, int>
     * groupTargets: Dictionary<int, Ship>
     * totalHeatCost: float
     * shouldFireAlphaStrike: bool

5. AIDecisionTimer.cs
   - Handles think delay for realism
   - AI doesn't decide instantly (feels more natural)
   - Configurable delay per difficulty

6. Modify TurnManager.cs
   - On Command phase start: Notify all AIControllers
   - Allow AI decision time
   - Before Simulation: Verify all AI committed

7. Team/Faction System
   - Add to Ship.cs: teamId property (0 = player, 1 = enemy)
   - Weapons check teamId to prevent friendly fire
   - PD only intercepts enemy projectiles

EDITOR AUTOMATION:

8. AIControllerSetupEditor.cs
   - Menu: "Hephaestus/Setup/Add AI Controller to Selected Ship"
     * Adds AIController component
     * Configures default behavior based on ship type
   - Menu: "Hephaestus/Setup/Configure Ship as Enemy"
     * Sets teamId = 1
     * Adds AIController if missing

9. AITestSceneSetup.cs
   - Menu: "Hephaestus/Testing/Create AI Controller Test Scene"
   - Creates:
     * Player ship (teamId 0)
     * Enemy ship with AIController (teamId 1)
     * AI debug display showing decision state
     * Manual target assignment controls

10. AIDebugVisualizer.cs
    - Shows AI decision state in Scene view
    - Current target (line to target)
    - Planned movement (ghost position)
    - Heat budget visualization
    - Behavior name display

11. AIControllerTestController.cs
    - OnGUI showing:
      * Current AI state
      * Target info
      * Decision breakdown
      * "Force Decision" button
      * Behavior swap dropdown
      * Step-by-step decision trace

UNIT TESTS (AIControllerTests.cs):

1. Test_AIControllerInitialization - Verify setup
2. Test_AIControllerFindsTarget - Auto-targets player
3. Test_AIContextPopulated - All context fields filled
4. Test_AIDecisionCalledOnCommandPhase - Timing correct
5. Test_AICommitsBeforeSimulation - Plans committed
6. Test_AIUsesShipSystems - Uses same Ship methods
7. Test_AITeamIdSet - Enemy has teamId 1
8. Test_AIDecisionTimer - Appropriate delay
9. Test_AIBehaviorSwappable - Can change difficulty
10. Test_AITargetSwitching - Can retarget

STATUS UPDATE:
- ✅ Step 5.1 Complete - AI Controller Foundation
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 328/328 passing
- ⏭️ Next: Step 5.2 - Basic AI Behavior
```

### VERIFICATION 5.1

1. **Run Setup**: Menu → Hephaestus → Testing → Create AI Controller Test Scene
2. **Run Tests**: Verify 328/328 passing
3. **Play Mode Verification**:
   - [ ] Enemy ship has AIController component
   - [ ] AI targets player ship automatically
   - [ ] Debug visualizer shows target line
   - [ ] AI context displays correct values
   - [ ] Team IDs correct (player=0, enemy=1)

---

## Step 5.2: Basic AI Behavior

**Time**: 3-4 hours
**Prerequisites**: Step 5.1 complete (328 tests passing)

### CLAUDE CODE PROMPT 5.2

```
CONTEXT:
Step 5.1 complete. AI controller foundation in place.

Implementing Basic AI - simple, aggressive, predictable. Good for tutorial enemies.

REFERENCE (from GDD):
- Move toward player if out of range
- Fire all weapons every turn
- Uses Emergency Cooling when heat > 100
- No other ability usage
- Predictable movement patterns

OBJECTIVE:
Create Basic AI behavior that provides easy, learnable opponents.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 5 section.

ARCHITECTURE:

1. BasicAIBehavior.cs (extends AIBehavior)
   - Simple aggressive behavior
   
   DetermineMovementTarget():
   - If distance > 20 units: Move directly toward target
   - If distance <= 20 units: Maintain current position
   - No flanking, no evasion
   - Predictable straight-line approach
   
   DetermineWeaponUsage():
   - Put ALL weapons in Group 1
   - Target player with Group 1
   - Fire every turn regardless of heat
   - No range optimization
   - No arc consideration (just fire if possible)
   
   DetermineAbilityUsage():
   - Only use Emergency Cooling
   - Trigger when heat > 100
   - No other abilities

2. Basic AI Characteristics:
   - Aggressive: Always closes distance
   - Simple: No weapon group tactics
   - Reactive: Only responds to critical heat
   - Exploitable: Player can predict and counter

3. Heat Management (Basic):
   - Fire until heat reaches 100+
   - Then use Emergency Cooling
   - Then continue firing
   - Will overheat if cooling on cooldown

4. Movement Execution:
   - Calculate direction to target
   - Use Ship.PlanMove() with max distance toward target
   - Respect ship's turn rate limits
   - No prediction of player movement

EDITOR AUTOMATION:

5. BasicAITestSetup.cs
   - Menu: "Hephaestus/Testing/Create Basic AI Test Scene"
   - Creates:
     * Player ship
     * Frigate with Basic AI
     * Distance markers
     * AI decision trace display

6. BasicAITestController.cs
   - Shows Basic AI decision making
   - Movement prediction line
   - Weapon group assignment display
   - Heat tracking
   - "Step Turn" to observe AI over time
   - Verify predictability

UNIT TESTS (BasicAIBehaviorTests.cs):

1. Test_BasicAIMovesTowardTarget - Closes distance when far
2. Test_BasicAIMaintainsRange - Stops closing at 20 units
3. Test_BasicAIFiresAllWeapons - All weapons assigned
4. Test_BasicAITargetsPlayer - Correct target
5. Test_BasicAIUsesEmergencyCooling - Triggers at 100+ heat
6. Test_BasicAINoOtherAbilities - Only emergency cooling
7. Test_BasicAICanOverheat - Will exceed safe heat
8. Test_BasicAIPredictableMovement - Same state = same decision
9. Test_BasicAIRespectsTurnRate - Doesn't exceed ship limits
10. Test_BasicAIFrigateBehavior - Works with Frigate config

STATUS UPDATE:
- ✅ Step 5.2 Complete - Basic AI Behavior
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 338/338 passing
- ⏭️ Next: Step 5.3 - Intermediate AI Behavior
```

### VERIFICATION 5.2

1. **Run Setup**: Menu → Hephaestus → Testing → Create Basic AI Test Scene
2. **Run Tests**: Verify 338/338 passing
3. **Play Mode Verification**:
   - [ ] Basic AI moves toward player when far
   - [ ] Basic AI fires all weapons every turn
   - [ ] Basic AI uses Emergency Cooling at high heat
   - [ ] Basic AI movement is predictable
   - [ ] Basic AI can overheat itself
   - [ ] Player can easily defeat Basic AI

---

## Step 5.3: Intermediate AI Behavior

**Time**: 4-5 hours
**Prerequisites**: Step 5.2 complete (338 tests passing)

### CLAUDE CODE PROMPT 5.3

```
CONTEXT:
Step 5.2 complete. Basic AI functional.

Implementing Intermediate AI - better tactics, heat management, defensive abilities.

REFERENCE (from GDD):
- Maintains optimal firing range (15-20 units)
- Basic heat management (stops at 80 heat)
- Uses Evasive Maneuver when shields low
- Uses Shield Boost when shields collapse
- Attempts to flank player
- Protects damaged sections by angling

OBJECTIVE:
Create Intermediate AI that provides a reasonable challenge.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 5 section.

ARCHITECTURE:

1. IntermediateAIBehavior.cs (extends AIBehavior)
   
   DetermineMovementTarget():
   - Calculate optimal range based on weapon loadout
   - If too far: Close to optimal range
   - If too close: Back off to optimal range
   - If shields < 50%: Retreat (increase distance)
   - If section damaged: Angle to protect that section
   - Basic flanking: Approach from side if possible
   
   DetermineWeaponUsage():
   - Group weapons by effective range:
     * Long range (Rail Guns): Fire at 20-30 units
     * Medium range (Cannons): Fire at 15-20 units
     * Short range: Fire at < 15 units
   - Check heat budget before assigning
   - Don't fire if would exceed 80 heat
   - Prioritize weapons in arc
   
   DetermineAbilityUsage():
   - Emergency Cooling: When heat > 80 (before critical)
   - Shield Boost: When shields = 0 and taking damage
   - Evasive Maneuver: When shields < 30%
   - Conservative ability usage

2. Optimal Range Calculation:
   - Based on weapon mix
   - Weighted by damage potential
   - Example: Heavy cannon ship prefers 15-20 range

3. Section Protection Logic:
   - Track which sections are damaged
   - Calculate angle to present undamaged section to enemy
   - Adjust movement to rotate ship

4. Heat Budget Management:
   - Calculate total heat if all planned weapons fire
   - If would exceed 80, reduce firing
   - Prioritize high-damage weapons

5. Retreat Logic:
   - Trigger conditions: shields < 50% OR multiple sections damaged
   - Movement away from target
   - Still fires weapons while retreating

EDITOR AUTOMATION:

6. IntermediateAITestSetup.cs
   - Menu: "Hephaestus/Testing/Create Intermediate AI Test Scene"
   - Creates:
     * Player ship
     * Destroyer with Intermediate AI
     * Range indicators (optimal zone)
     * AI decision breakdown display

7. IntermediateAITestController.cs
   - Optimal range visualization
   - Heat budget display
   - Weapon group assignments
   - Retreat trigger indicators
   - Section damage simulation
   - "Damage Shields" button to test defensive behavior

UNIT TESTS (IntermediateAIBehaviorTests.cs):

1. Test_IntermediateAIMaintainsOptimalRange
2. Test_IntermediateAIRetreatsWhenShieldsLow
3. Test_IntermediateAIStopsAt80Heat
4. Test_IntermediateAIUsesShieldBoost
5. Test_IntermediateAIUsesEvasiveManeuver
6. Test_IntermediateAIGroupsWeaponsByRange
7. Test_IntermediateAIProtectsDamagedSection
8. Test_IntermediateAICalculatesHeatBudget
9. Test_IntermediateAIFlankingAttempt
10. Test_IntermediateAIDestroyerBehavior

STATUS UPDATE:
- ✅ Step 5.3 Complete - Intermediate AI Behavior
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 348/348 passing
- ⏭️ Next: Step 5.4 - Advanced AI Behavior
```

### VERIFICATION 5.3

1. **Run Setup**: Menu → Hephaestus → Testing → Create Intermediate AI Test Scene
2. **Run Tests**: Verify 348/348 passing
3. **Play Mode Verification**:
   - [ ] Intermediate AI maintains optimal range
   - [ ] Intermediate AI retreats when shields low
   - [ ] Intermediate AI stops firing before overheating
   - [ ] Intermediate AI uses Shield Boost when shields depleted
   - [ ] Intermediate AI uses Evasive Maneuver defensively
   - [ ] More challenging than Basic AI

---

## Step 5.4: Advanced AI Behavior

**Time**: 5-6 hours
**Prerequisites**: Step 5.3 complete (348 tests passing)

### CLAUDE CODE PROMPT 5.4

```
CONTEXT:
Step 5.3 complete. Intermediate AI functional.

Implementing Advanced AI - sophisticated tactics, prediction, full ability usage.

REFERENCE (from GDD):
- Predicts player movement (reads planned position)
- Positions to flank or exploit PD blind spots
- Coordinates weapon groups for alpha strikes
- Multi-turn planning (setup → execute)
- Full tactical ability usage

OBJECTIVE:
Create Advanced AI that provides significant challenge.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 5 section.

ARCHITECTURE:

1. AdvancedAIBehavior.cs (extends AIBehavior)
   
   DetermineMovementTarget():
   - Read player's PlannedPosition (the ghost/projection)
   - Predict where player will be at turn end
   - Position to:
     * Flank (attack from side/rear)
     * Exploit PD blind spots (damaged turret arcs)
     * Stay in weapon arcs while dodging player arcs
   - If outnumbered: Kiting (stay at max range)
   - If advantage: Aggressive close-in
   
   DetermineWeaponUsage():
   - Lead targeting: Calculate intercept point for projectiles
   - Alpha strike timing:
     * If player shields low → All weapons
     * If player shields high → Sustained fire
   - Hold torpedoes for critical moments
   - Target damaged sections preferentially
   
   DetermineAbilityUsage():
   - Emergency Cooling: Before planned alpha strike
   - Overcharge Weapons: When player shields depleted
   - Sensor Burst: To counter player Evasive Maneuvers
   - PD Override: When detecting incoming missile salvo
   - Shield Boost: Tactically timed
   - Multi-turn planning: "Cool now, alpha next turn"

2. Player Position Prediction:
   - Access player ship's PlannedPosition
   - Calculate where player will be
   - Adjust targeting and positioning

3. Alpha Strike Logic:
   - Conditions for alpha strike:
     * Player shields down
     * Own heat low enough to sustain burst
     * High damage potential
   - Pre-alpha: Use Emergency Cooling
   - Alpha turn: Fire everything + Overcharge

4. Target Section Selection:
   - Identify player's damaged/breached sections
   - Position to attack those sections
   - Coordinate weapon arcs

5. Multi-Turn Planning:
   - AIPlanner.cs helper class
   - Plan 2-3 turns ahead
   - Track: "Turn N: Cool down, Turn N+1: Alpha strike"
   - Adjust plan based on changing state

6. PD Exploitation:
   - Detect which player PD turrets destroyed
   - Calculate coverage gaps
   - Attack from gap direction

EDITOR AUTOMATION:

7. AdvancedAITestSetup.cs
   - Menu: "Hephaestus/Testing/Create Advanced AI Test Scene"
   - Creates:
     * Player ship
     * Battleship with Advanced AI
     * Player movement ghost visible
     * AI planning visualization

8. AdvancedAITestController.cs
   - Multi-turn plan display
   - Prediction visualization
   - Alpha strike conditions
   - PD gap exploitation display
   - Player position prediction line
   - "Simulate Turns" to watch AI strategy unfold

9. AIPlanner Visualizer
   - Shows planned actions for next 2-3 turns
   - Color-coded by action type
   - Updates as state changes

UNIT TESTS (AdvancedAIBehaviorTests.cs):

1. Test_AdvancedAIReadsPlayerPlannedPosition
2. Test_AdvancedAIFlanksTarget
3. Test_AdvancedAIExploitsPDGaps
4. Test_AdvancedAIAlphaStrikeConditions
5. Test_AdvancedAIUsesOverchargeCorrectly
6. Test_AdvancedAIUsesEmergencyCoolingBeforeAlpha
7. Test_AdvancedAITargetsDamagedSections
8. Test_AdvancedAIMultiTurnPlanning
9. Test_AdvancedAIKitesWhenOutnumbered
10. Test_AdvancedAIBattleshipBehavior
11. Test_AdvancedAIUsesSensorBurst
12. Test_AdvancedAIUsesPDOverride

STATUS UPDATE:
- ✅ Step 5.4 Complete - Advanced AI Behavior
- List new/modified files
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 360/360 passing
- ⏭️ Next: Step 5.5 - Enemy Ship Spawning
```

### VERIFICATION 5.4

1. **Run Setup**: Menu → Hephaestus → Testing → Create Advanced AI Test Scene
2. **Run Tests**: Verify 360/360 passing
3. **Play Mode Verification**:
   - [ ] Advanced AI reads player's planned movement
   - [ ] Advanced AI attempts flanking maneuvers
   - [ ] Advanced AI exploits PD coverage gaps
   - [ ] Advanced AI uses Emergency Cooling before alpha strikes
   - [ ] Advanced AI uses Overcharge when player shields down
   - [ ] Significantly more challenging than Intermediate

---

## Step 5.5: Enemy Ship Spawning

**Time**: 3-4 hours
**Prerequisites**: Step 5.4 complete (360 tests passing)

### CLAUDE CODE PROMPT 5.5

```
CONTEXT:
Step 5.4 complete. All three AI difficulty levels functional.

Now implementing enemy ship spawning system and prefab configuration.

OBJECTIVE:
Create spawning system and properly configured enemy ship prefabs.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 5 section.

ARCHITECTURE:

1. EnemySpawner.cs (MonoBehaviour)
   - Spawns enemy ships at designated points
   - Properties:
     * spawnPoints: List<SpawnPoint>
     * enemyPrefabs: Dictionary<EnemyType, GameObject>
     * spawnedEnemies: List<Ship>
   - Methods:
     * Ship SpawnEnemy(EnemyType type, int spawnPointIndex)
     * List<Ship> SpawnWave(WaveDefinition wave)
     * void DespawnAll()
     * List<Ship> GetActiveEnemies()

2. SpawnPoint.cs (MonoBehaviour)
   - Marks spawn locations
   - Properties:
     * spawnPosition: Vector3
     * spawnRotation: Quaternion
     * facesPlayer: bool (auto-orient to player)
   - Gizmo visualization in editor

3. EnemyType.cs (enum)
   - Frigate, Destroyer, Battleship

4. WaveDefinition.cs (ScriptableObject)
   - Defines enemy wave composition
   - Properties:
     * enemies: List<EnemySpawnInfo>
     * delayBetweenSpawns: float
   - Preset waves: Tutorial, Easy, Medium, Hard, Boss

5. EnemySpawnInfo.cs (struct)
   - type: EnemyType
   - spawnPointIndex: int
   - behaviorOverride: AIBehavior (optional)

6. Enemy Prefab Configuration:
   Create prefabs with all required components:
   
   **Frigate Prefab:**
   - Ship (200 HP, 100 shields, 120 heat, 25 speed, 60° turn)
   - HeatManager, SectionManager, ShieldSystem, DamageRouter
   - WeaponManager with: 2x Rail Gun, 1x Missile Battery
   - PDSystem with 4 turrets
   - AbilitySystem with: Emergency Cooling, Evasive Maneuver
   - AIController (default: BasicAIBehavior)
   - TeamId = 1
   
   **Destroyer Prefab:**
   - Ship (350 HP, 150 shields, 140 heat, 18 speed, 45° turn)
   - All managers
   - WeaponManager with: 1x Rail Gun, 2x Newtonian Cannon, 1x Missile Battery
   - PDSystem with 6 turrets
   - AbilitySystem with: Emergency Cooling, Shield Boost, Overcharge
   - AIController (default: IntermediateAIBehavior)
   - TeamId = 1
   
   **Battleship Prefab:**
   - Ship (600 HP, 250 shields, 180 heat, 15 speed, 30° turn)
   - All managers
   - Full weapon loadout (9 weapons)
   - PDSystem with 12 turrets
   - AbilitySystem with all 6 abilities
   - AIController (default: AdvancedAIBehavior)
   - TeamId = 1

EDITOR AUTOMATION:

7. EnemyPrefabSetupEditor.cs
   - Menu: "Hephaestus/Setup/Create Enemy Prefabs"
     * Creates Frigate, Destroyer, Battleship prefabs
     * Configures all components from GDD specs
     * Saves to Assets/Prefabs/Ships/Enemies/
   - Menu: "Hephaestus/Setup/Update Enemy Prefab Stats"
     * Updates existing prefabs with GDD values

8. SpawnPointSetupEditor.cs
   - Menu: "Hephaestus/Setup/Create Spawn Points"
     * Creates spawn point ring around origin
     * Configurable count and radius

9. EnemySpawnTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Enemy Spawn Test Scene"
   - Creates:
     * Player ship at origin
     * 8 spawn points in circle
     * EnemySpawner configured
     * Spawn control UI

10. EnemySpawnTestController.cs
    - Buttons: "Spawn Frigate", "Spawn Destroyer", "Spawn Battleship"
    - Spawn point selector
    - Wave preset buttons
    - "Clear All" button
    - Enemy list with stats

UNIT TESTS (EnemySpawnTests.cs):

1. Test_SpawnFrigate - Correct stats
2. Test_SpawnDestroyer - Correct stats
3. Test_SpawnBattleship - Correct stats
4. Test_FrigateWeaponLoadout - Correct weapons
5. Test_DestroyerWeaponLoadout - Correct weapons
6. Test_BattleshipWeaponLoadout - Correct weapons
7. Test_EnemyTeamId - All have teamId = 1
8. Test_EnemyHasAIController - AI attached
9. Test_SpawnAtPoint - Correct position
10. Test_SpawnWave - Multiple enemies spawn
11. Test_DespawnAll - Cleans up properly
12. Test_GetActiveEnemies - Tracks spawned ships

STATUS UPDATE:
- ✅ Step 5.5 Complete - Enemy Ship Spawning
- List new/modified files
- 📁 Created: 3 enemy prefabs
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 372/372 passing
- ⏭️ Next: Step 5.6 - Victory/Defeat Conditions
```

### VERIFICATION 5.5

1. **Run Setup**: Menu → Hephaestus → Setup → Create Enemy Prefabs
2. **Run Setup**: Menu → Hephaestus → Testing → Create Enemy Spawn Test Scene
3. **Run Tests**: Verify 372/372 passing
4. **Play Mode Verification**:
   - [ ] Frigate spawns with correct stats
   - [ ] Destroyer spawns with correct stats
   - [ ] Battleship spawns with correct stats
   - [ ] All enemies have teamId = 1
   - [ ] All enemies have appropriate AI behavior
   - [ ] Wave spawning works

---

## Step 5.6: Victory/Defeat Conditions

**Time**: 3-4 hours
**Prerequisites**: Step 5.5 complete (372 tests passing)

### CLAUDE CODE PROMPT 5.6

```
CONTEXT:
Step 5.5 complete. Enemy spawning functional.

Implementing game flow management - tracking victory/defeat conditions.

REFERENCE (from GDD):
- Victory: All enemy ships destroyed
- Defeat: Player ship destroyed (hull = 0) OR Core Breach

OBJECTIVE:
Create game manager that tracks combat state and win/loss conditions.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 5 section.

ARCHITECTURE:

1. GameManager.cs (singleton MonoBehaviour)
   - Manages overall game state
   - Properties:
     * playerShip: Ship
     * enemySpawner: EnemySpawner
     * gameState: GameState enum
     * turnCount: int
     * combatStats: CombatStatistics
   - GameState: Setup, Playing, Victory, Defeat, Paused
   - Methods:
     * void StartCombat()
     * void CheckVictoryConditions()
     * void CheckDefeatConditions()
     * void OnVictory()
     * void OnDefeat()
     * void RestartCombat()
     * void PauseCombat()

2. CombatStatistics.cs
   - Tracks combat performance
   - Properties:
     * turnsPlayed: int
     * totalDamageDealt: float
     * totalDamageTaken: float
     * enemiesDestroyed: int
     * abilitiesUsed: int
     * weaponsFired: int
   - Methods:
     * void RecordDamageDealt(float amount)
     * void RecordDamageTaken(float amount)
     * void RecordEnemyDestroyed()
     * string GenerateSummary()

3. Victory/Defeat Event Flow:
   - Subscribe to Ship destruction events
   - On any ship destroyed:
     * If player ship: Trigger defeat
     * If enemy ship: Check if all enemies destroyed → Victory
   - Events: OnVictory, OnDefeat

4. VictoryScreen.cs (UI)
   - Displayed on victory
   - Shows: "VICTORY", combat statistics, buttons (Restart, Continue)
   - Animated entrance

5. DefeatScreen.cs (UI)
   - Displayed on defeat
   - Shows: "DEFEAT", combat statistics, cause of death, Retry button
   - Dramatic presentation

6. Ship Destruction Events:
   - Modify Ship.cs or ShipDeathController:
     * OnShipDestroyed event (fires for any ship)
     * Include ship reference and cause of death
   - GameManager subscribes to all ship destruction events

7. Turn Counting:
   - GameManager tracks turns
   - TurnManager notifies on turn complete
   - Statistics updated

EDITOR AUTOMATION:

8. GameManagerSetupEditor.cs
   - Menu: "Hephaestus/Setup/Create Game Manager"
     * Creates GameManager singleton in scene
     * Configures references

9. VictoryDefeatTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Victory Defeat Test Scene"
   - Creates:
     * GameManager
     * Player ship
     * Enemy ships
     * Victory/Defeat UI
     * Test controls

10. VictoryDefeatTestController.cs
    - "Kill All Enemies" button → Test victory
    - "Kill Player" button → Test defeat
    - "Damage Player to 1 HP" → Near-death state
    - Statistics display
    - State indicator

UNIT TESTS (VictoryDefeatTests.cs):

1. Test_VictoryOnAllEnemiesDestroyed
2. Test_DefeatOnPlayerDestroyed
3. Test_VictoryScreenAppears
4. Test_DefeatScreenAppears
5. Test_StatisticsTrackDamageDealt
6. Test_StatisticsTrackDamageTaken
7. Test_StatisticsTrackTurns
8. Test_RestartResetsCombat
9. Test_GameStateTransitions
10. Test_PartialEnemyDestructionNoVictory

STATUS UPDATE:
- ✅ Step 5.6 Complete - Victory/Defeat Conditions
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 382/382 passing
- ⏭️ Next: Step 5.7 - AI Coordination (Multi-Ship)
```

### VERIFICATION 5.6

1. **Run Setup**: Menu → Hephaestus → Testing → Create Victory Defeat Test Scene
2. **Run Tests**: Verify 382/382 passing
3. **Play Mode Verification**:
   - [ ] Destroying all enemies triggers victory
   - [ ] Victory screen appears with stats
   - [ ] Destroying player triggers defeat
   - [ ] Defeat screen shows cause of death
   - [ ] Statistics track correctly
   - [ ] Restart button works

---

## Step 5.7: AI Coordination (Multi-Ship)

**Time**: 3-4 hours
**Prerequisites**: Step 5.6 complete (382 tests passing)

### CLAUDE CODE PROMPT 5.7

```
CONTEXT:
Step 5.6 complete. Victory/defeat conditions working.

Implementing AI coordination for multi-ship combat (Advanced AI only).

REFERENCE (from GDD):
- Focus fire: All enemies target player's weakest ship/section
- Flanking: AIs approach from different angles
- Suppression: One AI draws fire while others flank

OBJECTIVE:
Create AI coordinator for multi-ship tactical coordination.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 5 section.

ARCHITECTURE:

1. AICoordinator.cs (singleton)
   - Coordinates multiple AI ships
   - Properties:
     * registeredAIs: List<AIController>
     * coordinationMode: CoordinationMode enum
     * focusTarget: Ship (shared target)
     * focusSection: SectionType (shared target section)
   - CoordinationMode: Independent, FocusFire, Flanking, Combined
   - Methods:
     * void RegisterAI(AIController ai)
     * void UnregisterAI(AIController ai)
     * void CoordinateAttack()
     * void AssignFocusFire(Ship target)
     * void AssignFlankingPositions()
     * void SelectFocusSection()

2. Focus Fire Logic:
   - Identify player's most damaged section
   - All AIs target that section
   - Coordinate timing for alpha strike

3. Flanking Assignment:
   - Calculate angles around target
   - Assign each AI to different approach angle
   - Prevent AI ships from clustering

4. Suppression Tactics:
   - One AI designated as "aggressor" (draws attention)
   - Others designated as "flankers"
   - Aggressor: Direct approach, fires constantly
   - Flankers: Circle around, wait for opening

5. Integration with AdvancedAIBehavior:
   - Advanced AI queries AICoordinator for assignments
   - Coordinator suggestions override individual decisions
   - Basic/Intermediate AI ignore coordinator

6. Coordination Timing:
   - Coordinator runs at start of Command phase
   - Evaluates battlefield state
   - Issues assignments to registered AIs
   - AIs incorporate assignments into decisions

EDITOR AUTOMATION:

7. AICoordinationTestSetup.cs
   - Menu: "Hephaestus/Testing/Create AI Coordination Test Scene"
   - Creates:
     * Player ship
     * 3 enemy ships (mix of types)
     * AICoordinator
     * Coordination visualization

8. AICoordinationTestController.cs
   - Coordination mode selector
   - Focus target/section display
   - Flanking position visualization
   - Per-AI assignment display
   - "Force Coordination" button
   - Angle/position diagram

9. CoordinationVisualizer.cs
   - Shows coordination state in scene
   - Lines showing assigned targets
   - Flanking angle indicators
   - Focus section highlight on player

UNIT TESTS (AICoordinationTests.cs):

1. Test_AIRegistration - AIs register with coordinator
2. Test_FocusFireAllTargetSame - All AIs same target
3. Test_FocusSectionSelection - Most damaged selected
4. Test_FlankingAngleAssignment - Different angles assigned
5. Test_FlankingNoOverlap - AIs don't cluster
6. Test_SuppressionRoleAssignment - Aggressor vs flanker
7. Test_CoordinationTiming - Runs at command phase start
8. Test_BasicAIIgnoresCoordinator - Only Advanced uses it
9. Test_CoordinatorUpdatesOnStateChange - Reacts to damage
10. Test_SingleAINoCoordination - Works with one AI

STATUS UPDATE:
- ✅ Step 5.7 Complete - AI Coordination
- List new/modified files
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 392/392 passing
- ⏭️ Next: Step 5.8 - Phase 5 Integration Testing
```

### VERIFICATION 5.7

1. **Run Setup**: Menu → Hephaestus → Testing → Create AI Coordination Test Scene
2. **Run Tests**: Verify 392/392 passing
3. **Play Mode Verification**:
   - [ ] Multiple AIs register with coordinator
   - [ ] Focus fire mode: All target same section
   - [ ] Flanking mode: AIs approach from different angles
   - [ ] Visualization shows assigned positions
   - [ ] Basic AI ignores coordination
   - [ ] Advanced AI follows coordination

---

## Step 5.8: Phase 5 Integration Testing

**Time**: 3-4 hours
**Prerequisites**: Step 5.7 complete (392 tests passing)

### CLAUDE CODE PROMPT 5.8

```
CONTEXT:
All Phase 5 systems implemented. Creating comprehensive integration tests.

OBJECTIVE:
Create integration tests verifying complete AI combat and full game scenarios.

REQUIREMENTS:
Follow all standards defined in CLAUDE.md Phase 5 section.

ARCHITECTURE:

1. Phase5IntegrationTests.cs
   - End-to-end AI combat tests

2. FullCombatTestSetup.cs
   - Menu: "Hephaestus/Testing/Create Full Combat Test Scene"
   - Complete game scenario:
     * Player ship (Hephaestus) with full loadout
     * Enemy spawner with wave definitions
     * GameManager with victory/defeat
     * All UI panels
     * Turn system active

3. FullCombatTestController.cs
   - Scenario presets:
     * "Tutorial" - 1 Frigate (Basic AI)
     * "Easy" - 2 Frigates
     * "Medium" - 1 Frigate + 1 Destroyer
     * "Hard" - 2 Destroyers
     * "Boss" - 1 Battleship
     * "Epic" - 1 Battleship + 2 Frigates
   - Auto-play option (AI vs AI for testing)
   - Turn-by-turn stepping
   - Combat statistics live display

INTEGRATION TESTS:

1. Test_FullCombat_BasicAI
   - Player vs Frigate (Basic)
   - Verify: AI makes decisions, combat resolves, winner determined

2. Test_FullCombat_IntermediateAI
   - Player vs Destroyer (Intermediate)
   - Verify: AI uses defensive abilities, maintains range

3. Test_FullCombat_AdvancedAI
   - Player vs Battleship (Advanced)
   - Verify: AI predicts movement, uses alpha strikes

4. Test_MultiEnemyCombat
   - Player vs 2 Frigates
   - Verify: Both AIs act, coordination works

5. Test_AIUsesPlayerSystems
   - Verify: AI heat accumulates, AI can overheat

6. Test_AIRespectsAmmoLimits
   - Verify: AI runs out of torpedoes/missiles

7. Test_AIDamageAffectsPerformance
   - Damage AI weapons
   - Verify: AI combat effectiveness reduced

8. Test_VictoryAfterAllEnemiesDestroyed
   - Full combat to victory
   - Verify: Victory triggers, stats accurate

9. Test_DefeatWhenPlayerDestroyed
   - Set player low HP, let AI win
   - Verify: Defeat triggers

10. Test_TurnCountAccumulates
    - Multi-turn combat
    - Verify: Turn count matches actual turns

11. Test_CombatStatisticsAccurate
    - Full combat
    - Verify: All statistics correct

12. Test_RestartResetsEverything
    - Combat, then restart
    - Verify: Full reset

STATUS UPDATE:
- ✅ Step 5.8 Complete - Phase 5 Integration Testing
- ✅ PHASE 5 COMPLETE
- List new/modified files
- 🧪 Integration Tests: 12/12 passing
- 🧪 Total Tests: 404/404 passing
- 📊 Phase 5 Summary: [List all major systems]
- ⏭️ Next Phase: Phase 6 - Polish & VFX
```

### VERIFICATION 5.8

1. **Run Setup**: Menu → Hephaestus → Testing → Create Full Combat Test Scene
2. **Run All Tests**: Verify 404/404 passing
3. **Manual Combat Scenarios**:

   **Tutorial (1 Frigate)**:
   - [ ] Basic AI moves toward player
   - [ ] Basic AI fires all weapons
   - [ ] Player can easily defeat
   - [ ] Victory screen appears
   
   **Medium (Frigate + Destroyer)**:
   - [ ] Both enemies engage
   - [ ] Destroyer uses better tactics
   - [ ] Challenging but winnable
   
   **Boss (Battleship)**:
   - [ ] Advanced AI uses prediction
   - [ ] Alpha strikes timed well
   - [ ] Very challenging fight
   
   **Epic (Battleship + 2 Frigates)**:
   - [ ] AI coordination visible
   - [ ] Focus fire on player
   - [ ] Overwhelming without good tactics

---

## Phase 5 Summary

### Systems Implemented
| System | Description |
|--------|-------------|
| AI Controller | Main decision component |
| Basic AI | Simple aggressive behavior |
| Intermediate AI | Range/heat management |
| Advanced AI | Prediction, alpha strikes |
| Enemy Spawning | Prefabs and wave system |
| Victory/Defeat | Game flow management |
| AI Coordination | Multi-ship tactics |

### Test Coverage
| Step | Tests Added | Running Total |
|------|-------------|---------------|
| 5.1 Controller | 10 | 328 |
| 5.2 Basic AI | 10 | 338 |
| 5.3 Intermediate | 10 | 348 |
| 5.4 Advanced | 12 | 360 |
| 5.5 Spawning | 12 | 372 |
| 5.6 Victory/Defeat | 10 | 382 |
| 5.7 Coordination | 10 | 392 |
| 5.8 Integration | 12 | 404 |

### Enemy Ship Summary
| Type | HP | Shields | Default AI |
|------|-----|---------|------------|
| Frigate | 200 | 100 | Basic |
| Destroyer | 350 | 150 | Intermediate |
| Battleship | 600 | 250 | Advanced |

### Next Phase
Phase 6: Polish & VFX
- Visual effects for weapons, damage, explosions
- Audio implementation
- Heat visualization on ships
- Screen shake and juice

---

## Appendix: Quick Reference

### Menu Items Created
```
Hephaestus/Setup/Add AI Controller to Selected Ship
Hephaestus/Setup/Configure Ship as Enemy
Hephaestus/Setup/Create Enemy Prefabs
Hephaestus/Setup/Update Enemy Prefab Stats
Hephaestus/Setup/Create Spawn Points
Hephaestus/Setup/Create Game Manager
Hephaestus/Testing/Create AI Controller Test Scene
Hephaestus/Testing/Create Basic AI Test Scene
Hephaestus/Testing/Create Intermediate AI Test Scene
Hephaestus/Testing/Create Advanced AI Test Scene
Hephaestus/Testing/Create Enemy Spawn Test Scene
Hephaestus/Testing/Create Victory Defeat Test Scene
Hephaestus/Testing/Create AI Coordination Test Scene
Hephaestus/Testing/Create Full Combat Test Scene
```

### Key Classes
```
AIController (MonoBehaviour)
AIBehavior (abstract)
BasicAIBehavior
IntermediateAIBehavior
AdvancedAIBehavior
AIContext (struct)
AICoordinator (singleton)
EnemySpawner (MonoBehaviour)
GameManager (singleton)
CombatStatistics
VictoryScreen (UI)
DefeatScreen (UI)
```