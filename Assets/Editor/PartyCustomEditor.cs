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
        private Texture2D _crownIcon;
        private const string CrownTexturePath = "Assets/Asset Store/Synty/InterfaceSciFiSoldierHUD/Sprites/HUD/SPR_HUD_SciFiSoldier_Triangle_02.png";
        
        private static string _folderPath;
        
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        private Image _crownImage;
        private UnitWindow _unitWindow;

        private void OnEnable()
        {
            _addUnitToPartyIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AddUnitToPartyTexturePath);
            _crownIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CrownTexturePath);
            _crownImage = new()
            {
                style =
                {
                    width = 20,
                    height = 20,
                    position = Position.Relative,
                    backgroundImage = new StyleBackground(_crownIcon),
                    left = 10
                }
            };
            
            if (_addUnitToPartyIcon == null)
            {
                Debug.LogError($"Failed to load {AddUnitToPartyTexturePath}");
            }
            if (_crownIcon == null)
            {
                Debug.LogError($"Failed to load {CrownTexturePath}");
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
            
            var saveButton = root.Q<Button>("Save");
            saveButton.RegisterCallback<ClickEvent>(_ => SaveParty());

            // Set the object field to accept GameObjects specifically
            objectField.objectType = typeof(AbstractUnit);
            objectField.value =
                FindObjectsByType<AbstractUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID)[0];
            
            _unit = (AbstractUnit)objectField.value;
            _folderPath = $"Assets/Parties/{_unit.name}/";
            
            var partyUnitButtons = root.Query<Image>(className: "party-unit-button").ToList();
            
            
            objectField.RegisterValueChangedCallback(evt =>
            {
                _unit = evt.newValue as AbstractUnit;
                _folderPath = $"Assets/Parties/{_unit.name}/";
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
                
                foreach (BattleUnitData battleUnitData in _unit.PartyList)
                {
                    if (battleUnitData == leader) continue;
                    
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(battleUnitData));
                }

                
                
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
                if (battleUnitData.isLeader)
                {
                    child.Add(_crownImage);
                }
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

            BattleUnitData so = CreateInstance<BattleUnitData>();
            so.Initialize(item.name, 1, item.icon, battleUnitPosition, isLeader);
            _unit.PartyList.Add(so);
            
            // Save it to disk
            SaveParty();
        }

        private static void SaveParty()
        {
            System.IO.Directory.CreateDirectory(_folderPath); // Create the physical folder
            AssetDatabase.Refresh(); // Register the new folder in the AssetDatabase

            BattleUnitData lastSO = null;
            foreach (BattleUnitData battleUnitData in _unit.PartyList)
            {
                string path = _folderPath + $"{battleUnitData.battleUnitPosition}.asset";
                if (!System.IO.File.Exists(path))
                    AssetDatabase.CreateAsset(battleUnitData, path);
                AssetDatabase.SaveAssets();
                lastSO = battleUnitData;
            }
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = lastSO;
        }

        public static void SwapUnit(BattleUnitPosition fromPosition, BattleUnitPosition toPosition)
        {
           _unit.SwapUnits(fromPosition, toPosition);
        }
    }
}
