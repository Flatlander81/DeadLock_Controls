using UnityEngine;

/// <summary>
/// Manages UI panel visibility and state transitions.
/// Coordinates WeaponConfigPanel and WeaponGroupPanel based on ship selection.
/// Three states:
/// - Nothing selected: All panels hidden
/// - Enemy selected: Show WeaponGroupPanel (firing)
/// - Player selected: Show WeaponConfigPanel (configuration)
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private WeaponConfigPanel weaponConfigPanel;
    [SerializeField] private WeaponGroupPanel weaponGroupPanel;

    [Header("Controller References")]
    [SerializeField] private TargetingController targetingController;

    [Header("Ship References")]
    [SerializeField] private Ship playerShip;

    /// <summary>
    /// Current UI state.
    /// </summary>
    private enum UIState
    {
        NothingSelected,
        EnemySelected,
        PlayerSelected
    }

    private UIState currentState = UIState.NothingSelected;

    private void Start()
    {
        // Find or create panel components
        InitializePanels();

        // Subscribe to targeting controller events
        if (targetingController != null)
        {
            targetingController.OnShipSelected += HandleShipSelected;
            targetingController.OnShipDeselected += HandleShipDeselected;
        }
        else
        {
            Debug.LogWarning("UIManager: No TargetingController assigned");
        }

        // Start with all panels hidden
        SetState(UIState.NothingSelected);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (targetingController != null)
        {
            targetingController.OnShipSelected -= HandleShipSelected;
            targetingController.OnShipDeselected -= HandleShipDeselected;
        }
    }

    /// <summary>
    /// Initialize panel components.
    /// </summary>
    private void InitializePanels()
    {
        // Find player ship if not assigned
        if (playerShip == null)
        {
            playerShip = FindFirstObjectByType<Ship>();
            Debug.LogWarning("UIManager: PlayerShip not assigned, auto-found first Ship");
        }

        // Create WeaponConfigPanel if not assigned
        if (weaponConfigPanel == null)
        {
            GameObject configPanelObj = new GameObject("WeaponConfigPanel");
            weaponConfigPanel = configPanelObj.AddComponent<WeaponConfigPanel>();
            configPanelObj.transform.SetParent(transform);
        }

        // Create WeaponGroupPanel if not assigned
        if (weaponGroupPanel == null)
        {
            GameObject groupPanelObj = new GameObject("WeaponGroupPanel");
            weaponGroupPanel = groupPanelObj.AddComponent<WeaponGroupPanel>();
            groupPanelObj.transform.SetParent(transform);
        }

        // Initialize panels
        if (playerShip != null)
        {
            weaponConfigPanel.Initialize(playerShip);

            if (targetingController != null)
            {
                weaponGroupPanel.Initialize(playerShip, targetingController);
            }
        }

        Debug.Log("UIManager: Panels initialized");
    }

    /// <summary>
    /// Handle ship selection event from TargetingController.
    /// </summary>
    private void HandleShipSelected(Ship ship)
    {
        if (ship == null) return;

        // Determine if it's player or enemy
        if (ship == playerShip)
        {
            SetState(UIState.PlayerSelected);
        }
        else
        {
            SetState(UIState.EnemySelected);
            weaponGroupPanel.SetTarget(ship);
        }

        Debug.Log($"[UIManager] Ship selected: {ship.gameObject.name}, State: {currentState}");
    }

    /// <summary>
    /// Handle ship deselection event from TargetingController.
    /// </summary>
    private void HandleShipDeselected()
    {
        SetState(UIState.NothingSelected);
        Debug.Log("[UIManager] Ship deselected, State: NothingSelected");
    }

    /// <summary>
    /// Set the UI state and update panel visibility.
    /// </summary>
    private void SetState(UIState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case UIState.NothingSelected:
                HideAllPanels();
                break;

            case UIState.EnemySelected:
                weaponConfigPanel.Hide();
                weaponGroupPanel.Show();
                break;

            case UIState.PlayerSelected:
                weaponConfigPanel.Show();
                weaponGroupPanel.Hide();
                break;
        }
    }

    /// <summary>
    /// Hide all UI panels.
    /// </summary>
    private void HideAllPanels()
    {
        if (weaponConfigPanel != null)
            weaponConfigPanel.Hide();

        if (weaponGroupPanel != null)
            weaponGroupPanel.Hide();
    }

    /// <summary>
    /// Force show weapon config panel (for debugging).
    /// </summary>
    public void ShowWeaponConfig()
    {
        SetState(UIState.PlayerSelected);
    }

    /// <summary>
    /// Force show weapon group panel (for debugging).
    /// </summary>
    public void ShowWeaponGroups(Ship target)
    {
        weaponGroupPanel.SetTarget(target);
        SetState(UIState.EnemySelected);
    }

    /// <summary>
    /// Get current UI state (for debugging).
    /// </summary>
    public string GetCurrentState()
    {
        return currentState.ToString();
    }
}
