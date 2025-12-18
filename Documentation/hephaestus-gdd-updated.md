# Hephaestus: Game Design Document

## Core Concept
Turn-based tactical space combat commanding a single capital ship against enemy vessels. BSG Deadlock's planning-then-execution loop meets submarine warfare tactics with a heat-based weapon economy and Battletech-inspired damage system.

**Design Pillars:**
- Attrition warfare: Systems fail, performance degrades, ships become combat ineffective before destruction
- Heat as primary resource: Every offensive action has thermal cost
- Positioning matters: Attack vectors determine what gets damaged
- No visible dice: Determinism where players can see, hidden RNG for critical variety

---

## Fundamental Gameplay Loop

### Turn Structure (BSG Deadlock Style)

**1. Command Phase** - Plan all actions simultaneously
- Movement planning with Bezier curve projections
- Weapon targeting assignments
- Ability activations
- All ships (player + AI) plan in parallel
- Review current status (heat, damage, cooldowns)
- Commit plans when ready

**2. Simulation Phase** - Watch execution unfold
- All planned actions execute simultaneously
- No player input during execution
- 3-5 second duration for moves to complete
- Ships execute Bezier movement
- Abilities activate and apply effects
- Weapons fire when in arc
- Projectiles travel/home toward targets
- Point Defense engages incoming threats
- Damage resolves (shields, armor, structure, criticals)
- Heat accumulates from weapons/abilities
- Heat penalties apply immediately
- Passive cooling applies (-20 heat)
- Ability cooldowns tick down
- Phase ends when all movement complete and projectiles resolved
- Automatically transitions to next Command Phase

---

## Damage System

### Design Philosophy
**Wrath of Khan, not arcade shooter.** Battles are wars of attrition where systems fail, performance degrades, and ships become combat ineffective long before they're destroyed. Every hit matters. Positioning determines what gets damaged.

---

### Ship Sections

Capital ships are divided into discrete hit sections, each with its own armor, structure, and mounted systems.

**Standard Section Layout:**

| Section | Description |
|---------|-------------|
| Fore | Front of ship, forward weapons, sensors |
| Aft | Rear of ship, engines, radiators |
| Port | Left side, broadside weapons |
| Starboard | Right side, broadside weapons |
| Dorsal | Top deck, PD coverage |
| Ventral | Underbelly, PD coverage |
| Core | Interior, protected, contains reactor |

**Implementation:** Each section is defined by human-placeable trigger colliders (child GameObjects). Designers can shape sections to match ship geometry.

---

### Section Properties

Each section tracks:

| Property | Description |
|----------|-------------|
| Armor | Ablative outer layer, absorbs damage first |
| Structure | Internal integrity, damage here triggers critical rolls |
| Mounted Systems | List of systems installed in this section |
| Slot Layout | Size-based allocation of systems within structure |

---

### Hit Location (Attack Vector)

**Deterministic:** Where a shot comes from determines which section it hits. No random hit location rolls.

Projectile collides with section's trigger volume → That section takes damage.

**Tactical Implications:**
- Flanking attacks target weaker side armor
- Head-on charges trade fore-section damage
- Elevation matters (dorsal vs ventral)
- Protecting damaged sections becomes critical

---

### Damage Flow

```
Incoming Damage
       ↓
[Shields - single bubble pool]
       ↓
Shield depleted? → Remaining damage continues
       ↓
[Section Armor]
       ↓
Armor depleted? → Overflow damage continues
       ↓
[Section Structure]
       ↓
Structure damage → Critical hit roll
       ↓
Structure depleted? → Section breached, all systems destroyed
```

**Overflow Rule:** Damage carries through layers. A 50 damage hit against 20 remaining armor deals 20 to armor, 30 to structure.

---

### Shield System

**Bubble Shield:** Single pool protecting entire ship from all angles.

| Property | Description |
|----------|-------------|
| Shield HP | Single pool (e.g., 200 for Hephaestus) |
| Coverage | All sections, all angles |
| Regeneration | None (shields do not passively regenerate) |
| Collapse | At 0 HP, shields are down |
| Reboot | Only via Shield Boost ability |

**Design Note:** Once shields fall, the knife fight begins. Getting them back costs significant heat.

---

### Critical Hit System

**Trigger:** Each attack that deals structure damage rolls once for a critical hit.

**Size-Based Slots:** Each section's structure value represents "critical space." Systems mounted there occupy slots based on their size. Empty space represents safe hits.

**Critical Roll Process:**
1. Attack deals damage to structure
2. Roll 1 to [Section Structure Value]
3. Check result against section's slot layout
4. If roll lands on a system → System takes damage
5. If roll lands on empty space → No critical effect

**Example:**
Section has 40 structure containing:
- Engine (size 15, slots 1-15)
- Radiator (size 5, slots 16-20)
- Empty (slots 21-40)

Roll 12 → Engine hit
Roll 18 → Radiator hit
Roll 35 → Empty space, no critical

---

### System Degradation

**Three States (Middle State Toggleable):**

| State | Effect |
|-------|--------|
| Operational | 100% effectiveness |
| Damaged | 50% effectiveness |
| Destroyed | Non-functional |

**Configuration:** `UseDamagedState` bool allows testing with or without the middle state.

- When `UseDamagedState = true`: First critical hit → Damaged, Second critical hit → Destroyed
- When `UseDamagedState = false`: First critical hit → Destroyed

**Damaged Effects by System Type:**

| System | Damaged Effect |
|--------|----------------|
| Weapon | Cooldown doubled OR damage halved |
| Main Engine | Max speed halved, turn rate halved |
| Radiator | Passive cooling halved |
| Sensors | Targeting range halved, Sensor Burst disabled |
| PD Turret | Engagement rate halved |
| Reactor Core | Heat capacity reduced OR generates passive heat |

**Destroyed Systems as Bullet Sponges:**
When a system is destroyed, it remains in its slot range. Future critical rolls that land on destroyed systems hit wreckage—no additional effect, but protects other systems in the section.

---

### Core Section

**Protected Interior:** The Core section cannot be hit directly under normal circumstances.

**Core Access Rules:**

| Condition | Effect |
|-----------|--------|
| Normal attack | Cannot hit Core, hits outer section |
| Adjacent section breached | Attacks from that angle can hit Core |
| Lucky shot | 5% chance any structure hit punches through to Core |

**Core Contents:**
- Reactor Core (primary system)
- If Reactor Core destroyed → Instant Core Breach → Ship destroyed

---

### Ammunition System

**Ammo as Critical Slots:** Weapons with limited ammunition have their magazines stored as separate systems.

| Ammo Type | Size | Breach Damage |
|-----------|------|---------------|
| Torpedo Magazine | 8 | 40 internal damage |
| Missile Magazine | 6 | 25 internal damage |

**Ammo Breach Effects:**

| State | Effect |
|-------|--------|
| Damaged | Weapon cooldown doubled, feed compromised |
| Destroyed | Magazine detonates, internal damage to section structure |

**Internal Damage:** Bypasses armor, applies directly to section structure. Can cause cascading section breach.

**CASE System (Future):**

| System | Size | Effect |
|--------|------|--------|
| CASE | 2 | Reduces ammo breach damage by 75% |

---

### Ship Death Conditions

**Two Ways to End a Ship:**

| Condition | Result |
|-----------|--------|
| Core Breach | Reactor destroyed, ship explodes, debris field |
| Combat Ineffective | All weapons AND engines destroyed, ship disabled |

**Future Salvage System:**

| End State | Salvage Value |
|-----------|---------------|
| Disabled | High (intact systems recoverable) |
| Destroyed | Low (scrap only) |

---

### Edge Cases (Deferred to Prototype Testing)

- Shots hitting gaps between section volumes
- Collider complexity (box vs mesh)
- Boundary overlap behavior between sections

---

## Heat System

### Core Heat Mechanic
**Weapons and most abilities generate Heat. Heat is THE primary resource.**

- **Heat Capacity**: Ships have a heat threshold (0-150 scale for Hephaestus)
- **Passive Cooling**: -20 heat per turn automatically
- **No Hard Cap**: Players CAN overheat if they want—consequences scale with severity

### Heat Penalty Tiers

| Heat Range | Accuracy | Speed | Special |
|------------|----------|-------|---------|
| 0-59 (Safe) | 100% | 100% | Normal operation |
| 60-79 (Minor) | 90% | 100% | Warning indicators |
| 80-99 (Moderate) | 80% | 90% | Sensor flicker |
| 100-119 (Severe) | 60% | 75% | System warnings |
| 120-149 (Critical) | 40% | 60% | 5 hull damage/turn |
| 150+ (Catastrophic) | 20% | 50% | 20 hull damage/turn |

---

## Weapon Systems

### Weapon Reference

| Weapon | Arc | Range | Speed | Damage | Heat | Cooldown | Ammo |
|--------|-----|-------|-------|--------|------|----------|------|
| Rail Gun | 360° | 30 | 40 u/s | 20 | 15 | None | ∞ |
| Newtonian Cannon | 60° | 20 | 15 u/s | 40 | 30 | None | ∞ |
| Torpedo | 30° | 25 | 5 u/s (homing) | 50 | 25 | 3T | 6 |
| Missile Battery | 360° | 35 | 8 u/s (homing) | 15 | 10 | 1T | 20 |

*Note: All damage values are placeholders pending balance testing.*

### Weapon Characteristics

**Rail Gun (Light Weapon)**
- **Arc**: 360° turret mount
- **Range**: Long (30 units)
- **Hit**: Fast projectile (40 units/second)—visible but nearly impossible to dodge
- **Damage**: 20
- **Heat**: +15
- **Cooldown**: None (can fire every turn)
- **Characteristics**: Precision weapon, good for finishing damaged targets

**Newtonian Cannon (Heavy Weapon)**
- **Arc**: 60° spinal mount (must face target directly—30° left/right)
- **Range**: Medium (20 units)
- **Hit**: Projectile travels at 15 units/second
- **Damage**: 40
- **Heat**: +30
- **Cooldown**: None (can fire every turn)
- **Characteristics**: Massive spinal-mounted cannon. High damage but requires precise positioning

**Torpedo (Strategic Weapon)**
- **Arc**: Forward firing only (±30°)
- **Range**: Medium-Long (25 units)
- **Hit**: Slow homing projectile (5 units/second, 90°/sec turn rate)
- **Damage**: 50 (devastating)
- **Heat**: +25
- **Cooldown**: 3 turns after firing
- **Ammo**: Limited (6 torpedoes total)
- **Characteristics**: Multi-turn travel time, can be intercepted by point defense

**Missile Battery (Tactical Weapon)**
- **Arc**: 360° launcher
- **Range**: Long (35 units)
- **Hit**: Fast homing (8 units/second, 90°/sec turn rate)
- **Damage**: 15
- **Heat**: +10
- **Cooldown**: 1 turn after firing
- **Ammo**: Limited (20 missiles total)
- **Characteristics**: Reliable damage, harder to intercept than torpedoes

### Targeting System

During Command Phase:
- Click enemy ship to select as target
- **Number keys 1-4** fire specific weapon groups at target
- **A key** fires Alpha Strike (all weapons)
- Visual indicators show firing arcs and predicted hit locations
- Weapons outside arc show "OUT OF ARC" warning
- Weapons on cooldown show "COOLDOWN: X TURNS" warning
- Projectile weapons show predicted intercept point based on target's planned movement

### Weapon Spin-Up System

**All weapons and abilities have activation delay during Simulation Phase.**

| Weapon/Ability | Spin-Up Time |
|----------------|--------------|
| Rail Gun | 0.2s |
| Newtonian Cannon | 0.5s |
| Missile Battery | 0.4s |
| Torpedo Launcher | 1.0s |
| Emergency Cooling | 0.1s |
| Shield Boost | 0.3s |
| Evasive Maneuver | 0.0s |
| Overcharge Weapons | 0.5s |
| Sensor Burst | 0.2s |
| PD Override | 0.3s |

**Interruption:** Only ship destruction cancels pending actions.

---

## Ability System

### Design Philosophy
**Abilities use Heat AND have Cooldowns, except Emergency Cooling which only has cooldown.**

This creates two resource management layers:
1. **Heat**: Immediate tactical cost (can I afford this now?)
2. **Cooldown**: Strategic timing cost (when will I need this again?)

### Ability Catalog

| Ability | Heat | Cooldown | Effect |
|---------|------|----------|--------|
| Emergency Cooling | 0 | 4 turns | -50 heat instantly |
| Shield Boost | 70 | 5 turns | +100 shields (fixed, tunable). Only usable when shields at 0 |
| Evasive Maneuver | 35 | 2 turns | 90° turn, 30 unit move override |
| Overcharge Weapons | 20 | 3 turns | +50% damage, +100% heat generation |
| Sensor Burst | 15 | 2 turns | Reveal enemy planned moves |
| PD Override | 30 | 3 turns | 2x PD engagement rate |

### Ability Details

**Emergency Cooling (Defensive)**
- **Effect**: Instantly vent -50 heat
- **Heat Cost**: None (this is the heat management tool)
- **Cooldown**: 4 turns
- **Use Case**: Prevent catastrophic overheat, enable alpha strike next turn
- **Tactical Note**: Long cooldown makes timing critical

**Shield Boost (Defensive)**
- **Effect**: Restore fixed amount of shields (default: 100 HP)
- **Heat Cost**: +70
- **Cooldown**: 5 turns
- **Restriction**: Only usable when shields are at 0
- **Use Case**: Emergency shield reboot after collapse
- **Tactical Note**: Massive heat cost means sacrificing offense for defense

**Evasive Maneuver (Mobility)**
- **Effect**: Ignore movement constraints for one turn (can turn 90°, move 30 units)
- **Heat Cost**: +35
- **Cooldown**: 2 turns
- **Use Case**: Escape bad positioning, flank enemies, dodge incoming torpedoes
- **Tactical Note**: High heat cost limits follow-up attacks

**Overcharge Weapons (Offensive)**
- **Effect**: +50% weapon damage for this turn, +100% heat generation
- **Heat Cost**: +20 (plus doubled weapon heat)
- **Cooldown**: 3 turns
- **Use Case**: Alpha strike to finish enemy, breakthrough moment
- **Tactical Note**: Extremely risky heat-wise, can push into catastrophic range

**Sensor Burst (Utility)**
- **Effect**: Reveal all enemy planned moves for this turn
- **Heat Cost**: +15
- **Cooldown**: 2 turns
- **Use Case**: Counter-positioning, predict enemy alpha strikes
- **Tactical Note**: Information advantage but eats into offensive heat budget

**Point Defense Override (Defensive)**
- **Effect**: Double PD engagement rate (4 targets per turret instead of 2)
- **Heat Cost**: +30
- **Cooldown**: 3 turns
- **Use Case**: Counter missile/torpedo saturation attacks
- **Tactical Note**: Proactive defense against predicted incoming barrage

### Ability Management

- Abilities are activated during Command Phase
- Effects trigger during Simulation Phase
- Cooldowns tick down at start of each Command Phase
- Heat is added when ability activates
- Multiple abilities can be used in same turn (heat permitting)
- UI shows heat projection: "If you fire all planned weapons + abilities, heat will be: X/150"

---

## Point Defense System

### Core Concept
**Point Defense (PD) turrets are reactive, not player-controlled.**

### PD Characteristics

| Property | Value |
|----------|-------|
| Coverage | Arc-based per turret |
| Engagement | Automatic during simulation |
| Base Intercept | 60% per target |
| Targets/Turn | 2 per turret (4 with PD Override) |
| Heat Cost | None (independent system) |

**Intercept Modifiers:**

| Target Type | Modifier |
|-------------|----------|
| Torpedo (slow) | +20% |
| Missile | Base 60% |
| Cannon shell | -30% |

**Degradation:**
- Damaged PD Turret: Engagement rate halved
- Destroyed PD Turret: Gap in coverage

### PD Configuration (Hephaestus)

- 2x Aft PD turrets
- 2x Port PD turrets
- 2x Starboard PD turrets
- 2x Dorsal PD turrets
- 2x Ventral PD turrets
- **Total: 8 PD turrets**

### Tactical Implications

- **Saturation Attacks**: Fire multiple missiles to overwhelm PD coverage
- **Arc Exploitation**: Attack from angles with weaker PD coverage
- **PD Damage**: PD turrets can be individually destroyed by enemy fire
- **Blind Spots**: Lose PD turrets = create vulnerable attack vectors

---

## Movement System

### Movement Properties

- **Projection-based planning**: Green ghost ship shows planned end position
- **Bezier curve movement**: Ships follow smooth cubic Bezier arcs
- **Movement constraints**: 5-20 unit range, 45° max turn
- **Path visualization**: Cyan line renderer shows planned movement arc
- **Collision indication**: Projection turns red when collision detected
- **Facing control**: Ships rotate to face movement direction
- **No cost to move**: Movement is always available each turn

### Movement Controls

- **Drag projection** to plan destination
- **E**: Elevation adjustment mode
- **R**: Rotation adjustment mode
- **Mouse Scroll**: Adjust elevation
- **Arrow Keys**: Adjust rotation
- **Enter/Space**: Confirm movement
- **Esc**: Cancel/Deselect

---

## Player Ship: The Hephaestus

### Base Stats

| Stat | Value |
|------|-------|
| Shields | 200 HP (bubble) |
| Total Armor | 460 HP |
| Total Structure | 300 HP (270 outer + 30 core) |
| Heat Capacity | 150 |
| Speed | 20 units/turn max |
| Turn Rate | 45° max |

---

### Section Layout

**FORE (Armor: 100 / Structure: 50)**

| Slots | System | Size |
|-------|--------|------|
| 1-8 | Newtonian Cannon | 8 |
| 9-16 | Torpedo Launcher | 8 |
| 17-24 | Torpedo Magazine | 8 |
| 25-30 | Sensors | 6 |
| 31-50 | Empty | 20 |

Critical chance: 60%

---

**AFT (Armor: 60 / Structure: 40)**

| Slots | System | Size |
|-------|--------|------|
| 1-15 | Main Engine | 15 |
| 16-20 | Radiator | 5 |
| 21-25 | Radiator | 5 |
| 26-29 | PD Turret | 4 |
| 30-33 | PD Turret | 4 |
| 34-40 | Empty | 7 |

Critical chance: 82.5%

---

**PORT (Armor: 80 / Structure: 50)**

| Slots | System | Size |
|-------|--------|------|
| 1-5 | Rail Gun | 5 |
| 6-11 | Missile Battery | 6 |
| 12-17 | Missile Magazine | 6 |
| 18-21 | PD Turret | 4 |
| 22-25 | PD Turret | 4 |
| 26-50 | Empty | 25 |

Critical chance: 50%

---

**STARBOARD (Armor: 80 / Structure: 50)**

| Slots | System | Size |
|-------|--------|------|
| 1-5 | Rail Gun | 5 |
| 6-11 | Missile Battery | 6 |
| 12-17 | Missile Magazine | 6 |
| 18-21 | PD Turret | 4 |
| 22-25 | PD Turret | 4 |
| 26-50 | Empty | 25 |

Critical chance: 50%

---

**DORSAL (Armor: 70 / Structure: 40)**

| Slots | System | Size |
|-------|--------|------|
| 1-4 | PD Turret | 4 |
| 5-8 | PD Turret | 4 |
| 9-40 | Empty | 32 |

Critical chance: 20%

---

**VENTRAL (Armor: 70 / Structure: 40)**

| Slots | System | Size |
|-------|--------|------|
| 1-4 | PD Turret | 4 |
| 5-8 | PD Turret | 4 |
| 9-40 | Empty | 32 |

Critical chance: 20%

---

**CORE (Armor: 0 / Structure: 30)**

| Slots | System | Size |
|-------|--------|------|
| 1-20 | Reactor Core | 20 |
| 21-30 | Empty | 10 |

Critical chance: 66.7% (if accessed)

Access: Adjacent section breached OR 5% lucky shot

---

### Hephaestus Weapon Summary

| Weapon | Count | Location |
|--------|-------|----------|
| Rail Gun | 2 | Port, Starboard |
| Newtonian Cannon | 1 | Fore |
| Torpedo Launcher | 1 | Fore |
| Missile Battery | 2 | Port, Starboard |
| PD Turret | 8 | Aft(2), Port(2), Starboard(2), Dorsal(2), Ventral(2) |

---

### System Size Reference

| System Type | Size | Notes |
|-------------|------|-------|
| Reactor Core | 20 | Destruction = ship kill |
| Main Engine | 15 | Critical for mobility |
| Newtonian Cannon | 8 | Spinal mount |
| Torpedo Launcher | 8 | Forward weapon |
| Torpedo Magazine | 8 | Breach: 40 damage |
| Missile Battery | 6 | Turret mount |
| Missile Magazine | 6 | Breach: 25 damage |
| Sensors | 6 | Targeting systems |
| Rail Gun | 5 | Compact turret |
| Radiator | 5 | Heat management |
| PD Turret | 4 | Point defense |

---

## Enemy Ships

### Frigate (Light)

| Stat | Value |
|------|-------|
| Shields | 100 HP |
| Heat Capacity | 120 |
| Speed | 25 units/turn |
| Turn Rate | 60° |

**Weapons:**
- 2x Rail Guns
- 1x Missile Battery

**Point Defense:**
- 4x PD turrets (limited coverage)

**Abilities:**
- Emergency Cooling (4T cooldown)
- Evasive Maneuver (2T cooldown)

*Section layout to be designed after prototype validation.*

---

### Destroyer (Medium)

| Stat | Value |
|------|-------|
| Shields | 150 HP |
| Heat Capacity | 140 |
| Speed | 18 units/turn |
| Turn Rate | 45° |

**Weapons:**
- 1x Rail Gun
- 2x Newtonian Cannons
- 1x Missile Battery

**Point Defense:**
- 6x PD turrets (good coverage)

**Abilities:**
- Emergency Cooling (4T cooldown)
- Shield Boost (5T cooldown)
- Overcharge Weapons (3T cooldown)

*Section layout to be designed after prototype validation.*

---

### Battleship (Heavy)

| Stat | Value |
|------|-------|
| Shields | 250 HP |
| Heat Capacity | 180 |
| Speed | 15 units/turn |
| Turn Rate | 30° |

**Weapons:**
- 3x Rail Guns
- 2x Newtonian Cannons
- 2x Torpedo Launchers
- 2x Missile Batteries

**Point Defense:**
- 12x PD turrets (overlapping coverage)

**Abilities:**
- Emergency Cooling (4T cooldown)
- Shield Boost (5T cooldown)
- Overcharge Weapons (3T cooldown)
- PD Override (3T cooldown)

*Section layout to be designed after prototype validation.*

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
- Section status diagram (showing damage state)

**Purpose:** Keep the view clean during planning, show only critical status.

---

#### State 2: Enemy Ship Selected

**What's visible:**
- All "Nothing Selected" elements PLUS:
- Enemy status (Hull, Shields, Section damage if intel available)
- Weapon Group firing interface:
  - Group 1 button [Hotkey: 1]
  - Group 2 button [Hotkey: 2]
  - Group 3 button [Hotkey: 3]
  - Group 4 button [Hotkey: 4]
  - Alpha Strike button [Hotkey: A]
- Each button shows:
  - Which weapons are in that group
  - Combined heat cost
  - Whether any weapons are on cooldown
  - Whether weapons are in arc
  - Ammo remaining (for limited weapons)
- Targeting lines from Hephaestus to selected enemy (color-coded by group)

**Interaction:**
- Click group button OR press number key → that group fires at this target
- Click Alpha Strike OR press A → all assigned weapons fire at this target
- Multiple enemies can be targeted simultaneously (Group 1 → Enemy A, Group 2 → Enemy B)

---

#### State 3: Hephaestus Selected

**What's visible:**
- All "Nothing Selected" elements PLUS:
- Weapon configuration panel (group assignments)
- Ability panel (expanded with full details)
- Ammo counts for limited weapons
- Section status with mounted systems

**Weapon Configuration Panel:**
```
WEAPONS:
Rail Gun 1        [Group: 1▼]  Heat: 15  Cooldown: Ready  Arc: 360°
Rail Gun 2        [Group: 1▼]  Heat: 15  Cooldown: Ready  Arc: 360°
Newtonian Cannon  [Group: 2▼]  Heat: 30  Cooldown: Ready  Arc: 60°
Torpedo Launcher  [Group: 3▼]  Heat: 25  Cooldown: 2T     Arc: 30°   Ammo: 5/6
Missile Battery 1 [Group: 4▼]  Heat: 10  Cooldown: Ready  Arc: 360°  Ammo: 18/20
Missile Battery 2 [Group: 4▼]  Heat: 10  Cooldown: 1T     Arc: 360°  Ammo: 18/20
```

**Ability Panel:**
```
ABILITIES:
[1] Emergency Cooling    Cooldown: Ready      Heat: 0
[2] Shield Boost         Cooldown: 1T         Heat: +70  (Shields must be 0)
[3] Evasive Maneuver     Cooldown: Ready      Heat: +35
[4] Overcharge Weapons   Cooldown: 2T         Heat: +20
[5] Sensor Burst         Cooldown: Ready      Heat: +15
[6] PD Override          Cooldown: 3T         Heat: +30
```

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

3. **Potential Heat** (lighter preview, only on hover):
   - Shows what heat would be if you activate the thing you're hovering over
   - Disappears when not hovering

**Numeric Display:**
- `40/150` (current heat / max capacity)
- `+60` (planned heat this turn, if any)
- Hover for detailed breakdown: "Rail Guns +30, Cannon +30"

**Position:** Top-center of screen (primary focus, always visible)

---

### Section Status Display

**Visual ship diagram showing:**
- All 7 sections (Fore, Aft, Port, Starboard, Dorsal, Ventral, Core)
- Color-coded status per section:
  - Green: Armor intact
  - Yellow: Armor damaged
  - Orange: Armor breached, structure taking damage
  - Red: Structure critical
  - Black: Section breached
- System icons within sections showing damage state
- Core section highlighted when exposed (adjacent section breached)

---

### Combat Log

Real-time feed showing:
- Hits by section ("Torpedo hit FORE section")
- Armor/structure damage numbers
- Critical hit notifications ("CRITICAL: Port Rail Gun DAMAGED")
- System destruction alerts ("WARNING: Missile Magazine DESTROYED - Internal explosion!")
- Ammo breach warnings
- Shield status changes

---

### Weapon Group System

#### Group Assignment

**Weapon Configuration Panel** (visible when Hephaestus selected):
- Each weapon has a dropdown/cycle button showing current group assignment
- Click button to cycle: `Unassigned → 1 → 2 → 3 → 4 → Unassigned`
- Weapons can only be in ONE group at a time
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
  - "LOW AMMO" warning for limited weapons
- Click button OR press hotkey → assigns that group to fire at selected target
- Visual: Colored line from Hephaestus to target (matches group color)

**Alpha Strike Button:**
- Fires ALL assigned weapons at selected target
- Shows total heat cost prominently
- Warning color if total heat would exceed safe threshold
- Hotkey: A

**Multi-targeting:**
- Can assign different groups to different enemies
- Example: Select Enemy A, press 1 → Select Enemy B, press 2
- Each group gets its own colored targeting line
- UI shows which groups are assigned where

---

### HUD Layout Specification

#### Top Bar (Always Visible)
```
┌─────────────────────────────────────────────────────────────────┐
│ Turn: 5          [COMMAND PHASE]          Heat: 40/150         │
│                                            [████████░░░░░]     │
│ Shields: 170/200    Section Status: [diagram]    +60 Planned   │
└─────────────────────────────────────────────────────────────────┘
```

#### Left Panel (When Hephaestus Selected)
```
┌─────────────────────────────────┐
│ WEAPON CONFIGURATION            │
│                                 │
│ Rail Gun 1      [Group: 1▼]    │
│   Heat: 15  Arc: 360°          │
│   Cooldown: Ready   STATUS: OK │
│                                 │
│ Rail Gun 2      [Group: 1▼]    │
│   Heat: 15  Arc: 360°          │
│   Cooldown: Ready   STATUS: OK │
│                                 │
│ Newtonian Cannon [Group: 2▼]   │
│   Heat: 30  Arc: 60°           │
│   Cooldown: Ready   STATUS: DMG│
│                                 │
│ Torpedo Launcher [Group: 3▼]   │
│   Heat: 25  Arc: 30°           │
│   Cooldown: 2T  Ammo: 5/6      │
│                                 │
│ Missile Battery 1 [Group: 4▼]  │
│   Heat: 10  Arc: 360°          │
│   Cooldown: Ready  Ammo: 18/20 │
│                                 │
│ Missile Battery 2 [Group: 4▼]  │
│   Heat: 10  Arc: 360°          │
│   Cooldown: 1T  Ammo: 18/20    │
└─────────────────────────────────┘
```

#### Right Panel (When Enemy Selected)
```
┌─────────────────────────────────┐
│ TARGET: Enemy Frigate Alpha     │
│                                 │
│ Shields: 0/100 (DOWN)          │
│ Section Status: [diagram]       │
│                                 │
│ ┌─────────────────────────────┐ │
│ │  [1] Group 1                │ │
│ │  Rail Guns x2               │ │
│ │  Heat: +30                  │ │
│ └─────────────────────────────┘ │
│                                 │
│ ┌─────────────────────────────┐ │
│ │  [2] Group 2                │ │
│ │  Newtonian Cannon (DMG)     │ │
│ │  Heat: +30                  │ │
│ └─────────────────────────────┘ │
│                                 │
│ ┌─────────────────────────────┐ │
│ │  [3] Group 3                │ │
│ │  Torpedo (ON COOLDOWN)      │ │
│ └─────────────────────────────┘ │
│                                 │
│ ┌─────────────────────────────┐ │
│ │  [4] Group 4                │ │
│ │  Missiles x2                │ │
│ │  Heat: +20  Ammo: 36        │ │
│ └─────────────────────────────┘ │
│                                 │
│ ┌─────────────────────────────┐ │
│ │  [A] ALPHA STRIKE           │ │
│ │  All weapons                │ │
│ │  Heat: +50                  │ │
│ └─────────────────────────────┘ │
└─────────────────────────────────┘
```

#### Bottom Bar (Always Visible - Abilities)
```
┌─────────────────────────────────────────────────────────────────┐
│ [1] Emergency      [2] Shield      [3] Evasive                  │
│     Cooling            Boost           Maneuver                 │
│     Ready              1T              Ready                    │
│     Heat: 0            Heat: +70       Heat: +35                │
│                        (Shields=0)                              │
│                                                                 │
│ [4] Overcharge     [5] Sensor      [6] PD Override              │
│     Weapons            Burst           2T                       │
│     2T                 Ready           Heat: +30                │
│     Heat: +20          Heat: +15                                │
└─────────────────────────────────────────────────────────────────┘
```

---

### Visual Feedback During Simulation Phase

**Movement:**
- Ships glide along Bezier curves
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
- Heat bar updates in real-time

**Damage Resolution:**
- Shield impacts: Blue ripple on shield surface
- Armor hits: Sparks, metallic impact
- Structure hits: Debris particles, hull section darkens
- Critical hits: Larger explosion, system sparks
- Ammo breach: Internal explosion effect, fire
- Section breach: Major structural failure visual
- Floating damage numbers (optional toggle)
- Combat log updates in real-time

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

**Movement:**
- Drag projection ghost to plan destination
- E: Elevation adjustment mode
- R: Rotation adjustment mode
- Enter/Space: Confirm movement

**Weapon Targeting:**
- Select enemy → Press number key (1-4) to fire that group
- Select enemy → Press A for Alpha Strike
- Multiple enemies: Select A, press 1 → Select B, press 2 (multi-target)

**Weapon Configuration:**
- Select Hephaestus → Click weapon's group dropdown to cycle assignment

**Abilities:**
- Press number keys 1-6 to activate abilities
- OR click ability icon in bottom bar

**Camera:**
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

**Damage Visibility:**
- Section status always visible in minimal form
- Detailed breakdown when own ship selected
- Enemy damage state shown when targeting
- Combat log provides moment-to-moment detail

**Information Hierarchy:**
- Critical: Always visible (Heat, Shields, Section Status, Phase)
- Important: Visible in relevant context (Weapons when targeting)
- Detailed: Available on demand (Full section breakdown when Hephaestus selected)

**Color Coding:**
- Weapon Groups: Blue, Red, Green, Yellow
- Heat Levels: Green → Yellow → Orange → Red
- Section Status: Green → Yellow → Orange → Red → Black
- System Status: Green (OK), Yellow (Damaged), Red (Destroyed)
- Availability: Full color (ready), Gray (cooldown), Yellow border (heat warning)
- Phase: Cyan (Command), Orange (Simulation)

---

## AI Behavior Framework

### AI Must Use Same Systems
**Critical Design Rule**: AI operates under identical constraints.

- AI plans movement using same Bezier curves and constraints
- AI manages heat identically (can overheat)
- AI manages ability cooldowns and heat costs
- AI weapons fire during same Simulation phase
- AI benefits from/suffers from PD coverage
- AI ships take section damage, suffer criticals, lose systems
- AI uses same ability system with heat costs and cooldowns

### AI Difficulty Tiers

#### Basic AI
- Moves toward player if out of range
- Fires all weapons every turn (aggressive heat management)
- Uses Emergency Cooling when heat > 100
- Doesn't use other abilities
- Predictable movement patterns
- No awareness of own section damage

#### Intermediate AI
- Maintains optimal firing range for weapons
- Basic heat management (stops firing at 80 heat)
- Uses Evasive Maneuver when shields low
- Uses Shield Boost when shields collapse
- Attempts to flank player
- Protects damaged sections by angling away

#### Advanced AI
- Predicts player movement based on patterns
- Sophisticated heat management (balances offense/defense)
- Coordinates with other AI ships (focus fire on damaged sections)
- Exploits PD blind spots
- Targets player's damaged/breached sections
- Uses abilities tactically:
  - Emergency Cooling before planned alpha strike
  - Overcharge Weapons when target section is exposed
  - Sensor Burst to counter player evasive maneuvers
  - PD Override when detecting incoming missile salvo

---

## Combat Flow Example

### Turn 1 - Opening Engagement

**Command Phase:**
1. Player plans movement: Arc toward enemy flank
2. Player assigns targets:
   - Rail Guns → Enemy Frigate (+30 heat)
   - Newtonian Cannon → Enemy Frigate (+30 heat)
   - Total: +60 heat this turn (within safe zone)
3. Player confirms

**Simulation Phase:**
- Ships execute Bezier movement simultaneously
- Weapons spin up, then fire when in arc
- Rail guns hit first (0.2s spin-up), strike Frigate's Port section
- Newtonian Cannon fires (0.5s spin-up), hits Frigate's Fore section
- Damage applies: Frigate shields absorb first hits, then collapse
- Remaining damage hits Port armor
- Enemy returns fire: 2 missiles incoming
- Player PD engages: Destroys 1 missile, 1 gets through
- Missile hits Starboard section, absorbed by shields
- Heat updates: +60 heat from weapons, -20 passive cooling = 40 net heat
- Phase ends, transitions to Command Phase

### Turn 3 - Shields Down, Knife Fight Begins

**Player Status:**
- Shields: 0 (collapsed last turn)
- Heat: 55
- All sections intact

**Enemy Frigate Status:**
- Shields: 0
- Port section: Armor breached, structure damaged
- Port Rail Gun: DAMAGED (crit from Turn 2)

**Command Phase:**
- Player faces choice:
  - **Aggressive**: Fire all weapons at Frigate's damaged Port section (+60 heat = 115 total, Severe penalties)
  - **Tactical**: Fire only Rail Guns at Port (+30 heat = 85 total), save heat
  - **Defensive**: Use Shield Boost (+70 heat = 125 total, Critical tier, but shields back)

**Player chooses Aggressive - finish the Frigate**

**Simulation Phase:**
- All weapons target Frigate Port section
- Damage overwhelms remaining structure
- Critical roll: Missile Magazine hit → DESTROYED
- Internal explosion: 25 damage to Port structure
- Port section BREACHED - all systems destroyed
- Frigate loses: Port Rail Gun (already damaged), Port Missile Battery, 2 Port PD turrets
- Frigate now has exposed Core from port angle
- Player heat: 115 (Severe penalties active)

### Turn 4 - The Kill

**Command Phase:**
- Player at 95 heat (115 - 20 cooling)
- Frigate Core exposed from Port side
- Player positions to maintain Port angle
- Fires Newtonian Cannon at Frigate (only needs one good hit)

**Simulation Phase:**
- Cannon hits Frigate Port (breached) → damage goes to Core
- Core structure takes damage
- Critical roll lands on Reactor Core
- **REACTOR CORE DESTROYED**
- **CORE BREACH - FRIGATE DESTROYED**

---

## Development Priorities

### Phase 0: Foundation (Complete)
- ✓ Turn-based gameplay with Command/Simulation phases
- ✓ BSG Deadlock-style movement planning
- ✓ Bezier curve movement execution
- ✓ Camera system

### Phase 1: Core Combat (Complete)
- ✓ Heat system with tiers and penalties
- ✓ Ability system with cooldowns
- ✓ Basic weapon framework

### Phase 2: Weapon Systems (Complete)
- ✓ All four weapon types implemented
- ✓ Projectile system (ballistic and homing)
- ✓ Targeting UI

### Phase 3: Damage System (NEW)
- [ ] ShipSection component (armor, structure, mounted systems)
- [ ] Volume-based hit detection (trigger colliders per section)
- [ ] Damage flow (shields → armor → structure with overflow)
- [ ] Critical hit system (size-based slots, hidden roll)
- [ ] System degradation states (operational/damaged/destroyed)
- [ ] Core section with protection rules
- [ ] Ammunition breach mechanics
- [ ] Shield system refactor (bubble, no regen, ability reboot only)
- [ ] Section status UI
- [ ] Combat log

### Phase 4: Point Defense System
- [ ] PD turret component with arc coverage
- [ ] Threat detection for incoming projectiles
- [ ] Interception calculations
- [ ] Visual feedback for PD engagement
- [ ] PD damage/destruction integration with section system

### Phase 5: Enemy AI
- [ ] Basic AI controller using same ship systems
- [ ] AI decision-making for movement, targeting, abilities
- [ ] AI heat management
- [ ] AI awareness of section damage (own and enemy)
- [ ] Multiple enemy spawning
- [ ] Victory/defeat conditions

### Phase 6: Polish & VFX
- [ ] Weapon fire effects
- [ ] Damage effects (impacts, explosions, fire)
- [ ] Section breach visuals
- [ ] Heat visualization on ship model
- [ ] Audio implementation
- [ ] Screen shake and juice

### Phase 7: Balance & MVP
- [ ] Damage value tuning
- [ ] Heat cost balancing
- [ ] System size adjustments
- [ ] Section armor/structure balancing
- [ ] Playtest iteration
- [ ] Bug fixing

---

## Success Metrics

The prototype is successful if:
- ✓ Fights feel like attrition, not burst damage races
- ✓ System degradation creates meaningful tactical decisions
- ✓ Positioning matters (attack vectors, protecting damaged sections)
- ✓ Ships become combat ineffective before destruction
- ✓ Both players and AI experience gradual performance loss
- ✓ Shield collapse creates dramatic phase shift in combat
- ✓ Critical hits feel impactful but not random/unfair
- ✓ Heat management remains central to decision-making
- ✓ Players can complete a 1v2 combat encounter
- ✓ 10-15 minute playtime per encounter
- ✓ Players want to replay

---

## Design Rationale

### Why Battletech-Inspired Damage?
**Attrition creates drama.**
- Binary HP systems make fights feel like number races
- Section-based damage creates tactical positioning
- System degradation means even "winning" ships are scarred
- Matches the Wrath of Khan fantasy perfectly

### Why Attack Vector Over Random Hit Location?
**Player agency over frustration.**
- Random hit locations feel unfair ("Why did it hit my reactor?!")
- Attack vectors reward good positioning
- Creates meaningful flanking and angling decisions
- Hidden crit rolls add variety without feeling arbitrary

### Why Bubble Shields That Don't Regen?
**Clear phase transition.**
- Shields up = safe, maneuvering phase
- Shields down = knife fight, every hit matters
- Regen muddies this transition
- Ability-based reboot makes shield recovery a dramatic choice

### Why Size-Based Critical Slots?
**Elegant probability without visible dice.**
- Bigger systems = bigger targets (intuitive)
- Empty space = padding/protection (design choice)
- System layout becomes meaningful ship design
- No "you rolled a 2" moments

### Why Destroyed Systems Stay in Slots?
**Prevents death spirals, adds depth.**
- First crits are scary (systems break)
- Later crits hit wreckage (diminishing returns)
- Focused fire has natural limits
- Ships stay in the fight longer

---

## Quick Reference Tables

### Ability Reference

| Ability | Heat | Cooldown | Effect |
|---------|------|----------|--------|
| Emergency Cooling | 0 | 4T | -50 heat |
| Shield Boost | 70 | 5T | +100 shields (only at 0) |
| Evasive Maneuver | 35 | 2T | 90° turn, 30u move |
| Overcharge Weapons | 20 | 3T | +50% dmg, +100% heat |
| Sensor Burst | 15 | 2T | Reveal enemy plans |
| PD Override | 30 | 3T | 2x PD rate |

### Weapon Reference

| Weapon | Arc | Range | Speed | Damage | Heat | CD | Ammo |
|--------|-----|-------|-------|--------|------|----|------|
| Rail Gun | 360° | 30 | 40 u/s | 20 | 15 | - | ∞ |
| Newtonian Cannon | 60° | 20 | 15 u/s | 40 | 30 | - | ∞ |
| Torpedo | 30° | 25 | 5 u/s | 50 | 25 | 3T | 6 |
| Missile Battery | 360° | 35 | 8 u/s | 15 | 10 | 1T | 20 |

### Heat Penalty Reference

| Heat | Accuracy | Speed | Special |
|------|----------|-------|---------|
| 0-59 | 100% | 100% | Normal |
| 60-79 | 90% | 100% | Minor |
| 80-99 | 80% | 90% | Moderate |
| 100-119 | 60% | 75% | Severe |
| 120-149 | 40% | 60% | 5 hull/turn |
| 150+ | 20% | 50% | 20 hull/turn |

### System Size Reference

| System | Size |
|--------|------|
| Reactor Core | 20 |
| Main Engine | 15 |
| Newtonian Cannon | 8 |
| Torpedo Launcher | 8 |
| Torpedo Magazine | 8 |
| Missile Battery | 6 |
| Missile Magazine | 6 |
| Sensors | 6 |
| Rail Gun | 5 |
| Radiator | 5 |
| PD Turret | 4 |

### Section Reference (Hephaestus)

| Section | Armor | Structure | Crit% |
|---------|-------|-----------|-------|
| Fore | 100 | 50 | 60% |
| Aft | 60 | 40 | 82.5% |
| Port | 80 | 50 | 50% |
| Starboard | 80 | 50 | 50% |
| Dorsal | 70 | 40 | 20% |
| Ventral | 70 | 40 | 20% |
| Core | 0 | 30 | 66.7% |

---

**Document Version**: 2.0
**Last Updated**: December 2024
**Status**: Ready for Engineering Review
