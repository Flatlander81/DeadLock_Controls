using UnityEngine;

/// <summary>
/// Central damage routing system for a ship.
/// Routes damage through: Shields -> Section Armor -> Section Structure.
/// Handles Core protection rules and lucky shot mechanics.
/// Auto-finds ShieldSystem and SectionManager on the ship.
/// </summary>
public class DamageRouter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShieldSystem shieldSystem;
    [SerializeField] private SectionManager sectionManager;
    [SerializeField] private Ship parentShip;
    [SerializeField] private CoreProtectionSystem coreProtection;

    [Header("Debug")]
    [SerializeField] private bool logDamageRouting = false;

    // Properties
    public ShieldSystem ShieldSystem => shieldSystem;
    public SectionManager SectionManager => sectionManager;
    public CoreProtectionSystem CoreProtection => coreProtection;

    private void Awake()
    {
        // Auto-find components
        if (shieldSystem == null)
        {
            shieldSystem = GetComponent<ShieldSystem>();
        }

        if (sectionManager == null)
        {
            sectionManager = GetComponent<SectionManager>();
            if (sectionManager == null)
            {
                sectionManager = GetComponentInChildren<SectionManager>();
            }
        }

        if (parentShip == null)
        {
            parentShip = GetComponent<Ship>();
        }

        if (coreProtection == null)
        {
            coreProtection = GetComponent<CoreProtectionSystem>();
        }
    }

    /// <summary>
    /// Sets references manually (for testing or editor setup).
    /// </summary>
    public void SetReferences(ShieldSystem shields, SectionManager sections, Ship ship = null, CoreProtectionSystem core = null)
    {
        shieldSystem = shields;
        sectionManager = sections;
        parentShip = ship;
        coreProtection = core;
    }

    /// <summary>
    /// Process damage targeting a specific section.
    /// Flow: Shields -> Core Protection Check -> Section Armor -> Section Structure.
    /// </summary>
    /// <param name="damage">Incoming damage amount.</param>
    /// <param name="targetSection">The section type being targeted.</param>
    /// <param name="attackDirection">Optional: Direction of attack for Core protection rules.</param>
    /// <returns>Detailed report of damage distribution.</returns>
    public DamageReport ProcessDamage(float damage, SectionType targetSection, Vector3? attackDirection = null)
    {
        if (damage <= 0f)
        {
            return new DamageReport();
        }

        // Handle Core protection rules
        SectionType actualTargetSection = targetSection;
        bool coreWasProtected = false;

        if (targetSection == SectionType.Core && coreProtection != null)
        {
            Vector3 direction = attackDirection ?? Vector3.forward;

            if (!coreProtection.CanHitCore(direction))
            {
                // Core is protected - redirect to adjacent section
                Vector3 localDir = transform.InverseTransformDirection(direction.normalized);
                actualTargetSection = coreProtection.GetAdjacentSection(localDir);
                coreWasProtected = true;

                if (logDamageRouting)
                {
                    Debug.Log($"[DamageRouter] Core protected - redirecting to {actualTargetSection}");
                }
            }
        }

        float remainingDamage = damage;
        float shieldDamage = 0f;
        bool shieldsDepleted = false;

        // Step 1: Shields absorb damage first
        if (shieldSystem != null && shieldSystem.IsShieldActive)
        {
            float beforeShields = shieldSystem.CurrentShields;
            remainingDamage = shieldSystem.AbsorbDamage(remainingDamage);
            shieldDamage = beforeShields - shieldSystem.CurrentShields;
            shieldsDepleted = beforeShields > 0f && shieldSystem.CurrentShields <= 0f;

            if (logDamageRouting)
            {
                Debug.Log($"[DamageRouter] Shields absorbed {shieldDamage:F1}, {remainingDamage:F1} remaining");
            }

            // If shields absorbed all damage
            if (remainingDamage <= 0f)
            {
                if (logDamageRouting)
                {
                    Debug.Log($"[DamageRouter] All damage absorbed by shields");
                }

                return new DamageReport(
                    totalIncoming: damage,
                    shieldDamage: shieldDamage,
                    armorDamage: 0f,
                    structureDamage: 0f,
                    overflowDamage: 0f,
                    shieldsDepleted: shieldsDepleted,
                    armorBroken: false,
                    sectionBreached: false,
                    sectionHit: actualTargetSection,
                    section: null,
                    coreWasProtected: coreWasProtected
                );
            }
        }

        // Step 2: Apply remaining damage to section
        ShipSection section = null;
        DamageResult sectionResult = new DamageResult();
        DamageResult coreOverflowResult = new DamageResult();
        bool hadCoreOverflow = false;

        if (sectionManager != null)
        {
            section = sectionManager.GetSection(actualTargetSection);
            if (section != null)
            {
                sectionResult = section.ApplyDamage(remainingDamage);

                if (logDamageRouting)
                {
                    Debug.Log($"[DamageRouter] Section {actualTargetSection}: Armor={sectionResult.DamageToArmor:F1}, " +
                             $"Structure={sectionResult.DamageToStructure:F1}, Overflow={sectionResult.OverflowDamage:F1}");
                }

                // Step 3: Handle overflow from breached sections - route to Core
                if (sectionResult.WasAlreadyBreached && sectionResult.OverflowDamage > 0f &&
                    actualTargetSection != SectionType.Core)
                {
                    ShipSection coreSection = sectionManager.GetSection(SectionType.Core);
                    if (coreSection != null)
                    {
                        coreOverflowResult = coreSection.ApplyDamage(sectionResult.OverflowDamage);
                        hadCoreOverflow = true;

                        if (logDamageRouting)
                        {
                            Debug.Log($"[DamageRouter] Breached section overflow! Core took {coreOverflowResult.DamageToArmor + coreOverflowResult.DamageToStructure:F1} damage!");
                        }
                    }
                }

                // Step 4: Check for lucky shot to Core on structure damage
                if (sectionResult.DamageToStructure > 0f &&
                    actualTargetSection != SectionType.Core &&
                    coreProtection != null)
                {
                    if (coreProtection.RollLuckyShot())
                    {
                        // Lucky shot! Route some damage to Core
                        ShipSection coreSection = sectionManager.GetSection(SectionType.Core);
                        if (coreSection != null)
                        {
                            // Lucky shot deals the same structure damage to Core
                            DamageResult luckyResult = coreSection.ApplyDamage(sectionResult.DamageToStructure);

                            if (logDamageRouting)
                            {
                                Debug.Log($"[DamageRouter] LUCKY SHOT! Core took {luckyResult.DamageToStructure:F1} damage!");
                            }

                            // Return combined report
                            return new DamageReport(
                                totalIncoming: damage,
                                shieldDamage: shieldDamage,
                                armorDamage: sectionResult.DamageToArmor,
                                structureDamage: sectionResult.DamageToStructure + luckyResult.DamageToStructure,
                                overflowDamage: sectionResult.OverflowDamage,
                                shieldsDepleted: shieldsDepleted,
                                armorBroken: sectionResult.ArmorBroken,
                                sectionBreached: sectionResult.SectionBreached || luckyResult.SectionBreached,
                                sectionHit: actualTargetSection,
                                section: section,
                                criticalResult: sectionResult.CriticalResult,
                                wasLuckyShot: true,
                                coreWasProtected: coreWasProtected
                            );
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[DamageRouter] Section {actualTargetSection} not found, damage lost");
            }
        }
        else
        {
            Debug.LogWarning("[DamageRouter] No SectionManager found, damage lost");
        }

        // Build final report, including any Core overflow damage
        float totalStructureDamage = sectionResult.DamageToStructure;
        float finalOverflow = sectionResult.OverflowDamage;
        bool anySectionBreached = sectionResult.SectionBreached;

        if (hadCoreOverflow)
        {
            totalStructureDamage += coreOverflowResult.DamageToArmor + coreOverflowResult.DamageToStructure;
            finalOverflow = coreOverflowResult.OverflowDamage; // Core's overflow (if Core also breached)
            anySectionBreached = anySectionBreached || coreOverflowResult.SectionBreached;
        }

        return new DamageReport(
            totalIncoming: damage,
            shieldDamage: shieldDamage,
            armorDamage: sectionResult.DamageToArmor,
            structureDamage: totalStructureDamage,
            overflowDamage: finalOverflow,
            shieldsDepleted: shieldsDepleted,
            armorBroken: sectionResult.ArmorBroken,
            sectionBreached: anySectionBreached,
            sectionHit: actualTargetSection,
            section: section,
            criticalResult: sectionResult.CriticalResult ?? coreOverflowResult.CriticalResult,
            coreWasProtected: coreWasProtected,
            overflowedToCore: hadCoreOverflow
        );
    }

    /// <summary>
    /// Process damage at a world position (determines section from collider).
    /// </summary>
    /// <param name="damage">Incoming damage amount.</param>
    /// <param name="worldPoint">World position where damage occurred.</param>
    /// <returns>Detailed report of damage distribution.</returns>
    public DamageReport ProcessDamageAtPoint(float damage, Vector3 worldPoint)
    {
        // Find the closest section to this point
        SectionType closestSection = FindClosestSection(worldPoint);
        return ProcessDamage(damage, closestSection);
    }

    /// <summary>
    /// Finds the closest section to a world point.
    /// </summary>
    private SectionType FindClosestSection(Vector3 worldPoint)
    {
        if (sectionManager == null)
        {
            return SectionType.Core;
        }

        ShipSection closest = null;
        float closestDistance = float.MaxValue;

        foreach (ShipSection section in sectionManager.GetAllSections())
        {
            if (section == null) continue;

            float distance = Vector3.Distance(worldPoint, section.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = section;
            }
        }

        return closest != null ? closest.SectionType : SectionType.Core;
    }

    /// <summary>
    /// Gets the total effective HP (shields + all sections).
    /// </summary>
    public float GetTotalEffectiveHP()
    {
        float total = 0f;

        if (shieldSystem != null)
        {
            total += shieldSystem.CurrentShields;
        }

        if (sectionManager != null)
        {
            total += sectionManager.GetTotalArmorRemaining();
            total += sectionManager.GetTotalStructureRemaining();
        }

        return total;
    }

    /// <summary>
    /// Gets maximum effective HP (max shields + all max sections).
    /// </summary>
    public float GetMaxEffectiveHP()
    {
        float total = 0f;

        if (shieldSystem != null)
        {
            total += shieldSystem.MaxShields;
        }

        if (sectionManager != null)
        {
            total += sectionManager.GetTotalMaxArmor();
            total += sectionManager.GetTotalMaxStructure();
        }

        return total;
    }
}
