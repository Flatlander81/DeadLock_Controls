# Hephaestus Implementation Guide - Part 2
## Weapon Systems (Phase 2)

---

## Document Overview

This is Part 2 of 4 in the complete Hephaestus implementation guide:
- Part 1: Overview + Phase 0 (Foundation) + Phase 1 (Core Combat)
- **Part 2**: Phase 2 (Weapon Systems) ← You are here
- Part 3: Phase 3 (Point Defense) + Phase 4 (Enemy AI)
- Part 4: Phase 5 (Polish & VFX) + Phase 6 (Balance & MVP Completion)

---

## Phase 2: Weapon Systems (Weeks 3-5)

### Overview
Phase 2 implements the complete weapon combat system with 4 weapon types, projectile physics, and targeting UI. This phase has the most parallel development opportunities with 3 independent tracks.

**Parallel Development**: 3 agents can work simultaneously
- 🔵 Track A: Weapon Base + Rail Gun + Newtonian Cannon
- 🔴 Track B: Projectile System (ballistic + homing)
- 🟢 Track C: Targeting UI System

**Timeline**: 2-3 weeks with parallel development (4-5 weeks sequential)

---

### Pre-Phase Manual Setup

#### MANUAL TASK 2.0: Create Weapon Structure
**Time**: 15 minutes

**Steps**:

1. Create folders:
```
Assets/Scripts/Combat/Weapons/
Assets/Scripts/Combat/Projectiles/
Assets/Scripts/Combat/Targeting/
Assets/Prefabs/Weapons/
Assets/Prefabs/Projectiles/
```

2. Create weapon hardpoint templates:
   - Duplicate player ship prefab for testing
   - Add child empty GameObjects named:
     * "RailGun_Port_Hardpoint"
     * "RailGun_Starboard_Hardpoint"
     * "Cannon_Forward_Hardpoint"
     * "Torpedo_Forward_Hardpoint"
     * "Missile_Dorsal_Hardpoint"
     * "Missile_Ventral_Hardpoint"
   - Position appropriately on ship mesh

3. Update IMPLEMENTATION_STATUS.md:
```markdown
### Current Phase: 2.1 - Weapon Systems
### Ready for parallel development (3 tracks)
```

**Status Update**: Document weapon structure created

---

### Phase 2 Architecture Overview

**What We're Building**:
```
Player Action: Select Enemy → Assign Weapon Groups → Fire
         ↓                        ↓                  ↓
  TargetingUI            WeaponManager          WeaponSystem
                                ↓                     ↓
                         (During Simulation)   Fire weapons
                                                      ↓
                                              ProjectileManager
                                                      ↓
                                           Spawn projectiles
                                                      ↓
                                            Projectile physics
                                                      ↓
                                              Hit detection
                                                      ↓
                                             Damage applied
```

**3 Parallel Tracks**:
- Track A builds weapon firing logic
- Track B builds projectile physics
- Track C builds player UI for targeting
- All integrate in Step 2.2

---

### Step 2.1A: Weapon Base Architecture 🔵
**Parallel Track A**

**Prerequisites**: Phase 1 complete, weapon structure created  
**Time Estimate**: 6-8 hours  
**Can run parallel with**: Steps 2.1B and 2.1C

---

#### CLAUDE CODE PROMPT 2.1A

```
CONTEXT:
Phase 1 complete (Heat + Abilities working). Now implementing weapon systems.

This is PARALLEL TRACK A of Phase 2. Track B (Projectiles) and Track C (Targeting UI) are running simultaneously.

COORDINATION:
- Track B is creating projectile classes - we define the spawning interface
- Track C is creating targeting UI - we define the weapon group interface
- We'll integrate all three in Step 2.2

Existing files:
- Assets/Scripts/Combat/HeatManager.cs
- Assets/Scripts/Movement/Ship.cs
- Assets/Scripts/Movement/TurnManager.cs

OBJECTIVE:
Create weapon system base architecture and implement 2 basic weapon types (Rail Gun and Newtonian Cannon).

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Combat/Weapons/WeaponSystem.cs (abstract base)
   - MonoBehaviour attached to weapon hardpoint GameObjects
   - Properties:
     * WeaponName: string
     * Damage: float
     * HeatCost: int
     * FiringArc: float (degrees: 360 = turret, 180 = forward, 30 = narrow)
     * MaxRange: float (units)
     * MaxCooldown: int (turns)
     * CurrentCooldown: int
     * SpinUpTime: float (seconds before firing during Simulation)
     * AmmoCapacity: int (0 = infinite)
     * CurrentAmmo: int
     * AssignedGroup: int (0-4, where 0 = unassigned)
     * AssignedTarget: Ship (set by targeting system)
   
   - Protected:
     * ownerShip: Ship
     * hardpointTransform: Transform (firing position)
   
   - Methods:
     * virtual void Initialize(Ship owner)
     * bool IsInArc(Vector3 targetPosition) - Check if target within firing arc
     * bool IsInRange(Vector3 targetPosition) - Check if target within range
     * bool CanFire() - Check: cooldown ready, has ammo, has target, target in arc + range
     * void AssignToGroup(int groupNumber) - Set AssignedGroup (0-4)
     * void SetTarget(Ship target) - Set AssignedTarget
     * IEnumerator FireWithSpinUp() - Coroutine: wait SpinUpTime, then Fire()
     * abstract void Fire() - Execute firing (spawn projectile or instant hit)
     * void StartCooldown() - Set CurrentCooldown to MaxCooldown
     * void TickCooldown() - Decrease CurrentCooldown
     * abstract ProjectileSpawnInfo GetProjectileInfo() - Return info for projectile spawning
   
   - ProjectileSpawnInfo struct:
     ```csharp
     public struct ProjectileSpawnInfo
     {
         public enum ProjectileType { InstantHit, Ballistic, Homing }
         public ProjectileType Type;
         public Vector3 SpawnPosition;
         public Quaternion SpawnRotation;
         public Vector3 TargetPosition;
         public Ship TargetShip;
         public float Damage;
         public float Speed;
         public Ship OwnerShip;
     }
     ```

2. CREATE: Assets/Scripts/Combat/Weapons/RailGun.cs
   - Inherits WeaponSystem
   - Settings:
     * Damage = 20
     * HeatCost = 15
     * FiringArc = 360 (turret)
     * MaxRange = 30
     * MaxCooldown = 0
     * SpinUpTime = 0.2f
     * AmmoCapacity = 0 (infinite)
   
   - Fire() implementation:
     * Instant-hit: Calculate hit immediately
     * Apply damage to target directly (no projectile travel)
     * Spawn visual tracer effect via ProjectileManager
     * Apply heat to owner ship
     * Start cooldown (none for rail gun)
   
   - GetProjectileInfo():
     * Return ProjectileType.InstantHit

3. CREATE: Assets/Scripts/Combat/Weapons/NewtonianCannon.cs
   - Inherits WeaponSystem
   - Settings:
     * Damage = 40
     * HeatCost = 30
     * FiringArc = 180 (forward hemisphere)
     * MaxRange = 20
     * MaxCooldown = 0
     * SpinUpTime = 0.5f
     * AmmoCapacity = 0 (infinite)
   
   - Fire() implementation:
     * Spawn ballistic projectile
     * Projectile travels at 2 units/second
     * Call ProjectileManager.SpawnBallisticProjectile(GetProjectileInfo())
     * Apply heat to owner ship
   
   - GetProjectileInfo():
     * Return ProjectileType.Ballistic
     * Calculate target position with lead: target.CurrentPosition + (target.Velocity * time_to_impact)

4. CREATE: Assets/Scripts/Combat/Weapons/WeaponManager.cs
   - Component on Ship GameObject
   - Discovers and manages all WeaponSystem components
   - Properties:
     * weapons: List<WeaponSystem>
     * weaponGroups: Dictionary<int, List<WeaponSystem>> (group 1-4 → weapons)
   
   - Methods:
     * void Start() - Find all WeaponSystem components on ship and children
     * void AssignWeaponToGroup(WeaponSystem weapon, int group)
     * List<WeaponSystem> GetWeaponsInGroup(int group)
     * void SetGroupTarget(int group, Ship target)
     * IEnumerator FireGroup(int group) - Fire all weapons in group (with spin-ups)
     * IEnumerator FireAlphaStrike(Ship target) - Fire all assigned weapons at target
     * void TickAllCooldowns() - Tick all weapon cooldowns
     * int CalculateGroupHeatCost(int group) - Sum heat of weapons in group
     * bool IsGroupReady(int group) - Check if all weapons in group can fire

5. CREATE: Assets/Scripts/Combat/ProjectileManager.cs (STUB for Track B)
   - Static class or singleton
   - Methods that Track B will implement:
     ```csharp
     public static void SpawnBallisticProjectile(ProjectileSpawnInfo info)
     {
         Debug.Log($"[STUB] Spawning ballistic projectile: {info.Damage} damage");
     }
     
     public static void SpawnHomingProjectile(ProjectileSpawnInfo info)
     {
         Debug.Log($"[STUB] Spawning homing projectile: {info.Damage} damage");
     }
     
     public static void SpawnInstantHitEffect(Vector3 start, Vector3 end, float damage)
     {
         Debug.Log($"[STUB] Instant hit from {start} to {end}: {damage} damage");
     }
     ```
   - Track B will replace with real implementation

6. MODIFY: Assets/Scripts/Movement/Ship.cs
   - Add properties:
     * WeaponManager: WeaponManager (component reference)
     * WeaponDamageMultiplier: float (for Overcharge ability, default 1.0)
     * WeaponHeatMultiplier: float (for Overcharge ability, default 1.0)

7. MODIFY: Assets/Scripts/Movement/TurnManager.cs
   - In Simulation phase execution order:
     * After abilities execute, before cooling:
     * Call ship.WeaponManager.FireQueuedWeapons() on all ships

8. CREATE INTERFACE CONTRACT: Assets/Scripts/Combat/ITargetingSystem.cs
   ```csharp
   // Interface for Track C (Targeting UI) to implement
   public interface ITargetingSystem
   {
       void SelectTarget(Ship target);
       void AssignGroupToTarget(int group, Ship target);
       void AlphaStrike(Ship target);
       Ship GetCurrentTarget();
   }
   ```

TESTING REQUIREMENTS:

Unit Tests (Assets/Tests/PlayModeTests/WeaponSystemTests.cs):
1. Test_WeaponInitialization - Create weapon, verify properties set
2. Test_ArcCheck - Test IsInArc() with various positions
3. Test_RangeCheck - Test IsInRange() with various distances
4. Test_CanFire - Verify conditions checked correctly
5. Test_GroupAssignment - Assign weapon to group, verify stored
6. Test_RailGunInstantHit - Fire rail gun, verify instant damage
7. Test_CannonBallisticSpawn - Fire cannon, verify projectile spawn called
8. Test_WeaponCooldown - Fire weapon, tick cooldown, verify decreases
9. Test_WeaponHeatCost - Fire weapon, verify heat added to ship
10. Test_WeaponGroupHeatCalculation - Multiple weapons in group, verify total heat
11. Test_SpinUpDelay - Fire weapon, verify Fire() called after SpinUpTime
12. Test_OverchargeMultiplier - Set multipliers, fire weapon, verify modified damage/heat

Manual Testing Instructions:
1. Create test scene with Ship prefab
2. Attach WeaponManager to ship
3. Attach RailGun and NewtonianCannon components to hardpoint children
4. Create dummy target ship at various positions
5. Add test UI:
   - Buttons: "Assign Rail Gun to Group 1", "Assign Cannon to Group 2"
   - Button: "Fire Group 1", "Fire Group 2", "Alpha Strike"
   - Display: Show weapon status (arc, range, cooldown, ammo)
6. Play scene and verify:
   - Weapons initialize correctly
   - Arc checking works (move target around)
   - Range checking works (move target closer/farther)
   - Rail gun fires instantly (stub logs message)
   - Cannon calls spawn projectile (stub logs message)
   - Heat accumulates when weapons fire
   - Cooldowns tick down
   - Weapon groups track assignments
   - Spin-up delays work

DELIVERABLES:
1. WeaponSystem.cs abstract base class
2. RailGun.cs implementation
3. NewtonianCannon.cs implementation
4. WeaponManager.cs component
5. ProjectileManager.cs stub (for Track B)
6. ITargetingSystem.cs interface (for Track C)
7. Modified Ship.cs and TurnManager.cs
8. WeaponSystemTests.cs with 12 unit tests
9. Test scene with working weapons (stub projectiles)

COORDINATION NOTES:
- ProjectileManager is stubbed - Track B will implement
- ITargetingSystem is defined - Track C will implement
- When all tracks complete, we'll integrate in Step 2.2

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 2.1A Complete - Weapon Base Architecture
- 📁 New Files: WeaponSystem.cs, RailGun.cs, NewtonianCannon.cs, WeaponManager.cs, ProjectileManager.cs (stub), ITargetingSystem.cs
- 🔧 Modified: Ship.cs, TurnManager.cs
- 🧪 Unit Tests: 12/12 passing
- 🎮 Manual Test: Weapons fire (with stub projectiles), groups work, heat applies
- ⏭️ Next: Wait for Track B (2.1B) and Track C (2.1C), then Integration 2.2
- 🚧 Parallel Status: Track A complete, waiting on B and C

Begin implementation now.
```

---

### Step 2.1B: Projectile System 🔴
**Parallel Track B**

**Prerequisites**: Phase 1 complete  
**Time Estimate**: 6-8 hours  
**Can run parallel with**: Steps 2.1A and 2.1C

---

#### CLAUDE CODE PROMPT 2.1B

```
CONTEXT:
Phase 1 complete. Phase 2 parallel development in progress.

This is PARALLEL TRACK B of Phase 2. Track A (Weapons) and Track C (Targeting UI) are running simultaneously.

COORDINATION:
- Track A is creating weapon base classes and defining ProjectileSpawnInfo struct
- We implement ProjectileManager that Track A will call
- Track C is creating targeting UI (independent of our work)

If Track A hasn't shared ProjectileSpawnInfo yet, recreate it:
```csharp
public struct ProjectileSpawnInfo
{
    public enum ProjectileType { InstantHit, Ballistic, Homing }
    public ProjectileType Type;
    public Vector3 SpawnPosition;
    public Quaternion SpawnRotation;
    public Vector3 TargetPosition;
    public Ship TargetShip;
    public float Damage;
    public float Speed;
    public Ship OwnerShip;
}
```

OBJECTIVE:
Create projectile physics system that handles ballistic and homing projectiles, collision detection, and damage application.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Combat/Projectiles/Projectile.cs (abstract base)
   - MonoBehaviour
   - Properties:
     * Damage: float
     * Speed: float
     * Lifetime: float (max seconds before auto-destroy, default 10)
     * OwnerShip: Ship (who fired this)
     * CurrentAge: float (time since spawn)
   
   - Methods:
     * virtual void Initialize(ProjectileSpawnInfo info)
     * abstract void UpdateMovement() - Called each frame, move projectile
     * void CheckCollisions() - Raycast or sphere cast for hits
     * void OnHit(Ship target) - Apply damage, destroy projectile
     * void OnLifetimeExpired() - Destroy projectile
     * virtual void OnDestroyed() - Cleanup, VFX

2. CREATE: Assets/Scripts/Combat/Projectiles/BallisticProjectile.cs
   - Inherits Projectile
   - Ballistic trajectory (straight line, no homing)
   - Properties:
     * initialVelocity: Vector3
     * currentVelocity: Vector3
   
   - UpdateMovement():
     * Move in straight line at Speed
     * No course correction
     * Check for collisions each frame
   
   - Used by: Newtonian Cannon

3. CREATE: Assets/Scripts/Combat/Projectiles/HomingProjectile.cs
   - Inherits Projectile
   - Seeks target, adjusts course
   - Properties:
     * TargetShip: Ship
     * TurnRate: float (degrees per second)
   
   - UpdateMovement():
     * Calculate direction to target current position
     * Rotate toward target at TurnRate
     * Move forward at Speed
     * Check for collisions each frame
     * If target destroyed, continue straight (ballistic)
   
   - Used by: Missiles and Torpedoes

4. CREATE: Assets/Scripts/Combat/ProjectileManager.cs (REAL IMPLEMENTATION)
   - Singleton or static class
   - Manages all active projectiles
   - Pools projectiles for performance
   
   - Properties:
     * activeProjectiles: List<Projectile>
     * projectilePools: Dictionary<ProjectileType, Queue<Projectile>>
   
   - Methods:
     * static void SpawnBallisticProjectile(ProjectileSpawnInfo info)
       - Get pooled projectile or instantiate new
       - Initialize with info
       - Add to activeProjectiles
     
     * static void SpawnHomingProjectile(ProjectileSpawnInfo info)
       - Get pooled projectile or instantiate new
       - Initialize with info, set target
       - Add to activeProjectiles
     
     * static void SpawnInstantHitEffect(Vector3 start, Vector3 end, float damage)
       - Spawn visual tracer (line renderer)
       - Apply damage immediately
       - No actual projectile object
     
     * void Update() - Call UpdateMovement() on all active projectiles
     
     * void ReturnToPool(Projectile proj) - Deactivate and pool for reuse
     
     * void ClearAllProjectiles() - Destroy all (end of combat)

5. CREATE: Assets/Prefabs/Projectiles/BallisticProjectile.prefab
   - Sphere mesh (or simple capsule)
   - Glowing material
   - Trail renderer component
   - Collider (sphere or capsule)
   - BallisticProjectile component

6. CREATE: Assets/Prefabs/Projectiles/HomingProjectile.prefab
   - Similar to ballistic but different color
   - HomingProjectile component
   - Particle system for thruster effect

7. CREATE: Assets/Scripts/Combat/Projectiles/InstantHitEffect.cs
   - Simple visual for instant-hit weapons (rail guns)
   - Line renderer from start to end
   - Fades out over 0.1 seconds
   - Auto-destroys after fade

8. Collision & Damage System:
   - Projectiles use sphere cast to detect ships
   - Check if hit ship is enemy (not owner)
   - Call ship.TakeDamage(Damage)
   - Destroy projectile on hit
   - Play impact VFX (placeholder)

TESTING REQUIREMENTS:

Unit Tests (Assets/Tests/PlayModeTests/ProjectileSystemTests.cs):
1. Test_BallisticSpawn - Spawn ballistic, verify created and moving
2. Test_BallisticTrajectory - Spawn ballistic, verify travels straight
3. Test_BallisticCollision - Spawn at target, verify collision detected
4. Test_BallisticDamageApplication - Projectile hits ship, verify damage applied
5. Test_BallisticLifetimeExpiry - Wait lifetime, verify auto-destroyed
6. Test_HomingSpawn - Spawn homing with target, verify created
7. Test_HomingSeeks - Spawn homing, move target, verify projectile turns
8. Test_HomingHitMovingTarget - Spawn homing at moving target, verify hit
9. Test_HomingTargetDestroyed - Destroy target mid-flight, verify continues ballistic
10. Test_InstantHitEffect - Spawn instant hit, verify tracer created and fades
11. Test_ProjectilePooling - Spawn 10, destroy 10, spawn 10 more, verify reused
12. Test_NoFriendlyFire - Spawn projectile, verify doesn't hit owner ship

Manual Testing Instructions:
1. Create test scene with 2 ships (player + enemy)
2. Add test UI:
   - Button: "Spawn Ballistic at Enemy"
   - Button: "Spawn Homing at Enemy"
   - Button: "Spawn Instant Hit"
   - Slider: Adjust spawn distance
3. Play scene and verify:
   - Ballistic projectiles spawn and travel straight
   - Ballistic projectiles hit target and apply damage
   - Ballistic projectiles auto-destroy after lifetime
   - Homing projectiles spawn and seek target
   - Homing projectiles adjust course when target moves
   - Homing projectiles hit moving targets
   - If target destroyed, homing continues straight
   - Instant hit creates visual tracer
   - No friendly fire
   - Multiple projectiles can be in flight simultaneously
   - Performance good with 20+ projectiles

DELIVERABLES:
1. Projectile.cs abstract base
2. BallisticProjectile.cs implementation
3. HomingProjectile.cs implementation
4. ProjectileManager.cs (real implementation, replacing stub)
5. InstantHitEffect.cs
6. Projectile prefabs (Ballistic, Homing)
7. ProjectileSystemTests.cs with 12 unit tests
8. Test scene demonstrating projectile physics

COORDINATION NOTES:
- ProjectileSpawnInfo struct must match Track A's definition
- Track A will call our ProjectileManager methods
- We're independent of Track C (Targeting UI)

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 2.1B Complete - Projectile System
- 📁 New Files: Projectile.cs, BallisticProjectile.cs, HomingProjectile.cs, ProjectileManager.cs, InstantHitEffect.cs, prefabs
- 🧪 Unit Tests: 12/12 passing
- 🎮 Manual Test: Projectiles spawn, travel, home, collide, damage correctly
- ⏭️ Next: Wait for Track A (2.1A) and Track C (2.1C), then Integration 2.2
- 🚧 Parallel Status: Track B complete, waiting on A and C

Begin implementation now.
```

---

### Step 2.1C: Targeting UI System 🟢
**Parallel Track C**

**Prerequisites**: Phase 1 complete  
**Time Estimate**: 6-8 hours  
**Can run parallel with**: Steps 2.1A and 2.1B

---

#### CLAUDE CODE PROMPT 2.1C

```
CONTEXT:
Phase 1 complete. Phase 2 parallel development in progress.

This is PARALLEL TRACK C of Phase 2. Track A (Weapons) and Track B (Projectiles) are running simultaneously.

COORDINATION:
- Track A is creating weapon systems - we create UI for assigning and firing them
- Track B is creating projectiles (independent)
- We'll integrate all in Step 2.2

If Track A hasn't shared WeaponManager interface yet, use this:
```csharp
public interface IWeaponManager
{
    void AssignWeaponToGroup(WeaponSystem weapon, int group);
    List<WeaponSystem> GetWeaponsInGroup(int group);
    void SetGroupTarget(int group, Ship target);
    void FireGroup(int group);
    void FireAlphaStrike(Ship target);
    int CalculateGroupHeatCost(int group);
    bool IsGroupReady(int group);
}
```

OBJECTIVE:
Create targeting UI that allows players to select targets, assign weapons to groups, and fire weapon groups.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Combat/Targeting/TargetingController.cs
   - Component on main camera or game manager
   - Handles target selection and weapon group firing
   - Properties:
     * currentTarget: Ship (currently selected enemy)
     * playerShip: Ship (reference to player's ship)
     * selectionIndicator: GameObject (visual indicator)
   
   - Methods:
     * void Update() - Check for mouse clicks on ships
     * void SelectTarget(Ship target) - Set currentTarget, show indicator
     * void DeselectTarget() - Clear currentTarget, hide indicator
     * void AssignGroupToCurrentTarget(int groupNumber)
     * void AlphaStrikeCurrentTarget()
     * Ship GetTargetUnderMouse() - Raycast to find ship

2. CREATE: Assets/Scripts/UI/WeaponConfigPanel.cs
   - UI panel shown when player ship (Hephaestus) selected
   - Displays list of all weapons on ship
   - Each weapon shows:
     * Weapon name
     * Group dropdown (Unassigned, 1, 2, 3, 4)
     * Heat cost
     * Firing arc
     * Cooldown status
     * Ammo count (if applicable)
   
   - Functionality:
     * Click dropdown to cycle weapon group assignment
     * Visual feedback when assignment changes
     * Grayed out if weapon on cooldown
   
   - Layout: Left side of screen

3. CREATE: Assets/Scripts/UI/WeaponGroupPanel.cs
   - UI panel shown when enemy ship selected
   - Displays 4 weapon group buttons + Alpha Strike button
   - Each group button shows:
     * Group number and color (1=Blue, 2=Red, 3=Green, 4=Yellow)
     * Weapons in that group
     * Total heat cost
     * "OUT OF ARC" warning if any weapon can't fire
     * "ON COOLDOWN" if any weapon not ready
   
   - Alpha Strike button shows:
     * "ALPHA STRIKE" text
     * All assigned weapons
     * Total heat cost
     * Warning if exceeds safe threshold
   
   - Functionality:
     * Click group button OR press number key (1-4)
     * Click Alpha Strike OR press Space/F
     * Visual confirmation when group queued
     * Hotkey hints visible
   
   - Layout: Right side of screen

4. CREATE: Assets/Scripts/UI/TargetingLineRenderer.cs
   - Visual feedback showing which groups target which enemies
   - Colored lines from player ship to targets
   - Colors match weapon groups:
     * Group 1: Blue, Group 2: Red, Group 3: Green, Group 4: Yellow
   - Lines appear during Command phase
   - Lines disappear during Simulation phase

5. CREATE: Assets/Scripts/UI/SelectionIndicator.cs
   - Visual indicator on selected ship
   - Rotating ring or highlight effect
   - Color: Cyan for enemy, Green for friendly
   - Follows ship if it moves

6. MODIFY: Assets/Scripts/Movement/MovementController.cs
   - Add target selection handling:
     * Left-click enemy: Select as target, show WeaponGroupPanel
     * Left-click player: Select Hephaestus, show WeaponConfigPanel
     * Left-click empty: Deselect
   
   - Add weapon group hotkeys:
     * Number keys 1-4: Fire that group at current target
     * Space or F: Alpha Strike at current target
   
   - Mode management:
     * Weapon targeting doesn't interfere with movement planning
     * Can plan movement and assign weapon targets in same Command phase

7. CREATE: Assets/Scripts/UI/UIManager.cs
   - Manages which UI panels visible based on selection state
   - Selection states:
     * Nothing selected: Minimal HUD
     * Enemy selected: Show WeaponGroupPanel
     * Player selected: Show WeaponConfigPanel
   
   - Methods:
     * void UpdateUI(SelectionState state)
     * void ShowWeaponConfigPanel()
     * void ShowWeaponGroupPanel()
     * void HideAllPanels()

TESTING REQUIREMENTS:

Unit Tests (Assets/Tests/PlayModeTests/TargetingSystemTests.cs):
1. Test_TargetSelection - Click enemy, verify selected
2. Test_TargetDeselection - Click empty space, verify deselected
3. Test_WeaponGroupAssignment - Assign weapon to group via UI, verify stored
4. Test_GroupFiring - Press number key, verify group fires
5. Test_AlphaStrike - Press Space, verify all weapons fire
6. Test_TargetingLine - Assign group, verify colored line appears
7. Test_MultiTargeting - Assign Group 1 to Enemy A, Group 2 to Enemy B
8. Test_UIStateTransitions - Select enemy/player/nothing, verify correct panels
9. Test_OutOfArcWarning - Target behind ship, verify warning
10. Test_CooldownWarning - Fire weapon, try again, verify warning
11. Test_HeatCostDisplay - Group with multiple weapons, verify total
12. Test_SelectionIndicator - Select ship, verify indicator appears

Manual Testing Instructions:
1. Create test scene with player ship + 2 enemy ships
2. Ensure player has multiple weapons (RailGun, Cannon)
3. Add all UI panels to Canvas:
   - WeaponConfigPanel (left)
   - WeaponGroupPanel (right)
   - Heat bar (top, from Phase 1)
   - Ability bar (bottom, from Phase 1)
4. Play scene and verify:
   
   SCENARIO A: Weapon Configuration
   - Left-click player ship
   - Verify WeaponConfigPanel appears
   - Click weapon's group dropdown, cycle through
   - Verify weapon assignments change
   
   SCENARIO B: Single Target Attack
   - Left-click enemy A
   - Verify selection indicator appears
   - Verify WeaponGroupPanel appears
   - Press "1" to fire Group 1
   - Verify colored line from player to enemy A
   - Verify heat cost preview
   - End turn (Simulation)
   - Verify weapons fire at enemy A
   
   SCENARIO C: Multi-Target Attack
   - Assign Rail Guns to Group 1, Cannon to Group 2
   - Left-click enemy A, press "1"
   - Left-click enemy B, press "2"
   - Verify two colored lines
   - End turn
   - Verify weapons fire at correct targets
   
   SCENARIO D: Alpha Strike
   - Left-click enemy
   - Press Space
   - Verify all weapon groups show targeting lines
   - Verify total heat cost shown
   - End turn
   - Verify all weapons fire
   
   SCENARIO E: Arc Validation
   - Position enemy behind player
   - Select enemy, try to fire forward-only weapon
   - Verify "OUT OF ARC" warning

DELIVERABLES:
1. TargetingController.cs
2. WeaponConfigPanel.cs with weapon list UI
3. WeaponGroupPanel.cs with group buttons
4. TargetingLineRenderer.cs
5. SelectionIndicator.cs
6. UIManager.cs
7. Modified MovementController.cs
8. TargetingSystemTests.cs with 12 unit tests
9. UI prefabs for all panels
10. Test scene demonstrating full targeting workflow

COORDINATION NOTES:
- We use IWeaponManager interface that Track A will implement
- Independent of Track B (Projectiles)
- Our UI triggers weapon firing, Track A's weapons spawn projectiles

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 2.1C Complete - Targeting UI System
- 📁 New Files: TargetingController.cs, WeaponConfigPanel.cs, WeaponGroupPanel.cs, TargetingLineRenderer.cs, SelectionIndicator.cs, UIManager.cs
- 🔧 Modified: MovementController.cs
- 🧪 Unit Tests: 12/12 passing
- 🎮 Manual Test: All scenarios pass, UI responsive, targeting intuitive
- ⏭️ Next: Wait for Track A (2.1A) and Track B (2.1B), then Integration 2.2
- 🚧 Parallel Status: Track C complete, waiting on A and B

Begin implementation now.
```

---

## Integration Checkpoint

**After Steps 2.1A, 2.1B, and 2.1C all complete, proceed to integration**

---

**End of Part 2 (partial) - I'm ready to create the next 2 documents (Part 3 and Part 4).**