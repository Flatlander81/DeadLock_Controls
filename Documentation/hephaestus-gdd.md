# Hephaestus: Game Design Document

## Core Concept
Turn-based tactical space combat commanding a single capital ship against enemy vessels. BSG Deadlock's planning-then-execution loop meets submarine warfare tactics with a heat-based weapon economy.

---

## UI/UX Design

### Design Philosophy
**BSG Deadlock-inspired interface with selection-based context and click/hotkey duality.**

The interface adapts based on what the player has selected:
- **Nothing selected**: Minimal HUD showing critical status
- **Enemy selected**: Weapon targeting interface
- **Hephaestus selected**: Full ship status, abilities, and weapon configuration

All UI elements support both mouse clicks AND hotkeys for accessibility and speed.

---

### Selection States & Context

#### State 1: Nothing Selected (Minimal HUD)
**What's visible:**
- Heat bar (layered visualization)
- Hull/Shield status
- Turn number
- Phase indicator (COMMAND / SIMULATION)
- Miniature cooldown indicators for abilities

**Purpose:** Keep the view clean during planning, show only critical status.

#### State 2: Enemy Ship Selected
**What's visible:**
- All "Nothing Selected" elements PLUS:
- Enemy status (Hull, Shields, optionally Heat if intel available)
- Weapon Group firing interface:
  - Group 1 button [Hotkey: 1]
  - Group 2 button [Hotkey: 2]
  - Group 3 button [Hotkey: 3]
  - Group 4 button [Hotkey: 4]
  - Alpha Strike button [Hotkey: Space or F]
- Each button shows:
  - Which weapons are in that group
  - Combined heat cost
  - Whether any weapons are on cooldown
  - Whether weapons are in arc
- Targeting lines from Hephaestus to selected enemy (color-coded by group)

**Interaction:**
- Click group button OR press number key → that group fires at this target
- Click Alpha Strike OR press hotkey → all assigned weapons fire at this target
- Multiple enemies can be targeted simultaneously (Group 1 → Enemy A, Group 2 → Enemy B)

#### State 3: Hephaestus Selected
**What's visible:**
- All "Nothing Selected" elements PLUS:
- Weapon configuration panel:
  ```
  WEAPONS:
  Rail Gun 1        [Group: 1▼]  Heat: 15  Cooldown: Ready  Arc: 360°
  Rail Gun 2        [Group: 1▼]  Heat: 15  Cooldown: Ready  Arc: 360°
  Newtonian Cannon  [Group: 2▼]  Heat: 30  Cooldown: Ready  Arc: 60°
  Torpedo Launcher  [Group: 3▼]  Heat: 25  Cooldown: 2T    Arc: 30°
  Missile Battery 1 [Group: 4▼]  Heat: 20  Cooldown: Ready  Arc: 360°
  Missile Battery 2 [Group: 4▼]  Heat: 20  Cooldown: Ready  Arc: 360°
  ```
- Ability panel (expanded):
  ```
  ABILITIES:
  [1] Emergency Cooling    Cooldown: Ready      Heat: 0
  [2] Shield Boost         Cooldown: 1T         Heat: +25
  [3] Evasive Maneuver     Cooldown: Ready      Heat: +35
  [4] Overcharge Weapons   Cooldown: 2T         Heat: +20
  [5] Sensor Burst         Cooldown: Ready      Heat: +15
  [6] PD Override          Cooldown: 3T         Heat: +30
  ```
- Ammo counts:
  - Torpedoes: 5/6
  - Missiles: 18/20

**Interaction:**
- Click weapon's group dropdown to cycle: Unassigned → 1 → 2 → 3 → 4 → Unassigned
- Click ability icon OR press number key to activate
- Unassigned weapons do not fire (tactical heat conservation)

---

### Heat Bar (Core UI Element)

**Layered progress bar visualization:**

```
[████████░░░░░░░░░░] 40/150
 Current  Planned
```

**Three layers:**
1. **Current Heat** (solid color, changes with heat level):
   - 0-59: Green
   - 60-79: Yellow
   - 80-99: Orange
   - 100-119: Red
   - 120+: Flashing Red

2. **Planned Heat** (semi-transparent overlay):
   - Shows heat that will be added from actions queued this turn
   - Updates in real-time as you plan weapon firing and abilities
   - Example: Current 40 + Planned 60 = 100 total (bar shows both)

3. **Potential Heat** (lighter preview, only on hover):
   - Shows what heat would be if you activate the thing you're hovering over
   - Disappears when not hovering

**Numeric Display:**
- `40/150` (current heat / max capacity)
- `+60` (planned heat this turn, if any)
- Hover for detailed breakdown: "Rail Guns +30, Cannon +30"

**Position:** Top-center of screen (primary focus, always visible)

---

### Weapon Group System

#### Group Assignment
**Weapon Configuration Panel** (visible when Hephaestus selected):
- Each weapon has a dropdown/cycle button showing current group assignment
- Click button to cycle: `Unassigned → 1 → 2 → 3 → 4 → Unassigned`
- Weapons can only be in ONE group at a time (assigning to new group removes from old)
- Unassigned weapons do not fire (heat conservation tactic)

**Visual Feedback:**
- Group numbers color-coded:
  - Group 1: Blue
  - Group 2: Red
  - Group 3: Green
  - Group 4: Yellow
  - Unassigned: Gray

#### Group Firing
**When enemy is selected:**
- Weapon group buttons appear (1-4)
- Each button shows:
  - Group number and color
  - Weapons in group (icons or names)
  - Combined heat cost
  - "OUT OF ARC" warning if any weapon can't fire
  - "ON COOLDOWN" if any weapon not ready
- Click button OR press hotkey → assigns that group to fire at selected target
- Visual: Colored line from Hephaestus to target (matches group color)

**Alpha Strike Button:**
- Fires ALL assigned weapons at selected target
- Shows total heat cost prominently
- Warning color if total heat would exceed safe threshold
- Hotkey: Space or F

**Multi-targeting:**
- Can assign different groups to different enemies
- Example: Select Enemy A, press 1 → Select Enemy B, press 2
- Each group gets its own colored targeting line
- UI shows which groups are assigned where

---

### Ability System UI

#### Ability Bar
**Always visible (minimal mode):**
- Small icons with cooldown timer overlays
- Hotkey numbers visible (1-6)
- Color-coded availability:
  - Available: Full brightness
  - On cooldown: Grayed out with "2T" overlay
  - Insufficient heat budget: Yellow warning border

**Expanded (when Hephaestus selected):**
- Full ability names
- Heat costs prominently displayed
- Cooldown status
- Descriptions (on hover)
- Click OR hotkey to activate

#### Ability Activation Flow
1. Player clicks ability icon OR presses hotkey
2. Ability marked as "queued" (visual confirmation)
3. Heat bar updates to show planned heat
4. During Simulation Phase:
   - Spin-up begins (visual/audio feedback)
   - After spin-up completes, ability activates
   - Heat applied at moment of activation
   - Cooldown begins

---

### Weapon Spin-Up System

**All weapons and abilities have activation delay during Simulation Phase.**

#### Spin-Up Properties (Per Weapon/Ability)
- **Spin-Up Time**: Tunable value (0.0s to 2.0s+)
- **Visual Feedback**: Charging glow effect on weapon hardpoint
- **Audio Feedback**: Power-up sound, crescendos to firing sound
- **UI Indicator**: Optional progress bar or pulsing icon

#### Example Spin-Up Times (Initial Tuning)
```
Rail Gun:          0.2s  (fast, always first to fire)
Newtonian Cannon:  0.5s  (medium)
Missile Battery:   0.4s  (medium-fast)
Torpedo Launcher:  1.0s  (slow, telegraphed)

Emergency Cooling:  0.1s  (near-instant)
Shield Boost:       0.3s  (fast defensive response)
Evasive Maneuver:   0.0s  (instant, movement-based)
Overcharge Weapons: 0.5s  (needs time to route power)
Sensor Burst:       0.2s  (fast intel)
PD Override:        0.3s  (quick defensive activation)
```

#### Simulation Phase Execution Order
1. **T=0s**: All ships begin movement (Bezier curves, no spin-up)
2. **T=0s+**: Weapons/abilities begin spin-up (visual/audio feedback)
3. **T=0.2s**: Rail Guns fire (if queued)
4. **T=0.4s**: Missile Batteries fire
5. **T=0.5s**: Newtonian Cannons fire
6. **T=1.0s**: Torpedoes fire
7. Heat accumulates as each weapon fires
8. If ship is destroyed, all pending actions cancel

#### Interruption (Current Implementation)
- **Only interruption: Ship destruction**
- If ship dies, all queued actions cancel (no heat applied, no cooldowns triggered)
- Future system: Damage to specific systems can interrupt spin-up

---

### HUD Layout Specification

#### Screen Regions

**Top Bar (Always Visible):**
```
┌─────────────────────────────────────────────────────────────┐
│ Turn: 5          [COMMAND PHASE]          Heat: 40/150      │
│                                            [████████░░░░░]   │
│ Hull: 450/500    Shields: 170/200         +60 Planned       │
└─────────────────────────────────────────────────────────────┘
```

**Left Panel (When Hephaestus Selected):**
```
┌─────────────────────────────┐
│ WEAPON CONFIGURATION        │
│                             │
│ Rail Gun 1      [Group: 1▼]│
│   Heat: 15  Arc: 360°       │
│   Cooldown: Ready           │
│                             │
│ Rail Gun 2      [Group: 1▼]│
│   Heat: 15  Arc: 360°       │
│   Cooldown: Ready           │
│                             │
│ Newtonian Cannon [Group: 2▼]│
│   Heat: 30  Arc: 60°        │
│   Cooldown: Ready           │
│                             │
│ Torpedo Launcher [Group: 3▼]│
│   Heat: 25  Arc: 30°        │
│   Cooldown: 2T  Ammo: 5/6   │
│                             │
│ Missile Battery 1 [Group: 4▼]│
│   Heat: 20  Arc: 360°       │
│   Cooldown: Ready           │
│                             │
│ Missile Battery 2 [Group: 4▼]│
│   Heat: 20  Arc: 360°       │
│   Cooldown: 1T  Ammo: 18/20 │
└─────────────────────────────┘
```

**Right Panel (When Enemy Selected):**
```
┌─────────────────────────────┐
│ TARGET: Enemy Frigate Alpha │
│                             │
│ Hull: 120/200               │
│ Shields: 0/100 (depleted)   │
│                             │
│ ┌─────────────────────────┐ │
│ │  [1] Group 1            │ │
│ │  Rail Guns x2           │ │
│ │  Heat: +30              │ │
│ └─────────────────────────┘ │
│                             │
│ ┌─────────────────────────┐ │
│ │  [2] Group 2            │ │
│ │  Newtonian Cannon       │ │
│ │  Heat: +30              │ │
│ └─────────────────────────┘ │
│                             │
│ ┌─────────────────────────┐ │
│ │  [3] Group 3            │ │
│ │  Torpedo (ON COOLDOWN)  │ │
│ └─────────────────────────┘ │
│                             │
│ ┌─────────────────────────┐ │
│ │  [4] Group 4            │ │
│ │  Missiles x2            │ │
│ │  Heat: +40              │ │
│ └─────────────────────────┘ │
│                             │
│ ┌─────────────────────────┐ │
│ │  [SPACE] ALPHA STRIKE   │ │
│ │  All weapons            │ │
│ │  Heat: +100             │ │
│ └─────────────────────────┘ │
└─────────────────────────────┘
```

**Bottom Bar (Always Visible - Abilities):**
```
┌─────────────────────────────────────────────────────────────┐
│ [1] Emergency      [2] Shield      [3] Evasive             │
│     Cooling            Boost           Maneuver             │
│     Ready              1T              Ready                │
│     Heat: 0            Heat: +25       Heat: +35            │
│                                                              │
│ [4] Overcharge     [5] Sensor      [6] PD Override         │
│     Weapons            Burst           2T                   │
│     2T                 Ready           Heat: +30            │
│     Heat: +20          Heat: +15                            │
└─────────────────────────────────────────────────────────────┘
```

---

### Visual Feedback During Simulation Phase

**Movement:**
- Ships glide along Bezier curves (already implemented)
- No spin-up, immediate execution

**Weapon Spin-Up:**
- Weapon hardpoint begins glowing (color-coded by weapon type)
- Glow intensity increases during spin-up
- Audio: Power-up hum/whine, pitch increases
- Particle effects: Energy gathering at weapon mount

**Weapon Fire:**
- Bright flash at moment of firing
- Projectile spawns (tracer, missile contrail, torpedo trail)
- Audio: Distinctive firing sound per weapon type
- Screen shake (subtle, optional)
- Heat bar updates in real-time (+15, +30, etc.)

**Ability Activation:**
- Visual effect on ship:
  - Emergency Cooling: Venting particles, blue glow fades
  - Shield Boost: Shield bubble pulses outward, brightens
  - Evasive Maneuver: Engine flare, ship jolts forward
  - Overcharge: Red energy crackling over weapons
  - Sensor Burst: Expanding wave effect
  - PD Override: Point defense turrets spin up, glow yellow
- Heat bar updates when ability fires

**Damage Resolution:**
- Shield impacts: Blue ripple on shield surface
- Hull hits: Sparks, debris particles, hull section darkens
- Floating damage numbers (optional, can be toggled)
- Ship status bars update in real-time

**Heat Visualization:**
- Ship material shader changes color based on heat:
  - 0-59: Normal (gray/white)
  - 60-79: Slight yellow glow on vents
  - 80-99: Orange glow, heat distortion
  - 100-119: Red glow, heavy distortion, sparks
  - 120+: Flashing red, systems failing, fire effects

---

### Input Summary

**Selection:**
- Left Click: Select ship (enemy or Hephaestus)
- Click empty space: Deselect

**Movement (already implemented):**
- Drag projection ghost to plan destination
- E: Elevation adjustment mode
- R: Rotation adjustment mode
- Enter/Space: Confirm movement

**Weapon Targeting:**
- Select enemy → Press number key (1-4) to fire that group
- Select enemy → Press Space/F for Alpha Strike
- Multiple enemies: Select A, press 1 → Select B, press 2 (multi-target)

**Weapon Configuration:**
- Select Hephaestus → Click weapon's group dropdown to cycle assignment

**Abilities:**
- Press number keys 1-6 to activate abilities
- OR click ability icon in bottom bar

**Camera (already implemented):**
- Shift + Left Mouse Drag: Orbit
- Ctrl + Left Mouse Drag: Pan
- Mouse Wheel: Zoom
- Q/E: Orbit left/right
- W/A/S/D: Pan
- R/F: Zoom in/out

**Phase Control:**
- End Turn button (GUI) to transition to Simulation Phase

---

### UI Design Principles

**Context Sensitivity:**
- UI adapts to what's selected (enemy, Hephaestus, or nothing)
- Reduces clutter, shows relevant info only

**Click + Hotkey Duality:**
- Every action supports both mouse and keyboard
- Players can choose their preferred interaction style
- Hotkeys always visible on UI elements

**Heat as Central Mechanic:**
- Heat bar is prominent, top-center
- All weapon/ability elements show heat cost
- Real-time preview of planned heat
- Visual feedback on ship itself (glowing, venting)

**Information Hierarchy:**
- Critical: Always visible (Heat, Hull, Shields, Phase)
- Important: Visible in relevant context (Weapons when targeting)
- Detailed: Available on demand (Weapon stats when Hephaestus selected)

**Feedback Loops:**
- Planning actions → Heat bar updates (preview)
- Simulation Phase → Visual/audio feedback per action
- Post-simulation → Updated status visible immediately for next Command Phase

**Color Coding:**
- Weapon Groups: Blue, Red, Green, Yellow
- Heat Levels: Green → Yellow → Orange → Red
- Availability: Full color (ready), Gray (cooldown), Yellow border (heat warning)
- Phase: Cyan (Command), Orange (Simulation)

---

## Fundamental Gameplay Loop

### Turn Structure (BSG Deadlock Style)
1. **Command Phase** - Plan all actions simultaneously
   - Movement planning with Bezier curve projections
   - Weapon targeting assignments
   - Ability activations
   - All ships (player + AI) plan in parallel
   - Review current status (heat, damage, cooldowns)
   - Commit plans when ready
   
2. **Simulation Phase** - Watch execution unfold
   - All planned actions execute simultaneously
   - No player input during execution
   - 3-5 second duration for moves to complete
   - Ships execute Bezier movement
   - Abilities activate and apply effects
   - Weapons fire when in arc
   - Projectiles travel/home toward targets
   - Point Defense engages incoming threats
   - Impacts resolve (damage applied, shields depleted, hull damaged)
   - Heat accumulates from weapons/abilities
   - Heat penalties apply immediately
   - Passive cooling applies (-20 heat)
   - Ability cooldowns tick down
   - Visual/audio feedback shows all updates in real-time
   - Phase ends when all movement complete and projectiles resolved
   - Automatically transitions to next Command Phase

---

## Movement System (Already Implemented)

### Current Implementation ✓
- **Projection-based planning**: Green ghost ship shows planned position
- **Bezier curve movement**: Smooth arcs with realistic space combat feel
- **Movement constraints**: 5-20 unit range, 45° max turn
- **Path visualization**: Cyan line shows planned arc
- **Collision detection**: Projection turns red on collision
- **Facing control**: Ships rotate to face movement direction
- **No cost to move**: Movement is always available each turn

### Enhancements to Consider
- **Facing override**: Ability to set final facing independent of movement direction (for broadside attacks)
- **Strafe movement**: Move sideways while maintaining facing
- **Emergency maneuvers**: High-cost evasive moves with tighter turns (ability-based)

---

## Weapon Systems (Heat Economy)

### Core Heat Mechanic
**Weapons and most abilities generate Heat. Heat is THE primary resource.**

- **Heat Capacity**: Ships have a heat threshold (0-150 scale)
- **Passive Cooling**: -20 heat per turn automatically
- **No Hard Cap**: Players CAN overheat if they want - consequences scale with severity

### Heat Penalty Tiers
```
0-59:   Normal operation
60-79:  Minor penalties (-10% accuracy)
80-99:  Moderate penalties (-20% accuracy, -10% movement speed, sensor flicker)
100-119: Severe penalties (-40% accuracy, -25% movement speed, systems glitching)
120-149: Critical state (-60% accuracy, -40% movement speed, 5 hull damage/turn)
150+:   Catastrophic (ship destruction risk, 20 hull damage/turn)
```

### Weapon Types

#### 1. Rail Gun (Light Weapon)
- **Arc**: 360° turret mount
- **Range**: Long (30 units)
- **Hit**: Fast projectile (40 units/second) - visible but nearly impossible to dodge
- **Damage**: 20
- **Heat**: +15
- **Cooldown**: None (can fire every turn)
- **Characteristics**: Precision weapon, good for finishing damaged targets. Fast enough to be reliable against stationary or slow targets, but requires minimal lead calculation against fast-moving ships.

#### 2. Newtonian Cannon (Heavy Weapon)
- **Arc**: 60° spinal mount (must face target directly - 30° left/right)
- **Range**: Medium (20 units)
- **Hit**: Projectile travels at 15 units/second
- **Damage**: 40
- **Heat**: +30
- **Cooldown**: None (can fire every turn)
- **Characteristics**: Massive spinal-mounted cannon built into ship's spine. High damage but requires precise positioning - ship must be aimed directly at target. Rewards good maneuvering and punishes poor positioning.

#### 3. Torpedo (Strategic Weapon)
- **Arc**: Forward firing only (±30°)
- **Range**: Very Long (50 units)
- **Hit**: Slow homing projectile (0.5 units/second)
- **Damage**: 80 (devastating)
- **Heat**: +25
- **Cooldown**: 3 turns after firing
- **Ammo**: Limited (6 torpedoes total)
- **Characteristics**: Multi-turn travel time, can be intercepted by point defense

#### 4. Missile Battery (Tactical Weapon)
- **Arc**: 360° launcher
- **Range**: Long (35 units)
- **Hit**: Fast homing (1.5 units/second)
- **Damage**: 30
- **Heat**: +20
- **Cooldown**: 1 turn after firing
- **Ammo**: Limited (20 missiles total)
- **Characteristics**: Reliable damage, harder to intercept than torpedoes

### Targeting System
During Command Phase:
- Press **T** to enter Targeting Mode (UI turns red/orange)
- Click enemy ship to target with primary weapon group
- **Number keys 1-4** assign specific weapons to specific targets
- **Hold Shift + Number** to assign multiple weapons to same target
- Visual indicators show firing arcs and predicted hit locations
- Weapons outside arc show "OUT OF ARC" warning
- Weapons on cooldown show "COOLDOWN: X TURNS" warning
- Projectile weapons show predicted intercept point based on target's planned movement

---

## Ability System (Heat + Cooldown Economy)

### Design Philosophy
**Abilities use Heat AND have Cooldowns, except Emergency Cooling which only has cooldown.**

This creates two resource management layers:
1. **Heat**: Immediate tactical cost (can I afford this now?)
2. **Cooldown**: Strategic timing cost (when will I need this again?)

### Ability Catalog

#### Emergency Cooling (Defensive)
- **Effect**: Instantly vent -50 heat
- **Heat Cost**: None (this is the heat management tool)
- **Cooldown**: 4 turns
- **Use Case**: Prevent catastrophic overheat, enable alpha strike next turn
- **Tactical Note**: Long cooldown makes timing critical

#### Shield Boost (Defensive)
- **Effect**: +100 temporary shield HP (lasts 2 turns)
- **Heat Cost**: +25
- **Cooldown**: 3 turns
- **Use Case**: Tank incoming damage, buy time for repositioning
- **Tactical Note**: Heat cost means you sacrifice offensive power for defense

#### Evasive Maneuver (Mobility)
- **Effect**: Ignore movement constraints for one turn (can turn 90°, move 30 units)
- **Heat Cost**: +35
- **Cooldown**: 2 turns
- **Use Case**: Escape bad positioning, flank enemies, dodge incoming torpedoes
- **Tactical Note**: High heat cost limits follow-up attacks

#### Overcharge Weapons (Offensive)
- **Effect**: +50% weapon damage for this turn, +100% heat generation
- **Heat Cost**: +20 (plus doubled weapon heat)
- **Cooldown**: 3 turns
- **Use Case**: Alpha strike to finish enemy, breakthrough moment
- **Tactical Note**: Extremely risky heat-wise, can push into catastrophic range

#### Sensor Burst (Utility)
- **Effect**: Reveal all enemy planned moves for this turn
- **Heat Cost**: +15
- **Cooldown**: 2 turns
- **Use Case**: Counter-positioning, predict enemy alpha strikes
- **Tactical Note**: Information advantage but eats into offensive heat budget

#### Point Defense Override (Defensive)
- **Effect**: Double PD engagement rate (4 targets per turret instead of 2)
- **Heat Cost**: +30
- **Cooldown**: 3 turns
- **Use Case**: Counter missile/torpedo saturation attacks
- **Tactical Note**: Proactive defense against predicted incoming barrage

### Ability Management
- Abilities are activated during Command Phase
- Effects trigger during Simulation Phase
- Cooldowns tick down at start of each Command Phase
- Heat is added immediately when ability is activated (affects current turn penalties)
- Multiple abilities can be used in same turn (heat permitting)
- UI shows heat projection: "If you fire all planned weapons + abilities, heat will be: 95/150"

---

## Point Defense System (Automated)

### Core Concept
**Point Defense (PD) turrets are reactive, not player-controlled.**

### PD Characteristics
- **Coverage Arcs**: Each PD turret covers a specific arc (fore, aft, port, starboard)
- **Automatic Engagement**: PD fires during Simulation phase when threats enter arc
- **Priority Targeting**: Engages closest/most dangerous threat first
- **Success Rate**: 60% base chance to destroy incoming projectile
  - Torpedoes: Easier to hit (+20% bonus) due to slow speed
  - Missiles: Standard 60%
  - Cannon projectiles: Harder to hit (-30%) due to speed
- **Rate of Fire**: Can engage 2 targets per turn per turret (4 with PD Override ability)
- **No Heat Generation**: PD operates independently of main heat economy

### PD Configuration (Per Ship)
```
Standard Capital Ship:
- 2x Forward PD turrets (120° arc each, overlapping front coverage)
- 2x Aft PD turrets (120° arc each, overlapping rear coverage)
- 2x Port PD turrets (90° arc each)
- 2x Starboard PD turrets (90° arc each)
```

### Tactical Implications
- **Saturation Attacks**: Fire multiple missiles to overwhelm PD coverage
- **Arc Exploitation**: Attack from angles with weaker PD coverage
- **PD Damage**: PD turrets can be individually destroyed by enemy fire
- **Blind Spots**: Lose PD turrets = create vulnerable attack vectors

---

## Ship Attributes (Simple Starting Point)

### Player Ship: The Hephaestus
```
Hull: 500 HP
Shields: 200 HP (regenerates 20/turn)
Heat Capacity: 150 (danger zone at 100)
Speed: 20 units/turn max
Turn Rate: 45° max

Weapons:
- 2x Rail Guns (port/starboard turrets)
- 1x Newtonian Cannon (forward mount)
- 1x Torpedo Launcher (forward)
- 2x Missile Batteries (dorsal/ventral)

Point Defense:
- 8x PD turrets (full coverage)

Abilities:
- Emergency Cooling (4 turn cooldown)
- Shield Boost (+25 heat, 3 turn cooldown)
- Evasive Maneuver (+35 heat, 2 turn cooldown)
- Overcharge Weapons (+20 heat base, 3 turn cooldown)
- Sensor Burst (+15 heat, 2 turn cooldown)
- PD Override (+30 heat, 3 turn cooldown)
```

### Enemy Ship Types (Basic Variety)

#### Frigate (Light)
```
Hull: 200 HP
Shields: 100 HP
Heat Capacity: 120
Speed: 25 units/turn (faster)
Turn Rate: 60° (more agile)

Weapons:
- 2x Rail Guns
- 1x Missile Battery

Point Defense:
- 4x PD turrets (limited coverage)

Abilities:
- Evasive Maneuver (+35 heat, 2 turn cooldown)
- Emergency Cooling (4 turn cooldown)
```

#### Destroyer (Medium)
```
Hull: 350 HP
Shields: 150 HP
Heat Capacity: 140
Speed: 18 units/turn
Turn Rate: 45°

Weapons:
- 1x Rail Gun
- 2x Newtonian Cannons
- 1x Missile Battery

Point Defense:
- 6x PD turrets (good coverage)

Abilities:
- Shield Boost (+25 heat, 3 turn cooldown)
- Emergency Cooling (4 turn cooldown)
- Overcharge Weapons (+20 heat, 3 turn cooldown)
```

#### Battleship (Heavy)
```
Hull: 600 HP
Shields: 250 HP
Heat Capacity: 180
Speed: 15 units/turn (slow)
Turn Rate: 30° (ponderous)

Weapons:
- 3x Rail Guns
- 2x Newtonian Cannons
- 2x Torpedo Launchers
- 2x Missile Batteries

Point Defense:
- 12x PD turrets (overlapping coverage)

Abilities:
- Emergency Cooling (4 turn cooldown)
- Shield Boost (+25 heat, 3 turn cooldown)
- Overcharge Weapons (+20 heat, 3 turn cooldown)
- PD Override (+30 heat, 3 turn cooldown)
```

---

## Combat Flow Example

### Turn 1 - Opening Engagement
**Command Phase:**
1. Player plans movement: Arc toward enemy flank (free action)
2. Player assigns targets:
   - Rail Guns → Enemy Frigate (2x15 heat = +30 heat)
   - Newtonian Cannon → Enemy Destroyer (+30 heat)
   - Total: +60 heat this turn (within safe zone)
3. Player confirms

**Simulation Phase:**
- Ships execute Bezier movement simultaneously
- Weapons fire when in arc
- Rail gun projectiles travel fast (40 u/s), Newtonian shells travel slow (2 u/s)
- Enemy returns fire: 3 missiles incoming
- Player PD engages: Destroys 2 missiles, 1 gets through
- Damage applied in real-time: Player shields take 30 damage (170/200 shields remaining)
- Heat updates in real-time: +60 heat from weapons, -20 passive cooling = 40 net heat (40/150 total)
- Heat penalty applies: Currently in safe zone (no penalties)
- UI updates continuously during simulation showing damage numbers, heat changes
- Phase automatically ends, transitions to next Command Phase

### Turn 2 - Heat Management Decision
**Command Phase begins immediately after Simulation ends**

**Player current state: 40 heat (from Turn 1's +60 heat, -20 cooling)**

**Option A - Aggressive:**
- Keep firing all weapons (+60 heat = 100 total)
- Risk critical penalties but maximize damage
- No abilities available (cooldowns or heat budget)

**Option B - Alpha Strike:**
- Use Overcharge Weapons ability (+20 heat)
- Fire all weapons with +50% damage (+90 heat from 1.5x weapon heat)
- Total: 110 heat (severe penalty tier next turn)
- Potential to destroy Frigate this turn

**Option C - Defensive:**
- Use Shield Boost (+25 heat)
- Fire only Rail Guns (+30 heat)
- Total: 55 heat (safe zone)
- Prepare for enemy counterattack

**Option D - Information:**
- Use Sensor Burst (+15 heat)
- See enemy plans, adjust positioning
- Fire Newtonian Cannon only (+30 heat)
- Total: 45 heat (safe), better positioning next turn

### Turn 3 - Cooling Decision Point
**Command Phase - Player assesses current state after Turn 2 Simulation**

**Player current state: 110 heat (severe penalties active)**

**Critical Choice:**
- Use Emergency Cooling (no heat, but 4 turn cooldown starts)
- Drop to 60 heat immediately, removes penalties
- Trade: Can't use cooling again for 4 turns
- Must manage heat carefully for next several turns

---

## AI Behavior Framework

### AI Must Use Same Systems
**Critical Design Rule**: AI operates under identical constraints.

- AI plans movement using same Bezier curves and constraints
- AI manages heat identically (can overheat if scripted poorly)
- AI manages ability cooldowns and heat costs
- AI weapons fire during same Simulation phase
- AI benefits from/suffers from PD coverage
- AI uses same ability system with heat costs and cooldowns

### AI Difficulty Tiers (Future)

#### Basic AI
- Moves toward player if out of range
- Fires all weapons every turn (heat management = aggressive)
- Uses Emergency Cooling when heat > 100
- Doesn't use other abilities
- Predictable movement patterns

#### Intermediate AI
- Maintains optimal firing range for weapons
- Basic heat management (stops firing at 80 heat)
- Uses Evasive Maneuver when shields low
- Uses Shield Boost when taking heavy fire
- Attempts to flank player

#### Advanced AI
- Predicts player movement based on patterns
- Sophisticated heat management (balances offense/defense)
- Coordinates with other AI ships (focus fire)
- Exploits PD blind spots
- Uses abilities tactically:
  - Emergency Cooling before planned alpha strike
  - Overcharge Weapons when target is low HP
  - Sensor Burst to counter player evasive maneuvers
  - PD Override when detecting incoming missile salvo

---

## Development Priorities (Prototype Phase)

### Phase 1: Core Systems Foundation (Weeks 1-3)
✓ Movement system (DONE - already implemented)
✓ Turn management (DONE - Command/Simulation phases working)
✓ Camera system (DONE - Homeworld-style orbit camera)

**Remaining:**
- [ ] Modal command interface (M/T/A hotkeys + UI state management)
- [ ] Heat system implementation (tracking, penalties, cooling)
- [ ] Ability system (heat costs, cooldowns, effect application)
- [ ] Basic ship attributes (HP, shields, heat capacity)

### Phase 2: Weapons Implementation (Weeks 4-6)
- [ ] Weapon class architecture (base weapon + 4 types)
- [ ] Targeting system (click-to-target, arc validation)
- [ ] Projectile physics (fast rail gun projectiles, slow ballistic cannons)
- [ ] Homing projectiles (missiles, torpedoes with simple seek behavior)
- [ ] Damage resolution (apply damage, shield/hull interaction)
- [ ] Heat generation on weapon fire
- [ ] Weapon cooldown system

### Phase 3: Point Defense System (Weeks 7-8)
- [ ] PD turret component (arc coverage, engagement logic)
- [ ] Threat detection (identify incoming projectiles)
- [ ] Interception calculations (hit/miss rolls, destruction)
- [ ] Visual feedback (PD firing effects, projectile destruction)
- [ ] PD damage/destruction by enemy fire
- [ ] PD Override ability integration

### Phase 4: Enemy AI (Weeks 9-11)
- [ ] Basic AI controller (uses same Ship.cs movement system)
- [ ] AI decision-making (movement planning, target selection)
- [ ] AI heat management (simple threshold-based)
- [ ] AI ability usage (basic heuristics)
- [ ] Multiple enemy ship spawning
- [ ] Victory/defeat conditions (destroy all enemies = win)

### Phase 5: Polish & Iteration (Weeks 12-15)
- [ ] UI/UX refinement (clearer mode indicators, better feedback)
- [ ] Visual effects (weapon fire, explosions, shield impacts)
- [ ] Audio (weapon sounds, impact sounds, UI feedback)
- [ ] Balance tuning (weapon damage, heat values, ability costs, cooldowns)
- [ ] Bug fixing and edge case handling
- [ ] Basic tutorial/onboarding

---

## Technical Architecture

### Class Structure (Proposed)

```
Core Systems:
- TurnManager.cs (EXISTING - manages phases)
- Ship.cs (EXISTING - extend with combat attributes)
- MovementController.cs (EXISTING - extend to modal system)
- OrbitCamera.cs (EXISTING - no changes needed)

New Systems:
- HeatManager.cs (tracks heat, applies penalties, cooling)
- AbilitySystem.cs (manages ability activation, cooldowns, heat costs)
  - Ability.cs (base class)
  - EmergencyCooling.cs
  - ShieldBoost.cs
  - EvasiveManeuver.cs
  - OverchargeWeapons.cs
  - SensorBurst.cs
  - PDOverride.cs
- WeaponSystem.cs (base class for all weapons)
  - RailGun.cs
  - NewtonianCannon.cs
  - TorpedoLauncher.cs
  - MissileBattery.cs
- TargetingController.cs (handles targeting mode, weapon assignment)
- PointDefenseSystem.cs (manages PD turrets, threat engagement)
- Projectile.cs (base class for all projectiles)
  - BallisticProjectile.cs (cannons)
  - HomingProjectile.cs (missiles, torpedoes)
- AIController.cs (enemy decision-making)
- CombatUIManager.cs (mode switching, HUD updates)
```

### Integration with Existing Systems

#### Extending Ship.cs
```csharp
public class Ship : MonoBehaviour
{
    // EXISTING movement properties
    public Vector3 PlannedPosition { get; private set; }
    public Quaternion PlannedRotation { get; private set; }
    public bool HasPlannedMove { get; private set; }
    
    // NEW combat properties
    public float MaxHull = 500f;
    public float CurrentHull { get; private set; }
    public float MaxShields = 200f;
    public float CurrentShields { get; private set; }
    
    public HeatManager HeatManager { get; private set; }
    public AbilitySystem AbilitySystem { get; private set; }
    public List<WeaponSystem> Weapons { get; private set; }
    public PointDefenseSystem PDSystem { get; private set; }
    
    // NEW methods
    public void TakeDamage(float damage) { }
    public void UseAbility(AbilityType type) { }
    public void ResetTurn() { } // Tick cooldowns, apply passive cooling
}
```

#### Extending TurnManager.cs
```csharp
public class TurnManager : MonoBehaviour
{
    // EXISTING phase management
    public enum Phase { Command, Simulation }
    public Phase CurrentPhase { get; private set; }
    
    // NEW combat integration
    private void StartSimulationPhase()
    {
        // EXISTING: ExecuteMove() on all ships
        foreach (Ship ship in allShips)
        {
            ship.ExecuteMove();
        }
        
        // NEW: Activate abilities
        foreach (Ship ship in allShips)
        {
            ship.AbilitySystem.ExecuteActivatedAbilities();
        }
        
        // NEW: Fire weapons, resolve combat
        foreach (Ship ship in allShips)
        {
            ship.FireWeapons();
        }
        
        // NEW: Update projectiles, PD engagement, damage resolution (all in real-time)
        UpdateProjectilesAndCombat();
        
        // NEW: Apply heat accumulation and penalties immediately
        foreach (Ship ship in allShips)
        {
            ship.HeatManager.AddHeat(); // From weapons/abilities
            ship.HeatManager.ApplyCooling(); // Passive -20
            ship.HeatManager.ApplyPenalties(); // Update stats based on current heat
        }
        
        // NEW: Tick cooldowns at end of simulation
        foreach (Ship ship in allShips)
        {
            ship.AbilitySystem.TickCooldowns();
        }
        
        // Automatically transition to next Command Phase
        StartCommandPhase();
    }
}
```

---

## Design Rationale

### Why Heat + Cooldown Instead of Action Points?
**Dual economy creates deeper decisions.**
- Action Points are generic currency (boring)
- Heat creates escalating tension (gradient, not binary)
- Cooldowns force strategic timing (when to use trump cards?)
- Combined: "Can I afford this heat NOW?" + "Will I need this ability LATER?"
- Emergent gameplay: Heat forces cooling periods, cooldowns prevent spamming best ability

### Why Emergency Cooling Has No Heat Cost?
**Tool must be usable when needed most.**
- If Emergency Cooling cost heat, it's useless when overheating (when you need it)
- Long cooldown (4 turns) prevents spamming
- Creates strategic choice: Use it now or save for emergency?
- "Get out of jail free" card with real cost (can't use again soon)

### Why Simultaneous Execution?
**Commitment over reaction.**
- Forces prediction and planning
- Eliminates "I go first" advantage
- More cerebral, less twitch-based
- Mirrors BSG Deadlock's successful formula

### Why Point Defense Automation?
**Reduce micromanagement burden.**
- Manual PD control = tedious busywork
- Automation lets players focus on strategy
- Creates interesting counter-play (saturate PD, exploit arcs)
- Arc-based system rewards positioning

### Why Limited Ammo on Torpedoes/Missiles?
**Strategic resource management.**
- Prevents infinite missile spam
- Makes torpedo use a significant decision
- Forces weapon diversity (can't just spam one weapon type)
- Creates comeback potential (enemy out of torpedoes = advantage)

### Why No Real-Time Elements?
**Tactical depth over mechanical skill.**
- Target audience wants thinking, not reflexes
- Allows players to carefully plan complex maneuvers
- Better for solo development (no netcode complexity)
- Easier to balance and tune

### Why Free Movement?
**Simplicity and focus.**
- Movement is baseline expectation, not resource sink
- Saves cognitive load for heat/ability decisions
- Makes positioning always available (encourages tactical play)
- Heat already gates offense, don't need to gate defense too

---

## Stretch Goals (Post-Prototype)

### Campaign Structure
- Series of missions with escalating difficulty
- Story context (convoy defense, station assault, fleet engagement)
- Unlock new abilities between missions
- Persistent damage (repairs between missions)

### Ship Customization
- Swap weapon loadouts before missions
- Module system (better shields, enhanced cooling, armor plating)
- Trade-offs (more weapons = less heat capacity)
- Ability loadout selection (pick 4 of 8 available abilities)

### Environmental Hazards
- Asteroid fields (destructible cover, movement hazards)
- Nebulae (sensor interference, reduced PD accuracy, slower passive cooling)
- Planetary gravity wells (affects projectile trajectories)
- Radiation zones (accelerates heat buildup)

### Advanced Weapons
- EMP weapons (disable systems, reset cooldowns, no hull damage)
- Flak cannons (area denial, good vs missiles)
- Plasma torpedoes (slow but bypass shields)
- Mine layers (area control, defensive play)

### Advanced Abilities
- **Tactical Warp**: Teleport short distance, +50 heat, 5T cooldown
- **Shield Overload**: Convert shields to damage burst, +40 heat, 4T cooldown
- **System Hack**: Disable enemy ability for 2 turns, +25 heat, 4T cooldown
- **Decoy Drone**: Launch target dummy for enemy missiles, +15 heat, 2T cooldown

### Multiplayer (Co-op)
- 2 players vs AI fleet
- Each commands their own capital ship
- Shared victory conditions
- Coordination required (heat management, focus fire)

---

## Questions for Iteration

1. **Turn Duration**: Should Simulation phase have fixed duration (3 sec) or scale with action complexity?

2. **Camera Control**: Should camera auto-follow during Simulation or let player free-look?

3. **Damage Numbers**: Floating damage text or just HP bar updates?

4. **Enemy Spawning**: Fixed starting positions or wave-based spawning?

5. **Victory Conditions**: Just "destroy all enemies" or objective variety (protect target, survive X turns, etc.)?

6. **Tutorial**: In-game tutorial mission or separate practice mode?

7. **Save System**: Save between missions or checkpoint system?

8. **Difficulty Scaling**: Player-selected difficulty or adaptive based on performance?

9. **Ability Unlocks**: All abilities available from start or unlock through gameplay?

10. **Heat Visual Feedback**: Should heat glow be subtle or dramatic?

---

## Success Metrics (MVP)

The prototype is successful if:
- ✓ Players can complete a 1v2 combat encounter (player vs 2 enemy frigates)
- ✓ Heat management creates meaningful decisions (not ignorable)
- ✓ Ability timing creates strategic depth (cooldowns matter)
- ✓ Movement planning feels intuitive and responsive
- ✓ Combat feels tactical, not random
- ✓ Point defense system is understandable and impactful
- ✓ AI provides reasonable challenge without frustration
- ✓ Players understand heat/cooldown economy without extensive tutorial
- ✓ No major bugs or game-breaking issues
- ✓ 10-15 minute playtime for single encounter
- ✓ Players feel clever when executing well-timed ability combos

---

## Key Design Insights

### The Heat/Cooldown Dance
**Heat is moment-to-moment tactics. Cooldowns are multi-turn strategy.**

Example scenario:
- Turn 1: Alpha strike with Overcharge (+110 heat total)
- Turn 2: Can't use Overcharge (cooldown), heat at 90, must choose:
  - Fire normally? (hit 120 heat, severe penalties)
  - Use Emergency Cooling? (drop to 70, but now unavailable for 4 turns)
  - Fire reduced weapons? (stay at safe heat, less damage)
- Turn 3-4: If used Emergency Cooling, must manage heat without safety net
- Turn 5+: Abilities come off cooldown, can plan next big play

This creates rhythm: **Build-up → Peak → Cooldown → Build-up**

### Ability Combos
**Abilities synergize when timed correctly:**

**Offensive Combo:**
1. Sensor Burst (see enemy plans, +15 heat)
2. Position to exploit revealed info
3. Overcharge Weapons (+20 heat)
4. Fire all weapons (high heat but devastating)
5. Total: ~125 heat (critical but worth it for kill)

**Defensive Combo:**
1. Shield Boost when detecting incoming fire (+25 heat)
2. Evasive Maneuver to break firing solution (+35 heat)
3. PD Override to catch remaining missiles (+30 heat)
4. Total: 90 heat (high but survives alpha strike)

**Sustained Combat:**
1. Fire weapons conservatively (40 heat/turn)
2. Stay in safe zone
3. Use abilities sparingly when opportunity presents
4. Save Emergency Cooling for true emergency

---

## Next Steps

1. **Review this design** - Does this align with your vision?
2. **Prioritize features** - Any systems to add/remove/modify?
3. **Start implementation** - Begin with modal command interface + heat system + ability system
4. **Iterate rapidly** - Prototype → test → refine cycle

---

## Appendix: Quick Reference Tables

### Ability Reference Card
| Ability | Heat Cost | Cooldown | Effect |
|---------|-----------|----------|--------|
| Emergency Cooling | 0 | 4 turns | -50 heat instantly |
| Shield Boost | +25 | 3 turns | +100 temp shields (2T) |
| Evasive Maneuver | +35 | 2 turns | 90° turn, 30 unit move |
| Overcharge Weapons | +20 | 3 turns | +50% damage, +100% heat |
| Sensor Burst | +15 | 2 turns | Reveal enemy plans |
| PD Override | +30 | 3 turns | 2x PD engagement rate |

### Weapon Reference Card
| Weapon | Arc | Range | Speed | Damage | Heat | Cooldown | Ammo |
|--------|-----|-------|-------|--------|------|----------|------|
| Rail Gun | 360° | 30 | 40 u/s | 20 | +15 | None | ∞ |
| Newtonian Cannon | 60° | 20 | 15 u/s | 40 | +30 | None | ∞ |
| Torpedo | 30° | 50 | Homing | 80 | +25 | 3T | 6 |
| Missile Battery | 360° | 35 | Homing | 30 | +20 | 1T | 20 |

### Heat Penalty Reference
| Heat Range | Accuracy | Speed | Special |
|------------|----------|-------|---------|
| 0-59 | 100% | 100% | Normal |
| 60-79 | 90% | 100% | Minor warning |
| 80-99 | 80% | 90% | Sensor flicker |
| 100-119 | 60% | 75% | System warnings |
| 120-149 | 40% | 60% | 5 hull dmg/turn |
| 150+ | 20% | 50% | 20 hull dmg/turn |

---

**Document Version**: 1.0  
**Last Updated**: November 18, 2025  
**Status**: Ready for Prototype Implementation