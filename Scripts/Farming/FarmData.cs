using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Farming {

    [System.Serializable]
    public struct TileData
    {
        public FarmTile.Condition condition;
        
        
    }
    [CreateAssetMenu(fileName = "FarmData", menuName = "Farming/FarmData")]
    public class FarmData : ScriptableObject
    {
        public List<TileData> savedTiles = new List<TileData> (); 
        public void Clear() => savedTiles.Clear(); 
    }
}
