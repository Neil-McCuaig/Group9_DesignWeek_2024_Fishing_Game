#if UNITY_EDITOR
using FishingGameTool2D.Debuging;
#endif

using FishingGameTool2D.Fishing.LootData;
using System.Collections.Generic;
using UnityEngine;

namespace FishingGameTool2D.Fishing.Float
{
    public class FishingFloatPathfinder2D
    {
        private List<Vector2> _pathPoints = new List<Vector2>();
        private static int _maxPathPoints = 3;
        private bool _initizlizePath = true;
        private Vector2 _moveVector;
        private static float _smoothedSpeed = 10f;
        private Vector2 _initialFloatPosition;

        public void FloatBehavior(FishingLootData lootData, Transform fishingFloatTransform, Vector3 transformPosition, float maxLineLength,
            float finalSpeed, bool attractInput, LayerMask fishingLayer, LayerMask fishingFloatLayer, GameViewType gameViewType)
        {
            Vector2 fishingFloatPosition = fishingFloatTransform.position;

            if (_initizlizePath)
                _initialFloatPosition = fishingFloatPosition;

            Vector2 attractVector = Vector3.zero;
            Vector2 lootVector = Vector3.zero;

            if (lootData != null && lootData._lootType == LootType.Fish)
            {
                // Initialize the path points.
                InitializePath(_pathPoints, _maxPathPoints, fishingFloatPosition, transformPosition, _initialFloatPosition, maxLineLength, fishingLayer, fishingFloatLayer, gameViewType);

                //If the number of points is too small, abort the code.
                if (_pathPoints.Count < 1)
                    return;


                float distanceBetweenPoints = Vector2.Distance(_pathPoints[0], _pathPoints[1]);
                float distanceToNextPoint = Vector2.Distance(fishingFloatPosition, _pathPoints[1]);

                if (distanceToNextPoint < (distanceBetweenPoints / 4f))
                {
                    // Remove the first point and set a new path point.
                    _pathPoints.RemoveAt(0);
                    SetNewPathPoint(_pathPoints, _maxPathPoints, transformPosition, _initialFloatPosition, maxLineLength, fishingFloatLayer, fishingLayer, gameViewType);
                }

                
                lootVector = (_pathPoints[1] - fishingFloatPosition).normalized;

#if UNITY_EDITOR
                Debuging();
#endif
            }

            if (attractInput)
            {
                float attractionStrength = 1.5f;
                attractVector = AttractVector(fishingFloatPosition, transformPosition, _initialFloatPosition, gameViewType).normalized;
                attractVector *= attractionStrength;

                attractVector = AvoidEdgeCollision(attractVector, fishingFloatPosition, fishingLayer, fishingFloatLayer);

                Debug.DrawRay(fishingFloatPosition, attractVector, Color.green);
            }

            Vector2 moveVector = lootVector + attractVector;
            _moveVector = Vector2.Lerp(_moveVector, moveVector, _smoothedSpeed * Time.deltaTime);

            fishingFloatTransform.position += (Vector3)_moveVector * finalSpeed * Time.deltaTime;
        }

        #region AttractDirection

        private Vector2 AttractVector(Vector2 fishingFloatPosition, Vector2 transformPosition, Vector2 initialFloatPosition, GameViewType gameViewType)
        {
            Vector2 direction = (transformPosition - fishingFloatPosition).normalized;

            if (gameViewType == GameViewType.SideView)
            {
                if (fishingFloatPosition.y >= initialFloatPosition.y)
                    direction.y = 0f;
            }

            return direction;
        }

        private Vector2 AvoidEdgeCollision(Vector2 direction, Vector2 fishingFloatPosition, LayerMask fishingLayer, LayerMask fishingFloatLayer)
        {
            int combinedLayerMask = fishingLayer | fishingFloatLayer;

            RaycastHit2D hit2D = Physics2D.Raycast(fishingFloatPosition, direction, 1f, ~combinedLayerMask);

            if (hit2D.collider != null)
            {
                Vector2 waterNormal = (fishingFloatPosition - hit2D.point).normalized;

                Vector2 avoidanceDirUp = new Vector2(-waterNormal.y, waterNormal.x).normalized;
                Vector2 avoidanceDirDown = new Vector2(waterNormal.y, -waterNormal.x).normalized;

                RaycastHit2D hitUp = Physics2D.Raycast(fishingFloatPosition, avoidanceDirUp, 1f, ~combinedLayerMask);
                RaycastHit2D hitDown = Physics2D.Raycast(fishingFloatPosition, avoidanceDirDown, 1f, ~combinedLayerMask);

                Vector2 selectedAvoidanceDir = (hitUp.collider == null || hitDown.distance < hitUp.distance) ? avoidanceDirDown : avoidanceDirUp;

                float avoidanceFactor = 0.7f;

                Vector2 finalDir = direction + selectedAvoidanceDir * avoidanceFactor;
                finalDir = finalDir.normalized;

                return finalDir;
            }

            return direction;
        }

        #endregion

        #region InitizlizePathAndSetNewPathPoint

        private void InitializePath(List<Vector2> pathPoints, int maxPathPoints, Vector2 fishingFloatPosition, Vector2 transformPosition, 
            Vector2 initialFloatPosition, float maxLineLength, LayerMask fishingLayer, LayerMask fishingFloatLayer, GameViewType gameViewType)
        {
            if (!_initizlizePath)
                return;

            if (pathPoints.Count == 0)
            {
                pathPoints.Add(fishingFloatPosition);
            }

            for (int i = 0; i < maxPathPoints; i++)
            {
                Vector2 newPathPoint = GetPathPoint(pathPoints[i], i > 0 ? pathPoints[i - 1] : pathPoints[i], transformPosition,
                    initialFloatPosition, maxLineLength, fishingLayer, fishingFloatLayer, gameViewType);
                pathPoints.Add(newPathPoint);
            }

            _initizlizePath = false;
        }

        private void SetNewPathPoint(List<Vector2> pathpoints, int maxPathPoints, Vector3 transformPosition, Vector2 initialFloatPosition, 
            float maxLineLength, LayerMask fishingLayer, LayerMask fishingFloatLayer, GameViewType gameViewType)
        {
            Vector2 newPathPoint = GetPathPoint(pathpoints[maxPathPoints - 1], pathpoints[maxPathPoints - 2], transformPosition, 
                initialFloatPosition, maxLineLength, fishingLayer, fishingFloatLayer, gameViewType);
            pathpoints.Add(newPathPoint);
        }

        #endregion

        #region GetPathPoint

        private Vector2 GetPathPoint(Vector2 currentPathPoint, Vector2 previousPathPoint, Vector2 transformPosition,
           Vector2 initialFloatPosition, float maxLineLength, LayerMask fishingLayer, LayerMask fishingFloatLayer, GameViewType gameViewType)
        {
            float range = 15f;

            Vector2 newPathPoint = Random.insideUnitCircle * range;
            newPathPoint = newPathPoint + currentPathPoint;

            newPathPoint = AdjustPathPointToEnviorment(currentPathPoint, newPathPoint, fishingLayer, fishingFloatLayer);

            Vector2 angleDir = previousPathPoint - currentPathPoint;
            Vector2 targetDir = newPathPoint - currentPathPoint;

            float angle = Mathf.Acos(Vector2.Dot(angleDir.normalized, targetDir.normalized)) * Mathf.Rad2Deg;
            float minAngleBetweenPathPoints = 60f;

            int itteration = 0;
            int maxWhileItteration = 400;

            while (!CheckPointVisibility(previousPathPoint, newPathPoint, fishingLayer, fishingFloatLayer) ||
                !CheckNewPathPointCorrectness(currentPathPoint, newPathPoint, transformPosition, maxLineLength) ||
                !CheckWaterLevel(initialFloatPosition, newPathPoint, gameViewType) || angle < minAngleBetweenPathPoints)
            {
                newPathPoint = Random.insideUnitCircle * range;
                newPathPoint = newPathPoint + currentPathPoint;

                newPathPoint = AdjustPathPointToEnviorment(currentPathPoint, newPathPoint, fishingLayer, fishingFloatLayer);

                angleDir = previousPathPoint - currentPathPoint;
                targetDir = newPathPoint - currentPathPoint;

                angle = Mathf.Acos(Vector2.Dot(angleDir.normalized, targetDir.normalized)) * Mathf.Rad2Deg;

                itteration++;

                if (itteration > maxWhileItteration)
                {
                    int itteration2 = 0;
                    int maxItteration2 = 1000;

                    while (!CheckWaterLevel(initialFloatPosition, newPathPoint, gameViewType) || !CheckNewPathPointCorrectness(currentPathPoint, newPathPoint, transformPosition, maxLineLength))
                    {
                        newPathPoint = Random.insideUnitCircle * range;
                        newPathPoint = newPathPoint + currentPathPoint;

                        newPathPoint = AdjustPathPointToEnviorment(currentPathPoint, newPathPoint, fishingLayer, fishingFloatLayer);

                        itteration2++;

                        if (itteration2 > maxItteration2)
                        {
                            newPathPoint = initialFloatPosition - new Vector2(0f, 0.5f);

                            break;
                        }
                    }

                    break;
                }
            }

            return newPathPoint;
        }

        private bool CheckWaterLevel(Vector2 initialFloatPosition, Vector2 newPathPoint, GameViewType gameViewType)
        {
            if (gameViewType == GameViewType.TopDownView)
                return true;

            float waterLevelMargin = 0.5f;

            if (newPathPoint.y > (initialFloatPosition.y - waterLevelMargin))
                return false;

            return true;
        }

        private Vector2 AdjustPathPointToEnviorment(Vector2 currentPathPoint, Vector2 newPathPoint, LayerMask fishingLayer, LayerMask fishingFloatLayer)
        {
            int combinedLayerMask = fishingLayer | fishingFloatLayer;

            RaycastHit2D hit2D = Physics2D.Linecast(currentPathPoint, newPathPoint, ~combinedLayerMask);

            if (hit2D.collider != null)
            {
                Vector2 direction = (currentPathPoint - hit2D.point).normalized;

                //Distance in front of the detected obstacle.
                float offsetDistance = 1.2f;

                Vector2 offsetPoint = hit2D.point + direction * offsetDistance;
                return offsetPoint;
            }

            return newPathPoint;
        }

        private bool CheckPointVisibility(Vector2 previousPathPoint, Vector2 newPathPoint, LayerMask fishingLayer, LayerMask fishingFloatLayer)
        {
            int combinedLayerMask = fishingLayer | fishingFloatLayer;

            if (Physics2D.Linecast(previousPathPoint, newPathPoint, ~combinedLayerMask))
                return false;

            return true;
        }

        private bool CheckNewPathPointCorrectness(Vector2 currentPathPoint, Vector2 newPathPoint, Vector2 transformPosition, float maxLineLength)
        {
            float minDistanceToNewPathPoint = 1f;

            float distanceToNewPathPoint = Vector2.Distance(currentPathPoint, newPathPoint);
            float distanceToLineAttachment = Vector2.Distance(newPathPoint, transformPosition);

            bool checking = true;

            if (distanceToNewPathPoint < minDistanceToNewPathPoint || distanceToLineAttachment > maxLineLength)
                checking = false;

            return checking;
        }

        #endregion

        public void ClearPathData()
        {
            _pathPoints.Clear();
            _initizlizePath = true;
        }

#if UNITY_EDITOR

        private void Debuging()
        {
            List<Vector3> pathPoints = new List<Vector3>();
            for (int i = 0; i < _pathPoints.Count; i++)
                pathPoints.Add(_pathPoints[i]);

            DebugUtilities.DrawPath(pathPoints, Color.red, Color.red, new Vector3(0.2f, 0.2f, 0.2f));
        }

#endif
    }
}
