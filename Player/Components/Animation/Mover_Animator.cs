using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib.Internal
{
    public class Mover_Animator : MonoBehaviour
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        public IPlayerHub player { get; private set; }

        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        IAnimator2D animator;
        public Animator2D Animator => animator.Animator;
        public SpriteRenderer sr => Animator.sr;

        [GetComponent]
        Animation2DRegisterer _animPlayer;
        MoverBase mover => player.Mover;


        private void Update()
        {
            var velo = mover.Velocity;
            var dir = velo.x;
                        
            if (dir == 0)
            {
                _animPlayer?.Pause();
            }
            else
            {
                if (dir > 0)
                {
                    sr.transform.localScale = new Vector3(1, 1, 1);
                }
                else
                {
                    sr.transform.localScale = new Vector3(-1, 1, 1);
                }

                _animPlayer?.Play();
            }
        }
    }
}