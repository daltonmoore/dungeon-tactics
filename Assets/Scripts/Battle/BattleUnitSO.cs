using Battle;
using UnityEngine;

namespace Units
{
    [CreateAssetMenu(fileName = "Battle Unit", menuName = "Units/Battle Unit")]
    public class BattleUnitSO : AbstractBattleUnitSO
    {
        [SerializeField] public Sprite sprite;
    }
}