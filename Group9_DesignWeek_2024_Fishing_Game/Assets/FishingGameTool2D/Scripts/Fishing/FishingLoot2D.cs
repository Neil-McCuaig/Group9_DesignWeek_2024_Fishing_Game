using FishingGameTool2D.Fishing.LootData;
using System.Collections.Generic;
using UnityEngine;

namespace FishingGameTool2D.Fishing.Loot
{
    [AddComponentMenu("Fishing Game Tool 2D/Fishing Loot 2D")]
    public class FishingLoot2D : MonoBehaviour
    {
        public List<FishingLootData> _fishingLoot = new List<FishingLootData>();
        

        /// <summary>
        /// Returns the list of fishing loot that can be caught.
        /// </summary>
        /// <returns>List of FishingLootData representing potential loot obtainable while fishing.</returns>
        public List<FishingLootData> GetFishingLoot() 
        { 
            return _fishingLoot; 
        }
    }
}
