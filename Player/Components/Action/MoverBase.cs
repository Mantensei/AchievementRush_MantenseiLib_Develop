using System.Collections;
using System.Collections.Generic;
using MantenseiLib;
using UnityEngine;
using DG.Tweening;

namespace MantenseiLib
{
    public class MoverBase : MonoBehaviour
    {
        public float speed = 2f;
        private bool isMoving = false;

        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        ActionStateLock _stateLock;

        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
        IRb2d _irb2d;
        public Rigidbody2D rb2d => _irb2d.rb2d;
        new Transform transform => _irb2d.transform;
        public Vector2 Velocity => rb2d.velocity;

        public void Move()
        {
            var dir = transform.Right2D();
            Move(dir.x);
        }

        public void Move(float dir)
        {
            if(_stateLock?.AllowMove == false)
                return;

            var velo = rb2d.velocity;
            rb2d.velocity = new Vector2(speed * dir, velo.y);
            isMoving = true; 
        }

        protected void Update()
        {
            if (!isMoving)
            {
                rb2d.velocity = new Vector2(0, rb2d.velocity.y);
            }
            isMoving = false;
        }
    }

}