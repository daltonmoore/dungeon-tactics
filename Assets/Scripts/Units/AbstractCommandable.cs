using System;
using Commands;
using EventBus;
using Events;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable, IPointerEnterHandler, IBattler
    {
        [field: SerializeField] public BaseCommand[] AvailableCommands { get; private set; }
        [field: SerializeField] public Transform Transform { get; private set; }
        [field: SerializeField] public bool IsSelected { get; protected set; }
        [field: SerializeField] public Owner Owner { get; set; }
        [field: SerializeField] public AbstractUnitSO UnitSO { get; private set; }
        [field:SerializeField] public GameObject CurrentTurnHighlight { get; set; }

        [SerializeField] protected GameObject decal;
        [SerializeField] protected bool debug;
        
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


        public bool IsMyTurn { get; set; }
        public bool EndedTurn { get; set; }

        public void HighlightForCurrentTurn()
        {
            if (CurrentTurnHighlight != null)
            {
                CurrentTurnHighlight.SetActive(true);
            }
        }

        public void ResetHighlightForCurrentTurn()
        {
            if (CurrentTurnHighlight != null)
            {
                CurrentTurnHighlight.SetActive(false);
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