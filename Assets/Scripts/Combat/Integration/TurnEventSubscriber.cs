using UnityEngine;

/// <summary>
/// Base class for components that need to respond to turn system events.
/// Subclasses override only the event handlers they need.
/// Automatically subscribes on enable and unsubscribes on disable.
/// </summary>
public abstract class TurnEventSubscriber : MonoBehaviour
{
    /// <summary>
    /// Subscribe to TurnManager events when enabled.
    /// </summary>
    protected virtual void OnEnable()
    {
        if (TurnManager.Instance != null)
        {
            SubscribeToEvents();
        }
        else
        {
            // TurnManager might not exist yet, try again in Start
            StartCoroutine(SubscribeWhenReady());
        }
    }

    /// <summary>
    /// Unsubscribe from TurnManager events when disabled.
    /// </summary>
    protected virtual void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// Wait for TurnManager to be ready then subscribe.
    /// </summary>
    private System.Collections.IEnumerator SubscribeWhenReady()
    {
        // Wait a frame for TurnManager to initialize
        yield return null;

        if (TurnManager.Instance != null)
        {
            SubscribeToEvents();
        }
        else
        {
            Debug.LogWarning($"{GetType().Name}: TurnManager not found. Turn events will not be received.");
        }
    }

    /// <summary>
    /// Subscribe to all TurnManager events.
    /// </summary>
    private void SubscribeToEvents()
    {
        TurnManager tm = TurnManager.Instance;
        if (tm == null) return;

        tm.OnTurnStart += HandleTurnStart;
        tm.OnCommandPhaseStart += HandleCommandPhaseStart;
        tm.OnSimulationPhaseStart += HandleSimulationPhaseStart;
        tm.OnSimulationPhaseEnd += HandleSimulationPhaseEnd;
        tm.OnTurnEnd += HandleTurnEnd;
        tm.OnSimulationProgress += HandleSimulationProgress;
    }

    /// <summary>
    /// Unsubscribe from all TurnManager events.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        TurnManager tm = TurnManager.Instance;
        if (tm == null) return;

        tm.OnTurnStart -= HandleTurnStart;
        tm.OnCommandPhaseStart -= HandleCommandPhaseStart;
        tm.OnSimulationPhaseStart -= HandleSimulationPhaseStart;
        tm.OnSimulationPhaseEnd -= HandleSimulationPhaseEnd;
        tm.OnTurnEnd -= HandleTurnEnd;
        tm.OnSimulationProgress -= HandleSimulationProgress;
    }

    #region Virtual Event Handlers

    /// <summary>
    /// Called when a new turn starts, before Command Phase.
    /// Override to handle turn start logic.
    /// </summary>
    /// <param name="turnNumber">The new turn number (1-indexed)</param>
    protected virtual void HandleTurnStart(int turnNumber) { }

    /// <summary>
    /// Called when Command Phase begins.
    /// Override to enable planning controls.
    /// </summary>
    protected virtual void HandleCommandPhaseStart() { }

    /// <summary>
    /// Called when Simulation Phase begins.
    /// Override to execute queued actions.
    /// </summary>
    protected virtual void HandleSimulationPhaseStart() { }

    /// <summary>
    /// Called when Simulation Phase ends, before cleanup.
    /// Override to finalize simulation results.
    /// </summary>
    protected virtual void HandleSimulationPhaseEnd() { }

    /// <summary>
    /// Called when a turn ends, after cleanup.
    /// Override to handle end-of-turn logic.
    /// </summary>
    /// <param name="completedTurnNumber">The turn number that just completed</param>
    protected virtual void HandleTurnEnd(int completedTurnNumber) { }

    /// <summary>
    /// Called each frame during simulation with progress value.
    /// Override to update progress displays or time-based effects.
    /// </summary>
    /// <param name="progress">Progress from 0 (start) to 1 (complete)</param>
    protected virtual void HandleSimulationProgress(float progress) { }

    #endregion
}
