using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib.Internal
{
	public class dmg_Test_03 : MonoBehaviour
	{
        //[GetComponent(HierarchyRelation.Parent)]
        //IPlayerHub player;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                var one = DamageInfo.one;
                DamageObject.Factory(one, transform.position)
                    .SetLifeTime(100)
                    .SetParent(transform)
                    ;
            }
        }
    } 
}
