using System.Collections.Generic;
using Battle;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class SimpleRuntimeUI : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset listEntryTemplate; 
        [SerializeField] private UIDocument turnOrderDoc;

        public static SimpleRuntimeUI Instance { get; private set; }
        
        private void OnEnable()
        {
            if (Instance != null)
            {
                Debug.LogError("Only one SimpleRuntimeUI can exist at a time for reasons!");
                return;
            }
            Instance = this;
        }
        
        public void InitializeTurnOrder(List<BattleUnitData> turnOrder)
        {
            var turnQueueController = new TurnQueueController();
            turnQueueController.InitializeBattleUnitList(turnOrderDoc.rootVisualElement, listEntryTemplate, turnOrder);
        }
    }
}