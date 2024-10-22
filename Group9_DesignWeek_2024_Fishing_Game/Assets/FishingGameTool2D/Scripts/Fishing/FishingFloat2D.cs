using FishingGameTool2D.Fishing.Loot;
using FishingGameTool2D.Fishing.LootData;
using FishingGameTool2D.CustomAttribute;
using System.Collections.Generic;
using UnityEngine;

namespace FishingGameTool2D.Fishing.Float
{
    public enum SubstrateType
    {
        Water,
        Land,
        InAir
    };

    [AddComponentMenu("Fishing Game Tool 2D/Fishing Float 2D")]
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(CircleCollider2D))]
    public class FishingFloat2D : MonoBehaviour
    {
        [System.Serializable]
        public class FishingFloatAnimationSettings
        {
            [InfoBox("Settings for additional float animation in the water, based on an Animation Curve. " +
                "An object representing the float must be placed as a child within the main float object for it to function properly.")]
            public Transform _floatRepresentation;
            public AnimationCurve _floatAnimationCurve;
            public float _animForce = 0.2f;
            public float _animSpeed = 0.3f;
        }

        public LayerMask _fishingFloatLayerMask;
        public LayerMask _excludeDetectionLayerMask;
        public float _checkerRadius = 0.05f;
        
        [Space, AddButton("Enable Float Animations", "_enableFloatAnim")]
        public bool _enableFloatAnim = false;

        [ShowVariable("_enableFloatAnim")]
        public FishingFloatAnimationSettings _fishingFloatAnimationSettings;

        [Space]
        public bool _enableDebugLog = false;

        private GameObject _waterObject;
        private Rigidbody2D _flaotRB;
        private CircleCollider2D _floatCollider;

        private void Start()
        {
            _flaotRB = GetComponent<Rigidbody2D>();
            _floatCollider = GetComponent<CircleCollider2D>();
            _flaotRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            HandleFloatAnim();
        }

        private void HandleFloatAnim()
        {
            if (!_enableFloatAnim || _fishingFloatAnimationSettings._floatRepresentation == null || _waterObject == null)
                return;

            float yPosAnimEvolve = _fishingFloatAnimationSettings._floatAnimationCurve.Evaluate(Time.time * _fishingFloatAnimationSettings._animSpeed);
            _fishingFloatAnimationSettings._floatRepresentation.localPosition = new Vector3(0f, yPosAnimEvolve * _fishingFloatAnimationSettings._animForce, 0f);
        }

        /// <summary>
        /// Checks the type of ground on which the float is located.
        /// </summary>
        /// <param name="fishingLayer">The layer mask representing the surfaces to check against.</param>
        /// <returns>The type of ground on which the float is located.</returns>
        public SubstrateType CheckSurface(LayerMask fishingLayer, bool startChecking)
        {
            if (!startChecking)
                return SubstrateType.InAir;
            else
                _floatCollider.enabled = true;

            int combinedLayerMask = _fishingFloatLayerMask | _excludeDetectionLayerMask;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _checkerRadius, ~combinedLayerMask);
            SubstrateType substrateType;

            foreach (Collider2D collider in colliders)
            {
                if((fishingLayer & (1 << collider.gameObject.layer)) != 0)
                {
                    _flaotRB.bodyType = RigidbodyType2D.Kinematic;
                    _flaotRB.velocity = Vector2.zero;
                    _floatCollider.enabled = false; 

                    _waterObject = collider.gameObject;
                    substrateType = SubstrateType.Water;

                    if(_enableDebugLog)
                        Debug.Log("[Fishing Float] " + substrateType);

                    return substrateType;
                }
                else if ((fishingLayer & (1 << collider.gameObject.layer)) == 0)
                {
                    _flaotRB.bodyType = RigidbodyType2D.Dynamic;
                    _floatCollider.enabled = true;

                    _waterObject = null;
                    substrateType = SubstrateType.Land;

                    if (_enableDebugLog)
                        Debug.Log("[Fishing Float] " + substrateType);

                    return substrateType;
                }
            }

            substrateType = SubstrateType.InAir;

            if (_enableDebugLog)
                Debug.Log("[Fishing Float] " + substrateType);

            return substrateType;
        }

        /// <summary>
        /// This function returns a list of fishing loot data associated with the water object.
        /// </summary>
        /// <returns>List<FishingLootData> - A list containing fishing loot data associated with the water object.</returns>
        public List<FishingLootData> GetLootDataFormWaterObject()
        {
            List<FishingLootData> lootDataList = _waterObject.GetComponent<FishingLoot2D>().GetFishingLoot();
            return lootDataList;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _checkerRadius);
        }

#endif
    }
}
