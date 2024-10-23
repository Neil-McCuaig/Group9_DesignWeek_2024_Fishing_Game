using UnityEngine;
using System;
using FishingGameTool2D.CustomAttribute;
using FishingGameTool2D.Fishing.Line;

namespace FishingGameTool2D.Fishing.Rod
{
    [AddComponentMenu("Fishing Game Tool 2D/Fishing Rod 2D")]
    [RequireComponent(typeof(LineRenderer))]
    public class FishingRod2D : MonoBehaviour
    {
        [Serializable]
        public class FishingLineSettings
        {
            public Transform _lineAttachment;
            [InfoBox("This is the range of the number of points for the Line Renderer. It is adjusted based on the distance between the line attachment and the float.")]
            public Vector2 _resolutionRange = new Vector2 { x = 20, y = 10 };
            [Range(-2f, 0f)]
            public float _simulateGravity = -1f;
            [Space]
            public Color _color = new Color32(0, 0, 0, 255);
            public float _width = 0.04f;
        }

        [BetterHeader("Fishing Line Settings", 20)]
        public FishingLineSettings _line;

        [Space, BetterHeader("Fishing Line Status", 20)]
        public FishingLineStatus2D _lineStatus;
        public bool _isLineBreakable = true;

        [Space, BetterHeader("Fishing Rod Settings", 20)]
        public float _baseAttractSpeed = 5f;
        [InfoBox("Fishing Angle is utilized in the calculation of line tension and lure retrieval speed. " +
            "It determines the maximum angle between the rod and the float, which is taken into account during the calculation.")]
        public float _fishingAngle = 60f;

        [Space, AddButton("Show Debug Options", "_showDebugOption")]
        public bool _showDebugOption = false;

        [ShowVariable("_showDebugOption")]
        [Space, BetterHeader("For Debug", 20), InfoBox("The variables below allow you to test the fishing rod during configuration. These variables are modified by the main Fishing System script.")]
        public Transform _fishingFloat;
        [ShowVariable("_showDebugOption")]
        public bool _lootCaught = false;

        #region PRIVATE VARIABLES

        private float _smoothedSimGravity;
        private LineRenderer _fishingLineRenderer;

        #endregion

        private void Awake()
        {
            _fishingLineRenderer = GetComponent<LineRenderer>();

            _fishingLineRenderer.startColor = _line._color;
            _fishingLineRenderer.endColor = _line._color;
            _fishingLineRenderer.startWidth = _line._width;
            _fishingLineRenderer.endWidth = _line._width;
        }

        private void Update()
        {
            FishingLine();
        }

        #region FishingLine

        private void FishingLine()
        {
            if (_lineStatus._isLineBroken || _fishingFloat == null)
            {
                _fishingLineRenderer.positionCount = 0;
                return;
            }

            float distance = Vector2.Distance(_line._lineAttachment.position, _fishingFloat.position);
            int resolution = CalculateLineResolution(distance, _line._resolutionRange);

            _lineStatus._currentLineLength = distance;
            _fishingLineRenderer.positionCount = (int)resolution;

            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)resolution;
                Vector2 position = CalculatePointOnCurve(t, _line._lineAttachment.position, CheckFishingFloatRepresentation(_fishingFloat).position, _lootCaught, _line._simulateGravity);
                _fishingLineRenderer.SetPosition(i, position);
            }
        }

        private Transform CheckFishingFloatRepresentation(Transform fishingFloat)
        {
            if (fishingFloat.childCount != 0)
            {
                return fishingFloat.GetChild(0);
            }

            return fishingFloat;
        }

        private static int CalculateLineResolution(float distance, Vector2 resolutionRange)
        {
            float minDis = 1f;
            float maxDis = 15f;

            float x = Mathf.InverseLerp(minDis, maxDis, distance);
            float value = Mathf.Lerp(resolutionRange.x, resolutionRange.y, x);

            return (int)value;
        }

        private Vector2 CalculatePointOnCurve(float t, Vector2 attachmentPosition, Vector2 floatPosition, bool lootCaught, float simulateGravity)
        {
            Vector2 pointA = attachmentPosition;
            Vector2 pointB = floatPosition;

            float lineTensionSpeed = 2f;
            _smoothedSimGravity = Mathf.Lerp(_smoothedSimGravity, lootCaught == true ? 0f : simulateGravity, Time.deltaTime * lineTensionSpeed);

            Vector2 controlPoint = Vector2.Lerp(pointA, pointB, 0.5f) + Vector2.up * _smoothedSimGravity;
            Vector2 pointOnCurve = CalculateBezier(pointA, controlPoint, pointB, t, floatPosition);

            return pointOnCurve;
        }

        private Vector2 CalculateBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t, Vector2 floatPosition)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector2 point = uuu * p0;
            point += 3 * uu * t * p1;
            point += 3 * u * tt * p2;
            point += ttt * floatPosition;

            return point;
        }

        #endregion

        /// <summary>
        /// Calculates the load status of the fishing line.
        /// </summary>
        /// <param name="attractInput">Determines if the fishing line is being attracted.</param>
        /// <param name="lootWeight">It uses the loot weight to calculate the load on the line.</param>
        /// <returns>A FishingLineStatus object representing the current status of the fishing line.</returns>
        public FishingLineStatus2D CalculateLineLoad(bool attractInput, float lootWeight, int lootTier)
        {
            Vector2 dir = _fishingFloat.position - transform.position;
            float angle = Vector2.Angle(transform.right, dir);
            angle = angle > _fishingAngle ? _fishingAngle : angle;

            if (attractInput)
            {
                float loadDecreaseFactor = 4f;
                float calculatedLootWeight = (lootWeight - (lootWeight / lootTier)) <= 0f ? 1f : (lootWeight - (lootWeight / lootTier));

                _lineStatus._currentLineLoad += ((angle * calculatedLootWeight) * Time.deltaTime) / loadDecreaseFactor;
                _lineStatus._currentLineLoad = _lineStatus._currentLineLoad > _lineStatus._maxLineLoad ? _lineStatus._maxLineLoad : _lineStatus._currentLineLoad;
            }
            else
            {
                _lineStatus._currentOverLoad = 0f;
                float loadReduction = 5f;
                _lineStatus._currentLineLoad -= loadReduction * Time.deltaTime;
                _lineStatus._currentLineLoad = _lineStatus._currentLineLoad < 0f ? 0f : _lineStatus._currentLineLoad;
            }

            if (_lineStatus._currentLineLoad == _lineStatus._maxLineLoad)
            {
                _lineStatus._currentOverLoad += Time.deltaTime;

                if (_lineStatus._currentOverLoad >= _lineStatus._overLoadDuration)
                {
                    if (_isLineBreakable)
                        _lineStatus._isLineBroken = true;

                    FishingSystem2D[] fishingSystem2D = FindObjectsOfType<FishingSystem2D>();

                    if (fishingSystem2D.Length > 1)
                        Debug.LogWarning("There is more than one object on the scene containing the Fishing System 2D component. " +
                            "Please remove the other components containing Fishing System 2D!");
                    else
                        fishingSystem2D[0].ForceStopFishing();
                }
            }

            _lineStatus._attractFloatSpeed = CalculateAttractSpeed(angle, _fishingAngle, _lineStatus._currentLineLoad, _lineStatus._maxLineLoad, _baseAttractSpeed, lootTier);

            return _lineStatus;
        }

        private float CalculateAttractSpeed(float angle, float fishingAngle, float currentLineLoad, float maxLineLoad, float baseAttractSpeed, int lootTier)
        {
            float normalizeAngle = angle / fishingAngle;
            float attractBonus = CalculateAttractBonus(currentLineLoad, maxLineLoad, lootTier);
            float speedBonus = normalizeAngle * attractBonus;
            float attractSpeed = baseAttractSpeed + speedBonus;

            return attractSpeed;
        }

        private static float CalculateAttractBonus(float currentLineLoad, float maxLineLoad, int lootTier)
        {
            float[] attractBonusMultiplier = { 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

            float x = Mathf.InverseLerp(0f, maxLineLoad, currentLineLoad);
            float value = Mathf.Lerp(1f, currentLineLoad * attractBonusMultiplier[lootTier], x);

            return value;
        }

        public void LootCaught(bool value)
        {
            _lootCaught = value;
        }

        public void FinishFishing()
        {
            _lineStatus._attractFloatSpeed = 0f;
            _lineStatus._currentLineLoad = 0f;
            _lineStatus._currentOverLoad = 0f;
            _lootCaught = false;
        }
    }
}
