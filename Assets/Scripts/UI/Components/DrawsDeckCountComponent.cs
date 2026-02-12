using Events.Game;
using UI.Components.Core;
using UnityEngine;

namespace UI.Components
{
    public class DeckDrawCountComponent : ReactiveLabelComponent<TileRedrawCompletedEvent, int>
    {
        protected override int ExtractValue(TileRedrawCompletedEvent evt)
            => evt.RemainingCardsInDeck; 
    }
}