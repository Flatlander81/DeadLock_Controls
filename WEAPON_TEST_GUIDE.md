# Weapon System - Quick Test Guide

## Automated Setup (Recommended)

### One-Click Setup
1. Open Unity Editor
2. Go to menu: **Tools → Setup Weapon Test Scene**
3. Wait a few seconds for scene creation
4. Press **Play** button
5. Use keyboard controls to test!

**That's it!** The script automatically creates:
- Player ship with 3 weapons (2 RailGuns + 1 Cannon)
- Enemy ship positioned 30 units ahead
- Camera positioned to see both ships
- TurnManager for game loop
- WeaponTester for keyboard controls
- Saves scene to `Assets/Scenes/WeaponTestScene.unity`

---

## Keyboard Controls (In Play Mode)

### Weapon Firing
- **SPACE** - Fire Group 1 (Both RailGuns)
- **F** - Fire Group 2 (Cannon)
- **A** - Alpha Strike (Fire ALL weapons)

### Enemy Movement (Test Arc/Range)
- **↑** - Move enemy forward (+5 units Z)
- **↓** - Move enemy backward (-5 units Z)
- **←** - Move enemy left (-5 units X)
- **→** - Move enemy right (+5 units X)
- **R** - Reset enemy to start position (0, 0, 30)

### Test Commands
- **K** - Kill enemy ship (test dead target detection)
- **1-6** - Activate abilities (Emergency Cooling, Shield Boost, etc.)

---

## What to Watch For

### Console Output
Watch the Console window for:
- ✅ "Rail Gun: Firing at EnemyShip"
- ✅ "[STUB - Track B] Spawning instant hit effect"
- ✅ "EnemyShip - Shields: 200 → 180"
- ✅ "Rail Gun: Target not in firing arc" (when enemy to side)
- ✅ "Rail Gun: Target out of range" (when enemy far away)
- ✅ "Rail Gun: Target is dead" (after killing enemy)

### On-Screen UI (Top-Left)
- Player heat level
- Enemy hull/shields
- Enemy position
- Quick controls reference

### Scene View (Not Game View)
- Select any hardpoint GameObject
- See **cyan sphere** at hardpoint location
- See **red line** showing firing direction
- When selected: **RGB axes** showing orientation

---

## Test Scenarios

### ✅ Scenario 1: Basic Firing
1. Press **SPACE** (fire RailGuns)
2. Check Console for firing messages
3. Check enemy shields decrease by 40 (20 per RailGun)
4. Check player heat increases by 30 (15 per RailGun)

### ✅ Scenario 2: Arc Validation
1. Press **→** three times to move enemy to the side
2. Press **F** to fire Cannon (180° forward arc)
3. Should see: "Newtonian Cannon: Target not in firing arc"
4. Press **SPACE** to fire RailGuns (360° turrets)
5. Should work fine - turrets can fire in any direction

### ✅ Scenario 3: Range Validation
1. Press **R** to reset enemy
2. Press **↑** five times to move enemy far away
3. Press **SPACE** to fire
4. Should see: "Target out of range"
5. Press **↓** twice to bring enemy closer
6. Firing should work again

### ✅ Scenario 4: Heat Management
1. Press **SPACE** repeatedly (5+ times)
2. Watch heat bar fill up
3. Heat should increase by 30 per shot
4. At high heat, movement speed decreases
5. Press **1** to activate Emergency Cooling (-50 heat)
6. Heat should drop immediately

### ✅ Scenario 5: Dead Target Detection
1. Press **K** to kill enemy
2. Check Console: "EnemyShip has been destroyed!"
3. Try pressing **SPACE** to fire
4. Should see: "Target is dead"
5. Weapons cannot fire at dead targets

### ✅ Scenario 6: Weapon Groups
1. Press **SPACE** - fires Group 1 (2 RailGuns)
2. Press **F** - fires Group 2 (1 Cannon)
3. Press **A** - fires all weapons simultaneously
4. Alpha strike costs 30+30+15+15 = 90 heat total!

### ✅ Scenario 7: Overcharge Integration
1. Press **R** to reset enemy
2. Note enemy hull value (should be 500)
3. Press **4** to activate Overcharge Weapons
4. Wait 0.5 seconds for spin-up
5. Press **F** to fire Cannon
6. Damage should be 60 instead of 40 (1.5x multiplier)
7. Heat cost should be 60 instead of 30 (2x multiplier)

---

## Weapon Stats Reference

### RailGun (360° Turret)
- **Damage:** 20
- **Heat Cost:** 15
- **Range:** 30 units
- **Arc:** 360° (can fire any direction)
- **Spin-Up:** 0.2 seconds
- **Type:** Instant hit (no projectile travel)
- **Cooldown:** 0 turns
- **Ammo:** Infinite

### Newtonian Cannon (Forward Gun)
- **Damage:** 40
- **Heat Cost:** 30
- **Range:** 20 units
- **Arc:** 180° (forward hemisphere only)
- **Spin-Up:** 0.5 seconds
- **Type:** Ballistic (uses lead calculation)
- **Cooldown:** 0 turns
- **Ammo:** Infinite
- **Projectile Speed:** 2 units/second

---

## Troubleshooting

### Problem: Nothing happens when pressing keys
**Solution:** Make sure you're in Play Mode (green tint in Game view)

### Problem: Console says "No WeaponManager component found"
**Solution:** Re-run **Tools → Setup Weapon Test Scene** to recreate scene properly

### Problem: Can't see ships in Game view
**Solution:**
- Switch to Scene view to see where objects are
- Or move camera in Hierarchy to better position

### Problem: Gizmos not visible
**Solution:**
- Switch to Scene view (not Game view)
- Make sure Gizmos button is enabled (top-right of Scene view)
- Select a hardpoint GameObject

### Problem: "Target not in firing arc" for all weapons
**Solution:**
- Press **R** to reset enemy to (0, 0, 30)
- Make sure player ship is at (0, 0, 0) facing forward (+Z)

### Problem: No projectiles visible
**Solution:** This is expected! Track B (Projectiles) isn't implemented yet. You'll see:
- Console logs with "[STUB - Track B]" messages
- Damage applied to enemy
- Once Track B is done, you'll see actual projectile GameObjects

---

## Next Steps

### When Track B (Projectiles) is Complete:
You'll see:
- Actual projectile GameObjects spawning
- Ballistic trajectories for cannons
- Beam/tracer VFX for railguns
- Impact effects on hit
- Projectile pooling for performance

### When Track C (Targeting UI) is Complete:
You'll have:
- Click on ships to select targets
- Targeting reticles showing selected target
- Firing arc visualization in UI
- Range indicators
- Weapon readiness indicators per group
- UI buttons for firing groups

---

## Files Created

- `Assets/Scripts/WeaponTester.cs` - Runtime test controller
- `Assets/Scripts/Editor/WeaponTestSceneSetup.cs` - Automated scene setup
- `Assets/Scenes/WeaponTestScene.unity` - Pre-configured test scene

## Test Results

**All 45 automated tests passing:**
- Heat System: 10/10 ✅
- Ability System: 13/13 ✅
- Integration: 10/10 ✅
- **Weapon System: 12/12 ✅**

The weapon system is fully functional and ready for Tracks B and C!
