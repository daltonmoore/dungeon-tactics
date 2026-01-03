using System.Reflection;
using TMPro;
using UnityEngine;
using Util;

namespace Dalton.Utils
{
    public static class Utils
    {
        private const int SortingOrderDefault = 5000;
        
        public static TextMeshPro CreateWorldText(
            string text,
            Transform parent = null,
            Vector3 localPosition = default(Vector3),
            int fontSize = 40,
            Color? color = null,
            TextAlignmentOptions textAlignmentOptions = TextAlignmentOptions.TopLeft,
            int sortingOrder = SortingOrderDefault,
            string sortingLayerName = Constants.DefaultSortingLayer)
        {
            if (color == null) color = Color.white;
            return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAlignmentOptions, sortingOrder, sortingLayerName);
        }

        private static TextMeshPro CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize,
            Color color, TextAlignmentOptions textAlignmentOptions, int sortingOrder, string sortingLayerName)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
            textMesh.alignment = textAlignmentOptions;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            MeshRenderer meshRenderer = textMesh.GetComponent<MeshRenderer>();
            meshRenderer.sortingOrder = sortingOrder;
            meshRenderer.sortingLayerName = sortingLayerName;
            return textMesh;
        }

        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMousePositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0;
            return vec;
        }

        private static Vector3 GetMousePositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }
        
        // Create a Text Popup in the World, no parent
        public static void CreateWorldTextPopup(string text, Vector3 localPosition) {
            CreateWorldTextPopup(null, text, localPosition, 40, Color.white, localPosition + new Vector3(0, 20), 1f);
        }
        
        // Create a Text Popup in the World
        private static void CreateWorldTextPopup(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, Vector3 finalPopupPosition, float popupTime)
        {
            TextMeshPro textMesh = CreateWorldText(parent, text, localPosition, fontSize, color,
                TextAlignmentOptions.BottomLeft, SortingOrderDefault, Constants.DefaultSortingLayer);
            Transform transform = textMesh.transform;
            Vector3 moveAmount = (finalPopupPosition - localPosition) / popupTime;
            FunctionUpdater.Create(delegate () {
                transform.position += moveAmount * Time.deltaTime;
                popupTime -= Time.deltaTime;
                if (popupTime <= 0f) {
                    UnityEngine.Object.Destroy(transform.gameObject);
                    return true;
                } else {
                    return false;
                }
            }, "WorldTextPopup");
        }
        
        public static void ClearEditorLog()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
    }
}