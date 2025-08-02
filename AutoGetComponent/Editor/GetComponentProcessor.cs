#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MantenseiLib.Editor
{
    /// <summary>
    /// �r���h�O�Ƀv���n�u��������������N���X
    /// </summary>
    public class GetComponentBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("[MantenseiLib] Starting pre-build prefab processing...");

            // �r���h�O�ɑS�v���n�u������
            GetComponentInitializerInjector.ProcessAllPrefabsForGetComponentInjection();

            Debug.Log("[MantenseiLib] Pre-build prefab processing completed!");
        }
    }

    /// <summary>
    /// �v���C���[�h�J�n���Ƀv���n�u��������������N���X
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeProcessor
    {
        static PlayModeProcessor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {

            //    Debug.Log("[MantenseiLib] Processing prefabs before entering play mode...");
                GetComponentInitializerInjector.ProcessAllPrefabsForGetComponentInjection();
            }
            else
            {
                //Debug.Log("[MantenseiLib] Exiting play mode, removing GetComponent auto initializers from all prefabs...");
                GetComponentInitializerInjector.RemoveGetComponentAutoInitializersFromAllPrefabs();
            }

            //if (state == PlayModeStateChange.ExitingEditMode)
            //{
            //    GetComponentInitializerInjector.ProcessAllPrefabsForGetComponentInjection();
            //    Debug.Log("[MantenseiLib] Processing prefabs before entering play mode...");
            //}
        }
    }
}
#endif