using System;
using System.Linq;
using Battle;
using Editor.UnitList;
using Units;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class PartyCustomEditor : EditorWindow
    {
        private static AbstractUnit _unit;
        
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
            objectField.value =
                FindObjectsByType<AbstractUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID)[0];
            
            _unit = (AbstractUnit)objectField.value;
            
            var partyUnitButtons = root.Query<Image>(className: "party-unit-button").ToList();
            
            
            objectField.RegisterCallback<ChangeEvent<AbstractUnit>>(evt =>
            {
                _unit = evt.newValue;
                foreach (var image in partyUnitButtons)
                {
                    UpdateImageWithPartyIcon(image);
                }
            });
            
            foreach (var image in partyUnitButtons)
            { 
                DragAndDropManipulator manipulator = new(image, root);
                UpdateImageWithPartyIcon(image);
                manipulator.SetPositionOfTargetToSpecificSlot(Enum.Parse<BattleUnitPosition>(image.name));
                CreateUnitButtonHandler(image, manipulator);
            }

            var clearButton = root.Q<Button>("Clear");
            clearButton.RegisterCallback<ClickEvent>(_ =>
            {
                
                var leader = _unit.PartyList.Where(u => u.isLeader).ToList().First();
                _unit.PartyList.Clear();
                _unit.PartyList.Add(leader);
                
                _unit.PartyList.RemoveAll(u => !u.isLeader);
                
                foreach (Image unitButton in partyUnitButtons)
                {
                    UpdateImageWithPartyIcon(unitButton);
                }
            
            });
        }

        private void UpdateImageWithPartyIcon(Image child)
        { 
            bool success = Enum.TryParse(child.name, out BattleUnitPosition battleUnitPosition);
            BattleUnitData battleUnitData = _unit.PartyList.Find(u => u.battleUnitPosition == battleUnitPosition);
            var unitInParty = battleUnitData != null;
            if (unitInParty)
            {
                child.sprite = battleUnitData.icon;
            }
            else
            {
                child.image = _addUnitToPartyIcon;
            }
        }

        private void CreateUnitButtonHandler(VisualElement child, DragAndDropManipulator manipulator)
        {
            Image image = child as Image;
            bool _ = Enum.TryParse(image.name, out BattleUnitPosition battleUnitPosition);
            var battleUnitData = _unit.PartyList.Find(u => u.battleUnitPosition == battleUnitPosition);
            if (battleUnitData != null)
            {
                image.sprite = battleUnitData.icon;
            }

            void ClickCallback(ClickEvent _)
            {
                if (manipulator.beganDrag)
                    return;
                
                if (_unitWindow != null)
                {
                    _unitWindow.Close();
                }

                _unitWindow = EditorWindow.CreateWindow<UnitTreeView>();

                void OnSelectedUnitForPartyCallback(IUnitOrGroup item)
                {
                    if (_unit == null)
                    {
                        return;
                    }

                    Debug.Log($"Selected unit {item.name} for position: {image.name}");
                    UpdatePartyWithNewUnit(Enum.Parse<BattleUnitPosition>(image.name), item);

                    image.sprite = item.icon;
                }

                _unitWindow.OnSelectedUnitForPartyCallback = OnSelectedUnitForPartyCallback;

                _unitWindow.Show();
            }

            image.UnregisterCallback<ClickEvent>(ClickCallback);
            image.RegisterCallback<ClickEvent>(ClickCallback);
        }

        private static void UpdatePartyWithNewUnit(BattleUnitPosition battleUnitPosition, IUnitOrGroup item)
        {
            var battleUnitData = _unit.PartyList.Find(u => u.battleUnitPosition == battleUnitPosition);
            bool isLeader = false;
            if (battleUnitData != null)
            {
                isLeader = battleUnitData.isLeader;
                _unit.PartyList.Remove(battleUnitData);
            }
            
            _unit.PartyList.Add(new BattleUnitData(null, item.name, 1, item.icon, battleUnitPosition, isLeader));
        }

        public static void SwapUnit(BattleUnitPosition fromPosition, BattleUnitPosition toPosition)
        {
           _unit.SwapUnits(fromPosition, toPosition);
        }
    }
}
