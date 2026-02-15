using System;
using System.Threading.Tasks;
using Managers;
using Models.Rewards;
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
        
        protected async override void Open()
        {
            base.Open();
            
            var result = GameManager.Instance.Run.CurrentRoundResult;
            if (result == null) return;

            totalLabel.text = "Total: 0$"; 
            continueButton.interactable = false;

            await BeginRecap(result);
            await AnimateTotal(result);

            continueButton.interactable = true;
        }

        protected override void Close()
        {
            base.Close();
            
            DestroyRewardEntries();
        }

        private async Task BeginRecap(RoundRewardResult result)
        {
            foreach (var entry in result.Entries)
            {
                var go = Instantiate(recapEntryPrefab, scrollViewRoot);
                var view = go.GetComponent<UIRewardEntry>();

                await view.AnimateEntry(entry);
            }
        }

        private async Task AnimateTotal(RoundRewardResult result)
        {
            var displayed = 0;
            var target = result.TotalCurrency;

            var duration = 0.75f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                displayed = Mathf.RoundToInt(Mathf.Lerp(0, target, t));
                totalLabel.text = $"Total: +{displayed}$";

                await Task.Yield();
            }

            totalLabel.text = $"Total: +{target}$";
        }
        
        private void DestroyRewardEntries()
        {
            for (var i = 0; i < scrollViewRoot.childCount; i++)
            {
                Destroy(scrollViewRoot.GetChild(i).gameObject);
            }
        }
    }
}
