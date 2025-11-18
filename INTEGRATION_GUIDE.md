# BSG Deadlock-Style Movement System - Integration Guide

## Overview

This is a turn-based tactical movement system for Unity that implements smooth, curved ship movement using cubic Bezier arcs. Ships plan their moves during a Command phase, then execute them simultaneously during a Simulation phase (WEGO - "We Go" system).

### Key Characteristics
- **Capital ship feel**: Ships cannot turn instantly; they follow smooth banking turn curves
- **Projection-based planning**: Semi-transparent projections show where ships will move
- **Turn-based execution**: All planned moves execute simultaneously
- **Realistic constraints**: Minimum/maximum movement distances, turn angle limits

## System Architecture

### Core Components

#### 1. **Ship.cs** - Individual Unit Controller
**Responsibility**: Manages a single ship's movement planning and execution.

**Key Features**:
- Movement constraints (min/max distance, turn angle)
- Cubic Bezier path calculation
- Semi-transparent projection visualization
- Smooth movement execution following curve tangent

**Key Methods**:
```csharp
public void PlanMove(Vector3 targetPos, Quaternion targetRot)
// Plans a move with distance/rotation constraints
// Calculates cubic Bezier control points
// Updates projection visualization

public void ExecuteMove()
// Starts smooth animation along the planned Bezier arc
// Called by TurnManager during Simulation phase

public void ResetPlannedMove()
// Clears planned move for new turn
// Called by TurnManager when returning to Command phase
```

**Public Properties**:
```csharp
Vector3 PlannedPosition { get; }      // Where ship will move to
Quaternion PlannedRotation { get; }   // Final facing direction
bool HasPlannedMove { get; }          // Whether move is planned
float MaxTurnAngle { get; }           // Turn constraint
```

#### 2. **TurnManager.cs** - Game Flow Controller
**Responsibility**: Manages phase transitions and coordinates all ships.

**Phases**:
- **Command**: Players plan movements (input enabled)
- **Simulation**: Movements execute simultaneously (input disabled)

**Key Methods**:
```csharp
public void EndCommandPhase()
// Switches to Simulation phase
// Checks for collisions
// Calls ExecuteMove() on all ships

private void EndSimulation()
// Automatically called after simulationDuration
// Returns to Command phase
// Calls ResetPlannedMove() on all ships
```

**Singleton Access**:
```csharp
TurnManager.Instance.CurrentPhase  // Command or Simulation
TurnManager.Instance.GetAllShips() // Array of all ships
```

#### 3. **MovementController.cs** - Player Input Handler
**Responsibility**: Translates mouse/keyboard input into movement plans.

**Key Features**:
- Ship selection via clicking
- Projection dragging for position planning
- Elevation/rotation adjustment modes
- UI controls and phase information

**Input Handling**:
- Left click: Select ship or drag projection
- E key: Elevation adjustment mode
- R key: Rotation adjustment mode
- Scroll wheel: Adjust elevation
- Arrow keys: Adjust rotation
- Enter/Space: Confirm movement
- Escape: Cancel adjustment

## How Movement Works

### The Cubic Bezier Arc System

Ships follow a **cubic Bezier curve** with 4 control points:

```
P0: Ship's current position (start)
P1: startControlPoint - ahead of ship in forward direction (creates flat start)
P2: arcControlPoint - offset perpendicular to movement (creates banking turn curve)
P3: PlannedPosition (end)
```

**Why Cubic Bezier?**
1. **Flat start**: P1 positioned ahead ensures ship starts moving in current direction
2. **Smooth curve**: P2 offset creates natural banking turn
3. **Simple math**: Efficient calculation, easy to visualize
4. **Predictable**: Same input always produces same curve

### Movement Execution Flow

1. **Planning Phase** (Command):
   ```
   User drags projection → MovementController.HandleProjectionDrag()
   → Ship.PlanMove(targetPos, targetRot)
   → Calculate Bezier control points
   → Update projection position/rotation
   → Draw cyan path line
   ```

2. **Execution Phase** (Simulation):
   ```
   TurnManager.EndCommandPhase()
   → Ship.ExecuteMove() for all ships
   → Ship.Update() interpolates along Bezier curve (t: 0→1 over 3 seconds)
   → Ship rotation follows curve tangent
   → Movement completes, returns to Command phase
   ```

### Key Algorithms

#### Cubic Bezier Interpolation
```csharp
Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
{
    float u = 1 - t;
    float uu = u * u;
    float uuu = uu * u;
    float tt = t * t;
    float ttt = tt * t;

    return (uuu * p0) + (3 * uu * t * p1) + (3 * u * tt * p2) + (ttt * p3);
}
```

#### Control Point Calculation
```csharp
// First control point: ahead of ship for flat start
startControlPoint = shipPosition + shipForward * straightStartLength;

// Second control point: offset perpendicular for curve
Vector3 midpoint = (shipPosition + targetPosition) * 0.5f;
float turnAngle = Vector3.SignedAngle(shipForward, movementDirection, Vector3.up);
Vector3 perpendicular = Vector3.Cross(movementDirection, Vector3.up);
float offset = distance * arcHeight * (abs(turnAngle) / 90f);
arcControlPoint = midpoint + perpendicular * sign(turnAngle) * offset;
```

#### Rotation Following Tangent
```csharp
// Sample curve slightly ahead to get tangent direction
Vector3 currentPos = CubicBezier(p0, p1, p2, p3, t);
Vector3 futurePos = CubicBezier(p0, p1, p2, p3, t + 0.01f);
Vector3 tangent = (futurePos - currentPos).normalized;
shipRotation = Quaternion.LookRotation(tangent);
```

## Integration Guide

### Scenario 1: Adding to Existing Turn-Based Game

**Requirements**:
- Unity project with existing game objects
- Turn-based game loop

**Steps**:

1. **Add Scripts**:
   ```
   Copy Ship.cs, TurnManager.cs, MovementController.cs to your project
   ```

2. **Configure Layers**:
   ```
   Edit > Project Settings > Tags and Layers
   Add layer: "Ship"
   ```

3. **Setup TurnManager**:
   ```csharp
   GameObject manager = new GameObject("TurnManager");
   manager.AddComponent<TurnManager>();
   // Configure: simulationDuration, collisionThreshold
   ```

4. **Setup MovementController**:
   ```csharp
   GameObject controller = new GameObject("MovementController");
   controller.AddComponent<MovementController>();
   // Assign: mainCamera, shipLayer, sensitivities
   ```

5. **Add to Your Ships**:
   ```csharp
   // On each ship GameObject:
   ship.layer = LayerMask.NameToLayer("Ship");
   ship.AddComponent<Ship>();
   ship.AddComponent<Rigidbody>().isKinematic = true;
   // Configure: minMoveDistance, maxMoveDistance, maxTurnAngle, arcHeight
   ```

6. **Integrate with Your Turn System**:
   ```csharp
   // In your existing turn system:
   if (playerClicksEndTurn)
   {
       TurnManager.Instance.EndCommandPhase();
   }

   // TurnManager automatically returns to Command phase after simulation
   ```

### Scenario 2: Using Only the Movement Algorithm

If you only want the Bezier arc movement without the full turn system:

**Extract the Core Algorithm**:

```csharp
public class BezierMovement
{
    // Configuration
    public float arcHeight = 0.3f;
    public float straightStartLength = 3f;

    // Calculate path from start to end
    public Vector3[] CalculatePath(Vector3 startPos, Vector3 startForward,
                                   Vector3 endPos, int resolution = 50)
    {
        // First control point: ahead for flat start
        Vector3 p1 = startPos + startForward * straightStartLength;

        // Second control point: offset for curve
        Vector3 midpoint = (startPos + endPos) * 0.5f;
        Vector3 direction = (endPos - startPos).normalized;
        float angle = Vector3.SignedAngle(startForward, direction, Vector3.up);
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        float distance = Vector3.Distance(startPos, endPos);
        float offset = distance * arcHeight * (Mathf.Abs(angle) / 90f);
        Vector3 p2 = midpoint + perpendicular * Mathf.Sign(angle) * offset;

        // Sample curve
        Vector3[] path = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            path[i] = CubicBezier(startPos, p1, p2, endPos, t);
        }
        return path;
    }

    private Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        return (u*u*u * p0) + (3*u*u*t * p1) + (3*u*t*t * p2) + (t*t*t * p3);
    }
}
```

### Scenario 3: Adapting for Real-Time (Non-Turn-Based)

**Changes Needed**:

1. **Remove Phase Restrictions**:
   ```csharp
   // In MovementController.Update(), remove:
   if (TurnManager.Instance.CurrentPhase != TurnManager.Phase.Command)
       return;
   ```

2. **Immediate Execution**:
   ```csharp
   // In Ship.PlanMove(), add:
   ExecuteMove(); // Start moving immediately
   ```

3. **Optional: Cancel Mid-Movement**:
   ```csharp
   public void CancelMove()
   {
       isExecutingMove = false;
       // Optionally lerp to a stop
   }
   ```

### Scenario 4: Network Multiplayer

**Synchronization Points**:

1. **Planning Phase** (Client-side):
   - Allow local player to plan moves
   - Show other players' projections from network data

2. **Execution** (Synchronized):
   ```csharp
   // Host/Server:
   [Command]
   void CmdSubmitMove(Vector3 targetPos, Quaternion targetRot)
   {
       // Validate move
       // Broadcast to all clients
       RpcExecuteMove(targetPos, targetRot);
   }

   [ClientRpc]
   void RpcExecuteMove(Vector3 targetPos, Quaternion targetRot)
   {
       ship.PlanMove(targetPos, targetRot);
       ship.ExecuteMove();
   }
   ```

3. **Deterministic Movement**:
   - Bezier curves are deterministic (same inputs = same output)
   - Only need to sync: startPos, startRot, endPos, endRot, timestamp
   - All clients calculate same curve locally

## API Reference

### Ship Component

**Serialized Fields** (Configure in Inspector):
```csharp
[Header("Movement Statistics")]
float minMoveDistance = 5f;        // Minimum movement distance
float maxMoveDistance = 20f;       // Maximum movement distance
float maxTurnAngle = 45f;          // Maximum turn per move (degrees)

[Header("Path Settings")]
float arcHeight = 0.3f;            // Arc curve amount (0.3 = 30% of distance)
float straightStartLength = 3f;    // Distance ahead for flat start control point
Color pathColor = Color.cyan;      // Path visualization color

[Header("Visual Components")]
GameObject projectionPrefab;               // Optional custom projection
Material normalProjectionMaterial;         // Green projection material
Material collisionProjectionMaterial;      // Red projection material (collisions)
```

**Public Methods**:
```csharp
void PlanMove(Vector3 targetPos, Quaternion targetRot)
// Plan movement to target with rotation
// Applies constraints (distance, turn angle)
// Updates projection and path visualization

void ExecuteMove()
// Begin smooth movement execution
// Should be called by TurnManager during Simulation phase

void ResetPlannedMove()
// Clear planned move
// Called when returning to Command phase

void Select()
// Mark ship as selected, show projection
// Called by MovementController on click

void Deselect()
// Deselect ship, hide projection if no move planned

void MarkCollision(bool willCollide)
// Set projection color (green/red) based on collision detection

GameObject GetProjection()
// Get projection GameObject for raycasting
```

**Public Properties**:
```csharp
Vector3 PlannedPosition { get; }      // Target position
Quaternion PlannedRotation { get; }   // Target rotation
bool HasPlannedMove { get; }          // Whether move is planned
float MaxTurnAngle { get; }           // Turn angle constraint
```

### TurnManager Component

**Serialized Fields**:
```csharp
float simulationDuration = 3f;     // How long Simulation phase lasts
float collisionThreshold = 2f;     // Distance for collision detection
```

**Public API**:
```csharp
static TurnManager Instance         // Singleton access
Phase CurrentPhase                  // Current game phase (Command/Simulation)

void EndCommandPhase()              // Switch to Simulation, execute moves
Ship[] GetAllShips()                // Get all ships in scene
void RefreshShipList()              // Refresh cached ship array
```

**Phases**:
```csharp
enum Phase
{
    Command,    // Planning phase - input enabled
    Simulation  // Execution phase - input disabled
}
```

### MovementController Component

**Serialized Fields**:
```csharp
Camera mainCamera;                      // Camera for raycasting
LayerMask shipLayer;                    // Layer for ship selection
KeyCode alternateSelectKey = Semicolon; // Quick elevation mode key
float elevationSensitivity = 1f;        // Scroll wheel sensitivity
float rotationSensitivity = 90f;        // Rotation adjustment speed
float groundPlaneY = 0f;                // Y position for ground plane
```

## Best Practices

### Performance

1. **Ship Count**: Current implementation handles <20 ships efficiently
   - Collision checking is O(n²)
   - For larger fleets, implement spatial partitioning

2. **Path Resolution**: Default 50 points per curve is good balance
   - Increase for smoother visualization on large monitors
   - Decrease for mobile or low-end hardware

3. **Update Optimization**: Movement only runs during Simulation phase
   - Most Update() calls early-exit when not executing

### Customization

**Adjust Movement Feel**:
```csharp
// Tighter turns (more agile ships):
arcHeight = 0.2f;
maxTurnAngle = 60f;

// Wider turns (heavier capital ships):
arcHeight = 0.4f;
maxTurnAngle = 30f;

// Longer straight start (more momentum):
straightStartLength = 5f;
```

**Custom Projection Appearance**:
```csharp
// Create prefab with custom mesh/materials
// Assign to Ship.projectionPrefab in Inspector
// Must have Renderer component for collision color changes
```

### Debugging

**Gizmos** (Scene view only):
- Cyan path: The Bezier curve
- Small cyan sphere: Start control point
- Yellow sphere: Arc control point
- Green sphere: Destination
- Red/green wire spheres: Min/max movement range

**Console Logs**:
- Ship selection/deselection
- Movement planning details
- Phase transitions
- Collision detections

**Enable Additional Logging**:
```csharp
// In Ship.CalculateArcControlPoint():
Debug.Log($"Cubic Bezier: straightStart={straightStartLength}, arc={offsetAmount}, turn={angle}");

// In Ship.Update():
Debug.Log($"Moving: t={t:F2}, pos={transform.position}, rot={transform.rotation.eulerAngles.y}");
```

## Common Integration Issues

### Issue 1: Projections Not Selectable
**Symptom**: Can't click on projections to drag them
**Cause**: Projection on wrong layer or no collider
**Fix**:
```csharp
// Ship creates projection with temporary collider
// Don't manually set projection to "Ignore Raycast" layer
```

### Issue 2: Ships Not Moving During Simulation
**Symptom**: Ships teleport or don't move
**Cause**: Update() not running or waypoints null
**Fix**:
```csharp
// Ensure Ship.ExecuteMove() is called
// Check isExecutingMove flag is true
// Verify moveStartPosition is set correctly
```

### Issue 3: Rotation Snapping
**Symptom**: Ship instantly rotates at movement start
**Cause**: Control points not creating flat start
**Fix**:
```csharp
// Increase straightStartLength
straightStartLength = 5f; // Default is 3f

// Or check startControlPoint calculation
// Should be: shipPos + shipForward * straightStartLength
```

### Issue 4: Path Wobbling
**Symptom**: Curved path has unexpected wobbles
**Cause**: Control point calculation issues
**Fix**:
```csharp
// Check perpendicular calculation
Vector3 perpendicular = Vector3.Cross(movementDirection, Vector3.up).normalized;

// Ensure movement direction is normalized
Vector3 direction = (endPos - startPos).normalized;
```

### Issue 5: Turn System Conflicts
**Symptom**: Multiple turn systems interfering
**Cause**: Existing turn manager conflicting with TurnManager
**Fix**:
```csharp
// Option 1: Integrate with existing system
if (yourTurnManager.IsCommandPhase())
{
    // Enable MovementController input
}

// Option 2: Use TurnManager.CurrentPhase as source of truth
if (TurnManager.Instance.CurrentPhase == TurnManager.Phase.Command)
{
    yourTurnManager.EnableCommands();
}
```

## Example Integration Scenarios

### Example 1: Adding to Existing RTS

```csharp
public class RTSGameManager : MonoBehaviour
{
    void Start()
    {
        // Let TurnManager handle phases
        TurnManager.Instance.OnPhaseChanged += OnPhaseChanged;
    }

    void OnPhaseChanged(TurnManager.Phase newPhase)
    {
        if (newPhase == TurnManager.Phase.Command)
        {
            // Enable UI, show "Plan Your Moves"
            commandUI.SetActive(true);
        }
        else
        {
            // Disable UI during execution
            commandUI.SetActive(false);
        }
    }

    public void OnPlayerClicksEndTurn()
    {
        // Trigger simulation
        TurnManager.Instance.EndCommandPhase();

        // Your custom logic
        RecordTurnForReplay();
        UpdatePlayerResources();
    }
}
```

### Example 2: AI Ship Control

```csharp
public class AIShipController : MonoBehaviour
{
    private Ship ship;

    void Start()
    {
        ship = GetComponent<Ship>();
    }

    void Update()
    {
        // Only plan during Command phase
        if (TurnManager.Instance.CurrentPhase != TurnManager.Phase.Command)
            return;

        // Don't re-plan if already planned
        if (ship.HasPlannedMove)
            return;

        // Simple AI: Move toward target
        Vector3 targetPos = CalculateTargetPosition();
        Quaternion targetRot = Quaternion.LookRotation(targetPos - transform.position);

        ship.PlanMove(targetPos, targetRot);
    }

    Vector3 CalculateTargetPosition()
    {
        // Your AI logic here
        GameObject enemy = FindNearestEnemy();
        return enemy.transform.position;
    }
}
```

### Example 3: Custom Path Visualization

```csharp
public class CustomPathRenderer : MonoBehaviour
{
    private Ship ship;
    private LineRenderer customLine;

    void Start()
    {
        ship = GetComponent<Ship>();

        // Create custom line renderer
        GameObject lineObj = new GameObject("CustomPath");
        customLine = lineObj.AddComponent<LineRenderer>();

        // Custom appearance
        customLine.material = yourMaterial;
        customLine.startWidth = 0.5f;
        customLine.endWidth = 0.5f;
        customLine.startColor = Color.blue;
        customLine.endColor = Color.red; // Gradient
    }

    void Update()
    {
        if (ship.HasPlannedMove)
        {
            // Draw custom visualization
            DrawCustomPath();
        }
    }

    void DrawCustomPath()
    {
        // Sample the Bezier curve
        int resolution = 100;
        customLine.positionCount = resolution;

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)(resolution - 1);
            // Access ship's internal path calculation
            // (You'd need to expose this or calculate separately)
            Vector3 point = CalculatePathPoint(t);
            customLine.SetPosition(i, point);
        }
    }
}
```

## Summary

This movement system provides:
- ✅ Smooth, curved capital ship movement
- ✅ Turn-based WEGO execution
- ✅ Visual planning with projections
- ✅ Realistic movement constraints
- ✅ Easy integration with existing systems
- ✅ Deterministic behavior for networking
- ✅ Efficient performance for small-medium fleets

**Integration Complexity**: Low to Medium
- Basic integration: ~30 minutes
- Full customization: 2-4 hours
- Network synchronization: 4-8 hours

**Dependencies**: Unity 2020.3+ (any render pipeline)

**License**: Provided as educational material

---

*Generated for Claude Code integration. Last updated: 2025-11-18*
