using System.Collections.Generic;
using UnityEngine;

namespace Editor
{
    [CreateAssetMenu(fileName = "UnitDatabase", menuName = "Units/UnitDatabase")]
    public class UnitDatabase : ScriptableObject
    {
        [SerializeField] public List<UnitGroup> unitGroups;
    }
}
