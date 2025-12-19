# Phase 3 Damage System - Testing Guide

---

## Overview

This guide provides step-by-step instructions for testing all Phase 3 Damage System features. Use the **Phase 3 Unified Test Level** for comprehensive testing, or individual test scenes for focused testing of specific systems.

**Test Count**: 188 automated tests
**Menu Location**: `Hephaestus > Testing > Create Phase 3 Unified Test Level`

---

## Quick Start

### Creating the Test Level

1. Open Unity
2. Go to menu: `Hephaestus > Testing > Create Phase 3 Unified Test Level`
3. A complete test scene will be created in the hierarchy
4. Press **Play** to enter Play Mode
5. Press **J** to toggle the test UI

### Test Controller Keyboard Shortcuts

| Key | Action |
|-----|--------|
| J | Toggle test UI on/off |
| K | Cycle through target ships (Player, Enemy1, Enemy2) |
| Tab | Switch between UI tabs |
| 1-7 | Quick select sections (Fore, Aft, Port, Starboard, Dorsal, Ventral, Core) |

---

## Test Scene Contents

The unified test level contains:

### Ships
- **PlayerShip** (Blue) - Your ship at origin
- **EnemyShip** (Red) - Primary target at (25, 0, 0)
- **EnemyShip2** (Red) - Secondary target at (15, 0, 15)

### Per-Ship Systems
Each ship is fully equipped with:
- All 7 sections (Fore, Aft, Port, Starboard, Dorsal, Ventral, Core)
- Shield System (200 max shields)
- Main Engine
- 2 Weapons (Newtonian Cannon, Missile Battery)
- Reactor Core
- Radiator
- Sensors
- Torpedo Magazine
- PD Turret

### Global Systems
- ProjectileManager
- TargetingController
- Full Damage UI (Section Panel, Combat Log, Shield Bar)

---

## Testing by Feature

### 1. Shield System Testing

**Location**: Combat Tab

**Tests to Perform**:

| Test | Steps | Expected Result |
|------|-------|-----------------|
| Shield Damage | Set damage to 50, click "Hit Shields" | Shield bar reduces by 50, combat log shows hit |
| Shield Overflow | Set damage to 250, click "Hit Shields" | Shields deplete (200), remaining 50 hits armor |
| Deplete Shields | Click "Deplete Shields" | Shields go to 0, "DEPLETED" shown |
| Restore Shields | Click "Restore Shields" | Shields return to 200/200 |

**Verification Checklist**:
- [ ] Shield bar updates in real-time
- [ ] Combat log records shield hits
- [ ] Shield depletion triggers UI indicator
- [ ] Overflow damage correctly passes to section armor

---

### 2. Section Damage Testing

**Location**: Sections Tab

**Tests to Perform**:

| Test | Steps | Expected Result |
|------|-------|-----------------|
| Armor Damage | Select Fore, click "Dmg" | Armor reduces, section turns yellow |
| Structure Damage | Deplete Fore armor, apply more damage | Structure takes damage, may trigger critical |
| Section Breach | Click "Brch" on any section | Section breached (red/black), structure at 0 |
| Breach All | Click "Breach All Outer Sections" | All 6 outer sections breached |

**Color Coding Reference**:
| Color | State |
|-------|-------|
| Green | Armor 100% |
| Yellow | Armor damaged (>50%) |
| Orange | Armor low (<50%) |
| Red | Structure damaged |
| Black/Dark Red | Breached |

**Verification Checklist**:
- [ ] Each section can be damaged independently
- [ ] Damage flows: Armor → Structure
- [ ] Breach occurs when structure hits 0
- [ ] Section colors update based on damage state
- [ ] Combat log records section damage and breaches

---

### 3. Critical Hit System Testing

**Location**: Systems Tab

**Tests to Perform**:

| Test | Steps | Expected Result |
|------|-------|-----------------|
| System Damage | Click "Dmg" next to Engine | Engine state: Operational → Damaged |
| System Destroy | Click "Dmg" twice on same system | System state: Damaged → Destroyed |
| Degradation Stats | Damage Engine, check stats | Speed/Turn Rate multipliers reduced |
| System Repair | Click "Fix" on damaged system | System returns to Operational |

**System Degradation Effects**:

| System | Damaged Effect | Destroyed Effect |
|--------|----------------|------------------|
| Engine | Speed/Turn x0.5 | Cannot move |
| Radiator | Cooling x0.5 | No passive cooling |
| Weapon | Cooldown x2 or Damage x0.5 | Cannot fire |
| Reactor | Heat Cap -30% or +10 heat/turn | CORE BREACH (ship death) |
| Sensors | Targeting x0.5 | Minimal targeting |
| Magazine | Normal | Internal explosion |
| PD Turret | Engagement x0.5 | No PD coverage |

**Verification Checklist**:
- [ ] Systems transition: Operational → Damaged → Destroyed
- [ ] Degradation multipliers update in real-time
- [ ] Damaged systems show yellow in list
- [ ] Destroyed systems show red in list
- [ ] Repair restores system to Operational

---

### 4. Core Protection Testing

**Location**: Core/Death Tab

**Tests to Perform**:

| Test | Steps | Expected Result |
|------|-------|-----------------|
| Core Protected | Attack Core with all sections intact | Damage redirected to outer section |
| Core Exposed | Breach Fore section, attack from Forward | Core takes damage |
| Direction Matters | Breach Fore, attack from Aft direction | Core still protected from rear |
| Lucky Shot | Attack sections repeatedly | ~5% chance to hit Core (watch combat log) |

**Core Access Rules**:
- Core cannot be hit directly unless adjacent section breached
- Adjacent section mapping:
  - Fore breach → Front attacks can hit Core
  - Aft breach → Rear attacks can hit Core
  - Port breach → Left attacks can hit Core
  - Starboard breach → Right attacks can hit Core
  - Dorsal breach → Top attacks can hit Core
  - Ventral breach → Bottom attacks can hit Core
- 5% "lucky shot" on any structure damage can bypass protection

**Verification Checklist**:
- [ ] Core Exposed indicator shows when adjacent breached
- [ ] Attack direction selector works
- [ ] Protected attacks show redirect in combat log
- [ ] Exposed attacks hit Core directly

---

### 5. Ship Death Conditions

**Location**: Core/Death Tab

**Death Conditions**:

| Condition | Trigger | Result |
|-----------|---------|--------|
| Core Breach | Reactor destroyed | Ship DESTROYED |
| Core Breached | Core section structure = 0 | Ship DESTROYED |
| All Sections Breached | All 7 sections breached | Ship DESTROYED |
| Combat Ineffective | All weapons AND engines destroyed | Ship DISABLED (not destroyed) |

**Tests to Perform**:

| Test | Steps | Expected Result |
|------|-------|-----------------|
| Reactor Death | Click "Destroy Reactor" | Ship destroyed, death cause: CoreBreach |
| Core Section Death | Breach Fore, then Core | Ship destroyed, death cause: CoreBreached |
| Combat Ineffective | Click "Combat Ineffective" | Ship disabled but not destroyed |

**Verification Checklist**:
- [ ] Destroyed ships show "DESTROYED" state
- [ ] Disabled ships show "DISABLED" state
- [ ] Death cause correctly identified
- [ ] Dead ships cannot be further controlled

---

### 6. Projectile Testing

**Location**: Projectiles Tab

**Tests to Perform**:

| Test | Steps | Expected Result |
|------|-------|-----------------|
| Ballistic | Click "Fire Ballistic" | Projectile spawns, travels to target, applies damage |
| Homing | Click "Fire Homing Missile" | Missile spawns, tracks target, applies damage |
| Burst Fire | Click "Fire 5 Projectiles" | 5 projectiles spawn, cumulative damage |

**Verification Checklist**:
- [ ] Projectiles spawn at shooter position
- [ ] Ballistic projectiles travel in straight line
- [ ] Homing missiles track target
- [ ] Damage applied on impact
- [ ] Combat log records hits

---

### 7. Damage UI Testing

**Location**: Status Tab + On-screen UI elements

**UI Elements to Verify**:

| Element | Location | What to Check |
|---------|----------|---------------|
| Section Status Panel | Screen left | Shows all 7 sections with colors |
| Shield Status Bar | Screen top | Shows shield value, "DEPLETED" when at 0 |
| Combat Log | Screen right | Scrolling log of combat events |
| Section Detail Popup | Click section | Shows mounted systems |

**Combat Log Categories**:
- **Hits**: Damage to shields/armor/structure
- **Criticals**: System damage events
- **Breaches**: Section breach events
- **System Damage**: System state changes

**Tests to Perform**:

| Test | Steps | Expected Result |
|------|-------|-----------------|
| Shield Bar | Damage shields | Bar reduces, shows current/max |
| Section Colors | Damage section | Color changes based on health |
| Combat Log | Apply damage | Entry appears in log |
| Log Categories | Use log buttons | Filter works correctly |
| Core Indicator | Breach section | "Core Exposed" warning appears |

**Verification Checklist**:
- [ ] All UI elements visible and updating
- [ ] Shield bar flashes on damage
- [ ] Section diagram reflects damage states
- [ ] Combat log scrolls and filters correctly
- [ ] Core exposure indicator appears when applicable

---

## Complete Test Scenarios

### Scenario 1: Light Skirmish

**Objective**: Test basic damage flow without ship death

1. Create unified test level, enter Play Mode
2. Target: EnemyShip
3. Apply 100 damage to shields (shields should be at 100)
4. Apply another 100 damage (shields deplete, 0 overflow)
5. Apply 50 damage (shields down, hits Fore armor)
6. **Verify**: Combat log shows shield depletion then armor damage

### Scenario 2: Critical Cascade

**Objective**: Test multiple system degradation

1. Create unified test level, enter Play Mode
2. Target: EnemyShip
3. Go to Systems tab
4. Damage Engine (should show Speed x0.5)
5. Damage Radiator (should show Cooling x0.5)
6. Damage Weapon (should show Cooldown x2)
7. **Verify**: All multipliers compound, ship significantly degraded

### Scenario 3: Core Breach

**Objective**: Test ship death via reactor destruction

1. Create unified test level, enter Play Mode
2. Target: EnemyShip
3. Go to Core/Death tab
4. Click "Destroy Reactor"
5. **Verify**: Ship shows DESTROYED, death cause = CoreBreach

### Scenario 4: Combat Ineffective

**Objective**: Test disabled state

1. Create unified test level, enter Play Mode
2. Target: EnemyShip
3. Go to Core/Death tab
4. Click "Combat Ineffective"
5. **Verify**: Ship shows DISABLED (not destroyed), cannot move or fire

### Scenario 5: Full Destruction Sequence

**Objective**: Test complete damage progression

1. Create unified test level, enter Play Mode
2. Target: EnemyShip
3. Deplete shields
4. Breach 3 sections (Fore, Port, Starboard)
5. Damage multiple systems
6. Attack Core from breached direction
7. Continue until Core section breached
8. **Verify**: Ship destroyed, all damage recorded in combat log

---

## Automated Tests

All 188 automated tests can be run via Unity Test Runner:

1. Open Test Runner: `Window > General > Test Runner`
2. Select "PlayMode" tab
3. Click "Run All"
4. All tests should pass (188/188)

### Test Categories

| Category | Count | Description |
|----------|-------|-------------|
| SectionTests | 12 | Section infrastructure |
| ShieldSystemTests | 12 | Shield mechanics |
| ProjectileDamageTests | 10 | Projectile integration |
| CriticalHitTests | 10 | Critical hit system |
| SystemDegradationTests | 10 | Degradation effects |
| CoreProtectionTests | 10 | Core protection rules |
| DamageUITests | 10 | UI updates |
| Phase3IntegrationTests | 10 | End-to-end integration |
| + Other existing tests | ~114 | Previous phase tests |

---

## Individual Test Scenes

For focused testing, individual test scenes are available:

| Menu Item | Purpose |
|-----------|---------|
| Create Section Test Scene | Section damage only |
| Create Shield Test Scene | Shield mechanics only |
| Create Projectile Damage Test Scene | Projectile integration |
| Create Critical Hit Test Scene | Critical hit system |
| Create Degradation Test Scene | System degradation |
| Create Core Protection Test Scene | Core access rules |
| Create Damage UI Test Scene | UI components |
| Create Damage System Integration Test Scene | Full integration |
| **Create Phase 3 Unified Test Level** | **ALL features** |

---

## Troubleshooting

### Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| UI not visible | UI toggled off | Press J to toggle UI |
| Damage not applying | Wrong target | Press K to switch targets |
| Systems not degrading | System already destroyed | Check system state in Systems tab |
| Core not taking damage | Not exposed | Breach adjacent section first |
| Tests failing | Script errors | Check Console for errors |

### Reset Options

- **Reset Target**: Resets current target ship to full health
- **Reset All**: Resets all ships and clears combat log
- **Repair All Systems**: Fixes all damaged/destroyed systems on target

---

## Test Sign-Off Checklist

Use this checklist to verify all Phase 3 features are working:

### Shield System
- [ ] Shields absorb damage before sections
- [ ] Overflow damage passes to sections
- [ ] Shield restoration works when depleted
- [ ] Shield bar UI updates correctly

### Section System
- [ ] All 7 sections take damage independently
- [ ] Armor absorbs before structure
- [ ] Section breach occurs at structure = 0
- [ ] Section colors reflect damage state

### Critical Hit System
- [ ] Structure damage triggers critical rolls
- [ ] Systems can be damaged and destroyed
- [ ] Degradation effects apply correctly
- [ ] Destroyed systems occupy slots (absorb hits)

### Core Protection
- [ ] Core protected when sections intact
- [ ] Core exposed when adjacent breached
- [ ] Attack direction affects access
- [ ] Lucky shots work (~5% chance)

### Ship Death
- [ ] Reactor destruction = Core Breach death
- [ ] Core section breach = death
- [ ] All sections breached = death
- [ ] All weapons + engines destroyed = disabled

### Damage UI
- [ ] Shield bar shows current/max
- [ ] Section diagram updates colors
- [ ] Combat log records all events
- [ ] Core exposure indicator works

### Projectiles
- [ ] Ballistic projectiles hit targets
- [ ] Homing missiles track targets
- [ ] Damage routed through DamageRouter
- [ ] Impact triggers full damage flow

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | Dec 2025 | Initial Phase 3 testing guide |

---

## Related Documentation

- `Documentation/Implementation/phase3-damage-system-guide.md` - Implementation guide
- `Documentation/hephaestus-gdd.md` - Game Design Document
- `CLAUDE.md` - Coding standards and implementation progress
