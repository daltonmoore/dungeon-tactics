using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.Tutorials.Popup
{
    public class PopupWindowContent : UnityEditor.PopupWindowContent
    {
        public override void OnOpen()
        {
            Debug.Log("Popup opened: " + this);
        }

        public override VisualElement CreateGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Tutorials/Popup/PopupWindowContent.uxml");
            return visualTreeAsset.CloneTree();
        }

        public override void OnClose()
        {
            Debug.Log("Popup closed: " + this);
        }
    }
}
