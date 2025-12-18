using UnityEngine;

/// <summary>
/// Detects projectile hits on a ship section collider.
/// Attach to section collider GameObjects. Auto-finds parent ShipSection.
/// </summary>
[RequireComponent(typeof(Collider))]
public class SectionHitDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipSection parentSection;

    [Header("Debug")]
    [SerializeField] private bool logHits = false;

    // Properties
    public ShipSection ParentSection => parentSection;

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
    /// </summary>
    /// <param name="projectile">The projectile that hit.</param>
    private void HandleProjectileHit(Projectile projectile)
    {
        if (parentSection == null)
        {
            Debug.LogWarning($"[SectionHitDetector] Hit detected but no parent section assigned on {gameObject.name}");
            return;
        }

        // Don't process hits from our own ship's projectiles
        Ship ownerShip = parentSection.ParentShip;
        if (ownerShip != null && projectile.OwnerShip == ownerShip)
        {
            return;
        }

        float damage = projectile.Damage;

        if (logHits)
        {
            Debug.Log($"[SectionHitDetector] {parentSection.SectionType} hit by projectile for {damage:F1} damage");
        }

        // Apply damage to section
        DamageResult result = parentSection.ApplyDamage(damage);

        // Destroy the projectile after hit
        projectile.OnDestroyed();

        if (logHits)
        {
            Debug.Log($"[SectionHitDetector] Damage result: {result}");
        }
    }

    /// <summary>
    /// Manually apply damage to this section (for testing or special damage sources).
    /// </summary>
    /// <param name="damage">Amount of damage to apply.</param>
    /// <returns>Result of damage application.</returns>
    public DamageResult ApplyDamage(float damage)
    {
        if (parentSection == null)
        {
            Debug.LogWarning($"[SectionHitDetector] Cannot apply damage - no parent section on {gameObject.name}");
            return new DamageResult();
        }

        return parentSection.ApplyDamage(damage);
    }
}
