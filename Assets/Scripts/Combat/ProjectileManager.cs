using UnityEngine;
using System.Collections.Generic;
using System;

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
    [SerializeField] private int maxPoolSize = 100;

    [Header("Default Projectile Settings")]
    [SerializeField] private Vector3 defaultBallisticScale = new Vector3(0.3f, 0.3f, 0.3f);
    [SerializeField] private Vector3 defaultHomingScale = new Vector3(0.2f, 0.5f, 0.2f);

    [Header("Runtime State")]
    [SerializeField] private List<Projectile> activeProjectiles = new List<Projectile>();
    [SerializeField] private List<InstantHitEffect> activeEffects = new List<InstantHitEffect>();

    // Pools for each projectile type
    private Queue<BallisticProjectile> ballisticPool = new Queue<BallisticProjectile>();
    private Queue<HomingProjectile> homingPool = new Queue<HomingProjectile>();
    private Queue<InstantHitEffect> instantHitPool = new Queue<InstantHitEffect>();

    // Active projectile counts by type (avoid FindAll allocations)
    private int activeBallisticCount = 0;
    private int activeHomingCount = 0;

    // Parent transforms for organization
    private Transform projectileParent;
    private Transform effectParent;

    // Thread-safety lock for singleton
    private static readonly object instanceLock = new object();

    // Singleton accessor with thread-safe double-checked locking
    public static ProjectileManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (instanceLock)
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
        InitializePool(ballisticProjectilePrefab, "ballistic projectile",
            () => CreatePooledObject<BallisticProjectile>(
                ballisticProjectilePrefab, PrimitiveType.Sphere, defaultBallisticScale,
                "BallisticProjectile", projectileParent, ballisticPool));

        InitializePool(homingProjectilePrefab, "homing projectile",
            () => CreatePooledObject<HomingProjectile>(
                homingProjectilePrefab, PrimitiveType.Capsule, defaultHomingScale,
                "HomingProjectile", projectileParent, homingPool));

        InitializePool(instantHitEffectPrefab, "instant hit effect",
            () => CreatePooledEffect(instantHitEffectPrefab, "InstantHitEffect", effectParent, instantHitPool));
    }

    /// <summary>
    /// Generic pool initialization helper.
    /// </summary>
    private void InitializePool(GameObject prefab, string typeName, Action createAction)
    {
        if (prefab != null || allowPoolGrowth)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                createAction();
            }
            Debug.Log($"Initialized {typeName} pool with {initialPoolSize} objects");
        }
    }

    /// <summary>
    /// Generic pooled object creation for projectiles.
    /// </summary>
    private T CreatePooledObject<T>(GameObject prefab, PrimitiveType fallbackPrimitive,
        Vector3 fallbackScale, string objectName, Transform parent, Queue<T> pool) where T : Component
    {
        GameObject obj;
        if (prefab != null)
        {
            obj = Instantiate(prefab, parent);
        }
        else
        {
            // Create default primitive if no prefab
            obj = GameObject.CreatePrimitive(fallbackPrimitive);
            obj.transform.localScale = fallbackScale;
            obj.transform.SetParent(parent);
        }

        obj.name = $"{objectName} (Pooled)";

        // Get or add the component
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            component = obj.AddComponent<T>();
        }

        obj.SetActive(false);
        pool.Enqueue(component);
        return component;
    }

    /// <summary>
    /// Create pooled effect (non-primitive fallback).
    /// </summary>
    private InstantHitEffect CreatePooledEffect(GameObject prefab, string objectName,
        Transform parent, Queue<InstantHitEffect> pool)
    {
        GameObject obj;
        if (prefab != null)
        {
            obj = Instantiate(prefab, parent);
        }
        else
        {
            obj = new GameObject(objectName);
            obj.transform.SetParent(parent);
        }

        obj.name = $"{objectName} (Pooled)";

        InstantHitEffect effect = obj.GetComponent<InstantHitEffect>();
        if (effect == null)
        {
            effect = obj.AddComponent<InstantHitEffect>();
        }

        obj.SetActive(false);
        pool.Enqueue(effect);
        return effect;
    }

    /// <summary>
    /// Generic method to get object from pool with growth and limit checking.
    /// </summary>
    private T GetFromPool<T>(Queue<T> pool, string typeName, Func<T> createFunc,
        Func<int> activeCountFunc) where T : class
    {
        if (pool.Count == 0)
        {
            if (allowPoolGrowth)
            {
                // Check pool size limit to prevent memory leaks
                int totalCount = pool.Count + activeCountFunc();
                if (totalCount >= maxPoolSize)
                {
                    Debug.LogError($"{typeName} pool at max size ({maxPoolSize})! Cannot create more.");
                    return null;
                }

                Debug.LogWarning($"{typeName} pool empty, creating new object");
                try
                {
                    return createFunc();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create {typeName}: {e.Message}");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"{typeName} pool exhausted and growth disabled!");
                return null;
            }
        }

        return pool.Dequeue();
    }

    /// <summary>
    /// Spawn a ballistic projectile from weapon.
    /// </summary>
    public static void SpawnBallisticProjectile(WeaponSystem.ProjectileSpawnInfo info)
    {
        BallisticProjectile projectile = Instance.GetFromPool(
            Instance.ballisticPool,
            "Ballistic",
            () => Instance.CreatePooledObject<BallisticProjectile>(
                Instance.ballisticProjectilePrefab, PrimitiveType.Sphere,
                Instance.defaultBallisticScale, "BallisticProjectile",
                Instance.projectileParent, Instance.ballisticPool),
            () => Instance.activeBallisticCount); // O(1) instead of O(n) FindAll

        if (projectile == null)
        {
            Debug.LogError("Failed to get ballistic projectile from pool!");
            return;
        }

        projectile.gameObject.SetActive(true);
        projectile.Initialize(info);
        Instance.activeProjectiles.Add(projectile);
        Instance.activeBallisticCount++;

        Debug.Log($"Spawned ballistic projectile at {info.SpawnPosition}");
    }

    /// <summary>
    /// Spawn a homing projectile from weapon.
    /// </summary>
    public static void SpawnHomingProjectile(WeaponSystem.ProjectileSpawnInfo info, float turnRate = 90f)
    {
        HomingProjectile projectile = Instance.GetFromPool(
            Instance.homingPool,
            "Homing",
            () => Instance.CreatePooledObject<HomingProjectile>(
                Instance.homingProjectilePrefab, PrimitiveType.Capsule,
                Instance.defaultHomingScale, "HomingProjectile",
                Instance.projectileParent, Instance.homingPool),
            () => Instance.activeHomingCount); // O(1) instead of O(n) FindAll

        if (projectile == null)
        {
            Debug.LogError("Failed to get homing projectile from pool!");
            return;
        }

        projectile.gameObject.SetActive(true);
        projectile.Initialize(info);
        projectile.SetTurnRate(turnRate);
        Instance.activeProjectiles.Add(projectile);
        Instance.activeHomingCount++;

        Debug.Log($"Spawned homing projectile at {info.SpawnPosition} targeting {info.TargetShip?.gameObject.name} (turnRate={turnRate})");
    }

    /// <summary>
    /// Spawn instant hit effect (for rail guns).
    /// </summary>
    public static void SpawnInstantHitEffect(Vector3 startPosition, Vector3 endPosition, float damage)
    {
        InstantHitEffect effect = Instance.GetFromPool(
            Instance.instantHitPool,
            "InstantHit",
            () => Instance.CreatePooledEffect(
                Instance.instantHitEffectPrefab, "InstantHitEffect",
                Instance.effectParent, Instance.instantHitPool),
            () => Instance.activeEffects.Count);

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
    /// Return projectile to pool.
    /// Called by Projectile.OnDestroyed().
    /// </summary>
    public static void ReturnToPool(Projectile projectile)
    {
        if (projectile == null) return;

        // Remove from active list
        Instance.activeProjectiles.Remove(projectile);

        // Reset and return to appropriate pool, decrement type counts
        projectile.ResetToPool();

        if (projectile is BallisticProjectile ballistic)
        {
            Instance.ballisticPool.Enqueue(ballistic);
            Instance.activeBallisticCount--;
        }
        else if (projectile is HomingProjectile homing)
        {
            Instance.homingPool.Enqueue(homing);
            Instance.activeHomingCount--;
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

        // Reset type counts (should already be 0, but ensure consistency)
        Instance.activeBallisticCount = 0;
        Instance.activeHomingCount = 0;

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

            // Reset type counts
            instance.activeBallisticCount = 0;
            instance.activeHomingCount = 0;

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
