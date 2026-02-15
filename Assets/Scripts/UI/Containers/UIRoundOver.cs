using Managers;
using TMPro;
using UI.Containers.Core;
using UI.Containers.Sub;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Containers
{
    public class UIRoundOver : UIContainer
    {
        [SerializeField] private RectTransform scrollViewRoot; 
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

            var rewardResult = GameManager.Instance.Run.CurrentRoundResult;
            if (rewardResult == null) return;

            var accumulated = 0; 
            
            foreach (var entry in rewardResult.Entries)
            {
                var go =  Instantiate(recapEntryPrefab, scrollViewRoot);
                var view = go.GetComponent<UIRewardEntry>(); 
                view.PopulateEntry(entry);
                
                accumulated += entry.SoftReward;
                totalLabel.text = $"+{accumulated}$";
            }
            
            continueButton.interactable = true;
        }
    }
}
