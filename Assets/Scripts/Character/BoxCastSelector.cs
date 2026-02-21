using UnityEngine;
using Farming;

namespace Character
{
    /// <summary>
    /// Selects tiles using Physics.BoxCast - casts a box shape forward from the player
    /// This is useful for creating a "work area" in front of the player
    /// AI-Generated alternative to PointSelector and RaycastSelector
    /// </summary>
    public class BoxCastSelector : TileSelector
    {
        [SerializeField] private Transform origin;
        [SerializeField] private Vector3 boxSize = new Vector3(1f, 0.5f, 1f);
        [SerializeField] private float maxDistance = 2f;
        [SerializeField] private LayerMask tileMask;
        [SerializeField] private Vector3 forwardOffset = Vector3.forward;

        void Start()
        {
            if (!origin) origin = transform;
        }

        public override bool TryGetTile(out FarmTile tile)
        {
            tile = null;
            if (!origin) origin = transform;

            // Calculate the direction to cast (usually forward)
            Vector3 direction = origin.TransformDirection(forwardOffset.normalized);
            
            // Perform box cast
            if (Physics.BoxCast(
                origin.position, 
                boxSize * 0.5f, 
                direction, 
                out RaycastHit hit, 
                origin.rotation, 
                maxDistance, 
                tileMask))
            {
                tile = hit.collider.GetComponentInParent<FarmTile>();
                return tile != null;
            }
            
            return false;
        }

        // Visualize the box cast in the editor
        void OnDrawGizmosSelected()
        {
            if (!origin) origin = transform;
            
            Vector3 direction = origin.TransformDirection(forwardOffset.normalized);
            
            // Draw the box at start position
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(origin.position, origin.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            
            // Draw the box at end position
            Gizmos.color = Color.red;
            Vector3 endPosition = origin.position + direction * maxDistance;
            Gizmos.matrix = Matrix4x4.TRS(endPosition, origin.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
            
            // Draw line showing the cast
            Gizmos.matrix = Matrix4x4.identity;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin.position, endPosition);
        }
    }
}