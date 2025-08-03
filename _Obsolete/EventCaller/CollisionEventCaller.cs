using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using System.Linq;
using System;

namespace MantenseiLib.Obsolete
{
    public class CollisionEventCaller : HitEventCaller<CollisionEventCaller>
    {
        protected override void OnCollisionEnter2D(Collision2D collision)
        {
            base.OnCollisionEnter2D(collision);

            if (collision.gameObject.CompareTags(TargetTags))
                InvokeEnterAction(collision.gameObject);
        }

        protected override void OnCollisionStay2D(Collision2D collision)
        {
            base.OnCollisionStay2D(collision);

            if (collision.gameObject.CompareTags(TargetTags))
                InvokeStayAction(collision.gameObject);
        }

        protected override void OnCollisionExit2D(Collision2D collision)
        {
            base.OnCollisionExit2D(collision);

            if (collision.gameObject.CompareTags(TargetTags))
                InvokeExitAction(collision.gameObject);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            transform.position = transform.parent.position;
        }
    }
    
}
