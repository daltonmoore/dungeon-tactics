using System;
using System.Collections.Generic;
using Data;
using Units;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class UnitWindow : EditorWindow
    {
        [SerializeField] protected VisualTreeAsset uxmlAsset;
        [SerializeField] protected UnitDatabase unitDatabase; 
        
        public Action<IUnitOrGroup> OnSelectedUnitForPartyCallback;
        public Label BattleUnitPositionLabel;

        // Expresses unit data as a list of the units themselves. Needed for ListView and MultiColumnView.
        protected List<Unit> Units
        {
            get
            {
                var retVal = new List<Unit>();
                foreach (UnitGroup group in unitDatabase.unitGroups)
                {
                    retVal.AddRange(group.units);
                }
                
                return retVal;
            }
        }
        
        // Expresses planet data as a list of TreeViewItemData objects. Needed for TreeView and MultiColumnTreeView.
        protected IList<TreeViewItemData<IUnitOrGroup>> treeRoots
        {
            get
            {
                int id = 0;
                var roots = new List<TreeViewItemData<IUnitOrGroup>>(unitDatabase.unitGroups.Count);
                foreach (var group in unitDatabase.unitGroups)
                {
                    var planetsInGroup = new List<TreeViewItemData<IUnitOrGroup>>(group.units.Count);
                    foreach (var unit in group.units)
                    {
                        planetsInGroup.Add(new TreeViewItemData<IUnitOrGroup>(id++, unit));
                    }

                    roots.Add(new TreeViewItemData<IUnitOrGroup>(id++, group, planetsInGroup));
                }
                return roots;
            }
        }
    }
    
    // Nested class that represents a group of units
    [System.Serializable]
    public class UnitGroup : IUnitOrGroup
    {
        [field: SerializeField]
        public string name { get; set; }
        

        public bool populated
        {
            get
            {
                var anyUnitPopulated = false;
                foreach (Unit unit in units)
                {
                    anyUnitPopulated = anyUnitPopulated || unit.populated;
                }

                return anyUnitPopulated;
            }
        }

        public Sprite icon { get; }

        [SerializeField]
        public List<Unit> units;

        public UnitGroup(string name, List<Unit> units)
        {
            this.name = name;
            this.units = units;
        }
    }
    
    // Nested interface that can be either a single unit or a group of units.
    public interface IUnitOrGroup
    {
        public string name { get; }

        public bool populated { get; }
        
        public Sprite icon { get; }
        
    }

    [System.Serializable]
    public class Unit : IUnitOrGroup
    {
        [field: SerializeField]
        public string name { get; set; }
        
        [field: SerializeField]
        public Sprite icon { get; set; }
        
        [field: SerializeField]

        public bool populated { get; }

        public Unit(string name, bool populated, Sprite icon, BattleUnitPosition battleUnitPosition)
        {
            this.name = name;
            this.populated = populated;
            this.icon = icon;
        }
    }
}
