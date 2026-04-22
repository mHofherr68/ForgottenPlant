using UnityEngine;

public class PersistentManagers : MonoBehaviour
{
    // Stores the single persistent instance of this manager object.
    private static PersistentManagers instance;

    private void Awake()
    {
        // Enforce singleton behavior and destroy duplicate instances.
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Store this instance and keep it alive across scene changes.
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}