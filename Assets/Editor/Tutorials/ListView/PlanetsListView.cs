using Editor.Tutorials.ListView;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanetsListView : PlanetsWindow
{
    [SerializeField]

    [MenuItem("Planets/Standard List")]
    public static void Summon()
    {
        GetWindow<PlanetsListView>("Standard Planet List");
    }

    public void CreateGUI()
    {
        // the protected variable 'uxmlAsset' is a VisualTreeAsset defined in the parent class PlanetsWindow
        uxmlAsset.CloneTree(rootVisualElement);
        var listView = rootVisualElement.Q<ListView>();
        
        // Set ListView.itemsSource to populate the data in the list
        listView.itemsSource = units;
        
        // Set ListView.makeItem to initialize each entry in the list
        listView.makeItem = () => new Label();
        
        // Set ListView.bindItem to bind an initialized entry to a data item.
        listView.bindItem = (VisualElement element, int index) => (element as Label).text = units[index].name;
    }
}
