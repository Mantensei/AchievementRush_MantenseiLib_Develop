#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MantenseiLib.Editor
{
    /// <summary>
    /// �v���n�u�Ɏ����I��PrefabComponentManager��ǉ�����G�f�B�^����
    /// </summary>
    public static class GetComponentInitializerInjector
    {
        static readonly bool enabled = false;
        private const string PROCESSED_PREFABS_KEY = "MantenseiLib_ProcessedPrefabs";

        /// <summary>
        /// �v���W�F�N�g���̑S�v���n�u������
        /// </summary>
        public static void ProcessAllPrefabsForGetComponentInjection()
        {
            if(!enabled) return;

            var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            var processedCount = 0;
            var skippedCount = 0;

            EditorUtility.DisplayProgressBar("Processing Prefabs", "Scanning prefabs...", 0f);

            try
            {
                for (int i = 0; i < prefabGuids.Length; i++)
                {
                    var guid = prefabGuids[i];
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var progress = (float)i / prefabGuids.Length;

                    EditorUtility.DisplayProgressBar("Processing Prefabs", $"Processing {path}", progress);

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null)
                    {
                        if (ProcessPrefabForGetComponentInjection(prefab, path))
                        {
                            processedCount++;
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }
                }

                AssetDatabase.SaveAssets();
                //Debug.Log($"[PrefabAutoProcessor] Completed! Processed: {processedCount}, Skipped: {skippedCount}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        /// <summary>
        /// �P��̃v���n�u������
        /// </summary>
        static bool ProcessPrefabForGetComponentInjection(GameObject prefab, string prefabPath)
        {
            if (prefab == null) return false;

            // ����PrefabComponentManager�����݂��邩�`�F�b�N
            if (prefab.GetComponent<GetComponentAutoInitializer>() != null)
            {
                return false;
            }

            // GetComponent�n�������g�p���Ă���MonoBehaviour�����邩�`�F�b�N
            if (!HasGetComponentAttributes(prefab))
            {
                return false;
            }

            // �v���n�u��ҏW���[�h�ŊJ��
            var prefabAsset = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var initializer = prefabAsset.GetComponent<GetComponentAutoInitializer>();
                if (initializer == null)
                {
                    initializer = prefabAsset.AddComponent<GetComponentAutoInitializer>();
                    //Debug.Log($"[PrefabAutoProcessor] Added PrefabComponentManager to {prefabPath}");
                }
                // �ύX��ۑ�
                PrefabUtility.SaveAsPrefabAsset(prefabAsset, prefabPath);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabAsset);
            }
        }

        /// <summary>
        /// �v���n�u��GetComponent�n�������g�p���Ă��邩�`�F�b�N
        /// </summary>
        private static bool HasGetComponentAttributes(GameObject prefab)
        {
            var monoBehaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var monoBehaviour in monoBehaviours)
            {
                if (monoBehaviour == null) continue;

                var type = monoBehaviour.GetType();

                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var field in fields)
                {
                    if (field.GetCustomAttribute<GetComponentAttribute>() != null)
                    {
                        return true;
                    }
                }

                var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var property in properties)
                {
                    if (property.GetCustomAttribute<GetComponentAttribute>() != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// �w�肳�ꂽ�v���n�u����PrefabComponentManager���폜
        /// </summary>
        public static void RemoveGetComponentAutoInitializersFromAllPrefabs()
        {
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            var removedCount = 0;

            EditorUtility.DisplayProgressBar("Removing PrefabComponentManagers", "Scanning prefabs...", 0f);

            try
            {
                for (int i = 0; i < prefabGuids.Length; i++)
                {
                    var guid = prefabGuids[i];
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var progress = (float)i / prefabGuids.Length;

                    EditorUtility.DisplayProgressBar("Removing PrefabComponentManagers", $"Processing {path}", progress);

                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab != null && prefab.GetComponent<GetComponentAutoInitializer>() != null)
                    {
                        var prefabAsset = PrefabUtility.LoadPrefabContents(path);
                        try
                        {
                            var initializer = prefabAsset.GetComponent<GetComponentAutoInitializer>();
                            if (initializer != null)
                            {
                                Object.DestroyImmediate(initializer);
                                PrefabUtility.SaveAsPrefabAsset(prefabAsset, path);
                                removedCount++;
                            }
                        }
                        finally
                        {
                            PrefabUtility.UnloadPrefabContents(prefabAsset);
                        }
                    }
                }

                AssetDatabase.SaveAssets();
                //Debug.Log($"[PrefabAutoProcessor] Removed PrefabComponentManager from {removedCount} prefabs");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
#endif
