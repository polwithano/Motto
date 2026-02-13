using Events.Core;
using Events.UI;
using Managers;
using Models.Charms;
using Models.Charms.Core;
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
            Bus<DisplayTooltipEvent<Charm>>.Raise(new DisplayTooltipEvent<Charm>(charmData));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipManager.Instance.HideTooltip<Charm>();
        }
    }
}