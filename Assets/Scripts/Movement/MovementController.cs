using UnityEngine;

/// <summary>
/// Handles player input for ship selection and movement planning.
/// Manages the projection dragging, elevation, and rotation adjustment modes.
/// </summary>
public class MovementController : MonoBehaviour
{
    /// <summary>
    /// Adjustment mode enumeration for movement fine-tuning.
    /// </summary>
    private enum AdjustmentMode
    {
        None,
        Elevation,
        Rotation
    }

    [Header("Debug Settings")]
    [SerializeField] private bool verboseLogging = false;

    [Header("Configuration")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private OrbitCamera orbitCamera;
    [SerializeField] private LayerMask shipLayer;
    [SerializeField] private KeyCode alternateSelectKey = KeyCode.Semicolon;
    [SerializeField] private KeyCode focusCameraKey = KeyCode.F;
    [SerializeField] private float elevationSensitivity = 1f;
    [SerializeField] private float rotationSensitivity = 90f;

    [Header("Ground Plane")]
    [SerializeField] private float groundPlaneY = 0f;

    // State tracking
    private Ship selectedShip;
    private bool isDraggingProjection = false;
    private Vector3 dragStartMousePos;
    private Vector3 projectionDragOffset;
    private AdjustmentMode adjustmentMode = AdjustmentMode.None;

    // Temporary collider for projection raycasting
    private BoxCollider tempProjectionCollider;

    /// <summary>
    /// Initialize camera reference if not set.
    /// </summary>
    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            Debug.LogError("MovementController: No camera found! Please assign a camera.");
        }

        if (orbitCamera == null && mainCamera != null)
        {
            orbitCamera = mainCamera.GetComponent<OrbitCamera>();
        }
    }

    /// <summary>
    /// Main update loop handles all input.
    /// </summary>
    private void Update()
    {
        // Only process input during Command phase
        if (TurnManager.Instance == null || TurnManager.Instance.CurrentPhase != TurnManager.Phase.Command)
        {
            return;
        }

        // Handle different input contexts
        if (isDraggingProjection)
        {
            HandleProjectionDrag();
        }
        else if (adjustmentMode != AdjustmentMode.None)
        {
            HandleMovementAdjustments();
        }
        else
        {
            HandleSelection();
        }
    }

    /// <summary>
    /// Handles ship selection and projection dragging initiation.
    /// </summary>
    private void HandleSelection()
    {
        // Left click to select ship or start dragging projection
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // First, try to hit a ship
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, shipLayer))
            {
                Ship clickedShip = hit.collider.GetComponent<Ship>();
                if (clickedShip != null)
                {
                    SelectShip(clickedShip);
                    return;
                }
            }

            // If a ship is selected, check if we're clicking on its projection
            if (selectedShip != null)
            {
                if (TryStartProjectionDrag(ray))
                {
                    return;
                }
            }

            // If we clicked nothing, deselect
            if (selectedShip != null)
            {
                DeselectCurrentShip();
            }
        }

        // Semicolon key to enter elevation mode directly
        if (Input.GetKeyDown(alternateSelectKey) && selectedShip != null)
        {
            adjustmentMode = AdjustmentMode.Elevation;
            Debug.Log("Entered Elevation adjustment mode (alternate key)");
        }

        // F key to focus camera on selected ship
        if (Input.GetKeyDown(focusCameraKey) && selectedShip != null && orbitCamera != null)
        {
            orbitCamera.FocusOn(selectedShip.transform);
            Debug.Log($"Camera focused on {selectedShip.gameObject.name}");
        }
    }

    /// <summary>
    /// Attempts to start dragging the selected ship's projection.
    /// </summary>
    private bool TryStartProjectionDrag(Ray ray)
    {
        GameObject projection = selectedShip.GetProjection();
        if (projection == null) return false;

        // Temporarily add a BoxCollider to the projection for raycasting
        tempProjectionCollider = projection.AddComponent<BoxCollider>();

        RaycastHit hit;
        bool hitProjection = Physics.Raycast(ray, out hit, Mathf.Infinity);

        if (hitProjection && hit.collider == tempProjectionCollider)
        {
            // Calculate offset from projection center
            projectionDragOffset = hit.point - projection.transform.position;
            isDraggingProjection = true;
            dragStartMousePos = Input.mousePosition;

            if (verboseLogging)
            {
                Debug.Log($"Started dragging {selectedShip.gameObject.name} projection");
            }

            // Keep the collider for now, remove it after drag
            return true;
        }

        // Remove temporary collider if we didn't hit it
        if (tempProjectionCollider != null)
        {
            Destroy(tempProjectionCollider);
            tempProjectionCollider = null;
        }

        return false;
    }

    /// <summary>
    /// Handles dragging the projection across the ground plane.
    /// </summary>
    private void HandleProjectionDrag()
    {
        if (selectedShip == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Cast ray to ground plane at ship's Y position
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, groundPlaneY, 0));
        float enter;

        if (groundPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            // Calculate target position with offset
            Vector3 targetPosition = hitPoint - projectionDragOffset;

            // Calculate direction from ship to target
            Vector3 direction = targetPosition - selectedShip.transform.position;

            // Zero out Y component to keep ship level (no pitch)
            direction.y = 0;

            // Calculate target rotation to face the movement direction
            Quaternion targetRotation;
            if (direction.sqrMagnitude > 0.001f) // Check if there's meaningful movement
            {
                // Make ship face the direction of movement
                targetRotation = Quaternion.LookRotation(direction);
            }
            else
            {
                // If barely moving, keep current rotation
                targetRotation = selectedShip.transform.rotation;
            }

            // Plan the move (this will clamp rotation to max turn angle and update projection)
            selectedShip.PlanMove(targetPosition, targetRotation);
        }

        // Mouse release ends projection drag
        if (Input.GetMouseButtonUp(0))
        {
            // Clean up temporary collider
            if (tempProjectionCollider != null)
            {
                Destroy(tempProjectionCollider);
                tempProjectionCollider = null;
            }

            // End dragging and enter adjustment mode
            isDraggingProjection = false;
            adjustmentMode = AdjustmentMode.Elevation;
            Debug.Log($"Drag ended. Position: {selectedShip.PlannedPosition}, Rotation: {selectedShip.PlannedRotation.eulerAngles.y:F1}° | Press Enter=Confirm, E/R=Adjust, Esc=Cancel");
        }
    }

    /// <summary>
    /// Handles elevation and rotation adjustment inputs.
    /// </summary>
    private void HandleMovementAdjustments()
    {
        if (selectedShip == null)
        {
            adjustmentMode = AdjustmentMode.None;
            return;
        }

        // Mode switching
        if (Input.GetKeyDown(KeyCode.E))
        {
            adjustmentMode = AdjustmentMode.Elevation;
            Debug.Log("Switched to Elevation adjustment mode");
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            adjustmentMode = AdjustmentMode.Rotation;
            Debug.Log("Switched to Rotation adjustment mode");
        }

        // Exit adjustment mode
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            adjustmentMode = AdjustmentMode.None;
            Debug.Log("Exited adjustment mode (Escape pressed)");
            return;
        }

        // Confirm movement
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            adjustmentMode = AdjustmentMode.None;
            Debug.Log("Movement confirmed (Enter/Space pressed)");
            return;
        }

        // Execute specific adjustment
        switch (adjustmentMode)
        {
            case AdjustmentMode.Elevation:
                HandleElevationAdjustment();
                break;
            case AdjustmentMode.Rotation:
                HandleRotationAdjustment();
                break;
        }
    }

    /// <summary>
    /// Handles elevation adjustment using mouse scroll wheel.
    /// </summary>
    private void HandleElevationAdjustment()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 currentPlannedPos = selectedShip.HasPlannedMove
                ? selectedShip.PlannedPosition
                : selectedShip.transform.position;

            Quaternion currentPlannedRot = selectedShip.HasPlannedMove
                ? selectedShip.PlannedRotation
                : selectedShip.transform.rotation;

            // Modify Y position
            currentPlannedPos.y += scroll * elevationSensitivity;

            selectedShip.PlanMove(currentPlannedPos, currentPlannedRot);
        }
    }

    /// <summary>
    /// Handles rotation adjustment using horizontal input (arrow keys/A/D).
    /// </summary>
    private void HandleRotationAdjustment()
    {
        float horizontal = Input.GetAxis("Horizontal");

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            Vector3 currentPlannedPos = selectedShip.HasPlannedMove
                ? selectedShip.PlannedPosition
                : selectedShip.transform.position;

            Quaternion currentPlannedRot = selectedShip.HasPlannedMove
                ? selectedShip.PlannedRotation
                : selectedShip.transform.rotation;

            // Create delta rotation
            float rotationDelta = horizontal * rotationSensitivity * Time.deltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0, rotationDelta, 0);

            // Apply to current rotation
            Quaternion newRotation = currentPlannedRot * deltaRotation;

            selectedShip.PlanMove(currentPlannedPos, newRotation);
        }
    }

    /// <summary>
    /// Selects a ship and deselects any previously selected ship.
    /// </summary>
    private void SelectShip(Ship ship)
    {
        if (selectedShip != null)
        {
            selectedShip.Deselect();
        }

        selectedShip = ship;
        selectedShip.Select();
        adjustmentMode = AdjustmentMode.None;

        // Automatically focus camera on newly selected ship
        if (orbitCamera != null)
        {
            orbitCamera.FocusOn(ship.transform);
        }
    }

    /// <summary>
    /// Deselects the currently selected ship.
    /// </summary>
    private void DeselectCurrentShip()
    {
        if (selectedShip != null)
        {
            selectedShip.Deselect();
            selectedShip = null;
        }
        adjustmentMode = AdjustmentMode.None;
    }

    /// <summary>
    /// Draws UI elements for controls and phase information.
    /// </summary>
    private void OnGUI()
    {
        // Movement controls help text (top-left)
        GUILayout.BeginArea(new Rect(10, 10, 300, 240));
        GUILayout.Box("Movement Controls");
        GUILayout.Label("Click ship to select");
        GUILayout.Label("Drag projection to move");
        GUILayout.Label("E - Elevation mode");
        GUILayout.Label("R - Rotation mode");
        GUILayout.Label("Scroll - Adjust elevation");
        GUILayout.Label("Arrow Keys - Adjust rotation");
        GUILayout.Label("Enter/Space - Confirm");
        GUILayout.Label("Esc - Cancel adjustment");
        GUILayout.Label("");
        GUILayout.Label("Camera Controls:");
        GUILayout.Label("Shift+Drag - Orbit camera");
        GUILayout.Label("Ctrl+Drag - Pan camera");
        GUILayout.Label("Q/E - Orbit left/right");
        GUILayout.Label("WASD - Pan camera");
        GUILayout.Label("R/F - Zoom in/out");
        GUILayout.Label("Scroll - Zoom");
        GUILayout.EndArea();

        // Current adjustment mode indicator
        if (adjustmentMode != AdjustmentMode.None && selectedShip != null)
        {
            GUILayout.BeginArea(new Rect(10, 220, 300, 50));
            GUILayout.Box($"Mode: {adjustmentMode}");
            GUILayout.Label($"Ship: {selectedShip.gameObject.name}");
            GUILayout.EndArea();
        }

        // Phase indicator (top-right)
        if (TurnManager.Instance != null)
        {
            GUILayout.BeginArea(new Rect(Screen.width - 160, 10, 150, 60));
            GUILayout.Box($"Phase: {TurnManager.Instance.CurrentPhase}");
            GUILayout.EndArea();

            // End Turn button (only visible during Command phase)
            if (TurnManager.Instance.CurrentPhase == TurnManager.Phase.Command)
            {
                GUILayout.BeginArea(new Rect(Screen.width - 160, 80, 150, 50));
                if (GUILayout.Button("End Turn", GUILayout.Height(40)))
                {
                    TurnManager.Instance.EndCommandPhase();
                    DeselectCurrentShip();
                }
                GUILayout.EndArea();
            }
        }
    }
}
