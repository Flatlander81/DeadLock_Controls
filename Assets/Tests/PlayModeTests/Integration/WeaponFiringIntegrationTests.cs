using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for Weapon Firing Queue integration with turn system.
/// Tests weapon queuing during Command Phase and execution during Simulation Phase.
/// </summary>
public class WeaponFiringIntegrationTests
{
    private GameObject turnManagerObj;
    private TurnManager turnManager;
    private GameObject coordinatorObj;
    private CombatCoordinator coordinator;
    private GameObject queueObj;
    private WeaponFiringQueue firingQueue;
    private GameObject projectileManagerObj;

    private GameObject playerShipObj;
    private Ship playerShip;
    private WeaponManager weaponManager;
    private GameObject targetShipObj;
    private Ship targetShip;

    private List<WeaponSystem> testWeapons;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing singletons
        if (TurnManager.Instance != null)
        {
            Object.Destroy(TurnManager.Instance.gameObject);
            yield return null;
        }

        if (CombatCoordinator.Instance != null)
        {
            Object.Destroy(CombatCoordinator.Instance.gameObject);
            yield return null;
        }

        if (WeaponFiringQueue.Instance != null)
        {
            Object.Destroy(WeaponFiringQueue.Instance.gameObject);
            yield return null;
        }

        // Create ProjectileManager (needed for weapons)
        projectileManagerObj = new GameObject("ProjectileManager");
        projectileManagerObj.AddComponent<ProjectileManager>();

        // Create target ship first
        targetShipObj = new GameObject("TargetShip");
        targetShipObj.transform.position = new Vector3(0, 0, 10); // In front of player
        targetShip = targetShipObj.AddComponent<Ship>();
        targetShipObj.AddComponent<HeatManager>();

        // Create player ship with weapons
        playerShipObj = new GameObject("PlayerShip");
        playerShipObj.transform.position = Vector3.zero;
        playerShip = playerShipObj.AddComponent<Ship>();
        playerShipObj.AddComponent<HeatManager>();
        weaponManager = playerShipObj.AddComponent<WeaponManager>();

        // Create test weapons
        testWeapons = new List<WeaponSystem>();
        CreateTestWeapon("Weapon1", playerShipObj, 1, 10, 0f); // Group 1, 10 heat, no spin-up
        CreateTestWeapon("Weapon2", playerShipObj, 1, 15, 0f); // Group 1, 15 heat, no spin-up
        CreateTestWeapon("Weapon3", playerShipObj, 2, 20, 0.2f); // Group 2, 20 heat, 0.2s spin-up
        CreateTestWeapon("Weapon4", playerShipObj, 2, 25, 0.3f); // Group 2, 25 heat, 0.3s spin-up

        // Create TurnManager
        turnManagerObj = new GameObject("TurnManager");
        turnManager = turnManagerObj.AddComponent<TurnManager>();

        // Create CombatCoordinator
        coordinatorObj = new GameObject("CombatCoordinator");
        coordinator = coordinatorObj.AddComponent<CombatCoordinator>();

        // Create WeaponFiringQueue
        queueObj = new GameObject("WeaponFiringQueue");
        firingQueue = queueObj.AddComponent<WeaponFiringQueue>();

        // Wait for all Start() methods
        yield return null;
        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        if (projectileManagerObj != null) Object.Destroy(projectileManagerObj);
        if (targetShipObj != null) Object.Destroy(targetShipObj);
        if (playerShipObj != null) Object.Destroy(playerShipObj);
        if (queueObj != null) Object.Destroy(queueObj);
        if (coordinatorObj != null) Object.Destroy(coordinatorObj);
        if (turnManagerObj != null) Object.Destroy(turnManagerObj);

        testWeapons?.Clear();
    }

    private void CreateTestWeapon(string name, GameObject parent, int group, int heatCost, float spinUpTime)
    {
        GameObject weaponObj = new GameObject(name);
        weaponObj.transform.SetParent(parent.transform);
        weaponObj.transform.localPosition = Vector3.forward; // Point toward target

        TestWeapon weapon = weaponObj.AddComponent<TestWeapon>();
        weapon.SetStats(heatCost, spinUpTime, group);

        testWeapons.Add(weapon);
    }

    #region Queue During Command Phase Tests

    [UnityTest]
    public IEnumerator Test_WeaponQueuesDuringCommand()
    {
        // Verify we're in command phase
        Assert.AreEqual(TurnPhase.Command, turnManager.CurrentPhase);

        // Queue a weapon
        bool success = firingQueue.QueueFire(testWeapons[0], targetShip);

        Assert.IsTrue(success, "Should successfully queue weapon during command phase");
        Assert.AreEqual(1, firingQueue.QueuedCount, "Queue should have 1 command");
        Assert.IsTrue(firingQueue.IsWeaponQueued(testWeapons[0]), "Weapon should be marked as queued");

        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_WeaponExecutesDuringSimulation()
    {
        // Queue a weapon
        firingQueue.QueueFire(testWeapons[0], targetShip);

        bool executionStarted = false;
        bool executionCompleted = false;

        firingQueue.OnQueueExecutionStarted += () => executionStarted = true;
        firingQueue.OnQueueExecutionCompleted += () => executionCompleted = true;

        // Start simulation
        turnManager.StartSimulation();

        // Wait for simulation to run
        yield return new WaitForSeconds(1.5f);

        Assert.IsTrue(executionStarted, "Queue execution should have started");
        Assert.IsTrue(executionCompleted, "Queue execution should have completed");
    }

    [UnityTest]
    public IEnumerator Test_QueuedHeatPreview()
    {
        // Queue weapons from groups 1 and 2
        firingQueue.QueueFire(testWeapons[0], targetShip); // 10 heat
        firingQueue.QueueFire(testWeapons[2], targetShip); // 20 heat

        int totalHeat = firingQueue.GetQueuedHeatCost();

        Assert.AreEqual(30, totalHeat, "Total queued heat should be 30");
        yield return null;
    }

    #endregion

    #region Execution Validation Tests

    [UnityTest]
    public IEnumerator Test_OutOfArcSkipped()
    {
        // Move target behind the player (out of arc for forward-facing weapons)
        targetShipObj.transform.position = new Vector3(0, 0, -10);

        // Queue weapon
        firingQueue.QueueFire(testWeapons[0], targetShip);

        bool commandFailed = false;
        firingQueue.OnCommandExecuted += (cmd, success) =>
        {
            if (!success) commandFailed = true;
        };

        // Start simulation
        turnManager.StartSimulation();

        // Wait for full simulation (movement + weapons phase)
        yield return new WaitForSeconds(2.0f);

        Assert.IsTrue(commandFailed, "Weapon should fail to fire when target is out of arc");
    }

    [UnityTest]
    public IEnumerator Test_SpinUpTimingRespected()
    {
        // Queue weapons with different spin-up times
        firingQueue.QueueFire(testWeapons[2], targetShip); // 0.2s spin-up
        firingQueue.QueueFire(testWeapons[3], targetShip); // 0.3s spin-up

        var executionOrder = new List<WeaponSystem>();
        firingQueue.OnCommandExecuted += (cmd, success) =>
        {
            if (success) executionOrder.Add(cmd.Weapon);
        };

        // Start simulation
        turnManager.StartSimulation();
        yield return new WaitForSeconds(1.5f);

        // Weapons should fire in spin-up order (lower first)
        if (executionOrder.Count >= 2)
        {
            Assert.AreEqual(testWeapons[2], executionOrder[0], "Weapon with lower spin-up should fire first");
            Assert.AreEqual(testWeapons[3], executionOrder[1], "Weapon with higher spin-up should fire second");
        }
    }

    [UnityTest]
    public IEnumerator Test_CooldownBlocksQueue()
    {
        // Put weapon on cooldown
        testWeapons[0].StartCooldown();

        // Try to queue
        bool success = firingQueue.QueueFire(testWeapons[0], targetShip);

        Assert.IsFalse(success, "Should not be able to queue weapon on cooldown");
        Assert.AreEqual(0, firingQueue.QueuedCount, "Queue should be empty");
        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_AmmoBlocksQueue()
    {
        // Create a weapon with limited ammo and deplete it
        GameObject ammoWeaponObj = new GameObject("AmmoWeapon");
        ammoWeaponObj.transform.SetParent(playerShipObj.transform);
        ammoWeaponObj.transform.localPosition = Vector3.forward;
        TestWeaponWithAmmo ammoWeapon = ammoWeaponObj.AddComponent<TestWeaponWithAmmo>();
        ammoWeapon.SetAmmo(1); // 1 ammo
        ammoWeapon.ConsumeAmmo(); // Now 0 ammo

        yield return null;

        // Try to queue
        bool success = firingQueue.QueueFire(ammoWeapon, targetShip);

        Assert.IsFalse(success, "Should not be able to queue weapon with no ammo");

        Object.Destroy(ammoWeaponObj);
    }

    #endregion

    #region Queue Management Tests

    [UnityTest]
    public IEnumerator Test_QueueClearedOnTurnEnd()
    {
        // Queue weapons
        firingQueue.QueueFire(testWeapons[0], targetShip);
        firingQueue.QueueFire(testWeapons[1], targetShip);

        Assert.AreEqual(2, firingQueue.QueuedCount);

        // Complete a turn
        turnManager.StartSimulation();
        turnManager.ForceEndTurn();
        yield return null;
        yield return null;

        Assert.AreEqual(0, firingQueue.QueuedCount, "Queue should be cleared at end of turn");
    }

    [UnityTest]
    public IEnumerator Test_CancelQueuedWeapon()
    {
        // Queue weapons
        firingQueue.QueueFire(testWeapons[0], targetShip);
        firingQueue.QueueFire(testWeapons[1], targetShip);

        Assert.AreEqual(2, firingQueue.QueuedCount);

        // Cancel one
        bool cancelled = firingQueue.CancelQueuedCommand(testWeapons[0]);

        Assert.IsTrue(cancelled, "Should successfully cancel queued command");
        Assert.AreEqual(1, firingQueue.QueuedCount, "Queue should have 1 command");
        Assert.IsFalse(firingQueue.IsWeaponQueued(testWeapons[0]), "Cancelled weapon should not be queued");
        Assert.IsTrue(firingQueue.IsWeaponQueued(testWeapons[1]), "Other weapon should still be queued");

        yield return null;
    }

    #endregion

    #region Group and Alpha Strike Tests

    [UnityTest]
    public IEnumerator Test_AlphaStrikeQueuesAll()
    {
        // Queue alpha strike
        int queued = firingQueue.QueueAlphaStrike(weaponManager, targetShip);

        Assert.AreEqual(4, queued, "Alpha strike should queue all 4 weapons");
        Assert.AreEqual(4, firingQueue.QueuedCount, "Queue should have 4 commands");

        foreach (var weapon in testWeapons)
        {
            Assert.IsTrue(firingQueue.IsWeaponQueued(weapon), $"{weapon.WeaponName} should be queued");
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_GroupFireQueuesGroup()
    {
        // Queue group 1 (weapons 0 and 1)
        int queued = firingQueue.QueueGroupFire(weaponManager, 1, targetShip);

        Assert.AreEqual(2, queued, "Group 1 should queue 2 weapons");
        Assert.AreEqual(2, firingQueue.QueuedCount, "Queue should have 2 commands");

        Assert.IsTrue(firingQueue.IsWeaponQueued(testWeapons[0]), "Weapon 1 should be queued");
        Assert.IsTrue(firingQueue.IsWeaponQueued(testWeapons[1]), "Weapon 2 should be queued");
        Assert.IsFalse(firingQueue.IsWeaponQueued(testWeapons[2]), "Weapon 3 should not be queued");
        Assert.IsFalse(firingQueue.IsWeaponQueued(testWeapons[3]), "Weapon 4 should not be queued");

        yield return null;
    }

    [UnityTest]
    public IEnumerator Test_MultipleTargetsSupported()
    {
        // Create second target
        GameObject target2Obj = new GameObject("Target2");
        target2Obj.transform.position = new Vector3(5, 0, 10);
        Ship target2 = target2Obj.AddComponent<Ship>();
        target2Obj.AddComponent<HeatManager>();

        yield return null;

        // Queue weapons at different targets
        firingQueue.QueueFire(testWeapons[0], targetShip);
        firingQueue.QueueFire(testWeapons[1], target2);

        Assert.AreEqual(2, firingQueue.QueuedCount);

        // Verify targets are different
        var commands = firingQueue.QueuedCommands;
        Assert.AreEqual(targetShip, commands[0].Target, "First command should target ship 1");
        Assert.AreEqual(target2, commands[1].Target, "Second command should target ship 2");

        Object.Destroy(target2Obj);
    }

    #endregion

    #region Test Helper Classes

    /// <summary>
    /// Test weapon implementation for unit tests.
    /// </summary>
    private class TestWeapon : WeaponSystem
    {
        private bool hasFired = false;

        public bool HasFired => hasFired;

        public void SetStats(int heat, float spinUp, int group)
        {
            weaponName = gameObject.name;
            heatCost = heat;
            spinUpTime = spinUp;
            assignedGroup = group;
            firingArc = 180f; // Forward arc
            maxRange = 50f;
            maxCooldown = 1;
            ammoCapacity = 0; // Infinite
        }

        protected override void Fire()
        {
            hasFired = true;
            Debug.Log($"{weaponName} fired!");
        }

        public override ProjectileSpawnInfo GetProjectileInfo()
        {
            return new ProjectileSpawnInfo
            {
                Type = ProjectileSpawnInfo.ProjectileType.InstantHit,
                SpawnPosition = transform.position,
                Damage = damage,
                OwnerShip = ownerShip
            };
        }
    }

    /// <summary>
    /// Test weapon with ammo for ammo-blocking tests.
    /// </summary>
    private class TestWeaponWithAmmo : WeaponSystem
    {
        public void SetAmmo(int count)
        {
            weaponName = gameObject.name;
            ammoCapacity = count;
            currentAmmo = count;
            firingArc = 180f;
            maxRange = 50f;
            maxCooldown = 0;
        }

        public void ConsumeAmmo()
        {
            currentAmmo = 0;
        }

        protected override void Fire()
        {
            Debug.Log($"{weaponName} fired!");
        }

        public override ProjectileSpawnInfo GetProjectileInfo()
        {
            return new ProjectileSpawnInfo();
        }
    }

    #endregion
}
