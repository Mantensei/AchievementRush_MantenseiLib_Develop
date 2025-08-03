using MantenseiLib.Develop;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dmg_Test_01 : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            HitDetector.Factory(transform.position + Vector3.right * 1)
                .OnHit((e) => Debug.Log($"{e.HitObject}"))
                .SetTargetTags("Player")
                //.SetDetectionType(DetectionType.Collision)
                ;
        }
    }
}
