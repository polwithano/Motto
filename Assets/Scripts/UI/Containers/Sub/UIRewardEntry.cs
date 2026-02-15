using System.Threading.Tasks;
using Models.Rewards;
using TMPro;
using UnityEngine;

namespace UI.Containers.Sub
{
    public class UIRewardEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private TextMeshProUGUI value;
        
        public void PopulateEntry(RoundRewardEntry entry)
        {
            var title = $"{entry.Label} ({entry.RawValue})";
            
            label.text = title;
            value.text = $"{entry.SoftReward}$";
        }

        public async Task AnimateEntry(RoundRewardEntry entry)
        {
            label.text = $"{entry.Label} ({entry.RawValue})";
            
            var displayed = 0; 
            var target = entry.SoftReward;
            
            var duration = .5f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t =  elapsed / duration;
                
                displayed = Mathf.RoundToInt(Mathf.Lerp(0, target, t));
                value.text = $"+{displayed}$";

                await Task.Yield(); 
            }
            
            value.text = $"+{target}$";

        }
    }
}