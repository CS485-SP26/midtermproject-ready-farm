using UnityEngine;
using Farming;

namespace Character 
{
    public class TileSelector : MonoBehaviour
    {
        [SerializeField] protected FarmTile activeTile; // good for debugging
        public FarmTile GetSelectedTile() { return activeTile; }

        protected void SetActiveTile(FarmTile tile)
        {
            if (activeTile != tile)
            {
                activeTile?.SetHighlight(false);
                activeTile = tile;
                activeTile?.SetHighlight(true);
            }
        }
    }
}
