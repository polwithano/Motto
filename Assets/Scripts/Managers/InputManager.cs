using System;
using Inputs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class InputManager : MonoBehaviourSingleton<InputManager>
    {
        private BaseInputActions _actions;

        public event Action<Vector2> OnLeftClick; 
        public event Action<Vector2> OnRightClick;
        
        [field: SerializeField] public Vector2 PointerPosition { get; private set; }

        [field: SerializeField] public bool PointerDown { get; private set; }
        [field: SerializeField] public bool PointerJustPressed { get; private set; }
        [field: SerializeField] public bool PointerJustReleased { get; private set; }
        [field: SerializeField] public bool IsDragging { get; private set; }
        
        private Vector2 _pressStartPosition;
        private const byte DragThreshold = 64;
        
        #region Mono
        private void Awake()
        {
          _actions = new BaseInputActions();
        }
        
        private void LateUpdate()
        {
            if (PointerJustReleased && !IsDragging)
                OnLeftClick?.Invoke(PointerPosition);

            PointerJustPressed = false;
            PointerJustReleased = false;

            if (!PointerDown)
                IsDragging = false;
        }
        private void OnEnable()
        {
            _actions.Enable();
            
            _actions.UI.RightClick.performed += HandleRightClick;
            _actions.UI.Press.performed += OnPressStarted;
            _actions.UI.Press.canceled += OnPressCanceled; 
            _actions.UI.Point.performed += OnPointerMoved;
        }

        private void OnDisable()
        {
            _actions.UI.RightClick.performed -= HandleRightClick;
            _actions.UI.Press.performed -= OnPressStarted;
            _actions.UI.Press.canceled -= OnPressCanceled; 
            _actions.UI.Point.performed -= OnPointerMoved;

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
        
        private void OnPointerMoved(InputAction.CallbackContext ctx)
        {
            PointerPosition = ctx.ReadValue<Vector2>();
            
            if (PointerDown && !IsDragging)
            {
                if (Vector2.Distance(_pressStartPosition, PointerPosition) > DragThreshold)
                    IsDragging = true;
            }
        }
        
        private void OnPressStarted(InputAction.CallbackContext ctx)
        {
            PointerDown = true;
            PointerJustPressed = true;
            IsDragging = false;

            var pos = Pointer.current != null ? Pointer.current.position.ReadValue() : PointerPosition;

            _pressStartPosition = pos;
            PointerPosition = pos;
        }

        private void OnPressCanceled(InputAction.CallbackContext ctx)
        {
            PointerDown = false;
            PointerJustReleased = true;
        }
    }
}
