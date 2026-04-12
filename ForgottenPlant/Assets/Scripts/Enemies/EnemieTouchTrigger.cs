using UnityEngine;

public class EnemyTouchTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemieBrain enemieBrain;

    [Header("Debug")]
    [SerializeField] private bool debugLogTouch = true;

    private void Awake()
    {
        if (enemieBrain == null)
            enemieBrain = GetComponentInParent<EnemieBrain>();
    }

    private void Reset()
    {
        enemieBrain = GetComponentInParent<EnemieBrain>();
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
