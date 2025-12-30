using UnityEngine;
using UnityEngine.UIElements;

public class CharacterListEntryController
{
    private Label _label;
    
    // This function retrieves a reference to the
    // character name label inside the ui elements
    public void SetVisualElement(VisualElement visualElement)
    {
        _label = visualElement.Q<Label>("character-name");
    }
    
    // This function receives the character whose name this list
    // element is supposed to display. Since the elements list
    // in a `ListView` are pooled and reused, it's necessary to
    // have a `Set` function to change which character's data to display.
    public void SetCharacterData(CharacterData characterData)
    {
        _label.text = characterData.name;
    }
}
