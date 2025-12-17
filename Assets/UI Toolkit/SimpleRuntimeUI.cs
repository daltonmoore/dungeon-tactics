using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI_Toolkit
{
    public class SimpleRuntimeUI : MonoBehaviour
    {
        private Button _button;
        private Toggle _toggle;

        private int _clickCount;

        private void OnEnable()
        {
            // the uxml is already instantiated by the UIDocument component
            var uiDocument = GetComponent<UIDocument>();
            
            _button = uiDocument.rootVisualElement.Q("button") as Button;
            _toggle =uiDocument.rootVisualElement.Q("toggle") as Toggle;
            
            _button.RegisterCallback<ClickEvent>(PrintClickMessage);

            var _inputFields = uiDocument.rootVisualElement.Q("input-message");
            _inputFields.RegisterCallback<ChangeEvent<string>>(InputMessage);
        }

        private void InputMessage(ChangeEvent<string> evt)
        {
            Debug.Log($"{evt.newValue} -> {evt.target}");
        }

        private void PrintClickMessage(ClickEvent evt)
        {
            ++_clickCount;
            Debug.Log("button was clicked!" + (_toggle.value ? " Count: " + _clickCount : ""));
        }
    }
}