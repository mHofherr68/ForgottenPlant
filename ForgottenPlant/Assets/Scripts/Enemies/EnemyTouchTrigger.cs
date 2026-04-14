using UnityEngine;

public class EnemyTouchTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyBrain enemieBrain;

    [Header("Debug")]
    [SerializeField] private bool debugLogTouch = true;

    private void Awake()
    {
        if (enemieBrain == null)
            enemieBrain = GetComponentInParent<EnemyBrain>();
    }

    private void Reset()
    {
        enemieBrain = GetComponentInParent<EnemyBrain>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemieBrain == null)
            return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
            player = other.GetComponentInParent<PlayerController>();

        if (player == null)
            return;

        if (debugLogTouch)
            Debug.Log($"{name}: Player touched enemy trigger.");

        enemieBrain.TriggerRearSearchFromTouch();
    }
}
