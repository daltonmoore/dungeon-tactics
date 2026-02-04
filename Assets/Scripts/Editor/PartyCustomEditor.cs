using System;
using System.IO;
using System.Linq;
using Battle;
using TacticsCore.Editor;
using TacticsCore.Data;
using TacticsCore.Units;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Editor
{
    public class PartyCustomEditor : EditorWindow
    {
        private static LeaderUnit _unit;
        
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
        private ObjectField _objectField;

        private void OnEnable()
        {
            _addUnitToPartyIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(AddUnitToPartyTexturePath);
            _crownIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CrownTexturePath);
            
            Selection.selectionChanged += SelectionChanged;
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

        private void OnDisable()
        {
            Selection.selectionChanged -= SelectionChanged;
        }

        private void SelectionChanged()
        {
            var temp = Selection.activeObject as GameObject;
            var abstractUnit = temp?.GetComponent<AbstractUnit>();
            if (abstractUnit != null)
            {
                _objectField.value = temp;
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
            _objectField = root.Q<ObjectField>("PartyBeingEdited");

            // Set the object field to accept GameObjects specifically
            _objectField.objectType = typeof(AbstractUnit);
            
            SetObjectFieldValueFromSelection();

            _unit = (LeaderUnit)_objectField.value;
            _folderPath = $"Assets/Parties/{_unit.name}/";
            
            var partyUnitButtons = root.Query<Image>(className: "party-unit-button").ToList();
            
            _objectField.RegisterValueChangedCallback(evt =>
            {
                _unit = evt.newValue as LeaderUnit;
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
                
                foreach (UnitSO unitData in _unit.PartyList)
                {
                    if (unitData == leader) continue;
                    
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(unitData));
                }

                
                
                _unit.PartyList.RemoveAll(u => !u.isLeader);
                
                foreach (Image unitButton in partyUnitButtons)
                {
                    UpdateImageWithPartyIcon(unitButton);
                }
            
            });
        }

        private void SetObjectFieldValueFromSelection()
        {
            var selectedGameObject = Selection.activeObject as GameObject;
            if (selectedGameObject != null && selectedGameObject.TryGetComponent(out AbstractUnit unit))
            {
                _objectField.value = unit;
            }
            else
            {
                _objectField.value = FindObjectsByType<AbstractUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID)[0];
            }
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
                if (manipulator.BeganDrag)
                    return;
                
                if (_unitWindow != null)
                {
                    _unitWindow.Close();
                }

                _unitWindow = CreateWindow<UnitTreeView>();

                void OnSelectedUnitForPartyCallback(IUnitOrGroup item)
                {
                    if (_unit == null)
                    {
                        return;
                    }

                    Debug.Log($"Selected unit {item.Name} for position: {image.name}");
                    UpdatePartyWithNewUnit(Enum.Parse<BattleUnitPosition>(image.name), item);

                    image.sprite = item.Icon;
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
            so.name = battleUnitPosition.ToString();
            so.Initialize(item.Name, 1, item.Icon, battleUnitPosition, isLeader, item.Stats.Find(s => s.type == StatType.Initiative).value);
            _unit.PartyList.Add(so);
            CreateOrUpdateAsset(so, $"{_folderPath}{battleUnitPosition}.asset");

            if (isLeader)
            {
                _unit.GetComponent<SpriteRenderer>().sprite = item.Icon;
            }
        }
        
        private static void CreateOrUpdateAsset(Object asset, string path)
        {
            // Check if an asset already exists at the specified path
            Object existingAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));

            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
            
            if (existingAsset == null)
            {
                // If it doesn't exist, create a new asset
                AssetDatabase.CreateAsset(asset, path);
                Debug.Log("Created new asset at: " + path);
            }
            else
            {
                // If it exists, update the existing asset's contents
                // This is useful if you are regenerating data but want to keep existing references intact in scenes/other assets.
                EditorUtility.CopySerialized(asset, existingAsset);
                Debug.Log("Updated existing asset at: " + path);
            }

            // Save assets to ensure changes are written to disk
            AssetDatabase.SaveAssets();
            // Refresh the AssetDatabase if files were created via System.IO or other external methods.
            // AssetDatabase.Refresh(); // Only needed if using System.IO to create files/folders
        }
        
        public static void SwapUnit(BattleUnitPosition fromPosition, BattleUnitPosition toPosition) 
        {
            if (fromPosition == toPosition) return; 
            var fromUnit = _unit.PartyList.Find(u => u.battleUnitPosition == fromPosition); 
            var toUnit = _unit.PartyList.Find(u => u.battleUnitPosition == toPosition);
            if (fromUnit != null) 
            { 
                fromUnit.battleUnitPosition = toPosition;
                CreateOrUpdateAsset(fromUnit, $"{_folderPath}{fromPosition}.asset");
                AssetDatabase.RenameAsset($"{_folderPath}{fromPosition}.asset", $"_temp_{toPosition}.asset"); 
            }

            if (toUnit != null)
            {
               toUnit.battleUnitPosition = fromPosition;
               CreateOrUpdateAsset(toUnit, $"{_folderPath}{toPosition}.asset");
               AssetDatabase.RenameAsset($"{_folderPath}{toPosition}.asset", $"{fromPosition}.asset");
            }
            AssetDatabase.RenameAsset($"{_folderPath}_temp_{toPosition}.asset", $"{toPosition}.asset");
            AssetDatabase.SaveAssets();
        }
    }
}
