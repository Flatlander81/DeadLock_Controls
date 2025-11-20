# Ability System Testing Guide

## Step-by-Step Manual Testing Instructions

### Prerequisites
- Unity Editor 6000.2.10f1 open with the DeadLock_Controls project
- Scene with a Ship GameObject (should already exist from previous testing)

---

## Part 1: Create Ability Data Assets

### Step 1: Use the Auto-Creator Tool (Easiest Method)
1. In Unity, click **Tools → Create Default Abilities** from the top menu
2. Check the Console - you should see: `Created 6 default ability data assets in Assets/Data/Abilities/`
3. Navigate to **Assets/Data/Abilities/** in the Project window
4. You should see 6 ScriptableObject files:
   - EmergencyCooling.asset
   - ShieldBoost.asset
   - EvasiveManeuver.asset
   - OverchargeWeapons.asset
   - SensorBurst.asset
   - PDOverride.asset

**Alternative: Create Abilities Manually**
1. Right-click in **Assets/Data/Abilities/** folder
2. Select **Create → Abilities → Emergency Cooling** (or any other ability)
3. Name the asset
4. Repeat for other abilities

### Step 2: Inspect an Ability Asset
1. Click on **EmergencyCooling.asset** in the Project window
2. Look at the Inspector - you should see:
   - **Ability Name**: "Emergency Cooling"
   - **Description**: "Instantly reduces heat by 50 points"
   - **Heat Cost**: 0
   - **Max Cooldown**: 3
   - **Spin Up Time**: 0
   - **Ability Color**: Light blue
   - **Cooling Amount**: 50 (specific to Emergency Cooling)

---

## Part 2: Set Up a Ship with Abilities

### Step 3: Find or Create a Ship
1. Open your test scene (the one with a Ship GameObject)
2. Select the **Ship** GameObject in the Hierarchy
3. Verify it has these components:
   - Ship (Script)
   - HeatManager (Script)
   - AbilitySystem (Script) ← If missing, add it via Add Component

### Step 4: Assign Abilities to the Ship
1. With the Ship selected, find the **AbilitySystem** component in Inspector
2. You should see a field: **Ability Data List**
3. Set the size to **3** (or however many abilities you want to test)
4. Drag ability assets from **Assets/Data/Abilities/** into the slots:
   - **Element 0**: EmergencyCooling
   - **Element 1**: ShieldBoost
   - **Element 2**: EvasiveManeuver

5. **IMPORTANT**: Click **Apply** or save the scene!

---

## Part 3: Test in Play Mode

### Step 5: Enter Play Mode
1. Press the **Play** button (or F5)
2. The ship should auto-select on start
3. Look at the **Console** - you should see: `AbilitySystem initialized on [ShipName]. Loaded 3 abilities.`

### Step 6: View the Debug UI
1. The debug UI should be visible on the right side of the Game view
2. You should see THREE panels:
   - **Ship Info** (top) - Shows position, rotation, movement stats
   - **Heat System Debug Panel** (middle) - Shows heat, penalties, buttons
   - **Abilities (Keys 1-6)** (bottom) - Shows your 3 abilities

### Step 7: Test Ability Activation

**Test Emergency Cooling (Key 1):**
1. **Make sure you're in Command phase** (not Simulation - check phase indicator)
2. First, add some heat: Click **+50 Heat** button 2-3 times
3. Watch the heat bar turn orange/red
4. Press **1** key (or click the "1. Emergency Cooling" button)
5. **IMMEDIATELY** the button should turn **GREEN** and show **[QUEUED]**
6. You should see in Console: `Queued ability: Emergency Cooling (Heat cost: 0)`
7. Press **End Turn** button
8. Phase changes to **Simulation** - notice ability buttons become DISABLED (grayed out)
9. Console should show: `TestShip activated Emergency Cooling! Reduced heat by 50`
10. Heat should drop by 50 points
11. When turn completes, phase returns to **Command** and buttons re-enable

**Important**:
- ✅ Ability hotkeys work anytime during **Command phase** - you don't need to be in movement mode!
- ❌ During **Simulation phase**, ability buttons and hotkeys are DISABLED (no user input allowed)

**Test Shield Boost (Key 2):**
1. Click **Damage -100** button to damage shields
2. Watch shields drop in the Heat System panel (shows current hull/shields)
3. Press **2** key (or click "2. Shield Boost" button)
4. You should see: `Queued ability: Shield Boost (Heat cost: 15)`
5. Notice **Planned Heat** increases by 15 in the heat display
6. Button turns **GREEN** with **[QUEUED]**
7. Press **End Turn**
8. Console: `TestShip activated Shield Boost! Restored 100 shields. Current: X/200`
9. Shields should increase by 100

**Test Evasive Maneuver (Key 3):**
1. Press **3** key
2. Should queue with heat cost 10
3. Press **End Turn**
4. Console: `TestShip activated Evasive Maneuver! Turn rate increased by 2x for 2 turns`
5. (Note: Turn rate will reset immediately due to current implementation - duration buffs need buff system)

### Step 8: Test Cooldowns and Phase Restrictions
1. After Emergency Cooling executes, try to activate it again immediately
2. The button should be **GRAY** and show **(CD:3)** or **(CD:4)**
3. Console: `Emergency Cooling is on cooldown (X turns remaining)`
4. Press **End Turn** multiple times
5. Watch the cooldown number decrease: (CD:3) → (CD:2) → (CD:1) → (CD:0)
6. Once cooldown reaches 0, button becomes clickable again

**Test Phase Restrictions:**
1. Queue an ability (press 1, 2, or 3)
2. Press **End Turn** - phase changes to Simulation
3. **Try pressing number keys** - nothing should happen
4. **Try clicking ability buttons** - they should be grayed out and unclickable
5. **Try clicking Move Ship button** - also grayed out and disabled
6. Wait for simulation to complete
7. Phase returns to Command - all buttons re-enable

### Step 9: Test Heat Capacity Limits
1. Click **+50 Heat** button 5 times (heat = 250)
2. Try to activate Shield Boost (costs 15 heat)
3. If heat is near 300 limit (2x max heat of 150), it should fail
4. Console: `Shield Boost cannot be used (insufficient resources or conditions not met)`
5. Button stays normal (not green)

### Step 10: Test Multiple Abilities in Queue
1. Make sure Emergency Cooling is off cooldown
2. Press **1** (Emergency Cooling) - should queue
3. Press **3** (Evasive Maneuver) - should also queue
4. Both buttons should be GREEN with [QUEUED]
5. Press **End Turn**
6. Console should show BOTH abilities executing:
   ```
   TestShip executing 2 queued abilities...
   Executing Emergency Cooling...
   TestShip activated Emergency Cooling! Reduced heat by 50
   Executing Evasive Maneuver...
   TestShip activated Evasive Maneuver! Turn rate increased by 2x for 2 turns
   TestShip finished executing abilities
   ```

---

## Part 4: Test Movement Mode Integration

### Step 11: Test Abilities During Movement Planning
1. Press **M** key to enter movement mode
2. The movement UI should appear with **Move Ship (M)** button
3. Click and drag the ship projection to plan a move
4. While still in movement mode, press **1** to queue Emergency Cooling
5. Ability should queue normally
6. Press **Space** or **Enter** to confirm movement
7. Press **End Turn**
8. Both movement AND ability should execute

### Step 12: Test Ability Hotkeys (1-6)
1. Make sure you're in Command phase (not during Simulation)
2. You do NOT need to be in movement mode - hotkeys work anytime!
3. Press number keys **1, 2, 3, 4, 5, 6**
4. Only abilities you assigned (1-3) should queue with buttons turning GREEN
5. Keys 4-6 should show warning: `Ability index X out of range (0-2)`
6. This confirms hotkey system works

**Note**: The visual feedback (button turning green) happens INSTANTLY when you press the hotkey, just like clicking the button!

---

## Part 5: Expected Visual Feedback

### What You Should See:

**Ability Buttons (Normal State - Command Phase):**
- White background
- Shows ability name and heat cost: "1. Emergency Cooling (Heat:0)"
- Clickable and responsive to hotkeys

**Ability Buttons (On Cooldown):**
- Gray background
- Shows cooldown remaining: "1. Emergency Cooling (CD:3)"
- Cannot click (but NOT because of phase)

**Ability Buttons (Queued):**
- Green background
- Shows [QUEUED]: "1. Emergency Cooling [QUEUED]"
- Heat bar shows planned heat increase

**Ability Buttons (Disabled - Simulation Phase):**
- Grayed out appearance
- NOT clickable
- Hotkeys don't work
- This applies to ALL abilities regardless of cooldown state

**Ability Buttons (Can't Afford):**
- Yellow background (if implemented)
- Shows heat cost but clicking fails

### Heat System Integration:
- **Planned Heat** increases when abilities queue
- Heat bar shows preview of total heat (current + planned)
- Heat commits when abilities execute
- Cooldowns tick down at end of each turn

---

## Troubleshooting

### Problem: "AbilitySystem initialized on TestShip. Loaded 0 abilities."
**Solution**: You didn't assign abilities to the AbilitySystem component. Go back to Step 4.

### Problem: "Ability 'Emergency Cooling' not found on TestShip"
**Solution**: The ability data list is empty or the ability names don't match. Check Step 4.

### Problem: Abilities don't appear in debug UI
**Solution**:
1. Make sure you're in Play mode
2. Check that DebugUI component is on a GameObject in the scene
3. Verify the ship has an AbilitySystem component

### Problem: Pressing number keys does nothing
**Solution**:
1. Make sure ship is selected (auto-selects on start)
2. Make sure you're in Command Phase (TurnManager should start in Command phase)
3. Check Console for error messages

### Problem: "Tools → Create Default Abilities" menu doesn't exist
**Solution**: The AbilityDataCreator.cs script might not be compiled. Check for compile errors in Console.

### Problem: Abilities execute but nothing happens
**Solution**:
- Emergency Cooling needs heat to reduce (add heat first)
- Shield Boost needs damaged shields (damage ship first)
- Evasive Maneuver effect is immediately reset (this is expected in current implementation)

---

## Success Criteria

✅ You have successfully tested the ability system if:
1. All 6 ability assets were created in Assets/Data/Abilities/
2. You assigned 3 abilities to a ship's AbilitySystem component
3. Ability UI panel shows all 3 abilities with correct names and heat costs
4. Pressing number keys 1-3 queues abilities (turns buttons green)
5. Pressing End Turn executes queued abilities with console output
6. Abilities go on cooldown after execution (gray buttons with CD timer)
7. Cooldowns tick down each turn
8. Heat cost is added to planned heat when queuing
9. Heat is committed when abilities execute
10. Multiple abilities can be queued and execute together

---

## Advanced Testing (Optional)

### Test All 6 Abilities:
1. Assign all 6 abilities to AbilitySystem (set size to 6)
2. Test each ability's unique effect:
   - **Emergency Cooling**: Reduces heat by 50
   - **Shield Boost**: Restores 100 shields
   - **Evasive Maneuver**: Doubles turn rate (resets immediately)
   - **Overcharge Weapons**: Increases damage multiplier
   - **Sensor Burst**: Detects enemies in radius
   - **PD Override**: Boosts point defense

### Create Custom Ability:
1. Create a new C# class inheriting from `AbilityData`
2. Override `Execute(Ship ship)` method
3. Add `[CreateAssetMenu]` attribute
4. Create instance via Create menu
5. Assign to ship and test

### Test Edge Cases:
- Queue 6 abilities at once
- Try to activate ability twice
- Queue ability, then clear queue (requires code modification)
- Test with multiple ships in scene
- Test heat going into Catastrophic tier while ability queued

---

## Next Steps After Testing

Once you've verified the ability system works:
1. Create more interesting ability effects
2. Implement duration/buff tracking system for timed abilities
3. Add ability animations/VFX
4. Create ability UI with icons and tooltips
5. Implement ability targeting system for enemy-targeted abilities
6. Add ability unlocking/progression system

---

## Questions or Issues?

If you encounter problems not covered in this guide:
1. Check the Console for error messages
2. Verify all components are attached to Ship GameObject
3. Ensure ability assets are properly created and assigned
4. Review the test results in TestResults.xml (should show 23/23 passing)
