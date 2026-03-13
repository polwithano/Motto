using System.Collections.Generic;
using Events.Core;
using Events.Game;
using Managers;
using UnityEngine;

namespace Animation.Backgrounds
{
    [RequireComponent(typeof(RectTransform))]
    public class UIRippleController : MonoBehaviour
    {
        [System.Serializable]
        public struct Ripple
        {
            public Vector2 uv;
            public float startTime;
            public float amplitude;
            public float frequency;
            public float speed;
            public float damping;
            public float width;
        }

        private readonly List<Ripple> ripples = new();

        private const int   MAX_RIPPLES         = 8;
        private const float MAX_RIPPLE_LIFESPAN = 10f;

        static readonly int MaxSourcesID = Shader.PropertyToID("_MaxSources");
        static readonly int SourcesID    = Shader.PropertyToID("_RippleSources");
        static readonly int ParamsID     = Shader.PropertyToID("_RippleParams");

        [Header("References")]
        [SerializeField] private Material target;

        [Header("Parameters")]
        [SerializeField] private bool  userInputs = false;
        [SerializeField] private float amplitude  = 1f;
        [SerializeField] private float frequency  = 80f;
        [SerializeField] private float speed      = 0.9f;
        [SerializeField] private float damping    = 2.0f;
        [SerializeField] private float width      = 0.035f;

        readonly Vector4[] sources = new Vector4[MAX_RIPPLES];
        readonly Vector4[] parms   = new Vector4[MAX_RIPPLES];

        private RectTransform _transform;
        private Camera        _camera;
        private Canvas        _canvas;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
            _canvas    = GetComponentInParent<Canvas>();
            _camera    = Camera.main;
        }

        private void Update()
        {
            if (!target) return;

            RippleCleanup();

            for (var i = 0; i < MAX_RIPPLES; i++)
            {
                sources[i] = Vector4.zero;
                parms[i]   = Vector4.zero;
            }

            var count = Mathf.Min(MAX_RIPPLES, ripples.Count);
            for (var i = 0; i < count; i++)
            {
                var r= ripples[ripples.Count - 1 - i];
                sources[i] = new Vector4(r.uv.x, r.uv.y, r.startTime, r.amplitude);
                parms[i]   = new Vector4(r.frequency, r.speed, r.damping, r.width);
            }

            target.SetInt(MaxSourcesID, MAX_RIPPLES);
            target.SetVectorArray(SourcesID, sources);
            target.SetVectorArray(ParamsID,  parms);
        }

        #region Event Handlers

        private void OnEnable()
        {
            Bus<TileMoveCompletedEvent>.OnEvent += HandleOnTileMoveCompleted;

            if (userInputs && InputManager.Instance != null)
                InputManager.Instance.OnLeftClick += HandlePointerClick;
        }

        private void OnDisable()
        {
            Bus<TileMoveCompletedEvent>.OnEvent -= HandleOnTileMoveCompleted;

            if (InputManager.Instance != null)
                InputManager.Instance.OnLeftClick -= HandlePointerClick;
        }

        private void HandleOnTileMoveCompleted(TileMoveCompletedEvent evt)
        {
            if (!target) return;
            if (WorldToUV(evt.View.transform.position, out var uv))
                AddRipple(uv);
        }

        /// <summary>
        /// Triggered on left click / tap when userInputs is true.
        /// screenPos is the raw screen-space pointer position from InputManager.
        /// </summary>
        private void HandlePointerClick(Vector2 screenPos)
        {
            if (!target || !userInputs) return;
            if (ScreenToUV(screenPos, out var uv))
                AddRipple(uv);
        }

        #endregion

        private void RippleCleanup()
        {
            var now = Time.time;
            for (var i = ripples.Count - 1; i >= 0; i--)
                if (now - ripples[i].startTime > MAX_RIPPLE_LIFESPAN)
                    ripples.RemoveAt(i);
        }

        /// <summary>
        /// Converts a world-space position to a UV on this RectTransform.
        /// </summary>
        private bool WorldToUV(Vector3 worldPos, out Vector2 uv)
        {
            uv = Vector2.zero;
            if (!_transform) return false;

            if (_canvas && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _camera = _canvas.worldCamera;

            var screenPos = RectTransformUtility.WorldToScreenPoint(_camera, worldPos);
            return ScreenToUV(screenPos, out uv);
        }

        /// <summary>
        /// Converts a screen-space position to a UV on this RectTransform.
        /// </summary>
        private bool ScreenToUV(Vector2 screenPos, out Vector2 uv)
        {
            uv = Vector2.zero;
            if (!_transform) return false;

            if (_canvas && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                _camera = _canvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _transform, screenPos, _camera, out var local))
                return false;

            var rect = _transform.rect;
            uv = new Vector2(
                Mathf.Clamp01((local.x - rect.xMin) / rect.width),
                Mathf.Clamp01((local.y - rect.yMin) / rect.height));
            return true;
        }

        private void AddRipple(Vector2 uv)
        {
            ripples.Add(new Ripple
            {
                uv        = uv,
                startTime = Time.time,
                amplitude = amplitude,
                frequency = frequency,
                speed     = speed,
                damping   = damping,
                width     = width
            });

            if (ripples.Count > MAX_RIPPLES) ripples.RemoveAt(0);
        }
    }
}