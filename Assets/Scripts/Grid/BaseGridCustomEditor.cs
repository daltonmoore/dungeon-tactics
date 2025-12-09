using UnityEditor;
using UnityEngine;

namespace Grid
{
    [CustomEditor(typeof(BaseGrid))]
    public class BaseGridCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            BaseGrid grid = (BaseGrid) target;
            
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