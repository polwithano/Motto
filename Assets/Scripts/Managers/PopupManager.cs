using Events;
using Events.Core;
using Events.Score;
using Interfaces;
using Models;
using Models.Charms;
using Models.Charms.Core;
using UI.Popup;
using UnityEngine;

namespace Managers
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private Canvas popupCanvas; 
        [SerializeField] private ScorePopup scorePopupPrefab;
        
        #region Mono
        private void OnEnable()
        {
            Bus<ScoringStepStartedEvent>.OnEvent += HandleOnScoreStepStarted; 
        }

        private void OnDisable()
        {
            Bus<ScoringStepStartedEvent>.OnEvent -= HandleOnScoreStepStarted; 
        }

        private void OnDestroy() => OnDisable();
        #endregion
    
        #region Subscribed
        private void HandleOnScoreStepStarted(ScoringStepStartedEvent evt)
        {
            foreach (var effect in evt.Entry.ScoreEffects)
            {
                var popupPosition = Vector3.zero;
                if (TryGetEffectEmitterPosition(evt.Entry.Emitter, out popupPosition))
                {
                    SpawnPopup(popupPosition, effect); 
                }
            }
        }
        #endregion
        
        private void SpawnPopup(Vector3 pos, ScoreEffect effect)
        {
            var popup = Instantiate(scorePopupPrefab, popupCanvas.transform);
            popup.transform.position = pos;
            popup.Play((int)effect.Value, effect.Target);
        }

        private bool TryGetEffectEmitterPosition(IEffectEmitter emitter, out Vector3 position)
        {
            position = Vector3.zero;

            if (emitter is Tile tile)
            {
                var view = BoardManager.Instance.GetTileViewFromTile(tile);
                position = view.transform.position;
                return true; 
            }

            if (emitter is Charm charm)
            {
                var view = CharmManager.Instance.GetCharmViewFromCharm(charm);
                position = view.transform.position;
                return true;
            }

            return false; 
        }
    }
}
