using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib.Internal
{
    public class Fall_Animator : MonoBehaviour
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        public IPlayerHub player { get; private set; }

        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        IAnimator2D animator;
        public Animator2D Animator => animator.Animator;
        public SpriteRenderer sr => Animator.sr;

        [GetComponent]
        Animation2DRegisterer _animPlayer;

        private void Update()
        {
            var velo = player.rb2d.velocity;
            var gravity = velo.y;

            if(!(player?.GroundChecker?.IsGround() == true) && velo.y < -0.1f)
            {
                _animPlayer?.Play();
            }
            else
            {
                _animPlayer?.Pause();
            }
        }
    } 
}
