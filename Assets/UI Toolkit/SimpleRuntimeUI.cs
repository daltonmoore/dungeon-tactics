using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class SimpleRuntimeUI : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset listEntryTemplate; 
        [SerializeField] private UIDocument turnOrderDoc;

        private void OnEnable()
        {
            var characterListController = new CharacterListController();
            characterListController.InitializeCharacterList(turnOrderDoc.rootVisualElement, listEntryTemplate);
        }
    }
}