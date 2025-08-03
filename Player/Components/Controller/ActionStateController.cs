using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib
{
    public class ActionStateController : HubChild<IPlayerHub>
    {
        [GetComponent(HierarchyRelation.Parent)] private ActionStateLock stateLock;
        [GetComponent] private Animation2DRegisterer animRegisterer;
        [SerializeField] StateLockOption lockOptions;

        public bool TryExecuteAction(System.Action actionCallback)
        {
            if (!stateLock?.TryLock(this, lockOptions) == true) return false;

            animRegisterer?.Play();
            actionCallback?.Invoke();

            return true;
        }

        public void EndAction()
        {
            stateLock?.Unlock(this);
            animRegisterer?.Pause();
        }
    } 
}