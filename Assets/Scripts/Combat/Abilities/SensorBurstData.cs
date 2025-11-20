using UnityEngine;

/// <summary>
/// Sensor burst ability - reveals enemy positions and targeting data.
/// Create via: Assets → Create → Abilities → Sensor Burst
/// </summary>
[CreateAssetMenu(fileName = "SensorBurst", menuName = "Abilities/Sensor Burst")]
public class SensorBurstData : AbilityData
{
    [Header("Sensor Burst Settings")]
    [SerializeField] private float detectionRadius = 100f;
    [SerializeField] private int durationTurns = 2;

    public override void Execute(Ship ship)
    {
        Debug.Log($"{ship.gameObject.name} activated Sensor Burst! Detecting enemies within {detectionRadius} units for {durationTurns} turns");

        // Find all ships within radius
        Collider[] hitColliders = Physics.OverlapSphere(ship.transform.position, detectionRadius);
        int enemiesDetected = 0;

        foreach (var hitCollider in hitColliders)
        {
            Ship enemyShip = hitCollider.GetComponent<Ship>();
            if (enemyShip != null && enemyShip != ship)
            {
                enemiesDetected++;
                Debug.Log($"Detected: {enemyShip.gameObject.name} at {enemyShip.transform.position}");
            }
        }

        Debug.Log($"Total enemies detected: {enemiesDetected}");
    }
}
