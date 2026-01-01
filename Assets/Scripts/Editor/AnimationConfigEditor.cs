using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

            string rootFolder = "Assets/Animations/" + config.characterName;

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

            if (GUILayout.Button("Make Animation"))
            {
                foreach (SpriteAnimationParent animationParent in config.spriteAnimationParents)
                {
                    foreach (SpriteAnimation spriteAnim in animationParent.spriteAnimations)
                    {
                        EditorCurveBinding binding = new();
                        binding.type = typeof(SpriteRenderer);
                        binding.path = "";
                        binding.propertyName = "m_Sprite";
                        AnimationClip clip = new();
                        clip.frameRate = 4;

                        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[spriteAnim.sprites.Count];
                        for (int i = 0; i < spriteAnim.sprites.Count; i++)
                        {
                            keyframes[i] = new ObjectReferenceKeyframe();
                            keyframes[i].time = i / clip.frameRate;
                            keyframes[i].value = spriteAnim.sprites[i];
                        }
                        
                        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
                        string path = rootFolder + "/" + animationParent.animationType + "_" + spriteAnim.direction + ".anim";
                        string directory = Path.GetDirectoryName(path);
                        if (!AssetDatabase.IsValidFolder(directory))
                        {
                            if (!AssetDatabase.IsValidFolder("Assets/Animations"))
                                AssetDatabase.CreateFolder("Assets", "Animations");
                            AssetDatabase.CreateFolder("Assets/Animations", config.characterName);
                        }
                        AssetDatabase.CreateAsset(clip, path);
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = clip;
                    }
                }
            }
            
            if (GUILayout.Button("Clear Animations"))
            {
                if (EditorUtility.DisplayDialog("Clear Animations", "Are you sure you want to clear all animations?",
                        "Yes", "No"))
                {
                    if (Directory.Exists(rootFolder)) Directory.Delete(rootFolder, true);
                    AssetDatabase.Refresh();
                    foreach (SpriteAnimationParent spriteAnimationParent in config.spriteAnimationParents)
                    {
                        spriteAnimationParent.spriteAnimations.Clear();
                    }
                }
            }
        }
    }
}