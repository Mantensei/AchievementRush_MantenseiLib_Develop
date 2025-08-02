using System;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace MantenseiLib
{
    public static class GetComponentUtility
    {
        static bool _hideErrorHandling = true;

        public static void GetOrAddComponent(MonoBehaviour monoBehaviour)
        {
            var getComponentPairs = AttributeUtility.GetAttributedFields<GetComponentAttribute>(monoBehaviour);

            foreach (var info in getComponentPairs)
            {
                object component = null;
                var memberInfo = info.memberInfo;
                var attribute = info.attribute;

                switch (attribute.GetComponentType)
                {
                    case GetComponentType.GetComponent:
                        component = GetComponentByRelations(monoBehaviour, memberInfo, attribute.relation, attribute.quantity);
                        break;

                    case GetComponentType.AddComponent:
                        component = AddComponentByRelation(monoBehaviour, memberInfo, attribute.relation);
                        break;
                }

                if (component == null && !attribute.HideErrorHandling && !_hideErrorHandling)
                {
                    Debug.LogWarning($"\"{memberInfo.GetMemberType()}\" の \"{memberInfo.Name}\" の \"{monoBehaviour.name}\" が見つかりません");
                }
                else
                {
                    SetComponent(monoBehaviour, memberInfo, component);
                }
            }
        }

        private static object GetComponentByRelations(MonoBehaviour obj, MemberInfo memberInfo, HierarchyRelation relations, QuantityType quantity)
        {
            Type componentType = memberInfo.GetMemberType();
            bool isArray = componentType.IsArray;
            Type elementType = isArray ? componentType.GetElementType() : componentType;

            object components = null;

            if (quantity == QuantityType.Single)
            {
                foreach (HierarchyRelation relation in Enum.GetValues(typeof(HierarchyRelation)))
                {
                    if (relation != HierarchyRelation.None && relations.HasFlag(relation))
                    {
                        components = GetComponentByRelation(obj, elementType, relation, quantity);
                        if (components as UnityEngine.Object != null) // Objectのnullは挙動が違うのでキャストしてnullチェック
                        {
                            break;
                        }
                    }
                }
            }
            else if (quantity == QuantityType.Multiple)
            {
                List<object> componentList = new List<object>();

                foreach (HierarchyRelation relation in Enum.GetValues(typeof(HierarchyRelation)))
                {
                    if (relation != HierarchyRelation.None && relations.HasFlag(relation))
                    {
                        object tempComponents = GetComponentByRelation(obj, elementType, relation, quantity);
                        if (tempComponents is Array tempArray)
                        {
                            foreach (var item in tempArray)
                            {
                                componentList.Add(item);
                            }
                        }
                    }
                }

                if (componentList.Count > 0)
                {
                    componentList = componentList.Distinct().ToList();
                    Array componentsArray = Array.CreateInstance(elementType, componentList.Count);
                    componentList.Distinct().ToArray().CopyTo(componentsArray, 0);

                    components = componentsArray;
                }
            }

            return components;
        }

        private static object GetComponentByRelation(MonoBehaviour obj, Type elementType, HierarchyRelation relation, QuantityType quantity)
        {
            switch (relation)
            {
                case HierarchyRelation.Parent:
                    return quantity == QuantityType.Single ? obj.transform.parent?.GetComponentInParent(elementType)
                                                           : obj.transform.parent?.GetComponentsInParent(elementType);

                case HierarchyRelation.Children:
                    if(quantity == QuantityType.Single)
                    {
                        foreach(Transform child in obj.transform)
                        {
                            var component = child.GetComponentInChildren(elementType);
                            if (component != null) return component;
                        }
                        return null;
                    }
                    else
                    {
                        List<Component> components = new List<Component>();
                        foreach (Transform child in obj.transform)
                        {
                            components.AddRange(child.GetComponentsInChildren(elementType));
                        }
                        return components.ToArray();
                    }

                case HierarchyRelation.Self:
                default:
                    return quantity == QuantityType.Single ? obj.GetComponent(elementType)
                                                           : obj.GetComponents(elementType);
            }
        }

        private static object AddComponentByRelation(MonoBehaviour obj, MemberInfo memberInfo, HierarchyRelation relation)
        {
            if (relation == HierarchyRelation.Self)
            {
                return obj.gameObject.AddComponent(memberInfo.GetMemberType());
            }
            else
            {
                Debug.LogWarning("親や子にAddComponentすることはサポートされていません。");
                return null;
            }
        }

        private static void SetComponent(MonoBehaviour obj, MemberInfo memberInfo, object component)
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                Type fieldType = fieldInfo.FieldType;
                if (fieldType.IsArray && component is Component[] componentArray)
                {
                    // コンポーネント配列をフィールドに設定
                    Array array = Array.CreateInstance(fieldType.GetElementType(), componentArray.Length);
                    Array.Copy(componentArray, array, componentArray.Length);
                    fieldInfo.SetValue(obj, array);
                }
                else
                {
                    // 単一のコンポーネントをフィールドに設定
                    fieldInfo.SetValue(obj, component);
                }
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                var setMethod = propertyInfo.GetSetMethod(true);
                if (setMethod != null)
                {
                    Type propertyType = propertyInfo.PropertyType;
                    if (propertyType.IsArray && component is Component[] componentArray)
                    {
                        // コンポーネント配列をプロパティに設定
                        Array array = Array.CreateInstance(propertyType.GetElementType(), componentArray.Length);
                        Array.Copy(componentArray, array, componentArray.Length);
                        setMethod.Invoke(obj, new[] { array });
                    }
                    else
                    {
                        // 単一のコンポーネントをプロパティに設定
                        setMethod.Invoke(obj, new[] { component });
                    }
                }
                else
                {
                    Debug.LogWarning($"プロパティ \"{propertyInfo.Name}\" のセッターが見つかりません \"{obj.GetType()}\" の \"{obj.name}\"");
                }
            }
        }

        public static Type GetMemberType(this MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                return fieldInfo.FieldType;
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                return propertyInfo.PropertyType;
            }
            return null;
        }
    }
}
