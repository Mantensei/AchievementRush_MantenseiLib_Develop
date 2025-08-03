using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using System.Linq;
using System;

namespace MantenseiLib.Obsolete
{
    public abstract class HitEventCaller<T> : HitEventCaller where T : HitEventCaller<T>
    {
        public virtual void OnConstruct() { }
        public static T Constructor(ColliderSettings setting, Transform transform)
        {
            (_, var tagType, var layerType, var timing, var shape, var size) = setting;

            var eventCaller = Constructor(transform, tagType, shape);
            eventCaller.gameObject.ChangeLayer(layerType);
            eventCaller.OnConstruct();

            return eventCaller;
        }

        //public static T Contstructor(Action<GameObject> action, EventTiming timing, Transform transform, LayerType layerType, TagType tagType)
        //{
        //    var eventCaller = Constructor(transform, tagType);
        //    eventCaller.gameObject.ChangeLayer(layerType);
        //    eventCaller.SetEvent((g) => action(g), timing);

        //    return eventCaller;
        //}


        public static T Constructor(Transform source, TagType tagType, ColliderShape shape = ColliderShape.rect, bool setParent = true)
        {
            var obj = new GameObject(typeof(T).Name + "_" + source.gameObject.name);
            obj.transform.CopyValues(source);

            if (setParent)
                obj.transform.SetParent(source);

            var eventCaller = obj.AddComponent<T>();
            eventCaller.ChangeTargetTags(tagType);
            eventCaller.ActivateCollider(shape);
            eventCaller.OnConstruct();

            return eventCaller;
        }
    }

    public abstract class HitEventCaller : BaseMonoBehaviour, IGetCollider
    {

        public string[] TargetTags { get; private set; } = TagManager.GetTags(TagType.allChara);
        public event Action<GameObject> onEnterEvent;
        public event Action<GameObject> onStayEvent;
        public event Action<GameObject> onExitEvent;
        public Collider2D Col { get; private set; }
        public List<Collider2D> IgnoreColliders { get; private set; } = new List<Collider2D>();

        protected void InvokeEnterAction(GameObject obj)
        {
            onEnterEvent?.Invoke(obj);
        }

        protected void InvokeStayAction(GameObject obj)
        {
            onStayEvent?.Invoke(obj);
        }

        protected void InvokeExitAction(GameObject obj)
        {
            onExitEvent?.Invoke(obj);
        }

        public void ChangeTargetTags(params string[] tags)
        {
            TargetTags = tags;
        }

        public void ChangeTargetTags(TagType tagType)
        {
            ChangeTargetTags(TagManager.GetTags(tagType));
        }

        public void ChangeLayer(LayerType layerType)
        {
            gameObject.ChangeLayer(layerType);
        }

        //public override void Initialize()
        //{
        //    base.Initialize();
        //    ActivateCollider();
        //}

        public void ActivateCollider(ColliderShape shape)
        {
            switch (shape)
            {
                case ColliderShape.rect:
                    Col = Col ?? gameObject.AddComponent<BoxCollider2D>();
                    break;
                case ColliderShape.circle:
                    Col = Col ?? gameObject.AddComponent<CircleCollider2D>();
                    break;
            }
        }

        public void ActivateCollider()
        {
            Col = Col ?? gameObject.AddComponent<BoxCollider2D>();
        }

        public virtual void SetEvent(Action<GameObject> action, EventTiming timing)
        {
            if ((timing & EventTiming.enter) == EventTiming.enter)
                onEnterEvent += action;

            if ((timing & EventTiming.stay) == EventTiming.stay)
                onStayEvent += action;

            if ((timing & EventTiming.exit) == EventTiming.exit)
                onExitEvent += action;
        }

        public virtual void SetSize(object size)
        {
            switch (Col)
            {
                case BoxCollider2D boxCollider2D:
                    boxCollider2D.size = (Vector3)size;
                    break;

                case CircleCollider2D circleCollider2D:
                    circleCollider2D.radius = ((Vector3)size).x / 2;
                    break;

                default: break;
            }
        }

        public Collider2D GetCollider() => Col;

        public List<Collider2D> GetIgnoreColliders() => IgnoreColliders;


        public void _DebugLog()
        {
            Debug.Log
                (
                $"GameObject = {gameObject} \n" +
                $"Tags = {TargetTags.JoinToString()} \n" +
                ""
                );
        }
    }

    [Flags]
    public enum EventTiming
    {
        enter   = 1,
        stay    = 2,
        exit    = 4,
    }

    public class EventCallSetting
    {

    }
}
