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
    }
}