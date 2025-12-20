using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for Turn System event infrastructure.
/// Tests TurnManager events, TurnEventSubscriber, and CombatCoordinator.
/// </summary>
public class TurnSystemIntegrationTests
{
    private GameObject turnManagerObj;
    private TurnManager turnManager;
    private GameObject coordinatorObj;
    private CombatCoordinator coordinator;
    private GameObject testShipObj;
    private Ship testShip;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Clean up any existing singleton
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

        // Create a test ship FIRST so TurnManager finds it
        testShipObj = new GameObject("TestShip");
        testShip = testShipObj.AddComponent<Ship>();

        // Create TurnManager
        turnManagerObj = new GameObject("TurnManager");
        turnManager = turnManagerObj.AddComponent<TurnManager>();

        // Create CombatCoordinator
        coordinatorObj = new GameObject("CombatCoordinator");
        coordinator = coordinatorObj.AddComponent<CombatCoordinator>();

        // Wait for Start() to run on all components
        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        if (testShipObj != null) Object.Destroy(testShipObj);
        if (coordinatorObj != null) Object.Destroy(coordinatorObj);
        if (turnManagerObj != null) Object.Destroy(turnManagerObj);
    }

    [UnityTest]
    public IEnumerator Test_TurnManagerEventsExist()
    {
        // Verify TurnManager is properly initialized
        Assert.IsNotNull(TurnManager.Instance, "TurnManager.Instance should exist");
        Assert.AreEqual(turnManager, TurnManager.Instance, "Instance should match our TurnManager");

        // Verify events can be subscribed to
        bool simulationPhaseStartFired = false;
        bool turnEndFired = false;

        turnManager.OnSimulationPhaseStart += () => simulationPhaseStartFired = true;
        turnManager.OnTurnEnd += (turn) => turnEndFired = true;

        // Trigger a full turn cycle
        turnManager.StartSimulation();
        turnManager.ForceEndTurn();
        yield return null;
        yield return null;

        Assert.IsTrue(simulationPhaseStartFired, "OnSimulationPhaseStart should have fired");
        Assert.IsTrue(turnEndFired, "OnTurnEnd should have fired");
    }

    [UnityTest]
    public IEnumerator Test_CommandPhaseEventFires()
    {
        // Verify we start in command phase
        Assert.AreEqual(TurnPhase.Command, turnManager.CurrentPhase, "Should start in command phase");

        bool eventFired = false;
        turnManager.OnCommandPhaseStart += () => eventFired = true;

        // Complete a turn - command phase should start for turn 2
        turnManager.StartSimulation();
        turnManager.ForceEndTurn();
        yield return null;
        yield return null;

        Assert.IsTrue(eventFired, "OnCommandPhaseStart should fire after turn ends");
        Assert.AreEqual(TurnPhase.Command, turnManager.CurrentPhase);
    }

    [UnityTest]
    public IEnumerator Test_SimulationPhaseEventFires()
    {
        bool eventFired = false;
        turnManager.OnSimulationPhaseStart += () => eventFired = true;

        turnManager.StartSimulation();
        yield return null;

        Assert.IsTrue(eventFired, "OnSimulationPhaseStart should fire when starting simulation");
        Assert.AreEqual(TurnPhase.Simulation, turnManager.CurrentPhase);
    }

    [UnityTest]
    public IEnumerator Test_TurnEndEventFires()
    {
        int completedTurn = -1;
        turnManager.OnTurnEnd += (turn) => completedTurn = turn;

        turnManager.StartSimulation();
        turnManager.ForceEndTurn();
        yield return null;
        yield return null;

        Assert.AreEqual(1, completedTurn, "OnTurnEnd should fire with completed turn number");
    }

    [UnityTest]
    public IEnumerator Test_TurnCounterIncrements()
    {
        Assert.AreEqual(1, turnManager.CurrentTurn, "Should start at turn 1");

        turnManager.StartSimulation();
        turnManager.ForceEndTurn();
        yield return null;
        yield return null;

        Assert.AreEqual(2, turnManager.CurrentTurn, "Turn should increment after completion");
    }

    [UnityTest]
    public IEnumerator Test_PhaseTransitionOrder()
    {
        var eventOrder = new List<string>();

        turnManager.OnSimulationPhaseStart += () => eventOrder.Add("SimStart");
        turnManager.OnSimulationPhaseEnd += () => eventOrder.Add("SimEnd");
        turnManager.OnTurnEnd += (t) => eventOrder.Add("TurnEnd");
        turnManager.OnTurnStart += (t) => eventOrder.Add("TurnStart");
        turnManager.OnCommandPhaseStart += () => eventOrder.Add("CommandStart");

        turnManager.StartSimulation();
        turnManager.ForceEndTurn();
        yield return null;
        yield return null;

        // Verify correct sequence
        Assert.IsTrue(eventOrder.Contains("SimStart"), "SimStart should be in order");
        Assert.IsTrue(eventOrder.Contains("SimEnd"), "SimEnd should be in order");
        Assert.IsTrue(eventOrder.Contains("TurnEnd"), "TurnEnd should be in order");

        Assert.IsTrue(eventOrder.IndexOf("SimStart") < eventOrder.IndexOf("SimEnd"),
            "SimStart should come before SimEnd");
        Assert.IsTrue(eventOrder.IndexOf("SimEnd") < eventOrder.IndexOf("TurnEnd"),
            "SimEnd should come before TurnEnd");
    }

    [UnityTest]
    public IEnumerator Test_SubscriberReceivesEvents()
    {
        // Create a test subscriber
        GameObject subscriberObj = new GameObject("TestSubscriber");
        TestEventSubscriber subscriber = subscriberObj.AddComponent<TestEventSubscriber>();

        yield return null; // Wait for subscriber to subscribe

        turnManager.StartSimulation();
        yield return null;

        Assert.IsTrue(subscriber.SimulationStartReceived, "Subscriber should receive simulation start event");

        Object.Destroy(subscriberObj);
    }

    [UnityTest]
    public IEnumerator Test_CombatCoordinatorOrchestrates()
    {
        Assert.IsNotNull(CombatCoordinator.Instance, "CombatCoordinator.Instance should exist");
        Assert.AreEqual(CombatCoordinator.SimulationStage.Idle, coordinator.CurrentStage,
            "Coordinator should start in Idle stage");

        turnManager.StartSimulation();
        yield return null;

        Assert.IsTrue(coordinator.IsSimulating, "Coordinator should be simulating");
        Assert.AreNotEqual(CombatCoordinator.SimulationStage.Idle, coordinator.CurrentStage,
            "Coordinator should not be Idle during simulation");
    }

    [UnityTest]
    public IEnumerator Test_SimulationProgressUpdates()
    {
        float lastProgress = -1;

        turnManager.OnSimulationProgress += (p) =>
        {
            lastProgress = p;
        };

        turnManager.StartSimulation();
        yield return new WaitForSeconds(0.2f);

        Assert.IsTrue(lastProgress >= 0, "Progress should be tracked");
        Assert.IsTrue(lastProgress <= 1, "Progress should be <= 1");
    }

    [UnityTest]
    public IEnumerator Test_MultipleSubscribers()
    {
        GameObject sub1Obj = new GameObject("Subscriber1");
        TestEventSubscriber sub1 = sub1Obj.AddComponent<TestEventSubscriber>();

        GameObject sub2Obj = new GameObject("Subscriber2");
        TestEventSubscriber sub2 = sub2Obj.AddComponent<TestEventSubscriber>();

        yield return null; // Wait for subscribers to subscribe

        turnManager.StartSimulation();
        yield return null;

        Assert.IsTrue(sub1.SimulationStartReceived, "Subscriber 1 should receive event");
        Assert.IsTrue(sub2.SimulationStartReceived, "Subscriber 2 should receive event");

        Object.Destroy(sub1Obj);
        Object.Destroy(sub2Obj);
    }

    /// <summary>
    /// Test helper class that extends TurnEventSubscriber.
    /// </summary>
    private class TestEventSubscriber : TurnEventSubscriber
    {
        public bool TurnStartReceived;
        public bool CommandStartReceived;
        public bool SimulationStartReceived;
        public bool SimulationEndReceived;
        public bool TurnEndReceived;
        public int LastTurnNumber;

        protected override void HandleTurnStart(int turnNumber)
        {
            TurnStartReceived = true;
            LastTurnNumber = turnNumber;
        }

        protected override void HandleCommandPhaseStart()
        {
            CommandStartReceived = true;
        }

        protected override void HandleSimulationPhaseStart()
        {
            SimulationStartReceived = true;
        }

        protected override void HandleSimulationPhaseEnd()
        {
            SimulationEndReceived = true;
        }

        protected override void HandleTurnEnd(int completedTurnNumber)
        {
            TurnEndReceived = true;
            LastTurnNumber = completedTurnNumber;
        }
    }
}
