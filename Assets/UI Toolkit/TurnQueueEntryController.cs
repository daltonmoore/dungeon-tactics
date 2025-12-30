using Battle;
using Units;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnQueueEntryController
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
    public void SetCharacterData(BattleUnitData characterData)
    {
        _label.text = characterData.characterName;
    }
}
