using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform target;
    public float height = 20f;
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Follow X and Z, keep fixed height
        Vector3 targetPosition = target.position;
        targetPosition.y = height;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        // Lock rotation (North up)
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}