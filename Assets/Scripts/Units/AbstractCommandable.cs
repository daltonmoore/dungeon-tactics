using System;
using Commands;
using EventBus;
using Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable, IPointerEnterHandler
    {
        [field: SerializeField] public BaseCommand[] AvailableCommands { get; private set; }
        [field: SerializeField] public Transform Transform { get; private set; }
        [field: SerializeField] public bool IsSelected { get; protected set; }
        [field: SerializeField] public Owner Owner { get; set; }
        [field: SerializeField] public AbstractUnitSO UnitSO { get; private set; }

        [SerializeField] protected GameObject decal;
        [SerializeField] protected bool debug;
        [SerializeField] private GameObject currentTurnHighlight;
        
        protected Animator Animator;

        private BaseCommand[] _initialCommands;

        protected virtual void Awake()
        {
            Transform = GetComponent<Transform>();
            Animator = GetComponent<Animator>();
        }


        public void SetCommandOverrides(BaseCommand[] overrides)
        {
            if (overrides == null || overrides.Length == 0)
            {
                AvailableCommands = _initialCommands;
            }
            else
            {
                AvailableCommands = overrides;
            }

            if (IsSelected)
            {
                Bus<UnitSelectedEvent>.Raise(Owner, new UnitSelectedEvent(this));
            }
        }
        
        public void OnPointerEnter(PointerEventData _)
        {
            
        }

        public void CurrentTurnHighlight()
        {
            if (decal != null && currentTurnHighlight != null)
            {
                decal.gameObject.SetActive(true);
                currentTurnHighlight.SetActive(true);
            }
        }

        public void Select()
        {
            if (decal != null)
            {
                decal.gameObject.SetActive(true);
            }
            
            IsSelected = true;
            Bus<UnitSelectedEvent>.Raise(Owner, new UnitSelectedEvent(this));
        }

        public void Deselect()
        {
            // if (decal != null)
            // {
            //     decal.gameObject.SetActive(false);
            // }
            // IsSelected = false;
        }
    }
}