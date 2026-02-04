using System.Collections.Generic;
using Battle;
using TacticsCore;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class TurnUI : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset listEntryTemplate; 
        [SerializeField] private UIDocument turnOrderDoc;
        private TurnQueueController _turnQueueController;

        public static TurnUI Instance { get; private set; }
        
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
            _turnQueueController = new TurnQueueController();
            _turnQueueController.InitializeBattleUnitList(turnOrderDoc.rootVisualElement, listEntryTemplate, turnOrder);
        }

        public void ShiftTopEntryToBottom()
        {
            _turnQueueController.ShiftTopEntryToBottomOfList();
        }
    }
}