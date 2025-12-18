using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

/// <summary>
/// Unit tests for Phase 3 - Section Infrastructure (Step 3.1).
/// Tests ship sections, damage flow, and section management.
/// </summary>
public class SectionTests
{
    private GameObject shipObject;
    private Ship ship;
    private SectionManager sectionManager;
    private ShipSection testSection;

    [SetUp]
    public void Setup()
    {
        // Create test ship
        shipObject = new GameObject("TestShip");
        ship = shipObject.AddComponent<Ship>();
        shipObject.AddComponent<HeatManager>();

        // Add SectionManager
        sectionManager = shipObject.AddComponent<SectionManager>();

        // Create a test section
        GameObject sectionObj = new GameObject("TestSection_Fore");
        sectionObj.transform.SetParent(shipObject.transform);
        testSection = sectionObj.AddComponent<ShipSection>();
        testSection.Initialize(SectionType.Fore, 100f, 50f, ship);
    }

    [TearDown]
    public void Teardown()
    {
        if (shipObject != null) Object.DestroyImmediate(shipObject);
    }

    // ==================== SECTION INITIALIZATION TESTS ====================

    /// <summary>
    /// Test 1: Section initializes with correct armor and structure values.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionInitialization()
    {
        yield return null;

        Assert.AreEqual(SectionType.Fore, testSection.SectionType, "Section type should be Fore");
        Assert.AreEqual(100f, testSection.MaxArmor, "Max armor should be 100");
        Assert.AreEqual(100f, testSection.CurrentArmor, "Current armor should start at max");
        Assert.AreEqual(50f, testSection.MaxStructure, "Max structure should be 50");
        Assert.AreEqual(50f, testSection.CurrentStructure, "Current structure should start at max");
        Assert.IsFalse(testSection.IsBreached, "Section should not be breached initially");
        Assert.AreEqual(ship, testSection.ParentShip, "Parent ship should be assigned");
    }

    /// <summary>
    /// Test 2: Section initializes from SectionDefinitions config.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionInitializationFromDefinitions()
    {
        yield return null;

        // Create section using definition-based initialization
        GameObject sectionObj = new GameObject("TestSection_Aft");
        sectionObj.transform.SetParent(shipObject.transform);
        ShipSection section = sectionObj.AddComponent<ShipSection>();
        section.Initialize(SectionType.Aft, ship);

        var config = SectionDefinitions.GetConfig(SectionType.Aft);

        Assert.AreEqual(SectionType.Aft, section.SectionType, "Section type should match");
        Assert.AreEqual(config.Armor, section.MaxArmor, "Armor should match definition");
        Assert.AreEqual(config.Structure, section.MaxStructure, "Structure should match definition");
    }

    // ==================== DAMAGE FLOW TESTS ====================

    /// <summary>
    /// Test 3: Damage flows through armor first.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DamageFlowsToArmorFirst()
    {
        yield return null;

        DamageResult result = testSection.ApplyDamage(30f);

        Assert.AreEqual(30f, result.DamageToArmor, "30 damage should go to armor");
        Assert.AreEqual(0f, result.DamageToStructure, "No damage should reach structure");
        Assert.AreEqual(70f, testSection.CurrentArmor, "Armor should be reduced to 70");
        Assert.AreEqual(50f, testSection.CurrentStructure, "Structure should be unchanged");
        Assert.IsFalse(result.ArmorBroken, "Armor should not be broken");
        Assert.IsFalse(result.SectionBreached, "Section should not be breached");
    }

    /// <summary>
    /// Test 4: Damage overflows from armor to structure.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DamageOverflowsToStructure()
    {
        yield return null;

        // Apply 120 damage (100 armor + 20 structure)
        DamageResult result = testSection.ApplyDamage(120f);

        Assert.AreEqual(100f, result.DamageToArmor, "100 damage should go to armor");
        Assert.AreEqual(20f, result.DamageToStructure, "20 damage should overflow to structure");
        Assert.AreEqual(0f, testSection.CurrentArmor, "Armor should be depleted");
        Assert.AreEqual(30f, testSection.CurrentStructure, "Structure should be 30");
        Assert.IsTrue(result.ArmorBroken, "Armor should be marked as broken");
        Assert.IsFalse(result.SectionBreached, "Section should not yet be breached");
    }

    /// <summary>
    /// Test 5: Section breaches when structure reaches 0.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionBreachAtZeroStructure()
    {
        yield return null;

        // Apply 150 damage (100 armor + 50 structure)
        DamageResult result = testSection.ApplyDamage(150f);

        Assert.AreEqual(100f, result.DamageToArmor, "100 damage should go to armor");
        Assert.AreEqual(50f, result.DamageToStructure, "50 damage should go to structure");
        Assert.AreEqual(0f, testSection.CurrentArmor, "Armor should be 0");
        Assert.AreEqual(0f, testSection.CurrentStructure, "Structure should be 0");
        Assert.IsTrue(result.ArmorBroken, "Armor should be broken");
        Assert.IsTrue(result.SectionBreached, "Section should be breached");
        Assert.IsTrue(testSection.IsBreached, "IsBreached should be true");
    }

    /// <summary>
    /// Test 6: Overflow damage is tracked when section is breached.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_OverflowDamageTracking()
    {
        yield return null;

        // Apply 175 damage (100 armor + 50 structure + 25 overflow)
        DamageResult result = testSection.ApplyDamage(175f);

        Assert.AreEqual(100f, result.DamageToArmor, "100 damage to armor");
        Assert.AreEqual(50f, result.DamageToStructure, "50 damage to structure");
        Assert.AreEqual(25f, result.OverflowDamage, "25 damage should overflow");
        Assert.AreEqual(150f, result.TotalDamageApplied, "150 total damage applied");
        Assert.IsTrue(result.SectionBreached, "Section should be breached");
    }

    /// <summary>
    /// Test 7: Damage to already breached section returns overflow.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_DamageToBreachedSectionOverflows()
    {
        yield return null;

        // Breach the section first
        testSection.ApplyDamage(150f);
        Assert.IsTrue(testSection.IsBreached, "Section should be breached");

        // Apply more damage
        DamageResult result = testSection.ApplyDamage(50f);

        Assert.AreEqual(0f, result.DamageToArmor, "No damage to armor");
        Assert.AreEqual(0f, result.DamageToStructure, "No damage to structure");
        Assert.AreEqual(50f, result.OverflowDamage, "All damage should overflow");
        Assert.IsTrue(result.WasAlreadyBreached, "Should indicate already breached");
    }

    // ==================== SECTION MANAGER TESTS ====================

    /// <summary>
    /// Test 8: SectionManager registers sections correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionManagerRegistration()
    {
        yield return null;

        sectionManager.RegisterSection(testSection);

        Assert.AreEqual(1, sectionManager.SectionCount, "Should have 1 registered section");
        Assert.AreEqual(testSection, sectionManager.GetSection(SectionType.Fore), "Should retrieve registered section");
    }

    /// <summary>
    /// Test 9: SectionManager auto-registers child sections.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionManagerAutoRegister()
    {
        yield return null;

        // Create additional sections as children
        foreach (SectionType type in new[] { SectionType.Aft, SectionType.Port, SectionType.Starboard })
        {
            GameObject obj = new GameObject($"Section_{type}");
            obj.transform.SetParent(shipObject.transform);
            ShipSection section = obj.AddComponent<ShipSection>();
            section.Initialize(type, 80f, 40f, ship);
        }

        sectionManager.AutoRegisterChildSections();

        // Should have 4 sections (Fore from setup + 3 new ones)
        Assert.AreEqual(4, sectionManager.SectionCount, "Should have 4 registered sections");
        Assert.IsNotNull(sectionManager.GetSection(SectionType.Fore), "Fore section should be registered");
        Assert.IsNotNull(sectionManager.GetSection(SectionType.Aft), "Aft section should be registered");
        Assert.IsNotNull(sectionManager.GetSection(SectionType.Port), "Port section should be registered");
        Assert.IsNotNull(sectionManager.GetSection(SectionType.Starboard), "Starboard section should be registered");
    }

    /// <summary>
    /// Test 10: SectionManager tracks breached sections.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionManagerBreachedTracking()
    {
        yield return null;

        sectionManager.RegisterSection(testSection);

        // Initially no breached sections
        Assert.AreEqual(0, sectionManager.GetBreachedSections().Count, "No sections should be breached initially");
        Assert.AreEqual(1, sectionManager.GetOperationalSections().Count, "1 section should be operational");

        // Breach the section
        testSection.ApplyDamage(150f);

        Assert.AreEqual(1, sectionManager.GetBreachedSections().Count, "1 section should be breached");
        Assert.AreEqual(0, sectionManager.GetOperationalSections().Count, "0 sections should be operational");
    }

    /// <summary>
    /// Test 11: SectionManager calculates totals correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionManagerTotals()
    {
        yield return null;

        // Create two sections
        GameObject section2Obj = new GameObject("TestSection_Aft");
        section2Obj.transform.SetParent(shipObject.transform);
        ShipSection section2 = section2Obj.AddComponent<ShipSection>();
        section2.Initialize(SectionType.Aft, 60f, 40f, ship);

        sectionManager.RegisterSection(testSection);
        sectionManager.RegisterSection(section2);

        // Check totals (Fore: 100/50, Aft: 60/40)
        Assert.AreEqual(160f, sectionManager.GetTotalMaxArmor(), "Total max armor should be 160");
        Assert.AreEqual(90f, sectionManager.GetTotalMaxStructure(), "Total max structure should be 90");
        Assert.AreEqual(160f, sectionManager.GetTotalArmorRemaining(), "Total armor remaining should be 160");
        Assert.AreEqual(90f, sectionManager.GetTotalStructureRemaining(), "Total structure remaining should be 90");

        // Apply some damage
        testSection.ApplyDamage(50f);

        Assert.AreEqual(110f, sectionManager.GetTotalArmorRemaining(), "Total armor remaining should be 110 after damage");
    }

    /// <summary>
    /// Test 12: Section reset restores full health.
    /// </summary>
    [UnityTest]
    public IEnumerator Test_SectionReset()
    {
        yield return null;

        // Damage and breach section
        testSection.ApplyDamage(150f);
        Assert.IsTrue(testSection.IsBreached, "Section should be breached");
        Assert.AreEqual(0f, testSection.CurrentArmor, "Armor should be 0");
        Assert.AreEqual(0f, testSection.CurrentStructure, "Structure should be 0");

        // Reset section
        testSection.Reset();

        Assert.IsFalse(testSection.IsBreached, "Section should not be breached after reset");
        Assert.AreEqual(100f, testSection.CurrentArmor, "Armor should be restored");
        Assert.AreEqual(50f, testSection.CurrentStructure, "Structure should be restored");
    }
}
