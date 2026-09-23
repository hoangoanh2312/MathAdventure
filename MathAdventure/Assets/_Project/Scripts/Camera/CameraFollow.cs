using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0.01f)] private float smoothTime = 0.2f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private bool limitToBounds = true;
    [SerializeField] private Vector2 minimumPosition = new Vector2(-7f, -4f);
    [SerializeField] private Vector2 maximumPosition = new Vector2(7f, 4f);

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        if (limitToBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minimumPosition.x, maximumPosition.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minimumPosition.y, maximumPosition.y);
        }

        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime,
            Mathf.Infinity,
            Time.deltaTime);

        smoothedPosition.z = offset.z;
        transform.position = smoothedPosition;
    }

    public void Configure(
        Transform followTarget,
        float followSmoothTime,
        Vector3 followOffset,
        Vector2 minimum,
        Vector2 maximum)
    {
        target = followTarget;
        smoothTime = followSmoothTime;
        offset = followOffset;
        limitToBounds = true;
        minimumPosition = minimum;
        maximumPosition = maximum;
    }
}
