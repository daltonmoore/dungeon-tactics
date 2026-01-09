using Battle;
using DG.Tweening;
using Units;
using UnityEngine;
using UnityEngine.UIElements;

public class TurnQueueEntryController
{
    private Label _label;
    private BattleUnitData _characterData;
    
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
        _characterData = characterData;
        _label.text = characterData.characterName;
    }

    public void OnMouseEnter()
    {
        Debug.Log("Mouse enter");
        var inBattleUnitTransform = _characterData.inBattleInstance.transform;
        _characterData.inBattleInstance.GetComponent<BattleUnit>().HighlightForHover();
        var initialScale = inBattleUnitTransform.localScale;
        inBattleUnitTransform.DOScale(Vector3.one * 2, .25f).onComplete += () => inBattleUnitTransform.DOScale(initialScale, .25f);
    }

    public void OnMouseLeave()
    {
        Debug.Log("Mouse leave"); 
        _characterData.inBattleInstance.GetComponent<BattleUnit>().ResetHighlightForHover();
    }
}
