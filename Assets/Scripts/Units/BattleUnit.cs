using System;
using Battle;
using Events;
using TacticsCore.Data;
using TacticsCore.EventBus;
using TacticsCore.Interfaces;
using TacticsCore.Units;
using UnityEngine;
using UnityEngine.UI;

namespace Units
{
    public class BattleUnit : AbstractUnit, IDamageable
    {
        [SerializeField] public GameObject currentTurnHighlight;

        public bool IsMyTurn { get; set; }
        public bool EndedTurn { get; set; }
        
        private float _health;
        private float _healthMax;

        protected override void Awake()
        {
            base.Awake();
            _healthMax = UnitSO.Health;
            _health = UnitSO.Health;
        }

        public void HighlightForCurrentTurn()
        {
            if (currentTurnHighlight != null)
            {
                currentTurnHighlight.SetActive(true);
            }
        }

        public void ResetHighlightForCurrentTurn()
        {
            if (currentTurnHighlight != null)
            {
                currentTurnHighlight.SetActive(false);
            }
        }

        public void HighlightForHover()
        {
            if (decal != null)
            {
                decal.gameObject.SetActive(true);
            }
        }

        public void ResetHighlightForHover()
        {
            if (decal != null)
            {
                decal.gameObject.SetActive(false);
            }
        }

        private Stat GetStat(StatType type)
        {
            BattleUnitData battleUnitData = UnitSO as BattleUnitData;
            return battleUnitData.stats.Find(s => s.type == type);
        }


        public float RollDamage()
        {
            return RollStat(GetStat(StatType.Damage));
        }

        private float RollStat(Stat stat)
        {
            return UnityEngine.Random.Range(stat.range.x, stat.range.y);
        }

        [field:SerializeField]
        public Slider HealthBar { get; set; }

        public void TakeDamage(float damage)
        {
            _health -= damage;
            HealthBar.value = _health/_healthMax;

            if (_health <= 0)
            {
                Instantiate(deathPrefab, transform);
                Bus<UnitDied>.Raise(Owner, new UnitDied(this));
            }
        }
    }
}