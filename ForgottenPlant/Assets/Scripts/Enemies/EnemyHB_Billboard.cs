using UnityEngine;

public class EnemyHB_Billboard : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null)
            return;

        Vector3 direction = cam.transform.position - transform.position;

        direction.y = 0f;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}
