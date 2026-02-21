using UnityEngine;
using Farming;

namespace Character
{
    public class PointSelector : TileSelector
    {
        public override bool TryGetTile(out FarmTile tile)
        {
            tile = activeTile;
            return tile != null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<FarmTile>(out FarmTile tile))
            {
                SetActiveTile(tile);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<FarmTile>(out FarmTile tile))
            {
                if (activeTile == tile)
                    SetActiveTile(null);
            }
        }
    }
}
