using UnityEngine;
using System.Collections;

/// <summary>
/// Testing script for Track B (Projectile System).
/// Provides keyboard controls to spawn and test projectiles.
/// </summary>
public class ProjectileTester : MonoBehaviour
{
    [Header("Ship References")]
    [SerializeField] private Ship playerShip;
    [SerializeField] private Ship enemyShip1;
    [SerializeField] private Ship enemyShip2;

    [Header("Test Controls")]
    [SerializeField] private KeyCode fireRailGunsKey = KeyCode.Space;
    [SerializeField] private KeyCode fireCannonKey = KeyCode.F;
    [SerializeField] private KeyCode spawnTestBallisticKey = KeyCode.P;
    [SerializeField] private KeyCode spawnTestHomingKey = KeyCode.H;
    [SerializeField] private KeyCode poolInfoKey = KeyCode.I;
    [SerializeField] private KeyCode clearProjectilesKey = KeyCode.C;
    [SerializeField] private KeyCode switchTargetKey = KeyCode.T;

    [Header("Manual Spawn Settings")]
    [SerializeField] private float testProjectileSpeed = 5f;
    [SerializeField] private float testProjectileDamage = 25f;

    private Ship currentTarget;
    private bool initialized = false;

    void Start()
    {
        StartCoroutine(InitializeAfterDelay());
    }

    private IEnumerator InitializeAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        if (playerShip == null || enemyShip1 == null)
        {
            Debug.LogError("ProjectileTester: Ships not assigned!");
            yield break;
        }

        // Set initial target
        currentTarget = enemyShip1;

        // Assign targets to weapons
        if (playerShip.WeaponManager != null)
        {
            var weapons = playerShip.WeaponManager.Weapons;
            foreach (var weapon in weapons)
            {
                weapon.SetTarget(currentTarget);
            }

            // Assign to groups
            if (weapons.Count > 0) playerShip.WeaponManager.AssignWeaponToGroup(weapons[0], 1);
            if (weapons.Count > 1) playerShip.WeaponManager.AssignWeaponToGroup(weapons[1], 1);
            if (weapons.Count > 2) playerShip.WeaponManager.AssignWeaponToGroup(weapons[2], 2);
        }

        initialized = true;
        PrintInstructions();
    }

    void Update()
    {
        if (!initialized) return;

        HandleWeaponFiring();
        HandleManualProjectileSpawn();
        HandleProjectileInfo();
        HandleTargetSwitch();
    }

    private void HandleWeaponFiring()
    {
        if (playerShip == null || playerShip.WeaponManager == null) return;

        // Fire RailGuns (Group 1) - Instant Hit Effects
        if (Input.GetKeyDown(fireRailGunsKey))
        {
            Debug.Log("[ProjectileTester] Firing RailGuns (instant hit beams)...");
            StartCoroutine(playerShip.WeaponManager.FireGroup(1));
        }

        // Fire Cannon (Group 2) - Ballistic Projectile
        if (Input.GetKeyDown(fireCannonKey))
        {
            Debug.Log("[ProjectileTester] Firing Cannon (ballistic projectile)...");
            StartCoroutine(playerShip.WeaponManager.FireGroup(2));
        }
    }

    private void HandleManualProjectileSpawn()
    {
        // Spawn test ballistic projectile
        if (Input.GetKeyDown(spawnTestBallisticKey))
        {
            SpawnTestBallistic();
        }

        // Spawn test homing projectile
        if (Input.GetKeyDown(spawnTestHomingKey))
        {
            SpawnTestHoming();
        }

        // Clear all projectiles
        if (Input.GetKeyDown(clearProjectilesKey))
        {
            Debug.Log("[ProjectileTester] Clearing all projectiles...");
            ProjectileManager.ClearAllProjectiles();
            Debug.Log($"Pool status: {ProjectileManager.GetPoolStatus()}");
        }
    }

    private void HandleProjectileInfo()
    {
        if (Input.GetKeyDown(poolInfoKey))
        {
            PrintPoolInfo();
        }
    }

    private void HandleTargetSwitch()
    {
        if (Input.GetKeyDown(switchTargetKey))
        {
            // Switch between enemy 1 and enemy 2
            if (enemyShip2 != null)
            {
                currentTarget = (currentTarget == enemyShip1) ? enemyShip2 : enemyShip1;

                Debug.Log($"[ProjectileTester] Target switched to {currentTarget.gameObject.name}");

                // Update weapon targets
                if (playerShip.WeaponManager != null)
                {
                    foreach (var weapon in playerShip.WeaponManager.Weapons)
                    {
                        weapon.SetTarget(currentTarget);
                    }
                }
            }
        }
    }

    private void SpawnTestBallistic()
    {
        Debug.Log("[ProjectileTester] Spawning test ballistic projectile...");

        Vector3 spawnPos = playerShip.transform.position + playerShip.transform.forward * 2f;
        Vector3 targetPos = currentTarget != null ? currentTarget.transform.position : spawnPos + Vector3.forward * 20f;

        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Ballistic,
            SpawnPosition = spawnPos,
            SpawnRotation = Quaternion.LookRotation((targetPos - spawnPos).normalized),
            TargetPosition = targetPos,
            TargetShip = currentTarget,
            Damage = testProjectileDamage,
            Speed = testProjectileSpeed,
            OwnerShip = playerShip
        };

        ProjectileManager.SpawnBallisticProjectile(info);

        Debug.Log($"  Spawned at: {spawnPos}");
        Debug.Log($"  Target: {targetPos}");
        Debug.Log($"  Speed: {testProjectileSpeed} units/sec");
        Debug.Log($"  Damage: {testProjectileDamage}");
    }

    private void SpawnTestHoming()
    {
        if (currentTarget == null)
        {
            Debug.LogWarning("[ProjectileTester] No target assigned for homing missile!");
            return;
        }

        Debug.Log($"[ProjectileTester] Spawning test homing missile at {currentTarget.gameObject.name}...");

        Vector3 spawnPos = playerShip.transform.position + playerShip.transform.forward * 2f;

        var info = new WeaponSystem.ProjectileSpawnInfo
        {
            Type = WeaponSystem.ProjectileSpawnInfo.ProjectileType.Homing,
            SpawnPosition = spawnPos,
            SpawnRotation = Quaternion.LookRotation((currentTarget.transform.position - spawnPos).normalized),
            TargetPosition = currentTarget.transform.position,
            TargetShip = currentTarget,
            Damage = testProjectileDamage * 1.5f, // Homing does more damage
            Speed = testProjectileSpeed * 0.8f, // Slower but seeks
            OwnerShip = playerShip
        };

        ProjectileManager.SpawnHomingProjectile(info);

        Debug.Log($"  Spawned at: {spawnPos}");
        Debug.Log($"  Tracking: {currentTarget.gameObject.name}");
        Debug.Log($"  Speed: {info.Speed} units/sec");
        Debug.Log($"  Damage: {info.Damage}");
        Debug.Log($"  Turn Rate: 90°/sec");
    }

    private void PrintPoolInfo()
    {
        Debug.Log("========================================");
        Debug.Log("PROJECTILE POOL STATUS");
        Debug.Log("========================================");
        Debug.Log(ProjectileManager.GetPoolStatus());
        Debug.Log($"Active Projectiles: {ProjectileManager.GetActiveProjectileCount()}");
        Debug.Log($"Pooled (Available): {ProjectileManager.GetPooledProjectileCount()}");
        Debug.Log("========================================");

        // Find all active projectiles in scene
        Projectile[] activeProj = Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None);
        Debug.Log($"Scene projectiles: {activeProj.Length}");
        foreach (var proj in activeProj)
        {
            if (proj.IsActive)
            {
                Debug.Log($"  - {proj.GetType().Name} at {proj.transform.position} (Age: {proj.CurrentAge:F1}s)");
            }
        }

        InstantHitEffect[] activeEffects = Object.FindObjectsByType<InstantHitEffect>(FindObjectsSortMode.None);
        Debug.Log($"Scene effects: {activeEffects.Length}");
        foreach (var effect in activeEffects)
        {
            if (effect.IsActive)
            {
                Debug.Log($"  - InstantHitEffect (Age: {effect.CurrentAge:F1}s)");
            }
        }
    }

    private void PrintInstructions()
    {
        Debug.Log("========================================");
        Debug.Log("PROJECTILE TEST CONTROLS");
        Debug.Log("========================================");
        Debug.Log("WEAPON FIRING:");
        Debug.Log("  SPACE - Fire RailGuns (instant hit beams)");
        Debug.Log("  F     - Fire Cannon (ballistic projectile)");
        Debug.Log("");
        Debug.Log("MANUAL SPAWN:");
        Debug.Log("  P - Spawn test ballistic projectile");
        Debug.Log("  H - Spawn homing missile at target");
        Debug.Log("  T - Switch target (Enemy1 <-> Enemy2)");
        Debug.Log("");
        Debug.Log("DEBUG:");
        Debug.Log("  I - Print pool info and active projectiles");
        Debug.Log("  C - Clear all projectiles (return to pool)");
        Debug.Log("");
        Debug.Log("WATCH FOR:");
        Debug.Log("  - Cyan beam tracers (RailGun instant hit)");
        Debug.Log("  - Cyan spheres flying (Cannon ballistic)");
        Debug.Log("  - Yellow capsules seeking (Homing missiles)");
        Debug.Log("  - Trail renderers behind projectiles");
        Debug.Log("========================================");
        Debug.Log($"Current Target: {currentTarget.gameObject.name}");
        Debug.Log($"Player Heat: {playerShip.HeatManager.CurrentHeat}");
        Debug.Log("========================================");
    }

    void OnGUI()
    {
        if (!initialized) return;

        // Simple on-screen display
        GUI.Box(new Rect(10, 10, 320, 280), "Projectile Test Controls");

        int y = 35;
        GUI.Label(new Rect(20, y, 300, 20), "=== WEAPON FIRING ==="); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), "SPACE - Fire RailGuns (beams)"); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), "F - Fire Cannon (ballistic)"); y += 20;
        y += 10;

        GUI.Label(new Rect(20, y, 300, 20), "=== MANUAL SPAWN ==="); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), "P - Spawn Ballistic"); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), "H - Spawn Homing Missile"); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), "T - Switch Target"); y += 20;
        y += 10;

        GUI.Label(new Rect(20, y, 300, 20), "=== DEBUG ==="); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), "I - Pool Info"); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), "C - Clear All Projectiles"); y += 20;
        y += 10;

        GUI.Label(new Rect(20, y, 300, 20), "=== STATUS ==="); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), $"Target: {currentTarget.gameObject.name}"); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), $"Active Projectiles: {ProjectileManager.GetActiveProjectileCount()}"); y += 20;
        GUI.Label(new Rect(20, y, 300, 20), $"Player Heat: {playerShip.HeatManager.CurrentHeat:F0}"); y += 20;
    }
}
