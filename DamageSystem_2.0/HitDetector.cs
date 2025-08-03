using System;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib.Develop
{
    public enum DetectionType
    {
        Trigger,
        Collision
    }

    public enum HitTiming
    {
        Enter,
        Stay,
        Exit
    }

    public enum ColliderShape
    {
        Box,
        Circle
    }

    public class HitInfo
    {
        public GameObject HitObject { get; set; }
        public Collider2D HitCollider { get; set; }
        public Collider2D OwnCollider { get; set; }
        public Vector2 ContactPoint { get; set; }
        public Vector2 ContactNormal { get; set; }
        public Vector2 RelativeVelocity { get; set; }
        public DetectionType DetectionType { get; set; }
    }

    public partial class HitDetector : MonoBehaviour, ITransformable<HitDetector>
    {
        [field:SerializeField] public DetectionType detectionType { get; private set; } = DetectionType.Trigger;
        [field:SerializeField] public HitTiming timing { get; private set; } = HitTiming.Enter;
        [field:SerializeField] public string[] targetTags { get; private set; } = null;
        [field: SerializeField] public LayerMask targetLayers { get; private set; } = -1;
        [field: SerializeField] public ColliderShape colliderShape { get; private set; } = ColliderShape.Box;

        private Collider2D ownCollider;

        public event Action<HitInfo> onHit;

        //同一ゲームオブジェクトに重複して判定しないようにする
        private int lastFrameNumber = -1;
        private HashSet<GameObject> hitObjectsThisFrame = new HashSet<GameObject>();

        // 追加
        [field: SerializeField] public float hitInterval { get; private set; } = 0f; // Stay時のヒット間隔

        // 追加
        private Dictionary<GameObject, float> lastHitTimes = new Dictionary<GameObject, float>();


        private void Start()
        {
            if (ownCollider == null)
                CreateCollider(colliderShape, Vector2.one);

            UpdateColliderType();
        }

        private void CreateCollider(ColliderShape shape, Vector2 size)
        {
            if (ownCollider != null)
                DestroyImmediate(ownCollider);

            switch (shape)
            {
                case ColliderShape.Box:
                    var boxCollider = gameObject.AddComponent<BoxCollider2D>();
                    boxCollider.size = size;
                    ownCollider = boxCollider;
                    break;

                case ColliderShape.Circle:
                    var circleCollider = gameObject.AddComponent<CircleCollider2D>();
                    circleCollider.radius = size.x * 0.5f; // sizeのx成分を直径として使用
                    ownCollider = circleCollider;
                    break;
            }

            colliderShape = shape;
        }

        private void UpdateColliderType()
        {
            if (ownCollider != null)
            {
                ownCollider.isTrigger = (detectionType == DetectionType.Trigger);
            }
        }

        private bool IsValidTarget(GameObject target)
        {
            // Layer check: targetLayersが-1なら全レイヤー許可
            if (targetLayers != -1 && !IsInLayerMask(target.layer, targetLayers))
                return false;

            // Tag check: targetTagsがnullまたは空配列なら全Tag許可
            if (targetTags != null && targetTags.Length > 0)
            {
                // タグ指定がある場合のみチェック
                bool tagMatch = false;
                foreach (var tag in targetTags)
                {
                    if (target.CompareTag(tag))
                    {
                        tagMatch = true;
                        break;
                    }
                }
                if (!tagMatch) return false;
            }

            return true;
        }

        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return (layerMask.value & (1 << layer)) != 0;
        }


        private void NotifyHit(HitInfo hitInfo)
        {
            // フレーム重複除去
            int currentFrame = Time.frameCount;
            if (currentFrame != lastFrameNumber)
            {
                hitObjectsThisFrame.Clear();
                lastFrameNumber = currentFrame;
            }

            if (hitObjectsThisFrame.Contains(hitInfo.HitObject))
                return;

            // Stay時のヒット間隔制御
            if (timing == HitTiming.Stay)
            {
                if (hitInterval > 0 && lastHitTimes.TryGetValue(hitInfo.HitObject, out float lastHitTime))
                {
                    if (Time.time - lastHitTime < hitInterval)
                        return;
                }
                lastHitTimes[hitInfo.HitObject] = Time.time;
            }
            else
            {
                lastHitTimes[hitInfo.HitObject] = Time.time;
            }

            hitObjectsThisFrame.Add(hitInfo.HitObject);
            onHit?.Invoke(hitInfo);
        }

        private HitInfo CreateHitInfo(GameObject hitObject, Collider2D hitCollider, HitTiming timing)
        {
            return new HitInfo
            {
                HitObject = hitObject,
                HitCollider = hitCollider,
                OwnCollider = ownCollider,
                DetectionType = detectionType,
                ContactPoint = Vector2.zero, // TODO: 実際の接触点を計算
                ContactNormal = Vector2.zero, // TODO: 実際の法線を計算
                RelativeVelocity = Vector2.zero // TODO: 相対速度を計算
            };
        }

        #region Trigger Events
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (timing != HitTiming.Enter) return;
            if (detectionType != DetectionType.Trigger) return;
            if (!IsValidTarget(other.gameObject)) return;

            var hitInfo = CreateHitInfo(other.gameObject, other, HitTiming.Enter);
            NotifyHit(hitInfo);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (timing != HitTiming.Stay) return;
            if (detectionType != DetectionType.Trigger) return;
            if (!IsValidTarget(other.gameObject)) return;

            var hitInfo = CreateHitInfo(other.gameObject, other, HitTiming.Stay);
            NotifyHit(hitInfo);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (timing != HitTiming.Exit) return;
            if (detectionType != DetectionType.Trigger) return;
            if (!IsValidTarget(other.gameObject)) return;

            var hitInfo = CreateHitInfo(other.gameObject, other, HitTiming.Exit);
            NotifyHit(hitInfo);
        }
        #endregion

        #region Collision Events
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (timing != HitTiming.Enter) return;
            if (detectionType != DetectionType.Collision) return;
            if (!IsValidTarget(collision.gameObject)) return;

            var hitInfo = CreateHitInfo(collision.gameObject, collision.collider, HitTiming.Enter);
            hitInfo.ContactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : Vector2.zero;
            hitInfo.ContactNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector2.zero;
            hitInfo.RelativeVelocity = collision.relativeVelocity;

            NotifyHit(hitInfo);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (timing != HitTiming.Stay) return;
            if (detectionType != DetectionType.Collision) return;
            if (!IsValidTarget(collision.gameObject)) return;

            var hitInfo = CreateHitInfo(collision.gameObject, collision.collider, HitTiming.Stay);
            hitInfo.ContactPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : Vector2.zero;
            hitInfo.ContactNormal = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector2.zero;
            hitInfo.RelativeVelocity = collision.relativeVelocity;

            NotifyHit(hitInfo);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (timing != HitTiming.Exit) return;
            if (detectionType != DetectionType.Collision) return;
            if (!IsValidTarget(collision.gameObject)) return;

            var hitInfo = CreateHitInfo(collision.gameObject, collision.collider, HitTiming.Exit);
            NotifyHit(hitInfo);
        }
        #endregion
    }

    // ファクトリメソッドとメソッドチェーン用のpartial class
    public partial class HitDetector
    {
        #region Static Factory Methods

        /// <summary>
        /// デフォルトBox Collider (1x1) でHitDetectorを生成
        /// </summary>
        public static HitDetector Factory(Vector2 position)
        {
            return Factory(position, Vector2.one, ColliderShape.Box);
        }

        /// <summary>
        /// 正方形のBox Collider でHitDetectorを生成
        /// </summary>
        public static HitDetector Factory(Vector2 position, float size)
        {
            return Factory(position, Vector2.one * size, ColliderShape.Box);
        }

        /// <summary>
        /// 長方形のBox Collider でHitDetectorを生成
        /// </summary>
        public static HitDetector Factory(Vector2 position, float width, float height)
        {
            return Factory(position, new Vector2(width, height), ColliderShape.Box);
        }

        /// <summary>
        /// 指定した形状とサイズでHitDetectorを生成
        /// </summary>
        public static HitDetector Factory(Vector2 position, Vector2 size, ColliderShape shape)
        {
            var gameObject = new GameObject("HitDetector");
            gameObject.transform.position = position;

            var detector = gameObject.AddComponent<HitDetector>();
            detector.colliderShape = shape;
            detector.CreateCollider(shape, size);
            detector.UpdateColliderType();

            return detector;
        }

        #endregion

        #region Method Chain Setters

        public HitDetector SetDetectionType(DetectionType type)
        {
            detectionType = type;
            UpdateColliderType();
            return this;
        }

        public HitDetector SetTargetTags(params string[] tags)
        {
            targetTags = tags;
            return this;
        }

        public HitDetector SetTargetLayers(LayerMask layers)
        {
            targetLayers = layers;
            return this;
        }

        public HitDetector SetCollider(ColliderShape shape, Vector2 size)
        {
            CreateCollider(shape, size);
            UpdateColliderType();
            return this;
        }

        public HitDetector SetCollider(ColliderShape shape, float size)
        {
            return SetCollider(shape, Vector2.one * size);
        }
        public HitDetector SetTiming(HitTiming timing)
        {
            this.timing = timing;
            return this;
        }

        public HitDetector SetHitInterval(float interval)
        {
            hitInterval = interval;
            return this;
        }

        public HitDetector OnHit(Action<HitInfo> action)
        {
            onHit += action;
            return this;
        }

        #endregion
    }
}