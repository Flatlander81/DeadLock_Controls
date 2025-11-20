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
    public enum AdjustmentMode
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
    [SerializeField] private DebugUI debugUI;
    [SerializeField] private TargetingController targetingController;
    [SerializeField] private Ship playerShip; // Only this ship can be moved
    [SerializeField] private LayerMask shipLayer;
    [SerializeField] private KeyCode movementModeKey = KeyCode.M;
    [SerializeField] private KeyCode focusCameraKey = KeyCode.F;
    [SerializeField] private float elevationSensitivity = 1f;
    [SerializeField] private float rotationSensitivity = 90f;

    [Header("Ground Plane")]
    [SerializeField] private float groundPlaneY = 0f;

    // State tracking
    private Ship selectedShip;
    private bool isMovementModeActive = false;
    private bool isDraggingProjection = false;
    private Vector3 dragStartMousePos;
    private Vector3 projectionDragOffset;
    private AdjustmentMode adjustmentMode = AdjustmentMode.None;

    // Temporary collider for projection raycasting
    private BoxCollider tempProjectionCollider;

    // Public accessors for DebugUI
    public bool IsMovementModeActive => isMovementModeActive;
    public AdjustmentMode CurrentAdjustmentMode => adjustmentMode;

    /// <summary>
    /// Initialize camera reference if not set.
    /// Auto-select first ship found in scene.
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

        // Find DebugUI if not assigned
        if (debugUI == null)
        {
            debugUI = FindObjectOfType<DebugUI>();
        }

        // Set movement controller reference in DebugUI
        if (debugUI != null)
        {
            debugUI.SetMovementController(this);
        }

        // Find TargetingController if not assigned
        if (targetingController == null)
        {
            targetingController = FindObjectOfType<TargetingController>();
        }

        // Find player ship if not assigned (look for ship named "Hephaestus" or get from TargetingController)
        if (playerShip == null && targetingController != null)
        {
            playerShip = targetingController.PlayerShip;
        }

        // If still not found, try to find by name
        if (playerShip == null)
        {
            GameObject hephaestus = GameObject.Find("Hephaestus");
            if (hephaestus != null)
            {
                playerShip = hephaestus.GetComponent<Ship>();
            }
        }

        // Last resort: find first ship (but warn user)
        if (playerShip == null)
        {
            playerShip = FindObjectOfType<Ship>();
            if (playerShip != null)
            {
                Debug.LogWarning($"MovementController: Could not find 'Hephaestus', using {playerShip.gameObject.name} as player ship");
            }
        }

        // Auto-select player ship only
        if (playerShip != null)
        {
            SelectShip(playerShip);
            Debug.Log($"Auto-selected player ship: {playerShip.gameObject.name}. Press M to enter movement mode.");
        }
        else
        {
            Debug.LogError("MovementController: No player ship found!");
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

        // Toggle movement mode with M key
        if (Input.GetKeyDown(movementModeKey))
        {
            ToggleMovementMode();
        }

        // Exit movement mode with Escape
        if (isMovementModeActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitMovementMode();
        }

        // Ability hotkeys work anytime during Command phase (not just in movement mode)
        HandleAbilityHotkeys();

        // Only handle movement input if movement mode is active
        if (!isMovementModeActive)
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
        // Left click to start dragging projection (ship already selected)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Check if we're clicking on the projection
            if (selectedShip != null && TryStartProjectionDrag(ray))
            {
                return;
            }
        }

        // F key to focus camera on selected ship
        if (Input.GetKeyDown(focusCameraKey) && selectedShip != null && orbitCamera != null)
        {
            orbitCamera.FocusOn(selectedShip.transform);
            Debug.Log($"Camera focused on {selectedShip.gameObject.name}");
        }
    }

    /// <summary>
    /// Handle ability activation hotkeys (1-6).
    /// Keys 1-4 are also used for weapon groups when targeting enemies.
    /// Priority: If enemy targeted, weapon groups take precedence.
    /// </summary>
    private void HandleAbilityHotkeys()
    {
        if (selectedShip == null || selectedShip.AbilitySystem == null) return;

        // Check if we're currently targeting an enemy
        // If so, let TargetingController handle keys 1-4 for weapon groups
        bool isTargetingEnemy = targetingController != null && targetingController.CurrentTarget != null;

        // Keys 1-4: Only handle if NOT targeting (otherwise weapon groups take priority)
        if (!isTargetingEnemy)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                selectedShip.AbilitySystem.TryActivateAbilityByIndex(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                selectedShip.AbilitySystem.TryActivateAbilityByIndex(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                selectedShip.AbilitySystem.TryActivateAbilityByIndex(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                selectedShip.AbilitySystem.TryActivateAbilityByIndex(3);
            }
        }

        // Keys 5-6: Always available for abilities (no conflict)
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            selectedShip.AbilitySystem.TryActivateAbilityByIndex(4);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            selectedShip.AbilitySystem.TryActivateAbilityByIndex(5);
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
    /// Toggles movement mode on/off.
    /// </summary>
    public void ToggleMovementMode()
    {
        if (isMovementModeActive)
        {
            ExitMovementMode();
        }
        else
        {
            EnterMovementMode();
        }
    }

    /// <summary>
    /// Enters movement mode - shows projection and allows movement planning.
    /// Only works for the player ship.
    /// </summary>
    private void EnterMovementMode()
    {
        if (selectedShip == null)
        {
            Debug.LogWarning("No ship selected to enter movement mode!");
            return;
        }

        // Double-check we're only moving the player ship
        if (selectedShip != playerShip)
        {
            Debug.LogWarning($"Cannot enter movement mode for {selectedShip.gameObject.name} - only player ship can be moved!");
            return;
        }

        isMovementModeActive = true;
        selectedShip.Select(); // Show projection
        Debug.Log($"Movement mode activated for {selectedShip.gameObject.name}. Press M or ESC to exit.");
    }

    /// <summary>
    /// Exits movement mode - hides projection but keeps ship selected.
    /// </summary>
    public void ExitMovementMode()
    {
        isMovementModeActive = false;
        adjustmentMode = AdjustmentMode.None;

        // Hide projection but keep ship selected
        if (selectedShip != null)
        {
            GameObject projection = selectedShip.GetProjection();
            if (projection != null && !selectedShip.HasPlannedMove)
            {
                projection.SetActive(false);
            }
        }

        Debug.Log("Movement mode deactivated.");
    }

    /// <summary>
    /// Selects a ship (does not show projection - use EnterMovementMode for that).
    /// Only allows selecting the player ship for movement.
    /// </summary>
    private void SelectShip(Ship ship)
    {
        // IMPORTANT: Only allow selecting the player ship for movement control
        if (ship != playerShip)
        {
            if (verboseLogging)
            {
                Debug.Log($"MovementController: Cannot select {ship.gameObject.name} - only player ship can be moved");
            }
            return;
        }

        // Hide previous ship's projection if switching ships
        if (selectedShip != null && selectedShip != ship)
        {
            GameObject oldProjection = selectedShip.GetProjection();
            if (oldProjection != null && !selectedShip.HasPlannedMove)
            {
                oldProjection.SetActive(false);
            }
        }

        selectedShip = ship;
        adjustmentMode = AdjustmentMode.None;

        // Update DebugUI to show this ship
        if (debugUI != null)
        {
            debugUI.SetTargetShip(ship);
        }

        // Automatically focus camera on selected ship
        if (orbitCamera != null)
        {
            orbitCamera.FocusOn(ship.transform);
        }
    }

    /// <summary>
    /// Hides projection but keeps ship selected (no longer deselects).
    /// </summary>
    private void DeselectCurrentShip()
    {
        // Don't actually deselect - just hide projection
        if (selectedShip != null)
        {
            GameObject projection = selectedShip.GetProjection();
            if (projection != null && !selectedShip.HasPlannedMove)
            {
                projection.SetActive(false);
            }
        }
        adjustmentMode = AdjustmentMode.None;
        isMovementModeActive = false;
    }
}
