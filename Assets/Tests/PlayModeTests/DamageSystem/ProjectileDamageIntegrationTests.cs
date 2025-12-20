using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Integration tests for projectile damage routing through shields and sections.
/// Tests that projectiles correctly route damage through DamageRouter.
/// </summary>
public class ProjectileDamageIntegrationTests
{
    private GameObject shipObject;
    private Ship ship;
    private ShieldSystem shieldSystem;
    private SectionManager sectionManager;
    private DamageRouter damageRouter;
    private Dictionary<SectionType, ShipSection> sections;
    private Dictionary<SectionType, SectionHitDetector> hitDetectors;

    [SetUp]
    public void Setup()
    {
        // Create test ship
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();
        shipObject.AddComponent<HeatManager>();

        // Add ShieldSystem
        shieldSystem = shipObject.AddComponent<ShieldSystem>();
        shieldSystem.Initialize(200f);

        // Add SectionManager
        sectionManager = shipObject.AddComponent<SectionManager>();

        // Add DamageRouter
        damageRouter = shipObject.AddComponent<DamageRouter>();
        damageRouter.SetReferences(shieldSystem, sectionManager, ship);

        // Create sections container
        GameObject sectionsContainer = new GameObject("Sections");
        sectionsContainer.transform.SetParent(shipObject.transform);

        // Create sections and hit detectors
        sections = new Dictionary<SectionType, ShipSection>();
        hitDetectors = new Dictionary<SectionType, SectionHitDetector>();

        foreach (SectionType sectionType in SectionDefinitions.GetAllSectionTypes())
        {
            var config = SectionDefinitions.GetConfig(sectionType);

            GameObject sectionObj = new GameObject($"Section_{sectionType}");
            sectionObj.transform.SetParent(sectionsContainer.transform);
            sectionObj.transform.localPosition = config.ColliderPosition;

            ShipSection section = sectionObj.AddComponent<ShipSection>();
            section.Initialize(sectionType, ship);
            sections[sectionType] = section;

            // Add collider
            BoxCollider boxCol = sectionObj.AddComponent<BoxCollider>();
            boxCol.size = config.ColliderSize;
            boxCol.isTrigger = true;

            // Add hit detector
            SectionHitDetector hitDetector = sectionObj.AddComponent<SectionHitDetector>();
            hitDetector.SetParentSection(section);
            hitDetector.SetDamageRouter(damageRouter);
            hitDetectors[sectionType] = hitDetector;

            sectionManager.RegisterSection(section);
        }
    }

    [TearDown]
    public void Teardown()
    {
        if (shipObject != null)
        {
            Object.DestroyImmediate(shipObject);
        }
    }

    /// <summary>
    /// Creates a test projectile aimed at the ship.
    /// </summary>
    private Projectile CreateTestProjectile<T>(float damage, Vector3 spawnPos, Vector3 targetPos) where T : Projectile
    {
        GameObject projectileObj = new GameObject("TestProjectile");
        projectileObj.transform.position = spawnPos;
        projectileObj.transform.LookAt(targetPos);

        // Add collider
        SphereCollider col = projectileObj.AddComponent<SphereCollider>();
        col.radius = 0.5f;
        col.isTrigger = true;

        T projectile = projectileObj.AddComponent<T>();

        WeaponSystem.ProjectileSpawnInfo info = new WeaponSystem.ProjectileSpawnInfo
        {
            SpawnPosition = spawnPos,
            SpawnRotation = projectileObj.transform.rotation,
            Damage = damage,
            Speed = 10f,
            OwnerShip = null,
            TargetShip = ship
        };

        projectile.Initialize(info);

        return projectile;
    }

    // ==================== TEST 1: Projectile Hits Section ====================

    /// <summary>
    /// Test 1: Projectile hits correct section based on hit detector.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ProjectileHitsSection()
    {
        yield return null;

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];
        ShipSection foreSection = sections[SectionType.Fore];

        // Create a mock projectile
        GameObject projectileObj = new GameObject("MockProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();

        // Initialize with test values
        WeaponSystem.ProjectileSpawnInfo info = new WeaponSystem.ProjectileSpawnInfo
        {
            Damage = 50f,
            Speed = 10f,
            OwnerShip = null, // Not owned by target
            TargetShip = ship
        };
        projectile.Initialize(info);

        // Simulate hit on Fore section
        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        // Verify section was correctly identified
        Assert.AreEqual(SectionType.Fore, report.SectionHit, "Section hit should be Fore");
        Assert.AreEqual(50f, report.TotalIncomingDamage, "Total damage should be 50");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 2: Damage Routes Through Shields ====================

    /// <summary>
    /// Test 2: Projectile damage is absorbed by shields first.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ProjectileDamageRoutedThroughShields()
    {
        yield return null;

        float initialShields = shieldSystem.CurrentShields;
        Assert.AreEqual(200f, initialShields, "Shields should start at 200");

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];

        // Create projectile
        GameObject projectileObj = new GameObject("MockProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 50f });

        // Simulate hit
        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        // Verify shields absorbed damage
        Assert.AreEqual(50f, report.ShieldDamage, "Shield should absorb 50 damage");
        Assert.AreEqual(0f, report.ArmorDamage, "No armor damage");
        Assert.AreEqual(150f, shieldSystem.CurrentShields, "Shields should be 150");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 3: Shield Overflow to Armor ====================

    /// <summary>
    /// Test 3: Damage overflows from shields to section armor.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ProjectileOverflowToArmor()
    {
        yield return null;

        // Set shields low
        shieldSystem.SetShields(30f);
        ShipSection foreSection = sections[SectionType.Fore];
        float initialArmor = foreSection.CurrentArmor;

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];

        // Create projectile with damage > remaining shields
        GameObject projectileObj = new GameObject("MockProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 50f });

        // Simulate hit: 50 damage, 30 shields = 20 overflow to armor
        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        Assert.AreEqual(30f, report.ShieldDamage, "Shield should absorb 30");
        Assert.AreEqual(20f, report.ArmorDamage, "Armor should take 20 overflow");
        Assert.AreEqual(0f, shieldSystem.CurrentShields, "Shields should be depleted");
        Assert.IsTrue(report.ShieldsDepleted, "ShieldsDepleted flag should be true");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 4: Section Breach ====================

    /// <summary>
    /// Test 4: Enough damage breaches section (armor + structure destroyed).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ProjectileSectionBreach()
    {
        yield return null;

        // Deplete shields
        shieldSystem.SetShields(0f);

        ShipSection foreSection = sections[SectionType.Fore];
        // Fore has 100 armor + 50 structure = 150 total
        float totalSectionHP = foreSection.CurrentArmor + foreSection.CurrentStructure;

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];

        // Create projectile with enough damage to breach
        GameObject projectileObj = new GameObject("MockProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 200f }); // More than 150

        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        Assert.IsTrue(report.ArmorBroken, "Armor should be broken");
        Assert.IsTrue(report.SectionBreached, "Section should be breached");
        Assert.IsTrue(foreSection.IsBreached, "Fore section should be breached");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 5: Ballistic Projectile Specific ====================

    /// <summary>
    /// Test 5: Ballistic projectile correctly applies damage.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_BallisticProjectileDamage()
    {
        yield return null;

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];

        // Create ballistic projectile
        GameObject projectileObj = new GameObject("BallisticTest");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();

        WeaponSystem.ProjectileSpawnInfo info = new WeaponSystem.ProjectileSpawnInfo
        {
            Damage = 75f,
            Speed = 15f,
            OwnerShip = null
        };
        projectile.Initialize(info);

        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        Assert.AreEqual(75f, report.TotalIncomingDamage, "Total damage should be 75");
        Assert.AreEqual(75f, report.ShieldDamage, "All damage to shields");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 6: Homing Projectile Specific ====================

    /// <summary>
    /// Test 6: Homing projectile correctly applies damage.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HomingProjectileDamage()
    {
        yield return null;

        SectionHitDetector aftDetector = hitDetectors[SectionType.Aft];

        // Create homing projectile
        GameObject projectileObj = new GameObject("HomingTest");
        HomingProjectile projectile = projectileObj.AddComponent<HomingProjectile>();

        WeaponSystem.ProjectileSpawnInfo info = new WeaponSystem.ProjectileSpawnInfo
        {
            Damage = 100f,
            Speed = 8f,
            OwnerShip = null,
            TargetShip = ship
        };
        projectile.Initialize(info);

        DamageReport report = aftDetector.HandleProjectileHit(projectile);

        Assert.AreEqual(100f, report.TotalIncomingDamage, "Total damage should be 100");
        Assert.AreEqual(SectionType.Aft, report.SectionHit, "Section hit should be Aft");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 7: Different Sections ====================

    /// <summary>
    /// Test 7: Hitting different sections correctly identifies them.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ProjectileHitsDifferentSections()
    {
        yield return null;

        SectionType[] sectionsToTest = { SectionType.Fore, SectionType.Aft, SectionType.Port, SectionType.Starboard };

        foreach (SectionType sectionType in sectionsToTest)
        {
            // Reset shields for each test
            shieldSystem.Reset();

            SectionHitDetector detector = hitDetectors[sectionType];

            GameObject projectileObj = new GameObject($"ProjectileFor{sectionType}");
            BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
            projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 25f });

            DamageReport report = detector.HandleProjectileHit(projectile);

            Assert.AreEqual(sectionType, report.SectionHit, $"Section hit should be {sectionType}");

            Object.DestroyImmediate(projectileObj);
        }
    }

    // ==================== TEST 8: DamageReport Accuracy ====================

    /// <summary>
    /// Test 8: DamageReport contains all correct data.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ProjectileDamageReport()
    {
        yield return null;

        // Set specific state
        shieldSystem.SetShields(50f);
        ShipSection foreSection = sections[SectionType.Fore];

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];

        // Create projectile: 100 damage
        // 50 to shields (depletes), 50 to armor
        GameObject projectileObj = new GameObject("MockProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 100f });

        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        Assert.AreEqual(100f, report.TotalIncomingDamage, "Total incoming should be 100");
        Assert.AreEqual(50f, report.ShieldDamage, "Shield damage should be 50");
        Assert.AreEqual(50f, report.ArmorDamage, "Armor damage should be 50");
        Assert.AreEqual(0f, report.StructureDamage, "Structure damage should be 0");
        Assert.IsTrue(report.ShieldsDepleted, "Shields should be depleted");
        Assert.IsFalse(report.ArmorBroken, "Armor should not be broken (100 armor - 50 = 50)");
        Assert.AreEqual(SectionType.Fore, report.SectionHit, "Section hit should be Fore");
        Assert.AreEqual(foreSection, report.Section, "Section reference should match");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 9: Multiple Projectiles Same Section ====================

    /// <summary>
    /// Test 9: Multiple projectiles hitting same section accumulate damage.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MultipleProjectileSameSection()
    {
        yield return null;

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];
        ShipSection foreSection = sections[SectionType.Fore];

        // Fire 4 projectiles at 50 damage each = 200 total
        // Shields: 200, so all goes to shields
        for (int i = 0; i < 4; i++)
        {
            GameObject projectileObj = new GameObject($"Projectile{i}");
            BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
            projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 50f });

            foreDetector.HandleProjectileHit(projectile);

            Object.DestroyImmediate(projectileObj);
        }

        Assert.AreEqual(0f, shieldSystem.CurrentShields, "Shields should be depleted after 200 damage");

        // Fire one more - should hit armor
        GameObject finalProjectile = new GameObject("FinalProjectile");
        BallisticProjectile finalProj = finalProjectile.AddComponent<BallisticProjectile>();
        finalProj.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 50f });

        DamageReport finalReport = foreDetector.HandleProjectileHit(finalProj);

        Assert.AreEqual(0f, finalReport.ShieldDamage, "No shield damage (depleted)");
        Assert.AreEqual(50f, finalReport.ArmorDamage, "50 damage to armor");
        Assert.AreEqual(50f, foreSection.CurrentArmor, "Fore armor should be 50 (100-50)");

        Object.DestroyImmediate(finalProjectile);
    }

    // ==================== TEST 10: Projectile Against Breached Section ====================

    /// <summary>
    /// Test 10: Damage to already breached section causes overflow.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ProjectileAgainstBreachedSection()
    {
        yield return null;

        // Deplete shields
        shieldSystem.SetShields(0f);

        // Breach the Fore section manually
        ShipSection foreSection = sections[SectionType.Fore];
        foreSection.ApplyDamage(150f); // Depletes 100 armor + 50 structure
        Assert.IsTrue(foreSection.IsBreached, "Fore section should be breached");

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];

        // Fire projectile at breached section
        GameObject projectileObj = new GameObject("MockProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 50f });

        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        // Damage overflows from breached section to Core
        // Core has 0 armor and 30 structure, so 30 goes to Core structure, 20 overflows
        Assert.AreEqual(0f, report.ArmorDamage, "No armor to damage on breached section");
        Assert.AreEqual(30f, report.StructureDamage, "Core takes 30 structure damage from overflow");
        Assert.AreEqual(20f, report.OverflowDamage, "Remaining 20 damage overflows past Core");
        Assert.IsTrue(report.OverflowedToCore, "Damage should have overflowed to Core");

        Object.DestroyImmediate(projectileObj);
    }
}
