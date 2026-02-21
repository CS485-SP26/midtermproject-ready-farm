using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Drag X Bot here
    public Vector3 offset = new Vector3(0, 2, -3); // Adjust Z for closeness
    public float smoothness = 0.125f;

    void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position based on target and offset
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move the camera
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothness);

        // Keep the camera looking at the player
        transform.LookAt(target.position + Vector3.up);
    }
}