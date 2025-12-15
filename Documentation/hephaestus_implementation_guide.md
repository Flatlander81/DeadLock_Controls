4. Record playtests (screen capture)

---

#### CLAUDE CODE PROMPT 6.1

```
CONTEXT:
Phase 5 complete - game is polished and playable. Now tuning balance based on playtesting data.

OBJECTIVE:
Create tools and systems for rapid balance iteration, then tune all values for optimal gameplay experience.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Debug/BalanceDebugger.cs
   - In-game tool for runtime balance adjustments
   - Inspector-accessible during Play mode
   - Properties (all with [Range] attributes):
     * Player ship stats (hull, shields, heat capacity)
     * Weapon damage values (all 4 types)
     * Weapon heat costs (all 4 types)
     * Weapon cooldowns
     * Ability heat costs (all 6 abilities)
     * Ability cooldowns
     * Passive cooling rate
     * Heat penalty thresholds
     * Enemy ship stats (per type)
     * PD success rates (torpedo, missile, cannon)
   
   - Methods:
     * void ApplyChanges() - Update all systems with new values
     * void ResetToDefaults() - Restore original balance values
     * void SaveCurrentBalance() - Export to JSON file
     * void LoadBalance(string filename) - Import from JSON

2. CREATE: Assets/Scripts/Testing/TestScenarioLoader.cs
   - Quick-load predefined battle scenarios
   - Scenarios:
     * Tutorial: 1 Frigate
     * Easy: 2 Frigates
     * Medium1: 1 Frigate + 1 Destroyer
     * Medium2: 2 Destroyers
     * Hard1: 1 Battleship
     * Hard2: 1 Battleship + 1 Frigate
     * Boss: 1 Battleship (Enhanced stats)
     * Stress Test: 3 Battleships (performance test)
   
   - Methods:
     * void LoadScenario(ScenarioType type)
     * void RestartCurrentScenario()

3. Balance Parameters to Tune (Reference from GDD):

   **Weapon Balance** (Initial → Tuned):
   - Rail Gun Damage: 20 → ?
   - Rail Gun Heat: 15 → ?
   - Cannon Damage: 40 → ?
   - Cannon Heat: 30 → ?
   - Missile Damage: 30 → ?
   - Missile Heat: 20 → ?
   - Torpedo Damage: 80 → ?
   - Torpedo Heat: 25 → ?

   **Ability Balance**:
   - Emergency Cooling: 0 heat, 4T cooldown → ?
   - Shield Boost: 25 heat, 3T cooldown → ?
   - Evasive Maneuver: 35 heat, 2T cooldown → ?
   - Overcharge: 20 heat, 3T cooldown → ?
   - Sensor Burst: 15 heat, 2T cooldown → ?
   - PD Override: 30 heat, 3T cooldown → ?

   **Heat Economy**:
   - Passive Cooling: -20/turn → ?
   - Heat Capacity: 150 → ?
   - Penalty Thresholds: 60/80/100/120 → ?

   **Ship Stats** (per type):
   - Player: 500HP, 200 shields → ?
   - Frigate: 200HP, 100 shields → ?
   - Destroyer: 350HP, 150 shields → ?
   - Battleship: 600HP, 250 shields → ?

4. CREATE: Assets/Scripts/Testing/CombatLogger.cs
   - Logs combat events for analysis
   - Tracks:
     * Damage dealt per weapon type
     * Heat accumulated per weapon/ability
     * Turn count to victory/defeat
     * Ship survivability
     * Weapon/ability usage frequency
     * PD interception rates
   
   - Exports to CSV for analysis:
     * Playtester ID, Scenario, Result, Turn Count, Damage Stats, etc.

5. Playtesting Goals & Metrics:

   **Tutorial (1 Frigate)**:
   - Target: 95%+ win rate
   - Target duration: 3-5 turns
   - Goal: Teach basics, build confidence

   **Easy (2 Frigates)**:
   - Target: 80-90% win rate
   - Target duration: 5-8 turns
   - Goal: Practice fundamentals

   **Medium Scenarios**:
   - Target: 50-70% win rate
   - Target duration: 8-12 turns
   - Goal: Challenge average players

   **Hard Scenarios**:
   - Target: 30-50% win rate
   - Target duration: 12-20 turns
   - Goal: Test skilled players

   **Boss (Battleship)**:
   - Target: 20-40% win rate
   - Target duration: 15-25 turns
   - Goal: Epic, difficult fight

6. Iterative Tuning Process:
   ```
   1. Playtest current balance (5-10 players per scenario)
   2. Collect data (win rates, durations, feedback)
   3. Identify issues:
      - Weapons too weak/strong?
      - Heat too punishing/forgiving?
      - Enemies too easy/hard?
      - Abilities too powerful/useless?
   4. Use BalanceDebugger to adjust values
   5. Playtest again
   6. Repeat until metrics met
   ```

7. Common Balance Issues to Watch For:

   **Dominant Strategies**:
   - Players always use same weapon/ability?
   - One strategy clearly superior?
   - Fix: Buff alternatives, nerf dominant choice

   **Unused Systems**:
   - Players never use certain weapons?
   - Abilities ignored?
   - Fix: Make them more impactful or reduce costs

   **Heat Economy Problems**:
   - Players never overheat (too forgiving)?
   - Players always overheat (too punishing)?
   - Fix: Adjust passive cooling, weapon costs, penalties

   **Difficulty Spikes**:
   - One scenario much harder than expected?
   - Difficulty progression uneven?
   - Fix: Adjust enemy stats, numbers, or AI behavior

8. Integration with Analytics:
   - Add Unity Analytics (optional)
   - Track:
     * Scenario completion rates
     * Average turn counts
     * Weapon usage statistics
     * Ability usage statistics
     * Most common cause of defeat

DELIVERABLES:
1. BalanceDebugger.cs with runtime tuning
2. TestScenarioLoader.cs with predefined battles
3. CombatLogger.cs for data export
4. Balance tuning documentation:
   - Initial values
   - Playtesting results
   - Tuned values
   - Rationale for changes
5. CSV export of playtesting data
6. Final balanced values (exported JSON)
7. Test scenarios (all difficulty levels)

MANUAL TESTING & TUNING:

TUNING PROCESS (Iterative):

ITERATION 1: Baseline Playtest
1. Use initial GDD values
2. Playtest all scenarios (5-10 players each)
3. Collect data:
   - Win rates
   - Turn durations
   - Player feedback
   - Combat logs
4. Identify problems

ITERATION 2: First Balance Pass
1. Analyze data from Iteration 1
2. Common issues found (examples):
   - "Torpedoes too weak for heat cost"
   - "Emergency Cooling cooldown too short"
   - "Frigates too easy"
3. Adjust values using BalanceDebugger:
   - Increase torpedo damage 80 → 100
   - Increase Emergency Cooling cooldown 4T → 5T
   - Increase Frigate HP 200 → 250
4. Export new balance JSON
5. Playtest again

ITERATION 3-5: Refinement
1. Continue adjusting based on feedback
2. Focus on problem areas
3. Watch for new issues created by changes
4. Test edge cases:
   - Can player overheat and recover?
   - Can player survive without abilities?
   - Is alpha strike too powerful?

FINAL VALIDATION:
1. Playtest all scenarios one more time
2. Verify metrics met:
   - [ ] Tutorial: 95%+ win rate
   - [ ] Easy: 80-90% win rate
   - [ ] Medium: 50-70% win rate
   - [ ] Hard: 30-50% win rate
   - [ ] Boss: 20-40% win rate
3. Verify durations appropriate
4. Collect final feedback:
   - [ ] Difficulty feels fair?
   - [ ] All weapons viable?
   - [ ] All abilities useful?
   - [ ] Heat management meaningful?
   - [ ] Enemies interesting?

SPECIFIC BALANCE CHECKS:

Check 1: Weapon Viability
- Are all 4 weapon types used?
- Does any weapon feel mandatory?
- Does any weapon feel useless?
- Target: Each weapon used in at least 30% of victories

Check 2: Ability Usage
- Are all 6 abilities used?
- Does any ability dominate strategy?
- Target: Each ability used in at least 40% of victories

Check 3: Heat Economy
- Do players frequently reach critical heat?
- Do players ever avoid using weapons due to heat?
- Target: Players reach 100+ heat in 30-40% of turns during intense combat

Check 4: Enemy Difficulty Progression
- Does each enemy type feel distinct?
- Is difficulty curve smooth?
- Target: Clear difference between Frigate < Destroyer < Battleship

Check 5: Combat Pacing
- Do battles feel too short/long?
- Is there downtime/boredom?
- Target: Constant engagement, tactical decisions each turn

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 6.1 Complete - Balance Tuning
- 📁 New Files: BalanceDebugger.cs, TestScenarioLoader.cs, CombatLogger.cs, balance JSONs
- 🧪 Playtesting: [X] sessions completed
- 🎮 Balance State: Tuned and validated
- 📊 Metrics: All scenarios meet target win rates
- ⏭️ Next: Step 6.2 - Bug Fixing & Edge Cases
- 🚧 Notes: [Key balance changes made, player feedback summary]

Begin implementation now.
```

---

### Step 6.2: Bug Fixing & Edge Cases

#### CLAUDE CODE PROMPT 6.2

```
CONTEXT:
Balance tuning complete. Now comprehensive bug hunting and edge case handling.

OBJECTIVE:
Identify and fix all remaining bugs, handle edge cases gracefully, improve game stability.

CRITICAL EDGE CASES TO TEST:

1. **Ship Destroyed Mid-Turn**
   - Ship queues actions during Command phase
   - Ship destroyed during Simulation phase (by enemy fire)
   - Expected: All queued actions cancel, no errors
   - Test: Verify projectiles in flight don't cause null references

2. **Target Destroyed Before Projectile Hits**
   - Projectile in flight, target destroyed
   - Options:
     A) Projectile continues ballistic, misses
     B) Projectile self-destructs
   - Decision: Implement option A for missiles/torpedoes
   - Test: Verify no null references when projectile tries to hit

3. **Simultaneous Deaths**
   - Player and enemy both die same turn
   - Projectiles in flight from both
   - Expected: Defeat takes priority (player died)
   - Test: Verify GameManager handles correctly

4. **Heat Edge Cases**:
   - Heat exceeds max (150+) - should keep accumulating
   - Heat becomes negative (shouldn't happen) - clamp to 0
   - Emergency Cooling at 0 heat - should work (no harm)
   - Heat penalty at exactly threshold (80, 100, 120) - correct tier?

5. **Weapon Edge Cases**:
   - Fire weapon with 0 ammo - prevented, "NO AMMO" message
   - Fire weapon on cooldown - prevented, "ON COOLDOWN" message
   - Fire weapon out of arc - prevented, "OUT OF ARC" message
   - Fire weapon out of range - prevented, "OUT OF RANGE" message
   - All weapons in group unavailable - group button disabled

6. **Ability Edge Cases**:
   - Use ability on cooldown - prevented
   - Use ability with insufficient heat - prevented
   - Use Shield Boost at max shields - allowed, shields increase
   - Use Emergency Cooling at 0 heat - allowed, no effect
   - Use Overcharge with no weapons - allowed, but no benefit

7. **Point Defense Edge Cases**:
   - All PD turrets destroyed - ship defenseless
   - PD engages friendly projectile - shouldn't happen (TeamID check)
   - PD turret destroyed mid-engagement - engagement continues
   - 0 operational turrets - no crashes, just no interceptions

8. **Movement Edge Cases**:
   - Plan movement into obstacle - red projection, prevented
   - Plan movement beyond max range - clamped to max
   - Plan turn beyond max angle - clamped to max
   - Evasive Maneuver active - different constraints applied

9. **UI Edge Cases**:
   - No ship selected, press number key - no crash
   - Select destroyed ship - deselect automatically
   - Rapid clicking/input spam - no duplicate actions
   - Tooltip remains open when source destroyed - tooltip hides

10. **Multiplayer Edge Cases** (if applicable):
    - Not applicable for single-player MVP
    - Document for future

SYSTEMATIC BUG HUNT:

**Category A: Null Reference Errors**
- Search codebase for all `GetComponent` calls
- Verify null checks exist
- Add defensive programming where needed
- Test: Destroy objects, verify no null refs

**Category B: Index Out of Range**
- All array/list accesses checked
- Bounds validation before access
- Test: Empty lists, single-element lists

**Category C: Division by Zero**
- All division operations checked
- Verify denominators non-zero
- Test: Edge values (0, very small numbers)

**Category D: Race Conditions**
- Events firing during object destruction
- Unsubscribe from events in OnDestroy()
- Test: Rapidly destroy/spawn objects

**Category E: Memory Leaks**
- Pooled objects returned correctly
- Event handlers unsubscribed
- Coroutines stopped on destroy
- Test: Long play sessions (30+ minutes)

**Category F: UI State Bugs**
- UI doesn't update when should
- UI updates when shouldn't
- Stale data displayed
- Test: Rapid state changes

**Category G: Input Bugs**
- Input doesn't register
- Input registers multiple times
- Input in wrong phase
- Test: Spam all input types

**Category H: Audio/Visual Bugs**
- VFX don't spawn
- Audio doesn't play
- VFX/audio play multiple times
- Test: Trigger all effects rapidly

TESTING PROCEDURE:

1. **Stress Test**: Spam all inputs randomly for 5 minutes
2. **Long Session Test**: Play for 30 minutes without restart
3. **Edge Case Checklist**: Manually test all edge cases listed above
4. **Victory/Defeat Test**: Trigger win/loss in various ways
5. **Performance Test**: Monitor memory, CPU, FPS over time

CREATE: Assets/Scripts/Testing/EdgeCaseTester.cs
- Automated edge case testing
- Methods for each edge case
- Logs results to console
- Run before each build

DELIVERABLES:
1. All critical edge cases handled
2. EdgeCaseTester.cs automated tests
3. Bug fix documentation (bug → cause → fix)
4. Defensive programming added throughout codebase
5. Null checks, bounds checks, validation everywhere
6. Memory leak check passed (30min+ session stable)
7. Stress test passed (5min input spam, no crashes)
8. Updated test suite (all 150+ tests pass)

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 6.2 Complete - Bug Fixing & Edge Cases
- 📁 New Files: EdgeCaseTester.cs
- 🔧 Modified: [Files with bug fixes]
- 🐛 Bugs Fixed: [Count]
- 🧪 Edge Cases: All handled gracefully
- 🎮 Stability: High (no crashes in 30min session)
- ⏭️ Next: Step 6.3 - Final MVP Validation
- 🚧 Notes: [Critical bugs fixed, remaining minor issues]

Begin implementation now.
```

---

### Step 6.3: Final MVP Validation & Release

#### MANUAL TASK 6.3: MVP Checklist
**Time**: 2-3 hours

**Complete MVP Validation:**

Walk through entire MVP success criteria from GDD:

**Core Systems Check**:
- [ ] Movement system works (Bezier curves, projections)
- [ ] Heat system works (accumulation, cooling, penalties)
- [ ] Weapon system works (all 4 types fire correctly)
- [ ] Ability system works (all 6 abilities functional)
- [ ] Point Defense works (intercepts projectiles)
- [ ] AI works (3 difficulty levels)
- [ ] Victory/Defeat works (correct conditions)

**Polish Check**:
- [ ] UI clear and readable
- [ ] VFX present for all actions
- [ ] Audio present for all actions
- [ ] Heat visualization works
- [ ] Tooltips helpful
- [ ] Help screen accessible

**Balance Check**:
- [ ] Tutorial winnable by beginners
- [ ] Medium scenarios provide challenge
- [ ] Hard scenarios difficult but fair
- [ ] All weapons viable
- [ ] All abilities useful
- [ ] Heat management meaningful

**Performance Check**:
- [ ] 60 FPS maintained (4 ships, full combat)
- [ ] No crashes (30min+ session)
- [ ] No memory leaks
- [ ] Loading times acceptable

**Bug Check**:
- [ ] No critical bugs
- [ ] All edge cases handled
- [ ] No console errors/warnings
- [ ] UI responsive

**Playability Check** (Have friend play):
- [ ] Tutorial teaches basics
- [ ] Controls intuitive
- [ ] Difficulty fair
- [ ] Combat engaging
- [ ] 10-15 min per encounter
- [ ] Want to play again

If ALL checks pass → **MVP COMPLETE!**

---

#### FINAL STATUS UPDATE

```markdown
# IMPLEMENTATION_STATUS.md - FINAL

## Project: Hephaestus Space Combat Prototype
## Status: ✅ MVP COMPLETE

### Final Statistics
- **Total Development Time**: [X weeks]
- **Total Code Files**: ~80
- **Total Tests**: 150+
- **Test Pass Rate**: 100%
- **Target FPS**: 60 (achieved)
- **Playable Scenarios**: 7

### Completed Phases
- ✅ Phase 0: Foundation (Movement, Camera, Turn Management)
- ✅ Phase 1: Core Combat Infrastructure (Heat, Abilities)
- ✅ Phase 2: Weapon Systems (4 weapon types, Projectiles, Targeting)
- ✅ Phase 3: Point Defense System
- ✅ Phase 4: Enemy AI (3 difficulty levels)
- ✅ Phase 5: Polish (UI, VFX, Audio, Shaders)
- ✅ Phase 6: Balance, Bug Fixes, MVP Validation

### MVP Success Criteria
✅ Players can complete 1v2 combat encounter
✅ Heat management creates meaningful decisions
✅ Ability timing creates strategic depth
✅ Movement planning feels intuitive
✅ Combat feels tactical, not random
✅ Point Defense system understandable
✅ AI provides reasonable challenge
✅ Players understand systems without extensive tutorial
✅ No major bugs
✅ 10-15 minute playtime per encounter
✅ Players want to replay

### Known Minor Issues
- [List any minor bugs or polish items for future]

### Future Enhancements (Post-MVP)
- Campaign mode
- More ship types
- Ship customization
- Environmental hazards
- Advanced weapons
- Multiplayer co-op

### Next Steps
1. Create builds (Windows, Mac, Linux)
2. Publish to itch.io or similar
3. Gather community feedback
4. Plan post-MVP features based on feedback

## 🎉 PROJECT COMPLETE - MVP DELIVERED
```

---

## APPENDIX: Quick Reference

### File Structure Summary
```
DeadLock_Controls/
├── Assets/
│   ├── Scripts/
│   │   ├── Movement/ (Phase 0)
│   │   ├── Combat/
│   │   │   ├── HeatManager.cs
│   │   │   ├── Abilities/ (6 abilities)
│   │   │   ├── Weapons/ (4 weapon types)
│   │   │   ├── Projectiles/ (3 projectile types)
│   │   │   ├── Targeting/
│   │   │   └── PointDefense/
│   │   ├── AI/ (3 behavior types)
│   │   ├── Management/
│   │   ├── UI/
│   │   ├── VFX/
│   │   ├── Audio/
│   │   ├── Debug/
│   │   └── Testing/
│   ├── Prefabs/
│   ├── Tests/
│   └── [Audio, VFX, Materials, Shaders folders]
└── PROJECT_STATUS.md, IMPLEMENTATION_STATUS.md
```

### Total Implementation Time Estimate
- **Sequential Development**: 15-16 weeks
- **With Parallel Development**: 9-10 weeks
- **With multiple devs/agents**: Potentially 6-8 weeks

### Testing Summary
- Unit Tests: ~150
- Integration Tests: ~40
- Manual Test Scenarios: ~50
- Performance validated throughout

---

## Document Complete!

This implementation guide provides:
1. ✅ Step-by-step instructions for entire prototype
2. ✅ Manual tasks clearly separated from code tasks
3. ✅ Detailed Claude Code prompts for each step
4. ✅ Unit testing requirements for each step
5. ✅ Manual testing instructions for each step
6. ✅ Status updates to track progress
7. ✅ Parallel development opportunities identified
8. ✅ Integration checkpoints between parallel work
9. ✅ Balance tuning methodology
10. ✅ MVP completion criteria

**Ready to begin implementation!**   **Small Explosion** (projectile destroyed):
   - Pop/crack sound
   - Brief, sharp
   - Mid-frequency
   - Reference: Firecracker, small explosion

   **Large Explosion** (ship destroyed):
   - Deep, rumbling boom
   - Multi-layered (primary explosion + secondary pops)
   - Long tail (echoing, rumbling)
   - Low-frequency emphasis
   - Reference: Movie explosions, depth charges

4. Point Defense Sounds:
   - Rapid burst (machine gun-like)
   - High rate of fire sound
   - Brief (0.2-0.3 seconds)
   - Multiple turrets = layered sound (not perfectly synchronized)

5. Ability Sounds:

   **Emergency Cooling**:
   - Hissing steam/venting
   - Pressure release
   - Mechanical whir (coolant pumps)

   **Shield Boost**:
   - Power-up sound
   - Energy surge
   - Humming (sustained while active)

   **Evasive Maneuver**:
   - Engine roar/boost
   - Thruster burst
   - Whoosh

   **Overcharge Weapons**:
   - Electrical crackling
   - Power routing sounds
   - Increasing pitch/intensity

   **Sensor Burst**:
   - Sonar ping
   - Electronic beep/chirp
   - Expanding wave sound

   **PD Override**:
   - Mechanical spin-up
   - Turrets activating
   - System activation beep

6. UI Sounds:

   **Button Click**: Subtle click/tap
   **Button Hover**: Soft tick/beep
   **Confirm Action**: Positive chime
   **Error/Warning**: Buzzer/warning beep
   **Turn Start**: Transition sound
   **Victory**: Triumphant fanfare
   **Defeat**: Somber tone

7. Background Music:

   **Combat Music**:
   - Tense, driving rhythm
   - Orchestral or electronic
   - Builds intensity during combat
   - Loop seamlessly
   - Reference: Battlestar Galactica soundtrack, Homeworld soundtrack

   **Victory Music**:
   - Triumphant, uplifting
   - Short (20-30 seconds)
   - Resolves to calm

   **Defeat Music**:
   - Somber, melancholic
   - Short (20-30 seconds)
   - Downbeat resolution

8. 3D Audio & Spatialization:
   - Weapon sounds use 3D positioning
   - Volume decreases with distance
   - Stereo panning based on direction
   - Explosions have wider spread (audio source spread parameter)
   - UI sounds always centered (2D)

9. Audio Mixing:
   - Music: -6dB to -10dB (background layer)
   - Important SFX (weapons, explosions): 0dB (full volume)
   - Ambient SFX (engines): -12dB to -15dB
   - UI sounds: -3dB
   - Prevent audio clipping with limiting

10. Audio Source Pooling:
    - Pre-create 20 AudioSource components
    - Reuse sources (don't create/destroy)
    - If all sources busy, oldest sound gets interrupted
    - Priority system: Explosions > Weapons > UI > Ambient

11. CREATE: Assets/Scripts/Audio/MusicController.cs
    - Manages music transitions
    - Methods:
      * void PlayCombatMusic()
      * void PlayVictoryMusic()
      * void PlayDefeatMusic()
      * void CrossFade(AudioClip newTrack, float fadeTime)

12. Integration Points:
    - WeaponSystem.Fire() → AudioManager.PlaySFX(weaponType)
    - Projectile.OnHit() → AudioManager.PlaySFX(impactType, hitPosition)
    - Ship.Die() → AudioManager.PlaySFX(LargeExplosion, shipPosition)
    - Ability.Execute() → AudioManager.PlaySFX(abilityType)
    - UIButton.OnClick() → AudioManager.PlaySFX(UIClick)
    - GameManager.OnVictory() → MusicController.PlayVictoryMusic()
    - GameManager.OnDefeat() → MusicController.PlayDefeatMusic()

DELIVERABLES:
1. AudioManager.cs with SFX management and pooling
2. MusicController.cs for music transitions
3. Audio clips for all sound types:
   - 4 weapon firing sounds
   - Shield/hull impact sounds
   - Small/large explosion sounds
   - PD firing sound
   - 6 ability sounds
   - UI sounds (click, hover, confirm, error)
   - Combat music (looping track)
   - Victory/defeat music stings
4. 3D audio configuration
5. Audio mixer with proper volume levels
6. Integration with all game systems
7. Test scene demonstrating all audio
8. Documentation of audio sources and usage

MANUAL TESTING:

Test all audio in context:

SCENARIO A: Weapon Audio
1. Fire each weapon type
2. Verify:
   - [ ] Each weapon has distinct sound
   - [ ] Sounds positioned in 3D space
   - [ ] Volume decreases with distance
   - [ ] Rapid firing doesn't cause clipping

SCENARIO B: Impact Audio
1. Fire at enemy with shields
   - [ ] Shield hit sound plays
2. Deplete shields, fire at hull
   - [ ] Hull impact sound plays (distinct from shield)
3. Destroy ship
   - [ ] Large explosion sound (dramatic, multi-layered)

SCENARIO C: Combat Audio Mix
1. Full combat scenario:
   - Player firing weapons
   - Enemy firing weapons
   - PD engaging projectiles
   - Multiple impacts
   - Background music playing
2. Verify:
   - [ ] All sounds audible
   - [ ] No severe clipping or distortion
   - [ ] Music doesn't overpower SFX
   - [ ] Important sounds (explosions) audible over others

SCENARIO D: Ability Audio
1. Use each ability:
   - [ ] Emergency Cooling: Venting/hissing
   - [ ] Shield Boost: Power-up sound
   - [ ] Evasive Maneuver: Engine boost
   - [ ] Overcharge: Electrical crackling
   - [ ] Sensor Burst: Sonar ping
   - [ ] PD Override: Turret spin-up

SCENARIO E: UI Audio
1. Navigate menus
   - [ ] Button hover: Subtle tick
   - [ ] Button click: Click sound
2. Invalid action (e.g., fire weapon on cooldown)
   - [ ] Error sound plays

SCENARIO F: Music Transitions
1. Combat starts
   - [ ] Combat music begins
   - [ ] Loops seamlessly
2. Player wins
   - [ ] Combat music fades out
   - [ ] Victory music plays
3. Restart combat
   - [ ] Combat music resumes

SCENARIO G: 3D Audio Positioning
1. Enemy ship to player's left
2. Enemy fires weapon
3. Verify:
   - [ ] Sound comes from left speaker
4. Rotate camera
   - [ ] Sound panning follows ship position (not camera)

SCENARIO H: Audio Performance
1. Heavy combat (4 ships, all firing)
2. 20+ sounds playing simultaneously
3. Verify:
   - [ ] No audio stuttering
   - [ ] No significant frame drops
   - [ ] Oldest sounds interrupted gracefully if source pool exhausted

DELIVERABLES CHECKLIST:
- [ ] AudioManager.cs implemented
- [ ] MusicController.cs implemented
- [ ] All weapon sounds added
- [ ] All impact sounds added
- [ ] All ability sounds added
- [ ] UI sounds added
- [ ] Music tracks added
- [ ] Audio mixer configured
- [ ] 3D audio working correctly
- [ ] Integration with all systems complete
- [ ] Test scene created

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 5.3 Complete - Audio System
- 📁 New Files: AudioManager.cs, MusicController.cs, audio clips
- 🔧 Modified: Weapon/Projectile/Ship/Ability/UI systems (audio integration)
- 🎮 Manual Test: All audio scenarios pass
- 🔊 Audio Mix: Balanced, no clipping
- ⏭️ Next: Phase 5 completion (wait for other tracks)
- 🚧 Parallel Status: Track C complete

Begin implementation now.
```

---

### Step 5.4: Heat Visualization & Shaders 🟡
**Parallel Track D**

#### CLAUDE CODE PROMPT 5.4

```
CONTEXT:
Phase 5 polish in progress. Track D handles heat visualization on ships using shaders and material changes.

OBJECTIVE:
Create visual feedback system that shows ship heat levels through glowing effects, venting particles, and material changes.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/VFX/HeatGlowEffect.cs
   - Component on each Ship
   - Dynamically changes ship appearance based on heat
   - Properties:
     * shipRenderer: Renderer (ship mesh)
     * heatMaterial: Material (shader with heat parameters)
     * ventParticles: ParticleSystem[] (venting effects)
     * heatManager: HeatManager (reference)
   
   - Methods:
     * void Update() - Monitor heat, update visuals
     * void SetHeatGlow(float heatLevel) - Adjust emission
     * void UpdateVentParticles(float heatLevel)
     * void ApplyHeatDistortion(bool enabled)

2. Heat Visualization Tiers:

   **Safe (0-59 heat)**:
   - Normal ship appearance
   - No glow
   - No particles
   - Standard material

   **Minor (60-79 heat)**:
   - Slight yellow glow from vents/engines
   - Subtle emissive color
   - Minimal heat distortion
   - Few vent particles (slow rate)

   **Moderate (80-99 heat)**:
   - Orange glow increasing
   - Heat haze/distortion effect
   - Vent particles increasing
   - Emissive intensity: 0.5

   **Severe (100-119 heat)**:
   - Red glow, prominent
   - Heavy heat distortion
   - Many vent particles
   - Occasional sparks from systems
   - Emissive intensity: 1.0

   **Critical (120-149 heat)**:
   - Flashing red glow
   - Severe distortion
   - Heavy venting (steam/smoke)
   - Sparks from multiple locations
   - Fire effects starting
   - Emissive intensity: 1.5 (pulsing)

   **Catastrophic (150+ heat)**:
   - Intense flashing red
   - Ship appears to be breaking down
   - Fire effects multiple locations
   - Electrical arcing
   - Heavy smoke
   - Warning: Imminent destruction

3. CREATE Heat Shader:
   - Shader: "Custom/ShipHeatShader"
   - Based on Standard shader with additions:
   
   Properties:
   - _MainTex: Base texture
   - _EmissionMap: Emission mask (where glow appears)
   - _HeatLevel: Float (0-1, controls intensity)
   - _HeatColor: Color (yellow → orange → red gradient)
   - _DistortionAmount: Float (heat haze effect)
   
   Shader logic:
   - Lerp emission color based on _HeatLevel
   - Apply distortion to UVs for heat haze
   - Pulse emission at high heat levels
   - Support for HDR emission (bloom effect)

4. Vent Particle Systems:
   - Position particles at ship's heat vent locations
   - Particle rate scales with heat:
     * 0-59 heat: 0 particles/sec
     * 60-79 heat: 5 particles/sec
     * 80-99 heat: 15 particles/sec
     * 100-119 heat: 30 particles/sec
     * 120+ heat: 60 particles/sec
   
   - Particle appearance:
     * Yellow-white at low heat
     * Orange at medium heat
     * Red-orange at high heat
   
   - Particle behavior:
     * Emit upward (or away from vent)
     * Fade over lifetime
     * Slight expansion
     * Random velocity variation

5. Fire Effects (Critical/Catastrophic):
   - Small fire particle systems at damage points
   - Appear when heat > 120
   - Multiple fire sources at 150+ heat
   - Fire colors: Orange/red with smoke
   - Audio: Crackling fire loop (from Track C)

6. Electrical Arcing (Catastrophic):
   - Line renderer effects
   - Arc between ship components
   - Sporadic (not constant)
   - Blue-white color
   - Brief duration (0.1-0.2s per arc)
   - Audio: Electric zap (from Track C)

7. CREATE: Assets/Scripts/VFX/HeatDistortionEffect.cs
   - Post-processing effect (optional)
   - Screen-space distortion around high-heat ships
   - Simulates heat haze in space
   - Properties:
     * distortionIntensity: float
     * distortionScale: float
   
   - Only active for ships with heat > 80

8. Integration with Emergency Cooling:
   - When Emergency Cooling used:
     * Trigger large vent particle burst
     * Glow rapidly fades (animate over 1 second)
     * Blue cooling particles (different from heat venting)
     * Audio: Hissing/venting (from Track C)

9. Performance Optimization:
   - Use material property blocks (not material instances)
   - Particle system limits (max 1000 particles per ship)
   - LOD: Reduce particle count for distant ships
   - Disable effects for off-screen ships

10. Integration Points:
    - HeatManager.OnHeatChanged → HeatGlowEffect.SetHeatGlow()
    - HeatManager.OnHeatTierChanged → HeatGlowEffect.UpdateVentParticles()
    - Ability.EmergencyCooling.Execute() → HeatGlowEffect.TriggerCoolingBurst()

DELIVERABLES:
1. HeatGlowEffect.cs component
2. HeatDistortionEffect.cs (optional post-processing)
3. Custom heat shader (ShipHeatShader)
4. Heat vent particle system prefabs (multiple tiers)
5. Fire effect prefabs (critical heat)
6. Electrical arc effect prefabs (catastrophic)
7. Material setup for ship meshes (using heat shader)
8. Integration with HeatManager
9. Test scene demonstrating all heat levels
10. Documentation of heat visualization system

MANUAL TESTING:

Test heat visualization at all tiers:

SCENARIO A: Heat Progression
1. Start with ship at 0 heat
   - [ ] Normal appearance, no glow
2. Increase heat to 65 (Minor)
   - [ ] Slight yellow glow appears
   - [ ] Few vent particles
3. Increase heat to 85 (Moderate)
   - [ ] Orange glow increases
   - [ ] More particles
   - [ ] Slight heat distortion visible
4. Increase heat to 105 (Severe)
   - [ ] Red glow prominent
   - [ ] Heavy venting
   - [ ] Sparks appear
5. Increase heat to 125 (Critical)
   - [ ] Flashing red glow
   - [ ] Fire effects start
   - [ ] Heavy smoke
6. Increase heat to 160 (Catastrophic)
   - [ ] Intense effects
   - [ ] Multiple fires
   - [ ] Electrical arcing
   - [ ] Ship looks like it's breaking down

SCENARIO B: Emergency Cooling Visual
1. Ship at 140 heat (Critical, red glow)
2. Use Emergency Cooling ability
3. Verify:
   - [ ] Large burst of blue cooling particles
   - [ ] Glow rapidly fades over 1 second
   - [ ] Heat drops to 90 (Moderate glow)
   - [ ] Venting particles decrease

SCENARIO C: Combat Heat Management
1. Full combat scenario
2. Player fires weapons repeatedly
3. Observe heat visualization:
   - [ ] Glow increases as heat accumulates
   - [ ] Venting increases
   - [ ] Visual feedback clear and readable
   - [ ] Doesn't obscure ship silhouette completely

SCENARIO D: Multiple Ships
1. Scene with 4 ships at different heat levels:
   - Ship A: 20 heat (safe)
   - Ship B: 75 heat (minor)
   - Ship C: 110 heat (severe)
   - Ship D: 140 heat (critical)
2. Verify:
   - [ ] Each ship's heat level visually distinct
   - [ ] Can identify hottest ship at a glance
   - [ ] Effects don't conflict or overlap

SCENARIO E: Performance Test
1. 4 ships all at critical heat (120+)
2. All have active fire effects, venting, sparks
3. Full combat ongoing
4. Verify:
   - [ ] 60 FPS maintained
   - [ ] No stuttering when effects spawn
   - [ ] Particle systems don't cause lag

SCENARIO F: Shader Quality
1. Examine heat shader up close
2. Verify:
   - [ ] Smooth color transitions (no banding)
   - [ ] Glow appears only on appropriate areas (vents, engines)
   - [ ] Distortion effect subtle, not overwhelming
   - [ ] HDR bloom works correctly (if enabled)

SCENARIO G: Integration with Gameplay
1. Player manages heat tactically
2. Verify:
   - [ ] Heat visualization helps decision-making
   - [ ] Easy to see when approaching danger zones
   - [ ] Emergency Cooling effect satisfying
   - [ ] Overheating feels dangerous (visually)

DELIVERABLES CHECKLIST:
- [ ] HeatGlowEffect.cs implemented
- [ ] Custom heat shader created
- [ ] Vent particle systems (6 tiers)
- [ ] Fire effects (critical/catastrophic)
- [ ] Electrical arc effects
- [ ] Cooling burst effect
- [ ] Material setup for ships
- [ ] Integration with HeatManager
- [ ] Performance optimized
- [ ] Test scene created

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 5.4 Complete - Heat Visualization & Shaders
- 📁 New Files: HeatGlowEffect.cs, HeatDistortionEffect.cs, ShipHeatShader, particle prefabs
- 🔧 Modified: Ship prefabs (heat shader materials), HeatManager (visual integration)
- 🎮 Manual Test: All heat tiers visualized correctly
- ⚡ Performance: 60 FPS with full heat effects
- ⏭️ Next: Phase 5 completion (wait for other tracks)
- 🚧 Parallel Status: Track D complete

Begin implementation now.
```

---

### Step 5.5: Phase 5 Four-Way Integration ⚫
**Sequential** - After ALL of 5.1, 5.2, 5.3, 5.4 complete

#### CLAUDE CODE PROMPT 5.5

```
CONTEXT:
Phase 5 parallel development complete:
- Track A: UI/UX Polish (context-sensitive UI, tooltips, help screen)
- Track B: Visual Effects (weapon VFX, explosions, shield effects)
- Track C: Audio System (SFX, music, 3D audio)
- Track D: Heat Visualization (shaders, particles, glow effects)

OBJECTIVE:
Integrate all polish systems to create a cohesive, polished gameplay experience.

FILES TO INTEGRATE:
From Track A: UIManager, TooltipSystem, TurnHistoryLog, HelpScreen, UIAnimations
From Track B: VFXManager, ShieldEffect, all VFX prefabs
From Track C: AudioManager, MusicController, all audio clips
From Track D: HeatGlowEffect, heat shader, particle systems

INTEGRATION TASKS:

1. Synchronize VFX + Audio
   - Every VFX should have corresponding audio
   - When VFXManager spawns effect, AudioManager plays sound
   - Examples:
     * Rail gun fires → Muzzle flash VFX + Rail gun audio
     * Shield hit → Shield ripple VFX + Shield impact audio
     * Ship explodes → Large explosion VFX + Explosion audio
   - Timing: Audio slightly precedes or matches VFX (not delayed)

2. UI + Audio Integration
   - Every UI interaction has sound
   - Button hover → Hover sound
   - Button click → Click sound
   - Panel opens → Slide/whoosh sound
   - Warning message → Warning beep
   - Victory screen → Victory music
   - Defeat screen → Defeat music

3. Heat Visualization + UI Integration
   - Heat bar color matches ship glow color
   - When ship reaches danger zones, UI warning indicators
   - Tooltip on heat bar shows current visual tier
   - "OVERHEATING" warning flashes when critical

4. Complete Combat Experience Polish:
   ```
   Player Action → Multiple Feedback Layers:
   
   Example: Fire Rail Gun
   1. UI: Button highlight, heat bar updates (+15)
   2. VFX: Muzzle flash, tracer line
   3. Audio: Rail gun crack sound
   4. Outcome: 
      - If hit: Shield ripple VFX + Shield impact audio + Damage number
      - If miss: No additional feedback
   
   Example: Use Emergency Cooling
   1. UI: Ability icon flashes, heat bar drops rapidly
   2. VFX: Blue venting particles burst, ship glow fades
   3. Audio: Hissing/venting sound
   4. Ship: Heat drops by 50, visual tier changes
   ```

5. Victory/Defeat Polish:
   - When victory triggered:
     * Slow-motion (optional, Time.timeScale = 0.3 for 2 seconds)
     * Screen flash (white)
     * Victory VFX (fireworks, celebration particles)
     * Victory audio (fanfare)
     * UI: Victory screen fades in with animation
   
   - When defeat triggered:
     * Camera shake (heavy)
     * Screen flash (red)
     * Player ship large explosion VFX
     * Defeat audio (somber sting)
     * UI: Defeat screen fades in

6. Tutorial Hints (Optional):
   - First time player does action, show tooltip
   - Examples:
     * First movement: "Use left-click to select ship, drag to move"
     * First weapon fire: "Press 1-4 to fire weapon groups"
     * First overheat: "WARNING: High heat reduces accuracy and speed"
   - Hints appear once, don't repeat

7. Settings Menu (Optional but recommended):
   - Master volume slider
   - Music volume slider
   - SFX volume slider
   - VFX quality dropdown (Low/Medium/High)
   - Controls remapping (future)
   - Save settings to PlayerPrefs

8. Performance Final Pass:
   - Profile with all systems active
   - Optimize VFX particle counts if needed
   - Reduce audio source pool if needed
   - Ensure 60 FPS with:
     * 4 ships in combat
     * All weapons firing
     * Full VFX and audio
     * Heat effects on all ships
     * All UI elements visible

TESTING REQUIREMENTS:

Integration Tests (Assets/Tests/PlayModeTests/Phase5IntegrationTests.cs):
1. Test_VFXAudioSync - Spawn VFX, verify audio plays
2. Test_UIAudioFeedback - Click button, verify click sound
3. Test_HeatVisualizationUISync - Heat change, verify UI + visuals match
4. Test_VictoryPolish - Trigger victory, verify VFX + audio + UI all activate
5. Test_DefeatPolish - Trigger defeat, verify all polish elements
6. Test_CompleteActionFeedback - Fire weapon, verify UI + VFX + audio all present
7. Test_EmergencyCoolingPolish - Use ability, verify all feedback layers
8. Test_TooltipAudioSync - Hover tooltip, verify no audio conflicts
9. Test_FullCombatPolish - Full combat, verify all systems working together
10. Test_PerformanceWithAllSystems - Profile, verify 60 FPS maintained

Manual Testing:

FULL POLISH VERIFICATION:

SCENARIO A: First-Time Player Experience
1. Start combat from fresh state
2. Observe:
   - [ ] Combat music begins
   - [ ] UI is clear and understandable
   - [ ] Tutorial hints appear (if implemented)
3. Plan movement:
   - [ ] Movement ghost clear
   - [ ] Path visualization smooth
4. Fire weapon:
   - [ ] UI shows heat increase
   - [ ] VFX: Weapon firing effects
   - [ ] Audio: Weapon sound
   - [ ] Target: Impact VFX + audio
   - [ ] Feedback: Clear hit/miss
5. Use ability:
   - [ ] UI: Icon pulses, heat updates
   - [ ] VFX: Ability effect spawns
   - [ ] Audio: Ability sound
   - [ ] Result: Ability takes effect visibly

SCENARIO B: Heat Management Experience
1. Fire weapons repeatedly
2. Observe heat progression:
   - [ ] Heat bar fills smoothly
   - [ ] Ship glow increases (yellow→orange→red)
   - [ ] Vent particles increase
   - [ ] Audio: Venting sounds increase
3. Reach critical heat (120+):
   - [ ] UI: "OVERHEATING" warning flashes
   - [ ] VFX: Fire effects, heavy venting
   - [ ] Audio: Warning alarm
   - [ ] Ship: Taking hull damage visible
4. Use Emergency Cooling:
   - [ ] UI: Heat drops dramatically
   - [ ] VFX: Blue cooling burst
   - [ ] Audio: Venting/hissing
   - [ ] Ship glow fades
   - [ ] Warning stops

SCENARIO C: Combat Satisfaction
1. Engage enemy in full combat
2. Evaluate "feel":
   - [ ] Weapons feel impactful (VFX + audio)
   - [ ] Hits feel satisfying
   - [ ] Damage feedback clear
   - [ ] Enemy destruction dramatic
   - [ ] Victory feels rewarding

SCENARIO D: UI Clarity Under Pressure
1. Intense combat (player vs 2 enemies)
2. Multiple things happening simultaneously
3. Verify UI remains readable:
   - [ ] Heat bar visible and clear
   - [ ] Ability cooldowns obvious
   - [ ] Weapon status clear
   - [ ] Enemy health visible
   - [ ] Tooltips don't obstruct view

SCENARIO E: Audio Experience
1. Full combat with headphones
2. Close eyes, listen:
   - [ ] Can identify weapon types by sound
   - [ ] 3D positioning clear (left/right)
   - [ ] Distance affects volume
   - [ ] Music doesn't overpower SFX
   - [ ] No audio clipping or distortion

SCENARIO F: Victory Experience
1. Destroy final enemy
2. Observe victory sequence:
   - [ ] Combat music fades
   - [ ] Victory VFX (optional)
   - [ ] Victory music plays
   - [ ] Screen animation (flash/fade)
   - [ ] Victory UI fades in smoothly
   - [ ] Statistics displayed clearly
   - [ ] Buttons responsive

SCENARIO G: Defeat Experience
1. Let enemy destroy player ship
2. Observe defeat sequence:
   - [ ] Large explosion VFX
   - [ ] Camera shake
   - [ ] Defeat music plays
   - [ ] Screen effect (red fade)
   - [ ] Defeat UI appears
   - [ ] Feels appropriately dramatic

SCENARIO H: Performance Final Test
1. Most demanding scenario:
   - 4 ships (player + 3 enemies)
   - All at critical heat (fire effects)
   - All firing weapons
   - 20+ projectiles in flight
   - PD systems active
   - Full UI visible
   - Music and SFX playing
2. Measure performance:
   - [ ] Stable 60 FPS
   - [ ] No stuttering
   - [ ] No audio crackling
   - [ ] UI responsive
   - [ ] Smooth gameplay

SCENARIO I: Settings (if implemented)
1. Open settings menu
2. Adjust volumes:
   - [ ] Master volume affects everything
   - [ ] Music volume affects music only
   - [ ] SFX volume affects SFX only
3. Change VFX quality:
   - [ ] Low: Reduced particles, simpler effects
   - [ ] Medium: Balanced
   - [ ] High: Full quality

DELIVERABLES:
1. Fully integrated polish systems
2. All feedback layers working together (UI + VFX + audio)
3. Victory/defeat sequences polished
4. Settings menu (optional)
5. Phase5IntegrationTests.cs with 10 tests
6. Final polish test scenes
7. Performance profiling report
8. Documentation of all polish elements

KNOWN ISSUES TO ADDRESS:
- VFX obscuring UI elements?
- Audio mixing issues?
- Performance drops in specific scenarios?
- UI readability problems?
- Feedback timing feels off?

Document any issues found and solutions implemented.

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 5.5 Complete - Phase 5 Integration
- ✅ Phase 5 Complete - Polish & Core Loop
- 🧪 Integration Tests: 10/10 passing
- 🧪 Total Tests: 150/150 passing
- 🎮 Manual Test: All polish scenarios pass
- ⚡ Performance: Stable 60 FPS with all systems
- 🎨 Polish Level: High (UI + VFX + Audio all integrated)
- ⏭️ Next: Phase 6 - Balance Tuning & MVP Completion
- 📊 Phase 5 Time: [Actual time for Phase 5]

Begin integration now.
```

---

**Phase 5 complete! Game is now fully polished with integrated VFX, audio, and UI.**

---

## Phase 6: MVP Completion (Week 16+)

### Step 6.1: Balance Tuning & Playtesting

#### MANUAL TASK 6.1: Set Up Playtesting

**Time**: Varies (ongoing throughout Step 6.1)

**Steps**:
1. Create test scenarios covering all difficulty levels
2. Recruit playtesters (friends, colleagues, online community)
3. Prepare feedback form:
   - Overall difficulty (1-10)
   - Most/least enjoyable aspects
   - Confusing elements
   - Balance issues (weapons, abilities, enemies)
   - Bugs encountered
4. Record playtests (screen capture)**Phase 3 complete! Point Defense system fully functional.**

---

## Phase 4: Enemy AI (Weeks 8-10)

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

---

### Phase 4 Parallel Development (3 Agents)

Phase 4 can be split into 3 parallel tracks:
- 🔵 Track A: AI Core Logic
- 🔴 Track B: Enemy Spawning & Management
- 🟢 Track C: Victory/Defeat System

---

### Step 4.1A: AI Core Logic 🔵
**Parallel Track A**

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
     * Allow think time (don't instant-decide, 1-2 second delay for realism)
   
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
**Parallel Track B** - Runs simultaneously with 4.1A and 4.1C

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
**Parallel Track C** - Runs simultaneously with 4.1A and 4.1B

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

**Phase 4 complete! Enemy AI fully functional with complete combat loop.**

---

## Phase 5: Polish & Core Loop (Weeks 12-15)

Phase 5 is highly parallelizable with 4 independent tracks.

### Pre-Phase Manual Setup

#### MANUAL TASK 5.0: Organize Assets
**Time**: 15 minutes

**Steps**:
1. Create folders:
   ```
   Assets/Audio/Music/
   Assets/Audio/SFX/Weapons/
   Assets/Audio/SFX/Impacts/
   Assets/Audio/SFX/UI/
   Assets/VFX/Weapons/
   Assets/VFX/Impacts/
   Assets/VFX/Environment/
   Assets/Shaders/
   ```

2. Source placeholder assets (free assets or Unity Asset Store):
   - Audio: Explosion sounds, weapon sounds, UI sounds
   - VFX: Particle effects, trail effects
   - Music: Combat track, victory/defeat stings

3. Update IMPLEMENTATION_STATUS.md for Phase 5

---

### Phase 5 Parallel Development (4 Agents)

Phase 5 splits into 4 completely independent tracks:
- 🔵 Track A: UI/UX Polish
- 🔴 Track B: Visual Effects
- 🟢 Track C: Audio System
- 🟡 Track D: Heat Visualization & Shaders

**All 4 can run simultaneously with zero dependencies**

---

### Step 5.1: UI/UX Polish 🔵
**Parallel Track A**

#### CLAUDE CODE PROMPT 5.1

```
CONTEXT:
Phases 1-4 complete. Core gameplay working. Now polishing UI for clarity and usability.

OBJECTIVE:
Improve all UI elements for better readability, usability, and visual appeal.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/UI/UIManager.cs
   - Central manager for all UI systems
   - Controls which panels visible based on selection context
   - Properties:
     * currentSelectionState: SelectionState enum
     * uiPanels: Dictionary<string, GameObject>
   
   - SelectionState enum:
     * NothingSelected
     * EnemySelected
     * PlayerShipSelected
   
   - Methods:
     * void UpdateUIState(SelectionState newState)
     * void ShowPanel(string panelName)
     * void HidePanel(string panelName)
     * void TogglePanel(string panelName)

2. Context-Sensitive UI States:

   **Nothing Selected (Minimal HUD)**:
   - Heat bar (top center)
   - Hull/Shield status (top left)
   - Turn number (top right)
   - Phase indicator (COMMAND / SIMULATION)
   - Ability bar (bottom, minimized icons only)
   
   **Enemy Selected**:
   - Everything from "Nothing Selected" PLUS:
   - Enemy status panel (right side):
     * Enemy name/type
     * Hull/Shield bars
     * Heat (if intel available)
   - Weapon Group Panel (right side):
     * 4 group buttons + Alpha Strike
     * Each shows heat cost, readiness, arc warnings
   
   **Player Ship Selected (Hephaestus)**:
   - Everything from "Nothing Selected" PLUS:
   - Weapon Configuration Panel (left side):
     * List of all weapons
     * Group assignment dropdowns
     * Heat costs, cooldowns, ammo
   - Ability Panel (bottom, expanded):
     * Full ability names and descriptions
     * Heat costs and cooldowns prominent

3. CREATE: Assets/Scripts/UI/TooltipSystem.cs
   - Shows detailed information on hover
   - Properties:
     * tooltipPanel: GameObject (UI panel)
     * tooltipText: TextMeshProUGUI
     * followMouse: bool
   
   - Methods:
     * void ShowTooltip(string content, Vector2 position)
     * void HideTooltip()
     * void UpdatePosition(Vector2 mousePos)
   
   - Tooltip content examples:
     * Hover weapon: "Rail Gun - Instant hit, 360° arc, 30 range, 15 heat"
     * Hover ability: "Emergency Cooling - Instantly reduce heat by 50. No heat cost. 4 turn cooldown."
     * Hover heat bar: Breakdown of heat sources

4. CREATE: Assets/Scripts/UI/TurnHistoryLog.cs (Optional)
   - Scrolling log of recent actions
   - Shows:
     * "Turn 5: Hephaestus fired Rail Guns at Frigate Alpha (40 damage)"
     * "Turn 5: Frigate Alpha destroyed"
     * "Turn 6: Hephaestus used Emergency Cooling (-50 heat)"
   - Maximum 10 entries, scrolls up
   - Positioned bottom-left corner

5. CREATE: Assets/Scripts/UI/HelpScreen.cs
   - Press H to show/hide
   - Displays all controls:
     * Movement controls
     * Camera controls
     * Weapon targeting controls
     * Ability hotkeys
     * Phase management
   - Keyboard shortcuts summary
   - Can be toggled during gameplay

6. UI Improvements to Existing Systems:

   **Heat Bar Enhancements**:
   - Clearer tier indicators (lines marking 60, 80, 100, 120)
   - Glow effect when in danger zones
   - Animation: Smooth fill/drain, not instant jumps
   - Tooltip: "Current: 80 (Moderate penalties)\nPlanned: +30 (from queued weapons)\nTotal: 110 (Severe penalties)"

   **Ability Bar Enhancements**:
   - Larger icons (more visible)
   - Hotkey numbers more prominent
   - Cooldown timer as circular progress bar (not just text)
   - Insufficient heat warning: Yellow glow/border
   - Animation: Pulse when ability becomes ready

   **Weapon Panels Enhancements**:
   - Group colors more vibrant (Blue, Red, Green, Yellow)
   - "OUT OF ARC" warning in bold red
   - "ON COOLDOWN" in gray with timer
   - Heat cost in large, bold text
   - Icons for weapon types (if available)

7. CREATE: Assets/Scripts/UI/UIAnimations.cs
   - Smooth transitions for panels
   - Methods:
     * IEnumerator FadeIn(GameObject panel, float duration)
     * IEnumerator FadeOut(GameObject panel, float duration)
     * IEnumerator SlideIn(GameObject panel, Vector2 direction, float duration)
     * IEnumerator ScalePulse(GameObject element, float scale, float duration)
   
   - Used for:
     * Panel appearances (fade in)
     * Button highlights (scale pulse on hover)
     * Warning indicators (flash/pulse)

8. Color Coding Consistency:
   - Friendly: Green/Cyan
   - Enemy: Red/Orange
   - Neutral: Gray/White
   - Warning: Yellow
   - Danger: Red (flashing)
   - Success: Green (brief flash)
   - Weapon Groups: Blue(1), Red(2), Green(3), Yellow(4)

9. Font & Readability:
   - Use high-contrast colors (white text on dark backgrounds)
   - Minimum font size: 14pt for body text, 18pt for important info
   - Headers: 24pt+
   - Drop shadows or outlines for text legibility

DELIVERABLES:
1. UIManager.cs for context-sensitive UI
2. TooltipSystem.cs with hover info
3. TurnHistoryLog.cs (optional)
4. HelpScreen.cs with controls summary
5. UIAnimations.cs for smooth transitions
6. Enhanced versions of existing UI:
   - HeatDisplay with better visuals
   - AbilityBarUI with larger icons and animations
   - WeaponConfigPanel with clearer layout
   - WeaponGroupPanel with bolder warnings
7. Updated UI prefabs with new styling
8. Documentation of UI states and transitions

MANUAL TESTING:

Test all UI states and transitions:

SCENARIO A: Context Switching
1. Start with nothing selected (minimal HUD)
2. Click enemy ship
   - [ ] WeaponGroupPanel slides in from right
   - [ ] Enemy status shows
3. Click player ship
   - [ ] WeaponGroupPanel disappears
   - [ ] WeaponConfigPanel slides in from left
4. Click empty space
   - [ ] All panels disappear
   - [ ] Return to minimal HUD

SCENARIO B: Tooltips
1. Hover over various elements:
   - Weapon in config panel
   - Ability icon
   - Heat bar
   - Group button
2. Verify:
   - [ ] Tooltip appears near mouse
   - [ ] Content accurate and helpful
   - [ ] Tooltip disappears when mouse leaves

SCENARIO C: Help Screen
1. Press H
   - [ ] Help screen appears (overlay)
   - [ ] All controls listed clearly
2. Press H again
   - [ ] Help screen disappears

SCENARIO D: Animations
1. Rapidly switch between selection states
2. Verify:
   - [ ] Panels animate smoothly (no instant pop-in)
   - [ ] No animation conflicts or glitches

SCENARIO E: Readability
1. Full combat scenario with all UI visible
2. Verify:
   - [ ] All text readable
   - [ ] No overlapping elements
   - [ ] Color coding consistent
   - [ ] Important info prominent

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 5.1 Complete - UI/UX Polish
- 📁 New Files: UIManager.cs, TooltipSystem.cs, TurnHistoryLog.cs, HelpScreen.cs, UIAnimations.cs
- 🔧 Modified: All UI components enhanced
- 🎮 Manual Test: All scenarios pass, UI clear and responsive
- ⏭️ Next: Phase 5 completion (wait for other tracks)
- 🚧 Parallel Status: Track A complete

Begin implementation now.
```

---

### Step 5.2: Visual Effects System 🔴
**Parallel Track B**

#### CLAUDE CODE PROMPT 5.2

```
CONTEXT:
Phase 5 polish in progress. Track B handles all visual effects.

OBJECTIVE:
Create comprehensive VFX system for weapons, impacts, explosions, and environmental effects.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/VFX/VFXManager.cs
   - Centralized VFX spawning and pooling
   - Properties:
     * vfxPools: Dictionary<VFXType, Queue<GameObject>>
     * activeVFX: List<GameObject>
   
   - VFXType enum:
     * RailGunMuzzleFlash
     * RailGunTracer
     * CannonMuzzleFlash
     * CannonProjectileTrail
     * MissileLaunch
     * MissileTrail
     * TorpedoLaunch
     * TorpedoTrail
     * ShieldHit
     * HullHit
     * SmallExplosion (projectile destroyed)
     * LargeExplosion (ship destroyed)
     * PDMuzzleFlash
     * PDTracer
     * EngineTrail
     * HeatVent (cooling effect)
   
   - Methods:
     * GameObject SpawnVFX(VFXType type, Vector3 position, Quaternion rotation)
     * void ReturnVFX(GameObject vfx)
     * void ClearAllVFX()

2. Weapon VFX:

   **Rail Gun**:
   - Muzzle flash: Bright white/blue flash (0.1s)
   - Tracer: Thin beam from weapon to target (0.2s fade)
   - Color: Cyan/white
   - Audio: Sharp crack

   **Newtonian Cannon**:
   - Muzzle flash: Orange fireball (0.2s)
   - Projectile: Glowing sphere with trail
   - Trail: Particle emitter, orange/yellow
   - Audio: Deep thump

   **Missile Battery**:
   - Launch: Smoke puff, fire burst
   - Projectile: Small missile mesh with bright trail
   - Trail: Particle emitter, orange contrail
   - Audio: Whoosh

   **Torpedo Launcher**:
   - Launch: Large smoke cloud, dramatic
   - Projectile: Large torpedo mesh with thick trail
   - Trail: Heavy particle emitter, white/gray contrail
   - Audio: Deep rumble

3. Impact VFX:

   **Shield Impacts**:
   - CREATE: Assets/Scripts/VFX/ShieldEffect.cs
   - Ripple effect from impact point
   - Shield bubble flashes brighter at hit location
   - Color: Blue/cyan
   - Particle burst (sparks)
   - Audio: Energy crackle

   **Hull Impacts**:
   - Sparks particle burst
   - Debris chunks (small mesh particles)
   - Scorch mark on hull (decal, optional)
   - Small fire/smoke if heavy damage
   - Audio: Metal impact, clang

4. Explosion VFX:

   **Small Explosion** (projectile destroyed by PD):
   - Particle burst (orange/yellow)
   - Brief light flash
   - Expanding smoke ring
   - Size: Small (1-2 unit radius)
   - Audio: Pop/crack

   **Large Explosion** (ship destroyed):
   - Multiple explosion bursts (staged)
   - Large fireball
   - Expanding debris field
   - Shockwave effect (distortion)
   - Light flash (screen flash for player ship)
   - Size: Large (5-10 unit radius)
   - Duration: 2-3 seconds
   - Audio: Deep boom, secondary explosions

5. Ship Effects:

   **Engine Trails**:
   - Continuous particle emitter from engine hardpoints
   - Color: Blue/white
   - Intensity increases with speed
   - Fades when ship stops

   **Heat Venting** (Emergency Cooling):
   - Blue particle burst from vents
   - Steam/vapor effect
   - Glowing vents cool down (material change)
   - Audio: Hissing steam

6. CREATE: Assets/Scripts/VFX/ShieldBubble.cs
   - Shader for shield visualization
   - Properties:
     * shieldStrength: float (0-1, controls opacity)
     * hitPoints: List<Vector3> (recent impact locations)
   
   - Visual:
     * Transparent bubble around ship (Fresnel shader)
     * Pulses brighter at impact points
     * Fades as shield depletes
     * Disappears when shields = 0

7. Environment Effects:

   **Space Dust**:
   - Subtle particle system
   - Gives sense of movement
   - White/gray particles drifting
   - Low density, atmospheric

   **Starfield** (optional):
   - Skybox or particle system
   - Distant stars
   - Adds depth to scene

8. Performance Optimization:
   - Object pooling for all VFX (don't instantiate/destroy)
   - Particle system limits:
     * Max particles per system: 100
     * Auto-disable when off-screen
   - LOD for distant effects (reduce quality)
   - Culling: Disable effects for off-screen ships

9. VFX Integration Points:

   Connect to existing systems:
   - WeaponSystem.Fire() → Spawn weapon VFX
   - Projectile.OnHit() → Spawn impact VFX
   - Ship.TakeDamage() → Spawn hit VFX (shield or hull)
   - Ship.Die() → Spawn explosion VFX
   - Ability.Execute() → Spawn ability VFX
   - PDTurret.Fire() → Spawn PD VFX

DELIVERABLES:
1. VFXManager.cs with pooling system
2. ShieldEffect.cs for shield visualization
3. VFX Prefabs for all effects:
   - 4 weapon types (muzzle + projectile)
   - Shield/hull impacts
   - Small/large explosions
   - Engine trails
   - Heat venting
   - PD effects
4. Particle systems configured and optimized
5. Shield bubble shader
6. Integration with weapon/projectile/ship systems
7. Test scene demonstrating all VFX
8. Documentation of VFX usage

MANUAL TESTING:

Test all VFX in combat:

SCENARIO A: Weapon VFX
1. Fire each weapon type
2. Verify:
   - [ ] Rail Gun: Muzzle flash + tracer
   - [ ] Cannon: Muzzle flash + projectile trail
   - [ ] Missile: Launch effect + contrail
   - [ ] Torpedo: Launch smoke + thick trail

SCENARIO B: Impact VFX
1. Fire at enemy with shields
   - [ ] Shield ripple effect on hit
   - [ ] Shield flashes brighter
   - [ ] Particle burst at impact point
2. Deplete shields, fire at hull
   - [ ] Sparks on hull impact
   - [ ] Debris particles
   - [ ] No shield effect

SCENARIO C: Explosion VFX
1. PD destroys missile
   - [ ] Small explosion at projectile location
   - [ ] Brief flash and smoke
2. Destroy enemy ship
   - [ ] Large, dramatic explosion
   - [ ] Multiple bursts
   - [ ] Debris field
   - [ ] Ship mesh disappears

SCENARIO D: Shield Visualization
1. Ship with full shields
   - [ ] Shield bubble visible (subtle)
2. Take damage
   - [ ] Shield pulses brighter at hit points
3. Shields deplete
   - [ ] Bubble fades out smoothly

SCENARIO E: Engine Trails
1. Ship at rest
   - [ ] Minimal/no engine trail
2. Ship moves
   - [ ] Engine trail appears
   - [ ] Intensity matches speed
3. Ship stops
   - [ ] Trail fades out

SCENARIO F: Performance
1. Full combat with 4 ships
2. All weapons firing
3. Multiple explosions
4. 20+ active VFX
5. Verify:
   - [ ] 60 FPS maintained
   - [ ] No visual glitches
   - [ ] VFX don't obscure gameplay

STATUS UPDATE:
Update IMPLEMENTATION_STATUS.md:
- ✅ Step 5.2 Complete - Visual Effects System
- 📁 New Files: VFXManager.cs, ShieldEffect.cs, ShieldBubble.cs, VFX prefabs
- 🔧 Modified: Weapon/Projectile/Ship systems (VFX integration)
- 🎮 Manual Test: All VFX scenarios pass
- ⚡ Performance: 60 FPS with full VFX
- ⏭️ Next: Phase 5 completion (wait for other tracks)
- 🚧 Parallel Status: Track B complete

Begin implementation now.
```

---

### Step 5.3: Audio System 🟢
**Parallel Track C**

#### CLAUDE CODE PROMPT 5.3

```
CONTEXT:
Phase 5 polish in progress. Track C handles all audio (SFX + music).

OBJECTIVE:
Create comprehensive audio system with weapon sounds, impact sounds, UI sounds, and music.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Audio/AudioManager.cs
   - Singleton managing all game audio
   - Properties:
     * musicSource: AudioSource (for background music)
     * sfxSources: List<AudioSource> (pool of sources for SFX)
     * audioClips: Dictionary<AudioType, AudioClip>
     * masterVolume: float
     * musicVolume: float
     * sfxVolume: float
   
   - AudioType enum:
     * RailGunFire
     * CannonFire
     * MissileLaunch
     * TorpedoLaunch
     * ShieldHit
     * HullHit
     * SmallExplosion
     * LargeExplosion
     * PDFire
     * EngineHum
     * AbilityActivate
     * EmergencyCooling
     * ShieldBoost
     * UIClick
     * UIConfirm
     * UIError
     * VictoryMusic
     * DefeatMusic
     * CombatMusic
   
   - Methods:
     * void PlaySFX(AudioType type, Vector3 position, float volumeScale = 1.0f)
     * void PlayMusic(AudioType type, bool loop = true)
     * void StopMusic(float fadeOutTime = 1.0f)
     * AudioSource GetAvailableSFXSource()
     * void SetMasterVolume(float volume)
     * void SetMusicVolume(float volume)
     * void SetSFXVolume(float volume)

2. Weapon Sounds:

   **Rail Gun**:
   - Sharp, high-pitched crack
   - Electronic whine
   - Brief, punchy
   - Reference: Laser rifle, railgun sound effects

   **Newtonian Cannon**:
   - Deep, resonant thump
   - Low-frequency boom
   - Mechanical clank (loading mechanism)
   - Reference: Tank cannon, artillery

   **Missile Battery**:
   - Whoosh/swoosh (launch)
   - Rocket engine sound (while in flight, fading with distance)
   - Reference: Rocket launch sounds

   **Torpedo Launcher**:
   - Deep rumble (launch)
   - Heavy mechanical sound
   - Compressed air release
   - Low hum (torpedo in flight)
   - Reference: Submarine torpedo sounds

3. Impact & Explosion Sounds:

   **Shield Impact**:
   - Energy crackle
   - Electric zap
   - High-pitched ping
   - Reference: Force field hit, electric arc

   **Hull Impact**:
   - Metal clang
   - Structural stress sound
   - Debris rattle
   - Reference: Metal impact, armor hit

   **Small Explosion** (projectile# Hephaestus - Step-by-Step Implementation Guide

## Overview
This guide provides a complete, step-by-step workflow for implementing the Hephaestus prototype. Each step includes manual tasks, Claude Code prompts, testing procedures, and status tracking to enable seamless handoffs between development sessions.

**Total Implementation Time**: 9-10 weeks with parallel development  
**Current Phase**: Phase 0 Complete (Movement, Camera, Turn Management)

---

## How to Use This Guide

### Step Format
Each step follows this structure:
1. **Prerequisites** - What must be complete before starting
2. **Manual Tasks** - Things you do in Unity
3. **Claude Code Prompt** - Copy/paste prompt for Claude Code
4. **Testing Instructions** - How to verify it works
5. **Status Update** - What to document in PROJECT_STATUS.md

### Parallel Development Workflow
- Steps marked with 🔵 can run in parallel with 🔴 steps
- Steps marked with 🟢 can run in parallel with 🔵 and 🔴 steps
- Steps marked with ⚫ must be done sequentially after parallel work completes

### Status Document
After each step, update `PROJECT_STATUS.md` with:
- ✅ What was completed
- 📁 New files created
- 🔧 Modified files
- 🧪 Test results
- ⏭️ Next step number
- 🚧 Any blockers or notes

---

## Phase 0: Foundation ✅ COMPLETE

Your existing systems are working:
- Movement system with Bezier curves
- Camera system (Homeworld-style)
- Turn management (Command/Simulation phases)
- Basic ship selection and input

**Confirm these work before proceeding to Phase 1.**

---

## Phase 1: Core Combat Infrastructure (Weeks 1-2)

### Pre-Phase Manual Setup

#### MANUAL TASK 1.0: Create Project Structure
**Time**: 10 minutes

**Steps**:
1. In Unity, create new folders:
   ```
   Assets/Scripts/Combat/
   Assets/Scripts/Combat/Abilities/
   Assets/Scripts/UI/
   Assets/Tests/
   Assets/Prefabs/UI/
   ```

2. Install Unity Test Framework:
   - Window → Package Manager
   - Search "Test Framework"
   - Install if not already installed

3. Create test assembly:
   - Right-click `Assets/Tests/` → Create → Testing → Tests Assembly Folder
   - Name it "PlayModeTests"

4. Create `IMPLEMENTATION_STATUS.md` in project root:
   ```markdown
   # Hephaestus - Implementation Status
   
   ## Current Phase: 1.1 - Heat System
   ## Last Updated: [Date]
   
   ### Completed Steps
   - Phase 0: Foundation ✅
   
   ### In Progress
   - Step 1.1: Heat System Foundation
   
   ### Blockers
   None
   
   ### Next Steps
   - Complete Step 1.1
   - Begin Step 1.2 (can run parallel)
   ```

**Status Update**: Document folder structure created

---

### Step 1.1: Heat System Foundation 🔵
**Parallel Track A** - Can run parallel with Step 1.2

**Prerequisites**: 
- Phase 0 complete
- Project structure created

---

#### CLAUDE CODE PROMPT 1.1

```
CONTEXT:
I'm implementing Phase 1 of the Hephaestus space combat prototype. We have a working movement system with turn-based gameplay (Command/Simulation phases), but no combat mechanics yet.

Existing files you'll need to integrate with:
- Assets/Scripts/Movement/Ship.cs - Handles movement planning and execution
- Assets/Scripts/Movement/TurnManager.cs - Manages Command/Simulation phase transitions
- Assets/Scripts/Movement/MovementController.cs - Player input handling

OBJECTIVE:
Implement the Heat System - the core resource management mechanic for the game. Heat is generated by weapons and abilities, accumulates over time, and applies escalating penalties if it gets too high.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Combat/HeatManager.cs
   - Component that attaches to Ship GameObject
   - Properties:
     * MaxHeat: float (default 150)
     * CurrentHeat: float (tracks actual heat, starts at 0)
     * PlannedHeat: float (preview of heat from queued actions)
     * PassiveCooling: float (default 20, applied each turn)
   
   - Heat Tiers (enum):
     * Safe: 0-59 (no penalties)
     * Minor: 60-79 (-10% accuracy)
     * Moderate: 80-99 (-20% accuracy, -10% speed, sensor flicker)
     * Severe: 100-119 (-40% accuracy, -25% speed, systems glitching)
     * Critical: 120-149 (-60% accuracy, -40% speed, 5 hull dmg/turn)
     * Catastrophic: 150+ (-80% accuracy, -50% speed, 20 hull dmg/turn)
   
   - Methods:
     * AddPlannedHeat(float amount) - Add to planned heat (for UI preview)
     * CommitPlannedHeat() - Transfer planned to current heat
     * ClearPlannedHeat() - Reset planned heat (if action cancelled)
     * ApplyPassiveCooling() - Subtract PassiveCooling from CurrentHeat (called by TurnManager)
     * InstantCooling(float amount) - Immediate heat reduction (for Emergency Cooling ability)
     * GetCurrentTier() - Return current HeatTier enum
     * GetPenalties() - Return HeatPenalties struct with current multipliers
   
   - Events:
     * OnHeatChanged(float newHeat)
     * OnHeatTierChanged(HeatTier newTier)
   
   - HeatPenalties struct:
     * AccuracyMultiplier (1.0 to 0.2)
     * SpeedMultiplier (1.0 to 0.5)
     * HullDamagePerTurn (0 to 20)
     * SensorFlicker (bool)

2. CREATE: Assets/Scripts/UI/HeatDisplay.cs
   - UI component showing three-layer heat bar
   - Layers:
     * Current Heat: Solid color (green→yellow→orange→red based on tier)
     * Planned Heat: Semi-transparent overlay showing current + planned
     * Preview Heat: Very light overlay (only on hover)
   - Numeric display: "40/150" with "+20 Planned" if applicable
   - Tooltip on hover: Breakdown of heat sources
   - Position: Top-center, always visible
   - Color transitions smooth between tiers

3. MODIFY: Assets/Scripts/Movement/Ship.cs
   - Add combat properties:
     * MaxHull (float, default 500)
     * CurrentHull (float, starts at MaxHull)
     * MaxShields (float, default 200)
     * CurrentShields (float, starts at MaxShields)
     * ShieldRegenRate (float, default 20)
     * HeatManager (component reference)
   
   - Add methods:
     * TakeDamage(float damage) - Apply damage (shields first, then hull)
     * RegenerateShields() - Called each turn, add ShieldRegenRate to CurrentShields
     * ApplyHeatPenalties() - Modify movement speed based on HeatManager penalties
     * ApplyHeatDamage() - Apply hull damage if heat in Critical/Catastrophic
     * Die() - Destroy ship, trigger death event
   
   - Integrate with existing ExecuteMove():
     * Apply speed penalty from HeatManager when calculating movement interpolation

4. MODIFY: Assets/Scripts/Movement/TurnManager.cs
   - In StartSimulationPhase() (or end of phase):
     * Call HeatManager.ApplyPassiveCooling() on all ships
     * Call Ship.RegenerateShields() on all ships
     * Call Ship.ApplyHeatDamage() on all ships
   
   - Track all ships in scene (find all Ship components on Start)

INTEGRATION NOTES:
- HeatManager should be added to Ship GameObject at runtime (AddComponent) or in prefab
- Heat penalties affect movement speed by modifying the interpolation speed in Ship.ExecuteMove()
- For now, accuracy penalties won't have visible effect (weapons not implemented yet)
- Hull damage from heat should apply AFTER movement completes in Simulation phase

TESTING REQUIREMENTS:

Unit Tests (create in Assets/Tests/PlayModeTests/HeatSystemTests.cs):
1. Test_HeatAccumulation - Add heat, verify CurrentHeat increases
2. Test_PassiveCooling - Apply cooling, verify heat decreases by 20
3. Test_HeatTiers - Verify correct tier at each threshold
4. Test_HeatPenalties - Verify penalty values at each tier
5. Test_PlannedHeat - Add planned heat, verify preview, commit, verify current
6. Test_InstantCooling - Apply instant cooling, verify immediate reduction
7. Test_HullDamageFromHeat - Set heat to 120, apply damage, verify hull decreased
8. Test_ShieldRegeneration - Damage shields, regenerate, verify increase
9. Test_ShipDeath - Reduce hull to 0, verify Die() called
10. Test_HeatEvents - Subscribe to events, verify they fire on heat changes

Manual Testing Instructions:
1. Create test scene with single Ship GameObject
2. Add HeatDisplay UI element (Canvas → UI → Image for heat bar)
3. Add test UI buttons to manually:
   - Add +20 heat
   - Add +50 heat
   - Apply instant cooling -50
   - Damage ship -100
   - End turn (triggers cooling and regen)
4. Observe:
   - Heat bar updates correctly (color changes)
   - Numeric display accurate
   - Ship takes hull damage at 120+ heat
   - Passive cooling applies each turn
   - Shield regeneration works
   - Ship movement slows at high heat (test during Simulation phase)

DELIVERABLES:
1. HeatManager.cs with all methods and properties
2. HeatDisplay.cs with three-layer UI visualization
3. Modified Ship.cs with combat stats and heat integration
4. Modified TurnManager.cs with cooling/regen calls
5. HeatSystemTests.cs with 10 unit tests (all passing)
6. Test scene setup for manual testing

STATUS UPDATE:
After completing this task, update IMPLEMENTATION_STATUS.md with:
- ✅ Step 1.1 Complete - Heat System Foundation
- 📁 New Files: HeatManager.cs, HeatDisplay.cs, HeatSystemTests.cs
- 🔧 Modified Files: Ship.cs, TurnManager.cs
- 🧪 Unit Tests: 10/10 passing
- 🎮 Manual Test: Heat bar displays, penalties apply, cooling works
- ⏭️ Next Step: 1.2 (Ability System) or Integration 1.3
- 🚧 Notes: [Any issues or observations]

Begin implementation now.
```

---

#### MANUAL TESTING 1.1
**Time**: 15 minutes

**After Claude Code completes:**

1. Open Unity test scene
2. Add HeatDisplay to Canvas (drag prefab or manually create UI)
3. Create test buttons:
   - Button: "Add Heat +20"
   - Button: "Add Heat +50"
   - Button: "Cool -50"
   - Button: "Damage -100"
   - Button: "End Turn"
4. Wire up buttons to call HeatManager methods
5. Play scene and verify:
   - [ ] Heat bar shows current heat visually
   - [ ] Colors transition through tiers (green→yellow→orange→red)
   - [ ] Numeric display accurate
   - [ ] Planned heat shows as overlay
   - [ ] Passive cooling works on "End Turn"
   - [ ] Hull damage applies at 120+ heat
   - [ ] Ship movement slows at high heat
   - [ ] Shield regeneration works

**If all tests pass, proceed to Step 1.2**

---

### Step 1.2: Ability System Foundation 🔴
**Parallel Track B** - Can run parallel with Step 1.1

**Prerequisites**:
- Phase 0 complete
- Project structure created
- Step 1.1 can provide HeatManager interface (or use stub)

---

#### MANUAL TASK 1.2A: Create Interface Definition
**Time**: 5 minutes

If Step 1.1 is still in progress, create this stub:

**Create**: `Assets/Scripts/Combat/IHeatManager.cs`
```csharp
public interface IHeatManager
{
    float CurrentHeat { get; }
    float MaxHeat { get; }
    float PlannedHeat { get; }
    void AddPlannedHeat(float amount);
    void CommitPlannedHeat();
    void ClearPlannedHeat();
    bool CanAffordHeat(float amount);
}
```

This allows ability system to develop independently.

---

#### CLAUDE CODE PROMPT 1.2

```
CONTEXT:
I'm implementing Phase 1 of the Hephaestus space combat prototype. The Heat System (Step 1.1) is [complete/in progress]. We need the Ability System that uses heat as a resource along with cooldowns.

Existing files:
- Assets/Scripts/Movement/Ship.cs - Ship behavior
- Assets/Scripts/Movement/TurnManager.cs - Phase management
- Assets/Scripts/Combat/HeatManager.cs - Heat tracking [or IHeatManager.cs interface if parallel]

OBJECTIVE:
Implement the Ability System - a flexible framework for special ship abilities with heat costs, cooldowns, and spin-up delays during execution.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Combat/Abilities/Ability.cs (abstract base class)
   - MonoBehaviour that attaches to Ship
   - Properties:
     * AbilityName: string
     * HeatCost: int
     * MaxCooldown: int
     * CurrentCooldown: int (starts at 0)
     * SpinUpTime: float (activation delay in seconds)
     * IsReady: bool (property, true if CurrentCooldown == 0)
     * CanAfford: bool (property, checks if ship can pay heat cost)
   
   - Protected:
     * ship: Ship (reference to owner)
     * isQueued: bool (marked for execution this turn)
   
   - Methods:
     * virtual void Initialize(Ship owner) - Set ship reference
     * bool TryActivate() - Queue ability if ready and affordable
     * abstract IEnumerator ExecuteWithSpinUp() - Coroutine: wait SpinUpTime, then Execute()
     * abstract void Execute() - Apply ability effect
     * void TickCooldown() - Decrease CurrentCooldown (called by AbilitySystem)
     * void StartCooldown() - Set CurrentCooldown to MaxCooldown
     * virtual void Cancel() - Cancel queued ability (if ship destroyed)

2. CREATE: Assets/Scripts/Combat/Abilities/AbilitySystem.cs
   - Component on Ship GameObject
   - Manages all abilities on ship
   - Properties:
     * abilities: List<Ability> (all abilities on this ship)
     * queuedAbilities: List<Ability> (abilities marked for execution this turn)
   
   - Methods:
     * void Start() - Find all Ability components on GameObject and children, register them
     * bool TryActivateAbility(string abilityName) - Attempt to activate by name
     * IEnumerator ExecuteQueuedAbilities() - Called during Simulation, execute all queued
     * void TickAllCooldowns() - Called at end of turn, tick all abilities
     * void ClearQueue() - Remove all queued abilities
     * Ability GetAbility(string name) - Find ability by name
     * List<Ability> GetAllAbilities() - Return all abilities

3. CREATE: Assets/Scripts/Combat/Abilities/EmergencyCooling.cs
   - Inherits from Ability
   - Settings: HeatCost=0, MaxCooldown=4, SpinUpTime=0.1f
   - Execute(): Call ship.HeatManager.InstantCooling(50)
   - VFX: Blue venting particles (placeholder for now)

4. CREATE: Assets/Scripts/Combat/Abilities/ShieldBoost.cs
   - Inherits from Ability
   - Settings: HeatCost=25, MaxCooldown=3, SpinUpTime=0.3f
   - Execute(): 
     * Add 100 to ship.CurrentShields (temporary shields)
     * Start coroutine to remove after 2 turns
   - Need: Track temporary shield amount separately
   - VFX: Shield bubble pulse (placeholder)

5. CREATE: Assets/Scripts/Combat/Abilities/EvasiveManeuver.cs
   - Inherits from Ability
   - Settings: HeatCost=35, MaxCooldown=2, SpinUpTime=0.0f (instant)
   - Execute():
     * Set ship.MovementConstraintsOverride = true
     * Override max turn angle to 90°
     * Override max distance to 30 units
     * Schedule reset after current movement completes
   - VFX: Engine flare (placeholder)

6. CREATE: Assets/Scripts/Combat/Abilities/OverchargeWeapons.cs
   - Inherits from Ability
   - Settings: HeatCost=20, MaxCooldown=3, SpinUpTime=0.5f
   - Execute():
     * Set ship.WeaponDamageMultiplier = 1.5f
     * Set ship.WeaponHeatMultiplier = 2.0f
     * Schedule reset at end of turn
   - VFX: Red energy crackling (placeholder)

7. CREATE: Assets/Scripts/Combat/Abilities/SensorBurst.cs
   - Inherits from Ability
   - Settings: HeatCost=15, MaxCooldown=2, SpinUpTime=0.2f
   - Execute():
     * Find all enemy ships
     * Make their PlannedPosition/PlannedRotation visible (reveal movement ghosts)
     * Duration: Until end of Simulation phase
   - VFX: Expanding wave (placeholder)

8. CREATE: Assets/Scripts/Combat/Abilities/PDOverride.cs
   - Inherits from Ability
   - Settings: HeatCost=30, MaxCooldown=3, SpinUpTime=0.3f
   - Execute():
     * Double ship.PointDefenseSystem.MaxTargetsPerTurret (from 2 to 4)
     * Schedule reset at end of turn
   - VFX: PD turrets glow yellow (placeholder)
   - NOTE: PointDefenseSystem doesn't exist yet, stub this out

9. CREATE: Assets/Scripts/UI/AbilityBarUI.cs
   - UI component showing 6 ability slots
   - Each slot displays:
     * Ability icon (use colored squares for now)
     * Ability name
     * Hotkey number (1-6)
     * Cooldown timer ("2T" overlay if on cooldown)
     * Heat cost text
   - Visual states:
     * Available: Full brightness
     * On Cooldown: Grayed out + timer overlay
     * Insufficient Heat: Yellow warning border
   - Layout: Horizontal bar, bottom of screen
   - Expand when Hephaestus selected (show full names + descriptions)

10. MODIFY: Assets/Scripts/Movement/Ship.cs
    - Add properties:
      * AbilitySystem: AbilitySystem (component reference)
      * MovementConstraintsOverride: bool (for Evasive Maneuver)
      * WeaponDamageMultiplier: float (for Overcharge, default 1.0)
      * WeaponHeatMultiplier: float (for Overcharge, default 1.0)
    
    - Modify PlanMove() or movement constraints:
      * Check MovementConstraintsOverride, use different limits if true

11. MODIFY: Assets/Scripts/Movement/TurnManager.cs
    - In StartSimulationPhase():
      * Call ship.AbilitySystem.ExecuteQueuedAbilities() on all ships (as coroutine)
    
    - At end of Simulation phase:
      * Call ship.AbilitySystem.TickAllCooldowns() on all ships

12. MODIFY: Assets/Scripts/Movement/MovementController.cs
    - Add hotkey handling (number keys 1-6):
      * If key pressed during Command phase, try to activate corresponding ability
      * Call ship.AbilitySystem.TryActivateAbility(abilityName)

INTEGRATION NOTES:
- If HeatManager not yet complete, use IHeatManager interface with stub implementation
- Abilities that reference systems not yet built (WeaponSystem, PointDefenseSystem) should have stub effects
- VFX are placeholders for now (Phase 5)
- Abilities queue during Command phase, execute during Simulation phase
- Spin-up delays should use Unity coroutines during Simulation
- If ship destroyed mid-spin-up, cancel all queued abilities

TESTING REQUIREMENTS:

Unit Tests (create in Assets/Tests/PlayModeTests/AbilitySystemTests.cs):
1. Test_AbilityActivation - Activate ability, verify queued
2. Test_AbilityCannotActivateOnCooldown - Try activate while on cooldown, verify fails
3. Test_AbilityCannotActivateInsufficientHeat - Try activate with low heat, verify fails
4. Test_AbilityCooldownTick - Start cooldown, tick, verify decreases
5. Test_AbilitySpinUp - Activate, verify Execute() called after SpinUpTime
6. Test_EmergencyCooling - Use ability, verify heat reduced by 50
7. Test_ShieldBoost - Use ability, verify shields increased, verify decay after 2 turns
8. Test_EvasiveManeuver - Use ability, verify movement constraints changed
9. Test_OverchargeWeapons - Use ability, verify multipliers set
10. Test_SensorBurst - Use ability, verify enemy positions revealed
11. Test_PDOverride - Use ability, verify PD rate doubled (stub for now)
12. Test_MultipleAbilitiesQueue - Queue 2 abilities, verify both execute
13. Test_AbilityInterruptOnDeath - Queue ability, destroy ship, verify cancelled

Manual Testing Instructions:
1. Create test scene with single Ship GameObject
2. Attach all 6 ability components to Ship
3. Add AbilityBarUI to Canvas
4. Add test UI buttons for each ability (1-6)
5. Add heat display (from Step 1.1)
6. Play scene and test:
   - Press number keys 1-6 to activate abilities
   - Verify abilities show in UI (icons, names, cooldowns)
   - Verify heat costs apply when abilities execute
   - Verify cooldowns prevent re-activation
   - Verify "insufficient heat" warning when can't afford
   - Verify abilities execute after spin-up delay
   - Test Emergency Cooling reduces heat
   - Test Shield Boost increases shields temporarily
   - Test Evasive Maneuver changes movement constraints
   - Test multiple abilities can be queued same turn

DELIVERABLES:
1. Ability.cs abstract base class
2. AbilitySystem.cs manager component
3. 6 concrete ability classes (EmergencyCooling, ShieldBoost, EvasiveManeuver, OverchargeWeapons, SensorBurst, PDOverride)
4. AbilityBarUI.cs with 6-slot interface
5. Modified Ship.cs with ability integration
6. Modified TurnManager.cs with ability execution
7. Modified MovementController.cs with hotkey handling
8. AbilitySystemTests.cs with 13 unit tests (all passing)
9. Test scene setup for manual testing

STATUS UPDATE:
After completing this task, update IMPLEMENTATION_STATUS.md with:
- ✅ Step 1.2 Complete - Ability System Foundation
- 📁 New Files: Ability.cs, AbilitySystem.cs, 6 ability classes, AbilityBarUI.cs, AbilitySystemTests.cs
- 🔧 Modified Files: Ship.cs, TurnManager.cs, MovementController.cs
- 🧪 Unit Tests: 13/13 passing
- 🎮 Manual Test: Abilities activate, heat costs apply, cooldowns work, UI displays correctly
- ⏭️ Next Step: Integration 1.3
- 🚧 Notes: [Any issues, especially if HeatManager was stubbed]

Begin implementation now.
```

---

#### MANUAL TESTING 1.2
**Time**: 20 minutes

**After Claude Code completes:**

1. Open test scene with Ship
2. Verify all 6 ability components attached to Ship GameObject
3. Add AbilityBarUI to Canvas
4. Play scene and test each ability:
   - [ ] Press 1-6 keys to activate abilities
   - [ ] Ability icons show in UI
   - [ ] Cooldown timers display correctly
   - [ ] Heat costs show and apply
   - [ ] "Insufficient heat" warning when can't afford
   - [ ] Abilities queue during Command phase
   - [ ] Abilities execute during Simulation with spin-up delays
   - [ ] Emergency Cooling reduces heat by 50
   - [ ] Shield Boost adds temporary shields
   - [ ] Evasive Maneuver changes movement limits
   - [ ] Multiple abilities can queue same turn
   - [ ] Cooldowns prevent re-activation

**If all tests pass, proceed to Integration 1.3**

---

### Step 1.3: Phase 1 Integration ⚫
**Sequential** - Must complete after Steps 1.1 AND 1.2

**Prerequisites**:
- Step 1.1 complete (Heat System)
- Step 1.2 complete (Ability System)

---

#### CLAUDE CODE PROMPT 1.3

```
CONTEXT:
Phase 1 parallel development is complete. We have:
- Step 1.1: Heat System (HeatManager, HeatDisplay, heat integration in Ship/TurnManager)
- Step 1.2: Ability System (6 abilities, AbilitySystem, AbilityBarUI)

OBJECTIVE:
Integrate the two systems together, replacing any stubs, fixing any interface mismatches, and ensuring they work harmoniously.

FILES TO REVIEW AND INTEGRATE:
- Assets/Scripts/Combat/HeatManager.cs
- Assets/Scripts/Combat/Abilities/Ability.cs
- Assets/Scripts/Combat/Abilities/AbilitySystem.cs
- Assets/Scripts/Combat/Abilities/*.cs (all 6 ability implementations)
- Assets/Scripts/Movement/Ship.cs
- Assets/Scripts/Movement/TurnManager.cs
- Assets/Scripts/UI/HeatDisplay.cs
- Assets/Scripts/UI/AbilityBarUI.cs

INTEGRATION TASKS:

1. Replace IHeatManager stub (if used) with real HeatManager
   - Update Ability.cs to use HeatManager instead of interface
   - Verify CanAfford property uses HeatManager.CanAffordHeat()

2. Connect ability heat costs to HeatDisplay
   - When ability is queued, call HeatManager.AddPlannedHeat(HeatCost)
   - Verify HeatDisplay shows planned heat overlay
   - When ability executes, call HeatManager.CommitPlannedHeat()
   - If ability cancelled, call HeatManager.ClearPlannedHeat()

3. Verify TurnManager integration
   - Ensure correct order:
     1. Start Simulation phase
     2. Execute movement
     3. Execute abilities (with spin-up delays)
     4. Apply passive cooling
     5. Tick ability cooldowns
     6. Apply heat damage
     7. Regenerate shields
   - Test with both systems active

4. Connect AbilityBarUI to HeatDisplay
   - When hovering over ability, show preview heat in HeatDisplay
   - When ability queued, update HeatDisplay immediately

5. Test all 6 abilities with heat system:
   - Emergency Cooling: Should work with heat at any level
   - Shield Boost: +25 heat should show in UI before execution
   - Evasive Maneuver: +35 heat should show in UI
   - Overcharge: +20 base heat should show
   - Sensor Burst: +15 heat should show
   - PD Override: +30 heat should show (even though PD not implemented yet)

6. Test ability + heat combinations:
   - Queue multiple abilities, verify total planned heat accurate
   - Test overheating via abilities (heat > 150)
   - Test using Emergency Cooling to enable big moves next turn
   - Test heat penalties affecting ship (reduced speed at high heat)

7. Create integration test scene:
   - Ship with both systems active
   - UI showing both heat bar and ability bar
   - Test buttons for various scenarios

TESTING REQUIREMENTS:

Integration Tests (create in Assets/Tests/PlayModeTests/Phase1IntegrationTests.cs):
1. Test_AbilityAddsPlannedHeat - Queue ability, verify HeatDisplay shows planned
2. Test_AbilityCommitsHeat - Execute ability, verify heat added to current
3. Test_AbilityCancelClearsPlannedHeat - Queue then cancel, verify planned cleared
4. Test_MultipleAbilitiesHeatStacking - Queue 3 abilities, verify total heat correct
5. Test_EmergencyCoolingAtHighHeat - Heat at 140, use Emergency Cooling, verify drops to 90
6. Test_AbilitiesPreventedByInsufficientHeat - Heat at 140, can't afford +25 ability
7. Test_TurnOrderCorrect - Verify: movement → abilities → cooling → damage → regen
8. Test_HeatPenaltiesWithAbilities - Use abilities to overheat, verify penalties apply
9. Test_AbilityHoverShowsPreview - Hover ability, verify preview heat in display
10. Test_ShieldBoostWithRegen - Use Shield Boost, verify regen still works on temp shields

Manual Testing Instructions:
1. Open integration test scene
2. Verify both UI elements visible (HeatDisplay + AbilityBarUI)
3. Play scene and execute these scenarios:
   
   SCENARIO A: Normal Combat Flow
   - Start turn (heat = 0)
   - Queue Shield Boost (+25 heat planned)
   - Queue Overcharge (+20 heat planned)
   - Verify HeatDisplay shows +45 planned
   - End turn (Simulation)
   - Verify abilities execute with spin-up delays
   - Verify heat becomes 45 after execution
   - Verify passive cooling reduces to 25 next turn
   
   SCENARIO B: Overheat Recovery
   - Manually set heat to 120 (Critical tier)
   - Verify ship takes hull damage each turn
   - Use Emergency Cooling
   - Verify heat drops to 70 (Safe tier)
   - Verify no more hull damage
   
   SCENARIO C: Heat Budget Management
   - Heat at 100
   - Try to queue Shield Boost (+25)
   - Try to queue Evasive Maneuver (+35)
   - Verify warning that total exceeds safe threshold
   - Can still execute if player confirms
   
   SCENARIO D: Multiple Abilities
   - Queue 3 different abilities same turn
   - Verify all show planned heat
   - Verify all execute in order during Simulation
   - Verify total heat accurate after all execute

DELIVERABLES:
1. Fully integrated Heat + Ability systems (no stubs)
2. HeatDisplay shows planned heat from queued abilities
3. AbilityBarUI shows heat costs and affordability
4. TurnManager executes both systems in correct order
5. Phase1IntegrationTests.cs with 10 tests (all passing)
6. Integration test scene demonstrating all features
7. Documentation of any issues found and resolved

STATUS UPDATE:
After completing this task, update IMPLEMENTATION_STATUS.md with:
- ✅ Step 1.3 Complete - Phase 1 Integration
- ✅ Phase 1 Complete - Core Combat Infrastructure
- 🔧 Modified Files: [List any files that needed integration fixes]
- 🧪 Integration Tests: 10/10 passing
- 🧪 Unit Tests Total: 23/23 passing (10 heat + 13 ability)
- 🎮 Manual Test: All scenarios pass, systems work together seamlessly
- ⏭️ Next Phase: Phase 2 - Weapon Systems (can start parallel development)
- 🚧 Notes: [Any observations about integration, performance, or future considerations]
- 📊 Phase 1 Time: [Actual time taken]

Begin integration now.
```

---

#### MANUAL TESTING 1.3
**Time**: 30 minutes

**Full integration test:**

1. Open integration test scene
2. Verify all UI elements present and positioned correctly
3. Run through all 4 test scenarios (A, B, C, D) from prompt
4. Document any issues found
5. Verify all unit + integration tests pass (23 total)

**Checklist**:
- [ ] Heat and abilities work together
- [ ] Planned heat displays correctly
- [ ] Multiple abilities can queue
- [ ] Emergency Cooling works at critical heat
- [ ] Heat penalties apply correctly
- [ ] Turn execution order is correct
- [ ] No errors in console
- [ ] Performance is smooth (60 FPS)

**If all tests pass, Phase 1 is complete! Proceed to Phase 2**

---

## Phase 2: Weapon Systems (Weeks 3-5)

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

2. Create empty GameObjects as weapon hardpoint templates:
   - Create Ship prefab variant for testing
   - Add child empty GameObjects named:
     * "RailGun_Port_Hardpoint"
     * "RailGun_Starboard_Hardpoint"
     * "Cannon_Forward_Hardpoint"
     * "Torpedo_Forward_Hardpoint"
     * "Missile_Dorsal_Hardpoint"
     * "Missile_Ventral_Hardpoint"
   - Position these appropriately on ship mesh

3. Update IMPLEMENTATION_STATUS.md:
   ```markdown
   ### Current Phase: 2.1 - Weapon Systems
   ### Ready for parallel development (3 tracks)
   ```

---

### Phase 2 Parallel Development (3 Agents)

Phase 2 can be split into 3 parallel tracks:
- 🔵 Track A: Weapon Base + Basic Weapons
- 🔴 Track B: Projectile System
- 🟢 Track C: Targeting UI

**All 3 can run simultaneously with proper coordination**

---

### Step 2.1A: Weapon Base Architecture 🔵
**Parallel Track A**

#### CLAUDE CODE PROMPT 2.1A

```
CONTEXT:
Phase 1 complete (Heat + Abilities working). Now implementing weapon systems.

This is PARALLEL TRACK A of Phase 2. Track B (Projectiles) and Track C (Targeting UI) are running simultaneously.

COORDINATION:
- Track B is creating projectile classes
- We need to define projectile spawning interface they can implement
- Track C is creating targeting UI
- We need to define weapon group interface they can use

Existing files:
- Assets/Scripts/Combat/HeatManager.cs
- Assets/Scripts/Movement/Ship.cs
- Assets/Scripts/Movement/TurnManager.cs

OBJECTIVE:
Create the weapon system base architecture and implement 2 basic weapon types (Rail Gun and Newtonian Cannon).

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Combat/Weapons/WeaponSystem.cs (abstract base)
   - MonoBehaviour attached to weapon hardpoint GameObjects
   - Properties:
     * WeaponName: string
     * Damage: float
     * HeatCost: int
     * FiringArc: float (degrees, 360 = turret, 180 = forward hemisphere, 30 = narrow)
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
     * ProjectileType: enum (InstantHit, Ballistic, Homing)
     * SpawnPosition: Vector3
     * SpawnRotation: Quaternion
     * TargetPosition: Vector3 (for ballistic)
     * TargetShip: Ship (for homing)
     * Damage: float
     * Speed: float

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
     * Spawn visual tracer effect (line renderer from hardpoint to target)
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
     * Projectile travels at 2 units/second toward target position
     * Call ProjectileManager.SpawnBallisticProjectile(GetProjectileInfo())
     * Apply heat to owner ship
   
   - GetProjectileInfo():
     * Return ProjectileType.Ballistic with target position (lead calculation)
     * Target position = target.CurrentPosition + (target.Velocity * time_to_impact)

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

5. CREATE: Assets/Scripts/Combat/ProjectileManager.cs (Interface for Track B)
   - Static class or singleton
   - Methods that Track B will implement:
     * static void SpawnBallisticProjectile(ProjectileSpawnInfo info)
     * static void SpawnHomingProjectile(ProjectileSpawnInfo info)
     * static void SpawnInstantHitEffect(Vector3 start, Vector3 end, float damage)
   
   - For now, implement as stubs that log to console
   - Track B will replace with real implementation

6. MODIFY: Assets/Scripts/Movement/Ship.cs
   - Add property:
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
   - Rail gun fires instantly (log message for now, since projectiles not implemented)
   - Cannon calls spawn projectile (stub logs message)
   - Heat accumulates when weapons fire
   - Cooldowns tick down
   - Weapon groups track assignments
   - Spin-up delays work (weapons fire after delay during Simulation)

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
**Parallel Track B** - Runs simultaneously with 2.1A and 2.1C

#### CLAUDE CODE PROMPT 2.1B

```
CONTEXT:
Phase 1 complete. Phase 2 parallel development in progress.

This is PARALLEL TRACK B of Phase 2. Track A (Weapons) and Track C (Targeting UI) are running simultaneously.

COORDINATION:
- Track A is creating weapon base classes and defining ProjectileSpawnInfo struct
- We need to implement ProjectileManager that Track A will call
- Track C is creating targeting UI (independent of our work)

You may need to recreate ProjectileSpawnInfo struct if Track A hasn't shared it yet:
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
Create the projectile physics system that handles ballistic and homing projectiles, collision detection, and damage application.

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
     * TurnRate: float (degrees per second, limits how fast it can turn)
   
   - UpdateMovement():
     * Calculate direction to target current position
     * Rotate toward target at TurnRate
     * Move forward at Speed
     * Check for collisions each frame
     * If target destroyed, continue straight (ballistic)
   
   - Used by: Missiles and Torpedoes

4. CREATE: Assets/Scripts/Combat/ProjectileManager.cs (Real Implementation)
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
       - Spawn visual tracer (line renderer or particle beam)
       - Apply damage immediately
       - No actual projectile object
     
     * void Update() - Call UpdateMovement() on all active projectiles
     
     * void ReturnToPool(Projectile proj) - Deactivate and pool for reuse
     
     * void ClearAllProjectiles() - Destroy all projectiles (end of combat)

5. CREATE: Assets/Prefabs/Projectiles/BallisticProjectile.prefab
   - Sphere mesh (or simple capsule)
   - Glowing material
   - Trail renderer component (for visual trail)
   - Collider (sphere or capsule)
   - BallisticProjectile component

6. CREATE: Assets/Prefabs/Projectiles/HomingProjectile.prefab
   - Similar to ballistic but different color
   - HomingProjectile component
   - Particle system for thruster effect

7. CREATE: Assets/Scripts/Combat/Projectiles/InstantHitEffect.cs
   - Simple visual effect for instant-hit weapons (rail guns)
   - Line renderer from start to end position
   - Fades out over 0.1 seconds
   - Auto-destroys after fade

8. Collision & Damage System:
   - Projectiles use sphere cast or overlap sphere to detect ships
   - Check if hit ship is enemy (not owner)
   - Call ship.TakeDamage(Damage)
   - Destroy projectile on hit
   - Play impact VFX (placeholder for now)

TESTING REQUIREMENTS:

Unit Tests (Assets/Tests/PlayModeTests/ProjectileSystemTests.cs):
1. Test_BallisticSpawn - Spawn ballistic, verify created and moving
2. Test_BallisticTrajectory - Spawn ballistic, verify travels in straight line
3. Test_BallisticCollision - Spawn at target, verify collision detected
4. Test_BallisticDamageApplication - Projectile hits ship, verify damage applied
5. Test_BallisticLifetimeExpiry - Wait lifetime, verify auto-destroyed
6. Test_HomingSpawn - Spawn homing with target, verify created
7. Test_HomingSeeks - Spawn homing, move target, verify projectile turns toward it
8. Test_HomingHitMovingTarget - Spawn homing at moving target, verify eventual hit
9. Test_HomingTargetDestroyed - Destroy target mid-flight, verify projectile continues ballistic
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
   - If target destroyed, homing projectiles continue straight
   - Instant hit creates visual tracer
   - No friendly fire (projectiles ignore owner)
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
- If Track A hasn't defined it yet, create it and share
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
**Parallel Track C** - Runs simultaneously with 2.1A and 2.1B

#### CLAUDE CODE PROMPT 2.1C

```
CONTEXT:
Phase 1 complete. Phase 2 parallel development in progress.

This is PARALLEL TRACK C of Phase 2. Track A (Weapons) and Track B (Projectiles) are running simultaneously.

COORDINATION:
- Track A is creating weapon systems with group assignment functionality
- We need to create UI that allows players to assign weapons to groups and fire them
- Track B is creating projectiles (independent of our work)

You may need to work with interface from Track A. If not available yet, use this:
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
Create the targeting UI system that allows players to select targets, assign weapons to groups, and fire weapon groups at selected targets.

ARCHITECTURE REQUIREMENTS:

1. CREATE: Assets/Scripts/Combat/Targeting/TargetingController.cs
   - Component on main camera or game manager
   - Handles target selection and weapon group firing
   - Properties:
     * currentTarget: Ship (currently selected enemy)
     * playerShip: Ship (reference to player's ship)
     * selectionIndicator: GameObject (visual indicator on selected ship)
   
   - Methods:
     * void Update() - Check for mouse clicks on ships
     * void SelectTarget(Ship target) - Set currentTarget, show indicator
     * void DeselectTarget() - Clear currentTarget, hide indicator
     * void AssignGroupToCurrentTarget(int groupNumber) - Fire group at current target
     * void AlphaStrikeCurrentTarget() - Fire all weapons at current target
     * Ship GetTargetUnderMouse() - Raycast to find ship under cursor

2. CREATE: Assets/Scripts/UI/WeaponConfigPanel.cs
   - UI panel shown when player ship (Hephaestus) is selected
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
   - UI panel shown when enemy ship is selected
   - Displays 4 weapon group buttons + Alpha Strike button
   - Each group button shows:
     * Group number and color (1=Blue, 2=Red, 3=Green, 4=Yellow)
     * Weapons in that group (icons or names)
     * Total heat cost for group
     * "OUT OF ARC" warning if any weapon can't fire
     * "ON COOLDOWN" if any weapon not ready
   
   - Alpha Strike button shows:
     * "ALPHA STRIKE" text
     * All assigned weapons
     * Total heat cost
     * Warning if total heat exceeds safe threshold
   
   - Functionality:
     * Click group button OR press number key (1-4) to fire group
     * Click Alpha Strike OR press Space/F to fire all
     * Visual confirmation when group queued
     * Hotkey hints visible
   
   - Layout: Right side of screen

4. CREATE: Assets/Scripts/UI/TargetingLineRenderer.cs
   - Visual feedback showing which groups target which enemies
   - Colored lines from player ship to targets
   - Colors match weapon groups:
     * Group 1: Blue
     * Group 2: Red
     * Group 3: Green
     * Group 4: Yellow
   - Lines appear during Command phase when groups assigned
   - Lines disappear during Simulation phase

5. CREATE: Assets/Scripts/UI/SelectionIndicator.cs
   - Visual indicator on selected ship
   - Rotating ring or highlight effect around ship
   - Color: Cyan for selected enemy, Green for selected friendly
   - Follows ship if it moves

6. MODIFY: Assets/Scripts/Movement/MovementController.cs
   - Add target selection handling:
     * Left-click on enemy ship: Select as target, show WeaponGroupPanel
     * Left-click on player ship: Select Hephaestus, show WeaponConfigPanel
     * Left-click on empty space: Deselect
   
   - Add weapon group hotkeys:
     * Number keys 1-4: Fire that group at current target (if target selected)
     * Space or F: Alpha Strike at current target (if target selected)
   
   - Mode management:
     * Weapon targeting doesn't interfere with movement planning
     * Can plan movement and assign weapon targets in same Command phase

7. CREATE: Assets/Scripts/UI/UIManager.cs
   - Manages which UI panels are visible based on selection state
   - Selection states:
     * Nothing selected: Minimal HUD
     * Enemy selected: Show WeaponGroupPanel, hide WeaponConfigPanel
     * Player ship selected: Show WeaponConfigPanel, hide WeaponGroupPanel
   
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
7. Test_MultiTargeting - Assign Group 1 to Enemy A, Group 2 to Enemy B, verify both lines
8. Test_UIStateTransitions - Select enemy/player/nothing, verify correct panels show
9. Test_OutOfArcWarning - Target behind ship, verify "OUT OF ARC" warning
10. Test_CooldownWarning - Fire weapon, immediately try again, verify "ON COOLDOWN" warning
11. Test_HeatCostDisplay - Group with multiple weapons, verify total heat correct
12. Test_SelectionIndicator - Select ship, verify indicator appears and follows

Manual Testing Instructions:
1. Create test scene with player ship + 2 enemy ships
2. Ensure player ship has multiple weapons (RailGun, Cannon, etc.)
3. Add all UI panels to Canvas:
   - WeaponConfigPanel (left side)
   - WeaponGroupPanel (right side)
   - Heat bar (top center, from Phase 1)
   - Ability bar (bottom, from Phase 1)
4. Play scene and verify:
   
   SCENARIO A: Weapon Configuration
   - Left-click player ship
   - Verify WeaponConfigPanel appears
   - Click weapon's group dropdown, cycle through options
   - Verify weapon assignments change
   - Verify UI updates immediately
   
   SCENARIO B: Single Target Attack
   - Left-click enemy ship A
   - Verify selection indicator appears on ship A
   - Verify WeaponGroupPanel appears (right side)
   - Press "1" to fire Group 1
   - Verify colored line appears from player to enemy A
   - Verify heat cost preview in heat bar
   - End turn (Simulation)
   - Verify weapons fire at enemy A
   
   SCENARIO C: Multi-Target Attack
   - Assign Rail Guns to Group 1, Cannon to Group 2
   - Left-click enemy A, press "1" (Group 1 → Enemy A)
   - Left-click enemy B, press "2" (Group 2 → Enemy B)
   - Verify two colored lines (one to each enemy)
   - End turn (Simulation)
   - Verify weapons fire at correct targets
   
   SCENARIO D: Alpha Strike
   - Left-click enemy ship
   - Press Space (Alpha Strike)
   - Verify all weapon groups show targeting lines
   - Verify total heat cost shown
   - Verify heat bar shows full planned heat
   - End turn (Simulation)
   - Verify all weapons fire
   
   SCENARIO E: Arc Validation
   - Position enemy behind player ship
   - Select enemy, try to fire forward-only weapon
   - Verify "OUT OF ARC" warning
   - Verify weapon doesn't fire

DELIVERABLES:
1. TargetingController.cs
2. WeaponConfigPanel.cs with weapon list UI
3. WeaponGroupPanel.cs with group buttons
4. TargetingLineRenderer.cs for visual feedback
5. SelectionIndicator.cs
6. UIManager.cs for panel management
7. Modified MovementController.cs with targeting hotkeys
8. TargetingSystemTests.cs with 12 unit tests
9. UI prefabs for all panels
10. Test scene demonstrating full targeting workflow

COORDINATION NOTES:
- We use IWeaponManager interface that Track A will implement
- If Track A's WeaponManager doesn't match interface, we'll adapt in integration
- We're independent of Track B (Projectiles)
- Our UI triggers weapon firing, Track A's weapons spawn projectiles from Track B

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

**After Steps 2.1A, 2.1B, and 2.1C complete, proceed to integration:**

---

### Step 2.2: Phase 2 Three-Way Integration ⚫
**Sequential** - Must complete after ALL of 2.1A, 2.1B, 2.1C

**Prerequisites**:
- Step 2.1A complete (Weapon Base + RailGun + Cannon)
- Step 2.1B complete (Projectile System)
- Step 2.1C complete (Targeting UI)

---

#### CLAUDE CODE PROMPT 2.2

```
CONTEXT:
Phase 2 parallel development complete. We have three independent systems that need integration:
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
   - Test heat penalties affect weapon accuracy (implementation in later phase)

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
     * Weapons spin up (visual/audio feedback)
     * Weapons fire (spawn projectiles)
     * Projectiles travel
     * Projectiles hit targets
     * Damage applies
     * Heat accumulates
   - Return to Command phase for next turn

6. Handle Edge Cases
   - Target destroyed before projectile hits (projectile continues or self-destructs?)
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
   - All updates happen smoothly without flickering

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
- 📊 Phase 2A Time: [Actual time taken for tracks A+B+C+Integration]

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

---

### Phase 3 Parallel Development (2 Agents)

Phase 3 can be split into 2 parallel tracks:
- 🔵 Track A: PD Logic System
- 🔴 Track B: PD Visual & Audio

---

### Step 3.1A: Point Defense Logic System 🔵
**Parallel Track A**

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
**Parallel Track B** - Runs simultaneously with 3.1A

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

**Phase 3 complete! Point Defense system fully functional.**

Document is at character limit. Should I continue with Phase 4 (Enemy AI), Phase 5 (Polish), and Phase 6 (MVP Completion) in the artifact?