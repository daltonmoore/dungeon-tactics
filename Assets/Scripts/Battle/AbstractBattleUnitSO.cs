using UnityEngine;

namespace Units
{
    public class AbstractBattleUnitSO : ScriptableObject
    {
        [SerializeField] public Sprite sprite;
        [SerializeField] public string characterName;
        [SerializeField] public int initiative;
    }
}