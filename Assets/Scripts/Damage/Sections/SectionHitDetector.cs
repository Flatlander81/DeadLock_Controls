using UnityEngine;

/// <summary>
/// Detects projectile hits on a ship section collider.
/// Routes damage through DamageRouter for proper shields → armor → structure flow.
/// Attach to section collider GameObjects. Auto-finds parent ShipSection.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SectionHitDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipSection parentSection;
    [SerializeField] private DamageRouter damageRouter;

    [Header("Debug")]
    [SerializeField] private bool logHits = false;

    // Last damage report for external access
    private DamageReport lastDamageReport;

    // Properties
    public ShipSection ParentSection => parentSection;
    public DamageRouter DamageRouter => damageRouter;
    public DamageReport LastDamageReport => lastDamageReport;

    private void Awake()
    {
        // Auto-find parent section if not assigned
        if (parentSection == null)
        {
            parentSection = GetComponentInParent<ShipSection>();
        }

        if (parentSection == null)
        {
            Debug.LogError($"[SectionHitDetector] No parent ShipSection found for {gameObject.name}");
        }

        // Ensure collider is set as trigger
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"[SectionHitDetector] Collider on {gameObject.name} is not a trigger, setting isTrigger=true");
            col.isTrigger = true;
        }
    }

    private void Start()
    {
        // Find DamageRouter in Start() to ensure Ship components are initialized
        // This runs after Awake() on all objects, so Ship.DamageRouter should be available
        if (damageRouter == null)
        {
            // Try to get from parent section's ship first
            if (parentSection != null && parentSection.ParentShip != null)
            {
                damageRouter = parentSection.ParentShip.DamageRouter;
            }

            // Fallback: search up the hierarchy
            if (damageRouter == null)
            {
                damageRouter = GetComponentInParent<DamageRouter>();
            }
        }
    }

    /// <summary>
    /// Sets the damage router reference.
    /// </summary>
    public void SetDamageRouter(DamageRouter router)
    {
        damageRouter = router;
    }

    /// <summary>
    /// Sets the parent section reference.
    /// </summary>
    /// <param name="section">The parent ShipSection.</param>
    public void SetParentSection(ShipSection section)
    {
        parentSection = section;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's a projectile
        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile == null)
        {
            projectile = other.GetComponentInParent<Projectile>();
        }

        if (projectile != null)
        {
            HandleProjectileHit(projectile);
        }
    }

    /// <summary>
    /// Handles a projectile hitting this section.
    /// Routes damage through DamageRouter for proper shields → armor → structure flow.
    /// </summary>
    /// <param name="projectile">The projectile that hit.</param>
    /// <returns>DamageReport with detailed damage distribution.</returns>
    public DamageReport HandleProjectileHit(Projectile projectile)
    {
        if (parentSection == null)
        {
            Debug.LogWarning($"[SectionHitDetector] Hit detected but no parent section assigned on {gameObject.name}");
            projectile.OnDestroyed();
            return new DamageReport();
        }

        // Don't process hits from our own ship's projectiles
        Ship ownerShip = parentSection.ParentShip;
        if (ownerShip != null && projectile.OwnerShip == ownerShip)
        {
            return new DamageReport();
        }

        float damage = projectile.Damage;
        Vector3 impactPoint = projectile.transform.position;

        if (logHits)
        {
            Debug.Log($"[SectionHitDetector] {parentSection.SectionType} hit by {projectile.GetType().Name} for {damage:F1} damage");
        }

        // Route damage through DamageRouter (shields → armor → structure)
        DamageReport report;
        if (damageRouter != null)
        {
            report = damageRouter.ProcessDamage(damage, parentSection.SectionType);
        }
        else
        {
            // Fallback: apply damage directly to section (bypasses shields)
            Debug.LogWarning($"[SectionHitDetector] No DamageRouter found, applying damage directly to section");
            DamageResult result = parentSection.ApplyDamage(damage);
            report = new DamageReport(
                totalIncoming: damage,
                shieldDamage: 0f,
                armorDamage: result.DamageToArmor,
                structureDamage: result.DamageToStructure,
                overflowDamage: result.OverflowDamage,
                shieldsDepleted: false,
                armorBroken: result.ArmorBroken,
                sectionBreached: result.SectionBreached,
                sectionHit: parentSection.SectionType,
                section: parentSection
            );
        }

        // Store for external access
        lastDamageReport = report;

        // Destroy the projectile after hit
        projectile.OnDestroyed();

        if (logHits)
        {
            Debug.Log($"[SectionHitDetector] {report}");
        }

        return report;
    }

    /// <summary>
    /// Manually apply damage to this section (for testing or special damage sources).
    /// Routes through DamageRouter for proper shields → armor → structure flow.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    /// <returns>DamageReport with detailed damage distribution.</returns>
    public DamageReport ApplyDamageViaRouter(float damage)
    {
        if (parentSection == null)
        {
            Debug.LogWarning($"[SectionHitDetector] Cannot apply damage - no parent section on {gameObject.name}");
            return new DamageReport();
        }

        DamageReport report;
        if (damageRouter != null)
        {
            report = damageRouter.ProcessDamage(damage, parentSection.SectionType);
        }
        else
        {
            // Fallback: apply damage directly to section
            DamageResult result = parentSection.ApplyDamage(damage);
            report = new DamageReport(
                totalIncoming: damage,
                shieldDamage: 0f,
                armorDamage: result.DamageToArmor,
                structureDamage: result.DamageToStructure,
                overflowDamage: result.OverflowDamage,
                shieldsDepleted: false,
                armorBroken: result.ArmorBroken,
                sectionBreached: result.SectionBreached,
                sectionHit: parentSection.SectionType,
                section: parentSection
            );
        }

        lastDamageReport = report;
        return report;
    }

    /// <summary>
    /// Apply damage directly to section (bypasses shields).
    /// Use ApplyDamageViaRouter for proper damage flow.
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    /// <returns>Result of direct section damage.</returns>
    public DamageResult ApplyDirectDamage(float damage)
    {
        if (parentSection == null)
        {
            Debug.LogWarning($"[SectionHitDetector] Cannot apply damage - no parent section on {gameObject.name}");
            return new DamageResult();
        }

        return parentSection.ApplyDamage(damage);
    }
}
