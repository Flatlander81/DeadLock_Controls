# DeadLock Controls - Project Status

## Project Overview
A turn-based tactical space combat game in Unity inspired by Battlestar Galactica: Deadlock.

## Current Implementation

### Movement System
- **Turn-based gameplay** with Command and Simulation phases managed by a TurnManager
  - Command Phase: Players plan their moves
  - Simulation Phase: All ships execute moves simultaneously
- **BSG Deadlock-style movement**: Ships plan moves during Command phase, then execute simultaneously during Simulation phase
- **Projection-based planning**: When a ship is selected, a semi-transparent green projection appears showing the planned end position
- **Bezier curve movement**: Ships follow smooth cubic Bezier arcs with a flat start tangent for realistic space combat feel
- **Movement constraints**:
  - Min distance: 5 units
  - Max distance: 20 units
  - Max turn angle: 45 degrees
- **Path visualization**: Cyan line renderer shows the planned movement arc in real-time
- **Collision indication**: Projection turns red when collision is detected (basic implementation exists)
- **Smooth movement execution**: Ships interpolate along Bezier curves during Simulation phase
- **Automatic rotation**: Ships face the direction of movement at the end of their arc

### Input Controls

#### Ship Selection & Movement
- **Left Click** on ship to select (auto-focuses camera on ship)
- **Drag projection** to plan movement destination
- **E** - Switch to Elevation adjustment mode
- **R** - Switch to Rotation adjustment mode (when ship selected)
- **Mouse Scroll** - Adjust elevation in adjustment mode
- **Arrow Keys** - Adjust rotation in rotation mode
- **Enter/Space** - Confirm planned movement
- **Esc** - Cancel current adjustment
- **Semicolon (;)** - Direct entry to elevation mode

#### Phase Management
- **End Turn Button** (GUI) - Transitions from Command to Simulation phase

### Camera System (Homeworld-style)
Full 3D orbit camera with keyboard and mouse controls:

#### Mouse Controls
- **Shift + Left Mouse Drag** - Orbit around focused ship/point
- **Ctrl + Left Mouse Drag** - Pan camera (moves focus point)
- **Mouse Wheel** - Zoom in/out

#### Keyboard Controls
- **Q/E** - Orbit left/right around focus point
- **W/A/S/D** - Pan camera (forward/left/back/right relative to camera)
- **R/F** - Zoom in/out (R = zoom in, F = zoom out)

#### Camera Features
- Auto-focuses on newly selected ships with smooth transitions
- Maintains focus on moving ships during Simulation phase
- Vertical angle constraints (-80° to +80°) to prevent camera flipping
- Smooth damping for fluid camera movement
- Configurable zoom range (5-50 units)

## File Structure
```
DeadLock_Controls/
├── Assets/
│   ├── Scripts/
│   │   └── Movement/
│   │       ├── Ship.cs - Individual ship movement, planning, and execution
│   │       ├── MovementController.cs - Player input handling and ship selection
│   │       ├── TurnManager.cs - Phase management (Command/Simulation)
│   │       └── OrbitCamera.cs - Homeworld-style camera control system
│   ├── Scenes/
│   │   └── TestScene.unity - Main test scene with ships and camera setup
│   ├── Materials/ - Ship materials and projection materials
│   ├── Meshes/ - Ship meshes and projection prefabs
│   └── Textures/ - Ship textures
├── ProjectSettings/
└── PROJECT_STATUS.md - This file
```

## Key Classes & Responsibilities

### Ship.cs
- Manages individual ship state and movement
- Handles projection creation and updates
- Executes Bezier curve movement during Simulation phase
- Calculates arc control points for realistic movement paths
- Provides selection/deselection visual feedback
- Properties: PlannedPosition, PlannedRotation, HasPlannedMove

### MovementController.cs
- Processes all player input during Command phase
- Manages ship selection state
- Handles projection dragging with mouse raycasting
- Provides elevation and rotation adjustment modes
- Integrates with OrbitCamera for focus management
- Displays on-screen GUI for controls and phase info

### TurnManager.cs
- Singleton pattern for global phase management
- Tracks current phase (Command or Simulation)
- Triggers ExecuteMove() on all ships when Simulation starts
- Automatically resets to Command phase after moves complete
- Manages game timing (3-second simulation duration)

### OrbitCamera.cs
- Implements spherical coordinate camera positioning
- Handles all camera input (mouse + keyboard)
- Provides smooth focus transitions between targets
- Supports both locked focus (following ships) and free-roam
- Configurable speeds for orbit, pan, and zoom

## Technical Details

### Movement Mathematics
- **Cubic Bezier Curves**: P(t) = (1-t)³P₀ + 3(1-t)²tP₁ + 3(1-t)t²P₂ + t³P₃
- **Control Points**:
  - P₀: Ship start position
  - P₁: Ahead of ship in forward direction (flat start)
  - P₂: Offset perpendicular to movement for arc
  - P₃: Planned end position
- **Tangent-based Rotation**: Ship rotation calculated from curve tangent at t=0.99

### Performance Optimizations
- Line renderer points reduced to 20 for performance
- Reuses LineRenderer instead of recreating each frame
- Bezier calculations cached during planning
- Material instances managed to prevent leaks

### Known Limitations
- Collision detection is basic (visual only, doesn't prevent moves)
- No multi-ship selection
- No undo functionality for planned moves
- Camera zoom conflicts with Rotation mode (both use R key)
- No save/load system
- Single scene only

## Current State

### What Works Well
✅ Smooth Bezier curve movement with realistic arcs
✅ Intuitive projection-based planning interface
✅ Flexible camera system with multiple control schemes
✅ Turn-based phase system
✅ Visual feedback (projections, paths, collision indicators)
✅ Movement constraints properly enforced

### What Needs Work
⚠️ Combat system (not implemented)
⚠️ AI for enemy ships (not implemented)
⚠️ Collision resolution (detection only, no prevention)
⚠️ UI/UX polish (minimal GUI currently)
⚠️ Ship variety (only single ship type tested)
⚠️ Game loop/objectives (no win/loss conditions)
⚠️ Performance testing with many ships

## Questions for Future Development

### 1. Combat System
- What kind of combat mechanics would work well with this turn-based movement system?
- Should weapons also be planned during Command phase?
- Simultaneous fire resolution or initiative-based?
- Weapon types: direct fire, missiles, point defense?
- Damage model: HP-based, component damage, or both?

### 2. AI System
- What level of AI complexity should I implement for enemy ships?
- Should AI use the same movement planning system or simplified version?
- How should AI evaluate tactical positions and threats?
- Difficulty levels or adaptive AI?

### 3. UI/UX Improvements
- What additional UI elements would help players plan their moves better?
  - Range indicators?
  - Firing arcs?
  - Predicted enemy positions?
  - Movement history?
  - Threat assessment visualization?
- Should there be a tactical overview/map view?
- How to display ship stats and status effects?

### 4. Ship Variety
- What ship types/classes should I implement?
  - Fighters, Corvettes, Frigates, Cruisers, Battleships?
- What should differentiate ship classes?
  - Speed/turn rate?
  - Weapon loadouts?
  - Armor/shields?
  - Special abilities?
- Should ships be customizable or fixed loadouts?

### 5. Game Loop & Structure
- What should the overall game structure be?
  - Campaign with story missions?
  - Skirmish/quick battle mode?
  - Procedurally generated scenarios?
- What are the win/loss conditions?
  - Destroy all enemies?
  - Objective-based (defend/escort/capture)?
  - Time limits?
- Progression system?
  - Unlock new ships?
  - Persistent fleet?
  - Experience/upgrades?

### 6. Technical Improvements
- Are there any obvious technical issues to address?
- Code architecture improvements needed?
- Performance optimizations for large battles?
- Network multiplayer considerations?
- Testing framework/automated tests?

### 7. Polish & Feel
- Audio system (ship sounds, weapons, ambience)?
- Visual effects (engine trails, weapon fire, explosions)?
- Screen shake/camera effects for combat?
- Particle systems for feedback?
- Post-processing effects?

## Next Steps - Prioritization Needed

Please help prioritize which features to implement next to create a compelling gameplay loop. Consider:
- What's the minimum viable product (MVP)?
- Which features provide the most gameplay value?
- What dependencies exist between features?
- What's the recommended development order?

## Development Environment
- **Unity Version**: 6000.2.10f1
- **Platform**: Windows (PC)
- **Language**: C#
- **Version Control**: Git

## Notes
- Project focuses on tactical gameplay over action
- Inspiration from BSG Deadlock and Homeworld series
- Emphasis on planning and positioning over reflexes
- 3D space combat but currently limited to horizontal plane
