using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib
{
    public class GetComponentManager : SingletonMonoBehaviour<GetComponentManager>
    {
        protected override void Awake()
        {
            base.Awake();

            foreach (var monoBehaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                GetComponentUtility.GetOrAddComponent(monoBehaviour);
            }
        }
    }
}