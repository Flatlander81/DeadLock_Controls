# Hephaestus Implementation Guide - Part 4
## Enemy AI, Polish & MVP Completion (Phases 4-6)

---

## Document Overview

This is Part 4 of 4 in the complete Hephaestus implementation guide:
- Part 1: Overview + Phase 0 (Foundation) + Phase 1 (Core Combat)
- Part 2: Phase 2 (Weapon Systems - Parallel Tracks)
- Part 3: Phase 2 Integration + Phase 3 (Point Defense)
- **Part 4**: Phase 4 (Enemy AI) + Phase 5 (Polish) + Phase 6 (MVP) ← You are here

---

## Phase 4: Enemy AI (Weeks 8-10)

### Overview
Phase 4 implements AI that controls enemy ships using the exact same systems as the player. No cheating - AI uses the same heat management, weapon systems, abilities, and constraints.

**Parallel Development**: 3 agents can work simultaneously
- 🔵 Track A: AI Core Logic
- 🔴 Track B: Enemy Spawning & Management
- 🟢 Track C: Victory/Defeat System

**Timeline**: 2-3 weeks with parallel development

---

### Pre-Phase Manual Setup

#### MANUAL TASK 4.0: Create AI Structure
**Time**: 10 minutes

**Steps**:

1. Create folders:
```
Assets/Scripts/AI/
Assets/Scripts/Management/
Assets/Prefabs/Ships/Enemies/
```

2. Create enemy ship prefabs:
   - Duplicate player ship prefab
   - Create variants: Frigate, Destroyer, Battleship
   - Adjust stats per ship type (reference GDD)
   - Different weapon loadouts per type

3. Update IMPLEMENTATION_STATUS.md for Phase 4

**Status Update**: Document AI structure created

---

### Step 4.1A: AI Core Logic 🔵
**Parallel Track A**

**Prerequisites**: Phases 1-3 complete  
**Time Estimate**: 8-10 hours  
**Can run parallel with**: Steps 4.1B and 4.1C

---

#### CLAUDE CODE PROMPT 4.1A

```
CONTEXT:
Phases 1-3 complete. Player has full combat capabilities. Now implementing AI that controls enemy ships using the same systems.

CRITICAL DESIGN PRINCIPLE: AI must use the exact same systems as the player. No cheating, same constraints.

Existing systems AI will use:
- Ship.cs (movement, heat, health)
- WeaponManager.cs (weapon groups, firing)
- AbilitySystem.cs (abilities with heat costs and cooldowns)
- PointDefenseSystem.cs (automatic, same for player and AI)

OBJECTIVE:
Create AI controller that makes tactical decisions during Command Phase and executes them using the same player systems.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/AI/AIController.cs
   - Component attached to enemy Ship GameObjects
   - Replaces player input during Command Phase
   - Properties:
     * controlledShip: Ship (the ship this AI controls)
     * currentBehavior: AIBehavior (strategy pattern)
     * targetShip: Ship (current target, usually player)
     * decisionTimer: float (think every N seconds to avoid constant replanning)
   
   - Methods:
     * void Start() - Initialize, set up references
     * void Update() - During Command phase, make decisions
     * void MakeDecisions() - Main decision-making loop
     * void PlanMovement() - Decide where to move
     * void PlanWeaponFire() - Decide which weapons to use
     * void PlanAbilityUsage() - Decide which abilities to activate
     * void ExecutePlans() - Commit all planned actions

2. CREATE: Assets/Scripts/AI/AIBehavior.cs (abstract base)
   - Strategy pattern for different AI difficulty levels
   - Abstract methods:
     * abstract Vector3 DetermineMovementTarget(Ship self, Ship target)
     * abstract void AssignWeaponTargets(Ship self, Ship target)
     * abstract void UseAbilities(Ship self)
   
   - Helper methods:
     * float GetIdealRange(Ship self) - Calculate optimal firing range
     * bool ShouldRetreat(Ship self) - Check if ship should disengage
     * float EvaluateThreatLevel(Ship self, Ship target) - Assess danger

3. CREATE: Assets/Scripts/AI/BasicAIBehavior.cs
   - Inherits AIBehavior
   - Simple aggressive AI (implement first)
   
   - DetermineMovementTarget():
     * If distance > 20 units: Move toward player
     * If distance < 20 units: Maintain current range
     * No fancy tactics, just close to firing range
   
   - AssignWeaponTargets():
     * Assign ALL weapons to Group 1
     * Target player with Group 1
     * Fire every turn if in arc and range
   
   - UseAbilities():
     * If heat > 100: Use Emergency Cooling (if available)
     * No other ability usage
   
   - Heat Management:
     * Fire all weapons every turn until heat reaches 100
     * Then use Emergency Cooling
     * Then fire again
     * Simple but can overheat itself

4. CREATE: Assets/Scripts/AI/IntermediateAIBehavior.cs
   - Inherits AIBehavior
   - Better tactics, heat management
   
   - DetermineMovementTarget():
     * Maintain optimal range (15-20 units for most weapons)
     * If shields < 50%: Retreat (move away from player)
     * If player flanking: Turn to face player
     * Circle strafing at optimal range
   
   - AssignWeaponTargets():
     * Assign weapons to groups intelligently:
       - Long-range weapons (torpedoes): Fire at max range
       - Medium weapons (cannons): Fire at 15-20 units
       - Short-range weapons (rail guns): Fire at < 20 units
     * Don't fire weapons out of optimal range
   
   - UseAbilities():
     * Emergency Cooling: Use when heat > 80 (before critical)
     * Shield Boost: Use when shields < 50% and taking damage
     * Evasive Maneuver: Use when shields < 30%
     * Conservative heat management: Stop firing at 80 heat

5. CREATE: Assets/Scripts/AI/AdvancedAIBehavior.cs
   - Inherits AIBehavior
   - Sophisticated tactics
   
   - DetermineMovementTarget():
     * Read player's PlannedPosition (predict movement)
     * Position to flank or exploit PD blind spots
     * Maintain optimal range for weapon mix
     * If outnumbered: Kiting tactics (stay at max range)
     * If advantage: Aggressive close-in
   
   - AssignWeaponTargets():
     * Lead targeting: Predict player position at projectile impact time
     * Weapon group coordination:
       - Alpha strike when player shields low
       - Sustained fire when player shields high
       - Hold torpedoes for critical moments
   
   - UseAbilities():
     * Emergency Cooling: Use before planned alpha strike
     * Overcharge Weapons: Use when player shields depleted
     * Sensor Burst: Use to counter player Evasive Maneuvers
     * PD Override: Use when detecting incoming missile salvo
     * Multi-turn planning: "If I use Emergency Cooling now, I can alpha strike next turn"

6. Multi-Ship AI Coordination (Advanced AI only):
   - CREATE: Assets/Scripts/AI/AICoordinator.cs (singleton)
   - Coordinates multiple AI ships
   - Methods:
     * void RegisterAI(AIController ai)
     * void CoordinateAttack(Ship target)
     * void AssignFocusFire(Ship target) - All AIs target same ship
     * void SpreadFire() - AIs target different player ships (if multiple)
   
   - Tactics:
     * Focus fire: All enemies target player's weakest ship
     * Flanking: AIs approach from different angles
     * Suppression: One AI draws fire while others flank

7. MODIFY: Assets/Scripts/Movement/TurnManager.cs
   - During Command Phase:
     * Call AIController.MakeDecisions() on all AI ships
     * Allow think time (1-2 second delay for realism)
   
   - AI makes decisions DURING Command phase, not before
   - Player and AI plan simultaneously (both can see each other's movement ghosts)

8. Decision-Making Flow:
   ```
   Each Command Phase (for each AI ship):
   1. Evaluate current state:
      - My heat, hull, shields
      - Target's heat, hull, shields
      - Distance to target
      - Weapon readiness
      - Ability cooldowns
   
   2. Make movement decision:
      - Call behavior.DetermineMovementTarget()
      - Plan movement using Ship.PlanMove()
   
   3. Make weapon decisions:
      - Call behavior.AssignWeaponTargets()
      - Assign weapons to groups
      - Set targets using WeaponManager
   
   4. Make ability decisions:
      - Call behavior.UseAbilities()
      - Activate abilities using AbilitySystem
   
   5. Commit plans:
      - All actions queued
      - Ready for Simulation phase
   ```

TESTING REQUIREMENTS:

Unit Tests (Assets/Tests/PlayModeTests/AITests.cs):
1. Test_AIInitialization - Create AI ship, verify components connected
2. Test_AIMovementPlanning - AI plans movement, verify Ship.PlannedPosition set
3. Test_AIWeaponAssignment - AI assigns weapons, verify WeaponManager updated
4. Test_AIFiring - AI fires weapons, verify projectiles spawn
5. Test_AIHeatManagement - AI heat > 100, verify Emergency Cooling used
6. Test_AIAbilityUsage - AI shields low, verify Shield Boost used
7. Test_BasicAIBehavior - Basic AI maintains 15-20 unit range
8. Test_IntermediateAIRetreat - Intermediate AI shields < 50%, verify retreats
9. Test_AdvancedAIPrediction - Advanced AI reads player PlannedPosition
10. Test_AIUsesPlayerSystems - Verify AI uses same Ship/Weapon/Ability systems as player
11. Test_AICanOverheat - AI fires too much, verify overheats (no cheating)
12. Test_AIRespectsCooldowns - AI ability on cooldown, verify can't use

Manual Testing:

Test Scenarios:

SCENARIO A: Basic AI Combat
1. Scene: Player ship vs 1 enemy Frigate (Basic AI)
2. Observe AI:
   - [ ] AI moves toward player if distance > 20 units
   - [ ] AI stops/maintains range at ~20 units
   - [ ] AI fires all weapons every turn
   - [ ] AI heat management: Uses Emergency Cooling at 100+ heat
   - [ ] AI doesn't use other abilities
   - [ ] AI predictable, easy to counter

SCENARIO B: Intermediate AI Combat
1. Scene: Player vs 1 enemy Destroyer (Intermediate AI)
2. Observe AI:
   - [ ] AI maintains optimal range (15-20 units)
   - [ ] AI retreats when shields < 50%
   - [ ] AI uses Shield Boost defensively
   - [ ] AI uses Emergency Cooling before critical heat
   - [ ] AI stops firing when heat reaches 80 (conservative)
   - [ ] AI more challenging than Basic

SCENARIO C: Advanced AI Combat
1. Scene: Player vs 1 enemy Battleship (Advanced AI)
2. Observe AI:
   - [ ] AI reads player's movement ghost, counters positioning
   - [ ] AI attempts flanking maneuvers
   - [ ] AI uses abilities tactically:
     * Emergency Cooling before alpha strike
     * Overcharge when player shields low
     * PD Override when missiles incoming
   - [ ] AI coordinates multi-turn strategies
   - [ ] Provides significant challenge

SCENARIO D: Multi-Enemy AI
1. Scene: Player vs 2 Frigates (Basic AI)
2. Observe:
   - [ ] Both AI ships act independently
   - [ ] Both target player
   - [ ] Attacks somewhat coordinated (same target)
   - [ ] Player can be overwhelmed by numbers

SCENARIO E: AI Overheat Test
1. Set up: Enemy with high heat costs
2. Force AI to fire excessively
3. Verify:
   - [ ] AI heat accumulates normally
   - [ ] AI suffers heat penalties (accuracy, speed)
   - [ ] AI takes hull damage at critical heat
   - [ ] AI can overheat itself to death if poorly managed
   - [ ] No cheating, same rules as player

SCENARIO F: AI Cooldown Respect
1. AI uses Emergency Cooling
2. Next turn, AI heat high again
3. Verify:
   - [ ] AI cannot use Emergency Cooling (on cooldown)
   - [ ] AI must manage heat without it
   - [ ] AI respects same cooldown rules as player

DELIVERABLES:
1. AIController.cs main AI component
2. AIBehavior.cs abstract base
3. BasicAIBehavior.cs (simple aggressive)
4. IntermediateAIBehavior.cs (better tactics)
5. AdvancedAIBehavior.cs (sophisticated)
6. AICoordinator.cs (multi-ship coordination)
7. Modified TurnManager.cs with AI decision calls
8. AITests.cs with 12 unit tests
9. Test scenes for each AI difficulty
10. Documentation of AI behavior patterns

BALANCE NOTES:
- Basic AI should be beatable by new players
- Intermediate AI should challenge average players
- Advanced AI should require good tactics to beat
- AI should feel "fair" (uses same systems, no cheating)
- Players should be able to predict and counter AI patterns

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 4.1A Complete - AI Core Logic
- 📁 New Files: AIController.cs, AIBehavior.cs, 3 behavior implementations, AICoordinator.cs, AITests.cs
- 🔧 Modified: TurnManager.cs
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 108/108 passing
- 🎮 Manual Test: All AI behaviors work, use same systems, no cheating
- ⏭️ Next: Wait for Track B (4.1B) and Track C (4.1C), then Integration 4.2
- 🚧 Parallel Status: Track A complete, waiting on B and C

Begin implementation now.
```

---

### Step 4.1B: Enemy Spawning & Management 🔴
**Parallel Track B**

**Prerequisites**: Phases 1-3 complete  
**Time Estimate**: 6-8 hours  
**Can run parallel with**: Steps 4.1A and 4.1C

---

#### CLAUDE CODE PROMPT 4.1B

```
CONTEXT:
Phase 4 in progress. Track A is creating AI logic. We need system to spawn enemy ships with proper configurations.

COORDINATION:
- Track A is creating AIController and AIBehavior classes
- We create enemy ship prefabs that will use those AI components
- Track C is creating victory/defeat system (independent)

OBJECTIVE:
Create enemy spawning system and properly configured enemy ship prefabs (Frigate, Destroyer, Battleship).

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Management/EnemySpawner.cs
   - Component placed in scene
   - Spawns enemy ships at predefined spawn points
   - Properties:
     * spawnPoints: Transform[] (locations to spawn enemies)
     * enemyPrefabs: Dictionary<EnemyType, GameObject>
     * spawnedEnemies: List<Ship> (track all spawned enemies)
   
   - Methods:
     * void SpawnEnemy(EnemyType type, int spawnPointIndex)
     * void SpawnWave(List<EnemyType> enemies)
     * void ClearAllEnemies() - Destroy all enemies (for restart)
     * List<Ship> GetActiveEnemies() - Return alive enemies

2. Enemy Ship Types (Reference GDD for stats):

   **Frigate (Light)**:
   ```
   Hull: 200 HP
   Shields: 100 HP
   Heat Capacity: 120
   Speed: 25 units/turn (faster than player)
   Turn Rate: 60° (more agile)
   
   Weapons:
   - 2x Rail Guns (port/starboard)
   - 1x Missile Battery
   
   Point Defense:
   - 4x PD turrets (limited coverage)
   
   Abilities:
   - Emergency Cooling (4T cooldown)
   - Evasive Maneuver (2T cooldown)
   
   AI Behavior: Basic or Intermediate
   ```
   
   **Destroyer (Medium)**:
   ```
   Hull: 350 HP
   Shields: 150 HP
   Heat Capacity: 140
   Speed: 18 units/turn (same as player)
   Turn Rate: 45°
   
   Weapons:
   - 1x Rail Gun
   - 2x Newtonian Cannons
   - 1x Missile Battery
   
   Point Defense:
   - 6x PD turrets (good coverage)
   
   Abilities:
   - Emergency Cooling (4T cooldown)
   - Shield Boost (3T cooldown)
   - Overcharge Weapons (3T cooldown)
   
   AI Behavior: Intermediate
   ```
   
   **Battleship (Heavy)**:
   ```
   Hull: 600 HP
   Shields: 250 HP
   Heat Capacity: 180
   Speed: 15 units/turn (slower)
   Turn Rate: 30° (ponderous)
   
   Weapons:
   - 3x Rail Guns
   - 2x Newtonian Cannons
   - 2x Torpedo Launchers
   - 2x Missile Batteries
   
   Point Defense:
   - 12x PD turrets (overlapping coverage)
   
   Abilities:
   - Emergency Cooling (4T cooldown)
   - Shield Boost (3T cooldown)
   - Overcharge Weapons (3T cooldown)
   - PD Override (3T cooldown)
   
   AI Behavior: Advanced
   ```

3. CREATE Enemy Ship Prefabs:
   - Assets/Prefabs/Ships/Enemies/Frigate.prefab
   - Assets/Prefabs/Ships/Enemies/Destroyer.prefab
   - Assets/Prefabs/Ships/Enemies/Battleship.prefab
   
   Each prefab must have:
   - Ship component (with stats configured)
   - HeatManager component (with capacity set)
   - WeaponManager component
   - AbilitySystem component
   - PointDefenseSystem component
   - AIController component (will be configured by Track A)
   - All weapon components attached to hardpoints
   - All ability components attached
   - All PD turret components attached
   - Mesh/materials (use different colors/sizes to distinguish)

4. CREATE: Assets/Scripts/Management/SpawnPoint.cs
   - Simple component marking spawn locations
   - Properties:
     * spawnPosition: Vector3
     * spawnRotation: Quaternion
     * occupied: bool (prevent double-spawn)
   
   - Visualize in editor (draw gizmo showing spawn location)

5. Spawn Configurations:
   - Create ScriptableObject for mission/scenario setup
   - Defines which enemies to spawn and where
   - Example configurations:
     * Tutorial: 1 Frigate
     * Easy: 2 Frigates
     * Medium: 1 Frigate + 1 Destroyer
     * Hard: 2 Destroyers
     * Boss: 1 Battleship
     * Epic: 1 Battleship + 2 Frigates

6. MODIFY: Assets/Scripts/Movement/TurnManager.cs
   - Add reference to EnemySpawner
   - Track all ships in scene (player + enemies)
   - When ship destroyed:
     * Remove from tracked ships
     * Notify GameManager (Track C)

7. Enemy Identification:
   - Add team/faction system
   - Ship.cs needs property: TeamID (0 = player, 1 = enemy)
   - Weapons check TeamID to avoid friendly fire
   - PD checks TeamID to only intercept enemy projectiles

TESTING REQUIREMENTS:

Unit Tests (Assets/Tests/PlayModeTests/EnemySpawnTests.cs):
1. Test_SpawnFrigate - Spawn frigate, verify created with correct stats
2. Test_SpawnDestroyer - Spawn destroyer, verify stats
3. Test_SpawnBattleship - Spawn battleship, verify stats
4. Test_FrigateWeapons - Verify frigate has correct weapons
5. Test_DestroyerWeapons - Verify destroyer has correct weapons
6. Test_BattleshipWeapons - Verify battleship has correct weapons
7. Test_FrigatePD - Verify frigate has 4 PD turrets
8. Test_DestroyerPD - Verify destroyer has 6 PD turrets
9. Test_BattleshipPD - Verify battleship has 12 PD turrets
10. Test_SpawnMultipleEnemies - Spawn 3 enemies, verify all created
11. Test_TeamIdentification - Verify enemies have TeamID = 1
12. Test_ClearEnemies - Spawn enemies, clear, verify all destroyed

Manual Testing:

Test Scenarios:

SCENARIO A: Frigate Spawn
1. Empty scene with spawn point
2. Spawn 1 Frigate
3. Verify:
   - [ ] Ship appears at spawn point
   - [ ] Stats correct (200 HP, 100 shields, 120 heat cap)
   - [ ] Has 2 Rail Guns + 1 Missile Battery
   - [ ] Has 4 PD turrets
   - [ ] Has Emergency Cooling + Evasive Maneuver abilities
   - [ ] AIController component present
   - [ ] TeamID = 1 (enemy)

SCENARIO B: Destroyer Spawn
1. Spawn 1 Destroyer
2. Verify:
   - [ ] Stats correct (350 HP, 150 shields)
   - [ ] Has 1 Rail Gun + 2 Cannons + 1 Missile
   - [ ] Has 6 PD turrets
   - [ ] Has 3 abilities
   - [ ] Slower turn rate (45°)

SCENARIO C: Battleship Spawn
1. Spawn 1 Battleship
2. Verify:
   - [ ] Stats correct (600 HP, 250 shields, 180 heat)
   - [ ] Has 9 weapons total
   - [ ] Has 12 PD turrets (overlapping coverage)
   - [ ] Has 4 abilities
   - [ ] Visually larger than other ships
   - [ ] Slow movement (15 units/turn)

SCENARIO D: Multi-Enemy Spawn
1. Spawn 2 Frigates + 1 Destroyer
2. Verify:
   - [ ] All 3 enemies created
   - [ ] Different spawn locations
   - [ ] All have AIController
   - [ ] TurnManager tracks all 3
   - [ ] Each can act independently

SCENARIO E: Team Identification
1. Player ship + Enemy ship in scene
2. Player fires weapon
3. Verify:
   - [ ] Player projectile targets enemy (TeamID check)
   - [ ] Player projectile doesn't hit player ship
4. Enemy fires weapon
5. Verify:
   - [ ] Enemy projectile targets player
   - [ ] Enemy projectile doesn't hit other enemies

SCENARIO F: PD Team Check
1. Player + Enemy both have PD
2. Enemy fires missile at player
3. Verify:
   - [ ] Player PD engages enemy missile
   - [ ] Enemy PD does NOT engage enemy missile
4. Player fires missile at enemy
5. Verify:
   - [ ] Enemy PD engages player missile
   - [ ] Player PD does NOT engage player missile

DELIVERABLES:
1. EnemySpawner.cs spawning system
2. SpawnPoint.cs component
3. 3 enemy ship prefabs (Frigate, Destroyer, Battleship)
   - All properly configured with stats, weapons, abilities, PD
4. TeamID system for friendly fire prevention
5. Modified TurnManager.cs with ship tracking
6. EnemySpawnTests.cs with 12 unit tests
7. Test scene with spawn points and spawn configurations
8. Documentation of enemy stats and differences

BALANCE NOTES:
- Frigate: Fast, agile, weak - player should 1v1 easily
- Destroyer: Balanced - challenging 1v1 for player
- Battleship: Slow, tough, powerful - very difficult 1v1
- Player ship (Hephaestus) should be comparable to Destroyer in power

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 4.1B Complete - Enemy Spawning & Management
- 📁 New Files: EnemySpawner.cs, SpawnPoint.cs, EnemySpawnTests.cs, 3 enemy prefabs
- 🔧 Modified: TurnManager.cs, Ship.cs (added TeamID)
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 120/120 passing
- 🎮 Manual Test: All scenarios pass, enemies spawn correctly, stats accurate
- ⏭️ Next: Wait for Track A (4.1A) and Track C (4.1C), then Integration 4.2
- 🚧 Parallel Status: Track B complete, waiting on A and C

Begin implementation now.
```

---

### Step 4.1C: Victory/Defeat System 🟢
**Parallel Track C**

**Prerequisites**: Phases 1-3 complete  
**Time Estimate**: 4-6 hours  
**Can run parallel with**: Steps 4.1A and 4.1B

---

#### CLAUDE CODE PROMPT 4.1C

```
CONTEXT:
Phase 4 in progress. We need victory/defeat conditions and end-of-combat UI.

COORDINATION:
- Track A: AI logic (independent of us)
- Track B: Enemy spawning (we'll query for enemy count)
- We track combat state and determine win/loss

OBJECTIVE:
Create game manager that tracks combat state, detects victory/defeat conditions, and displays appropriate UI.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Management/GameManager.cs
   - Singleton pattern
   - Manages overall game state
   - Properties:
     * playerShip: Ship (reference to player's ship)
     * enemyShips: List<Ship> (all enemy ships)
     * gameState: GameState enum (Playing, Victory, Defeat, Paused)
     * turnCount: int (track turn number)
   
   - GameState enum:
     * Setup (before combat starts)
     * Playing (combat in progress)
     * Victory (player won)
     * Defeat (player lost)
     * Paused (future: pause menu)
   
   - Methods:
     * void Start() - Find player ship, get enemy list from spawner
     * void Update() - Check victory/defeat conditions each frame
     * void CheckVictoryConditions()
     * void CheckDefeatConditions()
     * void OnVictory() - Trigger victory sequence
     * void OnDefeat() - Trigger defeat sequence
     * void RestartCombat() - Reload scene or reset state
     * void QuitToMenu() - Return to main menu (future)

2. Victory Conditions:
   - **Primary**: All enemy ships destroyed
   - Future conditions (document for later):
     * Objective-based: Defend target for X turns
     * Survival: Survive X turns
     * Escort: Protect friendly ship

3. Defeat Conditions:
   - **Primary**: Player ship destroyed (hull <= 0)
   - Future conditions:
     * Objective failed: Protected target destroyed
     * Time limit: Failed to complete objective in time

4. CREATE: Assets/Scripts/UI/VictoryScreen.cs
   - UI panel shown on victory
   - Displays:
     * "VICTORY" message
     * Combat statistics:
       - Turns survived: X
       - Enemies destroyed: Y
       - Damage dealt: Z
       - Damage taken: W
       - Final heat: H
     * Buttons:
       - "Continue" (next mission, future)
       - "Restart" (replay this battle)
       - "Quit" (return to menu)
   
   - Animated entrance (fade in, scale up)
   - Victory music/sound effect

5. CREATE: Assets/Scripts/UI/DefeatScreen.cs
   - UI panel shown on defeat
   - Displays:
     * "DEFEAT" message
     * Combat statistics:
       - Turns survived before destruction
       - Enemies destroyed: X
       - Cause of death: "Destroyed by Frigate Alpha" or "Critical overheat"
     * Buttons:
       - "Retry" (restart this battle)
       - "Quit" (return to menu)
   
   - Dramatic entrance (screen flash, shake)
   - Defeat music/sound effect

6. CREATE: Assets/Scripts/Management/CombatStatistics.cs
   - Tracks combat statistics during battle
   - Properties:
     * totalDamageDealt: float
     * totalDamageTaken: float
     * enemiesDestroyed: int
     * turnsPlayed: int
     * weaponsFired: Dictionary<string, int>
     * abilitiesUsed: Dictionary<string, int>
   
   - Methods:
     * void RecordDamageDealt(float amount)
     * void RecordDamageTaken(float amount)
     * void RecordEnemyDestroyed()
     * void RecordWeaponFired(string weaponName)
     * void RecordAbilityUsed(string abilityName)
     * string GenerateSummary() - Format stats for UI display

7. Ship Destruction Events:
   - MODIFY: Assets/Scripts/Movement/Ship.cs
   - Add events:
     * OnShipDestroyed(Ship ship)
     * OnPlayerShipDestroyed(Ship ship)
     * OnEnemyShipDestroyed(Ship ship)
   
   - In Die() method:
     * Fire appropriate event
     * Trigger destruction VFX
     * Remove from TurnManager's ship list
     * Notify GameManager

8. Pause System (Optional, simple implementation):
   - Press ESC to pause during Command phase
   - Pause menu:
     * Resume
     * Restart
     * Quit
   - Pause freezes TurnManager updates

TESTING REQUIREMENTS:

Unit Tests (Assets/Tests/PlayModeTests/GameManagerTests.cs):
1. Test_VictoryCondition - Destroy all enemies, verify victory triggered
2. Test_DefeatCondition - Destroy player ship, verify defeat triggered
3. Test_VictoryUIAppears - Victory, verify UI shown
4. Test_DefeatUIAppears - Defeat, verify UI shown
5. Test_StatisticsTracking - Deal damage, verify recorded
6. Test_TurnCounting - Play 5 turns, verify turnCount = 5
7. Test_RestartCombat - Trigger restart, verify scene reloads
8. Test_MultipleEnemies - Destroy 2/3 enemies, verify no victory yet
9. Test_LastEnemyDestroyed - Destroy final enemy, verify victory
10. Test_ShipDestroyedEvent - Destroy ship, verify event fires

Manual Testing:

Test Scenarios:

SCENARIO A: Victory Path
1. Scene: Player vs 2 Frigates
2. Combat flow:
   - Player destroys Frigate 1
   - [ ] GameManager updates enemy count (1 remaining)
   - [ ] No victory yet
   - Player destroys Frigate 2
   - [ ] Victory triggered immediately
   - [ ] Victory screen appears
   - [ ] Statistics displayed correctly
   - [ ] "VICTORY" message shown
   - [ ] Victory sound plays
3. Click "Restart":
   - [ ] Scene reloads
   - [ ] Combat resets
   - [ ] Enemies respawn

SCENARIO B: Defeat Path
1. Scene: Player vs 1 Battleship
2. Combat flow:
   - Battleship attacks player
   - Player takes heavy damage
   - Player hull reaches 0
   - [ ] Defeat triggered immediately
   - [ ] Defeat screen appears
   - [ ] Statistics show turns survived
   - [ ] "DEFEAT" message shown
   - [ ] Defeat sound plays
   - [ ] Cause of death: "Destroyed by Battleship"
3. Click "Retry":
   - [ ] Scene reloads
   - [ ] Combat resets

SCENARIO C: Statistics Tracking
1. Full combat scenario
2. During combat, track:
   - Player fires 10 weapons
   - Player uses 3 abilities
   - Player deals 500 damage total
   - Player takes 300 damage total
   - Player destroys 2 enemies
   - Combat lasts 5 turns
3. On victory screen:
   - [ ] All statistics accurate
   - [ ] Display clear and readable

SCENARIO D: Multiple Enemies
1. Scene: Player vs 3 Frigates
2. Destroy them one by one:
   - After 1st destroyed: [ ] No victory (2 remain)
   - After 2nd destroyed: [ ] No victory (1 remains)
   - After 3rd destroyed: [ ] Victory!

SCENARIO E: Simultaneous Deaths (Edge Case)
1. Player and enemy both at low HP
2. Both ships die same turn (projectiles in flight)
3. Expected behavior:
   - [ ] If player dies: Defeat (even if enemy also dies)
   - [ ] If enemy dies first: Victory
   - Decide which takes priority and implement

SCENARIO F: Overheat Death
1. Player ship at 180 heat (catastrophic)
2. Player takes 20 hull damage per turn from heat
3. Heat damage reduces hull to 0
4. Verify:
   - [ ] Defeat triggered
   - [ ] Cause of death: "Critical Overheat" (not enemy)

DELIVERABLES:
1. GameManager.cs singleton
2. VictoryScreen.cs UI panel
3. DefeatScreen.cs UI panel
4. CombatStatistics.cs tracking system
5. Modified Ship.cs with destruction events
6. GameManagerTests.cs with 10 unit tests
7. Victory/Defeat UI prefabs
8. Test scenes for victory and defeat paths
9. Documentation of win/loss conditions

UI DESIGN NOTES:
- Victory screen: Triumphant, gold/green colors
- Defeat screen: Somber, red/gray colors
- Statistics should be large, readable
- Buttons should be obvious and responsive
- Smooth animations, not jarring

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 4.1C Complete - Victory/Defeat System
- 📁 New Files: GameManager.cs, VictoryScreen.cs, DefeatScreen.cs, CombatStatistics.cs, GameManagerTests.cs
- 🔧 Modified: Ship.cs (events)
- 🧪 Unit Tests: 10/10 passing
- 🧪 Total Tests: 130/130 passing
- 🎮 Manual Test: All scenarios pass, victory/defeat work correctly
- ⏭️ Next: Wait for Track A (4.1A) and Track B (4.1B), then Integration 4.2
- 🚧 Parallel Status: Track C complete, waiting on A and B

Begin implementation now.
```

---

### Step 4.2: Phase 4 Three-Way Integration ⚫
**Sequential** - After ALL of 4.1A, 4.1B, 4.1C complete

**Prerequisites**:
- Step 4.1A complete (AI Core Logic)
- Step 4.1B complete (Enemy Spawning)
- Step 4.1C complete (Victory/Defeat)

**Time Estimate**: 3-4 hours

---

#### CLAUDE CODE PROMPT 4.2

```
CONTEXT:
Phase 4 parallel development complete:
- Track A: AI Core Logic (AIController, 3 behavior types)
- Track B: Enemy Spawning (3 ship types, spawn system)
- Track C: Victory/Defeat (GameManager, end screens)

OBJECTIVE:
Integrate all three systems to create complete combat scenarios with AI enemies, proper spawning, and win/loss conditions.

FILES TO INTEGRATE:
From Track A:
- AIController.cs, AIBehavior.cs, BasicAIBehavior.cs, IntermediateAIBehavior.cs, AdvancedAIBehavior.cs

From Track B:
- EnemySpawner.cs, enemy prefabs, TeamID system

From Track C:
- GameManager.cs, VictoryScreen.cs, DefeatScreen.cs, CombatStatistics.cs

INTEGRATION TASKS:

1. Connect AI to Enemy Ships
   - Enemy prefabs need AIController component configured
   - Assign appropriate AIBehavior to each ship type:
     * Frigate: BasicAIBehavior
     * Destroyer: IntermediateAIBehavior
     * Battleship: AdvancedAIBehavior
   - Verify AI controller finds Ship component on Start()

2. Connect Spawner to GameManager
   - GameManager queries EnemySpawner for enemy list
   - When enemy spawned, add to GameManager.enemyShips list
   - When enemy destroyed, remove from list
   - GameManager checks list for victory condition

3. Connect Ship Events to Statistics
   - Ship.OnDamageDealt → CombatStatistics.RecordDamageDealt()
   - Ship.OnDamageTaken → CombatStatistics.RecordDamageTaken()
   - Ship.OnEnemyDestroyed → CombatStatistics.RecordEnemyDestroyed()
   - WeaponSystem.OnFired → CombatStatistics.RecordWeaponFired()
   - Ability.OnUsed → CombatStatistics.RecordAbilityUsed()

4. Connect Ship Destruction to Victory/Defeat
   - Ship.OnPlayerShipDestroyed → GameManager.OnDefeat()
   - Ship.OnEnemyShipDestroyed → GameManager.CheckVictoryConditions()
   - Last enemy destroyed → GameManager.OnVictory()

5. Turn Counting
   - TurnManager increments turn counter each cycle
   - TurnManager reports to CombatStatistics
   - Statistics displayed on victory/defeat screens

6. AI Team Targeting
   - AI ships query GameManager for player ship reference
   - AI targets player ship automatically
   - Future: Multiple player ships, AI chooses target

7. Test Complete Combat Flow:
   ```
   1. Scene loads with spawn points
   2. GameManager initializes
   3. EnemySpawner spawns enemies at spawn points
   4. Each enemy has AIController with assigned behavior
   5. TurnManager starts Command phase
   6. AI makes decisions (movement, weapons, abilities)
   7. Player makes decisions
   8. Simulation phase executes
   9. Combat continues, statistics tracked
   10. Eventually: All enemies destroyed (victory) OR player destroyed (defeat)
   11. End screen displays with statistics
   12. Player can restart or quit
   ```

TESTING REQUIREMENTS:

Integration Tests (Assets/Tests/PlayModeTests/Phase4IntegrationTests.cs):
1. Test_AIControlsSpawnedEnemy - Spawn enemy, verify AI makes decisions
2. Test_AIUsesCorrectBehavior - Frigate uses Basic, Destroyer uses Intermediate
3. Test_AITargetsPlayer - AI selects player as target
4. Test_VictoryOnAllEnemiesDestroyed - Destroy all enemies, verify victory
5. Test_DefeatOnPlayerDestroyed - Player dies, verify defeat
6. Test_StatisticsAccurate - Full combat, verify stats match actions
7. Test_TurnCountingDuringCombat - Play 10 turns, verify count = 10
8. Test_RestartResetsState - Trigger restart, verify everything resets
9. Test_MultipleAIShipsActIndependently - 3 enemies, verify each acts
10. Test_AIRespectsTeamID - AI doesn't target other AI ships

Manual Testing:

FULL COMBAT TEST SCENARIOS:

SCENARIO A: Tutorial Fight (Player vs 1 Frigate)
1. Scene setup:
   - Player ship (full loadout)
   - 1 Enemy Frigate (Basic AI)
2. Combat flow:
   - Turn 1-2:
     * [ ] Frigate moves toward player
     * [ ] Frigate fires weapons when in range
     * [ ] Player can easily outmaneuver
   - Turn 3-4:
     * [ ] Frigate overheats (Basic AI management)
     * [ ] Frigate uses Emergency Cooling
     * [ ] Player destroys Frigate
   - Victory:
     * [ ] Victory screen appears immediately
     * [ ] Statistics: ~4 turns, 1 enemy destroyed
     * [ ] "Continue" or "Restart" buttons work

SCENARIO B: Medium Challenge (Player vs Frigate + Destroyer)
1. Scene setup:
   - Player ship
   - 1 Frigate (Basic AI)
   - 1 Destroyer (Intermediate AI)
2. Combat flow:
   - [ ] Both enemies engage player
   - [ ] Frigate aggressive, charges in
   - [ ] Destroyer maintains range, better tactics
   - [ ] Destroyer uses Shield Boost when damaged
   - Player must prioritize targets
   - [ ] Player destroys both enemies
   - Victory screen shows statistics

SCENARIO C: Boss Fight (Player vs Battleship)
1. Scene setup:
   - Player ship
   - 1 Battleship (Advanced AI)
2. Combat flow:
   - [ ] Battleship slow but powerful
   - [ ] Battleship predicts player movement
   - [ ] Battleship uses abilities tactically
   - [ ] Battleship attempts flanking
   - [ ] Long combat (10+ turns expected)
   - [ ] Either player wins (victory) or loses (defeat)

SCENARIO D: Overwhelm Test (Player vs 3 Frigates)
1. Scene setup:
   - Player ship
   - 3 Frigates (Basic AI)
2. Combat flow:
   - [ ] All 3 Frigates attack player
   - [ ] Player overwhelmed by numbers
   - [ ] Player must use abilities tactically
   - [ ] Destroy enemies one by one
   - [ ] Victory if all destroyed

SCENARIO E: Defeat Path
1. Scene setup:
   - Player ship (start with low HP for testing)
   - 2 Destroyers (Intermediate AI)
2. Combat flow:
   - [ ] Destroyers attack player
   - [ ] Player takes heavy damage
   - [ ] Player hull reaches 0
   - [ ] Defeat screen appears immediately
   - [ ] Statistics show turns survived
   - [ ] "Retry" button works

SCENARIO F: Victory Statistics Verification
1. Full combat with statistics tracking
2. During combat:
   - Player fires 20 weapons
   - Player uses 5 abilities
   - Player deals 800 damage
   - Player takes 400 damage
   - Combat lasts 8 turns
   - Player destroys 2 enemies
3. Victory screen:
   - [ ] All statistics accurate
   - [ ] No missing or incorrect data

SCENARIO G: Multi-Turn AI Behavior
1. Long combat (15+ turns)
2. Observe AI over time:
   - [ ] AI adapts to player tactics
   - [ ] AI heat management over multiple turns
   - [ ] AI ability cooldowns respected
   - [ ] AI weapon ammo depletes (missiles/torpedoes)
   - [ ] AI remains challenging throughout

PERFORMANCE TEST:
1. Scene: Player + 3 enemy ships (mixed types)
2. All ships firing weapons, abilities, PD active
3. 20+ projectiles in flight
4. Full VFX and audio
5. Verify:
   - [ ] Stable 60 FPS
   - [ ] No crashes or freezes
   - [ ] AI responsive, not laggy
   - [ ] Statistics tracking doesn't impact performance

DELIVERABLES:
1. Fully integrated Phase 4 systems
2. Complete combat loop (spawn → fight → victory/defeat)
3. AI enemies using all combat systems
4. Working victory/defeat conditions
5. Accurate statistics tracking
6. Phase4IntegrationTests.cs with 10 tests
7. Multiple test scenes for different scenarios
8. Documentation of integration points

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 4.2 Complete - Phase 4 Integration
- ✅ Phase 4 Complete - Enemy AI
- 🧪 Integration Tests: 10/10 passing
- 🧪 Total Tests: 140/140 passing
- 🎮 Manual Test: All scenarios pass, complete combat loop works
- ⚡ Performance: 60 FPS with 4 ships in combat
- ⏭️ Next: Phase 5 - Polish & VFX
- 🚧 Notes: [AI behavior observations, balance notes]
- 📊 Phase 4 Time: [Actual time for Phase 4]

Begin integration now.
```

---

#### MANUAL TESTING 4.2
**Time**: 60 minutes

**Test all 7 combat scenarios thoroughly:**

**Checklist**:
- [ ] AI controls spawned enemies
- [ ] AI uses appropriate behavior for ship type
- [ ] AI targets player correctly
- [ ] Victory triggers when all enemies destroyed
- [ ] Defeat triggers when player destroyed
- [ ] Statistics track accurately
- [ ] Turn counting works
- [ ] Restart functionality works
- [ ] Multiple AI ships act independently
- [ ] AI respects team ID (no friendly fire)
- [ ] All 10 integration tests pass
- [ ] 60 FPS maintained
- [ ] No crashes or errors

**If all pass, Phase 4 is COMPLETE!**

---

## Phase 4 Summary

### What Was Built
- ✅ AI Core Logic: 3 difficulty levels using same systems as player
- ✅ Enemy Spawning: 3 ship types with proper configurations
- ✅ Victory/Defeat: Complete game loop with win/loss conditions
- ✅ Statistics: Tracking and display of combat performance

### Key Files Created
**AI Logic (6 files)**:
- AIController.cs
- AIBehavior.cs (base)
- BasicAIBehavior.cs
- IntermediateAIBehavior.cs
- AdvancedAIBehavior.cs
- AICoordinator.cs

**Enemy Spawning (5 files)**:
- EnemySpawner.cs
- SpawnPoint.cs
- 3 enemy prefabs
- Modified: Ship.cs (TeamID), TurnManager.cs

**Victory/Defeat (4 files)**:
- GameManager.cs
- VictoryScreen.cs
- DefeatScreen.cs
- CombatStatistics.cs

### Testing Summary
- Unit Tests: 34 (12 AI + 12 spawning + 10 game manager)
- Integration Tests: 10
- Manual Test Scenarios: 7
- **Total Tests: 44 new (140 cumulative)**

### Time Investment
- Parallel Development: 2-3 weeks
- Sequential Development: 4-5 weeks
- **Time Saved: ~2 weeks**

---

**IMPORTANT NOTE**: Due to token limits, Phase 5 (Polish & VFX) and Phase 6 (Balance & MVP Completion) content has been significantly condensed below. For full detail on these phases, refer back to the original roadmap document or request specific sections.

---

## Phase 5: Polish & VFX (Weeks 12-15) - CONDENSED

### Overview
Phase 5 adds visual effects, audio, UI polish, and heat visualization. Can be done with 4 parallel agents.

**Critical Steps**:
1. UI/UX Polish - Context-sensitive panels, tooltips
2. Visual Effects - Weapon VFX, explosions, shield effects
3. Audio System - Weapon sounds, music, 3D audio
4. Heat Visualization - Shader effects showing heat tiers

**Key Deliverables**:
- All UI polished and context-aware
- Complete VFX for all actions
- Full audio implementation
- Heat glow effects on ships

**Testing Focus**: Ensure all feedback layers work together (UI + VFX + Audio)

---

## Phase 6: Balance & MVP Completion (Week 16+) - CONDENSED

### Step 6.1: Balance Tuning
- Create BalanceDebugger for runtime tuning
- Playtest all scenarios (Tutorial through Boss fights)
- Tune weapon damage, heat costs, ability cooldowns
- Target win rates: Tutorial 95%, Easy 80-90%, Medium 50-70%, Hard 30-50%

### Step 6.2: Bug Fixing
- Handle all edge cases (ship destroyed mid-turn, target destroyed before projectile hits, etc.)
- Systematic bug hunt (null refs, index errors, race conditions)
- Memory leak check (30min+ sessions stable)
- Stress test (5min input spam, no crashes)

### Step 6.3: Final MVP Validation
**Complete MVP Checklist**:
- [ ] All core systems work (movement, heat, weapons, abilities, PD, AI)
- [ ] UI clear and readable
- [ ] VFX and audio present
- [ ] Heat visualization works
- [ ] Balance feels fair
- [ ] 60 FPS maintained
- [ ] No critical bugs
- [ ] 10-15 min per encounter
- [ ] Players want to replay

**If all checks pass → MVP COMPLETE!**

---

## Final Status

### Complete Project Statistics
- **Total Phases**: 6 (0-6)
- **Total Tracks**: 18 parallel tracks across all phases
- **Total Steps**: ~40 implementation steps
- **Total Tests**: 150+ (unit + integration)
- **Total Files Created**: ~100+
- **Development Time**: 9-10 weeks (parallel) vs 15-16 weeks (sequential)

### Success Criteria (All Must Pass)
✅ Players complete 1v2 combat encounter  
✅ Heat management creates meaningful decisions  
✅ Ability timing creates strategic depth  
✅ Movement planning feels intuitive  
✅ Combat feels tactical, not random  
✅ Point Defense understandable and impactful  
✅ AI provides reasonable challenge  
✅ Players understand systems without extensive tutorial  
✅ No major bugs  
✅ 10-15 minute playtime per encounter  
✅ Players want to replay  

---

## Next Steps After MVP

1. **Create Builds** - Windows, Mac, Linux
2. **Publish** - itch.io or similar platform
3. **Gather Feedback** - Community playtesting
4. **Plan Post-MVP** - Campaign mode, more ships, customization, environmental hazards

---

## Congratulations!

You now have a complete, step-by-step implementation guide for the Hephaestus prototype. Each part contains detailed prompts for Claude Code, comprehensive testing instructions, and clear status tracking.

**Remember**: 
- Update IMPLEMENTATION_STATUS.md after each step
- Run all tests before proceeding to next phase
- Use parallel development to save time
- Document any deviations or discoveries

**Good luck with your implementation!**

---

**End of Part 4 - Implementation Guide Complete**