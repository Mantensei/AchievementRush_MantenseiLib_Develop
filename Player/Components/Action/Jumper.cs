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

			Jump(destination, duration);
		}

		void Jump(float destination, float duration)
		{
			//if (stateMarker?.TrySetState() == false)
			//	return;
			//else
			//	stateMarker?.TryResetState();

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
		}
    }
}