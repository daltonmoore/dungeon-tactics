using System;
using System.Collections.Generic;
using UnityEngine;

namespace Units
{
    [CreateAssetMenu(fileName = "Animation Config", menuName = "Units/Animation Config", order = 0)]
    public class AnimationConfigSO : ScriptableObject
    {
        public string characterName;
        public string variant;
        
        public List<SpriteAnimationParent> spriteAnimationParents = new();
    }

    [Serializable]
    public class SpriteAnimationParent
    {
        public AnimationType animationType;
        public List<SpriteAnimation> spriteAnimations = new();
        
        public enum AnimationType
        {
            Idle,
            Move
        }
    }
    
    [Serializable]
    public class SpriteAnimation
    {
        // example "Soldier_05_Idle
        public Direction direction = Direction.Down;
        public List<Sprite> sprites = new();
        
        public enum Direction
        {
            Down = 1,
            Left = 2,
            Right = 3,
            Up = 4
        }
    }
}