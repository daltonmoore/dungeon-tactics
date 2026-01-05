using Grid;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(BattleGrid))]
    public class BaseGridCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            BattleGrid grid = (BattleGrid) target;
            
            if(GUILayout.Button("Generate Grid")) 
            {
                grid.SetupGrid();
            }
            
            if(GUILayout.Button("Clear Grid")) 
            {
                grid.ClearGrid();
            }
        }
    }
}