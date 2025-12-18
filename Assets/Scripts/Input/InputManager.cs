using UnityEngine;
using System;

/// <summary>
/// Centralized input manager for handling hotkeys across different game systems.
/// Prevents duplicate hotkey handling code and manages input priority.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Ship playerShip;
    [SerializeField] private TargetingController targetingController;
    [SerializeField] private MovementController movementController;

    [Header("Hotkey Configuration")]
    [SerializeField] private KeyCode[] numberKeys = new KeyCode[]
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6
    };

    // Events for input actions
    public event Action<int> OnAbilityKeyPressed;
    public event Action<int> OnWeaponGroupKeyPressed;
    public event Action OnAlphaStrikeKeyPressed;

    /// <summary>
    /// Number of weapon groups (1-4).
    /// </summary>
    public const int WeaponGroupCount = 4;

    /// <summary>
    /// Number of ability slots (1-6).
    /// </summary>
    public const int AbilitySlotCount = 6;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Auto-find references if not assigned
        if (playerShip == null && targetingController != null)
        {
            playerShip = targetingController.PlayerShip;
        }

        if (targetingController == null)
        {
            targetingController = FindFirstObjectByType<TargetingController>();
        }

        if (movementController == null)
        {
            movementController = FindFirstObjectByType<MovementController>();
        }
    }

    private void Update()
    {
        // Only process input during Command phase
        if (!IsCommandPhase()) return;

        HandleNumberKeyInput();
        HandleAlphaStrikeInput();
    }

    /// <summary>
    /// Check if we're in Command phase.
    /// </summary>
    private bool IsCommandPhase()
    {
        return TurnManager.Instance != null &&
               TurnManager.Instance.CurrentPhase == TurnManager.Phase.Command;
    }

    /// <summary>
    /// Check if an enemy is currently targeted.
    /// </summary>
    public bool HasEnemyTargeted()
    {
        return targetingController != null && targetingController.CurrentTarget != null;
    }

    /// <summary>
    /// Handle number key input (1-6) for abilities and weapon groups.
    /// Priority: If enemy targeted, keys 1-4 fire weapon groups. Otherwise, activate abilities.
    /// </summary>
    private void HandleNumberKeyInput()
    {
        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (Input.GetKeyDown(numberKeys[i]))
            {
                int keyNumber = i + 1; // Convert 0-indexed to 1-indexed

                // Keys 1-4: Weapon groups take priority when targeting enemy
                if (keyNumber <= WeaponGroupCount && HasEnemyTargeted())
                {
                    FireWeaponGroup(keyNumber);
                    OnWeaponGroupKeyPressed?.Invoke(keyNumber);
                }
                else
                {
                    // Activate ability
                    ActivateAbility(i);
                    OnAbilityKeyPressed?.Invoke(i);
                }
            }
        }
    }

    /// <summary>
    /// Handle alpha strike input (A key when targeting).
    /// </summary>
    private void HandleAlphaStrikeInput()
    {
        if (Input.GetKeyDown(KeyCode.A) && HasEnemyTargeted())
        {
            FireAlphaStrike();
            OnAlphaStrikeKeyPressed?.Invoke();
        }
    }

    /// <summary>
    /// Activate ability by slot index (0-5).
    /// </summary>
    private void ActivateAbility(int slotIndex)
    {
        if (playerShip == null || playerShip.AbilitySystem == null) return;

        playerShip.AbilitySystem.TryActivateAbilityByIndex(slotIndex);
    }

    /// <summary>
    /// Fire weapon group (1-4).
    /// </summary>
    private void FireWeaponGroup(int groupNumber)
    {
        if (targetingController == null) return;

        targetingController.FireGroupAtCurrentTarget(groupNumber);
    }

    /// <summary>
    /// Fire all weapons at current target.
    /// </summary>
    private void FireAlphaStrike()
    {
        if (targetingController == null) return;

        targetingController.AlphaStrikeCurrentTarget();
    }

    // Static lookup array for number keys (avoids enum arithmetic)
    private static readonly KeyCode[] NumberKeyLookup = new KeyCode[]
    {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6
    };

    /// <summary>
    /// Check if a specific number key is pressed this frame.
    /// Uses array lookup instead of enum arithmetic for safety.
    /// </summary>
    public static bool IsNumberKeyDown(int number)
    {
        if (number < 1 || number > NumberKeyLookup.Length) return false;

        return Input.GetKeyDown(NumberKeyLookup[number - 1]);
    }

    /// <summary>
    /// Get the key code for a number key (1-6).
    /// Uses array lookup instead of enum arithmetic for safety.
    /// </summary>
    public static KeyCode GetNumberKeyCode(int number)
    {
        if (number < 1 || number > NumberKeyLookup.Length) return KeyCode.None;

        return NumberKeyLookup[number - 1];
    }
}
