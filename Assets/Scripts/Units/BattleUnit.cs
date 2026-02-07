using System;
using Battle;
using Events;
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

        protected BattleUnitSO BattleUnitSO;
        
        public bool IsMyTurn { get; set; }
        public bool EndedTurn { get; set; }
        
        public enum EActionType
        {
            Attack
        }
        
        private float _health;
        private float _healthMax;

        protected override void Awake()
        {
            base.Awake();
            _healthMax = unitSO.Health;
            _health = unitSO.Health;
            
            BattleUnitSO = UnitSO as BattleUnitSO;
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

        public void DoAction()
        {
            throw new NotImplementedException();
        }

        [field:SerializeField]
        public Slider HealthBar { get; set; }

        public void TakeDamage(int damage)
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