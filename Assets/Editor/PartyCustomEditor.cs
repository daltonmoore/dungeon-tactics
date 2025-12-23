using System;
using System.Linq;
using Battle;
using Editor.UnitList;
using Units;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class PartyCustomEditor : EditorWindow
    {
        private Texture2D _addUnitToPartyIcon;
        private const string AddUnitToPartyTexturePath =
            "Assets/Asset Store/Synty/InterfaceCore/Sprites/Icons_Input/Switch/ICON_Input_Switch_Button_Plus_Clean.png";
        
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        
        private UnitWindow _unitWindow;

        private void OnEnable()
        {
            _addUnitToPartyIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AddUnitToPartyTexturePath);
            
            if (_addUnitToPartyIcon == null)
            {
                Debug.LogError($"Failed to load {AddUnitToPartyTexturePath}");
            }
        }

        [MenuItem("Party/PartyCustomEditor")]
        public static void Summon()
        {
            PartyCustomEditor wnd = GetWindow<PartyCustomEditor>();
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
            
            
            // Set the object field to accept GameObjects specifically
            objectField.objectType = typeof(AbstractUnit);
            objectField.value = FindObjectsByType<AbstractUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID)[0]; 
            
           
            VisualElement backColumn = root.Q<VisualElement>("BackColumn");
            foreach (VisualElement child in backColumn.Children())
            {
                CreateUnitButtonHandler(child, objectField);
            }
            
            VisualElement frontColumn = root.Q<VisualElement>("FrontColumn");
            foreach (VisualElement child in frontColumn.Children())
            {
                CreateUnitButtonHandler(child, objectField);
            }
            
            var clearButton = root.Q<Button>("Clear");
            clearButton.RegisterCallback<ClickEvent>(_ =>
            {
                AbstractUnit unit = (AbstractUnit)objectField.value;
                unit.Party.RemoveAll(u => !u.isLeader);
                var leaderBattleUnitPosition = unit.Party[0].battleUnitPosition;
                
                foreach (Button unitButton in backColumn.Children())
                {
                    ClearUnitButtonIcon(unitButton, leaderBattleUnitPosition);
                }
                foreach (Button unitButton in frontColumn.Children())
                {
                    ClearUnitButtonIcon(unitButton, leaderBattleUnitPosition);
                }

            });
            
            DragAndDropManipulator manipulator =
                new(rootVisualElement.Q<VisualElement>("object"));
        }

        private void ClearUnitButtonIcon(Button unitButton, BattleUnitPosition leaderBattleUnitPosition)
        {
            Enum.TryParse(unitButton.name, out BattleUnitPosition buttonPosition);
                    
            if (buttonPosition == leaderBattleUnitPosition)
                return;
            Debug.Log("Clearing" + unitButton.name);
            unitButton.iconImage = _addUnitToPartyIcon;
        }

        private void CreateUnitButtonHandler(VisualElement child, ObjectField objectField)
        {
            Button button = child as Button;
            AbstractUnit unit = (AbstractUnit)objectField.value;
            bool success = Enum.TryParse(button.name, out BattleUnitPosition battleUnitPosition);
            var unitInParty = unit.Party.Exists(u => u.battleUnitPosition == battleUnitPosition && u.icon != null);
            if (unitInParty)
            {
                var item = unit.Party.Where(u => u.battleUnitPosition == battleUnitPosition).ToArray()[0];
                var image = button.iconImage;
                image.sprite = item.icon;
                button.iconImage = image;
            }

            void ClickCallback(ClickEvent _)
            {
                if (_unitWindow != null)
                {
                    _unitWindow.Close();
                }

                _unitWindow = EditorWindow.CreateWindow<UnitTreeView>();
                _unitWindow.BattleUnitPositionLabel = new Label($"{button.name}");

                void OnSelectedUnitForPartyCallback(IUnitOrGroup item)
                {
                    if (objectField.value == null)
                    {
                        return;
                    }

                    Debug.Log("Success? " + success);

                    

                    if (unitInParty) unit.Party.Remove(unit.Party.Where(u => u.battleUnitPosition == battleUnitPosition).ToArray()[0]);

                    unit.Party.Add(new BattleUnitData(null, item.name, 1, item.icon, battleUnitPosition, false));


                    string currentParty = "";
                    foreach (var battleUnitData in unit.Party)
                    {
                        currentParty += $"{battleUnitData.battleUnitPosition} = {battleUnitData.name}, \n ";
                    }

                    Debug.Log(currentParty);

                    var image = button.iconImage;
                    image.sprite = item.icon;
                    button.iconImage = image;
                }

                _unitWindow.OnSelectedUnitForPartyCallback = OnSelectedUnitForPartyCallback;

                _unitWindow.Show();
            }

            button.UnregisterCallback<ClickEvent>(ClickCallback);
            button.RegisterCallback<ClickEvent>(ClickCallback);
        }
    }
}
