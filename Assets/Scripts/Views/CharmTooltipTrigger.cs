using Events;
using Managers;
using Models.Charms;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Views
{
    public class CharmTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            var charmData = GetComponent<CharmView>().Charm;
            if (charmData == null) return;
            GameEvents.RaiseOnCharmFocus(charmData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideTooltip<Charm>();
        }
    }
}