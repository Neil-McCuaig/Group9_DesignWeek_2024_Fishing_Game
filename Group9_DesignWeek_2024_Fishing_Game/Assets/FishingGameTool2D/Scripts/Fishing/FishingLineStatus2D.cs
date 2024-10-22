using System;
using FishingGameTool2D.CustomAttribute;

namespace FishingGameTool2D.Fishing.Line 
{ 
    [Serializable]
    public class FishingLineStatus2D
    {
        public float _maxLineLoad;
        public float _overLoadDuration;
        public float _maxLineLength;

        [ReadOnlyField]
        public float _currentLineLength;
        [ReadOnlyField]
        public float _currentOverLoad;
        [ReadOnlyField]
        public float _currentLineLoad;
        [ReadOnlyField]
        public float _attractFloatSpeed;
        [ReadOnlyField]
        public bool _isLineBroken;
    }
}
