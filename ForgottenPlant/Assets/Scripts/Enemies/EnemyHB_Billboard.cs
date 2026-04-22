using UnityEngine;

public class EnemyHB_Billboard : MonoBehaviour
{
    // Reference to the main camera the billboard should face.
    private Camera cam;

    private void Start()
    {
        // Cache the main camera once at startup.
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        // Stop if no camera is available.
        if (cam == null)
            return;

        // Calculate the direction from this object to the camera.
        Vector3 direction = cam.transform.position - transform.position;

        // Keep the billboard upright by ignoring vertical difference.
        direction.y = 0f;

        // Rotate the object so it faces the camera on the horizontal axis.
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
