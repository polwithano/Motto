using TMPro;
using UI.Containers.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Containers
{
    public class UIRoundOver : UIContainer
    {
        [SerializeField] private GameObject recapEntryPrefab;
        [SerializeField] private TextMeshProUGUI totalLabel; 
        [SerializeField] private Button continueButton;
        
        protected override void Open()
        {
            base.Open();
            
            BeginRecap();
        }

        protected override void Close()
        {
            base.Close();
        }

        private void BeginRecap()
        {
            continueButton.interactable = false;
            totalLabel.text = ""; 
        }
    }
}
