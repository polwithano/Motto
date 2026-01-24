using System.Collections;
using System.Collections.Generic;
using Events;
using Models.Charms;
using UI.Tooltips;
using UnityEngine;

namespace Managers
{
    public class TooltipManager : MonoBehaviourSingleton<TooltipManager>
    {
        [SerializeField] private RectTransform canvasRect;
        
        [Header("Tooltip Prefabs")]
        [SerializeField] private CharmTooltip charmTooltipPrefab;

        private readonly Dictionary<System.Type, TooltipBase> _activeTooltips = new();
        private System.Type _currentActiveType;
        private Coroutine _hideRoutine;
        
        #region Mono
        private void OnEnable()
        {
            GameEvents.OnCharmFocus += HandleOnCharmFocus;
        }

        private void OnDisable()
        {
            GameEvents.OnCharmFocus -= HandleOnCharmFocus;
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed

        private void HandleOnCharmFocus(Charm charm)
        {
            ShowTooltip(charm);
        }
        #endregion

        private void ShowTooltip<T>(T data) where T : class
        {
            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
                _hideRoutine = null;
            }

            var tooltip = GetTooltipInstance<T>();
            _currentActiveType = typeof(T);
            tooltip.Populate(data);
            tooltip.Show();
        }

        public void HideTooltip<T>() where T : class
        {
            var type = typeof(T);

            // ✅ Delay the hide slightly to prevent flicker when quickly moving between charms
            if (_hideRoutine != null)
                StopCoroutine(_hideRoutine);

            _hideRoutine = StartCoroutine(DelayedHide(type, 0.05f));
        }

        private IEnumerator DelayedHide(System.Type type, float delay)
        {
            yield return new WaitForSeconds(delay);

            // Only hide if no tooltip was re-shown during the delay
            if (_currentActiveType == type)
            {
                var tooltip = GetTooltipInstanceByType(type);
                tooltip?.Hide();
                _currentActiveType = null; // ✅ Reset active type
            }

            _hideRoutine = null;
        }

        private TooltipBase GetTooltipInstance<T>() where T : class
        {
            var type = typeof(T);
            if (_activeTooltips.ContainsKey(type))
                return _activeTooltips[type];

            TooltipBase prefab = null;
            if (type == typeof(Models.Charms.Charm))
                prefab = charmTooltipPrefab;

            if (prefab == null)
            {
                Debug.LogError($"No tooltip prefab assigned for type {type.Name}");
                return null;
            }

            TooltipBase instance = Instantiate(prefab, canvasRect);
            _activeTooltips.Add(type, instance);
            return instance;
        }

        private TooltipBase GetTooltipInstanceByType(System.Type type)
        {
            if (_activeTooltips.TryGetValue(type, out var tooltip))
                return tooltip;
            return null;
        }
    }
}
