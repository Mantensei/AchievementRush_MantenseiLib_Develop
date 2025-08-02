using UnityEngine;
using UnityEngine.SceneManagement;

namespace MantenseiLib
{
    /// <summary>
    /// �����^�C���ŃV�[�����[�h���Ɏ����������s���N���X
    /// </summary>
    public static class RuntimeSceneProcessor
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// �A�v���P�[�V�����J�n���Ɉ�x�������s����鏉��������
        /// Awake���������^�C�~���O�Ŏ��s�����
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
                // �V�[�����̑S�Ẵ��[�gGameObject���擾
                var rootGameObjects = scene.GetRootGameObjects();

                foreach (var rootGameObject in rootGameObjects)
                {
                    if (rootGameObject == null) continue;

                    // PrefabComponentManager�����݂��Ȃ��I�u�W�F�N�g�̂ݏ���
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
                // ���g��MonoBehaviour������
                var monoBehaviours = gameObject.GetComponents<MonoBehaviour>();
                foreach (var monoBehaviour in monoBehaviours)
                {
                    if (monoBehaviour != null)
                    {
                        GetComponentUtility.GetOrAddComponent(monoBehaviour);
                    }
                }

                // �q�I�u�W�F�N�g���ċA�I�ɏ���
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