using System.Collections.Generic;
using UnityEngine;

namespace Models.Charms.Core
{
    [CreateAssetMenu(fileName = "CharmsDB", menuName = "Charms/CharmsDB", order = 999)]
    public class CharmDatabase : ScriptableObject
    {
        [field: SerializeField] public List<Charm> Charms { get; private set; }
    }
}
