using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime test controller for Turn System events.
/// Shows current phase, turn number, event log, and manual controls.
/// </summary>
public class TurnSystemTestController : TurnEventSubscriber
{
    [Header("UI Settings")]
    [SerializeField] private bool showUI = true;
    [SerializeField] private int maxLogEntries = 20;

    // Event log
    private List<string> eventLog = new List<string>();
    private Vector2 scrollPosition;

    // Cached state
    private string currentPhaseDisplay = "Unknown";
    private int currentTurn = 0;
    private float simulationProgress = 0f;
    private string coordinatorStage = "N/A";

    private void Start()
    {
        LogEvent("TurnSystemTestController initialized");
    }

    #region Event Handlers

    protected override void HandleTurnStart(int turnNumber)
    {
        currentTurn = turnNumber;
        LogEvent($"EVENT: OnTurnStart (Turn {turnNumber})");
    }

    protected override void HandleCommandPhaseStart()
    {
        currentPhaseDisplay = "COMMAND";
        LogEvent("EVENT: OnCommandPhaseStart");
    }

    protected override void HandleSimulationPhaseStart()
    {
        currentPhaseDisplay = "SIMULATION";
        LogEvent("EVENT: OnSimulationPhaseStart");
    }

    protected override void HandleSimulationPhaseEnd()
    {
        LogEvent("EVENT: OnSimulationPhaseEnd");
    }

    protected override void HandleTurnEnd(int completedTurnNumber)
    {
        LogEvent($"EVENT: OnTurnEnd (Turn {completedTurnNumber} complete)");
    }

    protected override void HandleSimulationProgress(float progress)
    {
        simulationProgress = progress;
        // Don't log progress updates - too spammy
    }

    #endregion

    private void Update()
    {
        // Update cached state
        if (TurnManager.Instance != null)
        {
            currentPhaseDisplay = TurnManager.Instance.CurrentPhase.ToString().ToUpper();
            currentTurn = TurnManager.Instance.CurrentTurn;
            simulationProgress = TurnManager.Instance.SimulationProgress;
        }

        if (CombatCoordinator.Instance != null)
        {
            coordinatorStage = CombatCoordinator.Instance.GetStageDescription();
        }

        // Keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (TurnManager.Instance != null && TurnManager.Instance.IsCommandPhase)
            {
                TurnManager.Instance.StartSimulation();
                LogEvent("USER: Started simulation (Space)");
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.ForceEndTurn();
                LogEvent("USER: Force ended turn (F)");
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            eventLog.Clear();
            LogEvent("Log cleared");
        }
    }

    private void LogEvent(string message)
    {
        string timestamp = Time.time.ToString("F2");
        eventLog.Add($"[{timestamp}] {message}");

        // Trim log if too long
        while (eventLog.Count > maxLogEntries)
        {
            eventLog.RemoveAt(0);
        }

        Debug.Log($"[TurnSystemTest] {message}");
    }

    private void OnGUI()
    {
        if (!showUI) return;

        // Main panel
        GUILayout.BeginArea(new Rect(10, 10, 350, 500));
        GUILayout.BeginVertical("box");

        // Header
        GUILayout.Label("=== TURN SYSTEM TEST ===", GetHeaderStyle());

        // Current State
        GUILayout.Space(10);
        GUILayout.Label($"Turn: {currentTurn}", GetInfoStyle());
        GUILayout.Label($"Phase: {currentPhaseDisplay}", GetPhaseStyle(currentPhaseDisplay));

        // Simulation Progress
        if (TurnManager.Instance != null && TurnManager.Instance.IsSimulationPhase)
        {
            GUILayout.Label($"Progress: {simulationProgress * 100:F0}%");

            // Progress bar
            Rect progressRect = GUILayoutUtility.GetRect(300, 20);
            GUI.Box(progressRect, "");
            GUI.color = Color.green;
            GUI.Box(new Rect(progressRect.x, progressRect.y, progressRect.width * simulationProgress, progressRect.height), "");
            GUI.color = Color.white;
        }

        // Combat Coordinator State
        GUILayout.Space(5);
        GUILayout.Label($"Stage: {coordinatorStage}", GetInfoStyle());

        // Controls
        GUILayout.Space(15);
        GUILayout.Label("--- CONTROLS ---", GetHeaderStyle());

        if (TurnManager.Instance != null)
        {
            if (TurnManager.Instance.IsCommandPhase)
            {
                if (GUILayout.Button("Start Simulation (SPACE)", GUILayout.Height(30)))
                {
                    TurnManager.Instance.StartSimulation();
                    LogEvent("USER: Started simulation (button)");
                }
            }
            else
            {
                GUI.enabled = false;
                GUILayout.Button("Simulation in progress...", GUILayout.Height(30));
                GUI.enabled = true;
            }

            if (GUILayout.Button("Force End Turn (F)", GUILayout.Height(25)))
            {
                TurnManager.Instance.ForceEndTurn();
                LogEvent("USER: Force ended turn (button)");
            }
        }
        else
        {
            GUILayout.Label("TurnManager not found!", GetErrorStyle());
        }

        if (GUILayout.Button("Clear Log (C)", GUILayout.Height(25)))
        {
            eventLog.Clear();
            LogEvent("Log cleared");
        }

        // Event Log
        GUILayout.Space(15);
        GUILayout.Label("--- EVENT LOG ---", GetHeaderStyle());

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        foreach (string entry in eventLog)
        {
            GUILayout.Label(entry, GetLogStyle(entry));
        }
        GUILayout.EndScrollView();

        GUILayout.EndVertical();
        GUILayout.EndArea();

        // Help panel (right side)
        GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 150));
        GUILayout.BeginVertical("box");
        GUILayout.Label("KEYBOARD SHORTCUTS", GetHeaderStyle());
        GUILayout.Label("SPACE - Start Simulation");
        GUILayout.Label("F - Force End Turn");
        GUILayout.Label("C - Clear Event Log");
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    #region GUI Styles

    private GUIStyle GetHeaderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    }

    private GUIStyle GetInfoStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 14;
        return style;
    }

    private GUIStyle GetPhaseStyle(string phase)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 18;
        style.fontStyle = FontStyle.Bold;

        if (phase == "COMMAND")
        {
            style.normal.textColor = Color.cyan;
        }
        else if (phase == "SIMULATION")
        {
            style.normal.textColor = Color.yellow;
        }
        else if (phase == "TURNEND")
        {
            style.normal.textColor = Color.green;
        }

        return style;
    }

    private GUIStyle GetLogStyle(string entry)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 10;
        style.wordWrap = true;

        if (entry.Contains("OnTurnStart"))
        {
            style.normal.textColor = Color.cyan;
        }
        else if (entry.Contains("OnSimulationPhaseStart"))
        {
            style.normal.textColor = Color.yellow;
        }
        else if (entry.Contains("OnTurnEnd"))
        {
            style.normal.textColor = Color.green;
        }
        else if (entry.Contains("USER"))
        {
            style.normal.textColor = Color.white;
        }

        return style;
    }

    private GUIStyle GetErrorStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.normal.textColor = Color.red;
        style.fontStyle = FontStyle.Bold;
        return style;
    }

    #endregion
}
