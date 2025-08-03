using System.Collections;
using System.Collections.Generic;
using MantenseiLib;
using UnityEngine;
using System.Linq;
using MantenseiLib.UnityAction.Action2D;

namespace MantenseiLib
{
    public class PlayerAI : HubChild<IPlayerHub>, ICallByGameEvent
    {
        MoverBase Mover => HUB.Mover;
        new Transform transform => HUB.transform;
        Timer timer;

        [SerializeField]
        string[] LayerMasks = new []{"Helper", "Stage"};
        string[] Tags = new[] { "Ground", "Player"};

        protected override void Start()
        {
            base.Start();

            timer = new Timer(1f / Mover.speed)
                .SetRepeat(true)
                .OnCompleteAction(ObserveRay);
        }

        protected override void Update()
        {
            base.Update();
            Mover.Move();
        }

        void ObserveRay()
        {
            if (!gameObject.activeSelf)
                return;

            var isGround = HUB.GroundChecker.IsGround();

            if (CheckWall())
            {
                if ((CheckJumpableWall() || CheckBackWall()) && isGround)
                {
                    HUB.Jumper.Jump();
                }
                else
                {
                    var scale = transform.localScale;
                    scale.x *= -1;
                    transform.localScale = scale;
                }
            }
        }

        public bool CheckWall()
        {
            var dir = Mathf.Sign(transform.localScale.x);
            return CheckForObstacle(dir * Vector2.right);
        }

        public bool CheckJumpableWall()
        {
            var up = Vector2.up;
            var dir = Vector2.right * Mathf.Sign(transform.localScale.x);

            return !CheckForObstacle(up) && !CheckForObstacle(up, dir);
        }

        public bool CheckBackWall()
        {
            var dir = Mathf.Sign(transform.localScale.x);
            return CheckForObstacle(dir * Vector2.right * -1);
        }

        public bool CheckForObstacle(Vector2 direction) => CheckForObstacle(Vector2.zero, direction);
        public bool CheckForObstacle(Vector3 origin, Vector2 direction)
        {
            var colliders = HUB.Colliders.Where(x => x.enabled).ToArray();
            void EnableCollider(bool value)
            {
                foreach (var collider in colliders)
                    collider.enabled = value;
            }

            var mask = LayerMask.GetMask(LayerMasks);
            EnableCollider(false); // 一時的に無効化
            var rayLength = 1;

            // RaycastAllで複数のヒット結果を取得
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position + origin, direction, rayLength, mask);
#if UNITY_EDITOR
            Debug.DrawRay(transform.position + origin, direction, Color.red, rayLength);
#endif

            // すべてのヒット結果を確認
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.gameObject.CompareTags(Tags))
                {
                    EnableCollider(true); // コライダーを有効化
                    return true; // 壁判定がある場合
                }
            }

            EnableCollider(true); // コライダーを有効化
            return false; // 壁判定がない場合
        }


        public void CallByGameEvent(GameEvent gameEvent)
        {
            switch (gameEvent)
            {
                case GameEvent.GameStart:
                    break;
                case GameEvent.GameClear:
                    break;
                case GameEvent.GameOver:
                    break;
                case GameEvent.GameExit:
                    break;
                case GameEvent.StageStart:
                    enabled = false;
                    break;
                case GameEvent.StageClear:
                    break;
                case GameEvent.StageMiss:
                    break;
                case GameEvent.StageExit:
                    break;
                case GameEvent.Pause:
                    break;
                case GameEvent.Play:
                    enabled = true;
                    break;
                case GameEvent.Fast:
                    break;
            }
        }
    }

    public interface IPlayerHub : IRb2d
    {
        //PlayerStateManager PlayerStateManager { get; }
        MoverBase Mover { get; }
        Jumper Jumper { get; }
        GroundChecker GroundChecker { get; }
        GroundChecker CeillingChecker { get; }
        Collider2D[] Colliders { get; }
    }
}