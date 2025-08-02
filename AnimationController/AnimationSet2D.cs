using MantenseiLib;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib.Animation
{
    //[CreateAssetMenu(fileName = "AnimationSet", menuName = "Animation/Animation Set 2D")]
    public class AnimationSet2D : ScriptableObject
    {
        [SerializeField] private List<AnimationData2D> animations = new List<AnimationData2D>();

        public AnimationData2D GetAnimation(string name)
        {
            return animations.Find(a => a.name == name);
        }

        public void AddAnimation(AnimationData2D animation)
        {
            animations.Add(animation);
        }
    } 
}