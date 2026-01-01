using System;
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
        private string _spriteFolderPath = "Assets/Asset Store/tactical_rpg_overworld/3x/Character sprites/";
        private string _animRootFolderPath;
        private const int FramesPerAnimation = 4;

        private Texture2D _iconForSpriteFolder;


        private void OnEnable()
        {
            _iconForSpriteFolder = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/com.unity.2d.sprite/Editor/ObjectMenuCreation/DefaultAssets/Textures/v2/HexagonPointTop.png");
        }

        public override void OnInspectorGUI()
        {
            
            AnimationConfigSO config = (AnimationConfigSO) target;

            _animRootFolderPath = "Assets/Animations/" + config.characterName;
            
            GUIStyle folderPingerButtonStyle = new(GUI.skin.button);
            folderPingerButtonStyle.padding = new RectOffset(30, 10, 5, 5);
            folderPingerButtonStyle.alignment = TextAnchor.MiddleLeft;
            
            GUIContent spriteFolderButtonContent = new($"Sprite Folder: {_spriteFolderPath}", _iconForSpriteFolder, "Folder containing sprites for animations");
            GUIContent animationFolderButtonContent = new($"Animation Folder: {_animRootFolderPath}", _iconForSpriteFolder, "Folder containing generated animations");

            if (GUILayout.Button(spriteFolderButtonContent, folderPingerButtonStyle, GUILayout.Height(48)))
            {
                PingFolder(_spriteFolderPath);
            }
            
            if (GUILayout.Button(animationFolderButtonContent, folderPingerButtonStyle, GUILayout.Height(48)))
            {
                PingFolder(_animRootFolderPath);
            }
            
            GUILayout.Label("Frames per animation: " + FramesPerAnimation, EditorStyles.largeLabel);
            
            DrawDefaultInspector();

            if (GUILayout.Button("Load Animations"))
            {
                foreach (SpriteAnimationParent animationParent in config.spriteAnimationParents)
                {
                    animationParent.spriteAnimations.Clear();
                    var sprites = AssetDatabase.LoadAllAssetsAtPath(_spriteFolderPath + config.characterName + "_" + config.variant + "_" +
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
                _spriteFolderPath = EditorUtility.OpenFolderPanel("Select Sprite Folder", _spriteFolderPath, "SpriteFolder");
                _spriteFolderPath = _spriteFolderPath.Substring(_spriteFolderPath.IndexOf("Assets", System.StringComparison.Ordinal));
                Debug.Log(_spriteFolderPath);
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
                        string path = _animRootFolderPath + "/" + animationParent.animationType + "_" + spriteAnim.direction + ".anim";
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
                    if (Directory.Exists(_animRootFolderPath)) Directory.Delete(_animRootFolderPath, true);
                    AssetDatabase.Refresh();
                    foreach (SpriteAnimationParent spriteAnimationParent in config.spriteAnimationParents)
                    {
                        spriteAnimationParent.spriteAnimations.Clear();
                    }
                }
            }
        }

        private void PingFolder(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath)) return;
            if (!Directory.Exists(folderPath)) return;
            // Load the asset at the path (a folder is treated as a DefaultAsset)
            if (folderPath.EndsWith('/'))
            {
                // Remove trailing slash
                folderPath = folderPath.Substring(0,
                    folderPath.LastIndexOf("/", StringComparison.Ordinal));
            }
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);

            if (obj != null)
            {
                // Give focus to the project window
                EditorUtility.FocusProjectWindow();

                // Select the object (the folder) in the project window
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
            else
            {
                Debug.LogError("Could not load object at path: " + folderPath);
            }
        }
    }
}