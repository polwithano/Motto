using System.Collections.Generic;
using Events;
using Events.Core;
using Events.Game;
using Models;
using UnityEngine;
using Views;

namespace Animation
{
    [RequireComponent(typeof(RectTransform))]
    public class UIRippleController : MonoBehaviour
    {
        [System.Serializable]
        public struct Ripple
        {
            public Vector2 uv;      // 0..1
            public float startTime; // Time.time
            public float amplitude; // typical 1
            public float frequency; // ~40..120
            public float speed;     // ~0.3..2 (uv units/sec)
            public float damping;   // ~0.8..4
            public float width;     // ~0.02..0.08
        }

        public Material targetMaterial;   // Material using UI/RippleMultiSource
        public int maxSources = 8;

        public float amplitude = 1f;
        public float frequency = 80f;
        public float speed = 0.9f;
        public float damping = 2.0f;
        public float width = 0.035f;

        readonly List<Ripple> ripples = new();
        static readonly int MaxSourcesID = Shader.PropertyToID("_MaxSources");
        static readonly int SourcesID = Shader.PropertyToID("_RippleSources");
        static readonly int ParamsID = Shader.PropertyToID("_RippleParams");

        // Arrays must match shader max (32 here)
        readonly Vector4[] sources = new Vector4[32];
        readonly Vector4[] parms = new Vector4[32];

        RectTransform rt;
        Canvas canvas;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
        }

        void Update()
        {
            if (!targetMaterial) return;
        
            // Optionnel: nettoyer des ripples trop vieux
            float now = Time.time;
            for (int i = ripples.Count - 1; i >= 0; i--)
            {
                // après ~5s, on retire (ajuste selon ton damping/speed)
                if (now - ripples[i].startTime > 5f) ripples.RemoveAt(i);
            }

            // Remplir buffers (dernier en premier = priorité)
            int count = Mathf.Min(maxSources, ripples.Count);
            for (int i = 0; i < 32; i++)
            {
                sources[i] = Vector4.zero;
                parms[i] = Vector4.zero;
            }

            for (int i = 0; i < count; i++)
            {
                var r = ripples[ripples.Count - 1 - i];
                sources[i] = new Vector4(r.uv.x, r.uv.y, r.startTime, r.amplitude);
                parms[i] = new Vector4(r.frequency, r.speed, r.damping, r.width);
            }

            targetMaterial.SetInt(MaxSourcesID, Mathf.Min(maxSources, 32));
            targetMaterial.SetVectorArray(SourcesID, sources);
            targetMaterial.SetVectorArray(ParamsID, parms);
        }

        private void OnEnable()
        {
            Bus<TileMoveCompletedEvent>.OnEvent += HandleOnTileMoveCompleted; 
        }

        private void OnDisable()
        {
            Bus<TileMoveCompletedEvent>.OnEvent -= HandleOnTileMoveCompleted; 
        }
        
        private void HandleOnTileMoveCompleted(TileMoveCompletedEvent evt)
        {
            if (!targetMaterial) return;

            if (WorldToUV(evt.View.transform.position, out var uv))
            {
                AddRipple(uv);
            }
        }

        bool WorldToUV(Vector3 worldPos, out Vector2 uv)
        {
            uv = Vector2.zero;

            if (!rt) return false;

            Camera cam = null;

            if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                cam = canvas.worldCamera;

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, cam, out local))
                return false;

            Rect rect = rt.rect;

            float x = (local.x - rect.xMin) / rect.width;
            float y = (local.y - rect.yMin) / rect.height;

            uv = new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));
            return true;
        }
    
        void AddRipple(Vector2 uv)
        {
            Ripple r = new Ripple
            {
                uv = uv,
                startTime = Time.time,
                amplitude = amplitude,
                frequency = frequency,
                speed = speed,
                damping = damping,
                width = width
            };

            ripples.Add(r);

            // Hard cap to avoid unbounded growth
            int hardCap = 64;
            if (ripples.Count > hardCap) ripples.RemoveAt(0);
        }

        bool TryGetUV(Vector2 screenPos, Camera cam, out Vector2 uv)
        {
            uv = Vector2.zero;

            if (!rt) return false;

            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, cam, out local))
                return false;

            Rect rect = rt.rect;
            float x = (local.x - rect.xMin) / rect.width;
            float y = (local.y - rect.yMin) / rect.height;

            uv = new Vector2(Mathf.Clamp01(x), Mathf.Clamp01(y));
            return true;
        }
    }
}
