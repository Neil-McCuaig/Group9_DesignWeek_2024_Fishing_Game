using UnityEngine;
using FishingGameTool2D.CustomAttribute;

namespace FishingGameTool2D.Fishing.CatchProbability
{
    [CreateAssetMenu(fileName = "Catch Probability Data", menuName = "Fishing Game Tool 2D/New Catch Probability Data")]
    public class CatchProbabilityData : ScriptableObject
    {
        [InfoBox("This stores custom probability values for different bait tier types, representing the likelihood of successfully catching loot (fish or other objects) in the game.")]
        public int _commonProbability = 5;
        public int _uncommonProbability = 12;
        public int _rareProbability = 22;
        public int _epicProbability = 35;
        public int _legendaryProbability = 45;
        [Space, InfoBox("Factor influencing the catch chance based on the minimum safe fishing distance.")]
        public float _minSafeFishingDistanceFactor = 5f;
    }
}
