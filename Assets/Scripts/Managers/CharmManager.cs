using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
