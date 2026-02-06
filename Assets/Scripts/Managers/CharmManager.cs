using System;
using System.Collections.Generic;
using System.Linq;
using Events;
using Events.Core;
using Events.Game;
using Models;
using Models.Charms;
using UnityEngine;
using UnityEngine.Rendering;
using Views;

namespace Managers
{
    public class CharmManager : MonoBehaviourSingleton<CharmManager>
    {
        [field: SerializeField] public List<Charm> ActiveCharms { get; private set; }
        [field: SerializeField] public SerializedDictionary<Charm, CharmView> CharmViews { get; private set; } = new(); 
        
        [SerializeField] private CharmView charmPrefab;
        [SerializeField] private Transform charmContainer;

        public List<Charm> OnWordStartCharms => ActiveCharms
            .Where(c => c.EffectTrigger == EffectTrigger.OnWordStart).ToList()
            .OrderBy(c => c.PriorityOverride).ToList();  
        
        public List<Charm> OnWordEndCharms => ActiveCharms
            .Where(c => c.EffectTrigger == EffectTrigger.OnWordEnd).ToList()
            .OrderBy(c => c.PriorityOverride).ToList();

        public List<Charm> OnLetterCharms => ActiveCharms
            .Where(c => c.EffectTrigger == EffectTrigger.OnLetter).ToList() 
            .OrderBy(c => c.PriorityOverride).ToList();  

        #region Mono
        private void OnEnable()
        {
            Bus<BoardUpdatedEvent>.OnEvent += HandleOnBoardUpdated;
        }

        private void OnDisable()
        {
            Bus<BoardUpdatedEvent>.OnEvent -= HandleOnBoardUpdated;
        }
        #endregion
        
        private void HandleOnBoardUpdated(BoardUpdatedEvent evt)
        {
            var context = GameManager.Instance.Run.Round;

            foreach (var charm in ActiveCharms)
            {
                var willTrigger = WillCharmTrigger(charm, evt.Word, evt.Tiles);
                var view = CharmViews[charm];
                view.SetActiveFeedback(willTrigger);
            }      
        }
        
        public void InitializeCharms()
        {
            foreach (var charm in ActiveCharms)
            {
                var charmView = Instantiate(charmPrefab, charmContainer);
                charmView.Populate(charm);
                
                CharmViews[charm] = charmView;
            }
        }

        public CharmView GetCharmViewFromCharm(Charm charm)
        {
            return CharmViews[charm];
        }
        
        private bool WillCharmTrigger(Charm charm, string word, List<Tile> tiles)
        {
            switch (charm.EffectTrigger)
            {
                case EffectTrigger.OnWordStart:
                    return charm.WillPreviewEffect(word);

                case EffectTrigger.OnWordEnd:
                    return tiles.Count > 0 && charm.WillPreviewEffect(word);

                case EffectTrigger.OnLetter:
                    for (var i = 0; i < tiles.Count; i++)
                    {
                        if (charm.WillPreviewEffect(word, tiles, i))
                            return true;
                    }
                    return false;

                default:
                    return false;
            }
        }
    }
}
