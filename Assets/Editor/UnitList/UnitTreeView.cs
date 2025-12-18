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

            var treeView = rootVisualElement.Q<TreeView>();
            
            // Call TreeView.SetRootItems() to populate the data in the tree
            treeView.SetRootItems(treeRoots);
            
            // Set TreeView.makeItem to initialize each node in the tree.
            treeView.makeItem = () => new Label();
            
            // Set TreeView.bindItem to bind an initialized node to a data item.
            treeView.bindItem = (element, index) =>
                (element as Label).text = treeView.GetItemDataForIndex<IUnitOrGroup>(index).name;
        }
    }
}
