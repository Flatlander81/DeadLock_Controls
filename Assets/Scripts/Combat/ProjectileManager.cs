using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all projectiles in the game.
/// Handles spawning, pooling, and cleanup of projectiles.
/// Called by Track A (Weapon System) to spawn projectiles.
/// </summary>
public class ProjectileManager : MonoBehaviour
{
    private static ProjectileManager instance;

    [Header("Prefab References")]
    [SerializeField] private GameObject ballisticProjectilePrefab;
    [SerializeField] private GameObject homingProjectilePrefab;
    [SerializeField] private GameObject instantHitEffectPrefab;

    [Header("Pooling Settings")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private bool allowPoolGrowth = true;

    [Header("Runtime State")]
    [SerializeField] private List<Projectile> activeProjectiles = new List<Projectile>();
    [SerializeField] private List<InstantHitEffect> activeEffects = new List<InstantHitEffect>();

    // Pools for each projectile type
    private Queue<BallisticProjectile> ballisticPool = new Queue<BallisticProjectile>();
    private Queue<HomingProjectile> homingPool = new Queue<HomingProjectile>();
    private Queue<InstantHitEffect> instantHitPool = new Queue<InstantHitEffect>();

    // Parent transforms for organization
    private Transform projectileParent;
    private Transform effectParent;

    // Singleton accessor
    public static ProjectileManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ProjectileManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ProjectileManager");
                    instance = go.AddComponent<ProjectileManager>();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Initialize pools and setup.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // Create parent transforms for organization
        projectileParent = new GameObject("ActiveProjectiles").transform;
        projectileParent.SetParent(transform);

        effectParent = new GameObject("ActiveEffects").transform;
        effectParent.SetParent(transform);

        // Initialize pools
        InitializePools();
    }

    /// <summary>
    /// Pre-populate pools with projectiles.
    /// </summary>
    private void InitializePools()
    {
        // Only initialize if prefabs are assigned
        if (ballisticProjectilePrefab != null)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePooledBallistic();
            }
            Debug.Log($"Initialized ballistic projectile pool with {initialPoolSize} projectiles");
        }

        if (homingProjectilePrefab != null)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePooledHoming();
            }
            Debug.Log($"Initialized homing projectile pool with {initialPoolSize} projectiles");
        }

        if (instantHitEffectPrefab != null)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreatePooledInstantHit();
            }
            Debug.Log($"Initialized instant hit effect pool with {initialPoolSize} effects");
        }
    }

    /// <summary>
    /// Create a pooled ballistic projectile.
    /// </summary>
    private BallisticProjectile CreatePooledBallistic()
    {
        GameObject obj;
        if (ballisticProjectilePrefab != null)
        {
            obj = Instantiate(ballisticProjectilePrefab, projectileParent);
        }
        else
        {
            // Create default if no prefab
            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.localScale = Vector3.one * 0.3f;
            obj.AddComponent<BallisticProjectile>();
            obj.transform.SetParent(projectileParent);
        }

        obj.name = "BallisticProjectile (Pooled)";
        BallisticProjectile projectile = obj.GetComponent<BallisticProjectile>();
        if (projectile == null)
        {
            projectile = obj.AddComponent<BallisticProjectile>();
        }

        obj.SetActive(false);
        ballisticPool.Enqueue(projectile);
        return projectile;
    }

    /// <summary>
    /// Create a pooled homing projectile.
    /// </summary>
    private HomingProjectile CreatePooledHoming()
    {
        GameObject obj;
        if (homingProjectilePrefab != null)
        {
            obj = Instantiate(homingProjectilePrefab, projectileParent);
        }
        else
        {
            // Create default if no prefab
            obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
            obj.AddComponent<HomingProjectile>();
            obj.transform.SetParent(projectileParent);
        }

        obj.name = "HomingProjectile (Pooled)";
        HomingProjectile projectile = obj.GetComponent<HomingProjectile>();
        if (projectile == null)
        {
            projectile = obj.AddComponent<HomingProjectile>();
        }

        obj.SetActive(false);
        homingPool.Enqueue(projectile);
        return projectile;
    }

    /// <summary>
    /// Create a pooled instant hit effect.
    /// </summary>
    private InstantHitEffect CreatePooledInstantHit()
    {
        GameObject obj;
        if (instantHitEffectPrefab != null)
        {
            obj = Instantiate(instantHitEffectPrefab, effectParent);
        }
        else
        {
            // Create default if no prefab
            obj = new GameObject("InstantHitEffect");
            obj.AddComponent<InstantHitEffect>();
            obj.transform.SetParent(effectParent);
        }

        obj.name = "InstantHitEffect (Pooled)";
        InstantHitEffect effect = obj.GetComponent<InstantHitEffect>();
        if (effect == null)
        {
            effect = obj.AddComponent<InstantHitEffect>();
        }

        obj.SetActive(false);
        instantHitPool.Enqueue(effect);
        return effect;
    }

    /// <summary>
    /// Spawn a ballistic projectile from weapon.
    /// </summary>
    public static void SpawnBallisticProjectile(WeaponSystem.ProjectileSpawnInfo info)
    {
        BallisticProjectile projectile = Instance.GetBallisticFromPool();

        if (projectile == null)
        {
            Debug.LogError("Failed to get ballistic projectile from pool!");
            return;
        }

        projectile.gameObject.SetActive(true);
        projectile.Initialize(info);
        Instance.activeProjectiles.Add(projectile);

        Debug.Log($"Spawned ballistic projectile at {info.SpawnPosition}");
    }

    /// <summary>
    /// Spawn a homing projectile from weapon.
    /// </summary>
    /// <param name="info">Projectile spawn configuration</param>
    /// <param name="turnRate">Turn rate in degrees per second (default 90)</param>
    public static void SpawnHomingProjectile(WeaponSystem.ProjectileSpawnInfo info, float turnRate = 90f)
    {
        HomingProjectile projectile = Instance.GetHomingFromPool();

        if (projectile == null)
        {
            Debug.LogError("Failed to get homing projectile from pool!");
            return;
        }

        projectile.gameObject.SetActive(true);
        projectile.Initialize(info);
        projectile.SetTurnRate(turnRate);
        Instance.activeProjectiles.Add(projectile);

        Debug.Log($"Spawned homing projectile at {info.SpawnPosition} targeting {info.TargetShip?.gameObject.name} (turnRate={turnRate})");
    }

    /// <summary>
    /// Spawn instant hit effect (for rail guns).
    /// </summary>
    public static void SpawnInstantHitEffect(Vector3 startPosition, Vector3 endPosition, float damage)
    {
        InstantHitEffect effect = Instance.GetInstantHitFromPool();

        if (effect == null)
        {
            Debug.LogError("Failed to get instant hit effect from pool!");
            return;
        }

        effect.gameObject.SetActive(true);
        effect.Initialize(startPosition, endPosition);
        Instance.activeEffects.Add(effect);

        Debug.Log($"Spawned instant hit effect from {startPosition} to {endPosition}");
    }

    /// <summary>
    /// Get ballistic projectile from pool.
    /// </summary>
    private BallisticProjectile GetBallisticFromPool()
    {
        if (ballisticPool.Count == 0)
        {
            if (allowPoolGrowth)
            {
                Debug.LogWarning("Ballistic pool empty, creating new projectile");
                return CreatePooledBallistic();
            }
            else
            {
                Debug.LogError("Ballistic pool exhausted and growth disabled!");
                return null;
            }
        }

        return ballisticPool.Dequeue();
    }

    /// <summary>
    /// Get homing projectile from pool.
    /// </summary>
    private HomingProjectile GetHomingFromPool()
    {
        if (homingPool.Count == 0)
        {
            if (allowPoolGrowth)
            {
                Debug.LogWarning("Homing pool empty, creating new projectile");
                return CreatePooledHoming();
            }
            else
            {
                Debug.LogError("Homing pool exhausted and growth disabled!");
                return null;
            }
        }

        return homingPool.Dequeue();
    }

    /// <summary>
    /// Get instant hit effect from pool.
    /// </summary>
    private InstantHitEffect GetInstantHitFromPool()
    {
        if (instantHitPool.Count == 0)
        {
            if (allowPoolGrowth)
            {
                Debug.LogWarning("Instant hit pool empty, creating new effect");
                return CreatePooledInstantHit();
            }
            else
            {
                Debug.LogError("Instant hit pool exhausted and growth disabled!");
                return null;
            }
        }

        return instantHitPool.Dequeue();
    }

    /// <summary>
    /// Return projectile to pool.
    /// Called by Projectile.OnDestroyed().
    /// </summary>
    public static void ReturnToPool(Projectile projectile)
    {
        if (projectile == null) return;

        // Remove from active list
        Instance.activeProjectiles.Remove(projectile);

        // Reset and return to appropriate pool
        if (projectile is BallisticProjectile ballistic)
        {
            ballistic.ResetToPool();
            Instance.ballisticPool.Enqueue(ballistic);
        }
        else if (projectile is HomingProjectile homing)
        {
            homing.ResetToPool();
            Instance.homingPool.Enqueue(homing);
        }
    }

    /// <summary>
    /// Return instant hit effect to pool.
    /// </summary>
    public static void ReturnInstantHitToPool(InstantHitEffect effect)
    {
        if (effect == null) return;

        Instance.activeEffects.Remove(effect);
        effect.ResetToPool();
        Instance.instantHitPool.Enqueue(effect);
    }

    /// <summary>
    /// Clear all active projectiles (combat end, scene change).
    /// </summary>
    public static void ClearAllProjectiles()
    {
        // Return all active projectiles to pool
        while (Instance.activeProjectiles.Count > 0)
        {
            Projectile proj = Instance.activeProjectiles[0];
            ReturnToPool(proj);
        }

        // Return all active effects to pool
        while (Instance.activeEffects.Count > 0)
        {
            InstantHitEffect effect = Instance.activeEffects[0];
            ReturnInstantHitToPool(effect);
        }

        Debug.Log("All projectiles cleared");
    }

    /// <summary>
    /// Get count of active projectiles.
    /// </summary>
    public static int GetActiveProjectileCount()
    {
        return Instance.activeProjectiles.Count;
    }

    /// <summary>
    /// Get count of pooled projectiles (available for reuse).
    /// </summary>
    public static int GetPooledProjectileCount()
    {
        return Instance.ballisticPool.Count + Instance.homingPool.Count;
    }

    /// <summary>
    /// Debug: Get pool status.
    /// </summary>
    public static string GetPoolStatus()
    {
        return $"Active: {Instance.activeProjectiles.Count}, " +
               $"Ballistic Pool: {Instance.ballisticPool.Count}, " +
               $"Homing Pool: {Instance.homingPool.Count}, " +
               $"Instant Hit Pool: {Instance.instantHitPool.Count}";
    }

    /// <summary>
    /// Reset the singleton instance. Used for testing to ensure clean state.
    /// Destroys the current instance and clears the static reference.
    /// </summary>
    public static void ResetInstance()
    {
        if (instance != null)
        {
            // Clear all active projectiles first
            instance.activeProjectiles.Clear();
            instance.activeEffects.Clear();
            instance.ballisticPool.Clear();
            instance.homingPool.Clear();
            instance.instantHitPool.Clear();

            // Destroy the game object
            if (Application.isPlaying)
            {
                Destroy(instance.gameObject);
            }
            else
            {
                DestroyImmediate(instance.gameObject);
            }

            instance = null;
        }
    }
}
