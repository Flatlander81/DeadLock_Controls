using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Test controller for projectile damage integration.
/// Provides GUI and hotkeys for spawning projectiles and testing damage routing.
/// </summary>
public class ProjectileDamageTestController : MonoBehaviour
{
    [Header("Target Ship")]
    [SerializeField] private Ship targetShip;
    [SerializeField] private ShieldSystem targetShieldSystem;
    [SerializeField] private SectionManager targetSectionManager;
    [SerializeField] private DamageRouter targetDamageRouter;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistance = 20f;
    [SerializeField] private float projectileDamage = 50f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private SectionType targetSection = SectionType.Fore;

    [Header("Projectile Prefabs")]
    [SerializeField] private GameObject ballisticPrefab;
    [SerializeField] private GameObject homingPrefab;

    [Header("Damage Log")]
    [SerializeField] private int maxLogEntries = 10;
    private List<DamageLogEntry> damageLog = new List<DamageLogEntry>();

    [Header("Runtime Info")]
    [SerializeField] private float currentShields;
    [SerializeField] private float totalArmor;
    [SerializeField] private float totalStructure;

    private struct DamageLogEntry
    {
        public float timestamp;
        public string projectileType;
        public SectionType sectionHit;
        public float shieldDamage;
        public float armorDamage;
        public float structureDamage;
    }

    private void Start()
    {
        // Create default projectile prefabs if not assigned
        if (ballisticPrefab == null)
        {
            ballisticPrefab = CreateDefaultProjectilePrefab<BallisticProjectile>("BallisticProjectile");
        }

        if (homingPrefab == null)
        {
            homingPrefab = CreateDefaultProjectilePrefab<HomingProjectile>("HomingProjectile");
        }
    }

    private GameObject CreateDefaultProjectilePrefab<T>(string name) where T : Projectile
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        prefab.name = name;
        prefab.transform.localScale = Vector3.one * 0.3f;
        prefab.AddComponent<T>();

        // Make it a trigger
        Collider col = prefab.GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Add trail renderer
        TrailRenderer trail = prefab.AddComponent<TrailRenderer>();
        trail.startWidth = 0.2f;
        trail.endWidth = 0.05f;
        trail.time = 0.5f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = Color.yellow;
        trail.endColor = Color.clear;

        prefab.SetActive(false);
        return prefab;
    }

    private void Update()
    {
        UpdateRuntimeInfo();
    }

    /// <summary>
    /// Sets target references from a ship.
    /// </summary>
    public void SetTargetShip(Ship ship)
    {
        targetShip = ship;
        if (ship != null)
        {
            targetShieldSystem = ship.ShieldSystem;
            targetSectionManager = ship.SectionManager;
            targetDamageRouter = ship.DamageRouter;
        }
    }

    /// <summary>
    /// Spawns a ballistic projectile aimed at the target section.
    /// </summary>
    public void SpawnBallisticProjectile()
    {
        SpawnProjectile(ballisticPrefab, false);
    }

    /// <summary>
    /// Spawns a homing projectile aimed at the target ship.
    /// </summary>
    public void SpawnHomingProjectile()
    {
        SpawnProjectile(homingPrefab, true);
    }

    private void SpawnProjectile(GameObject prefab, bool isHoming)
    {
        if (targetShip == null || targetSectionManager == null)
        {
            Debug.LogError("[ProjectileDamageTestController] No target ship assigned!");
            return;
        }

        // Get target section position
        ShipSection section = targetSectionManager.GetSection(targetSection);
        if (section == null)
        {
            Debug.LogError($"[ProjectileDamageTestController] Section {targetSection} not found!");
            return;
        }

        Vector3 sectionPos = section.transform.position;

        // Calculate spawn position (opposite side of section from ship center)
        Vector3 dirFromCenter = (sectionPos - targetShip.transform.position).normalized;
        Vector3 spawnPos = sectionPos + dirFromCenter * spawnDistance;

        // Direction toward section
        Vector3 direction = (sectionPos - spawnPos).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        // Spawn projectile
        GameObject projectileObj = Instantiate(prefab, spawnPos, rotation);
        projectileObj.SetActive(true);

        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            WeaponSystem.ProjectileSpawnInfo info = new WeaponSystem.ProjectileSpawnInfo
            {
                SpawnPosition = spawnPos,
                SpawnRotation = rotation,
                Damage = projectileDamage,
                Speed = projectileSpeed,
                OwnerShip = null, // No owner (test projectile)
                TargetShip = isHoming ? targetShip : null
            };

            projectile.Initialize(info);

            Debug.Log($"[ProjectileDamageTestController] Spawned {(isHoming ? "Homing" : "Ballistic")} projectile targeting {targetSection}");
        }
    }

    /// <summary>
    /// Logs a damage report.
    /// </summary>
    public void LogDamageReport(DamageReport report, string projectileType)
    {
        DamageLogEntry entry = new DamageLogEntry
        {
            timestamp = Time.time,
            projectileType = projectileType,
            sectionHit = report.SectionHit,
            shieldDamage = report.ShieldDamage,
            armorDamage = report.ArmorDamage,
            structureDamage = report.StructureDamage
        };

        damageLog.Insert(0, entry);

        // Trim log
        while (damageLog.Count > maxLogEntries)
        {
            damageLog.RemoveAt(damageLog.Count - 1);
        }
    }

    /// <summary>
    /// Toggles shields on/off.
    /// </summary>
    public void ToggleShields()
    {
        if (targetShieldSystem == null) return;

        if (targetShieldSystem.IsShieldActive)
        {
            targetShieldSystem.SetShields(0f);
            Debug.Log("[ProjectileDamageTestController] Shields DISABLED");
        }
        else
        {
            targetShieldSystem.Reset();
            Debug.Log("[ProjectileDamageTestController] Shields ENABLED");
        }
    }

    /// <summary>
    /// Resets all ship systems.
    /// </summary>
    public void ResetAll()
    {
        if (targetShieldSystem != null)
        {
            targetShieldSystem.Reset();
        }

        if (targetSectionManager != null)
        {
            targetSectionManager.ResetAllSections();
        }

        damageLog.Clear();
        Debug.Log("[ProjectileDamageTestController] All systems reset");
    }

    private void UpdateRuntimeInfo()
    {
        if (targetShieldSystem != null)
        {
            currentShields = targetShieldSystem.CurrentShields;
        }

        if (targetSectionManager != null)
        {
            totalArmor = targetSectionManager.GetTotalArmorRemaining();
            totalStructure = targetSectionManager.GetTotalStructureRemaining();
        }
    }

    private void OnGUI()
    {
        if (targetShip == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 400, 600));
        GUILayout.Label("Projectile Damage Test Controller", GUI.skin.box);

        // Ship status
        GUILayout.Space(5);
        GUILayout.Label("=== SHIP STATUS ===");

        if (targetShieldSystem != null)
        {
            float shieldPct = targetShieldSystem.GetShieldPercentage() * 100f;
            string shieldStatus = targetShieldSystem.IsShieldActive ? "ACTIVE" : "DEPLETED";
            GUILayout.Label($"Shields: {targetShieldSystem.CurrentShields:F0}/{targetShieldSystem.MaxShields:F0} ({shieldPct:F0}%) - {shieldStatus}");

            // Shield bar
            Rect barRect = GUILayoutUtility.GetRect(380, 20);
            GUI.color = targetShieldSystem.IsShieldActive ? Color.cyan : Color.gray;
            GUI.Box(new Rect(barRect.x, barRect.y, 380 * targetShieldSystem.GetShieldPercentage(), 20), "");
            GUI.color = Color.white;
        }

        if (targetSectionManager != null)
        {
            GUILayout.Label($"Total Armor: {totalArmor:F0}");
            GUILayout.Label($"Total Structure: {totalStructure:F0}");
            GUILayout.Label($"Breached Sections: {targetSectionManager.GetBreachedSections().Count}/{targetSectionManager.SectionCount}");
        }

        // Spawn settings
        GUILayout.Space(10);
        GUILayout.Label("=== SPAWN SETTINGS ===");

        GUILayout.Label($"Damage: {projectileDamage:F0}");
        projectileDamage = GUILayout.HorizontalSlider(projectileDamage, 10f, 300f);

        GUILayout.Label($"Speed: {projectileSpeed:F0}");
        projectileSpeed = GUILayout.HorizontalSlider(projectileSpeed, 5f, 50f);

        GUILayout.Label($"Target Section: {targetSection}");

        // Section buttons
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fore")) targetSection = SectionType.Fore;
        if (GUILayout.Button("Aft")) targetSection = SectionType.Aft;
        if (GUILayout.Button("Port")) targetSection = SectionType.Port;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Starboard")) targetSection = SectionType.Starboard;
        if (GUILayout.Button("Dorsal")) targetSection = SectionType.Dorsal;
        if (GUILayout.Button("Ventral")) targetSection = SectionType.Ventral;
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Core")) targetSection = SectionType.Core;

        // Action buttons
        GUILayout.Space(10);
        GUILayout.Label("=== ACTIONS ===");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Fire Ballistic [1]"))
        {
            SpawnBallisticProjectile();
        }
        if (GUILayout.Button("Fire Homing [2]"))
        {
            SpawnHomingProjectile();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Shields [S]"))
        {
            ToggleShields();
        }
        if (GUILayout.Button("Reset All [R]"))
        {
            ResetAll();
        }
        GUILayout.EndHorizontal();

        // Damage log
        GUILayout.Space(10);
        GUILayout.Label("=== DAMAGE LOG ===");

        foreach (var entry in damageLog)
        {
            string logLine = $"[{entry.timestamp:F1}s] {entry.projectileType} → {entry.sectionHit}: " +
                            $"S:{entry.shieldDamage:F0} A:{entry.armorDamage:F0} H:{entry.structureDamage:F0}";
            GUILayout.Label(logLine);
        }

        GUILayout.EndArea();

        // Keyboard shortcuts
        if (Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Alpha1:
                    SpawnBallisticProjectile();
                    break;
                case KeyCode.Alpha2:
                    SpawnHomingProjectile();
                    break;
                case KeyCode.S:
                    ToggleShields();
                    break;
                case KeyCode.R:
                    ResetAll();
                    break;
            }
        }
    }
}
