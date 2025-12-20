using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Validates weapon firing arcs during ship movement.
/// Provides movement-aware arc checking for the weapon firing queue.
/// Can check current positions or predict arc validity during planned movements.
///
/// Part of Phase 3.5.4 - Movement and Weapon Arc Integration.
/// </summary>
public class WeaponArcValidator : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int arcCheckSamples = 10;
    [SerializeField] private bool verboseLogging = false;

    [Header("References")]
    [SerializeField] private MovementExecutor movementExecutor;

    // Cached validation results
    private Dictionary<WeaponSystem, ArcValidationResult> cachedResults = new Dictionary<WeaponSystem, ArcValidationResult>();

    /// <summary>
    /// Result of arc validation for a weapon.
    /// </summary>
    public struct ArcValidationResult
    {
        public bool WillBeInArc;
        public float OptimalFiringTime;
        public float MinAngleToTarget;
        public List<(float start, float end)> FiringWindows;
        public bool IsInRangeAtOptimalTime;
        public string ValidationMessage;
    }

    private void Start()
    {
        // Auto-discover references
        if (movementExecutor == null)
        {
            movementExecutor = FindAnyObjectByType<MovementExecutor>();
        }

        Debug.Log("WeaponArcValidator initialized");
    }

    /// <summary>
    /// Validates if a weapon can fire at a target, accounting for planned movement.
    /// </summary>
    /// <param name="weapon">Weapon to validate</param>
    /// <param name="target">Target ship</param>
    /// <returns>Validation result with firing windows and optimal time</returns>
    public ArcValidationResult ValidateArc(WeaponSystem weapon, Ship target)
    {
        var result = new ArcValidationResult
        {
            WillBeInArc = false,
            OptimalFiringTime = -1f,
            MinAngleToTarget = float.MaxValue,
            FiringWindows = new List<(float start, float end)>(),
            IsInRangeAtOptimalTime = false,
            ValidationMessage = ""
        };

        if (weapon == null || target == null)
        {
            result.ValidationMessage = "Weapon or target is null";
            return result;
        }

        Ship firingShip = weapon.OwnerShip;
        if (firingShip == null)
        {
            result.ValidationMessage = "Weapon has no owner ship";
            return result;
        }

        // Check if both ships have planned movement or if we should use current positions
        bool hasMovement = firingShip.HasPlannedMove || target.HasPlannedMove;

        if (hasMovement && movementExecutor != null)
        {
            // Movement-aware validation
            result = ValidateWithMovement(weapon, firingShip, target);
        }
        else
        {
            // Static validation (current positions only)
            result = ValidateStatic(weapon, firingShip, target);
        }

        // Cache result
        cachedResults[weapon] = result;

        if (verboseLogging)
        {
            Debug.Log($"[WeaponArcValidator] {weapon.WeaponName}: {result.ValidationMessage}");
        }

        return result;
    }

    /// <summary>
    /// Validates arc using current static positions (no movement).
    /// </summary>
    private ArcValidationResult ValidateStatic(WeaponSystem weapon, Ship firingShip, Ship target)
    {
        var result = new ArcValidationResult
        {
            FiringWindows = new List<(float start, float end)>()
        };

        Vector3 targetPos = target.transform.position;
        bool isInArc = weapon.IsInArc(targetPos);
        bool isInRange = weapon.IsInRange(targetPos);

        result.WillBeInArc = isInArc;
        result.OptimalFiringTime = isInArc ? 0f : -1f;
        result.IsInRangeAtOptimalTime = isInRange;

        // Calculate angle
        Vector3 toTarget = (targetPos - weapon.transform.position).normalized;
        result.MinAngleToTarget = Vector3.Angle(weapon.transform.forward, toTarget);

        if (isInArc)
        {
            result.FiringWindows.Add((0f, 1f));
            result.ValidationMessage = isInRange
                ? "In arc and range (static)"
                : "In arc but out of range (static)";
        }
        else
        {
            result.ValidationMessage = $"Not in arc (angle: {result.MinAngleToTarget:F1}°, max: {weapon.FiringArc / 2f:F1}°)";
        }

        return result;
    }

    /// <summary>
    /// Validates arc accounting for ship movement during simulation.
    /// </summary>
    private ArcValidationResult ValidateWithMovement(WeaponSystem weapon, Ship firingShip, Ship target)
    {
        var result = new ArcValidationResult
        {
            FiringWindows = new List<(float start, float end)>(),
            MinAngleToTarget = float.MaxValue
        };

        float bestTime = -1f;
        float bestAngle = float.MaxValue;
        bool wasInArc = false;
        float windowStart = 0f;

        for (int i = 0; i <= arcCheckSamples; i++)
        {
            float t = i / (float)arcCheckSamples;

            // Get positions at this time
            Vector3 firingPos = firingShip.GetPositionAtTime(t);
            Quaternion firingRot = firingShip.GetRotationAtTime(t);
            Vector3 targetPos = target.GetPositionAtTime(t);

            // Calculate weapon's forward direction at this time
            // Weapon hardpoint inherits ship's rotation
            Vector3 weaponForward = firingRot * Vector3.forward;

            // Calculate angle to target
            Vector3 toTarget = (targetPos - firingPos).normalized;
            float angle = Vector3.Angle(weaponForward, toTarget);

            // Track minimum angle
            if (angle < result.MinAngleToTarget)
            {
                result.MinAngleToTarget = angle;
            }

            bool isInArc = angle < (weapon.FiringArc / 2f);

            if (isInArc)
            {
                // Prefer the time with best angle
                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    bestTime = t;
                }
            }

            // Track firing windows
            if (isInArc && !wasInArc)
            {
                windowStart = t;
                wasInArc = true;
            }
            else if (!isInArc && wasInArc)
            {
                result.FiringWindows.Add((windowStart, t));
                wasInArc = false;
            }
        }

        // Close final window if still open
        if (wasInArc)
        {
            result.FiringWindows.Add((windowStart, 1f));
        }

        result.WillBeInArc = bestTime >= 0f;
        result.OptimalFiringTime = bestTime;

        // Check range at optimal time
        if (bestTime >= 0f)
        {
            Vector3 firingPos = firingShip.GetPositionAtTime(bestTime);
            Vector3 targetPos = target.GetPositionAtTime(bestTime);
            float distance = Vector3.Distance(firingPos, targetPos);
            result.IsInRangeAtOptimalTime = distance <= weapon.MaxRange;

            result.ValidationMessage = result.IsInRangeAtOptimalTime
                ? $"In arc at t={bestTime:F2} ({result.FiringWindows.Count} window(s))"
                : $"In arc at t={bestTime:F2} but out of range";
        }
        else
        {
            result.ValidationMessage = $"Never in arc (min angle: {result.MinAngleToTarget:F1}°)";
        }

        return result;
    }

    /// <summary>
    /// Gets cached validation result for a weapon.
    /// Returns null if no cached result exists.
    /// </summary>
    public ArcValidationResult? GetCachedResult(WeaponSystem weapon)
    {
        if (cachedResults.TryGetValue(weapon, out var result))
        {
            return result;
        }
        return null;
    }

    /// <summary>
    /// Clears cached validation results.
    /// Call at start of each turn or when movement plans change.
    /// </summary>
    public void ClearCache()
    {
        cachedResults.Clear();
    }

    /// <summary>
    /// Checks if a weapon is currently in arc (instantaneous check, no movement).
    /// </summary>
    public bool IsInArcNow(WeaponSystem weapon, Ship target)
    {
        if (weapon == null || target == null) return false;
        return weapon.IsInArc(target.transform.position);
    }

    /// <summary>
    /// Checks if a weapon will be in arc at a specific time during simulation.
    /// </summary>
    public bool IsInArcAtTime(WeaponSystem weapon, Ship target, float normalizedTime)
    {
        if (weapon == null || target == null) return false;

        Ship firingShip = weapon.OwnerShip;
        if (firingShip == null) return false;

        Vector3 firingPos = firingShip.GetPositionAtTime(normalizedTime);
        Quaternion firingRot = firingShip.GetRotationAtTime(normalizedTime);
        Vector3 targetPos = target.GetPositionAtTime(normalizedTime);

        // Calculate weapon's forward direction
        Vector3 weaponForward = firingRot * Vector3.forward;

        // Calculate angle to target
        Vector3 toTarget = (targetPos - firingPos).normalized;
        float angle = Vector3.Angle(weaponForward, toTarget);

        return angle < (weapon.FiringArc / 2f);
    }

    /// <summary>
    /// Calculates the angle to target at a specific time.
    /// </summary>
    public float GetAngleToTargetAtTime(WeaponSystem weapon, Ship target, float normalizedTime)
    {
        if (weapon == null || target == null) return float.MaxValue;

        Ship firingShip = weapon.OwnerShip;
        if (firingShip == null) return float.MaxValue;

        Vector3 firingPos = firingShip.GetPositionAtTime(normalizedTime);
        Quaternion firingRot = firingShip.GetRotationAtTime(normalizedTime);
        Vector3 targetPos = target.GetPositionAtTime(normalizedTime);

        Vector3 weaponForward = firingRot * Vector3.forward;
        Vector3 toTarget = (targetPos - firingPos).normalized;

        return Vector3.Angle(weaponForward, toTarget);
    }

    /// <summary>
    /// Gets optimal firing time for a weapon against a target.
    /// </summary>
    public float GetOptimalFiringTime(WeaponSystem weapon, Ship target)
    {
        var result = ValidateArc(weapon, target);
        return result.OptimalFiringTime;
    }

    /// <summary>
    /// Validates all weapons on a ship against a target.
    /// </summary>
    public Dictionary<WeaponSystem, ArcValidationResult> ValidateAllWeapons(Ship firingShip, Ship target)
    {
        var results = new Dictionary<WeaponSystem, ArcValidationResult>();

        if (firingShip == null || target == null) return results;

        WeaponManager wm = firingShip.WeaponManager;
        if (wm == null) return results;

        WeaponSystem[] weapons = firingShip.GetComponentsInChildren<WeaponSystem>();
        foreach (var weapon in weapons)
        {
            results[weapon] = ValidateArc(weapon, target);
        }

        return results;
    }

    /// <summary>
    /// Sets the number of samples for arc checking.
    /// Higher values are more accurate but slower.
    /// </summary>
    public void SetArcCheckSamples(int samples)
    {
        arcCheckSamples = Mathf.Max(2, samples);
    }
}
