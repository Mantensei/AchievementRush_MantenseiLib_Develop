using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MantenseiLib;
using System.Linq;
using System;

namespace MantenseiLib.Obsolete
{
    public partial class DamageObject : BaseMonoBehaviour, IGetCollider, ITransformable<DamageObject>
    {
        public event System.Action onDestroyedEvent;
        public event System.Action onDeadEvent;
        public event Action<DamageResult> onDamagedEvent;
        partial void PartialDamageAction(DamageResult result);

        public int hp = 0;
        public bool dead = false;

        //[MantenseiDebug.DebugInfo(priority:-10)]
        TriggerEventCaller triggerEventCaller;
        //[MantenseiDebug.DebugInfo(priority:-10)]
        CollisionEventCaller collisionEventCaller;
        //[MantenseiDebug.DebugInfo(priority:-10)]
        public DamageInfo DamageInfo { get; set; } = DamageInfo.one;
        List<Collider2D> ignoreColliders = new();

        public IEnumerable<HitEventCaller> GetActiveEventCaller()
        {
            if (triggerEventCaller != null) yield return triggerEventCaller;
            if (collisionEventCaller != null) yield return collisionEventCaller;
        }

        Coroutine lifeTimerCoroutine;
        float lifeTime;

        public float LifeTime
        {
            get => lifeTime;
            set
            {
                if (lifeTimerCoroutine != null)
                    StopCoroutine(lifeTimerCoroutine);
                lifeTime = value;

                if(value != 0)
                    lifeTimerCoroutine = StartCoroutine(KillSelf(value));
            }
        }

        public TriggerEventCaller TriggerEventCaller
        {
            get => triggerEventCaller;
        }

        public TriggerEventCaller InitTriggerEventCaller(ColliderSettings colliderSettings)
        {
            if (triggerEventCaller == null)
            {
                triggerEventCaller = Construct<TriggerEventCaller>(colliderSettings);
            }
            return triggerEventCaller;
        }

        public CollisionEventCaller CollisionEventCaller
        {
            get => collisionEventCaller;
        }

        public CollisionEventCaller InitCollisionEventCaller(ColliderSettings colliderSettings)
        {
            if (collisionEventCaller == null)
            {
                collisionEventCaller = Construct<CollisionEventCaller>(colliderSettings);
                var rb2d = CollisionEventCaller.gameObject.AddComponent<Rigidbody2D>();
                rb2d.constraints = RigidbodyConstraints2D.FreezeAll;

                rb2d.gravityScale = 0;
                rb2d.drag = 0;
                rb2d.angularDrag = 0;
            }
            return collisionEventCaller;
        }

        public T Construct<T>(ColliderSettings colliderSettings) where T : HitEventCaller<T>
        {
            return HitEventCaller<T>.Constructor(colliderSettings, transform);
        }

        IEnumerator KillSelf(float lifeTime)
        {
            yield return new WaitForSeconds(lifeTime);
            //if(gameObject != null)
            if (this?.gameObject?.IsSafe() == true)
                    Destroy(gameObject);
        }

        public static DamageObject InstantiateSelf(DamageInfo damageInfo)
        {
            return InstantiateSelf(damageInfo, ColliderShape.circle);
        }
        
        public static DamageObject InstantiateSelf(DamageInfo damageInfo, ColliderShape colliderShape, EventTiming eventTiming = EventTiming.enter)
        {
            DamageObjectSettings damageObjectSettings = new DamageObjectSettings();
            damageObjectSettings.props = new DamageObjectProperties(damageInfo);
            damageObjectSettings.props.damageProps = new SerializableDamageInfo();
            damageObjectSettings.props.triggerSetting = ColliderSettings.DefaultTriggerSettings;
            damageObjectSettings.props.triggerSetting.TargetTag = TagType.allObjects;
            damageObjectSettings.props.triggerSetting.shape = colliderShape;
            damageObjectSettings.props.triggerSetting.EventTiming = eventTiming;


            return InstantiateSelf(damageObjectSettings);
        }

        public static DamageObject InstantiateSelf(DamageObjectSettings setting)
        {
            var props = setting.props;

            var go = new GameObject(typeof(DamageObject).Name);
#if UNITY_EDITOR
            //MantenseiDebug.TmpObject.InstantiateSelf(go.transform)
            //    .SetShape(props.triggerSetting.shape);
#endif

            var parent = props.parent ?? props.damageInfo.executor.GetOwner().transform;
            go.transform.CopyValues(parent);
            go.transform.SetParent(parent);
            var damageObject = go.AddComponent<DamageObject>();

            damageObject.LifeTime = setting.lifeTime;
            damageObject.hp = setting.hp;

            damageObject.SetCollisionDamage(props.collisionSetting);
            damageObject.SetTriggerDamage(props.triggerSetting);

            damageObject.DamageInfo = props.damageInfo;
            damageObject.onDamagedEvent += damageObject.PartialDamageAction;

            damageObject.onDamagedEvent += props.onDamageAction;
            damageObject.onDestroyedEvent += props.onDestroyAction;

            return damageObject;
        }

        public DamageObject DestroyTogether(GameObject go)
        {
            //onDestroyedEvent += () => gameObject.SetActive(false);
            onDestroyedEvent += () => Destroy(go);
            return this;
        }

        public DamageObject DestroyOnCollision(ColliderSettings colliderSetting = null, GameObject killTogether = null)
        {
            SetEventCallerSetting(CollisionEventCaller, colliderSetting);
            CollisionEventCaller.SetEvent((o) => { Destroy(gameObject); }, EventTiming.enter);
            if (killTogether != null)
                DestroyTogether(killTogether);
            return this;
        }

        public DamageObject DestroyOnTrigger(ColliderSettings colliderSetting = null, GameObject killTogether = null)
        {
            SetEventCallerSetting(TriggerEventCaller, colliderSetting);
            TriggerEventCaller.SetEvent((o) => { Destroy(gameObject); }, EventTiming.enter);
            if (killTogether != null)
                DestroyTogether(killTogether);
            return this;
        }

        public TriggerEventCaller SetTriggerDamage(ColliderSettings colliderSetting)
        {
            if (colliderSetting == null || colliderSetting.useSelf == false)
                return null;

            InitTriggerEventCaller(colliderSetting);

            SetEventCallerSetting(TriggerEventCaller, colliderSetting);
            return TriggerEventCaller;
        }

        public CollisionEventCaller SetCollisionDamage(ColliderSettings colliderSetting)
        {
            if (colliderSetting == null || colliderSetting.useSelf == false)
                return null;

            InitCollisionEventCaller(colliderSetting);

            SetEventCallerSetting(CollisionEventCaller, colliderSetting);
            return CollisionEventCaller;
        }

        public HitEventCaller<T> SetEventCallerSetting<T>(HitEventCaller<T> eventCaller, ColliderSettings setting) where T : HitEventCaller<T>
        {
            (_, var tagType, var layerType, var timing, var shape, var size) = setting;

            eventCaller.SetEvent(InflictDamage, timing);

            if (setting.onHitDestroy)
                //eventCaller.SetEvent((g) => DestroySelf(), timing);
                eventCaller.SetEvent((g) => Destroy(gameObject), timing);

            if (setting.destroyParentTogether)
            {
                if(transform?.parent?.gameObject != null)
                    DestroyTogether(transform.parent.gameObject);
            }

            eventCaller.ChangeTargetTags(tagType);
            eventCaller.ChangeLayer(layerType);
            eventCaller.SetSize(size);

            return eventCaller;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            onDestroyedEvent?.Invoke();
        }

        void InflictDamage(GameObject gameObject)
        {
            foreach (var damagable in gameObject.GetComponents<IDamagable>())
            {
                InflictDamage(damagable);
            }
        }

        void InflictDamage(IDamagable damagable)
        {
            if (damagable?.IsSafe() == false) return;

            var result = damagable.ApplyDamage(DamageInfo);
            onDamagedEvent?.Invoke(result);

            if (hp != 0 
                && !result.Contains(DamageResultType.miss))
                //&& !result.ResultTypes.Any())
            {
                hp--;
                if (hp <= 0 && dead == false)
                {
                    onDeadEvent?.Invoke();
                    Destroy(gameObject);
                    dead = true;
                }
            }
        }

        public Collider2D GetCollider() => GetActiveEventCaller().FirstOrDefault()?.Col;

        public IEnumerable<Collider2D> GetColliders()
        {
            return GetActiveEventCaller().Select(x => x.Col);
        }

        public List<Collider2D> GetIgnoreColliders() => ignoreColliders;

        public DamageObject SetLifeTime(float lifeTime)
        {
            LifeTime = lifeTime;
            return this;
        }
        
        public DamageObject SetLayer(string layerName)
        {
            gameObject.ChangeLayer(layerName);
            return this;
        }
        
        public DamageObject SetDestroyEvent(Action action)
        {
            onDestroyedEvent += action;
            return this;
        }

        public DamageObject SetAttackInterval(float interval)
        {
            if (interval == 0) return this;

            onDamagedEvent += g =>
            {
                foreach (var col in GetActiveEventCaller().Select(x => x.Col))
                    col.enabled = false;
                
                DelayedCall(interval, () =>
                {
                    foreach (var col in GetActiveEventCaller().Select(x => x.Col))
                        col.enabled = true;
                });
            };

            //var tmpDamage = DamageInfo.Damage;
            //var tmpCount = count;
            //onDamagedEvent += g =>
            //{
            //    tmpCount--;

            //    if (tmpCount <= 0)
            //    {
            //        DamageInfo.Damage = tmpDamage;
            //        tmpCount = count;
            //    }
            //    else
            //    {
            //        DamageInfo.Damage = 0;
            //    }
            //};


            return this;
        }
    }

    public interface IGetDamageObject
    {
        DamageObject GetDamageObject();
    }

    [Serializable]
    public struct DamageObjectSettings
    {
        public int hp;
        public float lifeTime;

        public DamageObjectProperties props;
    }

    [Serializable]
    public class DamageObjectProperties
    {
        [NonSerialized] public Transform parent;
        [NonSerialized] public DamageInfo damageInfo;

        public DamageInfo GetNewDamageInfo()
        {
            var info = GetNewDamageInfo(damageInfo.executor);
            info.AddDamageProp(damageProps.damageTypes.ToArray());
            return info;
        }

        public DamageInfo GetNewDamageInfo(Helper helper)
        {
            if (helper == null)
                throw new NullReferenceException("Helper‚ªnull‚Å‚·");

            var info = new DamageInfo(helper);
            var propInfo = damageProps;

            var serializableInfo = info.serializableInfo;
            serializableInfo.damage = propInfo.damage;
            serializableInfo.damageTypes = propInfo.damageTypes?.ToArray();
            serializableInfo.HitParticle = propInfo.HitParticle;
            serializableInfo.DestroyParticle = propInfo.DestroyParticle;

            return info;
        }

        public SerializableDamageInfo damageProps;

        public ColliderSettings collisionSetting;
        public ColliderSettings triggerSetting;

        [NonSerialized] public Action<DamageResult> onDamageAction;
        [NonSerialized] public System.Action onDestroyAction;

        public DamageObjectProperties()
        {
            damageInfo = new DamageInfo(1);
        }

        public DamageObjectProperties(DamageInfo damageInfo)
        {
            SetDamageInfo(damageInfo);
        }

        public void SetDamageInfo(DamageInfo damageInfo)
        {
            this.damageInfo = damageInfo;
            parent = damageInfo.executor?.transform;
        }
    }


    [Serializable]
    public class ColliderSettings
    {
        public bool useSelf = true;
        public ColliderShape shape = ColliderShape.rect;
        public bool IsTrigger;
        public bool onHitDestroy;
        public bool destroyParentTogether = false;
        public TagType TargetTag;
        public LayerType LayerType;
        public EventTiming EventTiming = EventTiming.enter;
        public Vector3 ColliderSize = Vector3.one;

        public static ColliderSettings DefaultTriggerSettings =>
            new ColliderSettings()
            {
                IsTrigger = true,
                TargetTag = TagType.allChara,
                LayerType = LayerType.Default,
                EventTiming = EventTiming.enter,
                ColliderSize = Vector3.one,
            };

        public static ColliderSettings DefaultCollisionSettings =>
            new ColliderSettings()
            {
                IsTrigger = false,
                LayerType = LayerType.IgnoreHelper,
                TargetTag = TagType.stage,
                EventTiming = EventTiming.enter,
                ColliderSize = Vector3.one,
            };

        public void Deconstruct(out bool isTrigger, out TagType targetTag, out LayerType layerType, out EventTiming eventTiming, out ColliderShape shape, out Vector3 colliderSize)
        {
            isTrigger = IsTrigger;
            targetTag = TargetTag;
            layerType = LayerType;
            eventTiming = EventTiming;
            shape = this.shape;
            colliderSize = ColliderSize;
        }

    }

    public enum ColliderShape
    {
        rect,
        circle,
    }
}
