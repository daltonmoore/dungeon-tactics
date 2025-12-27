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
            
            foreach (var image in partyUnitButtons)
            { 
                DragAndDropManipulator manipulator = new(image, root);
                UpdateImageWithPartyIcon(image);
                manipulator.SetPositionOfTargetToSpecificSlot(Enum.Parse<BattleUnitPosition>(image.name));
                CreateUnitButtonHandler(image, objectField, manipulator);
            }

            var clearButton = root.Q<Button>("Clear");
            clearButton.RegisterCallback<ClickEvent>(_ =>
            {
                // _unit.Party.RemoveAll(u => !u.isLeader);
                var leaderBattleUnitPosition = _unit.Party[0].battleUnitPosition;
                
                foreach (Image unitButton in partyUnitButtons)
                {
                    ClearUnitButtonIcon(unitButton, leaderBattleUnitPosition);
                }
            
            });
        }

        private static void UpdateImageWithPartyIcon(VisualElement child)
        { 
            Image image = child as Image;
            bool success = Enum.TryParse(image.name, out BattleUnitPosition battleUnitPosition);
            var unitInParty = _unit.Party.ContainsKey(battleUnitPosition) && _unit.Party[battleUnitPosition].icon != null;
            if (unitInParty)
            {
                image.sprite = _unit.Party[battleUnitPosition].icon;
            }
            else
            {
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Asset Store/Synty/InterfaceCore/Sprites/Icons_Input/Switch/ICON_Input_Switch_Button_Plus_Clean.png");
            }
        }

        private void ClearUnitButtonIcon(Image unitButton, BattleUnitPosition leaderBattleUnitPosition)
        {
            Enum.TryParse(unitButton.name, out BattleUnitPosition buttonPosition);
                    
            if (buttonPosition == leaderBattleUnitPosition)
                return;
            Debug.Log("Clearing" + unitButton.name);
            unitButton.image = _addUnitToPartyIcon;
        }

        private void CreateUnitButtonHandler(VisualElement child, ObjectField objectField,
            DragAndDropManipulator manipulator)
        {
            Image image = child as Image;
            bool _ = Enum.TryParse(image.name, out BattleUnitPosition battleUnitPosition);
            if (_unit.Party.ContainsKey(battleUnitPosition) && _unit.Party[battleUnitPosition].icon != null)
            {
                image.sprite = _unit.Party[battleUnitPosition].icon;
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
                _unitWindow.BattleUnitPositionLabel = new Label($"{image.name}");

                void OnSelectedUnitForPartyCallback(IUnitOrGroup item)
                {
                    if (_unit == null)
                    {
                        return;
                    }

                    UpdatePartyWithNewUnit(battleUnitPosition, item);

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
            _unit.Party[battleUnitPosition] = new BattleUnitData(null, item.name, 1, item.icon, battleUnitPosition, false);
        }

        public static void SwapUnit(BattleUnitPosition fromPosition, BattleUnitPosition toPosition)
        {
           _unit.SwapUnits(fromPosition, toPosition);
        }
    }
}
