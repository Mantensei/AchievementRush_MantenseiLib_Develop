using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dmg_Test_01 : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            var dmg = DamageInfo.one;

            DamageObject.Factory(dmg, transform.position + Vector3.right * 1)
                //.SetTiming(HitTiming.Enter)
                //.SetHitInterval(1f)
                //.OnHit((e) => Debug.Log($"{e.HitObject}"))
                .SetTargetTags("Player")
                //.SetDetectionType(DetectionType.Collision)
                ;
        }
    }
}
