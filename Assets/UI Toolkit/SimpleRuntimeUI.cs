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
            var turnQueueController = new TurnQueueController();
            turnQueueController.InitializeBattleUnitList(turnOrderDoc.rootVisualElement, listEntryTemplate);
        }
    }
}