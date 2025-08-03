using MantenseiLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MantenseiLib
{
    [System.Flags]
    public enum StateLockOption
    {
        None = 0,
        AllowMove = 1 << 0,
    }

    public class ActionStateLock : HubChild<IPlayerHub>
    {
        private MonoBehaviour _currentOwner = null;
        public bool IsLocked => _currentOwner != null;
        public bool AllowMove => _currentOwner == null || (LockOptions & StateLockOption.AllowMove) == StateLockOption.AllowMove;

        public StateLockOption LockOptions { get; private set; } = StateLockOption.None;

        public bool TryLock(MonoBehaviour requester)
        {
            return TryLock(requester, StateLockOption.None);
        }

        public bool TryLock(MonoBehaviour requester, StateLockOption blockSystems)
        {
            if (_currentOwner == null || _currentOwner == requester)
            {
                _currentOwner = requester;
                LockOptions = blockSystems;
                return true;
            }
            return false;
        }

        public void Unlock(MonoBehaviour requester)
        {
            if (_currentOwner == requester)
            {
                _currentOwner = null;
                LockOptions = StateLockOption.None;
            }
        }
    }
}