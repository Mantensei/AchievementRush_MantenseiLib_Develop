using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;

namespace MantenseiLib
{
    public static partial class Mantensei
    {
        public static T Instantiate<T>(T original) where T : Object
        {
            var instance = Object.Instantiate(original);
            GetOrAddComponent(instance);
            return instance;
        }

        public static T Instantiate<T>(T original, Transform parent) where T : MonoBehaviour
        {
            var instance = Object.Instantiate(original, parent);
            GetOrAddComponent(instance);
            return instance;
        }

        public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays) where T : MonoBehaviour
        {
            var instance = Object.Instantiate(original, parent, worldPositionStays);
            GetOrAddComponent(instance);
            return instance;
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : MonoBehaviour
        {
            var instance = Object.Instantiate(original, position, rotation);
            GetOrAddComponent(instance);
            return instance;
        }

        public static T Instantiate<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : MonoBehaviour
        {
            var instance = Object.Instantiate(original, position, rotation, parent);
            GetOrAddComponent(instance);
            return instance;
        }

        static void GetOrAddComponent(MonoBehaviour target)
        {
            foreach(var monoBehaviour in target.GetComponentsInChildren<MonoBehaviour>())
            {
                GetComponentUtility.GetOrAddComponent(monoBehaviour);
            }
        }

        static void GetOrAddComponent(Object target)
        {
            if(target is GameObject gameObject)
            {
                foreach(var monoBehaviour in gameObject.GetComponentsInChildren<MonoBehaviour>())
                {
                    GetComponentUtility.GetOrAddComponent(monoBehaviour);
                }
            }
        }
    }

    public static class GetComponentExtensions
    {
        public static T AddComponentAndGet<T>(this GameObject gameObject) where T : MonoBehaviour
        {
            var t = gameObject.AddComponent<T>();
            GetComponentUtility.GetOrAddComponent(t);
            return t;
        }

        public static Component AddComponentAndGet(this GameObject gameObject, Type componentType)
        {
            var component = gameObject.AddComponent(componentType);
            if (component is MonoBehaviour monoBehaviour)
            {
                GetComponentUtility.GetOrAddComponent(monoBehaviour);
            }
            return component;
        }

        public static Component AddComponentAndGet(this GameObject gameObject, string className)
        {
            var type = Type.GetType(className);
            if (type == null)
            {
                Debug.LogWarning($"Type '{className}' not found.");
                return null;
            }
            var component = gameObject.AddComponent(type);
            if (component is MonoBehaviour monoBehaviour)
            {
                GetComponentUtility.GetOrAddComponent(monoBehaviour);
            }
            return component;
        }
    }
}
