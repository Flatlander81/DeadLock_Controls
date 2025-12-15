# Targeting UI System - Quick Start

## 🚀 3-Step Setup

```
1. Unity Editor → Tools → Setup Targeting Test Scene
2. Press Play
3. Start testing!
```

---

## ⌨️ Keyboard Controls

| Key | Action | What You'll See |
|-----|--------|-----------------|
| **Left Click** | Select Ship | 🎯 Rotating ring indicator |
| **M** | Toggle Movement Mode (Player Only) | 📍 Show/hide movement projection |
| **1-4** | Fire Weapon Groups (Enemy Targeted) | 💥 Group weapons fire at target |
| **A** | Alpha Strike (Enemy Targeted) | 🔥 ALL weapons fire! |
| **5-6** | Activate Abilities | ⚡ Ship abilities (when available) |
| **ESC** | Deselect All | ❌ Clear selection |

**Important:** Only your ship (Hephaestus) can be moved! Enemy ships are for targeting only.

---

## 👀 What to Watch For

### Selecting Enemy Ship
- 🎯 Click on enemy ship
- 💠 Cyan rotating ring appears
- 📊 Weapon Group Panel opens (right side)
- 🔵 Colored targeting lines appear

### Selecting Player Ship (Hephaestus)
- 🎯 Click on player ship (green wireframe)
- 💚 Green rotating ring appears
- ⚙️ Weapon Config Panel opens (left side)
- 🔧 Shows all weapons with group assignments

### Weapon Group Configuration
- Click Hephaestus → See Weapon Config Panel
- Click group button to cycle: Unassigned → Group 1 → Group 2 → Group 3 → Group 4 → Unassigned
- Button colors match groups:
  - 🔵 Blue = Group 1
  - 🔴 Red = Group 2
  - 🟢 Green = Group 3
  - 🟡 Yellow = Group 4

### Firing Weapons
- Click enemy → Select target
- Press **1** → Fire Group 1 (blue line to target)
- Press **2** → Fire Group 2 (red line to target)
- Press **3** → Fire Group 3 (green line to target)
- Press **4** → Fire Group 4 (yellow line to target)
- Press **A** → Alpha Strike (ALL weapons fire)

---

## ✅ Quick Tests

### Test Target Selection
1. Click **Enemy1** (cube on right)
2. See cyan rotating ring ✅
3. See Weapon Group Panel (right side) ✅
4. Press **ESC**
5. Ring disappears ✅

### Test Weapon Configuration
1. Click **Hephaestus** (green wireframe)
2. See green rotating ring ✅
3. See Weapon Config Panel (left side) ✅
4. See list of weapons with groups ✅
5. Click group button next to "Rail Gun"
6. Watch it cycle through groups ✅
7. Button color changes ✅

### Test Weapon Firing
1. Click **Hephaestus**
2. Assign Rail Gun to **Group 1** (blue)
3. Assign Cannon to **Group 2** (red)
4. Click **Enemy1**
5. Press **1** (fire group 1)
6. See blue targeting line ✅
7. See rail gun beam effect ✅
8. Enemy shields drop ✅
9. Press **2** (fire group 2)
10. See red targeting line ✅
11. See cannon projectile fly ✅

### Test Alpha Strike
1. Click **Enemy2** (cube ahead)
2. Press **A** (alpha strike)
3. See ALL weapons fire! ✅
4. See multiple targeting lines ✅
5. Enemy takes massive damage ✅
6. Console shows "ALPHA STRIKE" ✅

### Test Heat Warnings
1. Click **Hephaestus**
2. Assign ALL weapons to **Group 1**
3. Click **Enemy1**
4. Weapon Group Panel shows total heat ✅
5. If heat > capacity, see red warning ✅
6. "WARNING: Will exceed heat capacity!" ✅

### Test Out-of-Arc Warnings
1. Click **Enemy3** (cube behind)
2. Check Weapon Group Panel
3. Cannon shows "[OUT OF ARC]" warning ✅
4. RailGun is ready (360° turret) ✅
5. Press **1** anyway
6. Only RailGun fires ✅

---

## 🎯 Success Checklist

After 10 minutes of testing, you should have seen:

- ✅ Cyan ring on selected enemy
- ✅ Green ring on selected player ship
- ✅ Weapon Config Panel (left side)
- ✅ Weapon Group Panel (right side)
- ✅ Colored targeting lines (blue/red/green/yellow)
- ✅ Group buttons cycling through assignments
- ✅ Weapon stats (arc, range, heat, cooldown)
- ✅ Heat cost totals
- ✅ "OUT OF ARC" warnings
- ✅ "COOLDOWN" warnings
- ✅ Alpha Strike firing all weapons
- ✅ Console logs for targeting actions

---

## 🐛 Common Issues

**No panel appears when selecting ship?**
→ Check UIManager exists in hierarchy
→ Check Console for initialization errors

**Can't fire weapons?**
→ Enemy must be selected first
→ Check weapon is in arc and range
→ Check weapon not on cooldown

**Targeting lines don't appear?**
→ Assign weapon group to target first
→ Check LineRenderer component created

**Groups don't fire?**
→ Assign weapons to groups first
→ Select enemy target
→ Press number key (1-4)

**Alpha Strike does nothing?**
→ Select enemy target first
→ Ensure weapons in arc/range
→ Check heat capacity

**Hotkeys don't work?**
→ Check TargetingController in scene
→ Verify MovementController integration
→ Enemy must be targeted for groups 1-4

**Can I move enemy ships?**
→ NO! Only Hephaestus (your ship) can be moved
→ Enemy ships are for targeting only
→ Movement mode only works on player ship

---

## 📊 On-Screen Info

**Weapon Config Panel (Left):**
- Ship name header
- List of all weapons
- Group assignment buttons (colored)
- Weapon stats per weapon
- Cooldown indicators

**Weapon Group Panel (Right):**
- Target name header
- 4 weapon group buttons
- Weapons in each group
- Heat cost per group
- Status warnings
- Alpha Strike button
- Total heat display

**Selection Indicators:**
- Rotating ring around ship
- Cyan = Enemy
- Green = Player/Friendly

**Targeting Lines:**
- From player to targets
- Blue = Group 1
- Red = Group 2
- Green = Group 3
- Yellow = Group 4

---

## 🎮 Advanced Testing

### Multi-Targeting
1. Assign Rail Gun → Group 1
2. Assign Cannon → Group 2
3. Click Enemy1
4. Press **1** (Rail Gun fires at Enemy1)
5. Click Enemy2
6. Press **2** (Cannon fires at Enemy2)
7. See two different targeting lines! ✅

### State Transitions
1. Nothing selected → No panels
2. Click Enemy → Weapon Group Panel appears
3. Click Hephaestus → Weapon Config Panel appears
4. Click Enemy again → Weapon Group Panel appears
5. Press ESC → All panels disappear

### Hotkey Priority
1. Click Hephaestus (no target)
2. Press **1-4** → Activates abilities (if available)
3. Click Enemy (target selected)
4. Press **1-4** → Fires weapon groups
5. Press **5-6** → Still activates abilities

---

## 📈 Performance

**Expected:**
- Smooth UI updates ✅
- Instant selection response ✅
- No lag when firing ✅
- Targeting lines follow ships ✅

---

## 📁 Files

**Scene:** `Assets/Scenes/TargetingTestScene.unity`

**Key Scripts:**
- `TargetingController.cs` - Target selection & firing
- `UIManager.cs` - Panel management
- `WeaponConfigPanel.cs` - Weapon assignment UI
- `WeaponGroupPanel.cs` - Firing UI
- `SelectionIndicator.cs` - Ring visuals
- `TargetingLineRenderer.cs` - Colored lines

**Prefabs:** `Assets/Prefabs/UI/`

---

## 🚀 Next Level Testing

See **TARGETING_TEST_GUIDE.md** for:
- 12 detailed test scenarios
- Integration with movement system
- Combat flow testing
- Multi-ship scenarios
- Performance profiling

---

## ✨ Test Results

**All 69/69 tests passing!**
- Heat: 10/10 ✅
- Abilities: 13/13 ✅
- Integration: 10/10 ✅
- Weapons: 12/12 ✅
- Projectiles: 12/12 ✅
- **Targeting: 12/12 ✅**

---

**Ready to command the fleet!** Click, target, and fire! ⚔️
