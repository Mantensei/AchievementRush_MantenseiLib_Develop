using System;
using System.Collections;
using UnityEngine;

namespace MantenseiLib.Develop
{
    public partial class DamageObject : MonoBehaviour, ITransformable<DamageObject>
    {
        [field:SerializeField] private DamageInfo damageInfo;
        [field:SerializeField] private float lifeTime = 5f;
        [field:SerializeField] private bool autoDestroy = true;

        private HitDetector _hitDetector;
        private Coroutine lifeTimeCoroutine;

        public event Action<HitInfo, DamageResult> onDamageApplied;
        public event Action<DamageObject> onDestroyed;

        #region Unity Lifecycle

        private void Start()
        {
            if (autoDestroy && lifeTime > 0f)
            {
                lifeTimeCoroutine = StartCoroutine(LifeTimeCoroutine());
            }
        }

        private void OnDestroy()
        {
            onDestroyed?.Invoke(this);
        }

        #endregion

        #region IHitReceiver Implementation

        void OnHit(HitInfo hitInfo)
        {
            if (damageInfo == null)
            {
                Debug.LogWarning("DamageInfo is null. Cannot apply damage.");
                return;
            }

            // ダメージ適用対象の取得
            var damageable = hitInfo.HitObject.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damageInfo);
                onDamageApplied?.Invoke(hitInfo, damageInfo.Result);
            }
        }

        #endregion

        #region Lifetime Management

        private IEnumerator LifeTimeCoroutine()
        {
            yield return new WaitForSeconds(lifeTime);
            DestroyObject();
        }

        public void DestroyObject()
        {
            if (lifeTimeCoroutine != null)
            {
                StopCoroutine(lifeTimeCoroutine);
                lifeTimeCoroutine = null;
            }

            Destroy(gameObject);
        }

        public void ExtendLifeTime(float additionalTime)
        {
            lifeTime += additionalTime;

            // コルーチンを再開始
            if (lifeTimeCoroutine != null)
            {
                StopCoroutine(lifeTimeCoroutine);
            }

            if (autoDestroy && lifeTime > 0f)
            {
                lifeTimeCoroutine = StartCoroutine(LifeTimeCoroutine());
            }
        }

        #endregion


        #region Private Methods

        private void CreateHitDetector(ColliderShape shape, Vector2 size)
        {
            _hitDetector = gameObject.AddComponent<HitDetector>();
            _hitDetector.SetCollider(shape, size)
                      .OnHit(this.OnHit);
        }

        #endregion
    }

    public partial class DamageObject
    {
        #region Factory Methods

        /// <summary>
        /// デフォルト設定でDamageObjectを生成
        /// </summary>
        public static DamageObject Factory(DamageInfo damageInfo, Vector2 position)
        {
            return Factory(damageInfo, position, Vector2.one, ColliderShape.Box);
        }

        /// <summary>
        /// 正方形のコライダーでDamageObjectを生成
        /// </summary>
        public static DamageObject Factory(DamageInfo damageInfo, Vector2 position, float size)
        {
            return Factory(damageInfo, position, Vector2.one * size, ColliderShape.Box);
        }

        /// <summary>
        /// 長方形のコライダーでDamageObjectを生成
        /// </summary>
        public static DamageObject Factory(DamageInfo damageInfo, Vector2 position, float width, float height)
        {
            return Factory(damageInfo, position, new Vector2(width, height), ColliderShape.Box);
        }

        /// <summary>
        /// 指定した形状とサイズでDamageObjectを生成
        /// </summary>
        public static DamageObject Factory(DamageInfo damageInfo, Vector2 position, Vector2 size, ColliderShape shape)
        {
            var gameObject = new GameObject("DamageObject");
            gameObject.transform.position = position;

            var damageObject = gameObject.AddComponent<DamageObject>();
            damageObject.CreateHitDetector(shape, size);

            damageObject.damageInfo = damageInfo;

            return damageObject;
        }

        #endregion


        #region Method Chain Setters

        public DamageObject SetDamageInfo(DamageInfo info)
        {
            damageInfo = info;
            return this;
        }

        public DamageObject SetLifeTime(float time)
        {
            lifeTime = time;
            return this;
        }

        public DamageObject SetAutoDestroy(bool enable)
        {
            autoDestroy = enable;
            return this;
        }

        public DamageObject SetDetectionType(DetectionType type)
        {
            _hitDetector?.SetDetectionType(type);
            return this;
        }

        public DamageObject SetTiming(HitTiming timing)
        {
            _hitDetector?.SetTiming(timing);
            return this;
        }

        public DamageObject SetTargetTags(params string[] tags)
        {
            _hitDetector?.SetTargetTags(tags);
            return this;
        }

        public DamageObject SetTargetLayers(LayerMask layers)
        {
            _hitDetector?.SetTargetLayers(layers);
            return this;
        }

        public DamageObject SetHitInterval(float interval)
        {
            _hitDetector?.SetHitInterval(interval);
            return this;
        }

        public DamageObject OnDamageApplied(Action<HitInfo, DamageResult> action)
        {
            onDamageApplied += action;
            return this;
        }

        public DamageObject OnDestroyed(Action<DamageObject> action)
        {
            onDestroyed += action;
            return this;
        }

        #endregion
    }
}