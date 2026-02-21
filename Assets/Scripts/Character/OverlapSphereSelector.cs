using UnityEngine;
using Farming;

namespace Character
{
    /// <summary>
    /// Selects tiles using Physics.OverlapSphere - finds the closest tile within a radius
    /// This is useful for controller-based gameplay or when you want a more forgiving selection area
    /// </summary>
    public class OverlapSphereSelector : TileSelector
    {
        [SerializeField] private Transform origin;
        [SerializeField] private float radius = 1.25f;
        [SerializeField] private LayerMask tileMask;

        public override bool TryGetTile(out FarmTile tile)
        {
            tile = null;
            if (!origin) origin = transform;

            // Find all colliders within the sphere
            Collider[] hits = Physics.OverlapSphere(origin.position, radius, tileMask);
            float bestDistance = float.PositiveInfinity;

            // Find the closest tile
            foreach (var collider in hits)
            {
                var farmTile = collider.GetComponentInParent<FarmTile>();
                if (!farmTile) continue;

                float distance = (farmTile.transform.position - origin.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    tile = farmTile;
                }
            }
            
            return tile != null;
        }

        // Optional: Visualize the selection sphere in the editor
        void OnDrawGizmosSelected()
        {
            if (!origin) origin = transform;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin.position, radius);
        }
    }
}