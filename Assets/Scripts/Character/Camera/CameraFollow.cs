using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float distance = 5f;   // How far behind
    public float height = 2f;     // How high above
    public float smoothSpeed = 10f;

    void LateUpdate()
    {
        if (!target) return;

        // Calculate position behind player
        Vector3 desiredPosition =
            target.position
            - target.forward * distance
            + Vector3.up * height;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
} 