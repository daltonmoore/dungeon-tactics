using Units;
using UnityEngine;

namespace Battle
{
    public abstract class AbstractBattleUnit : MonoBehaviour
    {
        [SerializeField] public AbstractBattleUnitSO battleUnitSO;
    }
}