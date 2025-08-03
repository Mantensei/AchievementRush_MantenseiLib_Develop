using System.Collections;
using System.Collections.Generic;
using MantenseiLib;
using UnityEngine;
using DG.Tweening;
using MantenseiLib.UnityAction.Action2D;
using System;

namespace MantenseiLib
{
	public class Jumper : HubChild<IPlayerHub>
	{
		public float height = 2;
		public float heightPerDuration = 1;
		Rigidbody2D rb2d => HUB.rb2d;
		float _gravityScale;
		Tween jumpTween;
		public bool Jumping => jumpTween?.active == true;
		new Transform transform => HUB.transform;
		GroundChecker CeillingChecker => HUB.CeillingChecker;
		GroundChecker GroundChecker => HUB.GroundChecker;

		public event Action OnKillAction;
		public event Action OnCompleteAction;

		[GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)]
		ActionStateController _stateController;

        //[GetComponent]
        //StateMarker stateMarker;

        protected override void Start()
        {
            base.Start();

			CeillingChecker?.OnGroundAction(KillTween);
		}

        public void GroundJump(float power = 1)
		{
			if(GroundChecker?.IsGround() != false)
				Jump(power);
		}

        public void Jump(float power = 1)
		{
			if (jumpTween != null) return;
			if (CeillingChecker?.IsGround() == true) return;


			float destination = transform.position.y + height * power;
			float duration = height * power * heightPerDuration;

			//_stateController‚ª‘¶Ý‚µ‚È‚¯‚ê‚ÎŽÀs
			if (_stateController == null)
			{
				JumpStart(destination, duration);
			}
			else
			{
				_stateController?.TryExecuteAction(() => JumpStart(destination, duration));
			}
		}

		void JumpStart(float destination, float duration)
		{
            _gravityScale = rb2d.gravityScale;
            rb2d.gravityScale = 0;

            var velo = rb2d.velocity;
            rb2d.velocity = Vector2.right * rb2d.velocity.x;

            jumpTween = transform
                .DOMoveY(destination, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(OnJumpComplete)
                .OnKill(OnJumpKill)
                ;
        }

		public void KillTween()
		{
			jumpTween.Kill();
		}

		void OnJumpComplete()
		{
			OnCompleteAction?.Invoke();
			jumpTween?.Kill();
		}

		void OnJumpKill()
		{
			rb2d.gravityScale = _gravityScale;
			jumpTween = null;

			OnKillAction?.Invoke();

			_stateController?.EndAction();
        }
    }
}