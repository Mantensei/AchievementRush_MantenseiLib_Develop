using System.Collections;
using System.Collections.Generic;
using MantenseiLib;
using UnityEngine;
using DG.Tweening;

namespace MantenseiLib
{
    public class MoverBase : HubChild<IHelper>
    {
        //[GetComponent]
        //StateMarker stateMarker;

        public float speed = 2f;
        private bool isMoving = false; 
        private Rigidbody2D rb2d => HUB.rb2d;
        new private Transform transform => HUB.transform;
        public Vector2 Velocity => rb2d.velocity;

        public void Move()
        {
            var dir = transform.Right2D();
            Move(dir.x);
        }

        public void Move(float dir)
        {
            var velo = rb2d.velocity;
            rb2d.velocity = new Vector2(speed * dir, velo.y);
            isMoving = true; 
        }

        protected override void Update()
        {
            base.Update();

            if (!isMoving)
            {
                rb2d.velocity = new Vector2(0, rb2d.velocity.y);
            }
            isMoving = false;
        }
    }

}