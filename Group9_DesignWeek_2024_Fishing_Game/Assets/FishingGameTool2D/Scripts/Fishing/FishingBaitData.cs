using UnityEngine;
using FishingGameTool2D.CustomAttribute;

namespace FishingGameTool2D.Fishing.BaitData
{
    public enum BaitTier
    {
        Uncommon,
        Rare,
        Epic,
        Legendary
    };

    [CreateAssetMenu(fileName = "Fishing Bait Data", menuName = "Fishing Game Tool 2D/New Fishing Bait")]
    public class FishingBaitData : ScriptableObject
    {
        [BetterHeader("Fishing Bait Settings", 20), Space]
        public BaitTier _baitTier;
        public string _baitName;
        [TextArea(10,40)]
        public string _baitDescription;
        public GameObject _baitPrefab;
    }
}
