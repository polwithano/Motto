using System;
using Inputs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class InputManager : MonoBehaviourSingleton<InputManager>
    {
        private BaseInputActions _actions;

        public event Action<Vector2> OnLeftClick; 
        public event Action<Vector2> OnRightClick;

        #region Mono
        private void Awake()
        {
          _actions = new BaseInputActions();
        }
        
        private void OnEnable()
        {
            _actions.Enable();
            
            _actions.UI.Click.performed += HandleLeftClick;
            _actions.UI.RightClick.performed += HandleRightClick;
        }

        private void OnDisable()
        {
            _actions.UI.Click.performed -= HandleLeftClick;
            _actions.UI.RightClick.performed -= HandleRightClick;

            _actions.Disable();
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        private void HandleLeftClick(InputAction.CallbackContext ctx)
        {
            var pos = Pointer.current != null 
                ? Pointer.current.position.ReadValue() 
                : Vector2.zero;

            OnLeftClick?.Invoke(pos);
        }

        private void HandleRightClick(InputAction.CallbackContext ctx)
        {
            var pos = Pointer.current != null 
                ? Pointer.current.position.ReadValue() 
                : Vector2.zero;

            OnRightClick?.Invoke(pos);
        }
    }
}
