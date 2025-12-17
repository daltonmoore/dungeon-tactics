using Battle;
using Units;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(Party))]
    public class PartyCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Party party = (Party) target;

            DrawDefaultInspector();
            
            if (GUILayout.Button("+"))
            {
                party.party.Add(new BattleUnitData());
            }
        }
    }
}
