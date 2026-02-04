using System.Collections.Generic;
using TacticsCore.Data;
using TacticsCore.EventBus;
using TacticsCore.Events;
using TacticsCore.Units;
using UI.Containers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI
{
    public class RuntimeUI : MonoBehaviour
    {
        [SerializeField] public ActionsUI actionsUI;
        
        private static RuntimeUI _instance;
        private AbstractCommandable _selectedUnit;
        
        private void Awake()
        {
            _instance = this;
            Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] += HandleUnitSelected;

        }

        private void Start()
        {
            actionsUI.Disable();
        }

        private void OnDestroy()
        {
            Bus<UnitSelectedEvent>.OnEvent[Owner.Player1] -= HandleUnitSelected;
        }

        private void HandleUnitSelected(UnitSelectedEvent args)
        {
            _selectedUnit = args.Unit as AbstractCommandable;
            RefreshUI();
        }
        
        private void RefreshUI()
        {
            if (_selectedUnit != null)
            {
                actionsUI.EnableFor(_selectedUnit);

                // if (_commandables.Count == 1)
                // {
                //     ResolveSingleUnitSelectedUI();
                // }
                // else
                // {
                //     unitIconUI.Disable();
                //     singleUnitSelectedUI.Disable();
                //     buildingSelectedUI.Disable();
                //     unitTransportUI.Disable();
                // }
            }

            // if (_commandables.Count == 0)
            // {
            //     DisableAllContainers();
            // }
        }
        
        public static bool IsPointerOverCanvas()
        {
            PointerEventData eventData = new(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject.transform.IsChildOf(_instance.transform))
                {
                    return true; // Pointer is over an element within the target canvas
                }
            }
            return false; // Pointer is not over any element within the target canvas
        }
    }
}
