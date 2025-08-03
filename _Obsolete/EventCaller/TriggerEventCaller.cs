using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using System.Linq;

namespace MantenseiLib.Obsolete
{
    public class TriggerEventCaller : HitEventCaller<TriggerEventCaller>
    {
        public override void OnConstruct()
        {
            Col.isTrigger = true;
        }

        public override void Initialize()
        {
            base.Initialize();
            Col.isTrigger = true;
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);

            if (collision.gameObject.CompareTags(TargetTags))
                InvokeEnterAction(collision.gameObject);
        }

        protected override void OnTriggerStay2D(Collider2D collision)
        {
            base.OnTriggerStay2D(collision);

            if (collision.gameObject.CompareTags(TargetTags))
                InvokeStayAction(collision.gameObject);
        }

        protected override void OnTriggerExit2D(Collider2D collision)
        {
            base.OnTriggerExit2D(collision);

            if (collision.gameObject.CompareTags(TargetTags))
                InvokeExitAction(collision.gameObject);
        }
    }

    public class TriggerEventCaller<T> : HitEventCaller<T> where T: HitEventCaller<T>
    {
        public override void Initialize()
        {
            base.Initialize();
            Col.isTrigger = true;
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);

            if (collision.gameObject.CompareTags(TargetTags))
                InvokeEnterAction(collision.gameObject);
        }

        protected override void OnTriggerStay2D(Collider2D collision)
        {
            base.OnTriggerStay2D(collision);

            if (collision.gameObject.CompareTags(TargetTags))
                InvokeStayAction(collision.gameObject);
        }

        protected override void OnTriggerExit2D(Collider2D collision)
        {
            base.OnTriggerExit2D(collision);

            if (collision.gameObject.CompareTags(TargetTags))
                InvokeExitAction(collision.gameObject);
        }
    }
}
