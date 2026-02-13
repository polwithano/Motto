using Events.Core;
using Events.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UpdateUIStateComponent : MonoBehaviour
    {
        [SerializeField] private UIType type; 
        [SerializeField] private UIState state;
        
        public void PushEvent() => Bus<SetUIContainerStateEvent>.Raise(new SetUIContainerStateEvent(type, state));
    }
}