using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
 [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Header("Boundary Settings (Optional)")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private void LateUpdate()
    {
        if (target == null)
        {
     Debug.LogWarning("CameraFollow: No target assigned!");
         return;
        }

 // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
   // Apply boundaries if enabled
        if (useBoundaries)
        {
     desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
     desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }
        
        // Smoothly interpolate to desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
// Always keep the original Z position (important for 2D camera)
     smoothedPosition.z = offset.z;
        
   transform.position = smoothedPosition;
    }

    // Helper method to set target at runtime
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Gizmos to visualize boundaries in editor
    private void OnDrawGizmosSelected()
    {
        if (!useBoundaries) return;

 Gizmos.color = Color.yellow;
        Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);
        Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);
        Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
   Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
    }
}
