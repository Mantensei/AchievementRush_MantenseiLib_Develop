using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib
{
    public class ActionStateLock : HubChild<IPlayerHub>
    {
        private MonoBehaviour _currentOwner = null;

        public bool TryLock(MonoBehaviour requester)
        {
            if (_currentOwner == null || _currentOwner == requester)
            {
                _currentOwner = requester;
                return true;
            }
            return false;
        }

        public void Unlock(MonoBehaviour requester)
        {
            if (_currentOwner == requester)
            {
                _currentOwner = null;
            }
        }

        public bool IsLocked => _currentOwner != null;
        public bool IsLockedBy(MonoBehaviour component) => _currentOwner == component;
    }

}