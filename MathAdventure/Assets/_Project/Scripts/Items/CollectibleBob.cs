using UnityEngine;

public class CollectibleBob : MonoBehaviour
{
    [SerializeField, Min(0f)] private float amplitude = 0.1f;
    [SerializeField, Min(0f)] private float frequency = 2.5f;

    private Vector3 origin;

    private void OnEnable()
    {
        origin = transform.localPosition;
    }

    private void Update()
    {
        transform.localPosition = origin + Vector3.up * (Mathf.Sin(Time.time * frequency) * amplitude);
    }
}
