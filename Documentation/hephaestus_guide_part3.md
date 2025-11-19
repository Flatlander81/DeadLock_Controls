# Hephaestus Implementation Guide - Part 3
## Phase 2 Integration & Phase 3 Point Defense

---

## Document Overview

This is Part 3 of 4 in the complete Hephaestus implementation guide:
- Part 1: Overview + Phase 0 (Foundation) + Phase 1 (Core Combat)
- Part 2: Phase 2 (Weapon Systems - Parallel Tracks)
- **Part 3**: Phase 2 Integration + Phase 3 (Point Defense) ← You are here
- Part 4: Phase 4 (Enemy AI) + Phase 5 (Polish) + Phase 6 (MVP Completion)

---

## Phase 2 Integration (Continuation from Part 2)

### Step 2.2: Phase 2 Three-Way Integration ⚫
**Sequential** - Must complete after ALL of 2.1A, 2.1B, 2.1C

**Prerequisites**:
- Step 2.1A complete (Weapon Base + RailGun + Cannon)
- Step 2.1B complete (Projectile System)
- Step 2.1C complete (Targeting UI)

**Time Estimate**: 3-4 hours

---

#### CLAUDE CODE PROMPT 2.2

```
CONTEXT:
Phase 2 parallel development complete. We have three independent systems:
- Track A: Weapon base architecture (WeaponSystem, RailGun, NewtonianCannon, WeaponManager)
- Track B: Projectile system (Projectile, BallisticProjectile, HomingProjectile, ProjectileManager)
- Track C: Targeting UI (TargetingController, WeaponConfigPanel, WeaponGroupPanel, UIManager)

OBJECTIVE:
Integrate all three systems into a cohesive weapon combat system where:
1. UI allows player to assign weapons to groups and select targets
2. Weapons fire projectiles during Simulation phase
3. Projectiles travel and hit targets
4. Damage applies and heat accumulates

FILES TO REVIEW AND INTEGRATE:
From Track A:
- Assets/Scripts/Combat/Weapons/WeaponSystem.cs
- Assets/Scripts/Combat/Weapons/RailGun.cs
- Assets/Scripts/Combat/Weapons/NewtonianCannon.cs
- Assets/Scripts/Combat/Weapons/WeaponManager.cs

From Track B:
- Assets/Scripts/Combat/Projectiles/Projectile.cs
- Assets/Scripts/Combat/Projectiles/BallisticProjectile.cs
- Assets/Scripts/Combat/Projectiles/HomingProjectile.cs
- Assets/Scripts/Combat/ProjectileManager.cs
- Assets/Scripts/Combat/Projectiles/InstantHitEffect.cs

From Track C:
- Assets/Scripts/Combat/Targeting/TargetingController.cs
- Assets/Scripts/UI/WeaponConfigPanel.cs
- Assets/Scripts/UI/WeaponGroupPanel.cs
- Assets/Scripts/UI/UIManager.cs
- Assets/Scripts/UI/TargetingLineRenderer.cs

INTEGRATION TASKS:

1. Connect Weapons to ProjectileManager
   - Replace ProjectileManager stub calls with real implementation
   - Verify RailGun calls ProjectileManager.SpawnInstantHitEffect()
   - Verify NewtonianCannon calls ProjectileManager.SpawnBallisticProjectile()
   - Test projectiles spawn at weapon hardpoint positions
   - Test projectiles travel toward target positions

2. Connect Targeting UI to WeaponManager
   - Replace IWeaponManager interface with real WeaponManager
   - Verify WeaponConfigPanel can read weapon list from WeaponManager
   - Verify group assignments propagate to WeaponManager
   - Verify WeaponGroupPanel correctly triggers weapon firing
   - Test targeting lines appear when groups assigned

3. Integrate with Heat System (Phase 1)
   - Verify weapon firing adds heat via HeatManager
   - Verify HeatDisplay shows planned heat from queued weapons
   - Verify weapon heat costs respect Overcharge ability multipliers
   - Test heat penalties affect weapon accuracy (visual only for now)

4. Integrate with Turn System
   - Verify execution order in TurnManager:
     1. Start Simulation
     2. Execute movement
     3. Execute abilities
     4. Fire weapons (with spin-up delays)
     5. Update projectiles (in flight)
     6. Resolve collisions
     7. Apply cooling
     8. Tick cooldowns
   - Test with multiple ships firing simultaneously

5. Test Complete Combat Flow
   - Player selects enemy during Command phase
   - Player assigns weapon groups to enemy
   - Player ends turn
   - Simulation phase:
     * Weapons spin up
     * Weapons fire (spawn projectiles)
     * Projectiles travel
     * Projectiles hit targets
     * Damage applies
     * Heat accumulates
   - Return to Command phase for next turn

6. Handle Edge Cases
   - Target destroyed before projectile hits (projectile continues ballistic)
   - Weapon on cooldown when group fired (skip that weapon)
   - Weapon out of arc when group fired (skip that weapon)
   - Multiple weapons in same group fire with different spin-up times
   - Projectile owner ship destroyed mid-flight (projectile continues)
   - Heat causes catastrophic damage during weapon fire sequence

7. Synchronize UI Updates
   - WeaponConfigPanel updates when weapon assignments change
   - WeaponGroupPanel updates when target changes
   - Targeting lines update when assignments change
   - Heat bar updates when weapons queued
   - All updates smooth without flickering

8. Performance Testing
   - Test with 4 ships (player + 3 enemies)
   - Each ship fires 4-6 weapons per turn
   - 20+ projectiles in flight simultaneously
   - Target 60 FPS maintained
   - No garbage collection spikes

TESTING REQUIREMENTS:

Integration Tests (Assets/Tests/PlayModeTests/Phase2IntegrationTests.cs):
1. Test_WeaponFiresProjectile - Fire weapon, verify projectile spawned
2. Test_RailGunInstantHit - Fire rail gun, verify instant damage applied
3. Test_CannonBallisticTravel - Fire cannon, verify projectile travels and hits
4. Test_UIWeaponGroupAssignment - UI assigns weapon to group, verify WeaponManager updated
5. Test_UITargetSelection - UI selects target, verify TargetingController updated
6. Test_UIFireGroup - UI fires group, verify weapons execute
7. Test_MultipleWeaponsInGroup - Group with 3 weapons, verify all fire
8. Test_WeaponSpinUpStaggering - 2 weapons, different spin-ups, verify fire at different times
9. Test_WeaponHeatIntegration - Fire weapon, verify heat added to HeatManager
10. Test_ProjectileDamageApplication - Projectile hits ship, verify TakeDamage() called
11. Test_TargetingLinesAppear - Assign group to target, verify colored line appears
12. Test_AlphaStrikeAllWeapons - Alpha strike, verify all assigned weapons fire
13. Test_OutOfArcWeaponSkipped - Weapon out of arc, verify doesn't fire in group
14. Test_CooldownWeaponSkipped - Weapon on cooldown, verify skipped in group
15. Test_TurnExecutionOrder - Verify: movement → abilities → weapons → projectiles → cooling

Manual Testing Instructions:
Create full combat test scene:
1. Player ship (Hephaestus) with:
   - 2x RailGun (hardpoints port/starboard)
   - 1x NewtonianCannon (hardpoint forward)
   - WeaponManager component
   - All Phase 1 systems (Heat, Abilities)

2. 2x Enemy ships (basic frigates) at different positions:
   - Each with 1-2 weapons
   - Basic AI or player-controlled for testing

3. Full UI setup:
   - Heat bar (top center)
   - Ability bar (bottom)
   - WeaponConfigPanel (left, shown when player selected)
   - WeaponGroupPanel (right, shown when enemy selected)
   - Targeting lines renderer

Test Scenarios:

SCENARIO A: Basic Combat Flow
1. Command Phase:
   - Left-click Hephaestus
   - Assign Rail Guns to Group 1
   - Assign Cannon to Group 2
   - Left-click Enemy A
   - Press "1" (fire Group 1 at Enemy A)
   - Verify: Blue targeting line appears, heat bar shows +30 planned
2. End Turn (Simulation):
   - Verify: Rail guns spin up (0.2s), fire instantly, damage applied
   - Verify: Heat becomes 30
3. Next Command Phase:
   - Verify: Heat reduced by passive cooling (30 - 20 = 10)
   - Verify: Weapons ready to fire again

SCENARIO B: Mixed Weapons Attack
1. Command Phase:
   - Assign all weapons to different groups
   - Select Enemy A
   - Press "1" (Rail Gun)
   - Press "2" (Cannon)
   - Verify: Two targeting lines (blue + red)
   - Verify: Heat bar shows +45 planned (15 + 30)
2. Simulation:
   - Verify: Rail gun fires at 0.2s (instant hit)
   - Verify: Cannon fires at 0.5s (ballistic projectile spawns)
   - Verify: Projectile travels toward enemy
   - Verify: Projectile hits and damages enemy
   - Verify: Heat accumulates to 45

SCENARIO C: Multi-Target Attack
1. Command Phase:
   - Assign Rail Guns (Group 1) and Cannon (Group 2)
   - Select Enemy A, press "1"
   - Select Enemy B, press "2"
   - Verify: Two targeting lines to different enemies
2. Simulation:
   - Verify: Group 1 fires at Enemy A
   - Verify: Group 2 fires at Enemy B
   - Verify: Both enemies take damage

SCENARIO D: Alpha Strike
1. Command Phase:
   - Assign all weapons to groups
   - Select Enemy A
   - Press Space (Alpha Strike)
   - Verify: All targeting lines appear
   - Verify: Heat bar shows +60 planned (all weapons)
2. Simulation:
   - Verify: All weapons fire (staggered by spin-up)
   - Verify: Rail guns fire first, cannon fires last
   - Verify: All projectiles hit
   - Verify: Enemy takes combined damage

SCENARIO E: Heat Management
1. Command Phase:
   - Current heat: 80
   - Try to fire all weapons (+60 heat = 140 total)
   - Verify: Heat bar shows warning (red zone)
   - Can still execute if player confirms
2. Simulation:
   - Verify: All weapons fire
   - Verify: Heat reaches 140 (Critical tier)
   - Verify: Ship takes hull damage from heat
   - Verify: Movement speed penalty applies next turn

SCENARIO F: Arc Validation
1. Command Phase:
   - Position enemy behind player
   - Try to fire Cannon (180° forward arc)
   - Verify: "OUT OF ARC" warning in UI
   - Weapon should not fire

SCENARIO G: Projectile in Flight
1. Simulation Phase:
   - Fire Cannon at distant enemy
   - Observe projectile travel (takes 10 seconds at 2 units/sec for 20 units)
   - Enemy can move during travel
   - Projectile hits where enemy WAS, not where enemy IS (ballistic)

Performance Test:
1. Spawn 4 ships total
2. Each ship fires 5 weapons per turn
3. 20 projectiles in flight
4. Verify: 60 FPS maintained
5. Check profiler: No GC spikes, physics stable

DELIVERABLES:
1. Fully integrated weapon system (Weapons → Projectiles → Damage)
2. Functional targeting UI (select targets, assign groups, fire)
3. Heat integration (weapon heat costs apply and display)
4. Turn system integration (correct execution order)
5. Phase2IntegrationTests.cs with 15 tests (all passing)
6. Full combat test scene
7. Documentation of any issues and resolutions
8. Performance profiling results

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 2.2 Complete - Phase 2 Three-Way Integration
- 🔧 Modified Files: [List any integration fixes]
- 🧪 Integration Tests: 15/15 passing
- 🧪 Unit Tests Total: 59/59 passing (23 Phase 1 + 36 Phase 2)
- 🎮 Manual Test: All 7 scenarios pass
- ⚡ Performance: 60 FPS with 20+ projectiles
- ⏭️ Next: Step 2.3 - Missile and Torpedo Implementation
- 🚧 Notes: [Integration challenges, performance observations]
- 📊 Phase 2A Time: [Actual time for tracks A+B+C+Integration]

Begin integration now.
```

---

#### MANUAL TESTING 2.2
**Time**: 45 minutes

**Full integration verification:**

1. Set up complete combat test scene as described
2. Run through all 7 test scenarios (A through G)
3. Document results for each scenario
4. Run performance test with profiler active
5. Verify all 15 integration tests pass

**Checklist**:
- [ ] Weapons fire projectiles correctly
- [ ] UI allows weapon group assignment
- [ ] Targeting UI works (select, assign, fire)
- [ ] Projectiles travel and hit targets
- [ ] Damage applies correctly
- [ ] Heat accumulates from weapons
- [ ] Heat bar shows planned weapon heat
- [ ] Targeting lines appear correctly
- [ ] Multi-targeting works
- [ ] Alpha Strike fires all weapons
- [ ] Arc validation prevents invalid firing
- [ ] Spin-up delays stagger weapon fire
- [ ] Turn execution order correct
- [ ] 60 FPS with 20+ projectiles
- [ ] No console errors

**If all pass, proceed to Step 2.3**

---

### Step 2.3: Missile and Torpedo Weapons ⚫
**Sequential** - After integration complete

**Prerequisites**:
- Step 2.2 complete (weapons + projectiles + UI integrated)
- Homing projectile system working

**Time Estimate**: 4-6 hours

---

#### CLAUDE CODE PROMPT 2.3

```
CONTEXT:
Phase 2 base integration complete. We have working RailGuns (instant) and Newtonian Cannons (ballistic). Now implementing Missiles and Torpedoes which use homing projectiles and limited ammo.

Existing files:
- WeaponSystem.cs (base class)
- HomingProjectile.cs (homing logic)
- ProjectileManager.cs (spawn methods)
- WeaponManager.cs (weapon group management)

OBJECTIVE:
Implement Missile Battery and Torpedo Launcher weapons with homing projectiles, limited ammo, and different tactical characteristics.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Combat/Weapons/MissileBattery.cs
   - Inherits WeaponSystem
   - Settings:
     * Damage = 30
     * HeatCost = 20
     * FiringArc = 360 (launcher can fire any direction)
     * MaxRange = 35
     * MaxCooldown = 1 (turn)
     * SpinUpTime = 0.4f
     * AmmoCapacity = 20
     * CurrentAmmo = 20 (starts full)
   
   - HomingProjectile properties:
     * Speed = 1.5 units/second (fast)
     * TurnRate = 90 degrees/second (agile)
     * Lifetime = 30 seconds
   
   - Fire() implementation:
     * Check ammo (if 0, can't fire)
     * Decrement ammo
     * Spawn homing projectile via ProjectileManager
     * Projectile seeks target ship's current position (updates each frame)
     * Apply heat to owner ship
     * Start cooldown
   
   - GetProjectileInfo():
     * Return ProjectileType.Homing with target ship reference

2. CREATE: Assets/Scripts/Combat/Weapons/TorpedoLauncher.cs
   - Inherits WeaponSystem
   - Settings:
     * Damage = 80 (devastating)
     * HeatCost = 25
     * FiringArc = 30 (narrow forward arc)
     * MaxRange = 50 (very long)
     * MaxCooldown = 3 (turns)
     * SpinUpTime = 1.0f (slow, telegraphed)
     * AmmoCapacity = 6
     * CurrentAmmo = 6
   
   - HomingProjectile properties:
     * Speed = 0.5 units/second (very slow)
     * TurnRate = 30 degrees/second (poor maneuverability)
     * Lifetime = 120 seconds (can travel very far)
   
   - Fire() implementation:
     * Check ammo (if 0, can't fire)
     * Decrement ammo
     * Spawn homing projectile (large, slow, high damage)
     * Projectile homes on target
     * Apply heat to owner ship
     * Long cooldown (3 turns)
   
   - Tactical characteristics:
     * Takes many seconds to reach distant targets
     * Easier for point defense to intercept (slow)
     * Massive damage if hits
     * Very limited ammo (only 6 shots per combat)

3. MODIFY: Assets/Scripts/UI/WeaponConfigPanel.cs
   - Add ammo display for weapons with AmmoCapacity > 0
   - Show: "Ammo: 18/20" or "Ammo: 5/6"
   - Gray out weapon in UI if ammo depleted
   - Warning color if ammo low (< 25%)

4. MODIFY: Assets/Scripts/UI/WeaponGroupPanel.cs
   - Show ammo status for groups with ammo-limited weapons
   - Warning if any weapon in group out of ammo
   - Example: "Group 3: Torpedoes (5/6 ammo)"

5. CREATE: Ammo Resupply System (for future)
   - For now, ammo doesn't resupply during combat
   - Future: Between-mission resupply
   - Document interface for later implementation

6. Visual Distinctions
   - Missiles: Smaller projectiles, fast, bright contrail
   - Torpedoes: Larger projectiles, slow, thick contrail, more visible
   - Different colors: Missiles (orange/yellow), Torpedoes (red/white)

TESTING REQUIREMENTS:

Unit Tests (Assets/Tests/PlayModeTests/HomingWeaponsTests.cs):
1. Test_MissileSpawn - Fire missile, verify spawned
2. Test_MissileHoming - Missile seeks moving target
3. Test_MissileHitMovingTarget - Missile intercepts moving target
4. Test_MissileSpeed - Verify missile faster than torpedo
5. Test_MissileAmmoConsumption - Fire missile, verify ammo decremented
6. Test_MissileAmmoDepletion - Fire 20 missiles, verify can't fire 21st
7. Test_MissileCooldown - Fire missile, verify 1 turn cooldown
8. Test_TorpedoSpawn - Fire torpedo, verify spawned
9. Test_TorpedoHoming - Torpedo seeks target (slower turn rate)
10. Test_TorpedoDamage - Torpedo hits, verify 80 damage (vs missile 30)
11. Test_TorpedoAmmoConsumption - Fire torpedo, verify ammo decremented
12. Test_TorpedoAmmoDepletion - Fire 6 torpedoes, verify can't fire 7th
13. Test_TorpedoCooldown - Fire torpedo, verify 3 turn cooldown
14. Test_TorpedoArcRestriction - Target outside 30° arc, verify can't fire
15. Test_MissileVsTorpedoSpeed - Both fired, verify missile arrives first

Manual Testing Instructions:
1. Add weapons to player ship:
   - 2x Missile Batteries (dorsal/ventral hardpoints)
   - 1x Torpedo Launcher (forward hardpoint)

2. Add to enemy ships for variety

Test Scenarios:

SCENARIO A: Missile Barrage
1. Assign both Missile Batteries to Group 4
2. Select enemy at medium range (20 units)
3. Fire Group 4
4. Observe:
   - 2 missiles spawn
   - Missiles travel fast (1.5 units/sec)
   - Missiles home on target, adjusting course
   - Missiles hit target within 10-15 seconds
   - Combined 60 damage applied
   - 40 heat accumulated (20 per missile)
   - Ammo: 18/20 for each launcher

SCENARIO B: Torpedo Strike
1. Assign Torpedo to Group 3
2. Select enemy at long range (40 units)
3. Fire Group 3
4. Observe:
   - Torpedo spawns (large, distinctive)
   - Torpedo travels slowly (0.5 units/sec)
   - Takes 80 seconds to reach target!
   - Torpedo adjusts course if enemy moves
   - 80 damage if hits
   - 25 heat accumulated
   - Ammo: 5/6
   - Weapon on 3-turn cooldown

SCENARIO C: Ammo Management
1. Fire missiles repeatedly (10 shots per launcher)
2. Observe ammo counters decreasing
3. Attempt to fire when ammo depleted
4. Verify: Weapon won't fire, UI shows "NO AMMO"

SCENARIO D: Mixed Long-Range Engagement
1. Enemy at 40 units distance
2. Fire: Missiles + Torpedo + Cannon
3. Observe:
   - Cannon projectile arrives first (~20 seconds)
   - Missiles arrive second (~27 seconds)
   - Torpedo arrives last (~80 seconds)
   - All home on target (or miss if target destroyed early)

SCENARIO E: Torpedo Arc Restriction
1. Position enemy to side or rear
2. Try to fire torpedo (30° forward arc)
3. Verify: "OUT OF ARC" warning
4. Reposition ship to face enemy
5. Now torpedo can fire

SCENARIO F: Saturation Attack
1. Assign all weapons to one target
2. Alpha Strike
3. Observe:
   - Rail guns hit instantly
   - Cannon projectile travels
   - Missiles home in
   - Torpedo slowly approaches
   - Staggered damage application
   - If target destroyed early, remaining projectiles continue or self-destruct

DELIVERABLES:
1. MissileBattery.cs implementation
2. TorpedoLauncher.cs implementation
3. Modified WeaponConfigPanel with ammo display
4. Modified WeaponGroupPanel with ammo warnings
5. Missile and Torpedo prefabs (distinct visuals)
6. HomingWeaponsTests.cs with 15 unit tests
7. Test scene with all 4 weapon types
8. Documentation of tactical differences between weapons

BALANCE NOTES:
- Missiles: Reliable damage, limited but reasonable ammo (20)
- Torpedoes: Devastating but scarce (6), slow, vulnerable to PD
- Consider: Should torpedo 1-shot frigates? (80 dmg vs 200 HP = no)
- Consider: Torpedo main use = heavy targets (destroyers, battleships)

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 2.3 Complete - Missile and Torpedo Implementation
- 📁 New Files: MissileBattery.cs, TorpedoLauncher.cs, HomingWeaponsTests.cs, prefabs
- 🔧 Modified: WeaponConfigPanel.cs, WeaponGroupPanel.cs
- 🧪 Unit Tests: 15/15 passing
- 🧪 Total Tests: 74/74 passing (59 previous + 15 new)
- 🎮 Manual Test: All scenarios pass, homing works, ammo management functional
- ⏭️ Next: Phase 2 Complete! Begin Phase 3 - Point Defense System
- 🚧 Notes: [Observations on homing behavior, balance considerations]
- 📊 Phase 2 Total Time: [Actual time for entire Phase 2]

Begin implementation now.
```

---

#### MANUAL TESTING 2.3
**Time**: 30 minutes

**Test all 6 scenarios thoroughly:**

**Checklist**:
- [ ] Missiles spawn and home correctly
- [ ] Torpedoes spawn and home (slower)
- [ ] Missiles faster than torpedoes
- [ ] Ammo decrements on firing
- [ ] Can't fire when ammo depleted
- [ ] UI shows ammo counts
- [ ] Missile cooldown (1 turn) works
- [ ] Torpedo cooldown (3 turns) works
- [ ] Torpedo arc restriction (30°) enforced
- [ ] Homing projectiles adjust course
- [ ] Damage values correct (30 missile, 80 torpedo)
- [ ] Heat costs correct (20 missile, 25 torpedo)
- [ ] No console errors
- [ ] Performance good with multiple homing projectiles

**If all pass, Phase 2 is COMPLETE!**

---

## Phase 2 Completion Checklist

Before proceeding to Phase 3, verify:
- [ ] All 4 weapon types implemented (RailGun, Cannon, Missile, Torpedo)
- [ ] Projectile system working (instant, ballistic, homing)
- [ ] Targeting UI functional (select, assign groups, fire)
- [ ] Weapon groups work (1-4 + Alpha Strike)
- [ ] Multi-targeting works (different groups → different enemies)
- [ ] Heat integration works (weapon heat costs apply)
- [ ] Ammo system works (limited ammo, UI displays, depletion)
- [ ] Arc validation works (weapons can't fire outside arc)
- [ ] Range validation works (weapons can't fire beyond range)
- [ ] Cooldowns work (weapons on cooldown can't fire)
- [ ] Spin-up delays work (staggered firing)
- [ ] All unit tests pass (74 total)
- [ ] Performance acceptable (60 FPS with 20+ projectiles)

**Phase 2 Duration**: Approximately 3 weeks with parallel development

---

## Phase 3: Point Defense System (Weeks 6-7)

### Overview
Phase 3 implements automated defensive systems that intercept incoming projectiles. Point Defense (PD) turrets are reactive, not player-controlled, providing passive protection against missiles and torpedoes.

**Parallel Development**: 2 agents can work simultaneously
- 🔵 Track A: PD Logic System
- 🔴 Track B: PD Visual & Audio

**Timeline**: 1-2 weeks with parallel development

---

### Pre-Phase Manual Setup

#### MANUAL TASK 3.0: Create PD Structure
**Time**: 15 minutes

**Steps**:

1. Create folders:
```
Assets/Scripts/Combat/PointDefense/
Assets/Prefabs/PointDefense/
Assets/VFX/PointDefense/
```

2. Add PD turret hardpoints to ship prefab:
   - Create 8 empty child GameObjects on ship:
     * "PD_Forward_Port"
     * "PD_Forward_Starboard"
     * "PD_Aft_Port"
     * "PD_Aft_Starboard"
     * "PD_Port_Forward"
     * "PD_Port_Aft"
     * "PD_Starboard_Forward"
     * "PD_Starboard_Aft"
   - Position appropriately around ship hull
   - Each should have forward direction pointing outward

3. Create simple turret mesh (cylinder + sphere, or use cubes)
   - Will add proper models later

4. Update IMPLEMENTATION_STATUS.md for Phase 3

**Status Update**: Document PD structure created

---

### Phase 3 Parallel Development (2 Agents)

Phase 3 can be split into 2 parallel tracks:
- 🔵 Track A: PD Logic System
- 🔴 Track B: PD Visual & Audio

---

### Step 3.1A: Point Defense Logic System 🔵
**Parallel Track A**

**Prerequisites**: Phases 1-2 complete  
**Time Estimate**: 6-8 hours  
**Can run parallel with**: Step 3.1B

---

#### CLAUDE CODE PROMPT 3.1A

```
CONTEXT:
Phases 1-2 complete. We have working weapons that fire projectiles. Now implementing Point Defense - an automated defensive system that intercepts incoming projectiles.

Existing files:
- Projectile.cs, BallisticProjectile.cs, HomingProjectile.cs
- ProjectileManager.cs
- Ship.cs
- WeaponSystem.cs

OBJECTIVE:
Create the Point Defense logic system that automatically detects and engages incoming threats during the Simulation phase.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Combat/PointDefense/PDTurret.cs
   - Component attached to PD hardpoint GameObjects
   - Properties:
     * CoverageArc: float (90-120 degrees, depends on turret position)
     * EngagementRange: float (default 15 units)
     * MaxTargetsPerTurn: int (default 2, or 4 with PD Override ability)
     * CurrentEngagementsThisTurn: int (tracks how many engaged this turn)
     * IsOperational: bool (false if destroyed by enemy fire)
     * TurretTransform: Transform (for visual rotation toward threats)
     * OwnerShip: Ship
   
   - Arc configuration examples:
     * Forward turrets: 120° arc centered on ship's forward
     * Aft turrets: 120° arc centered on ship's rear
     * Side turrets: 90° arc covering port/starboard
   
   - Methods:
     * bool IsInArc(Vector3 threatPosition) - Check if threat within coverage arc
     * bool IsInRange(Vector3 threatPosition) - Check if threat within 15 units
     * bool CanEngage() - Check: operational, not exceeded max targets
     * PDTarget SelectThreat(List<Projectile> threats) - Find best threat to engage
     * bool AttemptIntercept(Projectile target) - Roll for success, destroy if successful
     * void ResetEngagements() - Called at start of turn, reset counter
     * void TakeDamage() - Mark as non-operational

2. CREATE: Assets/Scripts/Combat/PointDefense/PDTarget.cs
   - Helper class for threat prioritization
   - Properties:
     * Projectile: Projectile reference
     * Distance: float (to ship)
     * Damage: float (projectile damage)
     * ProjectileType: enum (for success rate calculation)
     * Priority: float (calculated score)
   
   - Priority calculation:
     * Closer threats score higher
     * Higher damage threats score higher
     * Formula: Priority = (Damage / 10) * (1 / (Distance + 1))

3. CREATE: Assets/Scripts/Combat/PointDefense/PointDefenseSystem.cs
   - Component on Ship GameObject
   - Manages all PD turrets on ship
   - Properties:
     * turrets: List<PDTurret>
     * engagementLog: List<string> (for debugging/UI feedback)
   
   - Success rates by projectile type:
     * Torpedoes: 80% (slow, easy to hit)
     * Missiles: 60% (standard)
     * Ballistic (Cannon): 30% (fast, hard to hit)
     * Instant (Rail Gun): 0% (impossible to intercept)
   
   - Methods:
     * void Start() - Find all PDTurret components on ship and children
     * void Update() - During Simulation phase, continuously scan for threats
     * List<Projectile> DetectThreats() - Find all enemy projectiles in flight
     * void EngageThreats(List<Projectile> threats) - Each turret selects and engages
     * bool RollInterception(Projectile threat) - Calculate success based on type
     * void DestroyProjectile(Projectile proj) - Remove projectile, spawn VFX
     * void ResetAllTurrets() - Call ResetEngagements() on all turrets at turn start
     * int GetOperationalTurretCount() - Count turrets with IsOperational = true

4. Engagement Flow (During Simulation Phase):
   ```
   Each frame during Simulation:
   1. PointDefenseSystem.Update() runs on all ships
   2. DetectThreats() finds all enemy projectiles
   3. Filter threats: Only projectiles within 15 units of ship
   4. For each operational turret:
      a. Filter threats to those in turret's arc
      b. SelectThreat() picks highest priority threat
      c. AttemptIntercept(threat)
      d. Roll for success based on projectile type
      e. If success: DestroyProjectile(), increment engagement counter
      f. If turret reached MaxTargetsPerTurn, skip remaining
   5. Continue until all turrets engaged or no threats remain
   ```

5. MODIFY: Assets/Scripts/Combat/Projectiles/Projectile.cs
   - Add property: CanBeIntercepted: bool (true for missiles/torpedoes, false for instant)
   - Add method: OnIntercepted() - Called when PD destroys projectile
   - Projectile should have reference to ProjectileManager for removal

6. MODIFY: Assets/Scripts/Combat/Abilities/PDOverride.cs
   - Connect to PointDefenseSystem
   - Execute(): Set all turrets' MaxTargetsPerTurn = 4 (from 2)
   - Reset at end of turn: Set back to 2

7. CREATE: Assets/Scripts/Combat/PointDefense/PDDamageController.cs
   - Handles enemy weapons targeting and destroying PD turrets
   - PD turrets can be individually targeted by enemy
   - When turret takes damage, IsOperational = false
   - Visual indication (sparks, smoke, blackened)

8. MODIFY: Assets/Scripts/Movement/TurnManager.cs
   - At start of each Simulation phase:
     * Call ship.PointDefenseSystem.ResetAllTurrets() on all ships
   - During Simulation phase:
     * PD systems run automatically in Update()

TESTING REQUIREMENTS:

Unit Tests (Assets/Tests/PlayModeTests/PointDefenseTests.cs):
1. Test_PDTurretArcDetection - Projectile in arc, verify detected
2. Test_PDTurretRangeDetection - Projectile in range, verify detected
3. Test_PDTurretEngagementLimit - Fire 3 projectiles, verify only 2 intercepted
4. Test_PDSuccessRateTorpedo - Fire 100 torpedoes, verify ~80% intercepted
5. Test_PDSuccessRateMissile - Fire 100 missiles, verify ~60% intercepted
6. Test_PDSuccessRateCannon - Fire 100 cannon shots, verify ~30% intercepted
7. Test_PDCannotInterceptInstantHit - Fire rail gun, verify NOT intercepted
8. Test_PDPrioritization - Fire torpedo + missile, verify torpedo engaged first (higher damage)
9. Test_PDOverrideAbility - Use PD Override, verify max targets becomes 4
10. Test_PDTurretDestruction - Destroy turret, verify no longer engages
11. Test_MultipleTurretsEngageSameProjectile - Verify only one turret engages each projectile
12. Test_PDResetBetweenTurns - Engage 2 projectiles turn 1, verify can engage 2 more turn 2

Manual Testing Instructions:
1. Set up test scene:
   - Player ship with 8 PD turrets (2 forward, 2 aft, 2 port, 2 starboard)
   - Enemy ship that fires missiles/torpedoes at player

2. Test UI:
   - Display: Operational turrets count (8/8)
   - Display: Engagement log (which turrets firing at what)
   - Display: Interception success/fail messages

Test Scenarios:

SCENARIO A: Basic Interception
1. Enemy fires 2 missiles at player
2. Observe PD system:
   - Turrets detect threats
   - Closest threats prioritized
   - Turrets engage (roll for success)
   - If successful: Projectile destroyed (log message for now, VFX in Track B)
   - If failed: Projectile continues, hits ship
3. Verify: Maximum 2 interceptions per turret

SCENARIO B: Saturation Attack
1. Enemy fires 10 missiles simultaneously
2. With 8 turrets, max 16 interceptions possible (2 each)
3. Observe:
   - PD system engages threats
   - Some missiles destroyed
   - Some missiles overwhelm PD, hit ship
4. Count: Approximately 60% of missiles destroyed

SCENARIO C: Arc Coverage
1. Position enemy to side of player (port side)
2. Enemy fires missile
3. Observe:
   - Only port-side turrets engage (within arc)
   - Forward/aft/starboard turrets don't engage (out of arc)
4. Test all 4 sides (fore, aft, port, starboard)

SCENARIO D: PD Override Ability
1. Enemy fires 3 missiles at player
2. Standard PD: Each turret intercepts max 2 (some missiles get through)
3. Use PD Override ability
4. Same attack:
   - Each turret now intercepts max 4
   - More missiles destroyed
5. Verify: Override lasts one turn, then resets

SCENARIO E: Turret Destruction
1. Player has 8 operational turrets
2. Enemy targets and destroys 2 forward turrets
3. Player turret count: 6/8 operational
4. Enemy fires missiles from front
5. Observe:
   - Fewer turrets engage (forward coverage reduced)
   - More missiles get through
6. Verify: Destroyed turrets don't engage

SCENARIO F: Projectile Type Differences
1. Enemy fires:
   - 5 Torpedoes (slow)
   - 5 Missiles (medium)
   - 5 Cannon shots (fast)
   - 5 Rail Gun shots (instant)
2. Observe interception rates:
   - ~4 torpedoes destroyed (80%)
   - ~3 missiles destroyed (60%)
   - ~1.5 cannon shots destroyed (30%)
   - 0 rail gun shots destroyed (instant, can't intercept)

DELIVERABLES:
1. PDTurret.cs component
2. PDTarget.cs helper class
3. PointDefenseSystem.cs manager
4. PDDamageController.cs
5. Modified Projectile.cs with interception support
6. Modified PDOverride.cs ability connection
7. Modified TurnManager.cs with PD reset
8. PointDefenseTests.cs with 12 unit tests
9. Test scene with PD system active
10. Documentation of engagement logic and success rates

COORDINATION NOTE:
- Track B (parallel) is creating VFX and audio for PD firing
- We provide events they can hook into:
  * OnPDEngagement(PDTurret turret, Projectile target)
  * OnPDSuccess(PDTurret turret, Projectile destroyed)
  * OnPDFailure(PDTurret turret, Projectile missed)

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 3.1A Complete - PD Logic System
- 📁 New Files: PDTurret.cs, PDTarget.cs, PointDefenseSystem.cs, PDDamageController.cs, PointDefenseTests.cs
- 🔧 Modified: Projectile.cs, PDOverride.cs, TurnManager.cs
- 🧪 Unit Tests: 12/12 passing
- 🧪 Total Tests: 86/86 passing
- 🎮 Manual Test: All scenarios pass, PD engages correctly, success rates accurate
- ⏭️ Next: Wait for Track B (3.1B), then Integration 3.2
- 🚧 Parallel Status: Track A complete, waiting on B

Begin implementation now.
```

---

### Step 3.1B: Point Defense Visual & Audio 🔴
**Parallel Track B**

**Prerequisites**: Phase 1-2 complete  
**Time Estimate**: 4-6 hours  
**Can run parallel with**: Step 3.1A

---

#### CLAUDE CODE PROMPT 3.1B

```
CONTEXT:
Phase 3 in progress. Track A is implementing PD logic (detection, engagement, interception). We need to create visual and audio feedback for PD system.

COORDINATION:
- Track A is creating PointDefenseSystem and PDTurret classes
- They will provide events we can subscribe to:
  * OnPDEngagement(PDTurret turret, Projectile target)
  * OnPDSuccess(PDTurret turret, Projectile destroyed)
  * OnPDFailure(PDTurret turret, Projectile missed)
- If these events aren't ready, create stub event handlers

OBJECTIVE:
Create visual effects, audio, and UI feedback for the Point Defense system to make PD engagements clear and satisfying.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/VFX/PDVisualEffects.cs
   - Component that subscribes to PD events
   - Spawns and manages VFX for PD actions
   - Methods:
     * OnPDFiring(PDTurret turret, Projectile target)
       - Spawn tracer line from turret to projectile
       - Brief muzzle flash on turret
       - Turret rotates to face target
     
     * OnProjectileDestroyed(Projectile proj, Vector3 position)
       - Spawn small explosion effect
       - Particle burst (sparks, debris)
       - Brief flash of light
       - Remove projectile GameObject
     
     * OnPDMiss(PDTurret turret, Projectile target)
       - Spawn tracer that misses slightly
       - No explosion
       - Turret still rotates

2. CREATE PD VFX Prefabs:
   
   **PD Tracer Effect**:
   - Line renderer from turret to target
   - Color: Bright cyan or yellow
   - Duration: 0.1 seconds
   - Fades out quickly
   
   **Projectile Destruction Effect**:
   - Particle system: Small explosion
   - Color: Orange/yellow for missiles, white for torpedoes
   - Size: Proportional to projectile size
   - Sound: Small explosion sound
   - Duration: 0.5 seconds, then auto-destroy
   
   **PD Turret Muzzle Flash**:
   - Small light flash at turret position
   - Particle burst (small, brief)
   - Color: Bright white/yellow
   - Duration: 0.05 seconds

3. CREATE: Assets/Scripts/Audio/PDAudioController.cs
   - Manages all PD audio
   - Audio clips needed:
     * PDFireSound: Short burst (machine gun-like)
     * PDHitSound: Small explosion
     * PDMissSound: Whizz/whoosh
     * PDActivateSound: For PD Override ability (turrets spin up)
   
   - Methods:
     * PlayPDFire(Vector3 position) - Play firing sound at turret
     * PlayPDHit(Vector3 position) - Play explosion at projectile
     * PlayPDMiss(Vector3 position) - Play miss sound
     * PlayPDOverride() - Play PD Override activation sound

4. CREATE: Assets/Scripts/UI/PDStatusDisplay.cs
   - UI component showing PD system status
   - Displays:
     * Operational turret count: "PD: 8/8"
     * Engagement counter (this turn): "Engaged: 12/16"
     * Recent interceptions log:
       ```
       [Forward-Port] → Missile → DESTROYED
       [Aft-Starboard] → Torpedo → MISS
       [Port] → Missile → DESTROYED
       ```
     * Visual: Green when operational, yellow when damaged, red when many destroyed
   
   - Updates in real-time during Simulation phase

5. CREATE: Assets/Scripts/VFX/PDTurretAnimator.cs
   - Component on each turret GameObject
   - Rotates turret to face threats
   - Properties:
     * TurretMesh: Transform (the visual turret)
     * RotationSpeed: float (degrees per second)
     * CurrentTarget: Vector3 (what turret is aiming at)
   
   - Methods:
     * void Update() - Smoothly rotate toward CurrentTarget
     * void SetTarget(Vector3 targetPosition) - Update aim point
     * void ResetToDefault() - Return to neutral position

6. CREATE: Turret Damage Visual States:
   
   **Operational Turret**:
   - Normal material (clean, metallic)
   - Glows slightly when firing
   - Rotates smoothly
   
   **Damaged Turret**:
   - Darkened, scorched material
   - Sparking particle system (continuous)
   - Smoke trail
   - No glow, no rotation
   - Marked non-operational

7. MODIFY: Assets/Scripts/Combat/Abilities/PDOverride.cs
   - Add visual feedback when activated:
     * All turrets glow yellow/orange
     * Turrets spin faster (visual only)
     * Audio: Power-up sound
     * UI: "PD OVERRIDE ACTIVE" indicator

8. CREATE: Assets/Prefabs/PointDefense/PDTurret.prefab
   - Complete turret prefab with:
     * Turret mesh (cylinder base + rotating top)
     * PDTurret component (logic from Track A)
     * PDTurretAnimator component (visual rotation)
     * Particle systems (muzzle flash, damage sparks)
     * Audio source
     * Collider (can be targeted by enemies)

TESTING REQUIREMENTS:

Manual Testing (No unit tests needed for VFX/Audio):

Test Scenarios:

SCENARIO A: Basic VFX
1. Enemy fires missiles at player
2. Observe PD engagement:
   - [ ] Turrets rotate toward threats
   - [ ] Tracer lines appear (turret → projectile)
   - [ ] Muzzle flashes on turrets
   - [ ] Audio: PD firing sounds
3. On successful intercept:
   - [ ] Explosion effect at projectile position
   - [ ] Audio: Small explosion sound
   - [ ] Projectile disappears
4. On miss:
   - [ ] Tracer appears but no explosion
   - [ ] Projectile continues

SCENARIO B: Multiple Engagements
1. Enemy fires 10 missiles
2. Multiple turrets engage simultaneously
3. Observe:
   - [ ] Multiple tracers visible
   - [ ] Audio doesn't clip or overlap badly
   - [ ] Explosions staggered (not all same frame)
   - [ ] UI shows engagement count increasing
   - [ ] No performance issues

SCENARIO C: PD Override Visual
1. Activate PD Override ability
2. Observe:
   - [ ] All turrets glow yellow/orange
   - [ ] Audio: Power-up sound plays
   - [ ] UI shows "PD OVERRIDE ACTIVE"
   - [ ] Turrets appear to spin faster (visual effect)
3. After ability expires:
   - [ ] Turrets return to normal glow
   - [ ] UI indicator disappears

SCENARIO D: Turret Damage Visual
1. Enemy targets and destroys 2 PD turrets
2. Observe damaged turrets:
   - [ ] Material changes (scorched, darkened)
   - [ ] Sparks particle system active
   - [ ] Smoke trail
   - [ ] No longer rotates
   - [ ] No longer fires (logic from Track A)
3. UI updates:
   - [ ] "PD: 6/8" (shows 2 destroyed)
   - [ ] Visual: Yellow or red warning color

SCENARIO E: Engagement Log
1. PD system engages multiple threats
2. Observe UI log:
   - [ ] Recent interceptions displayed
   - [ ] Shows: Turret name, projectile type, result
   - [ ] Log scrolls (max 5 entries)
   - [ ] Updates in real-time during Simulation

SCENARIO F: Audio Mix
1. Full combat scenario:
   - Player weapons firing
   - Enemy weapons firing
   - PD engaging (10+ turrets)
   - Explosions from hits
2. Verify audio:
   - [ ] All sounds audible
   - [ ] No severe clipping
   - [ ] PD sounds distinguishable from weapon sounds
   - [ ] Volume balanced

DELIVERABLES:
1. PDVisualEffects.cs VFX controller
2. PDAudioController.cs audio manager
3. PDStatusDisplay.cs UI component
4. PDTurretAnimator.cs turret rotation
5. VFX Prefabs:
   - PD Tracer effect
   - Projectile destruction explosion
   - Turret muzzle flash
   - Damaged turret effects (sparks, smoke)
6. Audio clips (placeholder or free assets):
   - PD firing sound
   - PD hit sound
   - PD Override activation sound
7. PDTurret.prefab (complete with all components)
8. Modified PDOverride.cs with visual feedback
9. Test scene demonstrating all VFX and audio

COORDINATION NOTE:
- Track A provides the logic (when to fire, success/fail)
- We provide the feedback (visual/audio)
- Integration: Subscribe to Track A's events

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 3.1B Complete - PD Visual & Audio
- 📁 New Files: PDVisualEffects.cs, PDAudioController.cs, PDStatusDisplay.cs, PDTurretAnimator.cs, prefabs
- 🔧 Modified: PDOverride.cs
- 🎮 Manual Test: All scenarios pass, VFX clear, audio balanced
- ⏭️ Next: Wait for Track A (3.1A), then Integration 3.2
- 🚧 Parallel Status: Track B complete, waiting on A

Begin implementation now.
```

---

### Step 3.2: Phase 3 Integration ⚫
**Sequential** - After both 3.1A and 3.1B complete

**Prerequisites**:
- Step 3.1A complete (PD Logic)
- Step 3.1B complete (PD Visual & Audio)

**Time Estimate**: 2-3 hours

---

#### CLAUDE CODE PROMPT 3.2

```
CONTEXT:
Phase 3 parallel development complete:
- Track A: PD Logic (detection, engagement, interception rolls)
- Track B: PD VFX & Audio (tracers, explosions, sounds, UI)

OBJECTIVE:
Integrate PD logic with visual/audio feedback to create a complete, polished Point Defense experience.

FILES TO INTEGRATE:
From Track A:
- PDTurret.cs, PointDefenseSystem.cs, PDTarget.cs
- Modified: Projectile.cs, PDOverride.cs, TurnManager.cs

From Track B:
- PDVisualEffects.cs, PDAudioController.cs, PDStatusDisplay.cs, PDTurretAnimator.cs
- VFX prefabs, Audio clips
- PDTurret.prefab

INTEGRATION TASKS:

1. Connect Events from Logic to VFX
   - PointDefenseSystem fires events when turret engages
   - PDVisualEffects subscribes to these events
   - Verify: Event firing → VFX spawning correctly

2. Sync Turret Rotation with Engagement
   - When PDTurret selects target, update PDTurretAnimator.SetTarget()
   - Turret mesh rotates to face threat before firing
   - Timing: Rotation should complete before tracer appears

3. Connect Audio Triggers
   - PD firing → Play PDFireSound
   - Successful intercept → Play PDHitSound + spawn explosion VFX
   - Miss → Play PDMissSound, no explosion

4. UI Real-Time Updates
   - PDStatusDisplay queries PointDefenseSystem each frame
   - Display operational turret count
   - Display engagement log from recent intercepts
   - Update colors based on system health

5. PD Override Visual Integration
   - When PDOverride ability activates:
     * Set all turrets' MaxTargetsPerTurn = 4 (logic)
     * Trigger glow effect on all turrets (visual)
     * Play activation sound (audio)
     * Show UI indicator (UI)

6. Turret Damage Integration
   - When enemy weapon hits turret:
     * PDTurret.TakeDamage() marks non-operational (logic)
     * Spawn damage VFX (sparks, smoke) (visual)
     * Update material to scorched (visual)
     * Update UI turret count (UI)

7. Performance Optimization
   - Pool VFX prefabs (don't instantiate/destroy each time)
   - Limit audio source count (max 10 PD sounds simultaneously)
   - Optimize particle systems (reduce particle count if needed)

8. Testing Full Combat Scenario
   - Player ship vs 2 enemy ships
   - All ships have weapons and PD
   - Multiple projectiles in flight
   - PD systems engaging on all ships
   - Verify: Logic + VFX + Audio all working together

TESTING REQUIREMENTS:

Integration Tests (Assets/Tests/PlayModeTests/Phase3IntegrationTests.cs):
1. Test_PDEventTriggers - PD engages, verify VFX event fires
2. Test_PDSuccessVFX - Successful intercept, verify explosion spawned
3. Test_PDMissNoVFX - Miss, verify no explosion
4. Test_TurretRotationSync - Engage threat, verify turret aims before firing
5. Test_PDAudioPlays - PD fires, verify audio plays
6. Test_PDUIUpdates - PD engages, verify UI log updates
7. Test_PDOverrideVisualActivation - Use ability, verify turret glow
8. Test_TurretDamageVisual - Destroy turret, verify sparks/smoke appear
9. Test_MultipleSimultaneousEngagements - 10 threats, verify all VFX appear
10. Test_PDPerformance - 20+ projectiles + PD, verify 60 FPS maintained

Manual Testing:

FULL INTEGRATION TEST:
1. Scene setup:
   - Player ship (8 PD turrets, all operational)
   - 2 Enemy ships (each with missiles/torpedoes)
   - Full UI (heat, abilities, weapons, PD status)

2. Combat sequence:
   - Turn 1:
     * Enemy A fires 5 missiles at player
     * Enemy B fires 2 torpedoes at player
     * Observe PD:
       - [ ] Turrets rotate toward threats
       - [ ] Tracers appear
       - [ ] Audio: Rapid PD firing sounds
       - [ ] Some projectiles destroyed (explosions)
       - [ ] Some projectiles get through (hit ship)
       - [ ] UI log shows intercepts
       - [ ] ~4 missiles destroyed, ~1 torpedo destroyed
   
   - Turn 2:
     * Player uses PD Override ability
     * [ ] All turrets glow yellow
     * [ ] Audio: Power-up sound
     * [ ] UI: "PD OVERRIDE ACTIVE"
     * Enemy fires 8 missiles
     * [ ] More missiles destroyed (each turret can engage 4 instead of 2)
   
   - Turn 3:
     * Enemy targets and destroys 3 of player's PD turrets
     * [ ] 3 turrets show damage (sparks, smoke)
     * [ ] UI shows "PD: 5/8"
     * [ ] Turrets stop rotating, don't engage
     * Enemy fires missiles
     * [ ] Fewer interceptions (only 5 turrets active)
     * [ ] More missiles hit player

3. Performance check:
   - 3 ships total
   - 20 projectiles in flight
   - 15 PD turrets active (across all ships)
   - Multiple VFX active (tracers, explosions, weapon fire)
   - [ ] Stable 60 FPS
   - [ ] No audio crackling
   - [ ] No visual glitches

4. Polish check:
   - [ ] PD engagement is satisfying (visual + audio feedback)
   - [ ] Easy to understand what's happening
   - [ ] Turret destruction is obvious
   - [ ] PD Override feels impactful
   - [ ] UI provides useful information

DELIVERABLES:
1. Fully integrated PD system (logic + VFX + audio + UI)
2. PD engagements feel polished and satisfying
3. All events connected correctly
4. Phase3IntegrationTests.cs with 10 tests
5. Full combat test scene
6. Performance profiling results
7. Documentation of integration points

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 3.2 Complete - Phase 3 Integration
- ✅ Phase 3 Complete - Point Defense System
- 🧪 Integration Tests: 10/10 passing
- 🧪 Total Tests: 96/96 passing
- 🎮 Manual Test: Full integration test passes
- ⚡ Performance: 60 FPS with PD active
- ⏭️ Next: Phase 4 - Enemy AI
- 🚧 Notes: [Any integration challenges, polish observations]
- 📊 Phase 3 Time: [Actual time for Phase 3]

Begin integration now.
```

---

#### MANUAL TESTING 3.2
**Time**: 30 minutes

**Full integration test:**

1. Set up complete combat scene as described
2. Run through all 3 turn combat sequence
3. Verify all PD functionality works
4. Run performance check

**Checklist**:
- [ ] PD logic and VFX synchronized
- [ ] Turrets rotate to face threats
- [ ] Audio plays correctly
- [ ] UI updates in real-time
- [ ] PD Override ability works (logic + visual)
- [ ] Turret damage shows visually
- [ ] All 10 integration tests pass
- [ ] 60 FPS maintained
- [ ] No console errors

**If all pass, Phase 3 is COMPLETE!**

---

## Phase 3 Summary

### What Was Built
- ✅ PD Logic: Automated threat detection and interception
- ✅ PD Visual & Audio: Complete feedback system
- ✅ Integration: Logic, VFX, and audio working together
- ✅ UI: Status display showing turret health and engagements

### Key Files Created
**PD Logic (7 files)**:
- PDTurret.cs
- PDTarget.cs
- PointDefenseSystem.cs
- PDDamageController.cs
- PointDefenseTests.cs
- Modified: Projectile.cs, PDOverride.cs, TurnManager.cs

**PD Visual & Audio (8 files)**:
- PDVisualEffects.cs
- PDAudioController.cs
- PDStatusDisplay.cs
- PDTurretAnimator.cs
- VFX prefabs (tracers, explosions, muzzle flashes)
- Audio clips
- PDTurret.prefab

**Integration (1 file)**:
- Phase3IntegrationTests.cs

### Testing Summary
- Unit Tests: 12 (PD logic)
- Integration Tests: 10
- Manual Test Scenarios: 6
- **Total Tests: 22 new (96 cumulative)**

### Time Investment
- Parallel Development: 1-2 weeks
- Sequential Development: 2-3 weeks
- **Time Saved: ~1 week**

---

## Next Steps

With Phase 3 complete, you're ready to move to **Part 4** of this guide which covers Phase 4 (Enemy AI), Phase 5 (Polish & VFX), and Phase 6 (Balance & MVP Completion).

Phase 4 will implement:
- AI controller using same systems as player
- 3 AI difficulty levels (Basic, Intermediate, Advanced)
- Enemy spawning system
- Victory/defeat conditions

**Continue to Part 4 of the Implementation Guide**

---

**End of Part 3**