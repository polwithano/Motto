using Events.Game;
using UI.Components.Core;
using UnityEngine;

namespace UI.Components
{
    public class DrawsCountComponent : ReactiveLabelComponent<TileRedrawCompletedEvent, int>
    {
        protected override int ExtractValue(TileRedrawCompletedEvent evt) 
            => evt.RemainingDrawsCount; 
    }
}