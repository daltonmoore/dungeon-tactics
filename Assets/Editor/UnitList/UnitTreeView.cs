using System;
using System.Linq;
using Dalton.Utils;
using FlyingWormConsole3;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.UnitList
{
    public class UnitTreeView : UnitWindow
    {
        [MenuItem("Units/UnitTreeView")]
        public static void Summon()
        {
            UnitTreeView wnd = GetWindow<UnitTreeView>();
            wnd.titleContent = new GUIContent("UnitTreeView");
        }

        public void CreateGUI()
        {
            uxmlAsset.CloneTree(rootVisualElement);

            var treeView = rootVisualElement.Q<MultiColumnTreeView>();
            treeView.autoExpand = true;
            
            // Call MultiColumnTreeView.SetRootItems() to populate the data in the tree
            treeView.SetRootItems(treeRoots);
            
            // For each column, set Column.makeCell to initialize each node in the tree.
            treeView.columns["name"].makeCell = () => new Label();
            treeView.columns["select"].makeCell = () =>
            {
                var btn = new Button();
                btn.text = "Select";
                return btn;
            };
            treeView.columns["icon"].makeCell = () => new Image();
            treeView.columns["stats"].makeCell = () => new Label();


            // For each column, set Column.bindCell to bind an initialized node to a data item.
            treeView.columns["name"].bindCell = (element, index) =>
                (element as Label).text = treeView.GetItemDataForIndex<IUnitOrGroup>(index).name;
            treeView.columns["select"].bindCell = SetupSelectButton(treeView);
            treeView.columns["icon"].bindCell = (element, index) =>
                (element as Image).sprite = treeView.GetItemDataForIndex<IUnitOrGroup>(index).icon;
            treeView.columns["stats"].bindCell = (element, index) =>
            {
                string temp = "";
                var item = treeView.GetItemDataForIndex<IUnitOrGroup>(index);
                if (item == null || item.stats == null) return;
                
                foreach (Stat stat in item.stats)
                {
                    temp += $"{stat.type.ToString()}: {stat.value}\n";
                }

                (element as Label).text = temp;
            };
        }

        private Action<VisualElement, int> SetupSelectButton(MultiColumnTreeView treeView)
        {
            return (element, index) => {
                var selectUnitBtn = element as Button;
                var item = treeView.GetItemDataForIndex<IUnitOrGroup>(index);
                
                selectUnitBtn.visible = item.icon != null;
                selectUnitBtn.RegisterCallback<ClickEvent>(_ =>
                {
                    ConsoleProDebug.Clear();
                    OnSelectedUnitForPartyCallback(item);
                });
            };
        }
    }


}
