using System.Collections.Generic;
using TacticsCore.Data;
using TacticsCore.EventBus;
using TacticsCore.Events;
using TacticsCore.Units;
using UI.Containers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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
            Vector2 position = Mouse.current.position.ReadValue();
            return IsPointerOverCanvas(position);
        }

        public static bool IsPointerOverCanvas(Vector2 screenPosition)
        {
            if (_instance == null) return false;
            
            PointerEventData eventData = new(EventSystem.current)
            {
                position = screenPosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            if (EventSystem.current != null)
            {
                EventSystem.current.RaycastAll(eventData, results);
            }

            foreach (RaycastResult result in results)
            {
                // If it's a standard Canvas UI element, it will have a GameObject
                if (result.gameObject != null)
                {
                    // If it's part of our runtime UI canvas
                    if (result.gameObject.transform.IsChildOf(_instance.transform))
                    {
                        return true;
                    }
                }
            }
            
            // Check all UI Toolkit documents. 
            // We use a fallback because RaycastAll might fail if PanelRaycaster is missing,
            // or for specific UI Toolkit setups.
            var allDocs = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
            foreach (var doc in allDocs)
            {
                // Skip the cursor document
                if (doc.name == "UICursorDocument") continue;
                
                if (IsPointerOverUIToolkit(doc, screenPosition))
                {
                    Debug.Log($"Pointer over UI Toolkit document: {doc.name}");
                    return true;
                }
            }

            return false;
        }

        private static bool IsPointerOverUIToolkit(UIDocument doc, Vector2 screenPosition)
        {
            if (doc == null || doc.rootVisualElement == null) return false;

            // UI Toolkit panel.Pick expects panel-space coordinates (Y-down)
            Vector2 flippedScreenPos = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(doc.rootVisualElement.panel, flippedScreenPos);
            
            VisualElement picked = doc.rootVisualElement.panel.Pick(panelPos);
            
            // If we picked something and it's not the root and it's not set to Ignore picking
            if (picked == null || picked == doc.rootVisualElement || picked.pickingMode == PickingMode.Ignore)
            {
                return false;
            }

            // Explicitly ignore the cursor element if it's in this document
            if (picked.name == "ui-cursor") 
            {
                return false;
            }

            return true;
        }
    }
}
