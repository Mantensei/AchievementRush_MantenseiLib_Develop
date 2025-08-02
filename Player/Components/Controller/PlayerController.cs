using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib.UnityAction.Action2D
{
    public class PlayerController : MonoBehaviour
    {
        [GetComponent(HierarchyRelation.Parent)]
        IPlayerHub player;

        private void Update()
        {



            if(Input.GetKey(KeyCode.Space))
            {
                if(player.GroundChecker.IsGround())
                    player.Jumper.Jump();
            }

            var moveInput = Input.GetAxis("Horizontal");
            if (moveInput != 0)
            {
                player.Mover.Move(moveInput);
            }
        }
    }
}