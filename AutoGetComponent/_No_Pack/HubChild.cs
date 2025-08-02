using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib
{
    public abstract class HubChild<T> : BaseMonoBehaviour where T : IMonoBehaviour
    {
        [GetComponent(HierarchyRelation.Self | HierarchyRelation.Parent)] public T HUB { get; protected set; }
    }

    //public interface IHub : IMonoBehaviour { }

}