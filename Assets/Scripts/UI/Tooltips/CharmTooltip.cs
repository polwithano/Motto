using TMPro;
using UnityEngine;
using Models.Charms;
using Models.Charms.Core;
using UI.Helpers;

namespace UI.Tooltips
{
    public class CharmTooltip : TooltipBase
    {
        [Header("Charm-Specific UI")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        public override void Populate(object data)
        {
            if (data is not Charm charm)
            {
                Debug.LogWarning("CharmTooltip received invalid data type.");
                return;
            }

            titleText.text = charm.CharmName;
            descriptionText.text = CharmDescriptionParser.Parse(charm.CharmDescription, charm);
        }
    }
}