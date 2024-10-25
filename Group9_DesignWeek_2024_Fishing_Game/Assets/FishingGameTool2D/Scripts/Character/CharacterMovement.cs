using FishingGameTool2D.Fishing;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FishingGameTool2D.Example
{
    public class CharacterMovement : MonoBehaviour
    {
        public float _speed = 2f;
        public float _jumpForce = 2f;

        [Space]
        public Vector2 _groundCheckerSize;
        public Vector2 _groundCheckerPos;
        public LayerMask _groundMask;

        public GameManager gameManager;

        #region PRIVATE VARIABLES

        private Rigidbody2D _characterRB;
        private FishingSystem2D _fishingSystem2D;

        private Vector2 _inputMove;
        private bool _inputJump;

        #endregion

        private void Awake()
        {
            _characterRB = GetComponent<Rigidbody2D>();
            _fishingSystem2D = GetComponent<FishingSystem2D>();
        }

        private void Update()
        {
            HandleInput();
            JumpSystem();
            SpriteFlipSystem();
            ControlCastDir();
        }

        private void SpriteFlipSystem()
        {
            if (_inputMove.x < 0f)
                transform.localScale = new Vector3(1, 1, 1);
            else if(_inputMove.x > 0f)
                transform.localScale = new Vector3(-1, 1, 1);
        }

        private void ControlCastDir()
        {
            CastDir castDir = new CastDir();

            if (transform.localScale.x == 1)
                castDir = CastDir.left;
            else if(transform.localScale.x == -1)
                castDir = CastDir.right;

            _fishingSystem2D.SetCastDirection(castDir);
        }

        private void JumpSystem()
        {
           if(IsGrounded() && _inputJump)
                _characterRB.velocity = new Vector2(_characterRB.velocity.x, _jumpForce);
        }

        private bool IsGrounded()
        {
            bool isGrounded = Physics2D.OverlapBox((Vector2)transform.position + _groundCheckerPos, _groundCheckerSize, 0f, _groundMask);
            return isGrounded;
        }

        private void FixedUpdate()
        {
            MoveSystem();
        }

        private void MoveSystem()
        {
            Vector2 moveDir = new Vector2(_inputMove.x, 0f);
            Vector2 finalDir = moveDir * (_speed * 10) * Time.fixedDeltaTime;

            _characterRB.velocity = new Vector2(finalDir.x, _characterRB.velocity.y);
        }

        private void HandleInput()
        {
            _inputMove = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            _inputJump = Input.GetButtonDown("Jump");
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Fish"))
            {
                Destroy(other.gameObject);
                gameManager.counterFish++;
            }
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube((Vector2)transform.position + _groundCheckerPos, _groundCheckerSize);
        }

#endif
    }
}
