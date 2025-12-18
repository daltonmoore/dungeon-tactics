using Battle;
using Editor.UnitList;
using Units;
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
        
        private UnitWindow _unitWindow;

        [MenuItem("Window/PartyCustomEditor")]
        public static void ShowExample()
        {
            PartyCustomEditorUIToolkit wnd = GetWindow<PartyCustomEditorUIToolkit>();
            wnd.titleContent = new GUIContent("PartyCustomEditor");
        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement uxml = m_VisualTreeAsset.Instantiate();
            root.Add(uxml);
        
            // Reference the UI elements by the names you set in UI Builder
            var objectField = root.Q<ObjectField>("PartyBeingEdited");

            // Create a new field, disable it, and give it a style class.
            var csharpField = new ObjectField("C# Field");
            csharpField.SetEnabled(false);
            csharpField.AddToClassList("some-styled-field");
            root.Add(csharpField);
            csharpField.value = objectField.value;
            
            // Set the object field to accept GameObjects specifically
            objectField.objectType = typeof(GameObject);

            // Mirror the value of the UXML field into the C# field.
            objectField.RegisterCallback<ChangeEvent<Object>>((evt) => { csharpField.value = evt.newValue; });
           
            
            
            VisualElement backColumn = root.Q<VisualElement>("BackColumn");
            foreach (VisualElement child in backColumn.Children())
            {
                Button button = child as Button;
                button.clicked += () =>
                {
                    if (_unitWindow != null) { _unitWindow.Close(); }
                    _unitWindow = EditorWindow.CreateWindow<UnitsListView>();
                    _unitWindow.Show();
                    
                    if (objectField.value == null) { return; }
                    
                    GameObject gameObject = (GameObject)objectField.value;
                    AbstractUnit unit = gameObject.GetComponent<AbstractUnit>();
                    unit.Party.Add(new BattleUnitData());
                    
                    Debug.Log($"Button {button.name} clicked!");
                };

                
            }
        }
    }
}
