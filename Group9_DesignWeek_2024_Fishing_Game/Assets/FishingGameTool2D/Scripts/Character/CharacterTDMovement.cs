using FishingGameTool2D.Fishing;
using System;
using UnityEngine;

namespace FishingGameTool2D.Example
{
    public class CharacterTDMovement : MonoBehaviour
    {
        public float _moveSpeed = 5f;
        public Transform _fishingRodHolder;

        #region PRIVATE VARIABLES

        private Vector2 _inputMove;

        private Rigidbody2D _rigidbody2D;
        private Animator _animator;
        private FishingSystem2D _fishingSystem2D;

        private int _saveLookDir;

        #endregion

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _fishingSystem2D = GetComponent<FishingSystem2D>();
        }

        private void Update()
        {
            HandleInput();
            AnimationControl();
            FishingRodHolder();
            ControlCastDir();
        }

        private void FishingRodHolder()
        {
            if (_inputMove.y > 0)
                _fishingRodHolder.localScale = new Vector3(-1, 1, 1);
            else if(_inputMove.y < 0)
                _fishingRodHolder.localScale = new Vector3(1, 1, 1);

            if (_inputMove.x > 0)
                _fishingRodHolder.localScale = new Vector3(-1, 1, 1);
            else if (_inputMove.x < 0)
                _fishingRodHolder.localScale = new Vector3(1, 1, 1);
        }

        private void AnimationControl()
        {
            if(_inputMove.y != 0)
                _saveLookDir = _inputMove.y > 0 ? 1 : 0;

            if (_inputMove.x != 0)
                _saveLookDir = _inputMove.x > 0 ? 3 : 2;


            _animator.SetFloat("LookDir", _saveLookDir);
        }

        private void ControlCastDir()
        {
            CastDir castDir = new CastDir();

            if (_saveLookDir == 0)
                castDir = CastDir.down;
            else if (_saveLookDir == 1)
                castDir = CastDir.top;
            else if (_saveLookDir == 2)
                castDir = CastDir.left;
            else if (_saveLookDir == 3)
                castDir = CastDir.right;

            _fishingSystem2D.SetCastDirection(castDir);
        }

        private void FixedUpdate()
        {
            _rigidbody2D.MovePosition(_rigidbody2D.position + _inputMove * _moveSpeed * Time.fixedDeltaTime);
        }

        private void HandleInput()
        {
            _inputMove = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
    }
}
