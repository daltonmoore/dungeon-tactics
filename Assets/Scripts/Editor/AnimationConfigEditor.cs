using System.ComponentModel;
using System.Linq;
using Units;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor
{
    [CustomEditor(typeof(AnimationConfigSO))]
    public class AnimationConfigEditor : UnityEditor.Editor
    {
        private string _spriteFolder = "Assets/Asset Store/tactical_rpg_overworld/3x/Character sprites/";
        private const int FramesPerAnimation = 4;
        
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Sprite folder: " + _spriteFolder, EditorStyles.miniLabel);
            GUILayout.Label("Frames per animation: " + FramesPerAnimation, EditorStyles.largeLabel);
            
            DrawDefaultInspector();
            
            AnimationConfigSO config = (AnimationConfigSO) target;

            if (GUILayout.Button("Load Animations"))
            {
                foreach (SpriteAnimationParent animationParent in config.spriteAnimationParents)
                {
                    animationParent.spriteAnimations.Clear();
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(_spriteFolder + config.characterName + "_" + config.variant + "_" +
                                                                    animationParent.animationType + ".png").ToArray();
                
                    for (int i = 0; i < sprites.Length / FramesPerAnimation; i++)
                    {
                        animationParent.spriteAnimations.Add(new SpriteAnimation());
                    }
                
                    int currentAnim = 0, frame = 0;
                    for (int i = 0; i < sprites.Length; i++)
                    {
                        var sprite = sprites[i] as Sprite;
                        if (sprite is null) continue;
                    
                        if (frame > FramesPerAnimation - 1)
                        {
                            frame = 0;
                            currentAnim++;
                        }
                        Debug.Log(sprite.name);
                    
                        animationParent.spriteAnimations[currentAnim].sprites.Add(sprite);
                        animationParent.spriteAnimations[currentAnim].direction = (SpriteAnimation.Direction)currentAnim;
                        frame++;
                    }
                }
            }

            if (GUILayout.Button("Change Sprite Folder"))
            {
                _spriteFolder = EditorUtility.OpenFolderPanel("Select Sprite Folder", _spriteFolder, "SpriteFolder");
                _spriteFolder = _spriteFolder.Substring(_spriteFolder.IndexOf("Assets", System.StringComparison.Ordinal));
                Debug.Log(_spriteFolder);
            }
        }
    }
}