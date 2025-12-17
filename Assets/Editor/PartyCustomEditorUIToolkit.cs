using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public partial class PartyCustomEditorUIToolkit : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Window/UI Toolkit/PartyCustomEditor")]
        public static void ShowExample()
        {
            PartyCustomEditorUIToolkit wnd = GetWindow<PartyCustomEditorUIToolkit>();
            wnd.titleContent = new GUIContent("PartyCustomEditor");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            VisualElement label = new Label("Hello World! From C#");
            root.Add(label);

            // Instantiate UXML
            VisualElement uxml = m_VisualTreeAsset.Instantiate();
            root.Add(uxml);
        
            // Add functionality to UI elements, e.g., a button
            Button myButton = rootVisualElement.Q<Button>("Button"); // Use Query (Q) to find elements by name
            if (myButton != null)
            {
                myButton.clicked += () => Debug.Log("Button clicked!");
            }
        
            // Reference the UI elements by the names you set in UI Builder
            var objectField = root.Q<ObjectField>();

            var csharpField = new ObjectField("C# Field");
            csharpField.SetEnabled(false);
            csharpField.AddToClassList("some-styled-field");
            root.Add(csharpField);
        
            // Set the object field to accept GameObjects specifically
            if (objectField != null)
            {
                // Create a new field, disable it, and give it a style class.
            
                csharpField.value = objectField.value;
            
                // objectField.objectType = typeof(GameObject);

                // Mirror the value of the UXML field into the C# field.
                objectField.RegisterCallback<ChangeEvent<Object>>((evt) => { csharpField.value = evt.newValue; });
            }
            else
            {
                Debug.Log("Object field not found.");
            }
            
            
            VisualElement backColumn = root.Q<VisualElement>("BackColumn");
            foreach (VisualElement child in backColumn.Children())
            {
                Button button = child as Button;
                button.clicked += () =>
                {
                    Debug.Log($"Button {button.name} clicked!");
                    Button btn = root.Q<Button>(button.name);
                    GameObject party = (GameObject)objectField.value;
                    party.SetActive(!party.activeSelf);
                    Debug.Log($"GameObject activity toggled: {party.activeSelf}");
                    btn.text = party.activeSelf ? "Object is Active" : "Object is Inactive";
                };
            }
        }
    }
}
