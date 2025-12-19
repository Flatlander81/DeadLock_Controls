using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Integration tests for the complete weapon -> projectile -> damage system flow.
/// Tests that weapons fire projectiles that correctly route damage through
/// shields, sections, armor, structure, and trigger critical hits.
/// </summary>
public class WeaponDamageIntegrationTests
{
    private GameObject testRoot;
    private GameObject shooterObject;
    private GameObject targetObject;
    private Ship shooterShip;
    private Ship targetShip;
    private WeaponManager shooterWeaponManager;
    private ShieldSystem targetShields;
    private SectionManager targetSections;
    private DamageRouter targetDamageRouter;
    private Dictionary<SectionType, SectionHitDetector> hitDetectors;

    [SetUp]
    public void Setup()
    {
        testRoot = new GameObject("TestRoot");

        // Create ProjectileManager (required for spawning projectiles)
        GameObject projectileManagerObj = new GameObject("ProjectileManager");
        projectileManagerObj.transform.SetParent(testRoot.transform);
        projectileManagerObj.AddComponent<ProjectileManager>();

        // Create shooter ship with weapons
        shooterObject = CreateShipWithWeapons("Shooter", Vector3.zero);
        shooterShip = shooterObject.GetComponent<Ship>();
        shooterWeaponManager = shooterObject.GetComponent<WeaponManager>();

        // Create target ship with full damage system
        targetObject = CreateTargetShip("Target", new Vector3(15, 0, 0));
        targetShip = targetObject.GetComponent<Ship>();
        targetShields = targetObject.GetComponent<ShieldSystem>();
        targetSections = targetObject.GetComponent<SectionManager>();
        targetDamageRouter = targetObject.GetComponent<DamageRouter>();
    }

    [TearDown]
    public void Teardown()
    {
        if (testRoot != null)
        {
            Object.DestroyImmediate(testRoot);
        }
    }

    private GameObject CreateShipWithWeapons(string name, Vector3 position)
    {
        GameObject shipObj = new GameObject(name);
        shipObj.transform.SetParent(testRoot.transform);
        shipObj.transform.position = position;
        shipObj.transform.LookAt(new Vector3(15, 0, 0)); // Face target

        Ship ship = shipObj.AddComponent<Ship>();
        shipObj.AddComponent<HeatManager>();
        WeaponManager weaponManager = shipObj.AddComponent<WeaponManager>();

        // Add a RailGun
        GameObject railGunObj = new GameObject("RailGun");
        railGunObj.transform.SetParent(shipObj.transform);
        railGunObj.transform.localPosition = new Vector3(0, 0, 1);
        RailGun railGun = railGunObj.AddComponent<RailGun>();

        // Add a NewtonianCannon
        GameObject cannonObj = new GameObject("NewtonianCannon");
        cannonObj.transform.SetParent(shipObj.transform);
        cannonObj.transform.localPosition = new Vector3(0, 0, 1);
        NewtonianCannon cannon = cannonObj.AddComponent<NewtonianCannon>();

        return shipObj;
    }

    private GameObject CreateTargetShip(string name, Vector3 position)
    {
        GameObject shipObj = new GameObject(name);
        shipObj.transform.SetParent(testRoot.transform);
        shipObj.transform.position = position;

        Ship ship = shipObj.AddComponent<Ship>();
        shipObj.AddComponent<HeatManager>();

        // Add ShieldSystem
        ShieldSystem shieldSystem = shipObj.AddComponent<ShieldSystem>();
        shieldSystem.Initialize(200f);

        // Add SectionManager
        SectionManager sectionManager = shipObj.AddComponent<SectionManager>();

        // Add DamageRouter
        DamageRouter damageRouter = shipObj.AddComponent<DamageRouter>();
        damageRouter.SetReferences(shieldSystem, sectionManager, ship);

        // Create sections with colliders and hit detectors
        hitDetectors = new Dictionary<SectionType, SectionHitDetector>();

        foreach (SectionType sectionType in SectionDefinitions.GetAllSectionTypes())
        {
            var config = SectionDefinitions.GetConfig(sectionType);

            GameObject sectionObj = new GameObject($"Section_{sectionType}");
            sectionObj.transform.SetParent(shipObj.transform);
            sectionObj.transform.localPosition = config.ColliderPosition;

            ShipSection section = sectionObj.AddComponent<ShipSection>();
            section.Initialize(sectionType, ship);

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

        return shipObj;
    }

    // ==================== TEST 1: Weapon Fires Projectile ====================

    /// <summary>
    /// Test that weapons can fire projectiles at a target.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_WeaponFiresProjectile()
    {
        yield return null; // Wait for Start()

        RailGun railGun = shooterObject.GetComponentInChildren<RailGun>();
        Assert.IsNotNull(railGun, "RailGun should exist on shooter");

        // Initialize and set target
        railGun.Initialize(shooterShip);
        railGun.SetTarget(targetShip);

        Assert.IsTrue(railGun.CanFire(), "RailGun should be able to fire");

        // Fire the weapon
        var fireCoroutine = railGun.FireWithSpinUp();
        yield return fireCoroutine;

        // Projectile should have been spawned (check ProjectileManager or wait for impact)
        yield return new WaitForSeconds(0.5f); // Wait for projectile to travel

        // Verify damage was applied (shields should be reduced)
        Assert.Less(targetShields.CurrentShields, 200f, "Target shields should be reduced after hit");
    }

    // ==================== TEST 2: Projectile Damages Shields ====================

    /// <summary>
    /// Test that projectile damage is correctly absorbed by shields first.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ProjectileDamagesShields()
    {
        yield return null;

        float initialShields = targetShields.CurrentShields;
        Assert.AreEqual(200f, initialShields, "Target should start with 200 shields");

        // Create and fire a projectile manually
        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];

        GameObject projectileObj = new GameObject("TestProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo
        {
            Damage = 50f,
            Speed = 40f,
            OwnerShip = shooterShip,
            TargetShip = targetShip
        });

        // Simulate hit
        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        Assert.AreEqual(50f, report.ShieldDamage, "All damage should go to shields");
        Assert.AreEqual(0f, report.ArmorDamage, "No armor damage when shields are up");
        Assert.AreEqual(150f, targetShields.CurrentShields, "Shields should be 150 after 50 damage");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 3: Shield Overflow to Section ====================

    /// <summary>
    /// Test that damage overflows from depleted shields to section armor.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldOverflowToSection()
    {
        yield return null;

        // Set shields low
        targetShields.SetShields(30f);

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];
        ShipSection foreSection = targetSections.GetSection(SectionType.Fore);
        float initialArmor = foreSection.CurrentArmor;

        // Fire projectile with damage > remaining shields
        GameObject projectileObj = new GameObject("TestProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo
        {
            Damage = 80f, // 30 to shields, 50 overflow to armor
            OwnerShip = shooterShip
        });

        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        Assert.AreEqual(30f, report.ShieldDamage, "Shields should absorb 30");
        Assert.AreEqual(50f, report.ArmorDamage, "50 should overflow to armor");
        Assert.IsTrue(report.ShieldsDepleted, "Shields should be depleted");
        Assert.AreEqual(0f, targetShields.CurrentShields, "Shields should be 0");
        Assert.AreEqual(initialArmor - 50f, foreSection.CurrentArmor, "Armor should be reduced by 50");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 4: Multiple Weapons Fire ====================

    /// <summary>
    /// Test that multiple weapons can fire and all damage is applied.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_MultipleWeaponsFire()
    {
        yield return null;

        WeaponSystem[] weapons = shooterObject.GetComponentsInChildren<WeaponSystem>();
        Assert.GreaterOrEqual(weapons.Length, 2, "Should have at least 2 weapons");

        float initialShields = targetShields.CurrentShields;

        // Initialize and fire all weapons
        foreach (var weapon in weapons)
        {
            weapon.Initialize(shooterShip);
            weapon.SetTarget(targetShip);

            if (weapon.CanFire())
            {
                yield return weapon.FireWithSpinUp();
            }
        }

        // Wait for projectiles to hit
        yield return new WaitForSeconds(1f);

        // Shields should be reduced by both weapons
        Assert.Less(targetShields.CurrentShields, initialShields, "Shields should be reduced");
    }

    // ==================== TEST 5: Section Hit Detection ====================

    /// <summary>
    /// Test that SectionHitDetector correctly identifies the hit section.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionHitDetection()
    {
        yield return null;

        // Test hitting different sections
        SectionType[] sectionsToTest = { SectionType.Fore, SectionType.Aft, SectionType.Port };

        foreach (SectionType sectionType in sectionsToTest)
        {
            // Reset shields for each test
            targetShields.SetShields(200f);

            SectionHitDetector detector = hitDetectors[sectionType];

            GameObject projectileObj = new GameObject($"Projectile_{sectionType}");
            BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
            projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 25f, OwnerShip = shooterShip });

            DamageReport report = detector.HandleProjectileHit(projectile);

            Assert.AreEqual(sectionType, report.SectionHit, $"Section should be {sectionType}");

            Object.DestroyImmediate(projectileObj);
        }
    }

    // ==================== TEST 6: Section Breach from Projectile ====================

    /// <summary>
    /// Test that enough projectile damage can breach a section.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionBreachFromProjectile()
    {
        yield return null;

        // Deplete shields
        targetShields.SetShields(0f);

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];
        ShipSection foreSection = targetSections.GetSection(SectionType.Fore);

        // Calculate damage needed to breach (armor + structure + extra)
        float damageNeeded = foreSection.CurrentArmor + foreSection.CurrentStructure + 10f;

        GameObject projectileObj = new GameObject("HeavyProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo
        {
            Damage = damageNeeded,
            OwnerShip = shooterShip
        });

        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        Assert.IsTrue(report.ArmorBroken, "Armor should be broken");
        Assert.IsTrue(report.SectionBreached, "Section should be breached");
        Assert.IsTrue(foreSection.IsBreached, "Fore section should be breached");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 7: Damage Report Contains All Data ====================

    /// <summary>
    /// Test that DamageReport contains accurate damage breakdown.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DamageReportAccuracy()
    {
        yield return null;

        // Set specific shield amount
        targetShields.SetShields(40f);

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];
        ShipSection foreSection = targetSections.GetSection(SectionType.Fore);
        float initialArmor = foreSection.CurrentArmor;

        // 100 damage: 40 to shields, 60 to armor
        GameObject projectileObj = new GameObject("TestProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 100f, OwnerShip = shooterShip });

        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        Assert.AreEqual(100f, report.TotalIncomingDamage, "Total should be 100");
        Assert.AreEqual(40f, report.ShieldDamage, "Shield damage should be 40");
        Assert.AreEqual(60f, report.ArmorDamage, "Armor damage should be 60");
        Assert.AreEqual(0f, report.StructureDamage, "No structure damage yet");
        Assert.IsTrue(report.ShieldsDepleted, "Shields should be depleted");
        Assert.IsFalse(report.ArmorBroken, "Armor not yet broken");
        Assert.AreEqual(SectionType.Fore, report.SectionHit, "Section should be Fore");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 8: Homing Projectile Damage ====================

    /// <summary>
    /// Test that homing projectiles also route damage correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_HomingProjectileDamage()
    {
        yield return null;

        float initialShields = targetShields.CurrentShields;

        SectionHitDetector aftDetector = hitDetectors[SectionType.Aft];

        GameObject projectileObj = new GameObject("HomingMissile");
        HomingProjectile projectile = projectileObj.AddComponent<HomingProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo
        {
            Damage = 75f,
            Speed = 30f,
            OwnerShip = shooterShip,
            TargetShip = targetShip
        });

        DamageReport report = aftDetector.HandleProjectileHit(projectile);

        Assert.AreEqual(75f, report.TotalIncomingDamage, "Total damage should be 75");
        Assert.AreEqual(SectionType.Aft, report.SectionHit, "Section should be Aft");
        Assert.AreEqual(75f, report.ShieldDamage, "All damage to shields");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 9: Breached Section Overflow to Core ====================

    /// <summary>
    /// Test that damage to a breached section overflows to Core.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_BreachedSectionOverflowToCore()
    {
        yield return null;

        // Deplete shields
        targetShields.SetShields(0f);

        // Breach the Fore section
        ShipSection foreSection = targetSections.GetSection(SectionType.Fore);
        foreSection.ApplyDamage(200f); // Should breach it
        Assert.IsTrue(foreSection.IsBreached, "Fore should be breached");

        ShipSection coreSection = targetSections.GetSection(SectionType.Core);
        float initialCoreArmor = coreSection.CurrentArmor;

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];

        // Hit the breached section
        GameObject projectileObj = new GameObject("OverflowProjectile");
        BallisticProjectile projectile = projectileObj.AddComponent<BallisticProjectile>();
        projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 50f, OwnerShip = shooterShip });

        DamageReport report = foreDetector.HandleProjectileHit(projectile);

        // Damage should overflow to Core
        Assert.IsTrue(report.OverflowedToCore, "Damage should overflow to Core");

        Object.DestroyImmediate(projectileObj);
    }

    // ==================== TEST 10: Complete Combat Sequence ====================

    /// <summary>
    /// Test a complete combat sequence: multiple hits leading to section breach.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_CompleteCombatSequence()
    {
        yield return null;

        SectionHitDetector foreDetector = hitDetectors[SectionType.Fore];
        ShipSection foreSection = targetSections.GetSection(SectionType.Fore);

        // Phase 1: Deplete shields (200 HP)
        for (int i = 0; i < 4; i++)
        {
            GameObject proj = new GameObject($"ShieldBuster{i}");
            BallisticProjectile projectile = proj.AddComponent<BallisticProjectile>();
            projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 55f, OwnerShip = shooterShip });
            foreDetector.HandleProjectileHit(projectile);
            Object.DestroyImmediate(proj);
        }

        Assert.AreEqual(0f, targetShields.CurrentShields, "Shields should be depleted");

        // Phase 2: Damage armor (100 HP for Fore)
        for (int i = 0; i < 2; i++)
        {
            GameObject proj = new GameObject($"ArmorBreaker{i}");
            BallisticProjectile projectile = proj.AddComponent<BallisticProjectile>();
            projectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 55f, OwnerShip = shooterShip });
            foreDetector.HandleProjectileHit(projectile);
            Object.DestroyImmediate(proj);
        }

        Assert.AreEqual(0f, foreSection.CurrentArmor, "Armor should be depleted");

        // Phase 3: Damage structure until breach (50 HP for Fore)
        GameObject finalProj = new GameObject("FinalBlow");
        BallisticProjectile finalProjectile = finalProj.AddComponent<BallisticProjectile>();
        finalProjectile.Initialize(new WeaponSystem.ProjectileSpawnInfo { Damage = 60f, OwnerShip = shooterShip });
        DamageReport report = foreDetector.HandleProjectileHit(finalProjectile);
        Object.DestroyImmediate(finalProj);

        Assert.IsTrue(report.SectionBreached, "Section should be breached");
        Assert.IsTrue(foreSection.IsBreached, "Fore section should be marked as breached");
    }
}
