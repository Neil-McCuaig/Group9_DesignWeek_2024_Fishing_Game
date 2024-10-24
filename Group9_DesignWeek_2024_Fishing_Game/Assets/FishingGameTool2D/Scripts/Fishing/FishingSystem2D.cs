using FishingGameTool2D.CustomAttribute;
using FishingGameTool2D.Fishing.LootData;
using FishingGameTool2D.Fishing.CatchProbability;
using FishingGameTool2D.Fishing.BaitData;
using FishingGameTool2D.Fishing.Rod;
using FishingGameTool2D.Fishing.Float;
using FishingGameTool2D.Fishing.Line;

using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

namespace FishingGameTool2D.Fishing
{
    public enum GameViewType
    {
        SideView,
        TopDownView
    };

    public enum CastDir
    {
        left,
        right,
        top,
        down
    };

    public enum Invokes
    {
        Disable,
        Enable
    };

    [AddComponentMenu("Fishing Game Tool 2D/Fishing System 2D")]
    public class FishingSystem2D : MonoBehaviour
    {
        [System.Serializable]
        public class Events
        {
            public UnityEvent OnFloatCasted;
            public UnityEvent OnFloatAttracted;
            public UnityEvent OnLootCaught;
            public UnityEvent OnSuccessfulCatch;
            public UnityEvent OnFailedCatch;
        }

        [System.Serializable]
        public class AdvancedSettings
        {
            [ReadOnlyField]
            public FishingLootData _caughtLootData;

            [InfoBox("Custom Catch Probability Data.")]
            public CatchProbabilityData _catchProbabilityData;

            public bool _caughtLoot = false;
            public float _returnSpeedWithoutLoot = 2f;
            [InfoBox("The time interval in seconds between consecutive checks to determine if the catch has been successfully caught.")]
            public float _catchCheckInterval = 1f;
            [ReadOnlyField]
            public float _lootWeight;
        }
        [BetterHeader("Game Settings", 20), Space]
        public GameViewType _gameViewType;

        [BetterHeader("Fishing Settings", 20), Space]
        public FishingRod2D _fishingRod;

        [InfoBox("LayerMask used to determine on which layer fishing is allowed.")]
        public LayerMask _fishingLayer;
        public FishingBaitData _bait;

        [Space, BetterHeader("Cast And Attract Settings", 20)]
        public GameObject _fishingFloatPrefab;

        [Space, AddButton("Enable Attraction Point Correction", "_enableAttractionPointCorrection")]
        public bool _enableAttractionPointCorrection = false;
        [ShowVariable("_enableAttractionPointCorrection"), InfoBox("Correction for the attraction point position in 2D space. " +
            "This Vector2 allows fine-tuning the position where objects are attracted.")]
        public Vector2 _attractionPointCorrection;

        [Space]
        [InfoBox("A layer mask that defines the player. It is used to determine which layer to skip when checking the attraction path of the float.")]
        public LayerMask _characterLayer;
        public LayerMask _fishingFloatLayer;

        [HideInInspector]
        public bool _topdownViewCastSettings = false;
        [HideInInspector]
        public bool _sideViewCastSettings = true;

        [ShowVariable("_sideViewCastSettings")]
        public float _maxCastForce = 20f;
        [ShowVariable("_sideViewCastSettings")]
        public float _forceChargeRate = 4f;
        [ReadOnlyField, ShowVariable("_sideViewCastSettings")]
        public float _currentCastForce;

        [ShowVariable("_topdownViewCastSettings")]
        [InfoBox("Switching to the Topdown view changes the way the fishing float is cast. Now, the time the float spends in the air " +
            "is taken into account. When this time elapses, the surface beneath the float is checked.")]
        public float _maxCastTime = 2f;
        [ShowVariable("_topdownViewCastSettings")]
        public float _timeChargeRate = 0.3f;
        [ShowVariable("_topdownViewCastSettings")]
        public float _initialForce = 10f;
        [ReadOnlyField, ShowVariable("_topdownViewCastSettings")]
        public float _currentCastTime;

        public float _spawnFloatDelay = 0.3f;

        [InfoBox("Minimum distance required for successfully picking up or catching a target (e.g., fish or object).")]
        public float _catchDistance = 1f;

        [Space, BetterHeader("Events", 20), InfoBox("Select 'Enable' to display available Unity Events. Events enable seamless integration of various states from the Fishing System 2D.")]
        public Invokes _enableInvokes;

        [HideInInspector]
        public bool _showEvents = false;

        [ShowVariable("_showEvents")]
        public Events _events;

        [Space, AddButton("Advanced Settings", "_showAdvancedSettings")]
        public bool _showAdvancedSettings = false;

        [ShowVariable("_showAdvancedSettings")]
        public AdvancedSettings _advanced;

        [HideInInspector]
        public bool _attractInput;
        [HideInInspector]
        public bool _castInput;
        [HideInInspector]
        public bool _castFloat = false;

        #region PRIVATE VARIABLES

        private float _catchCheckIntervalTimer;
        private float _randomSpeedChangerTimer = 2f;
        private float _randomSpeedChanger = 1f;
        private float _finalSpeed;
        private float _saveCurrentCastTime;
        private float _castTimer;
        private bool _startChecking = false;

        private CastDir _castDir;
        private SubstrateType _substrateType;

        FishingFloatPathfinder2D _fishingFloatPathfinder2D = new FishingFloatPathfinder2D();

        #endregion

#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (_enableInvokes == Invokes.Enable)
                _showEvents = true;
            else
                _showEvents = false;

            if (_gameViewType == GameViewType.TopDownView)
            {
                _topdownViewCastSettings = true;
                _sideViewCastSettings = false;
            }
            else
            {
                _topdownViewCastSettings = false;
                _sideViewCastSettings = true;
            }
        }

#endif

        private void Awake()
        {
            _catchCheckIntervalTimer = _advanced._catchCheckInterval;
        }

        private void Update()
        {
            if (_fishingRod != null)
            {
                AttractFloat();
                CastFloat();
            }

            HandleInput();
        }

        private void AttractFloat()
        {
            if (_fishingRod._fishingFloat == null)
                return;

            if (_gameViewType == GameViewType.TopDownView)
            {
                _castTimer += Time.deltaTime;

                if (_castTimer > _saveCurrentCastTime)
                    _startChecking = true;
            }
            else
                _startChecking = true;

            if (!_advanced._caughtLoot)
                _substrateType = _fishingRod._fishingFloat.GetComponent<FishingFloat2D>().CheckSurface(_fishingLayer, _startChecking);

            if (_substrateType == SubstrateType.Water)
                _advanced._caughtLoot = CheckingLoot(_advanced._caughtLoot, _bait, _advanced._catchProbabilityData, transform.position, _fishingRod._fishingFloat.position);

            LineLengthLimitation(_fishingRod._fishingFloat.gameObject, transform.position, _fishingRod._lineStatus, _substrateType);

            if (_attractInput && _substrateType == SubstrateType.InAir && !_advanced._caughtLoot)
            {
                AttractInAir();
                return;
            }
            else if (_attractInput && _substrateType == SubstrateType.Land && !_advanced._caughtLoot)
            {
                if (AttractOnLand())
                    return;
            }
            else if (_attractInput && _substrateType == SubstrateType.Water && !_advanced._caughtLoot)
            {
                if (AttractInWaterWithoutLoot())
                    return;
            }
            else if (_advanced._caughtLoot && _fishingRod._fishingFloat != null)
            {
                if (AttractInWaterWithLoot())
                    return;
            }
        }

        private void AttractInAir()
        {
            Destroy(_fishingRod._fishingFloat.gameObject);
            _fishingRod._fishingFloat = null;
            _castTimer = 0;
            _startChecking = false;
        }

        private bool AttractOnLand()
        {
            float distance = Vector2.Distance(transform.position, _fishingRod._fishingFloat.position);

            Vector2 attractPoint = _enableAttractionPointCorrection == true ? (Vector2)transform.position + _attractionPointCorrection : transform.position;
            Vector2 direction = (attractPoint - (Vector2)_fishingRod._fishingFloat.position).normalized;

            Vector2 attractVector = direction * (_advanced._returnSpeedWithoutLoot * (_gameViewType == GameViewType.SideView ? 90f : 2f)) * Time.deltaTime;

            if (_gameViewType == GameViewType.SideView)
            {
                Rigidbody2D fishingFloatRB = _fishingRod._fishingFloat.GetComponent<Rigidbody2D>();
                fishingFloatRB.velocity = new Vector2(attractVector.x, fishingFloatRB.velocity.y);
            }
            else
                _fishingRod._fishingFloat.Translate(attractVector);

            if (distance <= _catchDistance)
            {
                Destroy(_fishingRod._fishingFloat.gameObject);
                _fishingRod._fishingFloat = null;
                _castTimer = 0;
                _startChecking = false;

                //InvokeEvent
                if (_enableInvokes == Invokes.Enable)
                    _events.OnFloatAttracted.Invoke();

                return true;
            }

            return false;
        }

        private bool AttractInWaterWithoutLoot()
        {
            float distance = Vector2.Distance(transform.position, _fishingRod._fishingFloat.position);

            Vector2 attractDir = _enableAttractionPointCorrection == true ? ((Vector2)transform.position + _attractionPointCorrection) : transform.position;

            _fishingFloatPathfinder2D.FloatBehavior(null, _fishingRod._fishingFloat, attractDir, _fishingRod._lineStatus._maxLineLength, _advanced._returnSpeedWithoutLoot,
                _attractInput, _fishingLayer, _fishingFloatLayer, _gameViewType);

            if (distance <= _catchDistance)
            {
                Destroy(_fishingRod._fishingFloat.gameObject);
                _fishingRod._fishingFloat = null;
                _castTimer = 0;
                _startChecking = false;

                //InvokeEvent
                if (_enableInvokes == Invokes.Enable)
                    _events.OnFloatAttracted.Invoke();

                return true;
            }

            return false;
        }

        private bool AttractInWaterWithLoot()
        {
            _fishingRod.LootCaught(_advanced._caughtLoot);

            while (_advanced._caughtLootData == null)
            {
                List<FishingLootData> lootDataList = _fishingRod._fishingFloat.GetComponent<FishingFloat2D>().GetLootDataFormWaterObject();
                _advanced._caughtLootData = ChooseFishingLoot(_bait, lootDataList);

                if (_advanced._caughtLootData != null)
                {
                    float lootWeight = Random.Range(_advanced._caughtLootData._weightRange._minWeight, _advanced._caughtLootData._weightRange._maxWeight);
                    _advanced._lootWeight = lootWeight;

                    _bait = null;
                }
            }

            Vector2 attractDir = _enableAttractionPointCorrection == true ? ((Vector2)transform.position + _attractionPointCorrection) : (Vector2)transform.position;

            AttractWithLoot(_advanced._caughtLootData, _fishingRod._fishingFloat, attractDir, _fishingLayer, _fishingFloatLayer, _attractInput, _advanced._lootWeight,
                _fishingRod, _gameViewType);

            if (_attractInput && _fishingRod._fishingFloat != null)
            {
                float distance = Vector2.Distance(transform.position, _fishingRod._fishingFloat.position);

                if (distance <= _catchDistance)
                {
                    GrabLoot(_advanced._caughtLootData, _fishingRod._fishingFloat.position, transform.position, _gameViewType);

                    Destroy(_fishingRod._fishingFloat.gameObject);
                    _fishingRod._fishingFloat = null;
                    _advanced._caughtLoot = false;
                    _advanced._caughtLootData = null;
                    _fishingRod.FinishFishing();
                    _castTimer = 0;
                    _startChecking = false;

                    //InvokeEvent
                    if (_enableInvokes == Invokes.Enable)
                    {
                        _events.OnFloatAttracted.Invoke();
                        _events.OnSuccessfulCatch.Invoke();
                    }

                    return true;
                }
            }

            return false;
        }

        private void LineLengthLimitation(GameObject fishingFloat, Vector2 transformPosition, FishingLineStatus2D fishingLineStatus, SubstrateType substrateType)
        {
            if (fishingLineStatus._currentLineLength > fishingLineStatus._maxLineLength && substrateType != SubstrateType.Water)
            {
                Vector2 fishingFloatPosition = fishingFloat.transform.position;
                Vector2 direction = (transformPosition - fishingFloatPosition).normalized;

                Rigidbody2D fishingFloatRB = fishingFloat.GetComponent<Rigidbody2D>();

                float speed = (fishingLineStatus._currentLineLength - fishingLineStatus._maxLineLength) / Time.deltaTime;
                float maxSpeed = 5f;
                float clampedSpeed = Mathf.Clamp(speed, -maxSpeed, maxSpeed);

                fishingFloatRB.velocity = direction * clampedSpeed;
            }
        }

        private bool CheckingLoot(bool caughtLoot, FishingBaitData baitData, CatchProbabilityData catchProbabilityData,
            Vector2 transformPosition, Vector2 fishingFloatPosition)
        {
            if (caughtLoot)
                return true;

            bool caught = false;

            _catchCheckIntervalTimer -= Time.deltaTime;

            if (_catchCheckIntervalTimer <= 0)
            {
                caught = CheckLootIsCatch(baitData, catchProbabilityData, transformPosition, fishingFloatPosition);
                _catchCheckIntervalTimer = _advanced._catchCheckInterval;
            }

            //InvokeEvent
            if (_enableInvokes == Invokes.Enable)
                if (caught)
                    _events.OnLootCaught.Invoke();

            return caught;
        }

        private bool CheckLootIsCatch(FishingBaitData baitData, CatchProbabilityData catchProbabilityData,
            Vector2 transformPosition, Vector2 fishingFloatPosition)
        {
            float distance = Vector2.Distance(transformPosition, fishingFloatPosition);
            float minSafeFishingDistanceFactor = 5f;

            int chanceVal = Random.Range(1, 100);

            int commonProbability = 5;
            int uncommonProbability = 12;
            int rareProbability = 22;
            int epicProbability = 35;
            int legendaryProbability = 45;

            if (catchProbabilityData != null)
            {
                commonProbability = catchProbabilityData._commonProbability;
                uncommonProbability = catchProbabilityData._uncommonProbability;
                rareProbability = catchProbabilityData._rareProbability;
                epicProbability = catchProbabilityData._epicProbability;
                legendaryProbability = catchProbabilityData._legendaryProbability;
                minSafeFishingDistanceFactor = catchProbabilityData._minSafeFishingDistanceFactor;
            }

            commonProbability = CalculateProbabilityValueByCastDistance(commonProbability, distance, minSafeFishingDistanceFactor);
            uncommonProbability = CalculateProbabilityValueByCastDistance(uncommonProbability, distance, minSafeFishingDistanceFactor);
            rareProbability = CalculateProbabilityValueByCastDistance(rareProbability, distance, minSafeFishingDistanceFactor);
            epicProbability = CalculateProbabilityValueByCastDistance(epicProbability, distance, minSafeFishingDistanceFactor);
            legendaryProbability = CalculateProbabilityValueByCastDistance(legendaryProbability, distance, minSafeFishingDistanceFactor);

            if (baitData == null)
            {
                if (chanceVal <= commonProbability)
                    return true;
                else
                    return false;
            }
            else
            {
                switch (baitData._baitTier)
                {
                    case BaitTier.Uncommon:

                        if (chanceVal <= uncommonProbability)
                            return true;
                        else
                            return false;

                    case BaitTier.Rare:

                        if (chanceVal <= rareProbability)
                            return true;
                        else
                            return false;

                    case BaitTier.Epic:

                        if (chanceVal <= epicProbability)
                            return true;
                        else
                            return false;

                    case BaitTier.Legendary:

                        if (chanceVal <= legendaryProbability)
                            return true;
                        else
                            return false;
                }
            }

            return false;
        }

        private static int CalculateProbabilityValueByCastDistance(float probability, float distance, float minSafeFishingDistanceFactor)
        {
            float minValue = 0.3f;
            float maxValue = 1f;

            float x = Mathf.InverseLerp(0, minSafeFishingDistanceFactor, distance);
            float value = Mathf.Lerp(minValue, maxValue, x);

            probability = probability * value;

            return (int)probability;
        }

        private FishingLootData ChooseFishingLoot(FishingBaitData baitData, List<FishingLootData> lootDataList)
        {
            for (int i = 0; i < lootDataList.Count; i++)
            {
                FishingLootData temp = lootDataList[i];
                int randomIndex = Random.Range(i, lootDataList.Count);
                lootDataList[i] = lootDataList[randomIndex];
                lootDataList[randomIndex] = temp;
            }

            float totalRarity = CalculateTotalRarity(lootDataList);
            List<float> lootRarityList = CalculatePercentageRarity(lootDataList, totalRarity);

            int baitTier = 0;

            if (baitData != null)
                baitTier = (int)baitData._baitTier + 1;

            float chanceVal = Random.Range(1f, 100f);
            float addedRarity = 0f;

            for (int i = 0; i < lootRarityList.Count; i++)
            {
                addedRarity += lootRarityList[i];

                if (chanceVal <= addedRarity)
                {
                    if (baitTier >= (int)lootDataList[i]._lootTier)
                        return lootDataList[i];
                    else
                        return null;
                }
            }

            return null;
        }

        private float CalculateTotalRarity(List<FishingLootData> lootDataList)
        {
            float totalRarity = 0;

            foreach (var lootData in lootDataList)
            {
                totalRarity += lootData._lootRarity;
            }

            return totalRarity;
        }

        private List<float> CalculatePercentageRarity(List<FishingLootData> lootDataList, float totalRarity)
        {
            List<float> lootRarityList = new List<float>();

            foreach (var lootData in lootDataList)
            {
                float percentageRarity = (lootData._lootRarity / totalRarity) * 100f;
                lootRarityList.Add(percentageRarity);

                //Debug.Log(lootData + " - Rarity Percentage: " + percentageRarity + "%");
            }

            return lootRarityList;
        }

        private void AttractWithLoot(FishingLootData lootData, Transform fishingFloatTransform, Vector2 transformPosition, LayerMask fishingLayer, LayerMask fishingFloatLayer,
            bool attractInput, float lootWeight, FishingRod2D fishingRod, GameViewType gameViewType)
        {
            Vector2 fishingFloatPosition = fishingFloatTransform.position;

            float lootSpeed = CalculateLootSpeed(lootData, lootWeight);
            float attractSpeed = CalculateAttractSpeed(fishingRod, attractInput, lootWeight, (int)lootData._lootTier);
            _finalSpeed = Mathf.Lerp(_finalSpeed, attractInput == true ? CalculateFinalAttractSpeed(lootSpeed, attractSpeed, lootData) : lootSpeed, 3f * Time.deltaTime);

            _fishingFloatPathfinder2D.FloatBehavior(lootData, fishingFloatTransform, transformPosition, fishingRod._lineStatus._maxLineLength, _finalSpeed, attractInput,
                fishingLayer, fishingFloatLayer, gameViewType);

            //Debug.Log("Loot Speed: " + lootSpeed + " | Attract Speed: " + attractSpeed + " | Final Speed: " + _finalSpeed);
        }

        private float CalculateLootSpeed(FishingLootData lootData, float lootWeight)
        {
            float[] speedMultipliersByTier = { 1.0f, 1.5f, 2.0f, 2.5f, 3.0f };
            float baseSpeed = 1.2f;

            int tier = (int)lootData._lootTier;

            _randomSpeedChangerTimer -= Time.deltaTime;
            if (_randomSpeedChangerTimer < 0)
            {
                _randomSpeedChanger = Random.Range(1f, 3f);
                _randomSpeedChangerTimer = Random.Range(2f, 4f);
            }

            float lootSpeed = (baseSpeed + lootWeight * 0.1f * speedMultipliersByTier[tier]) * _randomSpeedChanger;
            return lootSpeed;
        }

        private float CalculateAttractSpeed(FishingRod2D fishingRod, bool attractInput, float lootWeight, int lootTier)
        {
            FishingLineStatus2D fishingLineStatus = fishingRod.CalculateLineLoad(attractInput, lootWeight, lootTier);
            float attractionSpeed = fishingLineStatus._attractFloatSpeed;

            return attractionSpeed;
        }

        private float CalculateFinalAttractSpeed(float lootSpeed, float attractSpeed, FishingLootData lootData)
        {
            int tier = (int)lootData._lootTier;
            float[] speedFactorByTier = { 1.2f, 1.0f, 0.8f, 0.6f, 0.5f };

            float finalAttractSpeed = (attractSpeed - lootSpeed) * speedFactorByTier[tier];
            finalAttractSpeed = finalAttractSpeed < 2f ? 2f : finalAttractSpeed;

            return finalAttractSpeed;
        }

        private void GrabLoot(FishingLootData lootData, Vector2 fishingFloatPosition, Vector2 transformPosition, GameViewType gameViewType)
        {
            if (lootData._lootPrefab == null)
            {
                Debug.LogError("No loot prefab added!");
                return;
            }

            SpawnLootItem(lootData._lootPrefab, fishingFloatPosition, transformPosition, gameViewType);
        }

        private void SpawnLootItem(GameObject lootPrefab, Vector2 fishingFloatPosition, Vector2 transformPosition, GameViewType gameViewType)
        {
            Vector2 direction = ((transformPosition - fishingFloatPosition) + (Vector2.up * 2f)).normalized;
            Vector2 spawnPosition = fishingFloatPosition + new Vector2(0f, 1f);

            GameObject spawnedLootPrefab = Instantiate(lootPrefab, spawnPosition, Quaternion.identity);

            float distance = Vector2.Distance(fishingFloatPosition, transformPosition);
            float force = distance * 2f;

            if (gameViewType == GameViewType.TopDownView)
            {
                float time = distance * 0.5f;

                spawnedLootPrefab.GetComponent<Collider2D>().enabled = false;
                StartCoroutine(EnableLootCollider(spawnedLootPrefab, time));
            }

            spawnedLootPrefab.GetComponent<Rigidbody2D>().AddForce(direction * force, ForceMode2D.Impulse);
        }

        private IEnumerator EnableLootCollider(GameObject spawnedLootPrefab, float time)
        {
            yield return new WaitForSeconds(time);
            spawnedLootPrefab.GetComponent<Collider2D>().enabled = true;
        }

        private void CastFloat()
        {
            if (_fishingRod._fishingFloat != null || _castFloat || _fishingRod._lineStatus._isLineBroken)
                return;

            if (_castInput)
            {
                if (_gameViewType == GameViewType.SideView)
                    _currentCastForce = CalculateCastForce(_currentCastForce, _maxCastForce, _forceChargeRate);
                else
                    _currentCastTime = CalculateCastForce(_currentCastTime, _maxCastTime, _timeChargeRate);
            }

            if ((!_castInput && (_gameViewType == GameViewType.SideView ? _currentCastForce != 0f : _currentCastTime != 0f)) ||
                ((_gameViewType == GameViewType.SideView ? _currentCastForce : _currentCastTime) >= _maxCastForce))
            {
                Vector2 spawnPoint = _fishingRod._line._lineAttachment.position;
                Vector2 castDirection = Vector2.zero;

                switch (_castDir)
                {
                    case CastDir.left:

                        castDirection = -(Vector2)transform.right + Vector2.up;

                        break;

                    case CastDir.right:

                        castDirection = (Vector2)transform.right + Vector2.up;

                        break;

                    case CastDir.top:

                        castDirection = (Vector2)transform.up * 2f;

                        break;

                    case CastDir.down:

                        castDirection = -(Vector2)transform.up;

                        break;
                }

                StartCoroutine(CastingDelay(_spawnFloatDelay, castDirection, spawnPoint, _gameViewType == GameViewType.SideView ? _currentCastForce :
                    (_currentCastTime * _initialForce), _fishingFloatPrefab));

                _saveCurrentCastTime = _currentCastTime;

                _currentCastTime = 0;
                _currentCastForce = 0f;
            }
        }

        private IEnumerator CastingDelay(float delay, Vector2 castDirection, Vector2 spawnPoint, float castForce, GameObject fishingFloatPrefab)
        {
            _castFloat = true;

            yield return new WaitForSeconds(delay);
            _fishingRod._fishingFloat = Cast(castDirection, spawnPoint, castForce, fishingFloatPrefab);

            //InvokeEvent
            if (_enableInvokes == Invokes.Enable)
                _events.OnFloatCasted.Invoke();

            _castFloat = false;
        }

        private Transform Cast(Vector2 castDirection, Vector2 spawnPoint, float castForce, GameObject fishingFloatPrefab)
        {
            GameObject spawnedFishingFloat = Instantiate(fishingFloatPrefab, spawnPoint, Quaternion.identity);

            if (_gameViewType == GameViewType.TopDownView)
                spawnedFishingFloat.GetComponent<CircleCollider2D>().enabled = false;

            spawnedFishingFloat.GetComponent<Rigidbody2D>().AddForce(castDirection * castForce, ForceMode2D.Impulse);

            return spawnedFishingFloat.transform;
        }

        private float CalculateCastForce(float currentCastForce, float maxCastForce, float forceChargeRate)
        {
            currentCastForce += forceChargeRate * Time.deltaTime;
            currentCastForce = currentCastForce > maxCastForce ? maxCastForce : currentCastForce;

            return currentCastForce;
        }

        private void HandleInput()
        {
            _attractInput = Input.GetKey(KeyCode.N);
            _castInput = Input.GetKey(KeyCode.B);
        }

        public void ForceStopFishing()
        {
            Destroy(_fishingRod._fishingFloat.gameObject);
            _fishingRod._fishingFloat = null;
            _advanced._caughtLoot = false;
            _advanced._caughtLootData = null;
            _fishingRod.FinishFishing();
            _fishingFloatPathfinder2D.ClearPathData();

            //InvokeEvent
            if (_enableInvokes == Invokes.Enable)
                _events.OnFailedCatch.Invoke();
        }

        public void AddBait(FishingBaitData baitData)
        {
            if (_bait == null)
                _bait = baitData;
            else
            {
                Vector2 spawnPos = new Vector2(transform.position.x + 0.5f, transform.position.y);
                Instantiate(_bait._baitPrefab, spawnPos, Quaternion.identity);

                _bait = baitData;
            }
        }

        public void FixFishingLine()
        {
            if (_fishingRod._lineStatus._isLineBroken)
                _fishingRod._lineStatus._isLineBroken = false;
        }

        public void SetCastDirection(CastDir castDir)
        {
            _castDir = castDir;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (_enableAttractionPointCorrection)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube((Vector2)transform.position + _attractionPointCorrection, new Vector2(0.2f, 0.2f));
            }
        }

#endif
    }
}
