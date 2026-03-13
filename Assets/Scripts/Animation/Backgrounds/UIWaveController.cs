using System.Collections.Generic;
using Events.Core;
using Events.Game;
using UnityEngine;

namespace Animation.Backgrounds
{
    /// <summary>
    /// Drives FluidWaveURP.
    ///
    /// Physics — spring-damper:
    ///   a = −k·(h − target) − c·v    where c = 2·ζ·√k
    ///
    /// Turbulence — proportional to |v|, decays exponentially.
    ///
    /// Droplets — always launched from the wave crest at a random X.
    ///   Small drop  : single ejection on rising edge of dropSmallThreshold.
    ///   Large splash: burst on rising edge of dropLargeThreshold.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIWaveController : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("Materials")]
        public Material waveMaterial;           // FluidWaveURP
        public Material checkerboardMaterial;   // CheckerboardRippleURP (optional sync)

        [Header("Spring physics")]
        [Tooltip("Stiffness. Higher = snappier.")]
        public float springStrength = 20f;
        [Tooltip("1 = critically damped. < 1 = bouncy. > 1 = overdamped.")]
        [Range(0.1f, 2f)]
        public float dampingRatio   = 0.52f;

        [Header("Turbulence")]
        [Tooltip("|v| that maps to turbulence = 1 in the shader.")]
        public float turbulenceScale = 4.0f;
        [Tooltip("Exponential decay rate (per second).")]
        public float turbulenceDecay = 2.5f;

        [Header("Droplets — small")]
        [Tooltip("|v| threshold for a single small drop (rising-edge only).")]
        public float dropSmallThreshold = 0.55f;
        [Range(0.006f, 0.025f)]
        public float dropRadiusSmall    = 0.013f;
        public float dropSmallSpeedMin  = 0.28f;
        public float dropSmallSpeedMax  = 0.52f;

        [Header("Droplets — large splash")]
        [Tooltip("|v| threshold for a burst (rising-edge only).")]
        public float dropLargeThreshold = 2.0f;
        [Range(0.020f, 0.070f)]
        public float dropRadiusLarge    = 0.038f;
        [Range(2, 12)]
        public int   splashDropCount    = 7;
        public float splashSpeedMin     = 0.45f;
        public float splashSpeedMax     = 0.95f;
        [Tooltip("Angular spread of the fan in degrees (centred on straight up).")]
        [Range(20f, 170f)]
        public float splashAngleSpread  = 115f;

        [Header("Droplets — shared")]
        public float dropCooldown = 0.07f;
        [Range(1, 16)]
        public int   maxDrops     = 12;

        [Header("Rect")]
        [Tooltip("Leave 0 to auto-compute from RectTransform.")]
        public float rectAspect = 0f;

        // ── State ─────────────────────────────────────────────────────────────

        float _height     = 0.5f;
        float _target     = 0.5f;
        float _velocity   = 0f;
        float _turbulence = 0f;
        float _dropTimer  = 0f;
        bool  _wasSmall   = false;
        bool  _wasLarge   = false;

        // ── Drop pool ─────────────────────────────────────────────────────────

        struct Drop
        {
            public float spawnX, spawnY;
            public float spawnTime;
            public float radius;
            public float vx, vy;
        }

        readonly List<Drop> _drops  = new();
        readonly Vector4[]  _srcBuf = new Vector4[16];
        readonly Vector4[]  _velBuf = new Vector4[16];

        // ── Shader IDs ────────────────────────────────────────────────────────

        static readonly int ID_WaveHeight     = Shader.PropertyToID("_WaveHeight");
        static readonly int ID_WaveTurb       = Shader.PropertyToID("_WaveTurbulence");
        static readonly int ID_DropSources    = Shader.PropertyToID("_DropSources");
        static readonly int ID_DropVelocities = Shader.PropertyToID("_DropVelocities");
        static readonly int ID_MaxDrops       = Shader.PropertyToID("_MaxDrops");
        static readonly int ID_RectAspect     = Shader.PropertyToID("_RectAspect");
        static readonly int ID_SplitY         = Shader.PropertyToID("_SplitY");

        RectTransform _rt;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        void Awake()  => _rt = GetComponent<RectTransform>();
        void Start()  { _height = _target; PushToGPU(); }

        void OnEnable()  => Bus<TileMoveCompletedEvent>.OnEvent += HandleTileMoveCompleted;
        void OnDisable() => Bus<TileMoveCompletedEvent>.OnEvent -= HandleTileMoveCompleted;

        void Update()
        {
            float dt = Time.deltaTime;

            // Spring-damper
            float omega = Mathf.Sqrt(Mathf.Max(springStrength, 0.01f));
            float c     = 2f * dampingRatio * omega;
            _velocity  += (-springStrength * (_height - _target) - c * _velocity) * dt;
            _height    += _velocity * dt;

            // Turbulence
            float velMag  = Mathf.Abs(_velocity);
            _turbulence   = Mathf.Max(_turbulence, Mathf.Clamp01(velMag / turbulenceScale));
            _turbulence   = Mathf.Lerp(_turbulence, 0f, turbulenceDecay * dt);

            // Droplet spawning — rising-edge detection
            _dropTimer   -= dt;
            bool nowSmall = velMag > dropSmallThreshold;
            bool nowLarge = velMag > dropLargeThreshold;

            if (_dropTimer <= 0f)
            {
                if (nowLarge && !_wasLarge)
                {
                    SpawnCrestSplash();
                    _dropTimer = dropCooldown * 2.5f;
                }
                else if (nowSmall && !_wasSmall)
                {
                    SpawnCrestDrop();
                    _dropTimer = dropCooldown;
                }
            }
            _wasSmall = nowSmall;
            _wasLarge = nowLarge;

            // Expire drops (lifetime = 2 s, matches shader)
            float now = Time.time;
            _drops.RemoveAll(d => (now - d.spawnTime) > 2.15f);

            PushToGPU();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Animate wave to a new height (0 = bottom, 1 = top).</summary>
        public void SetTargetHeight(float h) => _target = Mathf.Clamp01(h);

        /// <summary>Teleport with no spring transition.</summary>
        public void SnapToHeight(float h)
        {
            _height = _target = Mathf.Clamp01(h);
            _velocity = 0f;
        }

        // ── Event handler ─────────────────────────────────────────────────────

        void HandleTileMoveCompleted(TileMoveCompletedEvent evt)
        {
            // Adapt to your game logic — this is a placeholder nudge.
            float nudge = Random.Range(0.008f, 0.022f)
                        * (Random.value > 0.5f ? 1f : -1f);
            SetTargetHeight(_target + nudge);
        }

        // ── Internal — drop spawning from crest ───────────────────────────────

        void SpawnCrestDrop()
        {
            float angle = Random.Range(65f, 115f) * Mathf.Deg2Rad;
            float speed = Random.Range(dropSmallSpeedMin, dropSmallSpeedMax);
            AddDrop(new Drop
            {
                spawnX    = Random.value,
                spawnY    = _height,
                spawnTime = Time.time,
                radius    = dropRadiusSmall,
                vx        = Mathf.Cos(angle) * speed,
                vy        = Mathf.Sin(angle) * speed
            });
        }

        void SpawnCrestSplash()
        {
            // All drops from the same X column for a coherent geyser shape
            float spawnX     = Random.value;
            float halfSpread = splashAngleSpread * 0.5f * Mathf.Deg2Rad;

            for (int i = 0; i < splashDropCount; i++)
            {
                float angle = Mathf.PI * 0.5f + Random.Range(-halfSpread, halfSpread);
                float speed = Random.Range(splashSpeedMin, splashSpeedMax);
                float r     = dropRadiusLarge * Random.Range(0.50f, 1.40f);

                AddDrop(new Drop
                {
                    spawnX    = spawnX + Random.Range(-0.03f, 0.03f),
                    spawnY    = _height,
                    spawnTime = Time.time + i * 0.016f,   // stagger burst
                    radius    = r,
                    vx        = Mathf.Cos(angle) * speed,
                    vy        = Mathf.Sin(angle) * speed
                });
            }
        }

        void AddDrop(Drop d)
        {
            _drops.Add(d);
            if (_drops.Count > maxDrops) _drops.RemoveAt(0);
        }

        // ── GPU upload ────────────────────────────────────────────────────────

        void PushToGPU()
        {
            float aspect = rectAspect > 0f
                ? rectAspect
                : _rt.rect.width / Mathf.Max(_rt.rect.height, 1f);

            if (waveMaterial)
            {
                waveMaterial.SetFloat(ID_WaveHeight, _height);
                waveMaterial.SetFloat(ID_WaveTurb,   _turbulence);
                waveMaterial.SetFloat(ID_RectAspect, aspect);
                waveMaterial.SetInt  (ID_MaxDrops,   Mathf.Min(maxDrops, 16));

                for (int i = 0; i < 16; i++) { _srcBuf[i] = _velBuf[i] = Vector4.zero; }

                int cnt = Mathf.Min(_drops.Count, 16);
                for (int i = 0; i < cnt; i++)
                {
                    var d = _drops[_drops.Count - 1 - i]; // most recent first
                    _srcBuf[i] = new Vector4(d.spawnX, d.spawnY, d.spawnTime, d.radius);
                    _velBuf[i] = new Vector4(d.vx, d.vy, 0f, 0f);
                }

                waveMaterial.SetVectorArray(ID_DropSources,    _srcBuf);
                waveMaterial.SetVectorArray(ID_DropVelocities, _velBuf);
            }

            // Keep checkerboard A/B boundary in sync with wave height
            if (checkerboardMaterial)
                checkerboardMaterial.SetFloat(ID_SplitY, _height);
        }

        // ── Editor helpers ────────────────────────────────────────────────────
#if UNITY_EDITOR
        [ContextMenu("TEST — Wave Up (0.72)")]
        void TestUp()     => SetTargetHeight(0.72f);

        [ContextMenu("TEST — Wave Down (0.28)")]
        void TestDown()   => SetTargetHeight(0.28f);

        [ContextMenu("TEST — Centre (0.50)")]
        void TestCentre() => SetTargetHeight(0.50f);

        [ContextMenu("TEST — Snap Top (0.90)")]
        void TestSnap()   => SnapToHeight(0.90f);

        [ContextMenu("TEST — Force Big Splash")]
        void TestSplash() => SpawnCrestSplash();

        [ContextMenu("TEST — Force Single Drop")]
        void TestDrop()   => SpawnCrestDrop();
#endif
    }
}