using UnityEngine;

public class EnemyTouchTrigger : MonoBehaviour
{
    [Header("References")]
    // Reference to the enemy brain that should react when the player touches this trigger.
    [SerializeField] private EnemyBrain enemieBrain;

    [Header("Debug")]
    // Enables debug logs when the player enters the touch trigger.
    [SerializeField] private bool debugLogTouch = true;

    private void Awake()
    {
        // Auto-assign the enemy brain from the parent object if not set manually.
        if (enemieBrain == null)
            enemieBrain = GetComponentInParent<EnemyBrain>();
    }

    private void Reset()
    {
        // Rebuild the reference automatically when Reset is used in the Inspector.
        enemieBrain = GetComponentInParent<EnemyBrain>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Stop if no enemy brain reference is available.
        if (enemieBrain == null)
            return;

        // Try to find the player controller on the entered collider.
        PlayerController player = other.GetComponent<PlayerController>();

        // If not found directly, also check parent objects.
        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        // Ignore all non-player collisions.
        if (player == null)
            return;

        if (debugLogTouch)
            Debug.Log($"{name}: Player touched enemy trigger.");

        // Trigger the enemy's rear search reaction.
        enemieBrain.TriggerRearSearchFromTouch();
    }
}