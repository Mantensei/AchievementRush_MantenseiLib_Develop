#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MantenseiLib.Editor
{
    /// <summary>
    /// プレハブに自動的にPrefabComponentManagerを追加するエディタ処理
    /// </summary>
    public static class GetComponentInitializerInjector
    {
        static readonly bool enabled = false;
        private const string PROCESSED_PREFABS_KEY = "MantenseiLib_ProcessedPrefabs";

        /// <summary>
        /// プロジェクト内の全プレハブを処理
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
        /// 単一のプレハブを処理
        /// </summary>
        static bool ProcessPrefabForGetComponentInjection(GameObject prefab, string prefabPath)
        {
            if (prefab == null) return false;

            // 既にPrefabComponentManagerが存在するかチェック
            if (prefab.GetComponent<GetComponentAutoInitializer>() != null)
            {
                return false;
            }

            // GetComponent系属性を使用しているMonoBehaviourがあるかチェック
            if (!HasGetComponentAttributes(prefab))
            {
                return false;
            }

            // プレハブを編集モードで開く
            var prefabAsset = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var initializer = prefabAsset.GetComponent<GetComponentAutoInitializer>();
                if (initializer == null)
                {
                    initializer = prefabAsset.AddComponent<GetComponentAutoInitializer>();
                    //Debug.Log($"[PrefabAutoProcessor] Added PrefabComponentManager to {prefabPath}");
                }
                // 変更を保存
                PrefabUtility.SaveAsPrefabAsset(prefabAsset, prefabPath);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabAsset);
            }
        }

        /// <summary>
        /// プレハブがGetComponent系属性を使用しているかチェック
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
        /// 指定されたプレハブからPrefabComponentManagerを削除
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
