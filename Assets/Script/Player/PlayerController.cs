using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Script
{
    public class PlayerController: MonoBehaviour
    {
        private PlayerInput _input;
        private InputAction _moveAction;
        public Vector2 MovementInput { get; protected set; }
        private void Awake()
        { 
            _input=GameManager.Instance.PlayerInput;
            _moveAction=_input.currentActionMap.FindAction("Move", true); 
        }

        private void Update()
        {
            MovementInput = _moveAction.ReadValue<Vector2>();
        }
    }
}