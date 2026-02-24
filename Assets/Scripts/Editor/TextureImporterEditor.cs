using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    public class TextureImporterEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        
        private Label workingPathLabel;
        private string workingPath;

        [MenuItem("Util/Texture Importer")]
        private static void Summon()
        {
            var window = GetWindow<TextureImporterEditor>();
            window.titleContent = new GUIContent("Texture Importer");
            window.Show();
        }

        private void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Instantiate UXML
            VisualElement uxml = visualTreeAsset.Instantiate();
            root.Add(uxml);

            var importButton = root.Q<Button>("Import");
            importButton.clicked += OnImportButtonClicked;

            // Load UXML and USS
            var selectFolderButton = rootVisualElement.Q<Button>("SelectFolder");
            selectFolderButton.clicked += OnSelectFolderButtonClicked;

            workingPathLabel = new Label("Working Path");
            
            root.Add(workingPathLabel);
        }

        private void OnSelectFolderButtonClicked()
        {
            workingPath = EditorUtility.OpenFolderPanel("Select File", "", "");

            if (!string.IsNullOrEmpty(workingPath))
            {
                Debug.Log("Selected folder path: " + workingPath);
                Debug.Log(workingPath.Substring(workingPath.IndexOf("Assets", StringComparison.Ordinal)));
                workingPath = workingPath.Substring(workingPath.IndexOf("Assets", StringComparison.Ordinal));
                
                workingPathLabel.text =  workingPath;
            }
        }

        private void OnImportButtonClicked()
        {
            foreach (var textureGUID in AssetDatabase.FindAssets("t:Texture2D", new[] { workingPath }))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(textureGUID);
                Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (textureImporter == null)
                {
                    Debug.LogError("Failed to get TextureImporter for texture");
                }
                else
                {
                    // FIRST STEP:
                    // Setup the texture asset to have the correct import settings
                    // Eg, Ensure that the texture is set to 'multiple' mode
                    textureImporter.textureType = TextureImporterType.Sprite;
                    textureImporter.spriteImportMode = SpriteImportMode.Multiple;
                    textureImporter.spritePixelsPerUnit = 100;
                    textureImporter.mipmapEnabled = false; // Mipmaps are unnecessary for sprites
                    textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                    textureImporter.filterMode = FilterMode.Point;
                    textureImporter.maxTextureSize =
                        8192; // Im setting this much larger incase we have very large spritesheets

                    // Reimport the texture with updated settings
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                    // SECOND STEP:
                    // Slice the texture and create SpriteRects that represent each sliced sprite
                    // Note: TextureImporter.spritesheet no longer works, it seems to get overridden
                    int sliceWidth = 48, sliceHeight = 48;
                    var factory = new SpriteDataProviderFactories();
                    factory.Init();
                    ISpriteEditorDataProvider dataProvider = factory.GetSpriteEditorDataProviderFromObject(texture);
                    dataProvider.InitSpriteEditorDataProvider();
                    dataProvider.SetSpriteRects(GenerateSpriteRectData(texture.width, texture.height, sliceWidth,
                        sliceHeight, texture.name));
                    dataProvider.Apply();

                    var assetImporter = dataProvider.targetObject as AssetImporter;
                    assetImporter.SaveAndReimport();
                }
            }
        }

        private static SpriteRect[] GenerateSpriteRectData(int textureWidth, int textureHeight, int sliceWidth,
            int sliceHeight, string textureName = "Slice")
        {
            List<SpriteRect> spriteRects = new List<SpriteRect>();

            for (int y = textureHeight; y > 0; y -= sliceHeight)
            {
                for (int x = 0; x < textureWidth; x += sliceWidth)
                {
                    SpriteRect spriteRect = new SpriteRect();
                    spriteRect.rect = new Rect(x, y - sliceHeight, sliceWidth, sliceHeight);
                    spriteRect.pivot = new Vector2(0.5f, 0f);
                    spriteRect.name = $"{textureName}_{x / sliceWidth}_{y / sliceHeight}";
                    spriteRect.alignment = SpriteAlignment.BottomCenter;
                    spriteRect.border = new Vector4(0, 0, 0, 0);

                    spriteRects.Add(spriteRect);
                }
            }

            return spriteRects.ToArray();
        }
    }
}