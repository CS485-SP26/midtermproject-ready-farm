using UnityEngine;
using Farming;

namespace Character
{
    public abstract class TileSelector : MonoBehaviour
    {
        [SerializeField] protected FarmTile activeTile;

        public FarmTile GetSelectedTile() => activeTile;

        // Child classes provide a selection method
        public abstract bool TryGetTile(out FarmTile tile);

        protected void SetActiveTile(FarmTile tile)
        {
            if (activeTile != tile)
            {
                activeTile?.SetHighlight(false);
                activeTile = tile;
                activeTile?.SetHighlight(true);
            }
        }

        protected virtual void Update()
        {
            if (TryGetTile(out FarmTile tile))
                SetActiveTile(tile);
            else
                SetActiveTile(null);
        }
    }
}
