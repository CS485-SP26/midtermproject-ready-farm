using UnityEngine;
using Farming;

namespace Character
{
    public class RaycastSelector : TileSelector
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private LayerMask tileMask;

        public override bool TryGetTile(out FarmTile tile)
        {
            tile = null;
            if (!cam) cam = Camera.main;

            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, tileMask))
            {
                tile = hit.collider.GetComponentInParent<FarmTile>();
                return tile != null;
            }
            return false;
        }
    }
}
