using UnityEditor;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace Editor.Tutorials.Popup
{
    public class PopupExample : EditorWindow
    {
        [MenuItem("Examples/Popup Example")]
        public static void Init()
        {
            EditorWindow window = EditorWindow.CreateInstance<PopupExample>();
            window.Show();
        
        }

        public void CreateGUI()
        {
            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Tutorials/Popup/PopupExample.uxml");
            visualTreeAsset.CloneTree(rootVisualElement);
        
            var button = rootVisualElement.Q<Button>();
            button.clicked += () => PopupWindow.Show(button.worldBound, new PopupWindowContent());
        }
    }
}
