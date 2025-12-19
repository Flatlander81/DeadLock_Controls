using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for Phase 3 - Shield System and Damage Routing (Step 3.2).
/// Tests shield absorption, damage routing, and Shield Boost activation.
/// </summary>
public class ShieldSystemTests
{
    private GameObject shipObject;
    private Ship ship;
    private ShieldSystem shieldSystem;
    private SectionManager sectionManager;
    private DamageRouter damageRouter;
    private ShipSection testSection;

    [SetUp]
    public void Setup()
    {
        // Create test ship
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();
        shipObject.AddComponent<HeatManager>();

        // Add ShieldSystem
        shieldSystem = shipObject.AddComponent<ShieldSystem>();
        shieldSystem.Initialize(200f); // GDD value

        // Add SectionManager
        sectionManager = shipObject.AddComponent<SectionManager>();

        // Add DamageRouter
        damageRouter = shipObject.AddComponent<DamageRouter>();
        damageRouter.SetReferences(shieldSystem, sectionManager, ship);

        // Create a test section
        GameObject sectionObj = new GameObject("TestSection_Fore");
        sectionObj.transform.SetParent(shipObject.transform);
        testSection = sectionObj.AddComponent<ShipSection>();
        testSection.Initialize(SectionType.Fore, 100f, 50f, ship);
        sectionManager.RegisterSection(testSection);
    }

    [TearDown]
    public void Teardown()
    {
        if (shipObject != null) Object.DestroyImmediate(shipObject);
    }

    // ==================== SHIELD INITIALIZATION TESTS ====================

    /// <summary>
    /// Test 1: Shield system initializes with correct values.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldInitialization()
    {
        yield return null;

        Assert.AreEqual(200f, shieldSystem.MaxShields, "Max shields should be 200");
        Assert.AreEqual(200f, shieldSystem.CurrentShields, "Current shields should start at max");
        Assert.IsTrue(shieldSystem.IsShieldActive, "Shields should be active");
        Assert.AreEqual(1f, shieldSystem.GetShieldPercentage(), "Shield percentage should be 100%");
    }

    /// <summary>
    /// Test 2: Shields absorb full damage when sufficient.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldAbsorbsFullDamage()
    {
        yield return null;

        float overflow = shieldSystem.AbsorbDamage(50f);

        Assert.AreEqual(0f, overflow, "No damage should overflow");
        Assert.AreEqual(150f, shieldSystem.CurrentShields, "Shields should be reduced to 150");
        Assert.IsTrue(shieldSystem.IsShieldActive, "Shields should still be active");
    }

    /// <summary>
    /// Test 3: Damage overflows when exceeding shields.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldOverflow()
    {
        yield return null;

        // Apply 250 damage (200 shields + 50 overflow)
        float overflow = shieldSystem.AbsorbDamage(250f);

        Assert.AreEqual(50f, overflow, "50 damage should overflow");
        Assert.AreEqual(0f, shieldSystem.CurrentShields, "Shields should be depleted");
        Assert.IsFalse(shieldSystem.IsShieldActive, "Shields should no longer be active");
    }

    /// <summary>
    /// Test 4: Shield depleted event fires when shields reach 0.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldDepletedEvent()
    {
        yield return null;

        bool eventFired = false;
        shieldSystem.OnShieldDepleted += () => eventFired = true;

        // Deplete shields
        shieldSystem.AbsorbDamage(200f);

        Assert.IsTrue(eventFired, "OnShieldDepleted should fire");
        Assert.IsFalse(shieldSystem.IsShieldActive, "Shields should be inactive");
    }

    /// <summary>
    /// Test 5: Shields do not regenerate (no passive regen).
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldNoRegeneration()
    {
        yield return null;

        // Damage shields
        shieldSystem.AbsorbDamage(100f);
        Assert.AreEqual(100f, shieldSystem.CurrentShields, "Shields should be at 100");

        // Wait some frames (simulating time passing)
        yield return null;
        yield return null;
        yield return null;

        // Shields should NOT have regenerated
        Assert.AreEqual(100f, shieldSystem.CurrentShields, "Shields should not regenerate");
    }

    /// <summary>
    /// Test 6: Shields can be restored when at 0.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldRestore()
    {
        yield return null;

        // Deplete shields
        shieldSystem.AbsorbDamage(200f);
        Assert.AreEqual(0f, shieldSystem.CurrentShields, "Shields should be depleted");
        Assert.IsTrue(shieldSystem.CanRestoreShields(), "Should be able to restore shields");

        // Restore shields
        bool restored = shieldSystem.RestoreShields(100f);

        Assert.IsTrue(restored, "Restore should succeed");
        Assert.AreEqual(100f, shieldSystem.CurrentShields, "Shields should be restored to 100");
        Assert.IsTrue(shieldSystem.IsShieldActive, "Shields should be active again");
    }

    /// <summary>
    /// Test 7: Shield restore blocked when shields are still active.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldRestoreBlockedWhenActive()
    {
        yield return null;

        // Shields are at 200 (active)
        Assert.IsFalse(shieldSystem.CanRestoreShields(), "Should not be able to restore active shields");

        // Try to restore
        bool restored = shieldSystem.RestoreShields(100f);

        Assert.IsFalse(restored, "Restore should fail");
        Assert.AreEqual(200f, shieldSystem.CurrentShields, "Shields should remain at 200");
    }

    /// <summary>
    /// Test 8: Shield Boost ability checks activation correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldBoostCanActivateCheck()
    {
        yield return null;

        // Create Shield Boost ability data
        ShieldBoostData shieldBoostData = ScriptableObject.CreateInstance<ShieldBoostData>();

        // With full shields - should NOT be able to activate
        Assert.IsFalse(shieldBoostData.CanActivate(ship), "Shield Boost should not activate with full shields");
        Assert.IsFalse(string.IsNullOrEmpty(shieldBoostData.GetActivationBlockedReason(ship)),
            "Should have blocked reason");

        // Deplete shields
        shieldSystem.SetShields(0f);

        // Now should be able to activate
        Assert.IsTrue(shieldBoostData.CanActivate(ship), "Shield Boost should activate with depleted shields");
        Assert.IsTrue(string.IsNullOrEmpty(shieldBoostData.GetActivationBlockedReason(ship)),
            "Should have no blocked reason");

        // Cleanup
        Object.DestroyImmediate(shieldBoostData);
    }

    // ==================== DAMAGE ROUTER TESTS ====================

    /// <summary>
    /// Test 9: DamageRouter sends damage to shields first.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DamageRouterShieldsFirst()
    {
        yield return null;

        DamageReport report = damageRouter.ProcessDamage(50f, SectionType.Fore);

        Assert.AreEqual(50f, report.TotalIncomingDamage, "Total incoming should be 50");
        Assert.AreEqual(50f, report.ShieldDamage, "All 50 should go to shields");
        Assert.AreEqual(0f, report.ArmorDamage, "No damage to armor");
        Assert.AreEqual(0f, report.StructureDamage, "No damage to structure");
        Assert.AreEqual(150f, shieldSystem.CurrentShields, "Shields should be reduced");
        Assert.AreEqual(100f, testSection.CurrentArmor, "Section armor should be untouched");
    }

    /// <summary>
    /// Test 10: Damage overflows from shields to section.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DamageRouterOverflowToSection()
    {
        yield return null;

        // Apply 250 damage (200 shields + 50 to section armor)
        DamageReport report = damageRouter.ProcessDamage(250f, SectionType.Fore);

        Assert.AreEqual(250f, report.TotalIncomingDamage, "Total incoming should be 250");
        Assert.AreEqual(200f, report.ShieldDamage, "200 should go to shields");
        Assert.AreEqual(50f, report.ArmorDamage, "50 should go to armor");
        Assert.AreEqual(0f, report.StructureDamage, "No damage to structure yet");
        Assert.IsTrue(report.ShieldsDepleted, "Shields should be depleted");
        Assert.AreEqual(0f, shieldSystem.CurrentShields, "Shields should be at 0");
        Assert.AreEqual(50f, testSection.CurrentArmor, "Section armor should be reduced to 50");
    }

    /// <summary>
    /// Test 11: DamageReport accurately tracks all damage values.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DamageReportAccuracy()
    {
        yield return null;

        // Deplete shields first
        shieldSystem.SetShields(0f);

        // Apply 175 damage (100 armor + 50 structure + 25 overflow)
        DamageReport report = damageRouter.ProcessDamage(175f, SectionType.Fore);

        Assert.AreEqual(175f, report.TotalIncomingDamage, "Total incoming should be 175");
        Assert.AreEqual(0f, report.ShieldDamage, "No shield damage (depleted)");
        Assert.AreEqual(100f, report.ArmorDamage, "100 to armor");
        Assert.AreEqual(50f, report.StructureDamage, "50 to structure");
        Assert.AreEqual(25f, report.OverflowDamage, "25 overflow");
        Assert.IsTrue(report.ArmorBroken, "Armor should be broken");
        Assert.IsTrue(report.SectionBreached, "Section should be breached");
        Assert.AreEqual(SectionType.Fore, report.SectionHit, "Section hit should be Fore");
        Assert.AreEqual(testSection, report.Section, "Section reference should match");
        Assert.AreEqual(150f, report.TotalDamageApplied, "Total applied should be 150 (armor + structure)");
    }

    /// <summary>
    /// Test 12: Shield percentage calculation is accurate.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_ShieldPercentage()
    {
        yield return null;

        // Full shields
        Assert.AreEqual(1f, shieldSystem.GetShieldPercentage(), 0.001f, "100% at full");

        // Half shields
        shieldSystem.AbsorbDamage(100f);
        Assert.AreEqual(0.5f, shieldSystem.GetShieldPercentage(), 0.001f, "50% at half");

        // Quarter shields
        shieldSystem.AbsorbDamage(50f);
        Assert.AreEqual(0.25f, shieldSystem.GetShieldPercentage(), 0.001f, "25% at quarter");

        // Empty shields
        shieldSystem.AbsorbDamage(50f);
        Assert.AreEqual(0f, shieldSystem.GetShieldPercentage(), 0.001f, "0% when depleted");
    }
}
