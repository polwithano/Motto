using System.Collections;
using System.Collections.Generic;
using Events;
using Events.Core;
using Events.UI;
using Models.Charms;
using Models.Charms.Core;
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
            Bus<DisplayTooltipEvent<Charm>>.OnEvent += HandleOnCharmFocus; 
        }
        
        private void OnDisable()
        {
            Bus<DisplayTooltipEvent<Charm>>.OnEvent -= HandleOnCharmFocus; 
        }
        
        private void OnDestroy() => OnDisable();
        #endregion
        
        #region Subscribed
        private void HandleOnCharmFocus(DisplayTooltipEvent<Charm> evt)
        {
            ShowTooltip(evt.ModelToDisplay);
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

            if (_hideRoutine != null)
                StopCoroutine(_hideRoutine);

            _hideRoutine = StartCoroutine(DelayedHide(type, 0.05f));
        }

        private IEnumerator DelayedHide(System.Type type, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (_currentActiveType == type)
            {
                var tooltip = GetTooltipInstanceByType(type);
                tooltip?.Hide();
                _currentActiveType = null; 
            }

            _hideRoutine = null;
        }

        private TooltipBase GetTooltipInstance<T>() where T : class
        {
            var type = typeof(T);
            if (_activeTooltips.TryGetValue(type, out var tooltipInstance))
                return tooltipInstance;

            TooltipBase prefab = null;
            if (type == typeof(Charm))
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
