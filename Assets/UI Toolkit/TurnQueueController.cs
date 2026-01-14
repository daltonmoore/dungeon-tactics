using System.Collections.Generic;
using Battle;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class TurnQueueController
    {
        // UXML template for list entries
        private VisualTreeAsset _listEntryTemplate;
    
        // UI element references
        private ListView _battleUnitList;
        private Label _charClassLabel;
        private Label _charNameLabel;
        private VisualElement _charPortrait;
    
        private List<BattleUnitData> _battleUnits;

        public void InitializeBattleUnitList(VisualElement root, VisualTreeAsset listElementTemplate, List<BattleUnitData> battleUnits)
        {
            // DEBUG_EnumerateBattleUnits();
            _battleUnits = battleUnits;
        
            // Store a reference to the template for the list entries
            _listEntryTemplate = listElementTemplate;
        
            // Store a reference to the character list element
            _battleUnitList = root.Q<ListView>("character-list");
        
            // store references to the selected 
            _charClassLabel = root.Q<Label>("character-class");
            _charNameLabel = root.Q<Label>("character-name");
            _charPortrait = root.Q<VisualElement>("character-portrait");
        
            FillBattleUnitList();
        
            // Register to get a callback when an item is selected
            _battleUnitList.selectionChanged += OnBattleUnitSelected;
        }

        public void ShiftTopEntryToBottomOfList()
        {
            // Get the item, remove it, and insert it at the new (previous) index
            var itemToMove = _battleUnits[0];
            _battleUnits.RemoveAt(0);
            _battleUnits.Add(itemToMove);

            // Refresh the ListView to reflect the data source change
            _battleUnitList.RefreshItems();
        }

        private void DEBUG_EnumerateBattleUnits()
        {
            _battleUnits = new List<BattleUnitData>();
            _battleUnits.AddRange(Resources.LoadAll<BattleUnitData>("TestingBattle"));
        }

        private void FillBattleUnitList()
        {
            // Set up a make item function for a list entry
            _battleUnitList.makeItem = () =>
            {
                // Instantiate the UXML tempalte for the entry
                var newListEntry = _listEntryTemplate.Instantiate();
            
                // Instantiate a controller for the data
                var newListEntryLogic = new TurnQueueEntryController();
            
                // Assign the controller script to the visual element
                newListEntry.userData = newListEntryLogic;
            
                // Initialize the controller script
                newListEntryLogic.SetVisualElement(newListEntry);

                newListEntry.RegisterCallback<MouseEnterEvent>(evt => newListEntryLogic.OnMouseEnter());
                newListEntry.RegisterCallback<MouseLeaveEvent>(evt => newListEntryLogic.OnMouseLeave());
            
                // return the root of the instantiated visual tree
                return newListEntry;
            };
        
            // Set up bind function for a specific list entry
            _battleUnitList.bindItem = (item, index) =>
            {
                (item.userData as TurnQueueEntryController)?.SetCharacterData(_battleUnits[index]);
            };
        
            // Set a fixed item height matching the height of the item provided in makeItem.
            // For dynamic height, see the virtualizationMethod property
            _battleUnitList.fixedItemHeight = 45;
        
            // Set the actual item's source list/array
            _battleUnitList.itemsSource = _battleUnits;
        }

        private void OnBattleUnitSelected(IEnumerable<object> selectedItems)
        {
            // Get the currently selected item directly from the ListView
            var selectedCharacter = _battleUnitList.selectedItem as BattleUnitData;
        
            // Handle none-selection (Escape to deselect everything)
            if (selectedCharacter == null)
            {
                // Clear
                _charClassLabel.text = "";
                _charNameLabel.text = "";
                _charPortrait.style.backgroundImage = null;
            
                return;
            }
        
            // Fill character details
            _charClassLabel.text = "CLASS?"; //selectedCharacter.name.ToString();
            _charNameLabel.text = selectedCharacter.characterName;
            _charPortrait.style.backgroundImage = new StyleBackground(selectedCharacter.icon);
            var inBattleUnitTransform = selectedCharacter.inBattleInstance.transform;
            var initialScale = inBattleUnitTransform.localScale;
            inBattleUnitTransform.DOScale(Vector3.one * 2, .25f).onComplete += () => inBattleUnitTransform.DOScale(initialScale, .25f);
            
        }
    }
}
