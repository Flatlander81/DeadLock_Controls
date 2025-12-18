using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Track C: Targeting UI System
/// Handles target selection, weapon group assignment, and firing coordination.
/// Integrates with Track A (Weapons) and Track B (Projectiles).
/// </summary>
public class TargetingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship playerShip;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject selectionIndicatorPrefab;

    [Header("Selection State")]
    [SerializeField] private Ship currentTarget;
    [SerializeField] private Ship selectedShip; // Can be player or enemy

    [Header("Visual Feedback")]
    private SelectionIndicator currentIndicator;
    private Dictionary<int, TargetingLineRenderer> groupTargetingLines = new Dictionary<int, TargetingLineRenderer>();

    // Public properties
    public Ship CurrentTarget => currentTarget;
    public Ship SelectedShip => selectedShip;
    public Ship PlayerShip => playerShip;

    // Events for UI updates
    public System.Action<Ship> OnTargetSelected;
    public System.Action OnTargetDeselected;
    public System.Action<Ship> OnShipSelected; // Any ship (player or enemy)
    public System.Action OnShipDeselected;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (playerShip == null)
        {
            playerShip = FindFirstObjectByType<Ship>();
            Debug.LogWarning("TargetingController: PlayerShip not assigned, auto-found first Ship");
        }

        // Initialize targeting line dictionary
        for (int i = 1; i <= 4; i++)
        {
            groupTargetingLines[i] = null;
        }
    }

    private void Update()
    {
        // Don't interfere with movement planning
        // Left-click for selection only
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }

        // Hotkeys for weapon groups (only if enemy targeted)
        if (currentTarget != null)
        {
            HandleWeaponGroupHotkeys();
        }
    }

    /// <summary>
    /// Handle mouse clicks for ship selection.
    /// </summary>
    private void HandleMouseClick()
    {
        Ship clickedShip = GetShipUnderMouse();

        if (clickedShip != null)
        {
            SelectShip(clickedShip);

            // If clicking enemy, also set as current target
            if (clickedShip != playerShip)
            {
                SelectTarget(clickedShip);
            }
        }
        else
        {
            // Clicked empty space
            DeselectAll();
        }
    }

    /// <summary>
    /// Raycast to find ship under mouse cursor.
    /// </summary>
    public Ship GetShipUnderMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f))
        {
            Ship ship = hit.collider.GetComponent<Ship>();
            if (ship == null)
            {
                ship = hit.collider.GetComponentInParent<Ship>();
            }
            return ship;
        }

        return null;
    }

    /// <summary>
    /// Select a ship (player or enemy) for UI interaction.
    /// </summary>
    public void SelectShip(Ship ship)
    {
        if (ship == null) return;

        // Deselect previous
        if (selectedShip != null)
        {
            RemoveSelectionIndicator();
        }

        selectedShip = ship;
        CreateSelectionIndicator(ship);

        OnShipSelected?.Invoke(ship);

        Debug.Log($"[TargetingController] Selected ship: {ship.gameObject.name}");
    }

    /// <summary>
    /// Select an enemy ship as current weapon target.
    /// </summary>
    public void SelectTarget(Ship target)
    {
        if (target == null || target == playerShip) return;

        currentTarget = target;
        OnTargetSelected?.Invoke(target);

        Debug.Log($"[TargetingController] Target selected: {target.gameObject.name}");
    }

    /// <summary>
    /// Deselect current target.
    /// </summary>
    public void DeselectTarget()
    {
        if (currentTarget == null) return;

        Ship previousTarget = currentTarget;
        currentTarget = null;

        OnTargetDeselected?.Invoke();

        Debug.Log($"[TargetingController] Target deselected: {previousTarget.gameObject.name}");
    }

    /// <summary>
    /// Deselect everything.
    /// </summary>
    public void DeselectAll()
    {
        DeselectTarget();

        if (selectedShip != null)
        {
            RemoveSelectionIndicator();
            selectedShip = null;
            OnShipDeselected?.Invoke();
        }
    }

    /// <summary>
    /// Assign a weapon group to fire at the current target.
    /// </summary>
    public void AssignGroupToCurrentTarget(int groupNumber)
    {
        if (currentTarget == null)
        {
            Debug.LogWarning("[TargetingController] No target selected for group assignment");
            return;
        }

        if (playerShip == null || playerShip.WeaponManager == null)
        {
            Debug.LogError("[TargetingController] Player ship or WeaponManager not found");
            return;
        }

        // Assign target to group
        playerShip.WeaponManager.SetGroupTarget(groupNumber, currentTarget);

        // Create/update targeting line
        UpdateTargetingLine(groupNumber, currentTarget);

        Debug.Log($"[TargetingController] Group {groupNumber} assigned to {currentTarget.gameObject.name}");
    }

    /// <summary>
    /// Fire a weapon group at current target.
    /// </summary>
    public void FireGroupAtCurrentTarget(int groupNumber)
    {
        if (currentTarget == null)
        {
            Debug.LogWarning($"[TargetingController] Cannot fire group {groupNumber}: No target selected");
            return;
        }

        if (playerShip == null || playerShip.WeaponManager == null)
        {
            Debug.LogError("[TargetingController] Player ship or WeaponManager not found");
            return;
        }

        // Ensure group is targeting current target
        AssignGroupToCurrentTarget(groupNumber);

        // Fire the group
        StartCoroutine(playerShip.WeaponManager.FireGroup(groupNumber));

        Debug.Log($"[TargetingController] Firing group {groupNumber} at {currentTarget.gameObject.name}");
    }

    /// <summary>
    /// Fire all weapons (Alpha Strike) at current target.
    /// </summary>
    public void AlphaStrikeCurrentTarget()
    {
        if (currentTarget == null)
        {
            Debug.LogWarning("[TargetingController] Cannot Alpha Strike: No target selected");
            return;
        }

        if (playerShip == null || playerShip.WeaponManager == null)
        {
            Debug.LogError("[TargetingController] Player ship or WeaponManager not found");
            return;
        }

        // Fire alpha strike
        StartCoroutine(playerShip.WeaponManager.FireAlphaStrike(currentTarget));

        Debug.Log($"[TargetingController] ALPHA STRIKE on {currentTarget.gameObject.name}");
    }

    /// <summary>
    /// Handle weapon group firing hotkeys (1-4) and Alpha Strike (A).
    /// Delegated to InputManager for centralized handling.
    /// </summary>
    private void HandleWeaponGroupHotkeys()
    {
        // Input handling is now centralized in InputManager
        // This method is kept for backwards compatibility but delegates to InputManager
        // If InputManager doesn't exist, fall back to local handling
        if (InputManager.Instance != null) return;

        // Fallback: Local handling if InputManager not present
        for (int i = 1; i <= InputManager.WeaponGroupCount; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + (i - 1))))
            {
                FireGroupAtCurrentTarget(i);
                return;
            }
        }

        // Alpha Strike with 'A' key
        if (Input.GetKeyDown(KeyCode.A))
        {
            AlphaStrikeCurrentTarget();
        }
    }

    /// <summary>
    /// Create selection indicator on ship.
    /// </summary>
    private void CreateSelectionIndicator(Ship ship)
    {
        if (selectionIndicatorPrefab != null)
        {
            GameObject indicatorObj = Instantiate(selectionIndicatorPrefab, ship.transform.position, Quaternion.identity);
            currentIndicator = indicatorObj.GetComponent<SelectionIndicator>();

            if (currentIndicator != null)
            {
                currentIndicator.Initialize(ship, ship == playerShip);
            }
        }
        else
        {
            Debug.LogWarning("[TargetingController] No selection indicator prefab assigned");
        }
    }

    /// <summary>
    /// Remove current selection indicator.
    /// </summary>
    private void RemoveSelectionIndicator()
    {
        if (currentIndicator != null)
        {
            Destroy(currentIndicator.gameObject);
            currentIndicator = null;
        }
    }

    /// <summary>
    /// Update or create targeting line for weapon group.
    /// </summary>
    private void UpdateTargetingLine(int groupNumber, Ship target)
    {
        if (groupTargetingLines.ContainsKey(groupNumber))
        {
            // Remove old line
            if (groupTargetingLines[groupNumber] != null)
            {
                Destroy(groupTargetingLines[groupNumber].gameObject);
            }

            // Create new line
            GameObject lineObj = new GameObject($"TargetingLine_Group{groupNumber}");
            TargetingLineRenderer lineRenderer = lineObj.AddComponent<TargetingLineRenderer>();
            lineRenderer.Initialize(playerShip, target, groupNumber);

            groupTargetingLines[groupNumber] = lineRenderer;
        }
    }

    /// <summary>
    /// Clear all targeting lines.
    /// </summary>
    public void ClearAllTargetingLines()
    {
        foreach (var kvp in groupTargetingLines)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }

        for (int i = 1; i <= 4; i++)
        {
            groupTargetingLines[i] = null;
        }
    }

    /// <summary>
    /// Get valid enemy targets (all ships except player).
    /// Uses cached ship list from TurnManager to avoid expensive scene search.
    /// </summary>
    public List<Ship> GetValidTargets()
    {
        List<Ship> targets = new List<Ship>();

        // Use TurnManager's cached ship list instead of FindObjectsByType
        Ship[] allShips = TurnManager.Instance != null
            ? TurnManager.Instance.GetAllShips()
            : FindObjectsByType<Ship>(FindObjectsSortMode.None);

        foreach (Ship ship in allShips)
        {
            if (ship != playerShip && !ship.IsDead)
            {
                targets.Add(ship);
            }
        }

        return targets;
    }

    /// <summary>
    /// Check if ship is valid target.
    /// </summary>
    public bool IsValidTarget(Ship ship)
    {
        return ship != null && ship != playerShip && !ship.IsDead;
    }
}
