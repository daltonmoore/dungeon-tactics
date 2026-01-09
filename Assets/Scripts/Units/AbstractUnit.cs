using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Battle;
using Drawing;
using Events;
using HexGrid;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

namespace Units
{
    public abstract class AbstractUnit : AbstractCommandable, IDamageable
    {
        [SerializeField] private AnimationConfigSO animationConfig;

        protected UnitSO unitSO;
       
        private float _health;
        private float _healthMax;
        
        
        protected override void Awake()
        {
            base.Awake();
            
            unitSO = UnitSO as UnitSO;
            _healthMax = unitSO.Health;
            _health = unitSO.Health;
            
        }

        [field:SerializeField]
        public Slider HealthBar { get; set; }

        public void TakeDamage(int damage)
        {
            _health -= damage;
            HealthBar.value = _health/_healthMax;
        }
    }
    
    public enum BattleUnitPosition
    {
        BackBottom = 0,     // mod 3 is back to 0
        BackCenter = 1,     // mod 3 is 1
        BackTop = 2,         // mod 3 is 2
        FrontBottom = 3,    // mod 3 is 0
        FrontCenter = 4,    // mod 3 is 1
        FrontTop = 5,       // mod 3 is 2
    }
}