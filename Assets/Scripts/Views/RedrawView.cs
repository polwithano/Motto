using Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Views
{
    public class RedrawView : MonoBehaviour
    {
        [SerializeField] private RectTransform lidTransform;
        [SerializeField] private Image lidFace; 
        [SerializeField] private Vector3 lidOpenedPosition; 
        [SerializeField] private Vector3 lidClosedPosition;
        [SerializeField] private Vector3 lidOpenedRotation; 
        [SerializeField] private Vector3 lidClosedRotation;
        [SerializeField] private Sprite lidOpenedFace; 
        [SerializeField] private Sprite lidClosedFace;

        public void SetHovered(bool hovered)
        {
            SetVisualState(hovered);
        }

        private void SetVisualState(bool enabled)
        {
            var position = enabled ? lidOpenedPosition : lidClosedPosition;
            var rotation = enabled ? lidOpenedRotation :  lidClosedRotation;
            var sprite = enabled ? lidOpenedFace : lidClosedFace;
            
            lidTransform.anchoredPosition = position;
            lidTransform.localRotation = Quaternion.Euler(rotation);
            lidFace.sprite = sprite;
        }
    }
}
