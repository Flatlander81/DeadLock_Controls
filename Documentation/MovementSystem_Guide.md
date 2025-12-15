# BSG Deadlock-Style Movement System

A turn-based tactical movement system inspired by Battlestar Galactica Deadlock, featuring projection-based planning and WEGO (We Go) turn execution.

## Overview

This system allows players to plan ship movements using semi-transparent "shadow" projections, which are then executed simultaneously in a turn-based structure. Ships have movement constraints including minimum/maximum distances, turn angles, and inertial stress mechanics.

## Components

### 1. Ship.cs
Manages individual ship units with the following features:
- **Movement constraints**: Min/max distance, turn angle limits
- **Inertial stress system**: Sharp turns accumulate stress that reduces future turning ability
- **Visual projection**: Semi-transparent representation of planned position (appears half max distance forward when selected)
- **Collision detection**: Projects change color (green/red) based on collision state

### 2. TurnManager.cs
Controls the game's phase structure:
- **Command Phase**: Players plan all movements
- **Simulation Phase**: Planned movements execute simultaneously
- **Collision checking**: Detects potential collisions between planned positions
- **Auto-advance**: Automatically returns to Command phase after simulation

### 3. MovementController.cs
Handles player input and interaction:
- **Ship selection**: Click to select ships (projection appears in front automatically)
- **Projection dragging**: Drag projection to desired position
- **Elevation adjustment**: Use scroll wheel to adjust height
- **Rotation adjustment**: Use arrow keys to adjust facing
- **UI controls**: On-screen help and phase information

## Unity Setup Instructions

### Step 1: Create Layers

1. Open Edit > Project Settings > Tags and Layers
2. Add a new layer called "Ship"
3. The "Ignore Raycast" layer should already exist

### Step 2: Create Ship GameObject

1. Create a new Cube (GameObject > 3D Object > Cube)
2. Rename it to "Ship_01"
3. Scale it to (2, 1, 4) for a capital ship appearance
4. Add a Rigidbody component:
   - Set "Is Kinematic" to true
5. Change the layer to "Ship"
6. Add the `Ship` component
7. Configure Ship component values:
   - Min Move Distance: 5
   - Max Move Distance: 20
   - Max Turn Angle: 45
   - Inertial Stress Threshold: 30

### Step 3: Create Projection Prefab (Optional)

If you want a custom projection appearance:

1. Duplicate the Ship GameObject
2. Rename to "ShipProjection"
3. Scale down to 0.9 of original size
4. Create a new Material:
   - Shader: Standard
   - Rendering Mode: Transparent
   - Color: Green (0, 255, 0) with Alpha ~128
5. Apply material to projection
6. Remove any Rigidbody or Ship components
7. Drag to Project window to create prefab
8. Delete from scene
9. Assign prefab to Ship component's "Projection Prefab" field

**Note**: If you don't create a custom prefab, the Ship component will automatically generate a projection at runtime.

### Step 4: Create Manager Objects

1. Create an empty GameObject, name it "TurnManager"
2. Add the `TurnManager` component
3. Configure settings:
   - Simulation Duration: 3 seconds
   - Collision Threshold: 2 units

4. Create another empty GameObject, name it "MovementController"
5. Add the `MovementController` component
6. Configure settings:
   - Main Camera: Drag your Main Camera here (or leave empty for auto-detect)
   - Ship Layer: Set to "Ship" layer
   - Alternate Select Key: Semicolon (or choose your preference)
   - Elevation Sensitivity: 1
   - Rotation Sensitivity: 90

### Step 5: Create Ground Plane

1. Create a Plane (GameObject > 3D Object > Plane)
2. Scale to (10, 1, 10)
3. Position at (0, 0, 0)

### Step 6: Position Camera

1. Select Main Camera
2. Position at approximately (0, 20, -20)
3. Rotate to look down at the scene (around 45-60 degrees on X axis)

### Step 7: Create Multiple Ships

1. Duplicate Ship_01 twice
2. Rename to Ship_02 and Ship_03
3. Position them apart from each other (e.g., (-10, 0, 0), (0, 0, 0), (10, 0, 0))
4. Give them different colors by creating and assigning different materials

## Testing Checklist

### Basic Functionality
- [ ] Ships can be selected by clicking
- [ ] Projections appear green and positioned in front when selected
- [ ] Projections are positioned half max distance forward from ship
- [ ] Projections can be dragged across ground plane
- [ ] Movement distance is clamped between min/max range

### Adjustment Modes
- [ ] Semicolon key starts elevation adjustment directly
- [ ] Mouse wheel adjusts elevation in elevation mode
- [ ] E key switches to elevation mode
- [ ] R key switches to rotation mode
- [ ] Arrow keys (or A/D) rotate ship during rotation mode
- [ ] Enter/Space confirms movement
- [ ] Escape cancels adjustment mode

### Movement Constraints
- [ ] Rotation is clamped to maxTurnAngle
- [ ] Sharp turns show inertial stress in console
- [ ] Inertial stress reduces subsequent turn radius
- [ ] Gizmos show movement paths in Scene view (yellow lines)
- [ ] Gizmos show min/max range spheres (green/red)

### Turn System
- [ ] Multiple ships can have planned moves simultaneously
- [ ] Ships with overlapping projections show red (collision warning)
- [ ] "End Turn" button executes all planned moves
- [ ] Ships move simultaneously during simulation phase
- [ ] System automatically returns to Command phase after simulation
- [ ] Ships reset for new turn after simulation ends

## Controls Reference

### Mouse Controls
- **Left Click on Ship**: Select ship (projection appears in front)
- **Left Click + Drag on Projection**: Move ship's planned position
- **Scroll Wheel**: Adjust elevation (in Elevation mode)

### Keyboard Controls
- **E**: Enter Elevation adjustment mode
- **R**: Enter Rotation adjustment mode
- **Semicolon (;)**: Quick-enter Elevation mode
- **Arrow Keys / A-D**: Rotate ship (in Rotation mode)
- **Enter / Space**: Confirm movement and exit adjustment mode
- **Escape**: Cancel adjustment mode

### UI Controls
- **End Turn Button**: Execute planned movements (only in Command phase)

## Advanced Features

### Inertial Stress System

Ships accumulate stress when turning sharply (beyond the `inertialStressThreshold`):
- Stress accumulates when turn angle exceeds threshold
- Accumulated stress reduces maximum turn angle for future turns
- Stress is reduced by 5 units each turn
- Formula: `effectiveMaxTurn = maxTurnAngle - (currentInertialStress * 0.5)`

### Collision Detection

The system uses sphere-sphere collision checks:
- Checks all pairs of ships with planned moves
- Compares distance between PlannedPositions
- If distance < collisionThreshold, both ships marked as colliding
- Collision only shown visually (red projections) - movements still execute

## Debug Features

### Console Logging
- Ship selection/deselection events
- Movement planning details (distance, angle, effective turn)
- Inertial stress accumulation
- Collision detection results
- Phase transitions

### Scene View Gizmos
- Yellow lines from current to planned position
- Green wire sphere showing minimum movement range
- Red wire sphere showing maximum movement range
- Only visible when ship has a planned move

## Troubleshooting

### Projections not appearing
- Check that Ship component is attached to ship GameObject
- Verify projection materials are set correctly
- Check that ship is on "Ship" layer

### Can't select ships
- Verify MovementController has correct Ship Layer set
- Check that ships have colliders
- Ensure camera reference is set in MovementController

### Projection not dragging
- Make sure you're clicking on the projection itself
- Try using the semicolon key to enter adjustment mode directly
- Check console for any error messages

### Ships not moving during simulation
- Verify all ships have planned moves (projections visible)
- Check that TurnManager exists in scene
- Ensure you clicked "End Turn" button
- Check console for phase transition messages

## Performance Considerations

### For Large Fleets (>20 ships)
- Consider implementing spatial partitioning for collision checks
- Use object pooling for projections
- Optimize by disabling projection rendering when off-screen

### Current Implementation
- O(n²) collision checking suitable for small fleets (<20 ships)
- All component references cached in Start()
- LayerMask used efficiently for raycasting

## Future Extensions

Potential additions to the system:
- Formation movement (multi-select)
- DRADIS range visualization
- Weapon firing arcs display
- Squadron/fighter unit support
- Electronic warfare effects
- Subsystem damage visualization
- Replay system for simulation phase
- Network synchronization for multiplayer
- AI for Cylon fleet movements

## Code Architecture

### Design Patterns Used
- **Singleton**: TurnManager for global access
- **Component-based**: Unity's component system for modular design
- **State Machine**: Phase enum for turn management
- **Observer**: Ships update projections based on planning state

### Key Design Decisions
- Projection as separate GameObject for easy visualization
- Temporary collider approach for projection raycasting
- Gizmo visualization for debug information
- On-screen GUI for accessibility
- Separation of concerns: Ship (data), TurnManager (flow), MovementController (input)

## License

This implementation is provided as educational material for game development.

## Credits

Inspired by Battlestar Galactica Deadlock's tactical movement system.
