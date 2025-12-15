# Targeting UI System - Comprehensive Test Guide

## Table of Contents
1. [Overview](#overview)
2. [Test Scenarios](#test-scenarios)
3. [Integration Tests](#integration-tests)
4. [Troubleshooting](#troubleshooting)
5. [Performance](#performance)

---

## Overview

This guide covers comprehensive testing of the **Phase 2 Track C - Targeting UI System**.

### Components Being Tested
- TargetingController (target selection, firing coordination)
- SelectionIndicator (visual feedback)
- WeaponConfigPanel (weapon group assignment)
- WeaponGroupPanel (firing interface)
- TargetingLineRenderer (visual targeting lines)
- UIManager (panel state management)

### Prerequisites
- Completed Track A (Weapon System)
- Completed Track B (Projectile System)
- Test scene set up (Tools → Setup Targeting Test Scene)

---

## Test Scenarios

### Scenario 1: Basic Target Selection

**Objective:** Verify ships can be selected and deselected.

**Steps:**
1. Press Play
2. Left-click on Enemy1 (right cube)
3. **Verify:** Cyan rotating ring appears
4. **Verify:** Weapon Group Panel appears on right side
5. **Verify:** Panel header shows "Target: Enemy1"
6. Press ESC
7. **Verify:** Ring disappears
8. **Verify:** Panel disappears

**Expected Results:**
- ✅ Selection indicator appears immediately
- ✅ Ring rotates smoothly at 45°/second
- ✅ Panel shows correct target name
- ✅ Deselection clears all visuals

**Failure Cases:**
- ❌ No ring appears → Check SelectionIndicator prefab
- ❌ Panel doesn't show → Check UIManager initialization
- ❌ Ring doesn't rotate → Check Update() method

---

### Scenario 2: Player Ship Selection

**Objective:** Verify player ship selection shows config panel.

**Steps:**
1. Press Play
2. Left-click on Hephaestus (green wireframe ship)
3. **Verify:** Green rotating ring appears
4. **Verify:** Weapon Config Panel appears on left side
5. **Verify:** Panel shows "=== HEPHAESTUS WEAPONS ==="
6. **Verify:** List of weapons displayed
7. **Verify:** Each weapon has group button
8. Click empty space
9. **Verify:** Panel disappears

**Expected Results:**
- ✅ Green ring distinguishes from enemy (cyan)
- ✅ Config panel appears on left (not right)
- ✅ All weapons listed with stats
- ✅ Group buttons are interactive

**Failure Cases:**
- ❌ Wrong color ring → Check Initialize() friendly parameter
- ❌ No weapons shown → Check WeaponManager discovery
- ❌ Buttons don't work → Check OnGUI() button handling

---

### Scenario 3: Weapon Group Assignment

**Objective:** Verify weapons can be assigned to groups 1-4.

**Steps:**
1. Click Hephaestus
2. Find "Rail Gun" in weapon list
3. Note current group (should be "Unassigned")
4. Click group button next to Rail Gun
5. **Verify:** Button changes to "Group 1" (blue)
6. Click button again
7. **Verify:** Changes to "Group 2" (red)
8. Click again
9. **Verify:** Changes to "Group 3" (green)
10. Click again
11. **Verify:** Changes to "Group 4" (yellow)
12. Click again
13. **Verify:** Returns to "Unassigned" (white)

**Expected Results:**
- ✅ Button cycles through all 5 states
- ✅ Colors match group assignments
- ✅ Console shows "Assigned [weapon] to group [N]"
- ✅ Assignment persists when re-selecting ship

**Failure Cases:**
- ❌ Button doesn't cycle → Check modulo logic (% 5)
- ❌ Wrong colors → Check GetGroupColor() method
- ❌ Assignment not saved → Check AssignWeaponToGroup()

---

### Scenario 4: Single Weapon Group Firing

**Objective:** Fire a single weapon group at a target.

**Steps:**
1. Click Hephaestus
2. Assign Rail Gun to Group 1 (click until blue)
3. Click Enemy1 (select target)
4. **Verify:** Weapon Group Panel shows Group 1 with Rail Gun
5. **Verify:** Heat cost shown (15)
6. **Verify:** Status shows "Ready" in green
7. Press **1** key
8. **Verify:** Blue targeting line appears from player to Enemy1
9. **Verify:** Cyan beam effect fires
10. **Verify:** Enemy1 shields decrease
11. **Verify:** Console shows "Firing group 1"

**Expected Results:**
- ✅ Targeting line appears before firing
- ✅ Line color matches group (blue for Group 1)
- ✅ Weapon fires after spin-up (0.2s for RailGun)
- ✅ Damage applied to target
- ✅ Line persists after firing

**Failure Cases:**
- ❌ No targeting line → Check UpdateTargetingLine()
- ❌ Weapon doesn't fire → Check FireGroupAtCurrentTarget()
- ❌ No damage → Check weapon CanFire() conditions

---

### Scenario 5: Multi-Group Targeting

**Objective:** Assign different groups to different targets.

**Steps:**
1. Click Hephaestus
2. Assign Rail Gun → Group 1 (blue)
3. Assign Cannon → Group 2 (red)
4. Click Enemy1
5. Press **1** (fire Group 1 at Enemy1)
6. **Verify:** Blue line to Enemy1
7. **Verify:** Rail Gun fires
8. Click Enemy2
9. Press **2** (fire Group 2 at Enemy2)
10. **Verify:** Red line to Enemy2
11. **Verify:** Cannon fires
12. **Verify:** Both targeting lines visible simultaneously

**Expected Results:**
- ✅ Each group can target independently
- ✅ Multiple targeting lines visible
- ✅ Lines stay after switching targets
- ✅ Correct weapons fire at correct targets

**Failure Cases:**
- ❌ Old line disappears → Check line persistence
- ❌ Wrong target → Check SetGroupTarget() logic
- ❌ Groups interfere → Check dictionary management

---

### Scenario 6: Alpha Strike

**Objective:** Fire all weapons simultaneously at one target.

**Steps:**
1. Click Hephaestus
2. Assign ALL weapons to different groups (distribute across 1-4)
3. Click Enemy2
4. **Verify:** Weapon Group Panel shows Alpha Strike section
5. **Verify:** Shows "FIRE ALL WEAPONS (X/X ready)"
6. **Verify:** Total heat displayed
7. Press **A** key
8. **Verify:** Console shows "ALPHA STRIKE on Enemy2"
9. **Verify:** Multiple weapon effects fire
10. **Verify:** Enemy2 takes massive damage
11. **Verify:** Multiple targeting lines appear

**Expected Results:**
- ✅ All ready weapons fire
- ✅ Heat cost is sum of all weapons
- ✅ Weapons fire in parallel (simultaneous)
- ✅ Single hotkey fires everything
- ✅ Dramatic visual effect

**Failure Cases:**
- ❌ Only some weapons fire → Check AlphaStrike() target setting
- ❌ Weapons fire sequentially → Check coroutine parallelization
- ❌ Wrong heat total → Check heat calculation logic

---

### Scenario 7: Out-of-Arc Detection

**Objective:** Verify weapons with limited arcs show warnings.

**Steps:**
1. Click Hephaestus
2. Assign Cannon (180° forward arc) to Group 2
3. Click Enemy3 (positioned behind player)
4. **Verify:** Weapon Group Panel shows Group 2
5. **Verify:** Cannon listed under group
6. **Verify:** Yellow warning: "[OUT OF ARC]"
7. **Verify:** Heat cost is 0 (weapon can't fire)
8. Press **2**
9. **Verify:** Cannon does NOT fire
10. **Verify:** Console shows "Target not in firing arc"

**Expected Results:**
- ✅ Warning appears before attempting to fire
- ✅ Heat cost excludes out-of-arc weapons
- ✅ Firing attempt is prevented
- ✅ Clear feedback why weapon can't fire

**Failure Cases:**
- ❌ No warning → Check IsInArc() call in UI
- ❌ Weapon fires anyway → Check CanFire() logic
- ❌ Wrong arc calculation → Check angle math

---

### Scenario 8: Cooldown Warnings

**Objective:** Verify weapons on cooldown show warnings.

**Steps:**
1. Click Hephaestus
2. Assign Heavy Laser to Group 3 (if available, has cooldown)
3. Click Enemy1
4. Press **3** (fire once)
5. **Verify:** Weapon fires successfully
6. Immediately check Weapon Group Panel
7. **Verify:** Yellow warning: "[COOLDOWN]"
8. **Verify:** Individual weapon shows "(CD:X)" where X is turns remaining
9. **Verify:** Weapon grayed out in config panel
10. Press **3** again
11. **Verify:** Weapon does NOT fire
12. **Verify:** Console shows "On cooldown"

**Expected Results:**
- ✅ Cooldown warning immediate after firing
- ✅ Countdown shows remaining turns
- ✅ Visual indication (gray text)
- ✅ Firing prevented during cooldown

**Failure Cases:**
- ❌ No cooldown → Check weapon MaxCooldown setting
- ❌ Warning doesn't appear → Check CurrentCooldown check
- ❌ Can fire on cooldown → Check CanFire() validation

---

### Scenario 9: Heat Capacity Warning

**Objective:** Verify warning when firing would exceed heat capacity.

**Steps:**
1. Click Hephaestus
2. Assign ALL weapons to Group 1
3. Check current heat (use DebugUI or console)
4. Click Enemy1
5. **Verify:** Group 1 shows very high heat cost (60+)
6. **Verify:** Alpha Strike section shows total
7. **Verify:** If total > max capacity, red warning appears
8. **Verify:** "WARNING: Will exceed heat capacity!"
9. Press **1** anyway (if allowed)
10. **Verify:** Weapons fire
11. **Verify:** Ship heat increases significantly
12. Check heat bar (if visible)

**Expected Results:**
- ✅ Warning appears in red text
- ✅ Calculation is correct (current + planned)
- ✅ Warning is preventive (shows before firing)
- ✅ Player can still fire (warning, not restriction)

**Failure Cases:**
- ❌ Wrong heat total → Check CalculateGroupHeatCost()
- ❌ Warning doesn't appear → Check MaxHeat comparison
- ❌ Heat not applied → Check weapon heat integration

---

### Scenario 10: Targeting Line Updates

**Objective:** Verify targeting lines follow moving ships.

**Steps:**
1. Click Hephaestus
2. Assign Rail Gun to Group 1
3. Click Enemy1
4. Press **1** (create targeting line)
5. **Verify:** Blue line from Hephaestus to Enemy1
6. Enter Movement Mode (press M)
7. Move Hephaestus to new position
8. Confirm movement
9. **Verify:** Blue line updates to start from new position
10. (If enemies can move) Move Enemy1
11. **Verify:** Line endpoint follows Enemy1

**Expected Results:**
- ✅ Line updates every frame
- ✅ Line start follows player ship
- ✅ Line end follows target ship
- ✅ No lag or stuttering
- ✅ Line destroyed if ship destroyed

**Failure Cases:**
- ❌ Line doesn't update → Check Update() method
- ❌ Line breaks → Check null ship checks
- ❌ Line persists after death → Check destruction logic

---

### Scenario 11: UI State Transitions

**Objective:** Verify clean transitions between UI states.

**Steps:**
1. Start with nothing selected
2. **Verify:** No panels visible
3. Click Enemy1
4. **Verify:** Weapon Group Panel appears (right)
5. **Verify:** Config panel NOT visible
6. Click Hephaestus (while Enemy1 selected)
7. **Verify:** Config Panel appears (left)
8. **Verify:** Group Panel disappears
9. **Verify:** Enemy1 deselected
10. Click Enemy2
11. **Verify:** Group Panel reappears
12. **Verify:** Config Panel disappears
13. **Verify:** Target switches to Enemy2
14. Press ESC
15. **Verify:** All panels disappear
16. **Verify:** All selections cleared

**Expected Results:**
- ✅ Only relevant panel shown for selection
- ✅ Smooth transitions (no flicker)
- ✅ State changes logged to console
- ✅ ESC clears everything

**Failure Cases:**
- ❌ Both panels visible → Check UIManager state logic
- ❌ Panels don't hide → Check Hide() calls
- ❌ State mismatch → Check event subscription

---

### Scenario 12: Hotkey Integration

**Objective:** Verify hotkeys work correctly with MovementController.

**Steps:**
1. Nothing selected
2. Press **1-6**
3. **Verify:** Abilities activate (if available on player ship)
4. Click Enemy1 (select target)
5. Press **1-4**
6. **Verify:** Weapon groups fire (NOT abilities)
7. **Verify:** Console shows "Firing group X"
8. Press **5-6**
9. **Verify:** Abilities still work
10. Press ESC (deselect)
11. Press **1-4** again
12. **Verify:** Back to activating abilities

**Expected Results:**
- ✅ Keys 1-4 context-sensitive
- ✅ Priority: targeting > abilities
- ✅ Keys 5-6 always abilities
- ✅ No conflicts or double-triggers

**Failure Cases:**
- ❌ Both fire → Check isTargetingEnemy condition
- ❌ Neither works → Check TargetingController reference
- ❌ Wrong action → Check hotkey order in Update()

---

## Integration Tests

### Movement + Targeting Integration

**Test Flow:**
1. Enter Movement Mode (M)
2. Plan movement for Hephaestus
3. Exit Movement Mode
4. Select Enemy1
5. Fire weapons
6. **Verify:** Both systems work independently
7. **Verify:** No interference
8. **Verify:** Hotkeys context-switch properly

### Heat + Targeting Integration

**Test Flow:**
1. Fire weapons to build heat
2. Monitor heat manager
3. Check heat warnings in UI
4. Wait for heat dissipation
5. **Verify:** Heat costs accurate
6. **Verify:** Warnings appear at thresholds
7. **Verify:** Heat affects weapon availability

### Projectiles + Targeting Integration

**Test Flow:**
1. Assign ballistic weapons (Cannon) to Group 1
2. Assign instant-hit (RailGun) to Group 2
3. Fire both groups at same target
4. **Verify:** Instant hits apply immediately
5. **Verify:** Ballistic projectiles spawn and travel
6. **Verify:** Both damage target
7. **Verify:** Targeting lines shown for both

---

## Troubleshooting

### Issue: Panels Don't Appear

**Symptoms:** Clicking ships does nothing, no UI.

**Diagnosis:**
1. Check Hierarchy for UIManager
2. Check Console for initialization errors
3. Verify TargetingController exists
4. Check event subscriptions

**Fix:**
- Run "Tools → Setup Targeting Test Scene" again
- Check UIManager.Initialize() called
- Verify panel references assigned

---

### Issue: Weapons Don't Fire

**Symptoms:** Pressing hotkeys does nothing.

**Diagnosis:**
1. Check if target is selected (check CurrentTarget)
2. Verify weapons assigned to groups
3. Check weapon CanFire() conditions
4. Look for arc/range/cooldown issues

**Fix:**
- Ensure enemy selected before firing
- Check weapon assignments in config panel
- Verify target in arc and range
- Check for cooldown status

---

### Issue: Targeting Lines Missing

**Symptoms:** No colored lines appear.

**Diagnosis:**
1. Check Console for LineRenderer errors
2. Verify TargetingLineRenderer component creation
3. Check Shader availability (Sprites/Default)
4. Verify line not behind camera

**Fix:**
- Ensure AssignGroupToCurrentTarget() called
- Check LineRenderer material/shader
- Verify sorting order (should be 50)
- Check camera rendering settings

---

### Issue: Wrong Panel Shows

**Symptoms:** Config panel shows for enemy or vice versa.

**Diagnosis:**
1. Check UIManager state
2. Verify ship == playerShip comparison
3. Check event routing

**Fix:**
- Verify playerShip reference set
- Check HandleShipSelected() logic
- Ensure only one UIManager in scene

---

### Issue: Hotkeys Not Working

**Symptoms:** Pressing 1-4 does nothing.

**Diagnosis:**
1. Check TargetingController Update() running
2. Verify CurrentTarget not null
3. Check Input.GetKeyDown() calls
4. Look for MovementController conflicts

**Fix:**
- Ensure target selected
- Check TargetingController in scene
- Verify MovementController integration
- Check console for input errors

---

## Performance

### Expected Performance Metrics

**Frame Rate:**
- 60+ FPS with targeting active
- No drops when creating targeting lines
- Smooth ring rotation

**Memory:**
- Minimal GC allocations
- LineRenderers pooled/reused
- No memory leaks

**Responsiveness:**
- < 1 frame delay for selection
- Immediate hotkey response
- Real-time line updates

### Performance Testing

**Test 1: Multiple Targets**
1. Create 10+ enemy ships
2. Cycle through selecting each
3. Monitor FPS
4. **Expected:** No degradation

**Test 2: All Groups Active**
1. Assign weapons to all 4 groups
2. Create targeting lines for all
3. Monitor frame time
4. **Expected:** 4 LineRenderers = negligible cost

**Test 3: Rapid Selection**
1. Rapidly click between ships
2. Monitor GC allocations
3. Check for memory spikes
4. **Expected:** Minimal allocations

---

## Success Criteria

### Functional Requirements ✅
- [x] Ships can be selected via mouse
- [x] Selection indicators appear correctly
- [x] Weapons can be assigned to groups 1-4
- [x] Weapon groups fire on hotkeys
- [x] Alpha Strike fires all weapons
- [x] Targeting lines show and update
- [x] UI panels show/hide based on selection
- [x] Out-of-arc warnings display
- [x] Cooldown warnings display
- [x] Heat warnings display

### Integration Requirements ✅
- [x] Works with Weapon System (Track A)
- [x] Works with Projectile System (Track B)
- [x] Integrates with Movement System
- [x] Integrates with Heat System
- [x] Hotkey coordination with abilities

### Polish Requirements ✅
- [x] Color-coded groups (Blue/Red/Green/Yellow)
- [x] Clear visual feedback
- [x] Informative warnings
- [x] Responsive controls
- [x] No bugs or crashes

---

## Test Report Template

```
Date: _______________
Tester: _______________
Build: _______________

Scenario Results:
[ ] Scenario 1: Target Selection - PASS / FAIL
[ ] Scenario 2: Player Selection - PASS / FAIL
[ ] Scenario 3: Group Assignment - PASS / FAIL
[ ] Scenario 4: Single Group Fire - PASS / FAIL
[ ] Scenario 5: Multi-Group Target - PASS / FAIL
[ ] Scenario 6: Alpha Strike - PASS / FAIL
[ ] Scenario 7: Arc Detection - PASS / FAIL
[ ] Scenario 8: Cooldown Warnings - PASS / FAIL
[ ] Scenario 9: Heat Warnings - PASS / FAIL
[ ] Scenario 10: Line Updates - PASS / FAIL
[ ] Scenario 11: UI Transitions - PASS / FAIL
[ ] Scenario 12: Hotkey Integration - PASS / FAIL

Issues Found:
1. _______________
2. _______________
3. _______________

Notes:
_______________
_______________
_______________

Overall: PASS / FAIL
```

---

## Conclusion

This comprehensive test guide ensures all aspects of the Targeting UI System are thoroughly validated. Follow the scenarios in order for best results, and refer to the troubleshooting section if issues arise.

For quick reference during testing, see **TARGETING_QUICK_START.md**.

Happy testing! ⚔️
