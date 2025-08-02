using UnityEngine;
using UnityEngine.SceneManagement;

namespace MantenseiLib
{
    /// <summary>
    /// ランタイムでシーンロード時に自動処理を行うクラス
    /// </summary>
    public static class RuntimeSceneProcessor
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// アプリケーション開始時に一度だけ実行される初期化処理
        /// Awakeよりも早いタイミングで実行される
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_isInitialized) return;

            _isInitialized = true;

            SceneManager.sceneLoaded += OnSceneLoaded;
            ProcessCurrentScene();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ProcessScene(scene);
        }

        private static void ProcessCurrentScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            ProcessScene(activeScene);
        }

        private static void ProcessScene(Scene scene)
        {
            if (!scene.IsValid()) return;

            try
            {
                // シーン内の全てのルートGameObjectを取得
                var rootGameObjects = scene.GetRootGameObjects();

                foreach (var rootGameObject in rootGameObjects)
                {
                    if (rootGameObject == null) continue;

                    // PrefabComponentManagerが存在しないオブジェクトのみ処理
                    if (rootGameObject.GetComponent<GetComponentAutoInitializer>() == null)
                    {
                        ProcessGameObjectHierarchy(rootGameObject);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing scene '{scene.name}': {ex.Message}");
            }
        }

        private static void ProcessGameObjectHierarchy(GameObject gameObject)
        {
            if (gameObject == null) return;

            try
            {
                // 自身のMonoBehaviourを処理
                var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
                foreach (var monoBehaviour in monoBehaviours)
                {
                    if (monoBehaviour != null)
                    {
                        GetComponentUtility.GetOrAddComponent(monoBehaviour);
                    }
                }

                // 子オブジェクトも再帰的に処理
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i);
                    if (child != null)
                    {
                        ProcessGameObjectHierarchy(child.gameObject);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing GameObject '{gameObject.name}': {ex.Message}");
            }
        }
    }
}