using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharacterListController
{
    // UXML template for list entries
    VisualTreeAsset _listEntryTemplate;
    
    // UI element references
    private ListView _characterList;
    private Label _charClassLabel;
    private Label _charNameLabel;
    private VisualElement _charPortrait;
    
    private List<CharacterData> _allCharacters;

    public void InitializeCharacterList(VisualElement root, VisualTreeAsset listElementTemplate)
    {
        EnumerateAllCharacters();
        
        // Store a reference to the template for the list entries
        _listEntryTemplate = listElementTemplate;
        
        // Store a reference to the character list element
        _characterList = root.Q<ListView>("character-list");
        
        // store references to the selected 
        _charClassLabel = root.Q<Label>("character-class");
        _charNameLabel = root.Q<Label>("character-name");
        _charPortrait = root.Q<VisualElement>("character-portrait");
        
        FillCharacterList();
        
        // Register to get a callback when an item is selected
        _characterList.selectionChanged += OnCharacterSelected;
    }

    private void EnumerateAllCharacters()
    {
        _allCharacters = new List<CharacterData>();
        _allCharacters.AddRange(Resources.LoadAll<CharacterData>("Characters"));
    }

    private void FillCharacterList()
    {
        // Set up a make item function for a list entry
        _characterList.makeItem = () =>
        {
            // Instantiate the UXML tempalte for the entry
            var newListEntry = _listEntryTemplate.Instantiate();
            
            // Instantiate a controller for the data
            var newListEntryLogic = new CharacterListEntryController();
            
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
            
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);
            
            // return the root of the instantiated visual tree
            return newListEntry;
        };
        
        // Set up bind function for a specific list entry
        _characterList.bindItem = (item, index) =>
        {
            (item.userData as CharacterListEntryController)?.SetCharacterData(_allCharacters[index]);
        };
        
        // Set a fixed item height matching the height of the item provided in makeItem.
        // For dynamic height, see the virtualizationMethod property
        _characterList.fixedItemHeight = 45;
        
        // Set the actual item's source list/array
        _characterList.itemsSource = _allCharacters;
    }

    private void OnCharacterSelected(IEnumerable<object> selectedItems)
    {
        // Get the currently selected item directly from the ListView
        var selectedCharacter = _characterList.selectedItem as CharacterData;
        
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
        _charClassLabel.text = selectedCharacter.Class.ToString();
        _charNameLabel.text = selectedCharacter.CharacterName;
        _charPortrait.style.backgroundImage = new StyleBackground(selectedCharacter.PortraitImage);
    }
}
