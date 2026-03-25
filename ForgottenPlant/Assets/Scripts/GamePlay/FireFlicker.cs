using UnityEngine;

public class FireFlicker : MonoBehaviour
{
    public float minIntensity = 2f;
    public float maxIntensity = 5f;

    private Light fireLight;

    private void Start()
    {
        fireLight = GetComponent<Light>();
    }

    private void Update()
    {
        fireLight.intensity = Random.Range(minIntensity, maxIntensity);
    }
}
