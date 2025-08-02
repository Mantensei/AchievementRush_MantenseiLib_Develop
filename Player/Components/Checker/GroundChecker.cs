using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using System.Linq;
using System;

namespace MantenseiLib.UnityAction.Action2D
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GroundChecker : BaseMonoBehaviour
    {
        [SerializeField] Direction direction = Direction.Down;
        public Direction Direction => direction;

        [SerializeField]
        ContactFilter2D contactFilter2D;
        public ContactFilter2D ContactFilter2D { get => contactFilter2D;}

        [GetComponent]
        BoxCollider2D boxCollider2D;
        [GetComponent(relation = HierarchyRelation.Parent)] 
        Rigidbody2D rb2d;

        public string[] groundTags = { "Stage", "Character" };

        private bool isGround = false;
        private bool isGroundEnter, isGroundStay, isGroundExit;

        public event Action onGroundAction;
        public void OnGroundAction(Action action)
        {
            onGroundAction += action;
        }

        bool IsOwner(Collider2D collider)
        {
            return false;
            //return collider.gameObject == OwnerChara.gameObject; 
        }
        bool Contacts()
        {
            return rb2d.IsTouching(ContactFilter2D);
            //return true;
            ////return OwnerChara.BoxCollider2D.IsTouching(ContactFilter2D);
        }

        public void LeftGround()
        {
            isGroundEnter = false;
            isGroundStay = false;
            isGroundExit = false;
            isGround = false;
        }

        //接地判定を返すメソッド
        //物理判定の更新毎に呼ぶ必要がある
        public bool IsGround()
        {
            bool wasGround = isGround;
            if (isGroundEnter || isGroundStay)
            {
                isGround = true;
            }
            else if (isGroundExit)
            {
                isGround = false;
            }

            isGroundEnter = false;
            isGroundStay = false;
            isGroundExit = false;

            if (wasGround == false && isGround == true)
                onGroundAction?.Invoke();

            return isGround;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);

            if (!Contacts())
                return;

            CheckTag(collision, ref isGroundEnter);
        }

        protected override void OnTriggerStay2D(Collider2D collision)
        {
            base.OnTriggerStay2D(collision);

            if (!Contacts())
                return;

            CheckTag(collision, ref isGroundStay);
        }

        protected override void OnTriggerExit2D(Collider2D collision)
        {
            base.OnTriggerExit2D(collision);
            CheckTag(collision, ref isGroundExit);
        }

        void CheckTag(Collider2D collision, ref bool value)
        {
            if (IsOwner(collision))
                return;

            value = groundTags.Any(x => collision.CompareTag(x));
            IsGround();
        }
    }
    
}
