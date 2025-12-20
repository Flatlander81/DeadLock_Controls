using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime test controller for weapon firing queue integration.
/// Provides OnGUI controls for testing weapon queuing and execution.
/// </summary>
public class WeaponFiringTestController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Ship playerShip;
    [SerializeField] private Ship targetShip;
    [SerializeField] private WeaponManager weaponManager;

    [Header("UI Settings")]
    [SerializeField] private bool showGUI = true;
    [SerializeField] private float guiWidth = 350f;

    // Event log
    private List<string> eventLog = new List<string>();
    private const int MaxLogEntries = 20;
    private Vector2 logScrollPosition;
    private Vector2 queueScrollPosition;

    private void Start()
    {
        // Auto-find references if not set
        if (playerShip == null)
        {
            playerShip = FindFirstObjectByType<Ship>();
        }

        if (playerShip != null && weaponManager == null)
        {
            weaponManager = playerShip.GetComponent<WeaponManager>();
        }

        if (targetShip == null)
        {
            // Find any ship that isn't the player
            foreach (var ship in FindObjectsByType<Ship>(FindObjectsSortMode.None))
            {
                if (ship != playerShip)
                {
                    targetShip = ship;
                    break;
                }
            }
        }

        // Subscribe to events
        SubscribeToEvents();

        LogEvent("Weapon Firing Test Controller initialized");
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (WeaponFiringQueue.Instance != null)
        {
            WeaponFiringQueue.Instance.OnCommandQueued += HandleCommandQueued;
            WeaponFiringQueue.Instance.OnCommandCancelled += HandleCommandCancelled;
            WeaponFiringQueue.Instance.OnQueueCleared += HandleQueueCleared;
            WeaponFiringQueue.Instance.OnQueueExecutionStarted += HandleExecutionStarted;
            WeaponFiringQueue.Instance.OnQueueExecutionCompleted += HandleExecutionCompleted;
            WeaponFiringQueue.Instance.OnCommandExecuted += HandleCommandExecuted;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnStart += HandleTurnStart;
            TurnManager.Instance.OnCommandPhaseStart += HandleCommandPhase;
            TurnManager.Instance.OnSimulationPhaseStart += HandleSimulationPhase;
            TurnManager.Instance.OnTurnEnd += HandleTurnEnd;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (WeaponFiringQueue.Instance != null)
        {
            WeaponFiringQueue.Instance.OnCommandQueued -= HandleCommandQueued;
            WeaponFiringQueue.Instance.OnCommandCancelled -= HandleCommandCancelled;
            WeaponFiringQueue.Instance.OnQueueCleared -= HandleQueueCleared;
            WeaponFiringQueue.Instance.OnQueueExecutionStarted -= HandleExecutionStarted;
            WeaponFiringQueue.Instance.OnQueueExecutionCompleted -= HandleExecutionCompleted;
            WeaponFiringQueue.Instance.OnCommandExecuted -= HandleCommandExecuted;
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnStart -= HandleTurnStart;
            TurnManager.Instance.OnCommandPhaseStart -= HandleCommandPhase;
            TurnManager.Instance.OnSimulationPhaseStart -= HandleSimulationPhase;
            TurnManager.Instance.OnTurnEnd -= HandleTurnEnd;
        }
    }

    #region Event Handlers

    private void HandleCommandQueued(WeaponFireCommand cmd)
    {
        LogEvent($"QUEUED: {cmd}");
    }

    private void HandleCommandCancelled(WeaponFireCommand cmd)
    {
        LogEvent($"CANCELLED: {cmd}");
    }

    private void HandleQueueCleared()
    {
        LogEvent("Queue cleared");
    }

    private void HandleExecutionStarted()
    {
        LogEvent("=== EXECUTION STARTED ===");
    }

    private void HandleExecutionCompleted()
    {
        LogEvent("=== EXECUTION COMPLETED ===");
    }

    private void HandleCommandExecuted(WeaponFireCommand cmd, bool success)
    {
        string status = success ? "FIRED" : "FAILED";
        LogEvent($"{status}: {cmd}");
    }

    private void HandleTurnStart(int turn)
    {
        LogEvent($"--- Turn {turn} Started ---");
    }

    private void HandleCommandPhase()
    {
        LogEvent("Phase: COMMAND");
    }

    private void HandleSimulationPhase()
    {
        LogEvent("Phase: SIMULATION");
    }

    private void HandleTurnEnd(int turn)
    {
        LogEvent($"--- Turn {turn} Ended ---");
    }

    #endregion

    private void LogEvent(string message)
    {
        string timestamp = Time.time.ToString("F2");
        eventLog.Add($"[{timestamp}] {message}");

        while (eventLog.Count > MaxLogEntries)
        {
            eventLog.RemoveAt(0);
        }
    }

    private void OnGUI()
    {
        if (!showGUI) return;

        float panelHeight = Screen.height - 20;
        GUILayout.BeginArea(new Rect(10, 10, guiWidth, panelHeight));
        GUILayout.BeginVertical("box");

        // Title
        GUILayout.Label("=== WEAPON FIRING TEST ===", GUI.skin.box);

        // Status section
        DrawStatusSection();

        GUILayout.Space(10);

        // Queue controls
        DrawQueueControls();

        GUILayout.Space(10);

        // Turn controls
        DrawTurnControls();

        GUILayout.Space(10);

        // Queue display
        DrawQueueDisplay();

        GUILayout.Space(10);

        // Heat preview
        DrawHeatPreview();

        GUILayout.Space(10);

        // Event log
        DrawEventLog();

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawStatusSection()
    {
        GUILayout.Label("STATUS", GUI.skin.box);

        string phase = TurnManager.Instance != null ? TurnManager.Instance.CurrentPhase.ToString() : "N/A";
        int turn = TurnManager.Instance != null ? TurnManager.Instance.CurrentTurn : 0;
        string stage = CombatCoordinator.Instance != null ? CombatCoordinator.Instance.GetStageDescription() : "N/A";

        GUILayout.Label($"Turn: {turn} | Phase: {phase}");
        GUILayout.Label($"Stage: {stage}");

        if (WeaponFiringQueue.Instance != null)
        {
            GUILayout.Label($"Queue: {WeaponFiringQueue.Instance.QueuedCount} commands");
            GUILayout.Label($"Executing: {WeaponFiringQueue.Instance.IsExecuting}");
        }
    }

    private void DrawQueueControls()
    {
        GUILayout.Label("QUEUE CONTROLS", GUI.skin.box);

        bool canQueue = TurnManager.Instance != null && TurnManager.Instance.CurrentPhase == TurnPhase.Command;

        GUI.enabled = canQueue && weaponManager != null && targetShip != null;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Queue Group 1"))
        {
            int queued = weaponManager.QueueFireGroup(1, targetShip);
            LogEvent($"Queued Group 1: {queued} weapons");
        }
        if (GUILayout.Button("Queue Group 2"))
        {
            int queued = weaponManager.QueueFireGroup(2, targetShip);
            LogEvent($"Queued Group 2: {queued} weapons");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Queue Group 3"))
        {
            int queued = weaponManager.QueueFireGroup(3, targetShip);
            LogEvent($"Queued Group 3: {queued} weapons");
        }
        if (GUILayout.Button("Queue Group 4"))
        {
            int queued = weaponManager.QueueFireGroup(4, targetShip);
            LogEvent($"Queued Group 4: {queued} weapons");
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Queue Alpha Strike"))
        {
            int queued = weaponManager.QueueAlphaStrike(targetShip);
            LogEvent($"Queued Alpha Strike: {queued} weapons");
        }

        GUI.enabled = WeaponFiringQueue.Instance != null && WeaponFiringQueue.Instance.QueuedCount > 0;

        if (GUILayout.Button("Clear Queue"))
        {
            WeaponFiringQueue.Instance?.ClearQueue();
            weaponManager?.ClearQueuedState();
        }

        GUI.enabled = true;
    }

    private void DrawTurnControls()
    {
        GUILayout.Label("TURN CONTROLS", GUI.skin.box);

        bool isCommand = TurnManager.Instance != null && TurnManager.Instance.CurrentPhase == TurnPhase.Command;
        bool isSimulating = TurnManager.Instance != null && TurnManager.Instance.CurrentPhase == TurnPhase.Simulation;

        GUI.enabled = isCommand;
        if (GUILayout.Button("Execute Turn (Start Simulation)"))
        {
            TurnManager.Instance?.StartSimulation();
        }

        GUI.enabled = isSimulating;
        if (GUILayout.Button("Force End Turn"))
        {
            TurnManager.Instance?.ForceEndTurn();
        }

        GUI.enabled = true;
    }

    private void DrawQueueDisplay()
    {
        GUILayout.Label("QUEUED COMMANDS", GUI.skin.box);

        if (WeaponFiringQueue.Instance == null)
        {
            GUILayout.Label("(No WeaponFiringQueue)");
            return;
        }

        var commands = WeaponFiringQueue.Instance.QueuedCommands;

        if (commands.Count == 0)
        {
            GUILayout.Label("(Queue empty)");
            return;
        }

        queueScrollPosition = GUILayout.BeginScrollView(queueScrollPosition, GUILayout.Height(100));

        foreach (var cmd in commands)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"  {cmd}", GUILayout.Width(guiWidth - 80));
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                weaponManager?.CancelQueuedWeapon(cmd.Weapon);
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
    }

    private void DrawHeatPreview()
    {
        GUILayout.Label("HEAT PREVIEW", GUI.skin.box);

        if (playerShip == null || playerShip.HeatManager == null)
        {
            GUILayout.Label("(No HeatManager)");
            return;
        }

        float currentHeat = playerShip.HeatManager.CurrentHeat;
        float maxHeat = playerShip.HeatManager.MaxHeat;
        int queuedHeat = weaponManager != null ? weaponManager.GetQueuedHeatCost() : 0;
        float projectedHeat = currentHeat + queuedHeat;

        GUILayout.Label($"Current: {currentHeat:F0} / {maxHeat:F0}");
        GUILayout.Label($"Queued Heat Cost: +{queuedHeat}");

        // Color warning if projected heat is high
        Color originalColor = GUI.color;
        if (projectedHeat > maxHeat * 0.8f)
        {
            GUI.color = Color.red;
        }
        else if (projectedHeat > maxHeat * 0.5f)
        {
            GUI.color = Color.yellow;
        }

        GUILayout.Label($"Projected: {projectedHeat:F0}");
        GUI.color = originalColor;

        // Heat bar
        float heatPercent = projectedHeat / maxHeat;
        GUILayout.BeginHorizontal();
        GUILayout.Label("Heat:", GUILayout.Width(40));
        Rect barRect = GUILayoutUtility.GetRect(guiWidth - 60, 20);

        // Background
        GUI.Box(barRect, "");

        // Current heat bar
        float currentPercent = currentHeat / maxHeat;
        Rect currentRect = new Rect(barRect.x, barRect.y, barRect.width * currentPercent, barRect.height);
        GUI.color = Color.yellow;
        GUI.Box(currentRect, "");

        // Projected addition
        if (queuedHeat > 0)
        {
            float addPercent = queuedHeat / maxHeat;
            Rect addRect = new Rect(barRect.x + barRect.width * currentPercent, barRect.y,
                                    barRect.width * addPercent, barRect.height);
            GUI.color = new Color(1f, 0.5f, 0f, 0.7f); // Orange
            GUI.Box(addRect, "");
        }

        GUI.color = originalColor;
        GUILayout.EndHorizontal();
    }

    private void DrawEventLog()
    {
        GUILayout.Label("EVENT LOG", GUI.skin.box);

        logScrollPosition = GUILayout.BeginScrollView(logScrollPosition, GUILayout.Height(150));

        foreach (string entry in eventLog)
        {
            GUILayout.Label(entry);
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("Clear Log"))
        {
            eventLog.Clear();
        }
    }
}
